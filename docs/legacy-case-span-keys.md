# Native case query keys within one source node

The owning main D0 producer now supplies its actual graph/node/span to the same
case Session when installing permissions. This is a typed internal producer
bridge, not a caller success flag. The opt-in native-key path requires a genuine
`AdvanceNodeEpoch(completedNode)`, the next source node (`completedNode+1`), and
strictly increasing spans. Empty intermediate spans remain valid according to
frontend98's maximum-span extent; no requirement for a physical arc is invented.

Native initial1FD240 and continuation201270 write query1648+1C=currentNode and
+20=currentSpan. Both wrapper233090 and backend093B50 compare these fields and
reset their cached query slots on mismatch. Consequently a fresh node/span key
invalidates both layers without resetting the entire backend per span. Reads
with those exact fields can reuse one query across successive owning maps.
Multiple pages/queries under the same installed map retain their native caches.

The older conservative prepared-query guard remains available. Missing/mismatched
keys, repeated/descending spans, unexpected source nodes, and external public
Session query/permission activity revoke the owning progression until another
actual V80 epoch. Public prepared Session behavior remains unchanged. No Refresh
operation fabricates an epoch, and source/graph/map identities remain checked.
External public queries are conservatively treated as cache contamination even
when their numeric fields happen to match.
Revoking this new progression is not a blanket rejection of the older prepared
API: a first/same-map ordinary prepared read can still be accepted by its old
conservative guard. It cannot use the new key exception to cross a later map.

The new headless probe starts at the existing raw own-ink/native first result,
main source720/geometry/NN path. One292 backend and one persistent query produce
397 records over10 actual D0 maps. V80 calls are exactly[-1,0,1,2,3], before the
node loop and after each completed node. There are no per-span resets. Independent
Python reconstructs the raw195 NN-cost permission union, ordered case propagation,
actual working maps and full397 case records. Every resulting context remains
live in the same symbolic owner. Six separate regressions reject repeated,
descending, wrong-key, external-query, external-refresh and missing-node-epoch
progressions without poisoning a newly valid owning session.

This is a source-level cache/permission proof. Its root query for a noninitial
span is not a claim that the native240 startup consumer runs there. Full290/240
startup, parent continuation scheduling, terminal selection and recognition
quality remain separate work.

Probe: `LegacyCaseSpanKeys.Probe AK LK engine separation case-data publication capture`.
Independent audit: `audit_output.py AK LK case-preparation-controls capture`.
Static audit: `tools/handwriting/audit_case_span_keys.py [report]` pins12 PE
instructions without loading the vendor engine.
