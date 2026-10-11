#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-3.0-or-later
set -euo pipefail
work=$(cd "$1" && pwd)
recipe=$(cd "$2" && pwd)
rid=$3
jobs=${4:-4}
export SOURCE_DATE_EPOCH=1789113600
export TMPDIR="$work/tmp"
mkdir -p "$TMPDIR" "$work/prefix" "$work/build-gmp" "$work/build-mpfr" "$work/build-core"
prefix="$work/prefix"
source="$work/sources/giac-bdbb1c41fe52d8b4fb7c1d15c2deaea88fc3803c"
case "$rid" in
  linux-x64) export CC=gcc CXX=g++; platform=linux ;;
  osx-x64|osx-arm64) export CC=clang CXX=clang++ MACOSX_DEPLOYMENT_TARGET=11.0; platform=darwin ;;
  *) echo "Unsupported native Giac RID: $rid" >&2; exit 2 ;;
esac
export CFLAGS='-O2 -std=gnu17'
cd "$work/build-gmp"
if [[ ! -f "$prefix/lib/libgmp.a" ]]; then
  bash "$work/sources/gmp-6.3.0/configure" --disable-shared --enable-static --disable-assembly --prefix="$prefix" >configure.log 2>&1
  make -j"$jobs" >build.log 2>&1
  make install >install.log 2>&1
fi
cd "$work/build-mpfr"
if [[ ! -f "$prefix/lib/libmpfr.a" ]]; then
  bash "$work/sources/mpfr-4.2.2/configure" --disable-shared --enable-static --with-gmp="$prefix" --prefix="$prefix" >configure.log 2>&1
  make -j"$jobs" >build.log 2>&1
  make install >install.log 2>&1
fi
# first.h uses a quoted local gmp.h; replace the upstream prebuilt header with
# the native header produced above, exactly as in the Windows source recipe.
cmp -s "$prefix/include/gmp.h" "$source/src/giac/headers/gmp.h" || cp "$prefix/include/gmp.h" "$source/src/giac/headers/gmp.h"
cd "$work/build-core"
make -f "$recipe/Makefile.posix" -j"$jobs" SOURCE="$source" PREFIX="$prefix" RECIPE="$recipe" PLATFORM="$platform" CXX="$CXX" >build.log 2>&1
strip icas
./icas --version
