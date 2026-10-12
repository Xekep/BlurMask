# Portable character-line category model

`models/line-geometry/character-line-categories.json` contains the complete
statically inspected **3923** exact symbol entries and **45** vertical profiles.
It includes **122** Cyrillic entries. `LegacyCharacterLineCategoryModel.Load`
reads this stable version1 JSON without a MyScript DLL, SDK, Python or private
analysis files. The source/profile data has MyScript provenance and is separate
from the repository's MIT implementation code.

The model has source PE SHA-256
`17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f`
and complete ordered-table SHA-256
`066c999f557389e2c286a8d88f40d5d3bf4e76f00abc7e63fa43823a187211fc`.
The ordinary resource manifest additionally fingerprints the complete JSON file,
including provenance and format metadata.

The table digest is independent of JSON whitespace/property order. Its exact
byte input is uint32LE entry count, followed by each entry's uint32LE UTF8 byte
length, strict UTF8 text and two signed category bytes; then uint32LE profile
count followed by all three binary32LE floats in each profile row. Entry order
is retained from the source catalog. The third profile float is reserved and
must be positive zero. Changing even one actual symbol, category or profile
changes the checked digest.

The loader checks format/version, source and table pins, exact counts, duplicate
JSON properties, duplicate symbol keys, strict text, category ranges, finite
profile values, Cyrillic cardinality and the computed digest. Reading is bounded
to 1MiB with short-read and cancellation support. The caller retains stream
ownership. The returned catalog and alphabet bindings snapshot their input data.

```csharp
using var stream = File.OpenRead("character-line-categories.json");
var model = LegacyCharacterLineCategoryModel.Load(stream);
var categories = model.Catalog.BindAlphabet(actualResourceAlphabet);
var producer = categories.CreateItemProducer();
var item = producer.PrepareNextItem(actualAlphabetLabel,
    sourceLeft, sourceRight, sourceTop, sourceBottom);
// Prepare each source item sequentially, then call LegacyPreparedCharacterLineFit.Fit.
```

Do not substitute a whole connected-word bbox for each symbol's source geometry.
Known symbol categories are prepared metadata, not recognition results invented
by this loader. The producer preserves the native retained-template semantics
for missing symbols; see [category/item bridge](legacy-prepared-character-line-categories.md).
The [C1A iterative fit](legacy-prepared-character-line-fit.md) and existing
font-size policy can consume its actual prepared results, but raw character
segmentation and the application connected-word integration remain unfinished.

The standalone probe loads only the committed model and checks **256** mixed
Latin/Russian prepared geometry cases through catalog, item producer, C1A fit
and font policy. Body/uppercase/descender bounds yield the shared line height,
rather than the combined tallest box. It also verifies model corruption, source
and payload pins, duplicate fields/keys, range errors, negative-zero reserved
values, bounded/short reads, stream ownership and cancellation in managed and
Windows NativeAOT builds. These are prepared value controls, not OCR accuracy.

```powershell
dotnet run --project tools/LegacyCharacterLineCategoryModel.Probe -c Release -- models/line-geometry/character-line-categories.json
dotnet publish tools/LegacyCharacterLineCategoryModel.Probe -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/character-line-category-model-aot
D:/BlurMask-build-artifacts-20261011/character-line-category-model-aot/LegacyCharacterLineCategoryModel.Probe.exe models/line-geometry/character-line-categories.json
python tools/verify-model-resources.py
```

For reproducible **static preparation only**, the optional exporter reads the
SHA-pinned original PE and writes the same JSON; it never loads vendor code:

```powershell
python tools/handwriting/export_character_line_categories.py research/myscript/sdk/build/x64/MyScriptDocument.dll models/line-geometry/character-line-categories.json
```

Installing the asset into every application package is a separate build/runtime
integration step; committing a recognition resource does not automatically
enable its profile in installers.
