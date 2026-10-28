# Complete bounded ordinary legacy glyph UTF8 emission

The native Engine0491a0 helper is **not a general in-place Unicode normalizer**. Its canonical decomposition and composition calls operate on a temporary vector. Static register/branch evidence proves the no-circled-start output path:

1. Original input vector is retained inR12 at0491cd.
2. R15B, the circled-replacement flag, is zeroed at04926c.
3. The helper searches the canonical scratch for UTF8F0000/F0001 markers.
4. No start marker branches0492d4→04a2f2.
5. The zero replacement flag branches04a2f5→04a42f, skipping the original vector's count reset at04a300 and every writeback operation.
6. The optional change flag receives zero, and the scratch vector is freed separately.

R15 is assigned from successful scratch replacement only at04a2c4. The pinned2060 canonical decomposition rows and940 composition outputs contain noF0000 target; algorithmic Hangul outputs only its bounded Jamo/syllable ranges. Consequently, valid Unicode input with noF0000 scalar cannot acquire a start marker through temporary canonical processing. A successful helper leaves the original interval intact, including decomposed accents, leading BOM, CR/LF, noncharacters and private markerF0001 without a start marker. Replacing this path with framework NFC would change native output.

`LegacyNativeOrdinaryGlyphUtf8.Emit` composes actual glyph274 descriptor→token expansion→primary/alternate raw bytes→this identity policy. Primary alphabet, result273+18 control lookup and result273+20 alternate table remain distinct owners. The independent code owns its byte output, enforces a1MiB budget and cancellation, and checks valid Unicode UTF8. The native extended malformed/5–6-byte codec paths, anyF0000 start-marker input, custom Unicode/charset overrides and native allocator failure paths remain outside this bounded branch. Such input is refused; no partly normalized text is published.

This completes **ordinary glyph byte emission**, not accepted source-record publication or the handwriting winner. Native021000 still interns the UTF8 interval through source2DA+118, classifies flags with019030/018b40 and supplies a40-byte source row. That actual owner/category lifecycle and actual winning source references are separate required stages. No source IDs, recognized labels or confidence are manufactured by the emitter.

Reproduce without executing vendor code or opening a GUI:

```powershell
python tools/LegacyGlyphUtf8Tokens.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/glyph-utf8-token-controls.json
python tools/LegacyOrdinaryGlyphUtf8.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/ordinary-glyph-utf8-controls.json
dotnet run --project tools/LegacyOrdinaryGlyphUtf8.Probe -c Release -- D:/BlurMask-build-artifacts-20261011/ordinary-glyph-utf8-controls.json D:/BlurMask-build-artifacts-20261011/glyph-utf8-token-controls.json models/ru-RU/ru_RU-ak-cur.lite.res
dotnet publish tools/LegacyOrdinaryGlyphUtf8.Probe -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/ordinary-glyph-utf8-aot
& D:/BlurMask-build-artifacts-20261011/ordinary-glyph-utf8-aot/LegacyOrdinaryGlyphUtf8.Probe.exe D:/BlurMask-build-artifacts-20261011/ordinary-glyph-utf8-controls.json D:/BlurMask-build-artifacts-20261011/glyph-utf8-token-controls.json models/ru-RU/ru_RU-ak-cur.lite.res
python tools/handwriting/audit_legacy_ordinary_utf8_passthrough.py research/myscript/sdk/build/x64 --output D:/BlurMask-build-artifacts-20261011/ordinary-utf8-passthrough-audit.json
```

The original oracle checks1036 exact-byte ordinary identity cases and12 explicit malformed/start-marker refusals; frozen token controls provide345 byte/helper compositions and105 restricted alternatives. The actual pinned RU resource supplies188 stored-label ordinary byte cases; one genuineF0000 alphabet token is refused because its circled lifecycle is unfinished. These are prepared descriptor/alphabet tests, not Russian OCR accuracy. The auditor checks10 pinned instructions/branches and all2060/940 table targets. Ownership, preservation of decomposed accents/BOM/line endings, budgets and cancellation are checked in managed and NativeAOT execution.
