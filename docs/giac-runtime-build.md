# Native Windows Giac runtime

`tools/prepare-giac-runtime.ps1` builds a standalone, statically linked Windows
x64 `icas.exe` from fixed source archives. This is BlurMask's minimal console
transport around Giac, not the Xcas graphical installer or an upstream release
named `2.0.0-21`. `--version` identifies the precise Giac commit.

The upstream [Giac compilation guide](https://www-fourier.univ-grenoble-alpes.fr/~parisse/giac_compile.html)
links the [GeoGebra Giac repository](https://github.com/geogebra/giac). The build
pins commit `bdbb1c41fe52d8b4fb7c1d15c2deaea88fc3803c` (2026-09-11), GMP 6.3.0,
MPFR 4.2.2 and w64devkit 2.10.0 / GCC 16.2.0. Every download has a checked
SHA256 in the helper and runtime manifest. It does not link the Giac repository's
prebuilt GMP/MPFR libraries. FLTK, readline and optional external CAS libraries
are excluded.

## Build and package

Install Git for Windows and 7-Zip, then run from PowerShell:

```powershell
./tools/prepare-giac-runtime.ps1 `
  -OutputDirectory .artifacts/giac-runtime/win-x64 `
  -SourcesDirectory .artifacts/installers `
  -CacheDirectory D:/BlurMask-Giac-build `
  -Jobs 4
```

The build cache must have a path without spaces. Source builds take several
minutes and need roughly 1.5 GB of working space. Logs remain under the cache's
`build-gmp`, `build-mpfr` and `build-core-win64` directories.

The runtime directory contains the executable, full GPL text, third-party
notices, dependency licenses, source instructions and `manifest.json`.
Only Windows system DLLs are permitted by the PE dependency check. The verified
fresh local build imports `KERNEL32.dll` and `msvcrt.dll` and has a 19,582,464-byte
executable (about 18.7 MiB); compiler/build changes may alter that size/hash.

The separately uploaded release asset is
`BlurMask-Giac-corresponding-sources-bdbb1c41.tar.gz`, with a `.sha256` sidecar.
It contains the exact complete Giac/GMP/MPFR source archives, BlurMask transport,
build helper/recipe and licenses. Build-tool source provenance is pinned through
the toolchain release and its Dockerfile; toolchain executables are not installed
with BlurMask. The runtime's manifest binds the source archive and executable
hashes. Do not publish a runtime without its matching source asset. Old Xcas
installer snapshots containing mismatched `doc/giac-1.7.0` are not used.

Completed runtime caching checks the recipe hash (helper plus every recipe file),
all pinned input hashes/URLs, executable/payload hashes and the actual source
archive hash. A failed check rebuilds from source. A source build cache only
avoids repeated compilation; it must not be shared as a trusted binary artifact
from an unrelated origin.

## Protocol and verification

The transport accepts `--sage`, reads commands from stdin and writes quoted
single-line TeX using literal backslashes, as the application's existing Giac
protocol expects. It evaluates the inner expression with Giac and serializes
actual vector/list/matrix structures. Numeric-only legacy `\mathrm{-digits}`
wrappers and the sign of rational values are formatted without changing values.
Matrix rows are kept on one line. Root lists preserve their list structure so
the application can distinguish a computed root set from an unresolved variable.
It does not substitute guessed answers or implement a second CAS.

```powershell
./tools/giac-native/test-transport.ps1 -Executable .artifacts/giac-runtime/win-x64/icas.exe
dotnet run --project tools/InkMath.Solver.Probe -c Release -- .artifacts/giac-runtime/win-x64/icas.exe
```

Verified headlessly against the source-built runtime: 8 explicit transport
controls; 31 grammar translations; 20 built-in exact integral fixtures with
independent Giac comparisons; 29 actual Giac calculus/algebra fixtures; running
and pre-start cancellation; undefined, identity, unchanged-input and injection
guards. No Xcas or BlurMask GUI was launched. Installer placement, bundled
discovery and installed application tests belong to the installer workflow.

### Windows build details

GMP's old configure tests use pre-C23 empty parameter lists, so the C compiler is
invoked with `-std=gnu17`. Git's MSYS shell and a pinned build-only MSYS m4 package
run Autoconf. Git's GNU text utilities precede the portable toolchain's BusyBox
utilities on PATH; the compiler, binutils and make still come from the pinned
toolchain. m4's MSYS runtime DLLs are copied from Git for Windows and never enter
the installed payload. GMP's generated Win64 `gmp.h` replaces upstream's bundled
platform header because Giac's `first.h` uses a quoted local include.

Native Windows make/libtool can truncate GMP's long final archive recipe at the
shell command limit. The build explicitly archives all compiled library object
groups through GNU ar's response file, retaining all 516 objects in the verified
GMP build, including its random-state routines. MPFR is then built against that
library. Giac uses the pinned upstream CMake source list/Windows definitions plus
the small transport. Make dependencies include the recipe and generated header;
an unchanged header is not copied again. PE timestamps are disabled at link time.
