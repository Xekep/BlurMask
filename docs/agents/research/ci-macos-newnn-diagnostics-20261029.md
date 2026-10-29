# Exact unchanged-Math CI mismatch: collect evidence before changing numerics

Run40812151429 failed the macOS ARM64 formula ABI job at
`LegacyMathCurrentSelectedIngress.Probe/audit_unchanged.py`: width1,
reversedfalse, field`NewNn`. The comparison is against the frozen unchanged
reference. It covers raw floating-frame hashes and numeric score fields, so the
outer field name does not establish whether this is platform arithmetic or a
recognizer regression. The job did not upload these captures; an exact leaf
diagnosis is not available from that run's log.

The auditor now prints at most eight differing leaves, with bounded string
values, plus capture/reference hashes before executing its original exact
assertion. Both original assertion ASTs and the reference are unchanged.
The formula ABI job uploads generated Math capture/report JSON on failure for
14days. A subsequent macOS capture must be inspected before proposing a
numerical or reference change; no tolerance or new expected output is admitted.

Root replayed the existing six unchanged cases successfully. Seven diagnostic
function controls checked equality, nested differences, output cap, lengths,
types, long strings and missing keys. A deliberately modified capture still
failed the original assertion and produced the expected leaf diagnostic.

Evidence: `E:/BlurMask-build-artifacts-20261014/ci-macos-prior-newnn-diagnostic-next-20261029`.
The separate Build-job failure in this run is the stale`41268` expectation in
`LegacyFirstOwnedRecognition.Probe`, whose actual result is`40843`. Its42-group
fixture needed its own independent replay; the earlier35-group PlainWord proof
must not be substituted for the whole owning fixture. The later
[owning57 replay](ru-first-owned57-ci-20261029.md) supplies that separate proof
and corrects its stale constant without changing the recognizer.

The separate `Math current-input parity` workflow now exercises just this
integration path on Windows, Linux and macOS. It fetches the existing pinned
official CPU runtime and exports the repository model directly, without waiting
for the full application/native-ABI job chain. Two fresh Managed processes and
one Native AOT process run before the exact comparisons. Captures, runtime
provenance and .NET host information upload on success and failure. Repetition,
Managed/native equality and the frozen unchanged reference remain exact.
This is an additional diagnostic workflow; it does not replace the full CI or
prove the cause before the platform captures have been inspected.
