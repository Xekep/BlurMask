# Native higher-arc recombination

`LegacyNativeHigherArcRecombination` joins the already ported workspace,
secondary merger and final projection into the remaining nontracing branch of
Text `18020e6a0`. It handles one actual heap record with a nonnegative previous
ending node and an original predecessor ordinal at or above the prepared
`230a` primary limit. The source-reset branch is a separate existing port.

The caller supplies the complete previous node's stored-row collection in its
actual ordinal order, owning-row lookup, immutable prepared source headers,
runtime policy, symbol contexts, and full graph transition callbacks. It does
not create a previous row, invent a graph ordinal or substitute the narrower
first-parser `23f` transition request. Counts are explicit: primary 1–20 and
secondary 2–20, with complete 40-byte blocks. Inactive capacity and caller
residue remain observable; input buffers are copied.

## Full transition request

`LegacyNativeWordRecombinationRequestFactory` ports the request stores in
`18020e6a0`. The last written field is byte `0x49`, so the request needs at least
74 bytes. Its portable owned representation is an aligned 80-byte snapshot.
Unknown padding, including callback flag bytes before their producer writes,
comes from explicit caller residue. `GetSnapshot()` returns a detached copy.

| Offset | Confirmed producer |
|---|---|
| `00` | Current ending node |
| `04` | Previous record's source node, unsigned16 at record `18` |
| `08` | Previous ending node: current node minus signed current span minus 1 |
| `0c` | Current graph arc at heap `1c` |
| `0e` | Previous packed arc from record `18 >> 22` |
| `10` | Previous graph arc at record `1c` |
| `12/13` | Current/previous geometry-list index from sampler table `78` |
| `14/18` | Current/previous first ID from sampler table `70` |
| `1c` | Current packed kind from heap `27 >> 1 & 7` |
| `20` | Sign-extended current span byte |
| `24` | Current label or literal special-symbol dispatch; unmatched dispatch retains residue |
| `26` | Previous record label |
| `28` | Reference to mutable transition cost |
| `34` | Impossible-cost limit `0x4000000` |
| `38/39/3a` | Current state low6 bits, zero, prepared permission byte `2309` |
| `40/44` | Current/previous prepared source kind: header `>> 9 & 15` |
| `48` | Current inherited flag from heap `07 & 1`, or zero in suffix dispatch |
| `49` | Flag `07 & 1` of the actual row referenced by previous record `1e` |

The category tables are not guesses based on script. Native `1800103e0`
constructs table `70` from serialized first IDs and table `78` from geometry
map indices, as represented by `LegacyCursiveCategoryProfile`. Both actual RU
profiles contain 35 composite categories.

Offset `28` contains a nonzero portable `LegacyNativeWordTransitionCostCell`
identity, not an address that could be passed into a vendor ABI. Graph callbacks
use its `Value` property. No vendor library is loaded or executed. Header and
row callbacks represent immutable memory reads; arbitrary stateful callbacks
are not claimed to reproduce the native timing of individual memory reads.
Cancellation is checked immediately after every external callback.

## Controller ordering

The controller initializes primary/secondary entries before the first graph
test. It restores the actual previous source record and category fields, calls
`118` unless the literal label/age/source-kind bypass applies, then calls `140`.
Rejection preserves the exact preinitialized output rather than filling an
invented fallback.

For an accepted initial path, it retains the corrected cost delta and
incremental delta separately. Native `local428` is then reset to the physical
cost delta before the first successor query. The initial secondary tails use
that physical delta. Later transition calls start from the retained corrected
delta, while incoming increments always use the original incremental delta.

Candidate rows are visited by ordinal from prepared `230a` to the actual node
count. Low6 state, context identity and the optional count cap filter first.
Only the previous graph arc field changes before the successor query. A match
then fills the remaining previous-record fields, performs `118`/`140`, populates
the persistent 800-byte workspace, runs literal `1f87c0` and copies best fields
back into the heap. Callback flag residue persists between candidates.

Mapped identities remain prefix identities for every merge in this loop.
Contexts are refreshed once after all candidates have been considered; doing
that inside the loop would change later deduplication. The final projection
consumes the chosen parent arc and chosen alternative index, preserving raw
context and record ownership.

`LegacyNativeHigherArcTransitionOperations.ForOrdinaryConnections` additionally
connects the full request to existing ordinary graph getters. Native frontend
`152140/152190` forwards to `21b` `50/58`: request `08` is the previous node,
`10` is the previous graph arc, and `20` is the **current** span selector.
Selector 0–14 and exact stored successor order/duplicates are preserved.
Required full-packet `118`/`140` implementations remain explicit.

## Verification and remaining work

The Python interpreter independently reads both real AK category profiles and
models request stores, branch ordering, callback cost/flag mutations, structural
filters, persistent workspace, live reverse merger writes and final projection.
It imports only the previously independent symbolic-compose and scalar-merge
oracle definitions; it does not call the managed implementation or consume its
output as expected values.

Managed and Windows Native AOT execute the same 512 full-buffer streams: 512
exact requests, 2,527 full request/cost callback snapshots, 3,509 owning-row
lookups, 287 accepted incoming merges, and 14 post-callback cancellation guards.
The controls include first-parser/main category profiles, signed spans,
unsigned source nodes above 65,535, special symbols and unmatched symbol
residue, integer wrap, counts 1/2/3/4/20, unavailable tails, callback rejection,
persistent flag bytes, unrelated structural rows and untouched capacity.

The real-resource integration uses actual RU raw ink, cuts, candidate graph,
NN/cache costs, root/child record writers, continuation writer and `211eb0`
alternative pools. Six actual higher stored rows then feed the full controller:
30 native connection queries, 72 explicit full-packet callbacks, 30 merges and
exact materialization. Prepared mode1 and the callback correction costs in this
fixture are diagnostic inputs, not recovered `118`/`140` geometry policy or
complete lexical-controller scheduling. Russian production acceptance, native
winner/confidence and the full independent engine remain unfinished.

Run from the repository root:

```powershell
python tools/LegacyWordHigherArc.Probe/reference_controls.py .artifacts/word-higherarc-controls.json
dotnet run --project tools/LegacyWordHigherArc.Probe -c Release -- .artifacts/word-higherarc-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
dotnet publish tools/LegacyWordHigherArc.Probe/LegacyWordHigherArc.Probe.csproj -c Release -r win-x64 -p:PublishAot=true -o .artifacts/word-higherarc-native
.artifacts/word-higherarc-native/LegacyWordHigherArc.Probe.exe .artifacts/word-higherarc-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

Local builds use separate artifact directories on drive D. Static evidence is
the private `research/myscript/output/MyScriptText.dll/decompiled.c`, address
`18020e6a0`, and the focused `TextFeaturesStage3/joined-1800103e0.c` category
builder. These private decompiler outputs are not runtime dependencies.
