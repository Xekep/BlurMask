# Fresh legacy text glyph caller

`LegacyNativeTextGlyphCaller` restores the executable bounded **fresh0213A0** vector loop: actual274 owner resolution → primaryHeaderE filter → actual glyph UTF8 emission → empty removal/smiley replacement →021000 source publication. It builds on the separately verified ordinary emitter, native string pool and first-cluster classification. It does not generate a recognized winner or stroke references.

## Actual owners and records

Default Text274 instance table2DC680 virtual28 is13AE90. The result273 field48 is the separately owned glyph service object passed to it. Its64-byte tree nodes have the normalized key at10, packed count word18, capacity word1A and array pointer20; virtual08=13AC80 returns this owner node. Array entries are **40bytes (0x28)**.120-byte276 word records contain linked glyph references and are a separate contract. The new owner never treats those120-byte words as0213A0 glyphs.

The reference includes a signed16-bit record index in bits40..55. Key normalization retains low24bits, arc16bits24..39 and only bit56; other high bits are ignored.13AE90 searches the owner BST then requires `0 <= index < ((short)(packedCount << 2) >> 4)`. The port owns exact supplied normalized keys and complete record arrays; lookup is equivalent without inventing pointer values or exposing native tree traversal order. Missing native nodes can dereference NULL; the bounded library explicitly rejects them.

Glyph word4 supplies kind flags, word6 supplies source publication flags, and word8 supplies the symbol/run descriptor. These are copied from the actual owned40-byte record, not inferred from decoded Unicode. The result keeps the original reference and owner identity.

## Caller policies

0213A0 forms each reference by bitwise OR of the actual base reference with the selected row's first ushort shifted40bits. The selected row has4bytes; its other ushort is preserved, including through vector removal.

When wrapper byte8 is nonzero, only kind0 with symbol equal to actual primary alphabet runtime wordE is removed. Engine053F20 forwards **serialized header field3 at payload+6** to runtime+E. This is not a text-space lookup: the pinned RU headerE is178 with no stored UTF8 entry; actual space is header field5=180. Both actual header/count and owner must be supplied, rather than guessing a filtered label.

Empty successful emitter output also removes the selected row. Removal decreases the marker index only when the current retained index is strictly smaller than it, matching the native in-place loop. This stage returns retained complete4-byte rows and the adjusted marker together with published records.

The caller replaces exactly3 emitted bytes E298B9 (U+2639) with F09F9981 (U+1F641) and E298BA (U+263A) with F09F9982 (U+1F642). It does not replace substrings, repeated runs, longer text or visually similar symbols. This occurs after glyph emission and before021000 publication. Exactly4 bytes F3B08080 or F3B08081 set source flag100000; they represent F0000/F0001, without assigning a handwriting meaning from a bounding box.

The fresh destination vector is not an assumption: actual024F30 zeros output68/70 immediately before0213A0. Nonempty destination reuse is not exposed by this API; native0213A0 writes from index0 despite its append-capacity calculation. Native allocation failure does not become fabricated success; portable owners/work/byte budgets throw explicitly.

## Verification and remaining boundary

Independent original controls cover512 ordinary vector calls plus256 explicitly prepared full-emitter fixture calls, headerE versus actual space, runs/empty alternate entries, complete selected row retention, successive removals around the marker, exact single smiley conversion and F0000/1 flags. Owner controls verify40-byte layout, signed/out-of-range index, normalized high bits, duplicate/missing nodes, negative packed count, owned storage and cancellation.188 stored pinned RU labels compose the actual primary emitter → caller → source publication without claiming OCR accuracy.

Managed and NativeAOT run the same headless probe:

```powershell
python tools/LegacyTextGlyphCaller.Probe/reference_controls.py D:/caller-controls.json
dotnet run --project tools/LegacyTextGlyphCaller.Probe -c Release -- D:/caller-controls.json models/text-unicode/legacy-text-publication-data.json models/text-unicode/legacy-normalization-data.json models/ru-RU/ru_RU-ak-cur.lite.res
python tools/handwriting/audit_legacy_text_glyph_caller.py --output D:/caller-audit.json
```

The default emitter restores the complete bounded ordinary no-circle path. **Actual full273.virtual50 emission is an explicit callback for circled-start input** until the native0491A0 circle writeback is restored; the256 fixture calls test caller policy, not the missing circle branch. Kind3 additionally requires the separately actual control lookup owner, and kind4 uses the actual alternate owner. Neither a primary alphabet alias nor a vendor DLL is executed to fill these boundaries. Actual winner materialization and raw endpoint publication remain separate.
