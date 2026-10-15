# Full-tag aggregate adapter

`LegacyTaggedAggregateDictionaryQuerySource` owns the full64-bit input tag
while the existing236910 aggregate performs its cursor, accepted-count bases,
relative skip and output source-ID stamping. None of those operations is
reimplemented or changed. The tag is **not reconstructed from Category18**.

Static236910 passes the same request pointer to child V40/V48 in both startup
and selected-source continuation. It does not clear input byte4 or alter the
case bits; it only makes Skip relative during a startup child call and stamps
output record32.byte4 afterward. The adapter therefore forwards the unchanged
tag, including source ID, header and case bits, inside a serialized private
tag frame. Failed calls invalidate the owner and cancellation cannot leak an
active tag to the next call.

`LegacyTaggedDictionaryChild.BindCase292` creates both case paths over the same
backend, context owner and ordinary graph. Normal V40 invokes233090's stateful
case expansion. Auxiliary V48 invokes231AE0's copied-case path and accepts
terminal state31 without a policy-table read. Wrapper292.V88 forwards to the
inner dictionary's eligibility predicate. A weak identity map associates each
actual prepared request object with its normal wrapper-cache identity without
retaining every dead request indefinitely.

The caller's side-cost flag must match the prepared case source flag. Selected
resource lexicalAL supplies the source-case predicate; prepared acousticAL
supplies the glyph form tables. Existing checked container90 and source-locale
selection remains in `LegacyNativeCaseDictionaryFactory`; this adapter does not
invent a configuration default.

The actual-resource probe exercises28 normal case states and32 auxiliary
states with deliberately different Category18 values77/99. Startup passes
the unchanged sourceID0 tag to the selected case child, then stamps output2;
continuation passes sourceID1 intact, then stamps output1. It verifies9,102
records and120 aggregate calls, including32 empty-query states using the
genuine092990 auxiliary map. The nonempty auxiliary stream uses a separately
declared broad map solely to stress terminal transport. An independent Python
consumer checks case variants, contexts, prices, all tag bits and stamped
source bytes against the raw source records and finite-state facts.

```powershell
LegacyTaggedAggregateQuery.Probe AK.res LK.res engine-profile.json case-data.json publication-data.json captures.json
python tools/LegacyTaggedAggregateQuery.Probe/audit_output.py preparation-controls.json captures.json
python tools/handwriting/audit_tagged_aggregate_query.py static.json
```

This is a query transport owner over already selected/prepared dictionary
children. A complete case-aware source factory must still connect symbol
enumeration232C80, ordered permissions234840 followed by inner68, queries,
terminal metadata and restart. It is not a claim that the direct raw-source
mask factory is case-aware or that the application's final Russian winner is
ready.
