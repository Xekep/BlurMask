# Multiple ordinary deferred placements

`LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred` extends the checked
ordinary noNN/shift1 branch to multiple deferred seeds. The older
`CreateSingleDeferred` keeps its original refusal of more than one deferred
seed. Both call the same restored native producer; no source ordering or graph
algorithm is replaced by an independently designed interleaving algorithm.

The evidence is Text `1801a1ef0`'s ordinary deferred branch, not its separate NN
reorder branch. The first pass visits placement offsets0,-1,-2. At lines181-182
of the static export it stops when the target node's last group is already a
deferred group. At lines239-255 it links to two preceding alternatives only
when the preceding last group is ordinary. The second pass, lines314-396,
restores a displaced ordinary group from the preceding node's second-last arc
and appends reciprocal skip edges. Together these branches allow adjacent
deferred groups while retaining their relative order. The port previously
contained those branches but limited the public checked scope to one deferred.

An independent oracle enumerates stable interleavings: ordinary groups retain
their relative order, deferred groups retain theirs, and every deferred group
occupies a position from its current seed position back at most two nodes.
This oracle is used for verification; the implementation continues to execute
the native linked-arc construction and encounter order. It does not enumerate
all permutations in production or select a preferred ordering.

1013 exhaustive patterns over2..10 source nodes are generated as actual raw
physical ink. The real Russian21b seed policy must first produce each specified
deferred pattern. The resulting graph's complete paths exactly equal the
oracle's interleavings, including adjacent and consecutive deferred groups.
One additional256-physical-stroke case exercises reference-count saturation200
while an ordinary group crosses255 deferred groups. In total1014 controls
check29770 complete paths and24290 arcs. Reciprocal adjacency, placement bytes,
seed reference counts and the old API guard are checked. For78 patterns the
independent oracle also enumerates every partition up to three groups per glyph:
the restored candidate merger reproduces195509 distinct complete partitions.

`LegacyNativeGraphWordRecognizer.RecognizeOrdinaryDeferred` passes the new graph
factory through the existing resource-backed source/cut/glyph/final-cache/lexical
bridge. The original `Recognize` method keeps its single-deferred factory and
guard. Whole-line geometry, source metadata, symbol masks, cache scope and
original bounded dictionary beam remain explicit as described in
[the graph word bridge](legacy-native-candidate-graph-word-bridge.md). This
extension does not restore native winner/confidence, path-dependent scoring,
source joins, token mutation, automatic line selection or other NN branches.

An actual-resource analytic MIR fixture plus two delayed physical strokes now
passes the complete producer/consumer path:9 nodes,84 candidate arcs and74
classified geometry groups. Every retained word has a valid linked graph path
and covers all prepared physical points. Repeating all raw input points yields
identical text, acoustic and lexical costs. Candidate, neural-group, expansion
and cancellation limits reject their respective controls. The diagnostic uses
an explicit partial scope and all-symbol mask; its ranking is not evidence that
automatic Russian handwriting acceptance is ready.

Managed and Windows Native AOT run the same controls with actual Russian AK/LK
resources and without loading the vendor DLL.

```powershell
python tools/LegacyDeferredGraph.Probe/reference_controls.py .artifacts/legacy-deferred-graph-controls.json
dotnet run --project tools/LegacyDeferredGraph.Probe -c Release -- .artifacts/legacy-deferred-graph-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```
