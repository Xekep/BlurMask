# Native single-deferred seed graph

`LegacyOrdinaryCursiveSeedGraph.CreateSingleDeferred` consumes the owned native
seed ordering snapshot from `LegacyOrdinaryCursiveSeedProducer`. It restores
the ordinary noNN, insertion-shift1 branch of Text`1801a1ef0`, its base-arc
helper`1801a1850`, context-set identity pass, and physical-overlap flag pass
`1801a4f10`. The scope is one fresh ordinary LTR source with unknown physical
boundary IDs and atmost one deferred seed. Multiple interacting deferred seeds
are explicitly refused. Optional geometry flag12e is0 in the checked actual
type21b.version0x10a resource; the alternate geometry branch is outside this
producer.

This is a real producer/consumer bridge from cut ownership to source graph
topology. It is not a chain of reordered atoms. Each node contains native arc
ordinals, seed group IDs, signed placement offsets, flags, reciprocal adjacent
predecessor/successor ordinals and packed context identities. The returned
reference counts are native saturated per-seed arc references. Coordinate
buffers and original point identities are preserved in the supplied seed
snapshot; the graph contains metadata only.

## Arc production

For an ordinary seed, `1a1850` creates an arc with flag4. Its predecessors are
the preceding node's last arc and, if there are alternatives, its second-last
arc; the first source node links to the source root ordinal−1. Assembly at
`1801a2951` stores the noNN boundary-suppression byte0 for this bounded branch,
so the additional flag2 is not guessed or enabled.

For a deferred group at nodeU, `1a1ef0` first creates placement arcs at nodesU,
U−1 andU−2, stopping at the source boundary. The signed placement offset is
0,−1 or−2. Flag1 is set when the placement offset differs from the insertion
displacement supplied by`19f850`. Predecessor links preserve the previous
ordinary alternatives. Then the native second pass restores displaced
ordinary groups at U andU−1 and links their skip paths. Linked-list helper
`18019d1d0` appends edges without deduplication; the port preserves encounter
order and reciprocal links. It does not replace this with an ordinary chain.

The placement byte is distinct from the native merged glyph's span byte.
Initial seed arcs have zero merged span. Future candidate merging reads the
span at arc offset4; the signed placement offset is at offset5. Keeping those
fields distinct matters for the later span-cap consumer.

## Context identities and source overlap

The final`1a1ef0` pass shares start identities among arcs with equal predecessor
sets, and end identities among arcs with equal successor sets. The first start
ID is `node<<7`; additional canonical set IDs use a six-bit local serial and
the native even/odd start/end packing. These IDs describe graph context sets;
they are neither lexical states nor invented source timestamps.

`1a4f10` propagates physical-stroke bitsets over every linked alternative.
An arc's prefix excludes its own group. Its suffix includes its own group and
all successors. If the prefix and suffix intersect, flag4 is cleared. This is
an exact union over graph alternatives, not a single selected path or point
bounding-box overlap. It preserves source physical-stroke IDs from the atom
ranges. The port uses owned integer sets for this bounded data-only stage.

## Verification and remaining consumer

An independent original Python oracle constructs complete stable-move paths
for one deferred seed: the delayed group can occupy its current position or
the preceding one/two positions while ordinary groups retain their relative
order. From these complete paths it independently derives node variants,
adjacency sets and canonical context IDs.384 controls check9655 arcs and1106
complete paths, including source-root boundaries and all checked placement
positions. The graph paths preserve every seed exactly once.

A real Russian resource fixture runs raw MIR ink with a late cross-stroke
through the actual cut controller, actual21b seed policy and this graph. It
produces the restored placement alternatives and clears7 physical-overlap
flags. The graph preserves all prepared atom ownership across complete paths.
Managed and Windows Native AOT run the same controls without loading vendor
engine code.

```powershell
python tools/LegacySeedGraph.Probe/reference_controls.py .artifacts/legacy-seed-graph-controls.json
dotnet run --project tools/LegacySeedGraph.Probe -c Release -- .artifacts/legacy-seed-graph-controls.json models/ru-RU/ru_RU-ak-cur.lite.res
```

Remaining: independently verify interacting deferred groups; restore the
native graph mutation/join and merged-glyph candidate consumers; connect this
nonlinear graph to ordinary native search, terminal selection and posterior
policy. The earlier recognition endpoint continues to refuse deferred input.
This component does not imply native Russian cursive auto-recognition is
finished or that the current original linear dictionary beam can consume the
graph unchanged.
