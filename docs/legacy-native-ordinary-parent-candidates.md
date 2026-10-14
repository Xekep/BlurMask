# Ordinary main parent candidate production

This batch connects the regular branch of Text`201270`: actual stored parent88
and typed continuation query rows produce full80-byte requests, actual main
frontend118/140 geometry and frontend128 acoustic scoring, the bounded native
path guard, and complete initial-writer records in owning heaps. It is the parent
candidate stage, not the outer`2144f0` scheduler or final winner.

## Request producer

`LegacyNativeWordParentRequestFactory` restores the literal initializer from
`201270`. It is deliberately distinct from startup`1fd240` and higher-arc
`20e6a0`; applying either existing request factory unchanged would be wrong.

| Request offset | Actual producer |
|---|---|
|00|current ending node|
|04|parent row U16 source node18|
|08|previous ending node, native param6|
|0c|actual returned current source arc|
|0e|parent packed source arc, row18>>22|
|10|parent graph arc, row1c|
|12/13|current/parent composite geometry indices|
|14/18|literal native zero FirstId fields|
|1c low byte|query flags&7; overridden to7 if parent header6.bit0 is set|
|20|current source span|
|24/26|query current symbol / parent symbol|
|28|owned portable cost-cell identity|
|30|parent incremental cost14|
|34|native impossible limit0x4000000|
|38/39/3a|query length /zero/ permission2309|
|3c low byte|zero before callbacks; upper padding retains caller residue|
|40/44|current/parent prepared source kinds from header>>9&15|
|48/49|query tag top-byte bit0 /parent header7.bit0|

The cost cell starts at signed32-wrapped query base cost plus parent candidate10.
The full supplied80-byte residue is copied; unwritten padding is not invented.
The current/previous source nodes and packed/current graph ordinals remain
distinct. The native hard-zero FirstId fields are not replaced with the composite
FirstId mapper used by the higher-arc path.

## Regular producer and consumers

`LegacyNativeOrdinaryParentWordCandidates.ProcessPage` is called after the
existing `LegacyOrdinaryDictionaryQueryPreparation.ApplyContinuation` gate and
real dictionary query preparation. It owns copies of the bounded parent and
query page before callbacks. Source headers are resolved by actual aggregate
source IDs, current/previous source-graph lineage comes from the owned ordinary
connections, and costs come from actual selected cache rows.

The bounded scope is ordinary frontend280=false,2601=false and tracing=false,
same-descriptor plain letters/digits, query kinds0/2, and existing main297
geometry. It preserves literal regular row gates:

* Query flag8 with an already nonnegative parent auxiliary30 skips the row.
* U+F0001 with parent auxiliary30=−1 skips the row; the remaining primitive
  branch is unsupported rather than changed into a regular glyph.
* KnowledgeF5 skips current source-kind2 rows. This controller field is explicit.
* The prepared qualifier consumes the **query context directly**; it does not
  add the startup FFFA graft to a parent query.
* Start/End category variants and actual successor order are retained. Input
  bit10 restricts variants to one. The ordinary frontend's false280 means the
 600 input bits do not enable case rewriting.

Each candidate begins with its parent increment and combined query/parent cost,
then calls actual main118,140,128 in native order. The resulting accumulator30
is retained for the new writer record. Native`1fcf40` accepts the proven regular
branch when kind is not7 and request3c low byte is zero. Here the packet producer
sets that byte zero and the supported main callbacks preserve it; the consumer
checks both conditions. It does not return an invented true value for marked,
kind7, source-boundary, control-root or other unported paths.

All writer fields come from their producers:

* Current source node/arc/category and previous **stored row ordinal** are
  distinct; the previous graph arc is used for graph traversal.
* Context08 is the real current query context. In this unmarked regular branch
  neither the sourceC1 nor knowledgeA0 context-rewrite paths apply.
* HeaderFlag5 is suppressed by controller22f7. Otherwise kind0 inherits the
  parent high header bit OR the current alphabet flag; kind2 uses the current
  alphabet flag alone. The flag is literal alphabet.virtual40 data.
* Auxiliary30 inherits the parent value unless query flag8 supplies current
  arc.StartContextId. Auxiliary34 inherits except kind2, which supplies that
  current start identity. Both use native signed reinterpretation of arc+10.
* The owning anchor38/3c is inherited from the **materialized** parent. Native
 `213410` already changes a first record's FFFF anchor arc into its destination
  ordinal, so resetting it toFFFF here would destroy lineage.
* Count26, HeaderFlag6, requested secondary and state bits6/7 are zero in this
  bounded branch. Kind and small state come from query flags/length. Native
  secondary scale, previous-context suppression, cache/E8 suppression and
  actual row lookups remain explicit policy.

Allocation failure stops immediately. Failed/canceled/throwing preparation
invalidates the serialized producer. Postcallback cancellation is checked for
acoustic caches and row lookup before further ownership consumption. Partially
completed native heap mutation is not presented as rolled back.

## Verification and limits

The independent Python generator reads the actual main category payload188222
and produces1024 full80 requests, including distinct previous source/ending
nodes, packed and graph arcs, arbitrary residue, both parent header branches,
different geometry indices, inherited accumulators and signed32 wrapping. Six
additional guards reject malformed extents/categories and cancellation.

The actual resource consumer starts with raw analytic ink, ordinary graph, real
NN/final195 caches, source permissions and frontend110. Ordinary startup emits
11 real stored parent88 records. Their real tags enter typed native continuation
queries:11 pages/23 query rows feed25 complete118/140/128 candidate chains and25
owning allocations. No scripted geometry/cost callbacks are used. Ten additional
controls cover special-kind refusal, ownership widths, allocation stop,
postcallback cancellation/invalidation, and mutation of original query/parent
buffers during a cache callback. Lineage checks preserve each parent's actual
materialized anchor.
Four additional independent truth-table consumer controls verify that kind0
uses the current alphabet flag even when the parent flag is clear, inherits a
set parent flag for an unflagged current letter, clears when both are clear,
and kind2 does not inherit a set parent flag. These are controlled graph/cache
inputs, separate from the real NN resource fixture.

The same generator, resource consumer and controls run in Managed and Windows
NativeAOT with five arguments: controls JSON, Russian acoustic resource, Russian
lexical resource, engine JSON and separation JSON. Static inspection never
executes vendor DLLs.

The resource fixture still explicitly supplies guide90/80 and same-descriptor
source facts. First-result-derived main source/frame production is a separate
upstream task. Whole `201270` includes protocol/primitive/case/quote/kind7,
source-specific and tracing branches beyond this regular endpoint. `1fe740`,
full`2144f0` scheduling/winner, and calibrated Russian AI-pero acceptance remain
unfinished. These producer/consumer checks are not word accuracy or acceptance
confidence evidence.
