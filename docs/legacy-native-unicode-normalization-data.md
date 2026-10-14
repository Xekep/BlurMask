# Portable native Unicode normalization data

`LegacyNativeUnicodeNormalizationData` removes the runtime dependency on a private PE file for the three scalar services used by the legacy canonical pipeline. It loads `models/text-unicode/legacy-normalization-data.json`, validates provenance, exact dimensions, ordering and the entire canonical binary-table checksum. It does **not** implement or invoke the full Engine0491a0 text stream normalizer.

The statically verified default Engine global+78 service provides:

| Native method | Exact portable data/behavior |
| --- | --- |
| virtual0=`05ebb0` | 517 ordered12-byte combining-class intervals at1f8560; a gap returns−1 |
| virtual8=`05ed00` | 2060 canonical rows within the5722-record decomposition table; algorithmic Hangul; service declines037E and0387 |
| virtual18=`05ec00` | 940 composition pairs indexed at1f9da0; two algorithmic Hangul branches; absent pair returns−1 |

The original exporter reads the pinned Engine bytes, then independently checks these data against the official [UnicodeData9](https://www.unicode.org/Public/9.0.0/ucd/UnicodeData.txt) and [DerivedNormalizationProps9](https://www.unicode.org/Public/9.0.0/ucd/DerivedNormalizationProps.txt). All267753 assigned code points, including expanded First/Last intervals, match native combining classes. All2060 canonical mappings match the public rows. All940 native pairs exactly match the public canonical two-component mappings after Full_Composition_Exclusion. Native combining-class gaps must still be preserved:4038 code points return−1, while other unassigned code points can lie in class0 intervals. Neither host-framework defaults nor a Unicode category guess reproduces that distinction.

The model includes the **exact native517 range coverage**, not only public nonzero combining classes. The Greek punctuation refusal remains a service policy on top of the public decomposition data. Hangul is handled arithmetically with the proven L/V and LV/T bounds. Returned decomposition arrays are copies; composition and class lookups do not expose model storage. These scalar methods accept the native numeric code-point domain0..10FFFF; strict text encoding validation belongs to the stream/codec stage.

Provenance is pinned in the model and loader:

* Engine SHA256: `d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c`.
* UnicodeData9 SHA256: `68dfc414d28257b9b5d6ddbb8b466c768c00ebdf6cbf7784364a9b6cad55ee8f`.
* DerivedNormalizationProps9 SHA256: `02d8e12cea7c61a9f3cc5fbf8eaceadf8da23fe83d60c64cf647088830b810cc`.
* Ordered portable binary-table SHA256: `7a0cd7a1a1a931466a868718394b90dc591726bad93909f7fbe5e5ea0fa2a885`.

The table digest covers each section's signed32 little-endian row count and every ordered signed32 field: `(first,last,class)` intervals; `(codePoint,count,components...)` decompositions; `(first,second,result)` composition triples. The JSON exporter writes canonical LF bytes, and the runtime verifies the numeric table rather than depending on whitespace. A host override of the native Unicode service is outside the pinned default model.

Reproduce read-only extraction and the public-data checks:

```powershell
python tools/handwriting/export_legacy_unicode_normalization_data.py research/myscript/sdk/build/x64/MyScriptEngine.dll D:/BlurMask-build-artifacts-20261011/unicode-property-audit/UnicodeData-9.0.0.txt D:/BlurMask-build-artifacts-20261011/unicode-normalization-next/DerivedNormalizationProps.txt models/text-unicode/legacy-normalization-data.json
dotnet run --project tools/LegacyUnicodeNormalizationData.Probe -c Release -- models/text-unicode/legacy-normalization-data.json D:/BlurMask-build-artifacts-20261011/unicode-property-audit/UnicodeData-9.0.0.txt D:/BlurMask-build-artifacts-20261011/unicode-normalization-next/DerivedNormalizationProps.txt
dotnet publish tools/LegacyUnicodeNormalizationData.Probe -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/unicode-normalization-data-aot
& D:/BlurMask-build-artifacts-20261011/unicode-normalization-data-aot/LegacyUnicodeNormalizationData.Probe.exe models/text-unicode/legacy-normalization-data.json D:/BlurMask-build-artifacts-20261011/unicode-property-audit/UnicodeData-9.0.0.txt D:/BlurMask-build-artifacts-20261011/unicode-normalization-next/DerivedNormalizationProps.txt
```

The probe independently parses the pinned official data and checks267753 assigned classes,2060 direct mappings,940 eligible pairs and all11172 Hangul syllables. It checks distinct gap/covered-unassigned behavior, Greek refusal, missing pairs, storage ownership, out-of-range input, altered numeric tables, provenance, truncation and model budgets. No DLL or GUI is executed. The stream decomposition/reordering/composition control flow, private circled markers, accepted source publication and final handwriting winner are still separate unfinished stages.
