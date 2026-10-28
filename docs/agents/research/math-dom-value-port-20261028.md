# DOM numeric values and actual fresh Math publication

This increment ports value operations into `InkRecognition.Math`, using the
new read-only batch exporter. It does not complete CB8 edit reuse or Math2.

## Restored operations

- Engine default global90 service37 V88/089DB0, **base10 only**: optional ASCII
  whitespace under flag1, optional sign unless flag4, digit-prefix end offset,
  unchanged output on failure and native unchecked64-bit wrap on overflow.
- B8/08AEF0 signed decimal, B0/08AE70 unsigned decimal and A8/08AE00 boolean
  formatting. Required length excludes NUL; short buffers remain unchanged.
- Document0078D0 typed value-to-text dispatch: borrowed heap/inline text,
  signed inline-length byte with zero meaning15, nonnull empty values, signed
  and unsigned scalar lanes, boolean and exact floating-service arguments.
- Document031E20 numeric attribute reader: full bounded consumption for strings,
  raw64/sign-extended32/zero-extended32/boolean cases and the actual floating
  +/-0.5 then x64 CVTT conversion. NaN/infinity/overflow produce native integer
  indefinite, not the caller fallback. Unsupported types report02000000 before
  fallback; malformed strings do not report that error.

`Math2DomStringService.cs` implements the default service primitives.
`Math2DomValueConversion.cs` provides borrowed stack-only views and dispatch.
They convert DOM identifiers/values; they are not a formula calculator and do
not add a second solver beside Giac.

## Genuine consuming caller

Fresh publisher registration still admits **tag4 only** from the actual temporary
DOM. It now uses the proven031E20 typed branch and0336C0's INT64_MAX sentinel.
Tag collection uses033870's distinct -1 sentinel. No byte serialization or new
per-tag allocation is introduced; existing owner/lifetime/preflight checks remain.

This is a real caller integration, but current tag4 values bypass0078D0. String
and floating branches are separately tested, not claimed to be reached by that
producer. The opcode7 edit matcher still needs an actual prior nonempty result,
its retained Ink/registry/ranges and produced edit actions. Snapshot-shaped test
data cannot supply those owners. No acceptance policy or source quota is widened.

## Static evidence

Fresh Document batch exports cover0078D0/007B50/032800 and031E20/0336C0/033870,
from PE SHA256 `17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f`.
Engine V88 and B0/B8/C0 exports bind the default vtable20F290 to their actual
function pointers, PE SHA256 `d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c`.
Every exported instruction is independently compared with the original PE.
A8 has no function entry in the retained Ghidra database; its exact PE instructions
and true/false literals are checked without modifying that database.

Requests and reproducible controls: [DOM batch](../../../tools/handwriting/ports/dom-value-20261028/request.json),
[default service](../../../tools/handwriting/ports/dom-parser-20261028/README.md),
[caller analysis](../../../tools/handwriting/ports/cb8-owner-20261028/README.md).
Local exports remain in ignored research/`.artifacts` directories; no vendor
engine was loaded or executed.

## Verification

4852 checks execute the actual compiled library: independent arbitrary-precision
integer/string controls, raw IEEE numeric fixtures, typed storage/length/dispatch
and sentinels. Floating text tests verify service arguments only. The full Math
library and probe passed Portable PDB/source binding against the reviewed sources.

The existing fresh publisher probe then ran six real model/official-ORT cases
under CPU2 with the same new Math DLL. Its independent oracle passed:70 observed
failure prefixes,180 guards and zero feature error. New sentinel controls leave
the old capture schema unchanged; capture SHA256 is
`574c580830e6f8c509422be486fdc5e0be1280a1d29e4b95dae5c174d959c51f`.
Local evidence: `E:/BlurMask-build-artifacts-20261014/math-dom-port-20261028/`.
Its test project uses an explicit prebuilt unchanged core DLL instead of rebuilding
935 text files; actual Math source files are copied byte-exact and PDB-authenticated.

```powershell
python -B tools/handwriting/ports/dom-parser-20261028/reference_controls.py .artifacts/dom-strings.json
python -B tools/Math2DomValue.Probe/reference_controls.py .artifacts/dom-numbers.json
dotnet run --project tools/Math2DomValue.Probe -c Release -- .artifacts/dom-strings.json .artifacts/dom-numbers.json
```

The workflow `Math DOM value port` runs managed and Native AOT checks on Windows,
Linux and macOS using the same controls frozen before compilation.
All three managed/Native AOT jobs passed in
[run40705184780](https://github.com/Xekep/BlurMask/actions/runs/40705184780)
for code commit `15de9737c5d9c24635ccfa99553ad02d46d5f304`.
This increment is development after release1.2.8.1; its published artifacts
are unchanged by these source commits.

## Remaining boundary

The floating text formatter C0 uses custom dtoa. The subsequent
[bounded layout port](math-dom-floating-layout-20261028.md) implements composition
and default signed-zero/special conversion. Nonzero finite dtoa still requires
an explicit generator; `.NET ToString` is not substituted. True prior-result editing, nonfresh
owner construction and whole recognition integration remain unfinished. These
controls establish neither handwriting accuracy nor full engine completion.

The [next-dependency packet](../../../tools/handwriting/ports/dom-float-20261028/README.md)
records the freshly exported dtoa/helpers, exact notation branches and rounding
environment query. Its unusual NaN/Infinity outputs are static predictions,
not vendor runtime observations. That original packet itself remains static
evidence; the subsequent layout port documents the admitted implementation.
