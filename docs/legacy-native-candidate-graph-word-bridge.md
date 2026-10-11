# Ordinary merged candidate graph and word bridge

`LegacyOrdinaryCursiveCandidateGraph` consumes the native ordinary seed snapshot
and its checked single-deferred seed graph. It restores the bounded same-source,
noNN span-merging path of Text `1801a5920`, `1801a3b30`, `1801a35a0` and
`1801a37b0`. The input remains one ordinary LTR word with explicit source70 flags
and F0 physical-boundary metadata; the checked seed producer refuses unsupported
metadata. Cross-source joins, token-group mutations and interacting multiple
deferred seeds remain outside this API.

## Native producer/consumer evidence

`1a5920` visits source nodes and their existing candidate arcs in encounter order.
`1a3b30` extends a left candidate using its adjacent singleton successors. It
skips a right arc with flag8 and applies the restored ordinary maximum-span
guard. The merged span is left span plus one. Initial seed span is zero: the
signed deferred placement byte at native arc offset5 is a different field from
the merged span at offset4. Treating a deferred placement as a span changes both
the merge cap and predecessor anchor.

`1a35a0` assembles group ownership and uses the sampler297 range composer. The
port keeps physical range order and reuses the previously restored
`LegacyCursiveInkAtoms.Compose`. Complete composed range vectors identify newly
merged groups; native merge hints start after the initial singleton groups, so
the port does not intern initial singleton records. An interned group's seed
membership describes its first geometry composition. Logical arc span is
separate metadata; callers must not infer a general span from that membership.

The native arc reuse check asks whether the left predecessor list and right
adjacent-successor list are contained in an existing arc's lists. It does not
require equal lists. A reused arc clears flag1 if either input lacks that flag.
`1a37b0` otherwise appends an arc whose flags are
`(leftFlags & 0x1e) | ((leftFlags & rightFlags) & 1)`, with the left start-context
ID and right end-context ID. Predecessors are anchored at
`endingNode - mergedSpan - 1`. Reciprocal successors retain their node-distance
vectors, rather than flattening deferred alternatives into a reordered chain.
The data-only result owns its range, adjacency and metadata arrays.

This stage does not recreate native merged-group endpoint-category records,
special/cross-source geometry branches, later source joins or a final native
recognition controller. It exposes explicit candidate-count/cancellation guards.

## Resource-backed endpoint and original search boundary

`LegacyNativeGraphWordRecognizer.Recognize` connects:

1. Resource-configured ordinary ingress, closed-contour source probability and
   Russian continuous cuts through `LegacyOrdinaryCursiveCutSource`.
2. Native LTR seed ordering, single-deferred placements and merged candidate graph.
3. Raw chronological range ownership, prepared glyph normalization, neural
   families and context through `LegacyPreparedWordGroupEvaluator`.
4. Caller-configured final195-entry class cache, composite geometry and acoustic
   candidate scorer through `LegacyCursiveFinalCacheGroupScorer`.
5. The real packed dictionary and whole-word lexical scorer.

The new `LegacyGraphWordSearch` is an **original bounded dictionary beam**. Its
frontiers belong to candidate arcs; every prefix follows the arc's actual
predecessors and source span. A linear frontier indexed only by the number of
covered atoms would allow invalid joins between deferred placements. The
returned word retains each glyph's node/arc position, which permits consumers
to recover the actual selected geometry and source lineage.

Candidate costs must be prefix independent. Each arc retains at most one best
prefix per dictionary state, then applies the explicit per-arc beam width.
Terminal additive lexical scoring runs before final pruning. A wide beam in
the independent tests covers the entire small dictionary; the production-sized
dictionary uses a bounded beam, so optimality is not promised. The older linear
beam prunes per node; these policies can retain different lower-ranked words.
Neither is claimed as the native word-search/winner policy.

The endpoint caches neural classifications by composed group identity; scope
and final-cache costs are still evaluated per reachable arc. Terminal scores
are re-evaluated for the retained acoustic alternative, preventing a discarded
alternative's score from being attached to an output. It does not accumulate
every expanded terminal/cost pair in memory. Limits bound merged candidates,
unique neural groups, retained prefixes and expanded choices.

The line baseline/height, normalization context frame, source-scope flags,
symbol mask, category-map selection, related forms, shortlist policy and lexical
history remain explicit. Source probability is supplied from the same filtered
physical record, and inserted points retain raw chronology. No source timestamp
or source-scope flag is guessed. This endpoint produces ranked words and costs,
**not automatic acceptance confidence**. Native path-dependent corrections,
language/mask/controller selection, complete multi-source line grouping and
winner/confidence policy still require further work.

## Independent and actual-resource verification

The original Python oracle enumerates stable deferred placements and all allowed
partitions, independently of the native arc construction/merging implementation.
192 controls yield4056 native candidates and15194 distinct complete partitions.
Tests compare exact partition sets, reciprocal node-distance links, physical
range ownership and span/placement distinction.

A separate exhaustive label-product oracle uses a small independently serialized
packed dictionary, signed candidate costs, duplicate labels, an absent label,
the impossible-cost sentinel and an additive terminal filter. The graph beam
with sufficient capacity reproduces15546 exact dictionary word minima. Returned
glyph positions must form linked source-root-to-terminal paths. Cancellation,
candidate/expansion/neural budgets are exercised.

Actual Russian AK/LK resources run analytic MIR strokes through the complete
bridge. The ordinary7-node/28-candidate case matches the existing ordinary-chain
endpoint's first five texts and costs:
`то:50888, мне:52602, мы:54665, мир:55698, МЮ:57917`.
Adding one delayed physical cross-stroke reaches the previously unsupported
consumer:8 nodes,52 candidate arcs,47 classified geometry groups, with retained
complete source ownership. Repeating every raw input point produces identical
word texts, acoustic costs and total costs, verifying ingress deduplication and
inserted chronology through the end-to-end path.

These are transport and search-correctness controls, not evidence of handwriting
accuracy: `мир` remains fourth in the ordinary fixture. The diagnostic fixtures
deliberately use an explicit partial scope and all-symbol mask; they do not
claim those policies are the native automatic Russian language policy. Managed
and Windows Native AOT pass the same tests without loading the vendor DLL.

```powershell
python tools/LegacyCandidateGraph.Probe/reference_controls.py .artifacts/legacy-candidate-graph-controls.json
dotnet run --project tools/LegacyCandidateGraph.Probe -c Release -- .artifacts/legacy-candidate-graph-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```
