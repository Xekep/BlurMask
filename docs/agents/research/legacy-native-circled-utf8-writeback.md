# Default circled UTF8 helper

This is the complete bounded **default valid UnicodeUTF8** Engine0491A0 marker-classification and conditional writeback stage, including its canonical scratch. It restores all supported payload decisions, not an ASCII-only subset. It is independent of vendor DLL execution and does not claim handwriting recognition or winner selection.

## Proven default bindings

Engine031910 bootstrap reads descriptor pointer DAT252408, whose pinned value is20F3C0, builtin service37. Without an explicit custom override it registers this descriptor and writes global90=descriptor20=20F290. Virtual10 is089680. Its assembly supplies the explicit interval length to bounded memchr, checks remaining length, then compares the interior bytes. Marker search therefore honors the supplied end and can cross embedded NULs; it is not unbounded C-string matching.

0491A0 prepares scratch through048190/04AA10 using the default UTF8 service. The portable canonical scratch is already independently verified against official Unicode9 data and normalization tests. Start marker is F0000/F3B08080; end marker is F0001/F3B08081. It takes the first start marker after the current cursor and the first following end marker. Missing start/end stops searching. An unrecognized payload remains in the scratch; the next search starts after its end marker. Nested starts are not recursively reparsed before that end.

Successful mapping invokes094960 to replace the entire start/payload/end interval with one circled scalar.094960 updates the end iterator to the new insertion end. The implementation streams unchanged suffixes in linear time, which produces identical bytes because a replacement cannot introduce F0000/F0001. It avoids repeatedly allocating and moving the entire remaining buffer for long marker sequences.

Crucially, original output is changed **only if at least one replacement succeeds**.04A2F2 tests changed register15B and skips destination clear/copy when zero. Thus ordinary accents, BOMs, noncharacters and malformed-looking marker placement are not silently normalized when there is no successful replacement. Once any mapping succeeds, the complete canonical scratch is written back, including normalization elsewhere and the default UTF8 writer's FFFE/FFFF→1A substitution. The public result exposes owned bytes and the changed bit; it does not fabricate native pointer/ABI state or a custom charset.

## Complete payload decisions

ASCII arithmetic produces103 distinct targets: digits0..50 and Latin A..Z/a..z. Exact byte lengths1/2 matter; leading zeroes, signs, spaces and arbitrary longer numeric strings are not inferred as circled numbers.

The native non-ASCII decision tree has158 accepted literal paths:139 at byte length3,17 at6, one at12 and one at15. They produce127 distinct targets in3260..327E,3280..32B0 and32D0..32FE. Together with ASCII this is230 distinct supported outputs.31 extra paths accept compatibility Hangul Jamo aliases; this is an explicit native matching rule, not broad NFKC conversion.

The original read-only exporter traverses the pinned static comparison CFG, propagating byte-domain equality constraints. Every accepted path must constrain **every payload byte to one literal**; unsupported instructions, ambiguous domains and budget overruns abort. It does not load the DLL, call a vendor routine, emulate a CPU/native executable, or produce results from sample OCR labels. Switch targets and source SHA are checked before extraction. All127 standard literal payloads independently match NFC forms of official Unicode9 `<circle>` decompositions. The portable numeric model is checked again in managed/NativeAOT code.

`models/text-unicode/legacy-circled-payload-data.json`:3064bytes, assetSHA701f03d601fa843a980abc4e9cf0c7175e707bb208b64866439b34871e9c05e2; numeric tableSHAa162c735d60ca0c0d0581cc40a4e3882e1de805de205fb18cfa8d50f983e45cc. Engine source pin isd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c. Runtime needs only portable models.

## Independent checks and actual composition

1050 independent string-level cases cover all240 official Unicode9 `<circle>` entries, both successful and unsuccessful writeback with leading BOM/decomposed accents/noncharacters, embedded NULs, unmatched/nested markers, unknown payloads and multiple successive conversions.232 public decomposition inputs map to230 distinct targets: supplementary italic circled C/R have the same plain payloads as ordinary circled C/R and resolve to those existing targets; eight remaining decompositions are unrecognized. The tests do not create additional mappings for them.

Additional controls cover31 explicit native aliases,65792 exhaustive one/two-byte arithmetic decisions,4096 consecutive marker replacements, corruption/size/cancellation guards, and189 stored actual RU alphabet glyphs including an unpaired F0000 token. A further composition uses actual owned40-byte274 descriptors → full glyph emitter/helper → fresh0213A0 caller → string pool/classification; converted A, unpaired F0000/F0001, smiley conversion and empty removal verify the boundaries together. Prepared owners do not become fabricated accepted OCR winners.

The existing fresh caller's explicit full-emitter callback can now receive `LegacyNativeCircledUtf8Writeback.Emit`; no old frozen ordinary emitter API was changed. Native custom input/output charsets, invalid/legacy five/six-byte UTF8, allocation failure layout and non-default property services remain outside this declared scope.

```powershell
python tools/LegacyCircledUtf8.Probe/reference_controls.py D:/UnicodeData-9.0.0.txt models/text-unicode/legacy-circled-payload-data.json D:/circle-controls.json
dotnet run --project tools/LegacyCircledUtf8.Probe -c Release -- models/text-unicode/legacy-circled-payload-data.json models/text-unicode/legacy-normalization-data.json D:/circle-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/text-unicode/legacy-text-publication-data.json
python tools/handwriting/audit_legacy_circled_writeback.py --output D:/circle-audit.json
python tools/handwriting/export_legacy_circled_payload_data.py --output D:/circle-model.json
```

Primary independent data: [Unicode9 UnicodeData](https://www.unicode.org/Public/9.0.0/ucd/UnicodeData.txt), SHA68dfc414d28257b9b5d6ddbb8b466c768c00ebdf6cbf7784364a9b6cad55ee8f. The pinned fixture generator uses these public expectations and original marker cases; no vendor runtime executes during any probe.
