# Fresh one-shot compact publication

The owned DIRECT and CASE compact materializers now accept finalized prefixes
outside the graph's eight-atom lookahead. The old whole-lookahead restriction
was a limit of the first ported stage, not a native `207CB0` input denial.

The supported boundary remains explicit: a fresh pass has at most 120 graph
nodes. `NEW240/20BAA0` initializes `2490` to 0. The selected frontend leaf `151BF0` writes
knowledge54=120. At `215DC0..215DE3`, `2144F0` calls `207CB0` only if
`node+1 < GraphCount && knowledge54+2490 <= node+1`, using signed comparisons.
Consequently no earlier publication can have changed `2490` for a source of 120
nodes or fewer. Longer sources using the original `RunOwned` route remain
refused before winner inspection. The additive live route is documented in
`legacy-main-live-compact-publication.md`. Intermediate publication is before the
node's source V80, so it must retain the live D0 map and query caches; the final
post-V80 result lease cannot be substituted. This is not a native physical ring
allocation or arbitrary reused-controller state claim.

The existing reachability producer initializes the active node bitmap from the
true graph lookahead frontier, marks native primary-pool and beam predecessors,
activates earlier nodes only through secondary lineage, and queries `207780` for
eligible earlier primary rows. Publication itself scans all `2490..last` nodes,
not only the active suffix. All existing protocol, associated-source,
forced-boundary marker and unsupported cached-parent refusals remain intact.

The independent raw-LK oracle now reconstructs that active frontier and its
conditional queries before building the complete compact records. It preserves
all source rows, alternative pools, outside-source allocated bytes and allocator
residue. Case-state bits stay attached to full tags. Neither the implementation
nor oracle reads expected text to produce a result.

All nine recorded words now reach compact publication, covering 7–22 nodes.
The 60-node generated source stress also passes, crossing the existing periodic
cost normalization window while remaining below 120. A generated 124-node pass
is explicitly refused by this one-shot entrypoint; its missing winner is not
replaced with a fabricated one. The opt-in LIVE test executes its real native
publication gate and preserves that missing winner.
Managed and Windows Native AOT captures match byte for byte for these cases and
the synthetic four-node regression. Eleven publication audits independently
compare 4,280,320 compact bytes and 4,840,512 beam/pool bytes. These are source and
record transport checks, not emitted text or handwriting accuracy claims.
The separate `21FFD0` and glyph-chain consumers still own those later stages.

`audit_main_compact_one_shot.py` pins the constructor reset, selected knowledge
writer, exact periodic gate and caller. The former complete-lookahead tests and
DIRECT byte outputs remain regressions. Vendor files are only read statically.
