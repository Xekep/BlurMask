#!/usr/bin/env bash
# Package a verified Ubuntu 22.04 linux-x64 Native AOT payload on Void/glibc.
set -euo pipefail
cd "$(dirname "$0")/.."
version="${1:-1.0.0}"
payload="$(realpath "${2:-src/NovaPen/bin/Release/net11.0/linux-x64/publish}")"
output="${3:-.artifacts/void-installers}"
[[ "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+([.-][A-Za-z0-9.-]+)?$ ]] || { echo 'Invalid version.' >&2; exit 2; }
[[ "$EUID" != 0 ]] || { echo 'Package as an unprivileged builder.' >&2; exit 2; }
[[ "$(xbps-uhelper arch)" == x86_64 && "$(getconf GNU_LIBC_VERSION)" == glibc* ]] || {
  echo 'Void packaging supports x86_64 glibc only, not musl.' >&2; exit 2;
}
[[ -f "$payload/NovaPen" ]] || { echo 'Verified native Linux payload is missing.' >&2; exit 2; }
command -v xbps-create >/dev/null
mkdir -p "$output"
output="$(realpath "$output")"
# XBPS uses the final underscore for package revision. Keep dev/prerelease labels with dots.
version="${version//-/.}"
package="$output/novapen-${version}_1.x86_64.xbps"
[[ ! -e "$package" ]] || { echo "Package already exists: $package" >&2; exit 2; }
stage="$(mktemp -d)"
trap 'rm -rf -- "$stage"' EXIT
root="$stage/package"
install -d "$root/opt/novapen" "$root/usr/bin" "$root/usr/share/doc/novapen"
cp -a "$payload/." "$root/opt/novapen/"
find "$root/opt/novapen" -type f \( -name '*.dbg' -o -name '*.pdb' \) -delete
chmod 755 "$root/opt/novapen/NovaPen"
chmod 755 "$root/opt/novapen/runtimes/linux-x64/solver/icas"
ln -s /opt/novapen/NovaPen "$root/usr/bin/novapen"
install -Dm644 packaging/novapen.desktop "$root/usr/share/applications/novapen.desktop"
install -Dm644 packaging/novapen.desktop "$root/etc/xdg/autostart/novapen.desktop"
sed -i 's|^Exec=/opt/novapen/NovaPen$|Exec=/opt/novapen/NovaPen --minimized|' "$root/etc/xdg/autostart/novapen.desktop"
install -Dm644 src/NovaPen/Assets/NovaPen.svg "$root/usr/share/icons/hicolor/scalable/apps/novapen.svg"
install -Dm644 LICENSE "$root/usr/share/licenses/novapen/LICENSE"
# Keep all font/runtime licenses and the complete native payload byte-for-byte.
(cd "$root/opt/novapen"; find . -type f -print0 | sort -z | xargs -0 sha256sum) \
  > "$root/usr/share/doc/novapen/payload.sha256"
# Normalize input timestamps without stripping or rewriting tested ELF binaries.
epoch="${SOURCE_DATE_EPOCH:-$(git log -1 --format=%ct)}"
[[ "$epoch" =~ ^[0-9]+$ ]]
find "$root" -exec touch -h -d "@$epoch" {} +
# xbps-create takes complete package patterns, unlike xbps-src template shorthand.
dependencies='glibc>=2.35 libgcc>=0 libstdc++>=0 zlib>=0 libssl3>=0 libcrypto3>=0 icu>=0 ca-certificates>=0'
dependencies+=' libX11>=0 libxcb>=0 libICE>=0 libSM>=0 fontconfig>=0 libXcursor>=0 libXrandr>=0 libXi>=0 libXrender>=0 dbus>=0'
dependencies+=' wayland>=0 libxkbcommon>=0 libglvnd>=0'
cd "$output"
xbps-create -A x86_64 -n "novapen-${version}_1" \
  -s 'Native desktop annotation, privacy masks and shared canvas' \
  -S 'Native AOT application; no separately installed .NET runtime. Requires Void glibc. X11/XWayland supports desktop drawing; optional native Wayland supports the separate canvas. XWayland and the graphics driver are supplied by the desktop environment.' \
  -l 'MIT, GPL-3.0-or-later' -m 'Xekep <noreply@kiiko.ru>' -H 'https://github.com/Xekep/NovaPen' \
  -D "$dependencies" -F /etc/xdg/autostart/novapen.desktop --compression zstd "$root"
[[ -s "$package" ]]
sha256sum "$(basename "$package")" > "$package.sha256"
echo "Built $package"
