# Owned English packed lexical data

`LegacyEnglishLkPackedData.Read(actualEnglishSource247, lexicalBytes, token)` now reads the actual English LK main packed dictionary and AL104 into a reusable immutable data owner. It retains the exact selected English247 identity, lexical alphabet, main13B prefix, serialized21-child metadata and packed dictionary. Existing RU loading guards and queries retain their behavior; the only old source edit makes `LegacyPackedLexicon` a sealed partial class. The NEW companion implements exact2B4/v102/aligned01000001 decoding through private existing packed storage.

This is lexical DATA, not a language model, mask/controller configuration or handwriting winner. Actual language2D2/v101 remains an explicitly named `UnsupportedLanguageBoundary`; no RU dense2D1 object, n-gram order, context or case epoch is fabricated. Serialized resource order is data, not controller priority.

## Ownership, coordinates and bounds

The reader requires the actual EN247/v110 acoustic fingerprint and exact3300812-byte English LK fingerprint. It hashes in32768-byte cancellation-aware blocks, copies a bounded private resource snapshot, then verifies that snapshot again. Caller mutation after Read cannot affect either dictionary or alphabet. The private whole-LK snapshot, count array and depth workspace are not retained after load; the source247 owner is shared by identity. Public inventory access returns copies, prefix records/strings are immutable and packed storage has no writable public surface.

Alphabet104 payload has absolute lexical-resource offset294 and length2274. The reusable prefix's `AlphabetPayloadOffset` is110 relative to its main13B at184; `LexicalAlphabetAbsoluteOffset` separately reports294. This preserves the existing prefix record's coordinate convention. The acoustic alphabet217 belongs to the exact selected source247. Exact-text map diagnostics include18 missing accented lexical entries; no normalization or alias policy is inferred from them.

Preflight computes the complete conservative logical load/copy envelope before large copies:2*3300812+666548+583232+1416416+666547+256KiB+64KiB =10262047 bytes, capped at32MiB. This includes two whole-resource copy/hash-work allowances, labels/counts/relative-offset/depth arrays, alphabet and metadata bookkeeping. It is a logical work bound, not process RSS. Final dictionary storage is exactly2082964 bytes:666548 label bytes and1416416 computed17-bit offset bytes;583232 count bytes and666547 depth bytes are temporary.

The exact owner admits666547 nodes, maximum depth43, terminal count260074, label width8, child-count width7, relative-offset width17, metadata1 and complete node1252412 consumption. All loops and hash blocks check cancellation. Tree validation checks every source label/sibling interval, root sentinel, relative-offset range, count coverage and observed maximum depth. No partial owner is published on failure; loading is stateless and a refused/canceled call does not poison later independent loads. The private diagnostic checkpoint API is internal to the probe; production has no caller callback or allocation provider.

## Verification

The raw-only probe receives model files, no expected labels, maps or output buffers. It reads every node, child interval and transition, and verifies `Contains` for every260074 actual terminal path. Managed and NativeAOT captures must match exactly. The independent Python oracle runs the committed static resource reference, derives every node record and full terminal-label digest directly from the actual packed data, and compares all104 lexical-to-acoustic exact UTF8 entries.

The suite exercises55 refusal guards,12 canceled loads (including after32768 bytes inside both SHA passes and after65536 validated nodes), caller-buffer mutation, defensive inventory copies, version/count/width/metadata/padding tampering and byte-budget boundaries. A separately named PREPARED parser control accepts skipped alignment-padding changes as native array reading does; the public exact-resource owner rejects such tampering by SHA. The old public v100 loader still refuses Englishv102. These controls are not handwriting accuracy results.

Existing English checkpoint A, V2, parent, directchild source and plain-text captures remain exact. RU packed trie/related-form/glyph-cut controls and128 independent dictionary-query streams/5120 operations/165045 complete records continue to pass, including the real RU root-skip backend. These old API tests exercise unchanged guards and representation behavior, not English language ranking.

```powershell
dotnet run --project tools/LegacyEnglishLkPacked102.Probe -c Release -- models capture.json
python -I -B tools/LegacyEnglishLkPacked102.Probe/audit_runtime.py models capture.json raw-reference.json
```

The prior portable packed102 native auditor still verifies raw bytes and complete native body/dispatch pins without vendor execution. See `legacy-english-lk-packed102-boundary.md` for that static proof and open configured-selection/case/mask/language dependencies. No Native ABI, complete engine transfer, English/Russian readiness or UI recognition quality improvement is claimed by this data checkpoint.
