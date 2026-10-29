# Recognition status after NovaPen 1.0.0

NovaPen 1.0.0 is published from commit `571d310bcdc6bdd3639ee296ca69591ddf3b93fb`.
It does not contain the Russian first-network correction described below.
Russian cursive and the complete English MAIN publication path remain unfinished.
Passing numerical probes is not evidence of complete handwriting recognition.

## Russian first acoustic correction

The owned Russian source at offset 6120 requires mixed exponential/sigmoid
probabilities for its trajectory and raster networks. The previous implementation
used plain exponential probabilities for both. The special output label is read
from the authenticated AL header, rather than inferred from expected text.
The correction is restricted to the proved source fingerprint; other resources
retain their existing dispatch until their own contracts are established.

Native ownership: Text `1B9730 → 1B86F0` passes argument 4 equal to zero;
the AL special label selects `V68 → Engine 0EB6C0 → 0E9360`.
The loaded runtime flag at +78 controls masking, not this converter selection.
Trajectory evaluation/conversion still precedes raster evaluation/conversion.

Evidence stored outside Git under `E:/BlurMask-build-artifacts-20261014`:

- `ru-first-source720-native-frontier-next-20261029`: authenticated dispatch,
  AL layout and independent integer/Decimal numerical reconstruction.
- `ru-first-native-admission-tests-next-20261029`: 261 source-ordered inputs,
  32,530 groups, 11,710,800 Q15 values and 5,855,400 costs checked against
  independent arithmetic. Old-library negative controls reject the old route.
- `ru-first-fix-root-20261029`: root build with all 935 authored core documents
  authenticated through its portable PDB; before/after public calls using the
  same 19 pinned resources/input files and unchanged two-second call limit.

Root also published and ran the first acoustic probe as Windows Native AOT
against that exact candidate library: build 27.45 seconds, run 0.072 seconds,
both exit code zero, CPU affinity limited to two processors. This covers the
real mixed route, 1,265 truncations, 128 analytic input ensembles, shape/mode
and cancellation guards. The real-resource C# reference shares the ordinary
probability kernel; independent Python integer/Decimal checks provide the
separate numerical evidence. It is not a vendor-runtime execution comparison.

All nine public calls completed in both versions. Exact words remain **4/9**;
the correction changes intermediate probabilities, costs and one final score /
geometry result, but does not improve this small corpus's word accuracy.
These continuous-stroke samples do not establish general cursive accuracy.
The next investigation must follow the actual downstream MAIN/linguistic
consumer, without tuning labels, thresholds or budgets to these answers.

The historical PlainWord CI cost is separately corrected using independent
model/recurrence/terminal replay: `41268 → 40843`, with the same literal mixed-script
path and unchanged geometry contribution. See the [CI evidence](ru-first-plainword-ci-20261029.md).
This is an exact test expectation update, not a handwriting quality improvement.

## Remaining boundaries

- English: the 949-document candidate and fresh selected/index continuation are
  still staged. A source-budget refusal is not a successful publication; neither
  the candidate nor diagnostic-only exports prove completed English recognition.
- Math: main now contains 80 authored library documents, including the
  THIRD/candidate/default-diff path and typed temporary-writer kernel. Six real
  neural pipelines reach actual differences and CB8 filtering; all observed
  actions are filtered. This does not prove original-document application,
  revision handling or repeat recognition. Verification and remaining ownership
  boundaries are recorded in [the Math transfer](math-prior-default-diff-20261029.md).
  The [typed temporary application](math-temporary-apply-20261029.md) additionally
  verifies owning empty traversal/header consumption; actual kept mutations and
  original-document updates remain unproved.
- Solver: Giac is the separate upstream CAS. Its tests with supplied expressions
  establish solver behavior, not correct recognition of handwritten integrals.
- Automatic classification, Russian/English end-to-end quality and application
  integration still require broader real-ink checks before declaring completion.

Historical reports retain their original names and hashes. Active engineering
links point here so a deleted old release is not presented as the current build.

## Publication CI after the rename

Run `40779833550` reached the publication source gate on Linux/macOS and rejected
the renamed project metadata. The existing project hash described the historical
BlurMask package ID and repository URL. The gate now authenticates the exact
NovaPen project bytes, inverses only those two metadata substitutions, then
requires the original historical project hash and all existing diagnostic policy
checks. The immutable source proof metadata is unchanged. Unknown project edits,
including diagnostic defaults and distribution guards, remain rejected.

Root reran the full `tools/ci/check-global-publication.py` gate with this change
and the corrected Russian core: default 935 / diagnostic 937 authored PDB gates,
both entries, all three actual publication calls and independent paired audit
passed. Inputs remained unchanged. Evidence:
`E:/BlurMask-build-artifacts-20261014/publication-branding-root-20261029`
(226.71 seconds, CPU2). Four deliberately altered project variants were also
rejected by the metadata authentication helper. This local result does not claim
that the newly queued full GitHub CI run has already finished.
