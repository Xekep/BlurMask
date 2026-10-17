# Owning ordinary compact graph

`LegacyFreshMainCompactOrdinaryGraph.Prepare(AK, actualGlobalState)` retains the
same [cross-source/pruned owner](legacy-main-cross-source-graph.md) and converts
its ordinary groups and arc selector tables. It preserves descriptor and line
ownership, physical ranges, source chronology and allocation IDs. It does not
loop independent line samplers or invent `SeedGroupIndices` for a global graph.

`19C4E0` converts wide96-byte groups to compact56-byte groups. This is a layout
conversion, **not** a post-prune live-group filter: every ordinal below the
allocated group count is copied, including zero-reference groups. The earlier
`19D470` append does filter local dead groups; those are different stages.
Wide00/02 become compact00/02, wide51/52/50 become compact04/05/06, kind07 stays07,
and bbox40..4C becomes28..34. Ordinary ranges move from wide20 into compact08;
history storage is released. Portable semantic snapshots own copied ranges and
do not pretend to contain native pointers, unspecified padding or freed history.

`19C310` converts the root first, then nodes in ascending order and their arcs
in existing order. It accumulates selector counts and copies linked entries in
order. Selector0 holds predecessors and selectors1..cap hold outgoing lists;
the root predecessor list is empty. Root sentinel−1 becomes unsignedFFFF.
All arc scalar fields and context IDs are retained. The compact ordinal getter
uses these cumulative boundaries without re-sorting or deduplication.

The supported ordinary cap is1..8, including actual main cap8. Each selector
table's unwrapped total must be at most2048. Native accumulation uses U16;
oversized wrapped sums could allocate too little storage. The portable producer
refuses them before allocation rather than reproducing unsafe copying. It does
not model native slab pointers or allocator failure. Cancellation/failure
publishes no partial owning result; public ranges/indices are defensive copies.

The executable probe covers66 prepared state projections, including a retained
dead group in64 cases, root selectors across all eight positions, exact2047/2048
totals and2049 refusal. Seven genuine raw owning inputs cover five cross-source
groups. No expected text selects a cut, group or source. Python independently
checks every field, bounds bit pattern, range, cumulative boundary and ordinal.
The static auditor pins32 instructions in three hashed PE blocks.

This is not a completed multiline sampler. Native samplerV90 (`1941B0`)
transition caches, global120 source DFS, lazy227 context production, per-owner
normalization/NN and multiline decoding remain separate dependencies. No
recognition-accuracy or final-winner result is claimed.

Probe arguments: AK, engine, separation, case, publication, line30, line50,
capture. Audit with `python tools/LegacyMainCompactGraph.Probe/audit_output.py capture`
and `python tools/handwriting/audit_main_compact_ordinary_graph.py --output report`.
