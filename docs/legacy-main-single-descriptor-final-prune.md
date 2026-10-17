# Native final-node eligibility for one main descriptor

The owning `LegacyFreshMainSingleLineSampler.Sample` now calls additive
`LegacyOrdinarySamplerGroupPruning.PrepareSingleDescriptor`. The earlier
`Prepare` API keeps its local-only behavior for existing prepared consumers.
No group IDs, group ranges, seed ownership or endpoint context IDs are remapped.
The resulting arc ordinals retain their exact original provenance, and the
existing source-range owner rebinds to that same pruning result.

The missing native step was explicit. `1A61C0` performs local mode0 on
nodes0..N−2. Later `19E8B0` performs boundary mode1 then mode0 at the descriptor's
final node. For one ordinary descriptor every group has secondaryFFFF;
`1A5C28→1A616F` therefore skips all mode1 callbacks. Ordinary194140 eligibility
depends only on stored ranges/span and assigned guide, so local nodes followed
by final mode0 equal one ascending all-node eligibility pass. Reciprocal edges
are removed/renumbered using the existing algorithm; arbitrary global source
pair metadata is not admitted by this single-descriptor method. The new complete
pass also preserves native1A5BD0/1A5BE6's signed16 traversal gate.

There is a genuine owned raw-input regression: two open M-shaped physical
strokes, atX0 and100, Y10..90, pass real first TEXT→source720→24B/24D/29B/265.
The assigned main height is40 and actual11C is20480. Local pruning removes12
arcs but leaves four final candidates, original groups32..35/spans4..7, whose
raw separation is21558. Native final mode0 removes those four. Wider raw gaps
also exercise the correction, without changing a model, threshold, expected
text, mask or source height. No OCR label accuracy is asserted.

`LegacyMainFinalNodePruning.Probe` checks72 prepared local/full pairs and four
actual owning inputs. Three actual inputs reject four final candidates each;
theX70 input remains unchanged. Independent Python recomputes raw binary32
geometry and literal mutable deletion for local and full passes, including
all retained original ordinals and reciprocal vectors. C# verifies96 retained
actual source-range callbacks and rejects foreign/canceled bindings.

The existing14-physical main ingress and first local-pruning gates remain
regressions. Only actual above-threshold final candidates should change; native
group compaction and globally indexed multiline227/NN ownership remain separate.

Probe arguments: AK, engine, separation, case, publication, line30, line50,
capture. Audit with
`python tools/LegacyMainFinalNodePruning.Probe/audit_output.py capture` and
`python tools/handwriting/audit_main_single_descriptor_final_prune.py --output report`.
