# English LK: actual aligned packed dictionary boundary

The actual English LK is3300812 bytes, SHA2569f4ef50bca4a1e3cc42189c279b72bf8fdf03f62e638673079d57075e168f7c9. Its resource root is13F/v102. Its serialized21 children are one13D/v10A grammar configuration, one13B/v300 main lexicon and19 grammar13A/v203 objects. This is serialized order, not an asserted configured controller priority or permission epoch.

## Actual fields and missing owners

Main13B occupies184..2832716. It delegates inherited139/v301 and138/v100 metadata. The identifier is `en_US-lk-text.lite.lex`, weight60=0, weight64=65536, field68=1, languageScale=1. Its actual lexical alphabet is type2B/version104 with104 entries at payload294..2568. The following13B flags are1: dictionary2B4/v102 at2572..1252412 and language2D2/v101 at1252412..2832716 are present. Existing RU prefix and language parsers accept alphabet103 and2D1/v100 respectively; neither should be relabeled as English.

The selected actual English acoustic alphabet has217 entries, payload99..7907, from en_US-ak-cur.lite.res SHA256ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657. Exact UTF8 comparison finds18 lexical accented strings without an exact acoustic string. This is a diagnostic map fact, not a proposal to alias them or claim native normalization disabled. The native source normalization flag path and case alternatives must be traced before producing masks. Existing RussianModelBundle/CaseSymbols owners remain unsuitable even where generic pure case/context helpers can be shared.

The object hierarchy matters:13A and13B derive139?27B?138;13D derives138. Native20E.Add1EAEB0 appends each supplied27B to its ordered resource vector, keeps13D separately, and increments revision38 after accepted changes. Root13F reader1D1240 loads serialized children and aligns between them; its enumeration1D1B40/1D1C40 can flatten nested roots. This does NOT establish that the application's configured source collection includes all21 children or only main13B. The owning configuration selection, resulting source priority, case/grammar wrapper choice, reset/permission epochs and main mask publication are explicit next boundaries; no mask owner is implemented by this package.

## Aligned packed-array variant

2B4 flags are0; nodeCount666547, maximumWordLength43. Labels use8-bit packed values, with terminal bit80 and label mask7F. The first packed header is0x01000001 at2600, count666547, width8, metadata byte1, two preserved padding bytesFF; data starts2612 and ends669160. The child-offset width byte is17 at669160. The second packed header begins UNALIGNED at669161. It requires three bytes of padding before its count, then width7/metadata1/two more padding bytes; data begins669176 and ends1252408. The final terminal count is260074 at1252408, and complete dictionary consumption ends1252412.

Engine08F980 reads the array version. For versions above01000000 it uses stream28/30 alignment around count/width/storage and skips the additional metadata byte. Actual version01000001 chooses those branches. The normal copy branch allocates an owned8-byte internal header plus ceil(count*width/32) words, stores count/width and own-storage marker0, then reads words. Its alternate stream-backed pointer branch has a separate lifetime contract and is not needed for an independent owning implementation. Writer08FC90 emits01000001, the metadata byte1 and matching alignment. This is an aligned wire representation of the same packed labels/counts, not permission to overwrite either header or reinterpret the old unaligned bytes.

Engine default global031910 (no external registration callback) loads registration pointer252420?20F690 and installs registration+20=20F640 at root metadata+98. Custom provider replacement is outside this proof. Engine table20F640+18 binds08F980, +20 binds writer08FC90, and +30 binds bulk getter08FF40. The bit getter reads an unaligned-width value across at most two32-bit words. Text248D90 selects its single packed-label backend when flags mask0x4 is clear (actualflags0), derives terminal/mask from stored label width, reconstructs relative child offsets from breadth-first child counts, then reads terminal count. Text091CE0 computes child intervals. Text093B50/0920A0 query/materializer consume these same backend arrays; node0 adds one to its child start before map filtering. Text248730 enumerates terminal paths. Packed format version does not justify bypassing working/preserved/auxiliary maps or fresh source epoch rules.

## Independent reference

`tools/LegacyEnglishLkPacked102.Probe/reference_actual.py` reads only the exact raw resource and emits a compact diagnostic. It extracts every value twice using independent byte-window and32-bit-boundary methods, repacks all words and checks byte equality including unused tail bits. It verifies all666547 nodes, exact root/child coverage, increasing distinct sibling labels,104-label bounds,17-bit relative child offsets and maximum depth43. A streaming DFS visits all260074 terminal paths and hashes their label sequences; no expected words/labels enter this computation. Bounded deterministic node queries also record the root-child skip boundary. This is a dictionary representation test, not pen recognition accuracy or full source query permission equivalence.

`tools/handwriting/audit_english_lk_packed102.py` checks exact raw resource fields and optionally the pinned Engine/Text PE bytes. Its JSON pins use actual complete Ghidra function-body ranges, direct and indirect call-site bytes and concrete table bindings. The proof is portable and needs no research imports, E paths, Ghidra install or vendor DLL execution. Full body hashes pin the reviewed static boundaries; they are not a blanket all-branches native equivalence claim.

```powershell
python -I -B tools/LegacyEnglishLkPacked102.Probe/reference_actual.py models actual-packed.json
python -I -B tools/handwriting/audit_english_lk_packed102.py --models models --output audit.json
#Optional original vendor bytes, STATIC ONLY:
python -I -B tools/handwriting/audit_english_lk_packed102.py --models models --sdk path/to/sdk/build/x64 --output native-audit.json
```

## Proposed next reusable runtime stage, subject to review

NEW typed English packed-data owner may read actual English source247 and exact lexical resource, retaining identity to that selected source, actual lexical104 alphabet, immutable root inventory/prefix and validated dictionary. It may expose reusable exact dictionary transitions and data-only2D2 boundary metadata. It must not expose a fabricated language model, English mask, priority epoch or winner. Old public RU guards stay unchanged. A proposed ONE old seam is `LegacyPackedLexicon` sealed?sealed partial declaration, permitting a NEW companion to construct its existing private packed storage after a NEW exact-v102 reader validates all fields. No header/version rewrite, old Load relaxation or Russian model owner is needed. Bounds/cancellation, complete preflight and copied storage are mandatory before publishing the new data owner. An actual English configured-source/mask owner remains a subsequent native closure and review.
