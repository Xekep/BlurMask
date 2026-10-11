#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
rid="${1:-linux-x64}"
version="${2:-1.0.0}"
payload="${3:-src/BlurMask/bin/Release/net11.0/$rid/publish}"
output="${4:-.artifacts/installers}"
case "$rid" in linux-x64) arch=amd64;; linux-arm64) arch=arm64;; *) echo 'Debian packaging requires a Linux RID.' >&2; exit 2;; esac
[[ "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+([.-][A-Za-z0-9.-]+)?$ ]] || { echo 'Invalid version' >&2; exit 2; }
version="${version/-/\~}"
[[ -f "$payload/BlurMask" ]] || { echo 'Native publish payload is missing.' >&2; exit 2; }
stage="$(mktemp -d)"
trap 'rm -rf -- "$stage"' EXIT
root="$stage/package"
install -d "$root/DEBIAN" "$root/opt/blurmask" "$root/usr/bin" "$root/usr/share/applications" "$root/usr/share/icons/hicolor/scalable/apps" "$root/usr/share/doc/blurmask" "$root/etc/xdg/autostart"
cp -a "$payload/." "$root/opt/blurmask/"
find "$root/opt/blurmask" -type f \( -name '*.dbg' -o -name '*.pdb' \) -delete
chmod 755 "$root/opt/blurmask/BlurMask"
if [[ -f "$root/opt/blurmask/runtimes/$rid/solver/icas" ]]; then chmod 755 "$root/opt/blurmask/runtimes/$rid/solver/icas"; fi
ln -s /opt/blurmask/BlurMask "$root/usr/bin/blurmask"
install -m 644 packaging/blurmask.desktop "$root/usr/share/applications/blurmask.desktop"
install -m 644 packaging/blurmask.desktop "$root/etc/xdg/autostart/blurmask.desktop"
sed -i 's|^Exec=/opt/blurmask/BlurMask$|Exec=/opt/blurmask/BlurMask --minimized|' "$root/etc/xdg/autostart/blurmask.desktop"
install -m 644 src/BlurMask/Assets/BlurMask.svg "$root/usr/share/icons/hicolor/scalable/apps/blurmask.svg"
install -m 644 LICENSE "$root/usr/share/doc/blurmask/copyright"
printf '\nBundled Giac is GPL-3.0-or-later. Complete solver/dependency licenses and\ncorresponding-source instructions: /opt/blurmask/runtimes/%s/solver/.\n' "$rid" >> "$root/usr/share/doc/blurmask/copyright"
printf '/etc/xdg/autostart/blurmask.desktop\n' > "$root/DEBIAN/conffiles"
mkdir "$stage/debian"
printf 'Source: blurmask\nSection: graphics\nPriority: optional\nMaintainer: Xekep <noreply@kiiko.ru>\n\nPackage: blurmask\nArchitecture: %s\nDescription: Desktop annotation and shared canvas\n' "$arch" > "$stage/debian/control"
elfs=(-e"$root/opt/blurmask/BlurMask")
[[ ! -f "$root/opt/blurmask/runtimes/$rid/solver/icas" ]] || elfs+=(-e"$root/opt/blurmask/runtimes/$rid/solver/icas")
while IFS= read -r -d '' library; do elfs+=(-e"$library"); done < <(find "$root/opt/blurmask" -name '*.so' -print0)
dependencies="$(cd "$stage" && dpkg-shlibdeps -O -l"$root/opt/blurmask" "${elfs[@]}")"
# dpkg-shlibdeps sees the executable's ELF dependencies. Avalonia also loads
# X11/fontconfig and .NET globalization libraries at runtime.
dependencies="${dependencies#shlibs:Depends=}"
cat > "$root/DEBIAN/control" <<EOF
Package: blurmask
Version: $version
Architecture: $arch
Maintainer: Xekep <noreply@kiiko.ru>
Section: graphics
Priority: optional
Depends: $dependencies, libx11-6, libxcb1, libxcb-shape0, libice6, libsm6, libfontconfig1, libxcursor1, libxrandr2, libxi6, libxrender1, libdbus-1-3, libwayland-client0, libxkbcommon0, libegl1, libicu70 | libicu72 | libicu74 | libicu76 | libicu78, libssl3 | libssl3t64, zlib1g, ca-certificates
Description: Desktop annotation and shared canvas
 Native desktop drawing, privacy masks, screenshots and collaborative canvas.
 No separately installed .NET runtime is required. Uses X11/XWayland, with optional native Wayland for the separate canvas.
EOF
mkdir -p "$output"
package="$output/blurmask_${version}_${arch}.deb"
dpkg-deb --root-owner-group --build "$root" "$package"
(cd "$output" && sha256sum "$(basename "$package")") > "$package.sha256"
echo "Built $package"
