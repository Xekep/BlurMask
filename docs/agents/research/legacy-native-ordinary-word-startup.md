# Ordinary main word-startup query consumer

`LegacyNativeOrdinaryWordStartup` restores the regular single-symbol,
nontracing branch of Text`1801fd240`. It consumes complete native32-byte source
query pages, performs actual context graft/qualification, enumerates serialized
category alternatives and owning source-root arcs, scores full80-byte acoustic
requests, and emits fully populated initial word records through the native heap
router. It does not infer a winning word or recognition confidence.

## Proven producer chain

The frontend is the existing ordinary222/21b-backed graph. Its virtual280
`155710` forwards graph.virtual1C8, whose ordinary21b target`05a400` returns
false. Controller2601=false and tracing2368=false are the bounded branch
contract. The native600 input bits therefore do not enable`1f7080` here; only
input bit10 changes the regular category-variant count from two to one.
Controller22f7, permission2309, suppressPreviousContext2609, live secondary scale,
prepared selected-source headers, final cache rows and optional frontend graphE8
suppression remain explicit owning inputs. None are supplied as implicit
diagnostic defaults by this API.

`20c1c0` resolves actual UTF8 alphabet labels into the controller:

| Native field | UTF8 symbol | Scope |
|---|---|---|
|24ba/24bc|apostrophe/quote|separate context/category rewrite, unsupported here|
|24dc|U+F0001|skipped before context/header/cache lookup|
|24de|U+200C|complex context rewrite remains unsupported|
|24e2|U+0651|skipped before context/header/cache lookup|
|24ec/24ee/24f0/24f2|U+F0002/3/4/8|extra secondary allocations, unsupported here|

Absent alphabet labels remainFFFF. Protocol/extra symbols and non-single-symbol
query contexts are unsupported. The source tag's bit41 is **not** grounds for
rejecting a regular letter: native uses that tag flag in the writer, while special
alternative remapping is conditional on apostrophe/quote. Real Russian lowercase
query rows carry that flag. Both flagged and unflagged letters are tested.

For each supported query row the producer:

1. Resolves the actual prepared source header by source tag byte04. A missing or
   zero source ID is an ownership error.
2. Verifies the query context's current symbol, grafts the existing FFFA prefix
   for a single initial context, then calls the prepared frontend110 qualifier.
3. Obtains native source-root successors via21b50/58 with previous node−1,
   preserving ordinal order. It enumerates StartCompositeMap, then
   EndCompositeMap unless input bit10 suppresses the second variant. An absent
   ending map stops variants; a missing starting map is unsupported because the
   native geometry lookup has no bounded valid index in that case.
4. Builds the exact startup request with source kind from the prepared header,
   geometry index from the actual composite profile, query base cost and query
   flags. The supplied full80-byte residue remains explicit. Frontend128 consumes
   the actual node/arc cache and updates cost plus accumulator.
5. On accepted scoring, derives all initial writer fields below and invokes the
   existing initial writer/router. Allocation failure stops the pass immediately,
   matching native`local_res10`/`bVar21`, rather than silently skipping the row.

## Initial record ownership and fields

| Writer field | Restored producer |
|---|---|
|source tag|query00|
|context08|actual single-symbol FFFA graft|
|candidate10 / increment14|scored cost cell / request30|
|source node / packed arc / source arc|ending node / actual returned root ordinal|
|composite category|actual selected Start/End map|
|previous arc / anchor arc|FFFF from the native startup branch|
|span / anchor ending|current initial source span / ending node|
|count26|zero from ordinary frontend280=false,2601=false branch|
|header flag5|actual alphabet.virtual40 bit unless controller22f7 suppresses it|
|header flag6 / requested secondary / state bits6,7|zero in this regular first allocation|
|kind27|query flags low3|
|state21|query length +((query flags>>5)&3), then native six-bit writer masking|
|auxiliary34|actual owning graph arc+10, represented by StartContextId|
|auxiliary30|auxiliary34 when query flag8 is set; otherwise−1|

The alphabet flag is a literal native flag, not a guessed uppercase property.
Engine class2b table`1801de1b0` slot40=`052b50` reads the optional alphabet bitset
at runtime58; reader`053f20` produces that bitset from the stored resource.
`LegacyInkAlphabet.IsSymbolFlagged` already restores that payload.

Frontend168=`152a10` forwards graphB8=`1a7bf0`, which reads resolved **arc+10**,
not group+10. Native`1a1ef0` assigns predecessor-set start identities to arc+10;
the already ported seed graph reproduces those identities. Native`1a37b0`
copies left arc+10 and right arc+14 into a merged arc. The candidate graph's
StartContextId/EndContextId preserve that producer. Auxiliary34 is the signed
reinterpretation of the owning U32 start identity; it is not an invented source
timestamp, group ID or zero constant.

Calls must be serialized. Callback cancellation is checked by the existing
scorer and after allocation. Failure, exception or cancellation invalidates the
producer. A published heap allocation remains owned by the supplied router;
there is no rollback claim for a partially completed native page.

## Verification

The independent Python oracle parses the actual acoustic alphabet/flag bitset and
main category payload at188222. Its512 cases compare complete128-byte heap
records, covering both query source IDs, native tag bit41, all source kinds,
nonuniform geometry indices, signed32 wrap, optional suppression, query length
and state masking, zero/nonzero flag8, alphabet flags and their suppression,
one/two category alternatives and word-bitset qualification. Stable context IDs
are checked against the separately proven symbolic owner, rather than pretending
to be native pointer values. The controls emit330 scoring requests and196 owning
allocations. Sixteen additional controls cover failure, postcallback cancellation,
invalidated reentry, bad widths, missing header/context ownership, unsupported
symbol branches and early excluded-label skips.

The real resource consumer uses raw analytic strokes→ordinary cuts→actual
candidate graph→prepared mask→real NN/final195 cache→actual source permissions→
frontend110→typed native query pages. One whole-prefix NN row yields10 actual
query labels,10 ordinary accepted allocations and10 materialized records with
writer fields produced by this API. Capacities are explicit ample test inputs;
quota/controller capacity production remains a separate existing stage.

Generator and Managed/Windows NativeAOT probe use the same five arguments:

```text
controls.json
models/ru-RU/ru_RU-ak-cur.lite.res
models/ru-RU/ru_RU-lk-text.lite.res
models/ru-RU/legacy-engine-profile.json
models/ru-RU/legacy-separation-profile.json
```

Static extraction and independent controls do not execute vendor DLLs. Main
first-result-derived source/frame ownership and guide/cache policy remain
explicit in the resource fixture. Apostrophe/quote, protocol tokens, primitive
extra allocations, general context normalization, tracing and controller2601
branches still require restoration. Full`201270` parent candidate generation,
`2144f0` orchestration/winner and calibrated Russian AI-pero acceptance remain
unfinished; this is not a complete native decoder or accuracy claim.
