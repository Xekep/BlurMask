#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-3.0-or-later
# Native source builds only. RID OutputDir SourcesDir CacheDir Jobs.
set -euo pipefail
tools=$(cd "$(dirname "$0")" && pwd)
root=$(cd "$tools/.." && pwd)
rid=${1:-linux-x64}
output=${2:-$root/.artifacts/giac-runtime/$rid}
sources=${3:-$root/.artifacts/installers}
cache=${4:-$root/.artifacts/giac-build/$rid}
jobs=${5:-4}
case "$jobs" in ''|*[!0-9]*) echo 'Jobs must be an integer from 1 to 16.' >&2; exit 2 ;; esac
if (( jobs < 1 || jobs > 16 )); then echo 'Jobs must be an integer from 1 to 16.' >&2; exit 2; fi
case "$(uname -s):$(uname -m):$rid" in
  Linux:x86_64:linux-x64) compiler=g++; checker=readelf ;;
  Darwin:x86_64:osx-x64|Darwin:arm64:osx-arm64) compiler=clang++; checker=otool ;;
  *) echo "Giac supports native Linux x64/macOS x64/arm64 builds; host $(uname -s)/$(uname -m), requested $rid." >&2; exit 2 ;;
esac
for dependency in bash make m4 curl tar xz python3 "$compiler" "$checker" strip; do
  command -v "$dependency" >/dev/null || { echo "Missing Giac build tool: $dependency" >&2; exit 2; }
done
if [[ "$rid" == osx-* ]]; then command -v lipo >/dev/null; fi
mkdir -p "$output" "$sources" "$cache"
output=$(cd "$output" && pwd)
sources=$(cd "$sources" && pwd)
cache=$(cd "$cache" && pwd)
# Autoconf and GNU make source/include variables are not reliable with spaces.
case "$cache:$tools" in *' '*|*$'\t'*|*$'\n'*) echo 'Use build cache and recipe paths without whitespace.' >&2; exit 2 ;; esac
helper="$tools/giac-native/package-posix.py"
python3 "$helper" validate-output --rid "$rid" --output "$output"
if python3 "$helper" cached --rid "$rid" --output "$output" --sources "$sources" --tools "$tools" 2>/dev/null; then
  echo "Using verified Giac runtime and corresponding sources: $output/icas"
  exit 0
fi
while IFS=$'\t' read -r name expected url; do
  if [[ ! -f "$cache/$name" ]]; then
    echo "Downloading $name"
    curl --fail --location --retry 3 --silent --show-error "$url" --output "$cache/$name"
  fi
done < <(python3 "$helper" specs --rid "$rid")
python3 "$helper" verify-inputs --rid "$rid" --cache "$cache"
recipe_hash=$(python3 "$helper" recipe-hash --rid "$rid" --tools "$tools")
work="$cache/build-$rid-${recipe_hash:0:16}"
mkdir -p "$work/sources"
for name in giac-source.tar.gz gmp-6.3.0.tar.xz mpfr-4.2.2.tar.xz; do
  case "$name" in
    giac-source.tar.gz) directory=giac-bdbb1c41fe52d8b4fb7c1d15c2deaea88fc3803c ;;
    gmp-6.3.0.tar.xz) directory=gmp-6.3.0 ;;
    mpfr-4.2.2.tar.xz) directory=mpfr-4.2.2 ;;
  esac
  if [[ ! -f "$work/sources/.$directory.complete" ]]; then
    tar -xf "$cache/$name" -C "$work/sources"
    touch "$work/sources/.$directory.complete"
  fi
done
echo "Building pinned static Giac/GMP/MPFR for $rid; logs: $work/build-*/"
if ! bash "$tools/giac-native/build-posix.sh" "$work" "$tools/giac-native" "$rid" "$jobs"; then
  echo "Giac source build failed; inspect $work/build-*/{configure,build,install}.log" >&2
  for log in "$work"/build-*/configure.log "$work"/build-*/build.log; do
    if [[ -f "$log" ]]; then echo "--- $log" >&2; tail -n 25 "$log" >&2; fi
  done
  exit 1
fi
python3 "$tools/giac-native/test-transport.py" "$work/build-core/icas"
python3 "$helper" package --rid "$rid" --output "$output" --sources "$sources" --cache "$cache" --work "$work" --tools "$tools"
echo "$output/icas"
