#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-3.0-or-later
set -eu
work=$(cd "$1" && pwd)
recipe=$(cd "$2" && pwd)
jobs=${3:-4}
# Prefer Git's GNU/MSYS text utilities: busybox's native sed mishandles some
# Autoconf-generated scripts/Windows paths. GCC/binutils/make still come from
# the pinned native toolchain (Git does not provide those compiler tools).
export PATH="/usr/bin:$work/toolchain/w64devkit/bin:$PATH"
export TMPDIR="$work/tmp"
export CC='gcc -std=gnu17'
export M4="$work/m4/usr/bin/m4.exe"
export SOURCE_DATE_EPOCH=1789113600
prefix=$(cygpath -m "$work/prefix")
bash_path=$(cygpath -m -s /usr/bin/bash.exe)
mkdir -p "$work/tmp" "$work/prefix" "$work/build-gmp" "$work/build-mpfr" "$work/build-core-win64"
cd "$work/build-gmp"
if [ ! -f "$work/prefix/lib/libgmp.a" ]; then
  bash ../sources/gmp-6.3.0/configure --host=x86_64-w64-mingw32 --build=x86_64-w64-mingw32 --disable-shared --enable-static --disable-assembly --prefix="$prefix" >configure.log 2>&1
  # Stay comfortably below Windows' command-line limit when libtool packs
  # hundreds of objects into a static archive.
  sed -i 's/^max_cmd_len=.*/max_cmd_len=4000/' libtool
  make -j"$jobs" SHELL="$bash_path" >build.log 2>&1
  make install SHELL="$bash_path" >install.log 2>&1
fi
# Native Windows make/libtool can truncate the final long link recipe at the
# shell's 8192-character boundary. Every object has already compiled; use an
# explicit GNU ar response file so all prescribed library objects are retained,
# including the random-state routines at the tail of the upstream recipe.
objects=( *.o mpf/*.o mpz/*.o mpq/*.o mpn/*.o printf/*.o scanf/*.o rand/*.o )
printf '%s\n' "${objects[@]}" >libgmp-complete.objects
rm -f libgmp-complete.a
ar qc libgmp-complete.a @libgmp-complete.objects
ranlib libgmp-complete.a
cp libgmp-complete.a "$work/prefix/lib/libgmp.a"
nm "$work/prefix/lib/libgmp.a" | grep -q 'T __gmp_randinit_default'
cd "$work/build-mpfr"
if [ ! -f "$work/prefix/lib/libmpfr.a" ]; then
  bash ../sources/mpfr-4.2.2/configure --host=x86_64-w64-mingw32 --build=x86_64-w64-mingw32 --disable-shared --enable-static --with-gmp="$prefix" --prefix="$prefix" >configure.log 2>&1
  sed -i 's/^max_cmd_len=.*/max_cmd_len=4000/' libtool
  make -j"$jobs" SHELL="$bash_path" >build.log 2>&1
  make install SHELL="$bash_path" >install.log 2>&1
fi
# Upstream first.h includes "gmp.h" from its own directory. Replace its
# platform-specific prebuilt header with the header generated for this build.
header="$work/sources/giac-bdbb1c41fe52d8b4fb7c1d15c2deaea88fc3803c/src/giac/headers/gmp.h"
cmp -s "$work/prefix/include/gmp.h" "$header" || cp "$work/prefix/include/gmp.h" "$header"
cd "$work/build-core-win64"
make -f "$(cygpath -m "$recipe/Makefile")" -j"$jobs" SOURCE="$(cygpath -m "$work/sources/giac-bdbb1c41fe52d8b4fb7c1d15c2deaea88fc3803c")" PREFIX="$prefix" RECIPE="$(cygpath -m "$recipe")" >build.log 2>&1
./icas.exe --version
