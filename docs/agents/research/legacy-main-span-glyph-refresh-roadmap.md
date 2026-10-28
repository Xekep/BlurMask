# Negative-span lazy glyph refresh

`LegacyOwnedMainSpanGlyphSource` and `LegacyOwnedMainSpanGlyphRefresh` now restore
the bounded negative signed-arc branch of `217670` for an actual fresh raw
single-line result. General protocol contexts, multiple descriptors and the
complete string-wrapper caller remain separate. The positive V140/scoring
services are unchanged.

## Actual caller

`frontend222.V188 = 152B20` forwards to its graph's
`21B.V120 = 1A9AE0`. For a negative signed arc in owner64.sourceKey10:

1. Sign-extend the low24 key bits into `lastContext`.
2. Sign-extend key bits24..39 into `delta`; `firstContext = lastContext + delta`.
3. Set source-export mode to key bit56, preserve owner28/30 as the output pair,
   and pass a null residual-output pointer.
4. Call the exporter for its count, allocate `24 * count` bytes and assign
   owner28. Allocation failure returns false with the previous writes retained.
5. Call the same exporter again to fill the source records. Store exact float
   bits `3F800000` into owner1C and return success.

The two context values are fixed-point graph context identities, not ordinary
graph node/arc ordinals or screen coordinates. The allocation has no two-byte
residual tail. The source exporter returns `void`; this caller does not test a
success result after either export. It does not append a glyph descriptor, call
`216860`, or calculate `1B8/1C0` scores on this branch. It leaves the existing
glyph records alone. Zero count and allocator behavior must remain an explicit
contract in any port, rather than silently treating an empty source as success.

## Why reversing the ordinary export is insufficient

`1A9AE0` first tries its forward source-path traversal when `firstArgument <=
secondArgument`. The negative-span call ordinarily supplies the later context
first. Its fallback therefore has independent behavior:

* It scans native graph node `(argument0 >> 7)` for an eligible arc whose start
  context equals argument0. Eligible means **arc span byte4 equals zero**, or
  group kind1. The first match in native order matters. Ordinary merged arcs
  with a nonzero span do not qualify merely because their group kind is0.
* Native arc **offset18 hex** is a signed protocol-owner index. It is not the
  predecessor pointer at08 or a point ordinal. A nonnegative value can select
  `graph298` and the `1A96D0/1A9800` protocol-source branch.
* A failed/no protocol match can select the global trailing protocol record
  `graph1F0/200/208`, depending on mode and final-node position.
* With no starting arc, mode1 separately looks for an arc whose start context
  equals argument1, then publishes its first point as a degenerate source
  interval. Mode0 instead uses `1A9650`, whose lookup is by matching **end**
  context, and can use the source physical endpoint.
* The remaining ordinary fallback can return one or two degenerate endpoint
  records. It compares actual raw source keys and `source.V98` adjacency. A
  matched starting arc and an absent starting arc have different endpoints.
  Sorting/reversing the existing forward membership does not reproduce this.

Protocol `1A9800` can group multiple raw source IDs into intervals and has its
own count/fill and contiguous-key rules. Its fallback and allocation failures
must not be replaced with an invented empty protocol list.

## Fresh owner and protocol absence

The factory takes `LegacyFreshMainSingleLinePlainRecognitionResult`, whose
private construction retains the actual raw preprocessing, one assigned
ordinary descriptor, sampled graph, compact materialization, confidence and
publication pool. It verifies the exact sampled/controller/graph identities,
complete physical interval, plain source135 and nontracing/no-capture scope.
There is no caller-supplied `noProtocol` flag or manufactured graph facade.

The native factory/lifecycle evidence is:

1. Type21B descriptor `2E1D40` points to constructor operations `2E1E00`:
   `19C970`, `19CB00`, `19CC50`. `19C970` clears pending1F0 flags, global
   token200/count208 and protocol storage298. Clone and buffer constructors
   call it before copying exactly configuration8..160; serialized data cannot
   populate these later runtime fields through that copy.
2. `150D00` clones selected247.V90 into fresh frontend70 through the object
   service, then invokes graph V10=`1A6980` to bind the source and sampler.
   The initializer forwards its configuration; it does not replace the
   cleared pending runtime flags with saved protocol state.
3. `151DC0` sees the fresh graph node count0, prepares the source, then calls
   graph interfaceA4. `1AE740` resolves that interface to `2E1F10`, whose
   entry is `1A61C0`.
4. The actual typed source has exactly one descriptor of low-nibble kind0.
   `1A61C0` therefore dispatches `19F850`, not kind2 `1A4590` or kind6
   `19F070`. Within `19F850`, kind0 skips the nonordinary pending-control
   producers. There is no preceding separator descriptor in this owner.
5. `19EB10` receives four zero pending gate bytes, returns `FFFF` and allocates
   no auxiliary context. Ordinary seed construction `1A1EF0` copies group4
   into arc18; ordinary merging `1A37B0` carries that arc18 value. Local
   pruning and ordinary append preserve this empty auxiliary state.

This closes the fresh single-descriptor path. The pending kind3 events in the
global append/multiline work remain real and are not covered by this factory.

## Endpoint and refresh behavior

The committed backtrack producer already emits span keys using the native
`last | ushort(first-last)<<24 | alternate<<56` encoding from actual decoded
context endpoints. The current original source chronology, raw key-record index
table and `V98` adjacency are owned by `LegacyOrdinaryGraphSourceRanges`.
The new service performs the native reverse singleton lookup and endpoint
decision itself; it does not call/reverse the forward membership exporter.

`source218.VA8 = 168ED0` resolves the physical ending key through secondary38,
optional118/factor128, mapD8 and ending-key table108. It is not a shortcut to
the last sampled point's key. `VB0 = 168F50` indexes E8 into the original F8 key
table. Algebraically VA8 is VA0 at the last actual physical point. The portable
service resolves that point through the actual composed chronology and ending
metadata. It snapshots all source identities, ending identities, ranges and the
original F8 key-record table when prepared, so later caller-accessible memory
cannot change an active source owner. Original indices survive physical reorder.

The mode1 missing-start branch returns the first point of the singleton arc
starting at the earlier context. Other ordinary fallbacks choose the matching
starting arc's first point, or a physical ending key. A prior ending key is
combined only when the actual key equality/next-F8-record condition holds;
otherwise two degenerate records retain their native order. Missing singleton
lookups that would require an unsafe native pointer access are explicit bounded
refusals, not fabricated intervals.

`Read` retains the actual span key, context identities and mode. It executes
count and fill over an immutable snapshot and owns exactly24*N source bytes.
`RefreshOwner` additionally requires that key in the exact result's real glyph
pool, retains one refresh epoch per raw result and reports scalar bits3F800000
only on completion. It adds no records, calls no symbol/scoring services and
preserves all existing40-byte glyph records. Interrupted refresh is refused on
retry; native allocator-failure replay is not implemented. These stages do not
claim accepted text or complete `0213A0/021000` integration.

## Verification

Six real raw inputs include a reversed physical order. Their real CASE decoder,
terminal, compact backtrack, association and publication produce six genuine
negative pool owners. These refresh without changing packed count or any40-byte
record. Additionally5,452 explicitly prepared span requests over the actual
graphs succeed;5,210 requests lacking native singleton endpoints are refused.
The output contains8,944 source references and3,486 two-record results. Both
bit56 values and all six endpoint decisions are covered. The independent Python
audit builds the original key/ending-key table from the analytic raw point
counts, resolves the captured fixed-point map, scans native-order singleton
arcs and checks all source24 bytes. Expected transcriptions are never supplied.

Managed and Windows NativeAOT captures are compared byte for byte. There are72
scope/cancellation/lifetime guards, including attempts to alter the graph range
and point-map memories after source preparation. The probe CLI is AK, LK,
engine profile, separation profile, case data, publication data, line30 model,
line50 model and output JSON. Run `audit_output.py capture.json` independently.

`tools/handwriting/audit_main_span_glyph_refresh.py` pins17 PE regions, four
virtual bindings, constructor registration and110 exact instructions. Ghidra
read-only focused exports corroborate the clone/configuration boundary and
caller contracts. Static analysis never loads or executes vendor code.
