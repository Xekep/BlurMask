#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
rid="${1:-linux-x64}"
version="${2:-1.0.0}"
payload="${3:-src/NovaPen/bin/Release/net11.0/$rid/publish}"
output="${4:-.artifacts/installers}"
case "$rid" in linux-x64) arch=amd64;; linux-arm64) arch=arm64;; *) echo 'Debian packaging requires a Linux RID.' >&2; exit 2;; esac
[[ "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+([.-][A-Za-z0-9.-]+)?$ ]] || { echo 'Invalid version' >&2; exit 2; }
version="${version/-/\~}"
[[ -f "$payload/NovaPen" ]] || { echo 'Native publish payload is missing.' >&2; exit 2; }
stage="$(mktemp -d)"
trap 'rm -rf -- "$stage"' EXIT
root="$stage/package"
install -d "$root/DEBIAN" "$root/opt/novapen" "$root/usr/bin" "$root/usr/share/applications" "$root/usr/share/icons/hicolor/scalable/apps" "$root/usr/share/doc/novapen" "$root/etc/xdg/autostart"
cp -a "$payload/." "$root/opt/novapen/"
find "$root/opt/novapen" -type f \( -name '*.dbg' -o -name '*.pdb' \) -delete
chmod 755 "$root/opt/novapen/NovaPen"
if [[ -f "$root/opt/novapen/runtimes/$rid/solver/icas" ]]; then chmod 755 "$root/opt/novapen/runtimes/$rid/solver/icas"; fi
ln -s /opt/novapen/NovaPen "$root/usr/bin/novapen"
install -m 644 packaging/novapen.desktop "$root/usr/share/applications/novapen.desktop"
# Keep the XDG autostart ID stable: preserve disabled user overrides and replace the old conffile.
install -m 644 packaging/novapen.desktop "$root/etc/xdg/autostart/blurmask.desktop"
sed -i 's|^Exec=/opt/novapen/NovaPen$|Exec=/opt/novapen/NovaPen --minimized|' "$root/etc/xdg/autostart/blurmask.desktop"
install -m 644 src/NovaPen/Assets/NovaPen.svg "$root/usr/share/icons/hicolor/scalable/apps/novapen.svg"
install -m 644 LICENSE "$root/usr/share/doc/novapen/copyright"
printf '\nBundled Giac is GPL-3.0-or-later. Complete solver/dependency licenses and\ncorresponding-source instructions: /opt/novapen/runtimes/%s/solver/.\n' "$rid" >> "$root/usr/share/doc/novapen/copyright"
printf '/etc/xdg/autostart/blurmask.desktop\n' > "$root/DEBIAN/conffiles"
mkdir "$stage/debian"
printf 'Source: novapen\nSection: graphics\nPriority: optional\nMaintainer: Xekep <noreply@kiiko.ru>\n\nPackage: novapen\nArchitecture: %s\nDescription: Desktop annotation and shared canvas\n' "$arch" > "$stage/debian/control"
elfs=(-e"$root/opt/novapen/NovaPen")
[[ ! -f "$root/opt/novapen/runtimes/$rid/solver/icas" ]] || elfs+=(-e"$root/opt/novapen/runtimes/$rid/solver/icas")
while IFS= read -r -d '' library; do elfs+=(-e"$library"); done < <(find "$root/opt/novapen" -name '*.so' -print0)
dependencies="$(cd "$stage" && dpkg-shlibdeps -O -l"$root/opt/novapen" "${elfs[@]}")"
# dpkg-shlibdeps sees the executable's ELF dependencies. Avalonia also loads
# X11/fontconfig and .NET globalization libraries at runtime.
dependencies="${dependencies#shlibs:Depends=}"
cat > "$root/DEBIAN/control" <<EOF
Package: novapen
Conflicts: blurmask
Replaces: blurmask
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
package="$output/novapen_${version}_${arch}.deb"
dpkg-deb --root-owner-group --build "$root" "$package"
(cd "$output" && sha256sum "$(basename "$package")") > "$package.sha256"
echo "Built $package"
