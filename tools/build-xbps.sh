#!/usr/bin/env bash
# Package a verified Ubuntu 22.04 linux-x64 Native AOT payload on Void/glibc.
set -euo pipefail
cd "$(dirname "$0")/.."
version="${1:-1.0.0}"
payload="$(realpath "${2:-src/BlurMask/bin/Release/net11.0/linux-x64/publish}")"
output="${3:-.artifacts/void-installers}"
[[ "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+([.-][A-Za-z0-9.-]+)?$ ]] || { echo 'Invalid version.' >&2; exit 2; }
[[ "$EUID" != 0 ]] || { echo 'Package as an unprivileged builder.' >&2; exit 2; }
[[ "$(xbps-uhelper arch)" == x86_64 && "$(getconf GNU_LIBC_VERSION)" == glibc* ]] || {
  echo 'Void packaging supports x86_64 glibc only, not musl.' >&2; exit 2;
}
[[ -f "$payload/BlurMask" ]] || { echo 'Verified native Linux payload is missing.' >&2; exit 2; }
command -v xbps-create >/dev/null
mkdir -p "$output"
output="$(realpath "$output")"
# XBPS uses the final underscore for package revision. Keep dev/prerelease labels with dots.
version="${version//-/.}"
package="$output/blurmask-${version}_1.x86_64.xbps"
[[ ! -e "$package" ]] || { echo "Package already exists: $package" >&2; exit 2; }
stage="$(mktemp -d)"
trap 'rm -rf -- "$stage"' EXIT
root="$stage/package"
install -d "$root/opt/blurmask" "$root/usr/bin" "$root/usr/share/doc/blurmask"
cp -a "$payload/." "$root/opt/blurmask/"
find "$root/opt/blurmask" -type f \( -name '*.dbg' -o -name '*.pdb' \) -delete
chmod 755 "$root/opt/blurmask/BlurMask"
ln -s /opt/blurmask/BlurMask "$root/usr/bin/blurmask"
install -Dm644 packaging/blurmask.desktop "$root/usr/share/applications/blurmask.desktop"
install -Dm644 packaging/blurmask.desktop "$root/etc/xdg/autostart/blurmask.desktop"
sed -i 's|^Exec=/opt/blurmask/BlurMask$|Exec=/opt/blurmask/BlurMask --minimized|' "$root/etc/xdg/autostart/blurmask.desktop"
install -Dm644 src/BlurMask/Assets/BlurMask.svg "$root/usr/share/icons/hicolor/scalable/apps/blurmask.svg"
install -Dm644 LICENSE "$root/usr/share/licenses/blurmask/LICENSE"
# Keep all font/runtime licenses and the complete native payload byte-for-byte.
(cd "$root/opt/blurmask"; find . -type f -print0 | sort -z | xargs -0 sha256sum) \
  > "$root/usr/share/doc/blurmask/payload.sha256"
# Normalize input timestamps without stripping or rewriting tested ELF binaries.
epoch="${SOURCE_DATE_EPOCH:-$(git log -1 --format=%ct)}"
[[ "$epoch" =~ ^[0-9]+$ ]]
find "$root" -exec touch -h -d "@$epoch" {} +
dependencies='glibc>=2.35 libgcc libstdc++ zlib libssl3 libcrypto3 icu ca-certificates'
dependencies+=' libX11 libxcb libICE libSM fontconfig libXcursor libXrandr libXi libXrender dbus'
dependencies+=' wayland libxkbcommon libglvnd'
cd "$output"
xbps-create -A x86_64 -n "blurmask-${version}_1" \
  -s 'Native desktop annotation, privacy masks and shared canvas' \
  -S 'Native AOT application; no separately installed .NET runtime. Requires Void glibc. X11/XWayland supports desktop drawing; optional native Wayland supports the separate canvas. XWayland and the graphics driver are supplied by the desktop environment.' \
  -l MIT -m 'Xekep <noreply@kiiko.ru>' -H 'https://github.com/Xekep/BlurMask' \
  -D "$dependencies" -F /etc/xdg/autostart/blurmask.desktop --compression zstd "$root"
[[ -s "$package" ]]
sha256sum "$(basename "$package")" > "$package.sha256"
echo "Built $package"
