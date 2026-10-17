# Live intermediate compact publication

`RunOwnedWithIntermediatePublication` on the DIRECT and typed CASE main passes
adds the real `2144F0` intermediate `207CB0` lifetime. The public route uses the
signed native condition `node+1 < GraphCount && knowledge54+2490 <= node+1`.
Owned fresh knowledge54 is120. Publication runs after periodic cost normalization
and before the node's source V80. It keeps the producing D0 map and the exact
outer290 reader; it does not reset, replace or replay that query source.

The common publication kernel carries compact storage2460/count2470, node starts
2478, prefix owners2488 and next unpublished node2490. A later publication appends
to that storage. New2488 entries receive the invocation's initial2490 minus1;
the end sentinel receives the last published node. Earlier table entries and
compact bytes survive. Final materialization consumes a copy of the retained
state, so it does not mutate the immutable intermediate pass result.

Completed node buffers accept only the actual publication writes: beam byte6
bit1 and ushort22, primary-pool byte1B bit7 and ushort1C. Their entire allocations
are checked before either array is copied. Source/retained counts, costs, other
flag bits and outside-source bytes are preserved. Future search nodes read these
same buffers. Public observation and result getters return deep copies.

LIVE queries hold a separate source lease, checked against the actual controller
node, last D0 map, epoch and backend query revision. Existing search reads are
allowed before capture. Own publication reads advance that same lease; an
external read, map refresh or V80 invalidates it. The lease ends before the real
V80. The optional observer runs before live writeback, followed by a lease and
cancellation check. Observer failures and cancellation fail the single-use pass.

## Fresh CASE V58 proof

The preceding node's V80 calls `231ED0`, setting cursor8FC0 to `INT_MAX` and
count8FC4 to0. Normal `233090` queries use eight separate1068-byte slots beginning
at38 and ending exactly at8378. They do not populate the V58 cache at8F80/8FC0.
`233FC0` fetches the same inner `2B7.V58` when cursor>=count; that actual inner
`05A3F0` returns0. Its empty fetch stores cursor0/count0, so subsequent fetches
remain empty even without another reset. This proves the original fallback for
the fresh owned factory before final V80, rather than borrowing the final-reset
assumption. Nonempty/custom V58 expansion remains outside this route.

`audit_main_live_compact_publication.py` pins four complete PE regions,25 exact
instruction sequences and three VTable bindings. It also checks the normal-query
cache domain. Vendor DLLs are read only; they are never loaded or executed.

## Verification and limits

The generated124- and140-node CASE inputs hit the real gate on node119, publish
the first120 nodes and continue with2490=120. Their active finite chains are
empty there: compact count0 and no terminal winner. They verify live clearing,
writeback, preserved storage and lifecycle, not nonempty long-text recognition.

A separate internal friend-only prepared test invokes the same kernel at node2
of an actual four-node source, then lets node3 continue after writeback. It is
explicitly captured as `nativeGate=false`; no public force-node API exists.
That test publishes716 records through275 continuation checks/466 backend calls.
Final append yields1057 records/index1019, with the same complete compact bytes
as independent one-shot publication. It is a prepared consumer test, not evidence
that the native120 gate fired on a four-node source.

Managed and Windows Native AOT captures are compared by the independent raw-LK
publication oracle, including all compact allocator residue and full beam/pool
allocations. The prepared test has37 transport/lifetime controls: forbidden
field writes, foreign controller/count/extents, cancellation, observer failure,
external V80, single-use failure and copy isolation. The actual native-gate
fixtures retain the31 node-writeback controls. Existing short one-shot outputs
remain regressions.

Run `LegacyMainLiveCompactPublication.Probe` with AK, LK, engine JSON, separation
JSON, case data, publication data, line30, line50, physical count and capture path.
Physical counts31/35 produce124/140 nodes;1 selects the explicitly prepared
four-node consumer test. Audit each capture with this probe's `audit_output.py`
using AK, LK and capture path. All build and output paths may be external to the
repository.

Physical ring allocation, associated/protocol branches, forced-boundary markers,
unsupported cached-parent lineages and nonempty long-source publication remain
explicit limitations. This stage does not assert complete engine restoration,
emitted text quality or Russian handwriting acceptance confidence.
