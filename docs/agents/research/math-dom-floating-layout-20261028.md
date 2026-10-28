# DOM floating text: bounded layout port

The subsequent [finite mode2 generator](math-dom-mode2-digits-20261028.md) closes
the nonzero finite boundary described below for the actual precision9/17 caller.
This file retains the earlier compositor-stage scope and verification evidence.

This is development after 1.2.8.1. The full recognition engine, nonzero finite
dtoa and prior-result ownership are not complete. This increment establishes
text composition from a supplied digit tuple; it does not improve or measure
Russian/English handwriting accuracy.

## Code and admission

[Math2DomFloatingFormatter](../../../src/InkRecognition.Math/Math2DomFloatingFormatter.cs)
ports Engine08AFD0's first output pass for the actual Document0078D0 caller:
precision9/17, parameter-1, destination capacity at least50 and digit count no
larger than precision. It preserves the digit-count-dependent notation test,
negative sign, decimal-point placement, zero padding, lowercase exponent with
no positive plus sign, and final NUL. Finite output is at most24 bytes excluding
NUL; the special output is at most15. Therefore the original buffer retry and
secondary last-digit increment are outside this admitted contract.

All tuple validation happens before destination mutation. Source and destination
must be disjoint, just as native dtoa's local buffer is disjoint from the caller's
output. Finite decimal positions are bounded to[-323,309]; special position9999
requires exactly `Infinity` or `NaN`. Invalid precision, lengths, bytes, position,
aliases and capacity refuse explicitly. A supplied finite generator cannot change
the input sign or classification. None of these checks confers DOM ownership.

The default DOM floating service now handles signed zero and IEEE special values
through the statically closed08BBD0 branches. It inspects original sign bits before
classification. For fidelity to the wrapper's branches, special output is
`I.nfinitye9998` / `N.aNe9998`, optionally prefixed by minus. This spelling follows
the instruction trace, **not vendor runtime observations**, and is not normalized
to conventional .NET strings. IEEE default floating environment is assumed as in
the previous numeric port; arbitrary altered MXCSR/DAZ/FTZ is not admitted.

Finite nonzero formatting still throws without an explicit
`IMath2DomDigitGenerator`. The generator seam is tested with a supplied tuple only;
that test is not an implementation or equivalence proof of dtoa. Normal values,
including minimum subnormals, are never silently formatted by .NET ToString.

## Static evidence

The [batch request](../../../tools/handwriting/ports/dom-float-20261028/layout-request.json)
exports08AFD0 and08BBD0 together using the new read-only exporter. Source PE SHA256:
`d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c`.
Local result `.artifacts/dom-floating-layout-20261028-export`: passed, two functions,
15.87seconds, instruction bytes compared with PE and project hashes unchanged.
The vendor runtime was not loaded.

Key ranges:08B080..08B0A3 notation;08B104..08B165 required length;
08B186..08B19F secondary increment;08B19F..08B2F3 rendering/NUL;
08BC7C..08BDB7 sign/zero/special classification. The
[previous dependency packet](../../../tools/handwriting/ports/dom-float-20261028/README.md)
retains the unclosed dtoa quick paths, rounding-environment query and next work.

## Verification

The existing `Math2DomValue.Probe` consumes Python controls frozen before C# build:
every finite decimal position[-323,309], every length1..precision, both signs,
both precisions, plus zero and special tuples:32928 composition fixtures.
The oracle inserts strings by decimal position rather than copying the native
output loops; ten explicit notation/special anchors check the oracle itself.
Twenty raw IEEE fixtures exercise actual tag5/tag10 default DOM conversion,
including signed zero, infinities, signaling/quiet NaN payloads and signs.
Refusal, extent and allocation checks supplement the prior integer/DOM tests.

The test measures compositor allocation only (10000 warmed calls). It does not
benchmark whole recognition, establish finite digit generation equivalence or
widen formula admission. The existing workflow runs the same probe in managed
and Native AOT builds on Windows, Linux and macOS.

Local final build/PDB/runtime evidence is retained at
`E:/BlurMask-build-artifacts-20261014/math-dom-layout-20261028-final/`.
Source binding covers all67 authored Math files and the probe. The unchanged
core DLL is explicitly pinned rather than rebuilding the text engine.

The managed final probe passed70775 checks against Math DLL SHA256
`37a6c1453fa76f0f51f2806946d29e90dd343780ec1ecafc5628eb1a2e9bb10a`.
The unchanged fresh publisher probe ran against this same DLL with the actual
model and official ONNX Runtime: six publications,70 failure prefixes and180
guards passed its independent oracle. This reuses the previously built unchanged
probe (its archived source was byte-compared with the repository); it is not a
claim that a DOM floating field was produced by that narrow tag4 publication path.
Windows Native AOT passed the same70775 frozen-input checks; publish25.39seconds,
native execution0.08seconds in this local CPU2 envelope. These are individual
verification timings, not an application performance comparison.

Managed and Native AOT checks on Windows, Linux and macOS all passed in
[workflow40720795216](https://github.com/Xekep/BlurMask/actions/runs/40720795216)
for code commit `9f99160ec9c3d5952d0112c38425e1c82d52da34`.
This confirms these numerical/storage/layout controls across those platforms;
it does not establish the remaining finite dtoa or recognition algorithms.
