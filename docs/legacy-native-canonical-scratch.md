# Exact canonical scratch used by the circled-glyph helper

`LegacyNativeCanonicalScratch` ports the default Engine048190→04ba10/04b870→04aa10 scalar control flow using the independently audited portable Unicode data. This is the helper's **temporary scratch**. It does not replace ordinary glyph UTF8 or imply accepted text-source publication.

The native input has two decomposition branches. An original class0 code point resets the combining-run boundary and recursively appends canonical mappings through04ba10. A nonzero class code point uses04b870 with a **copy** of that boundary cursor; it scans existing scratch forward and chooses a stable insertion position. Recursive nonstarter components retain their own updated insertion hint. Copying the cursor matters: moving the original run boundary after each insertion would fail descending combining-class order. The exact comparison is `existingClass<=currentClass || existingClass==previousReadClass`, with previousReadClass initially256. Native gap class−1 is retained, including its unusual placement before a class0 starter.

The composition pass04aa10 remembers an actual starter and starts the blocking class at−1. It attempts a pair only when the current class is strictly larger than the last uncomposed class. A successful composition updates the starter and removes the consumed scalar without advancing that blocking class. A new uncomposed class0 starts another group. Canonical data and the two Hangul branches come from `LegacyNativeUnicodeNormalizationData`; no framework normalization or case conversion is used.

The checked UTF8 adapter also recovers the default codec: initializer094500 skips a leading UTF8 BOM only in scratch. Writer094960 replaces surrogate numeric values andFFFE/FFFF with the iterator's default1A code point. This differs from ordinary0491a0 passthrough, which preserves the original bytes when no circled replacement occurs. The byte adapter supports valid Unicode UTF8; the native extended malformed/5–6-byte codec and other charset callbacks remain separate. The scalar adapter consumes already-decoded native numeric0..10FFFF values and does not invent an input iterator/BOM removal.

Explicit portable budgets are65536 decoded inputs,262144 scratch scalars,1MiB encoded bytes,1048576 work visits and recursion depth32. Cancellation and exhaustion refuse the operation before publishing a result. These are independent implementation budgets, not native model thresholds.

The independent authoritative corpus is [Unicode9 NormalizationTest.txt](https://www.unicode.org/Public/9.0.0/ucd/NormalizationTest.txt), SHA256`2d48d848656b3cf889df59980ab13551988950c4ca8c5190a17c691a17f8b9bb`. The probe reads the official five-column normalization relations directly. All93610 projections match except **exactly two proven service-policy deviations**: isolated037E and0387 remain unchanged because native05ed00 explicitly declines their canonical decompositions. These deviations are reported and checked, not corrected to match the public golden output. They do not involve handwritten samples or label accuracy.

Reproduce with headless managed/native code:

```powershell
dotnet run --project tools/LegacyCanonicalScratch.Probe -c Release -- models/text-unicode/legacy-normalization-data.json D:/BlurMask-build-artifacts-20261011/unicode-normalization-next/NormalizationTest.txt
dotnet publish tools/LegacyCanonicalScratch.Probe -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/canonical-scratch-aot
& D:/BlurMask-build-artifacts-20261011/canonical-scratch-aot/LegacyCanonicalScratch.Probe.exe models/text-unicode/legacy-normalization-data.json D:/BlurMask-build-artifacts-20261011/unicode-normalization-next/NormalizationTest.txt
```

Additional original controls cover Russian composed letters, descending mark order, native gap class−1, surrogate/noncharacter1A substitution, Hangul blocking, BOM/CRLF byte policy, independent owned storage, input/output/work bounds and cancellation. A direct comparison with `LegacyNativeOrdinaryGlyphUtf8` ensures temporary normalized bytes are not silently written back to ordinary text. No GUI or vendor DLL is executed. Circled-marker mapping and writeback, accepted021000 string ownership/classification, source references and the real recognition winner remain separate stages.
