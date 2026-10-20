# Russian recognition: fixed blind diagnostic

The current experimental application adapter is still the older first-stage
recognizer. The independently executable full global dictionary path produces
better results on this diagnostic, but neither result establishes usable Russian
cursive recognition or completion of the engine.

| Unchanged path | Exact words / all inputs | Character edits / reference characters | CER |
| --- | --- | --- | --- |
| A: full global raw WORD output | 136 / 261 (52.11%) | 334 / 1276 | 26.18% |
| B: actual source6120 first-stage owner | 29 / 261 (11.11%) | 727 / 1276 | 56.97% |
| C: current experimental UI adapter | 29 / 261 (11.11%) | 727 / 1276 | 56.97% |

All paths use the same frozen input coordinates and acoustic resource. Each has
257 supported/nonempty outputs and four refusals or empty outputs; all261 remain
in the denominator. B and C return identical text on every input. A correctly
recognizes107 words that B misses; B recognizes no complete word that A misses.
The minimum-score gate and two-second timeout in C do not explain this gap.
This compares complete pipelines; it does not isolate a causal dictionary effect.

## Input and evaluation provenance

The recordings come from the already pinned Russian track archive described in
the [coordinate fixture attribution](../tools/test-data/russian-recorded-words/README.md).
The archive SHA256 is
`2960052698884e00d8fa428295f538d52c30f47ea3f7695e5c7be610fd943edf`.
The broader set includes ten writers and29 sessions, excluding all writers3,7,12
used by the original nine-word diagnostic. Selection follows available source
files rather than recognition outputs. Original X is preserved and original Y
is negated, matching the committed loader. Each record is one continuous track:
pen-up metadata is unavailable, and no release events or stroke splits are
invented. Original targets, including a mixed Latin character, remain unchanged.

Inference receives only blind coordinates. Expected targets are opened after
all outputs and the independent raw-acoustic-alphabet emission audit. The first
stage uses its actual inline AL103 namespace, not the main alphabet;1227 emitted
Glyph40 rows were decoded independently. Managed and NativeAOT captures are
byte-identical. This set is not representative, and overlap with vendor model
training is unknown. No running proprietary SDK was used as an accuracy oracle.

| Frozen evidence | SHA256 |
| --- | --- |
| Blind coordinates | `c8fe150f3ca960cf7216da88c1dd90e1045aed2ef4652a8fa66c9e7a85353dce` |
| Full-global capture | `83f9abee51b93e369768e6900c3e449499b2878354e4b80590888591236418ac` |
| B/C capture | `9b9565a0f9f89303ec51670d926981067a4c786e3dab7fd4e3c828d74245b19a` |
| Global diagnostic manifest (63 artifacts) | `67388523035ae4c491551a8bd071039da2c978bdb203db0a6d2e4f520e1703fe` |
| A/B/C manifest (15 artifacts) | `e5030cdcefb990551008d6926761a8b45736536b016f62df28d92152ef2a6e3f` |

The immutable evidence remains under
`E:/BlurMask-build-artifacts-20261014/`, in the two corresponding
`russian-*-frozen-manifest-20261020.json` manifests. Those manifests reference
exact source, capture, input, evaluation and diagnostic files; they must be
verified before reuse. Large query snapshots are not duplicated into the
repository. The root review rechecked all78 referenced artifact hashes and
independently recomputed all-input Levenshtein totals from the captures and
post-inference targets.

## What still fails

The original-nine diagnostic remains4/9 exact with16 edits in33 reference
characters. Snapshot-only taps preserve its original full capture. Correct
dictionary candidates for the failing words are sometimes emitted and then
discarded by bounded cost heaps or the primary quota. In another case the correct
acoustic word survives but loses after language costs. These are not evidence
of a missing Cyrillic alphabet or absent word dictionary.

The one-track inputs produce no physical multi-stroke context windows. That is
an input/model mismatch possibility, not proof that inventing pen-up events
would fix recognition. No beam size, language weight, score threshold, model
parameter or output filter was tuned against these expected answers.

The next application integration must preserve actual global output provenance,
define an explicit admission policy and retain ink on unsupported or uncertain
inputs. Raw ranking costs must not be relabeled as calibrated confidence.
English main inference, automatic text/formula choice, placement and original
document acceptance remain separate work. This diagnostic does not enable a new
default recognizer or qualify a release.
