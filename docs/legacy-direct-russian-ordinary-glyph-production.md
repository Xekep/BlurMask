# DIRECT ordinary glyph production

The additive `LegacyOwnedRussianOrdinaryGlyphProducer.Prepare` overload accepts
the actual fresh DIRECT controller, its confidence owner, separation table,
same-confidence source callbacks and the checked portable publication data.
It shares the existing216BC0 numerical implementation with the CASE overload.
The earlier CASE API, including its unresolved replacement refusal after the
cursor decrement, keeps its behavior. `Policy` remains the CASE property;
DIRECT exposes `DirectPolicy` and refuses access to an invented CASE owner.

The DIRECT binding checks model, graph, selected resource and dictionary
identities. It derives an empty native wide-pair set from that alphabet and
scans every stored scalar with the original script table. Script5 is refused,
so no Arabic shaping map or caller-supplied absent-map flag is needed.
The real ordinary source callback closes216860's replacement branch: its
type0 graph returnsFFFF, retaining replacement label189 with external flag0.
Synthetic quote conversion still runs first and tests only the low byte of
the conversion argument. It is independent of confidence's score-label remap.

Managed controls exercise3288 prepared72 rows over46 actualNN groups, including
184 replacement cases, synthetic quotes, nonzero multi-stroke separation and
all four stored score/cost channels. The independent Python oracle parses
actual resource calibration/category data and checks every scratch/full40
byte. A further integration runs the actual DIRECT loop, terminal selection,
207CB0 compact materializer and numeric21FFD0 walker, then feeds its genuine
ordinary72 row into this producer. It does not replace the trailing protocol
row with an ordinary character or assert an expected transcript.

The existing CASE probe is rerun unchanged. Its complete JSON capture remains
SHA256 `451e40637c8ed03eaa089902c5be6d20e628ad764738f4018afdc5611b6c0ffe`.
The static audit pins107 instructions, five function bodies and five source,
graph and cache bindings, including the exact replacement fallback branch.

This is not full216BC0,21FFD0 or218560. The72-byte input remains explicit at
this API boundary; the extra integration proves one actual producer path.
Protocol multi-output, word segmentation/flush and chain publication remain
separate. No vendor library is loaded as executable code.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet run --project tools/LegacyDirectGlyphProduction.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/direct-glyph-build -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json models/text-unicode/legacy-case-data.json models/text-unicode/legacy-text-publication-data.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res E:/BlurMask-build-artifacts-20261014/direct-glyph-managed.json
python tools/LegacyDirectGlyphProduction.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-separation-profile.json E:/BlurMask-build-artifacts-20261014/direct-glyph-managed.json
python tools/handwriting/audit_direct_glyph_production.py E:/BlurMask-build-artifacts-20261014/direct-glyph-static.json
```
