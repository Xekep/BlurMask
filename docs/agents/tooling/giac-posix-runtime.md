# Native Giac runtime on Linux and macOS

The POSIX recipe builds a separate `icas` stdin/stdout solver from the same
GeoGebra/Giac commit and the same GMP/MPFR source archives as the Windows recipe.
It never uses upstream prebuilt CAS libraries and never builds Xcas GUI.
`icas.cpp` is the common transport; the Windows recipe files are unchanged.

```bash
bash tools/prepare-giac-runtime.sh linux-x64 OUTPUT SOURCES CACHE 4
bash tools/prepare-giac-runtime.sh osx-arm64 OUTPUT SOURCES CACHE 4
bash tools/prepare-giac-runtime.sh osx-x64 OUTPUT SOURCES CACHE 4
```

The five arguments are RID, output directory, corresponding-source directory,
build cache, and jobs (1–16). Defaults are `linux-x64`,
`.artifacts/giac-runtime/$RID`, `.artifacts/installers`,
`.artifacts/giac-build/$RID`, and 4. A native matching OS/architecture is required;
cross compilation is rejected. Autoconf build-cache and recipe paths must have
no whitespace. Copying the pinned archives into CACHE enables offline source
reuse. Every input is hash checked even when already present.

Linux requires GCC/G++, make, m4, curl, tar, xz, Python 3 and binutils. On Ubuntu
these are provided by `build-essential m4 curl xz-utils python3 binutils`.
macOS requires Xcode command-line tools, make, m4, curl, tar, xz and Python 3.
The archive releases already contain configure scripts; installing Autoconf
or downloading Homebrew GMP/MPFR is unnecessary.

GMP and MPFR are built static on all three RIDs. Linux additionally links the
GCC/C++ runtimes static, retaining only OS glibc-family dependencies. macOS uses
system libc++ and libSystem and targets macOS 11.0. The dependency audit rejects
unexpected shared libraries, including external GMP, MPFR and Linux libstdc++.
This is not a fully static Linux executable: its minimum glibc depends on the
CI host used to build it. The output manifest records compiler identity and
direct system dependencies. Architecture is independently checked with
`readelf` or `lipo` before packaging.

The output contains `icas`, GPL/LGPL/TinyMT notices and texts, `SOURCES.txt`,
and a hash manifest. Linux also supplies the GCC Runtime Library Exception.
Each RID receives its own release asset:
`BlurMask-Giac-corresponding-sources-bdbb1c41-$RID.tar.gz` and SHA256 sidecar.
The source archive contains all three original archives, the complete POSIX
recipe, the common transport, licenses and build instructions. Its internal
manifest omits its own archive hash to avoid a circular hash reference; the
runtime manifest supplies the final archive hash. Reuse requires exact input,
recipe, payload and corresponding-source hashes; unlisted output files or
symbolic links are rejected.

Run `python3 tools/giac-native/test-posix-packaging.py` for independent source
pin, cache, source archive, dependency and architecture controls that execute
no target binary. Building runs eight portable single-line CAS transport
controls on the newly built executable. The same portable controls pass on the
independently source-built Windows executable. Linux/macOS execution must be
validated in native CI; Windows-only syntax/packaging tests do not prove that
either target compiles or evaluates correctly. Application `--math-solver-test`
is a separate end-to-end check and handwriting OCR accuracy is unrelated to
these solver tests.
