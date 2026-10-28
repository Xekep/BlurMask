# Primary28C alphabet conversion: static proof and next bounded runtime proposal

This is the next executable prerequisite after the owning13A.V80 primitive. It proves the primary16-entry source-to-acoustic code table for the exact retained Russian number source and acoustic135. No runtime is implemented in this checkpoint. It does not synthesize a28C query, assume empty auxiliary histories, select sources automatically or claim recognition quality.

## Actual native owners and defaults

28C descriptor2E91D0 points to metadata2E9238. Metadata+10 points to initializer list2E91B0, whose third pointer is23DFE0. The complete224-byte initializer binds the supplied13A source at+28, clears+34C and+34D, zeroes the eight0x50-byte cursor records at+C8..+347, initializes state/callback slots and sets the ring cursor index to0. Preparation23E380 reads+34C only as the optional normalization fallback gate after an unsuccessful direct target-alphabet lookup. The selected fresh no-property-mutation owner has this flag0. This is a fresh-owner scope, not an inference about later property changes.

The retained13A source uses V30=00CA630, a complete five-byte getter of source+70, to obtain its inline alphabet. Existing139/301 reader1D6700 reads the serialized source flags into source+68 and creates that alphabet owner. In the exact number child[299000,301580), raw flags at299096 are0x101; alphabet kind2B/version103 is at299104; its payload is[299110,299573). The three bytes before inline23C at299576 are alignment storage. Source flags0x101 are not silently treated as the28C normalization flag or a case-wrapping option.

The exact acoustic135 resource has its192-label alphabet payload[99,5233), and stored sourceKind135 at5233. The source and acoustic resources retain their independent label spaces. Engine readable alphabet dispatch1DE1B0 has V28=0527E0 exact byte-string lookup, V30=052A80 text extraction via its reverse slot and V38=052650 reverseFFFF virtual-symbol test. Complete lookup/extraction bodies and the complete24-byte virtual leaf are pinned. Text018F70 constructs a bounded byte-string argument and calls the supplied alphabet's V28. The already-restored stored alphabet parser supplies the same actual token bytes; no Unicode case-folding, word correction or model output is used here.

All16 actual source reverse slots are concrete. The source contains one-byte NUL at label0, digits0..9 at labels1..10, space at11, period at12, comma at13, plus at14 and minus at15. Native23E380 handles special byte strings BEFORE direct target lookup. NUL maps toFFFD; ordinary space maps toFFFB, despite the acoustic alphabet containing a literal space atID180. Digits and punctuation then use the actual acoustic byte lookup. The exact primary map, indexed by source label, is:

```text
FFFD, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, FFFB, 141, 139, 138, 185
```

The source's digit0 is label1, mapping to acoustic0. Source label0 must neither become acoustic digit0 nor be dropped as unknown. The static audit derives the table directly from the two pinned alphabets and native special literals, then projects all299 actual outgoing source labels through it. This projection is a consumer-index check, not an implementation of state0 query/history semantics. The first state0 arc to target97 maps toFFFD.

The broader native special branches also include NBSP→FFFC, LF→FFF9, replacement→FFFF and multiple private-use codes. Their exact literals and full23E380 body are pinned as boundaries, but the proposed first runtime factory admits only this exact16-label source. It must not imply arbitrary token/normalization support by offering a generic special-string recognizer.

## Primary versus secondary tables

23E380 produces primary table+48, secondary table+50 and a working primary copy+58. The secondary table does not simply equal primary: ordinary matched labels stayFFFF there; special-space/LF treatment depends on external knowledge+C4. Source0 NUL remainsFFFD. This checkpoint does not prove that external C4 producer and does not expose a secondary-map boolean pretending to be an actual default. The primary table is independent of that condition. Fallback normalization34C is a different field and is proven0 for the admitted fresh owner.

Preparation also populates target-alphabet special IDs+A0..+BA, including digit/lower/upper contiguous-range checks. Those are consumers for later history and special branches; the next runtime map does not invent a full prepared28C owner merely by allocating their numbers.

## Exact cursor, paging and history boundary

Preparation calls retained13A.V78, whose actual getter10BAB0 returns raw23C field20 and clears its flag. The observed value12 supplies scratch allocation counts for this owner. It is not a universal initial state, fanout invariant or independently invented query limit. It allocates eight cursor records, each0x50 bytes, with per-cursor target/label/cost arrays, retaining the same source cost table. Complete23DED0 resets each cursor's cached state and counters and sets ring index+348 to0.

23F6F0 rejects disabled query flags, retains caller/source cursor identity and selects a cursor from the eight-slot ring when needed. Repeated state requests reuse the retained adjacency slice; changing relevant query fields reprojects mapped labels. A page request moving backwards resets emitted/scan counters. It calls the source V80 on a new state and maps each returned source label through+48 or+50 according to the entry mode. State0 delegates through callback+98, selected during preparation by the source alphabet's U+200D lookup and the initial V80 result. Callback240010 and alternative2406C0 are complete pinned bodies, not swapped with ordinary nonzero-state emission.

The state0 callbacks skip mappedFFFF rows, carry scan and emitted counters separately, emit up to the requested page length and preserve source target/byte7/cost data and caller flags. They can obtain or retain symbol-history objects through0918D0 when corresponding caller history owners exist. Complete2413D0 is an additional auxiliary/history consumer reached by28C.V58. The actual13A.V58 is04A2C0=true, unlike the ordinary2B7 false branch. Neither alphabet conversion nor this outgoing primitive grants a receipt proving those auxiliary histories empty. No full28C runtime should be made by borrowing the2B7 empty-auxiliary adapter.

## Proposed NEW runtime checkpoint, subject to root review

Implement a NEW immutable `LegacyFst28CPrimaryAlphabetMap` plus NEW probe/oracle/doc only. No old source, outgoing, app, dictionary or default seams. The actual factory takes the owning `LegacyFst13AOutgoingSource` and actual acoustic resource bytes. Require that source's actual-fresh135 authority and exact retained root/child identity; obtain the source alphabet from that child envelope, validate kind/version/origin/count/span and source flags0x101, snapshot the bounded acoustic bytes before hash/sourceKind/alphabet reads, and load the exact192-label acoustic alphabet. Do not accept caller-supplied labels, a source-name string, arbitrary calibration or generic diagnostic outgoing ownership as actual authority.

Retain the same typed outgoing source and own16 immutable primary code entries. A bounded label lookup can expose the primary code and special/ordinary classification. Labels>=16 refuse. Defensive copies, source identity, cancellation/work/byte preflight, caller mutation, concurrent immutable use and Managed/NativeAOT parity must be tested. No old-result coercion: the existing outgoing result does not contain its source owner, so a convenience method must not accept a foreign detached result and claim same-source provenance. The first map can simply expose source-bound label lookup, leaving composed query results for a separately proved owning adapter.

Tests must independently derive all16 mappings from both actual resource byte streams, verify all299 outgoing source-label indices and preserve NUL/space special precedence with negative digit0/literal-space controls. Exercise exact same source/AK ownership, equal/foreign root and generic diagnostic source refusals, wrong AK snapshot, bounds/cancellation/mutation and detached storage. No targets, winner tuning or expected numeric strings. The primary map should not expose secondary C4 policy, cursors, paging, normalization fallback, history insertion or28C-success state. Those remain explicitly missing before full query admission.

Reproduce static proof with:

```text
python tools/handwriting/audit_linguistic_fst28c_primary_map.py --sdk SDK_X64 --models MODELS --output REPORT.json
```

The checkpoint validates11 complete Ghidra functions/11 full regions and25 exact data/leaf spans from both pinned PE files, then derives16 mapping entries and299 label projections. Native binaries were only read statically. The body/data proof and projected map do not improve the current Russian132/261 result or establish bilingual readiness.
