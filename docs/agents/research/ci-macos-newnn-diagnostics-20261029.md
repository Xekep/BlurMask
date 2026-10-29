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
fixture needs its own independent replay; the earlier35-group PlainWord proof
must not be substituted for the whole owning fixture.
