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
normal-query/permission activity revoke the owning progression until another
actual V80 epoch. No Refresh
operation fabricates an epoch, and source/graph/map identities remain checked.
External public normal queries are conservatively treated as cache contamination even
when their numeric fields happen to match.
Revoking this new progression is not a blanket rejection of the older prepared
API: a first/same-map ordinary prepared read can still be accepted by its old
conservative guard. It cannot use the new key exception to cross a later map.

Auxiliary V48 queries are distinct.292.V48=231AE0 bypasses233090 and the inner
backend selects immutable60 instead of filtered50.204290 writes1C from the
language query's node and20 from a sign-extended parent-row span byte, so these
fields need not equal the last D0 span. Auxiliary reads now validate the current
owning lease and the immutable60 identity without registering a normal50-map
observation or revoking normal key progression. Initial and refreshed maps share
the same private60 array. Mode is already part of093B50's shared slot key; changing
normal/auxiliary mode resets that slot through its native comparison, with no
invented global or per-span reset.

Ten actual span controls interleave owned/public auxiliary queries with span-1
and resume identical normal records. The actual RU auxiliary map admits zero
terminal records; this does not establish recognition quality. A separate broad
supplied-map positive control exercises nonempty auxiliary results across a50
permission refresh, an empty denied normal query, and normal slot recovery.
It is explicitly a prepared backend control, not the native RU60 map. An auxiliary
read through a stale owning lease remains rejected.

The new headless probe starts at the existing raw own-ink/native first result,
main source720/geometry/NN path. One292 backend and one persistent query produce
397 records over10 actual D0 maps. V80 calls are exactly[-1,0,1,2,3], before the
node loop and after each completed node. There are no per-span resets. Independent
Python reconstructs the raw195 NN-cost permission union, ordered case propagation,
actual working maps and full397 case records. Every resulting context remains
live in the same symbolic owner. Seven separate regressions reject repeated,
descending, wrong-key, external-query, external-refresh and missing-node-epoch
progressions and stale auxiliary leases without poisoning a newly valid owning session.

This is a source-level cache/permission proof. Its root query for a noninitial
span is not a claim that the native240 startup consumer runs there. Full290/240
startup, parent continuation scheduling, terminal selection and recognition
quality remain separate work.

Probe: `LegacyCaseSpanKeys.Probe AK LK engine separation case-data publication capture`.
Independent audit: `audit_output.py AK LK case-preparation-controls capture`.
Static audit: `tools/handwriting/audit_case_span_keys.py [report]` pins23 PE
instructions, two auxiliary dispatch slots and complete refresh/forwarder
function hashes without loading the vendor engine.
