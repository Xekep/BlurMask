# Ordinary secondary-row restart and candidate reduction

This bounded bridge ports the ordinary plain-symbol branch of Text `1fe740`.
It connects source restart tags, stored secondary rows, typed dictionary queries,
the main sampler's geometry callbacks, final acoustic cache scoring, and native
heap allocation. It is not the complete `2144f0` scheduler, linguistic `204290`
cache preparation, final winner publication, or acceptance confidence.

## Actual source restart tag

Dictionary `2b7.virtual50=093f60` reads the incoming tag's byte05.bit2 and returns
the owned source's runtime80 or runtime78. The constructor `092790` writes:

- runtime78: node0, source ID0, byte05=8, all other bytes zero;
- runtime80: node1, source ID0, byte05=12, all other bytes zero.

`LegacyPreparedDictionaryRestartTags.CreateFresh()` represents these literal
constructor values. Configured overrides require an explicit pair.
Aggregate `28d.virtual50=236b50` selects the actual incoming source ID, clears that
ID for the child getter, and stamps the returned ID only when the **returned**
byte05.bit2 is set. It retains all other64 bits. Assembly at `236b88..236bb2`
confirms this; the decompiler's concatenation loses the upper24 bits.

The returned value eventually reaches stored record28 through
`204290 -> 1f9080`. Record28 is a packed source query tag, not record00, a symbol
context identity, an ordinal, or a pointer. The new consumer refuses the initial
`FFFFFFFFFFFFFFFF` placeholder. This batch does not infer the unported
linguistic costs or context recombinations that surround that writer.

## Group and query producer

`LegacyNativeAlternativeWordQueryPreparation.GroupSecondaryRows` starts at the
actual primary limit230a and reads through the supplied complete stored-row
extent23f8. It skips record21.bit7 and the explicit mode2601/count26/span limit.
Consecutive eligible rows share full record28 and record21.low6; skipped rows do
not break a group. A repeated key after another key creates another group.
There is no global deduplication or sorting. Span addition preserves signed32
wrap in the native limit comparison.

`Apply` installs the actual restart node, state21 into query18, mode-dependent
remaining limit19, owner/span, `100h` plus optional boundary bit4, incoming flags,
and zero skip. It clears the query parent context while preserving the owned
context container, page capacity and query cache identity. A rejected early
mode limit does not mutate the query.

## Regular geometry packet and reduction

`LegacyNativeWordAlternativeRequestFactory` supplies the full owned80-byte
packet used by the regular `1fe740` geometry loop. Unlike `201270`, it obtains
both FirstIds from the sampler composite table and starts the cost cell with
the parent candidate cost **without** the new query cost. It inherits the
parent increment14. Previous source node, previous ending node, packed source
arc and graph arc remain distinct. Byte49 comes from the owned row addressed by
the parent's previous-row ordinal1e in the previous ending row, not the parent's
own byte7. The caller supplies complete stack residue; unassigned bytes survive.

`LegacyNativeAlternativeWordArcReduction` implements the native1024-slot arrays:
first arc encounter defines order, initial costs are `04000000h`, and only a
strictly lower signed geometry-adjusted cost replaces the best parent. Equal
costs keep the first parent and its increment/flags. Boundary254c clears saved
callback rewrite flags. The controller stops enumeration when1024 distinct
arcs have been encountered, including when later arcs might duplicate existing
entries; it does not manufacture extra capacity or sort candidates.

`LegacyNativeOrdinaryAlternativeWordCandidates.ProcessQuery` runs actual
main118/140 for each parent and each reciprocal successor in producer order.
It then adds the query base cost with signed32 wrap and calls final-cache128
once per retained arc. The nontracing native implementation leaves the last
geometry packet's previous fields in place for this128 phase; only the current
arc, reduced cost and reduced increment are installed. The final scorer uses
the current node/arc, source kind40 and geometry index12.

The selected best parent ordinal reaches the native initial writer. The new
word anchor is current ending node/FFFF, auxiliary34 is the actual graph arc's
StartContextId, and auxiliary30 is that value only for query flag8. Header5 is
the current alphabet flag OR the selected parent's high bit when restart
tag28.bit42 is set, unless the explicit header suppression is enabled.

This consumer supports ordinary frontend280=false, nontracing, mode2601=false,
plain letters/digits and query kinds0/2. It refuses positive live knowledge.ec
secondary routing, active position penalties, special labels, nonzero query
extra contexts, and geometry context rewrite flags. The actual main source
frame/descriptor facts, prepared source headers, cache policy, source68
permission and heap capacities remain owned inputs. Exceptions, callback
cancellation or allocation failure invalidate the serialized producer.

## Verification

The independent Python oracle reads the actual RU sampler table directly and
generates1024 complete80-byte packets,512 aggregate restart cases,512 secondary
group/query cases (including signed overflow), and1024 strict-order reduction
cases (negative costs, impossible costs, ties and rewrite flag scopes).

The actual-resource fixture uses11 NN-derived stored parent rows, the proved
source50 getter, existing1f9080 writer, one restart group, actual permission-
filtered dictionary pages, and the real main sampler118/140 and final-cache128.
It produces121 geometry calls,11 reduced acoustic calls and11 allocations.
There are no scripted geometry or acoustic callbacks in this fixture. Its
secondary cache costs/state come explicitly from the NN parent fixture; it
does not claim that the missing linguistic204290 producer generated them.
The guide90/80, same-descriptor source facts and one-alternative storage are
explicit diagnostic inputs, not a replacement for the complete native source
or scheduler. Capacity/source ownership and post-callback cancellation guards
cover failure invalidation as well as successful resource consumption.

Run from the repository root:

```powershell
python tools/LegacyWordAlternativeCandidates.Probe/reference_controls.py .artifacts/word-alternative-candidate-controls.json
dotnet run --project tools/LegacyWordAlternativeCandidates.Probe -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/word-alternative-candidates-managed-artifacts -- .artifacts/word-alternative-candidate-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

The same probe supports Windows NativeAOT. Vendor binaries are inspected
statically and are never executed by these checks.
