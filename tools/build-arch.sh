#!/usr/bin/env bash
# Run as a normal user on Arch (base-devel installed); only packages a native payload.
set -euo pipefail
cd "$(dirname "$0")/.."
version="${1:-1.0.0}"
payload="$(realpath "${2:-src/NovaPen/bin/Release/net11.0/linux-x64/publish}")"
output="${3:-.artifacts/installers}"
[[ "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+([.-][A-Za-z0-9.-]+)?$ ]] || { echo 'Invalid version' >&2; exit 2; }
[[ "$EUID" != 0 ]] || { echo 'makepkg requires a non-root user.' >&2; exit 2; }
[[ -f "$payload/NovaPen" ]] || { echo 'Native Linux x64 payload is missing.' >&2; exit 2; }
[[ "$(uname -m)" == x86_64 ]] || { echo 'Arch packaging currently supports x86_64 only.' >&2; exit 2; }
command -v makepkg >/dev/null
mkdir -p "$output"
output="$(realpath "$output")"
stage="$(mktemp -d)"
trap 'rm -rf -- "$stage"' EXIT
mkdir "$stage/payload"
cp -a "$payload/." "$stage/payload/"
find "$stage/payload" -type f \( -name '*.dbg' -o -name '*.pdb' \) -delete
# Normalize tar metadata for repeatable input hashes; preserve every native byte.
export SOURCE_DATE_EPOCH="${SOURCE_DATE_EPOCH:-$(git log -1 --format=%ct)}"
tar --sort=name --mtime="@$SOURCE_DATE_EPOCH" --owner=0 --group=0 --numeric-owner -C "$stage" -cf "$stage/payload.tar" payload
cp packaging/arch/PKGBUILD "$stage/PKGBUILD"
cp packaging/novapen.desktop "$stage/novapen.desktop"
cp src/NovaPen/Assets/NovaPen.svg "$stage/novapen.svg"
cp LICENSE "$stage/LICENSE"
# Arch pkgver disallows hyphens. Preserve prerelease/dev labels with underscores.
export BLURMASK_VERSION="${version//-/_}"
export PKGDEST="$output"
export PACKAGER='Xekep <noreply@kiiko.ru>'
cd "$stage"
makepkg --geninteg >> PKGBUILD
makepkg --nodeps --clean --noconfirm
package="$output/novapen-$BLURMASK_VERSION-1-x86_64.pkg.tar.zst"
[[ -f "$package" ]]
(cd "$output" && sha256sum "$(basename "$package")") > "$package.sha256"
echo "Built $package"
