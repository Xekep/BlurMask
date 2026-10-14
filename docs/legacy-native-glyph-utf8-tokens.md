# Legacy273 glyph tokens and UTF8 source ownership

The old result path already has a concrete UTF8 producer, independently of modern EED. `0213a0` and `021760` call legacy273.virtual50=`13c920`; the builtin273 descriptor is Text`2dc7b0`, method table`2dc740`. `13c920` calls `13c840`, then Text global+18/+8 service274.virtual68=`13a0b0`. Initializer`002530` installs descriptor274 at`2dc630`, method table`2dc680`.

The real glyph record fields used here are ushort+4 kind flags and ushort+8 symbol/run length. Service274.virtual50=`139d90` returns one token for ordinary kinds; kinds1/3 use `(field8>>2)&0x1fff`. Virtual58=`139c90` produces a four-byte `(ushort label, byte alternateFlag, zero padding)` token, repeated that many times by`13c840`. Kinds0/2 retain the symbol, kind4 additionally selects alternate storage, kind1 maps the low two bits to reserved labelsFFF9 throughFFFC. Kind3 looks up linefeed/space/NBSP through the **separate result273+18 control-alphabet owner**. Its nonempty subtype1 leaves an undefined native label; the portable component refuses it rather than inventing output.

These alphabet/storage owners must remain distinct:

* The primary alphabet is the external third parameter to`13c920`, supplied by its caller's frontend profile. It is not result273+20.
* Result273+18 is the dynamic control-alphabet lookup used by`139c90`; the bounded port requires its real lookup callback for nonempty kind3.
* Result273+20/+30 form the alternate raw-byte vector and offset vector. `1f42d0` copies these from the actual source clone's+170/+180 fields. Alternate token labels index that table only when a table is present; with no alternate table the native emitter follows the primary-alphabet branch.

`LegacyNativeGlyphUtf8Tokens` implements this descriptor→repeated-token→raw-byte emission stage. It preserves primary alphabet bytes, alternate offset slices and the native reserved-token output: linefeed, space, NBSP, replacement character, and private-use markersF0000/F0010–F0018. It owns all returned arrays and alternate-table storage, with explicit token/byte budgets and cancellation. Nonempty kind3 requires a prepared native control lookup; no acoustic/lexical-label alias or hardcoded Cyrillic substitution is installed.

This stage is **before native canonical processing and accepted source-record publication**. After raw bytes, `13a0b0` calls Engine global+68.virtualE0=`0491a0`. That function first canonically decomposes/reorders with`048190` and then composes through`04aa10`, using the pinned Unicode service. It also consumes private markersF0000/F0001 for circled-character transformations. Replacing this with string concatenation or the host framework's current normalization would not prove native equivalence. The API deliberately emits pre-canonical bytes; a fixture containing `e`+combining acute stays decomposed. Alternate bytes can also be invalid pending the native conversion stage, so this API does not pretend they are accepted text.

`021000` is the subsequent40-byte text-source record builder: it initializes+0, copies the glyph+6 low10 bits into result flags+8, interns the UTF8 interval through the actual source2DA+118 owner at record+10, and stores byte count at+20. It then classifies through`019030` and its native Unicode category service, or fallback`018b40`. That owner/category stage, the complete winner materializer, wrapper group-prefix construction, normalization and actual character source references remain required for full end-to-end output; this bounded component does not synthesize any of them.

The optional static auditor verifies the actual service tables and direct producer/normalizer calls without loading vendor code. Pinned SHA256: Text`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`, Engine`d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c`.

Reproduce the portable controls:

```powershell
python tools/LegacyGlyphUtf8Tokens.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/glyph-utf8-token-controls.json
dotnet run --project tools/LegacyGlyphUtf8Tokens.Probe -c Release -- D:/BlurMask-build-artifacts-20261011/glyph-utf8-token-controls.json models/ru-RU/ru_RU-ak-cur.lite.res
dotnet publish tools/LegacyGlyphUtf8Tokens.Probe -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/glyph-utf8-token-aot
& D:/BlurMask-build-artifacts-20261011/glyph-utf8-token-aot/LegacyGlyphUtf8Tokens.Probe.exe D:/BlurMask-build-artifacts-20261011/glyph-utf8-token-controls.json models/ru-RU/ru_RU-ak-cur.lite.res
python tools/handwriting/audit_legacy_glyph_utf8_bindings.py research/myscript/sdk/build/x64 --output D:/BlurMask-build-artifacts-20261011/glyph-utf8-bindings-audit.json
```

Independent original fixtures cover736 descriptors (719 valid/17 undefined refused),524288 count combinations and450 exact-byte cases. The pinned real RU acoustic resource supplies189 actual stored-label ordinary glyph→UTF8 byte checks, includingЁ/Й; those labels are prepared records, not OCR accuracy. Guards include invalid alternate offsets, output budgets, ownership and cancellation. No vendor DLL is executed.
