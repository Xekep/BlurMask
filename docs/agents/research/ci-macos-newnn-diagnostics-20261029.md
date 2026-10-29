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

## First focused run and exact raw tensor sidecars

Run40822882800 completed: Windows and Linux pass; macOS fails the retained
frozen-reference assertion. The uploaded macOS Managed, repeated-Managed and
Native captures are byte-identical. Exhaustive comparison with the reference
finds exactly42 differing leaves: six unchanged cases times seven `RawHashes`.
Every other leaf agrees, including all FirstCopy score bits, text, ranges and
position data. Independent comparison receipt:
`E:/BlurMask-build-artifacts-20261014/macos-prior-newnn-independent-next-20261029/ALL-LEAVES.json`,
SHA256 `d228c798bf2f68ae1a99565d42051fad8672628d319db5c593cb9afbe4c82ee4`.
Hashes alone do not establish numerical error magnitude or its source.

The opt-in probe sidecar now copies exact float bits through the existing
same-owner completed-history inspection method. Two narrowly retained private
fields are read to locate that actual history; no field is written and no
tensor enters recognition. Each copied frame must reproduce its existing hash.
The original473 controls, all original assertions, reference and model policy
remain unchanged. Sidecar input pins include the loaded ORT binary and model;
both unchanged and changed six-case routes are collected before decoder disposal.

Root authenticated all three probe sources via PDB. Disabled, enabled and
repeated Managed runs preserve the original capture exactly. Native AOT and its
exact sidecar audit also pass: all8316 observed values across12 histories are
finite, with no NaN/infinity. Actual bits and dimensions match between repeated
Managed and Native runs. This is Windows evidence, not a cross-platform error
bound. No tolerance or reference exception has been added.

The first native link failed on an existing265-character NuGet library path.
That attempt is preserved. A second publish used a shorter package root after
checking the library bytes were identical; it passed with no source change.
Evidence: `E:/BlurMask-build-artifacts-20261014/math-current-raw-attention-sidecar-independent-next-20261029`.
`root-native2/FINAL.json` SHA256:
`3a20934d487995d53c61840cd11c6a6e833f8f60a4773fe7d08a9c958ca99927`.
The next platform sidecars are needed before changing any numeric contract.
