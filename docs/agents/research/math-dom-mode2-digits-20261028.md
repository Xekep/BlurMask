# DOM mode2: exact finite digit generation

This closes the finite digit-generator boundary left by the
[bounded floating compositor](math-dom-floating-layout-20261028.md) for the
actual Document0078D0 caller. It does not complete the full recognition engine,
nonfresh CB8/result ownership, Russian/English cursive, or all dtoa modes0..9.

## Production boundary

[Math2DomExactDigits](../../../src/InkRecognition.Math/Math2DomExactDigits.cs)
implements the reviewed **mode2, nearest-even, precision9/17** significant-digit
contract using integer arithmetic. Default DOM text conversion now uses it for
finite nonzero values. Signed zero and specials retain the previous instruction-
derived branch behavior, including the unusual native NaN/Infinity spelling.
No SDK binary, C reference, additional native library or network service is
required at runtime. The generator adds no NuGet dependency.

For binary64, decode sign, exponent and significand from the original bits.
Normals use `(2^52 + fraction) * 2^(exponent-1075)`; subnormals use
`fraction * 2^-1074`. Convert that exact value to numerator/denominator. An integer
approximation to log10 is only a starting point: rational comparisons with
powers10 establish `10^(dp-1) <= value < 10^dp` exactly. Scale by10^(precision-dp),
divide, compare twice the remainder with the denominator and round ties upward
only for an odd quotient. A carry to10^precision increments dp. Remove trailing
zeros without changing dp. The quotient is bounded by10^17, so its final decimal
write uses UInt64 and the supplied span. The nonzero lower bound10^(precision-1)
justifies the trailing-zero loop and prevents a zero-quotient path.

An allocation-free fast path handles exact positive magnitudes fitting UInt64.
It first proves that IEEE bit shifts lose no fractional or high bits, determines
the decimal width, and performs the same quotient/remainder nearest-even rounding
when width exceeds precision. The largest divisor is10^11; doubling its remainder
cannot overflow UInt64. Other values use BigInteger with bounded binary exponents
and powers10 (at most340 for the admitted precisions). There are no shared mutable
number buffers or per-call caches. The component allocation control covers10000
warmed exact-integer calls only; general rational conversion does allocate.

Explicit custom formatter injection remains available. `SpecialValuesOnly`
still refuses nonzero finite values, allowing its refusal contract to be tested.
Precision, output capacity and finite/zero/special classification guards remain.
The supported native-comparison environment is IEEE nearest rounding, gradual
underflow, with DAZ/FTZ disabled. Arbitrarily altered floating environments are
not admitted as equivalent to native execution.

## Native algorithm evidence and remaining limits

The new read-only batch export of Engine08AFD0 and08BBD0 passed in16.42seconds:
`.artifacts/dom-dtoa-contract-20261028-export`. Its PE instruction bytes and
project/tool hashes are checked by the batch tool. The binary SHA256 remains
`d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c`.

Native mode2 clears the left/right mode at08C004..08C025 and uses the requested
positive digit limit. The quick path is gated by limit<=14 and rounding query1
at08C0DC..08C0E9; precision17 bypasses it. The non-left/right bigint loop at
08CB41..08CB95 extracts decimal quotients, stops on a zero remainder or the digit
limit, then compares twice the remainder at08CBA1..08CBCF. Equality rounds up only
for an odd last digit.08CBD5..08CBE3 strips trailing zeros;08CB11..08CB39 and
08CBE8..08CBF4 propagate carry and adjust the decimal position. The small-integer
path at08C470 onward applies the same halfway/parity rule. The quick path checks
conservative error bounds and falls back when a final rounding decision is
ambiguous (08C3F0..08C456).

This implementation follows that numeric contract, **not** a line-by-line port
of every allocator and floating quick-path instruction. Original exact source
revision identification and arbitrary mode/rounding/capacity behavior remain
outside this change. The vendor engine was not executed. Static branch matching
plus the independent numerical controls below do not claim byte-for-byte vendor
runtime equivalence for every possible input or whole-engine completion.

## Two independent numerical references

[Python Decimal controls](../../../tools/Math2DomValue.Probe/finite_controls.py)
round exact binary floats with a decimal significant-precision context. The C#
implementation does not import this oracle.91556 frozen tuples cover all2047
finite binary64 exponent fields with multiple fraction patterns, all finite
binary32 exponent fields, random64/32-bit values, neighbors of powers10, exact
halfway cases, both signs, both precisions, UInt64 path boundaries and small
integers. Both digit tuples and final text are checked. All binary64 precision17
texts must also round-trip to the original bits; promoted binary32 values exercise
the actual default tag10 conversion.

A second reference is [David Gay's Netlib dtoa](https://netlib.org/fp/dtoa.c),
preserved unmodified with its complete permission notice and
[source provenance](../../../tools/Math2DomValue.Probe/reference/PROVENANCE.md).
The standalone C driver compares mode2 with mode6 (quick path disabled) for every
tuple, then compares both with the precompiled Decimal controls. Its NO_BF96 build
retains the older quick-path/bigint structure. The fetched revision is not asserted
to be the vendor's original source version. This C code is test tooling only.

[run_reference.py](../../../tools/Math2DomValue.Probe/reference/run_reference.py)
builds the pinned source with x64 MSVC on Windows or cc on Linux/macOS, records
compiler/source/control hashes, limits subprocess time and checks inputs again
after execution. The workflow freezes controls and verifies the C reference
**before** compiling/running the managed implementation. All inputs are then
reused unchanged for Native AOT checks.

## Local final evidence

`E:/BlurMask-build-artifacts-20261014/math-dom-dtoa-20261028-final/` retains the
final source/PDB proof for68 authored Math files plus the probe, the unchanged
explicit core DLL pin, frozen controls, independent C output and build/runtime
logs. No GUI or Explorer process was launched.

- 91556 Netlib mode2/mode6/Decimal tuples agree.
- Managed library probe:544291 numeric, DOM, extent, refusal, allocation, layout
  and finite mode2 checks passed in2.03seconds in this local CPU2 envelope.
- Windows Native AOT passed the same544291 checks in0.69seconds; publish28.85seconds.
- Six actual model/official-ORT fresh publications passed the independent oracle:
  cold1/prepared3/16,70 failure prefixes,180 guards, zero feature error. The
  unchanged prebuilt probe source was compared with the repository byte-for-byte,
  and its Math dependency was replaced by this exact newly reviewed DLL.
- Final Math DLL SHA256:
  `63396aa7fef07d2c9f49c8e79dc1898aaf404d8cea5c130deef24507693da78b`.
- Frozen finite controls SHA256:
  `1b59cd468381fc82b23407d9600e77b5aa9ca86bf2a8f76182706ff379406137`.

The timings are individual verification runs, not recognition performance claims.
The fresh Math publication path still consumes only its admitted tag4 attributes;
these formatting checks do not manufacture an actual prior result, opcode7 owner,
recognition acceptance or improved handwriting quality.

The independent C reference, managed implementation and Native AOT all passed on
Windows, Linux and macOS in [run40731690820](https://github.com/Xekep/BlurMask/actions/runs/40731690820)
for code commit `cb620b77d4a025e490a3dbf0d77496892e5db385`. Source pins and
all three job/step summaries are retained locally in `ci-verification.json`.
