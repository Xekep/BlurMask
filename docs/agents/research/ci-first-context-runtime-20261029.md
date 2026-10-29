# CI repairs after the native first-model correction

CI run40806261227 failed at the same ClosedPhysical consumer on Windows,
macOS and Linux. Its corrected FIRST probabilities can retain a legitimate
terminal blank row; the probe still called a deliberately restricted context
helper that refuses such rows. The full owning production context algorithm was
already implemented and used by `LegacyFreshFirstOwnedRecognition`.

## Exact cause and retained algorithm

The unchanged fourteen-physical fixture has 78 points, 57 first nodes and
378 groups. The source fingerprint remains
`AA2AF98C8C550603E797F304DB204AEDD1102482CC8A6DDCC0ED8B62CF8113D2`.
The restricted helper rejects one finite row: node56,row3, blank symbol192,
traits0, arc4,span4,previous0. The historical pre-correction model control admits
the same input without that row. Separate raw-PDB authentication and canonical
source comparison isolate the already proved FirstAcousticModel correction;
this does not justify reverting the native-correct route.

The repair changes ten integration callers from
`RunSupportedNonSkippedContext()` to
`LegacyFirstBlankContextPasses.Run(ak,factory,cuts,prepared,plan,table)`.
Their actual source, factory, cuts, graph, producer, scores and table stay the
same. No primary row, input geometry, cap, score, materializer or expected result
is changed. The two intentional restricted-helper controls and its production
refusal remain intact. No Core production source is modified by this repair.

Strict/full before-primary captures are byte-identical. The full context and
original word materializer succeed. Independent B0/B8 replay checks all 570 finite
records and 8,014 callback observations, including eight blank-parent restrictions
and two blank-context identity shares. Its per-record numerical assertions are
unchanged; only the stress suite's final aggregate coverage requirement is adapted
to this single actual diagnostic case. It does not claim synthetic or nonzero
ancestor-skip coverage.

All ten probe-only builds/PDB gates and original integration pipelines passed
against authenticated cached Core937. The eight-argument DirectWordPass physical
path and the ClosedPhysical active-limiter audit are included. Three isolated
gap-family harness attempts first lacked generated reference-control files;
their original generators supplied those files and the three retries passed,
with failed logs retained. This fixes test consumers, not handwriting quality.

Evidence:
`E:/BlurMask-build-artifacts-20261014/ci-closed-physical-context-next-20261029`.
Root also rebuilt the current main ClosedPhysical source, authenticated its
portable PDB before execution, and reran the complete pipeline plus the original
active-limiter audit successfully. This representative rebuild reuses Core937
only after raw source/PE/PDB authentication.

## Portable raw append test runtime

The Formula runtime ABI job separately stopped before raw append execution:
`LegacyOwnedRawInkAppend.Probe` required exactly Microsoft.NETCore.App 10.0.12,
which was absent on that CI host. This local verification policy had accidentally
been embedded in the portable test project.

The project now targets the ordinary net10.0 minimum without a machine-specific
patch requirement. Authenticated local runners still select10.0.12 with
`dotnet exec --fx-version 10.0.12 --roll-forward Disable`. Root verified the emitted
configuration requests minimum10.0.0 without `Disable`, then reran all279 raw
append controls successfully under the explicit local runtime. No recognition
or package runtime dependency changes.

Runtime-policy evidence:
`E:/BlurMask-build-artifacts-20261014/prior-raw-runtime-policy-root-20261029`.
Canonical main ClosedPhysical verification is recorded separately under
`E:/BlurMask-build-artifacts-20261014/prior-ci-repair-root-20261029`.
These local results do not claim the subsequent full GitHub CI has finished.
