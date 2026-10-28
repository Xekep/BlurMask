# Default Engine string service: DOM decimal parse and scalar formatting

Static-only source: MyScriptEngine.dll SHA256
`d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c`.
Bootstrap031910 / descriptor252408 ->20F3C0 installs default global90
service37 table20F290. Host overrides are outside these kernels.

PE section-mapped table pointers: V88=089DB0, A8=08AE00,
B0=08AE70, B8=08AEF0, C0=08AFD0 (all VA base180000000).
Read-only batch parser export `.artifacts/dom-parser-20261028-export`
passed; formatter `.artifacts/dom-formatter-valid-20261028-export` passed
for B0/B8/C0. Their instruction bytes were independently matched to PE.
The first formatter batch is intentionally retained as failed: A8 is absent
as a function entry in the existing database. `static_pe_evidence.py` disassembles
its exact PE bytes with Capstone without changing the project.

`ParseDecimal` covers only base10, with an explicitly bounded byte interval.
Flags bit1 skips initial ASCII09..0D and20. Bit4 rejects either leading sign;
other bits have no effect. A single optional sign is otherwise accepted.
At least one digit is required. NUL, high bytes and all nondigits stop parsing;
trailing bytes are not consumed, and success returns their offset. Overflow is
unchecked modulo2^64, including final negation. Failure preserves output.
The kernel returns -1 for native null. It does not impose an all-input-consumed
condition, UTF8 decoding, range checks or trailing-space consumption.

`FormatSigned` is B8 decimal signed64, including INT64_MIN. It returns the
byte count excluding NUL; a buffer needs count+1 to be written. An insufficient
buffer is unchanged. Native unsigned B0 similarly accepts a caller base and
uses lowercase digits. Its general-base kernel is not included here.

A8 copies `true\0` (4+1 bytes from1801DCF80) or `false\0`
(5+1 bytes from1801DCF88), returns4 or5 regardless of capacity, and leaves
insufficient buffers untouched. The native bool entry also skips writes for
null destination or source/destination identity; spans cover bounded buffers.

C0 uses custom dtoa helper08BBD0, decimal/exponent choice and precision/buffer
adjustments. No .NET float formatting equivalence is claimed or implemented.
Caller09/17 precision is not sufficient evidence for a replacement.

Validation uses a Python arbitrary-precision regex/int/string oracle rather
than executing vendor code. 1760 parse fixtures include overflow, signs,
whitespace, trailing bytes and all flag combinations. 1252 formatting fixtures
include signed extremes, random values, booleans and short/exact/extra buffers.
The tiny managed Release probe passed all3012 fixtures.

```powershell
python -B tools/handwriting/ports/dom-parser-20261028/reference_controls.py .artifacts/dom-parser-controls-20261028.json
dotnet run --project tools/handwriting/ports/dom-parser-20261028/Probe.csproj -c Release -- .artifacts/dom-parser-controls-20261028.json
python -B tools/handwriting/ports/dom-parser-20261028/static_pe_evidence.py research/myscript/sdk/build/x64/MyScriptEngine.dll
```
