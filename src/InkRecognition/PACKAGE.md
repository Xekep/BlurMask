# BlurMask.InkRecognition

Reusable CPU ink recognition for .NET 10 and Native AOT, independent of the
BlurMask desktop UI. No MyScript SDK DLL is loaded or executed.

The package includes the portable BLSTM executor and experimental recovered
legacy model readers, glyph feature composers, integer acoustic costs,
dictionary/language scoring, prepared cursive atoms and line geometry.
Math and Raw Content adapters are separate packages in the same release.

Model data is supplied separately. The release's `InkRecognition-models.zip`
contains the repository model catalog, its SHA-256 manifest and engine profile.
The MIT license applies to implementation code; the model assets retain their
documented MyScript provenance.

```csharp
using InkRecognition;

var bundle = LegacyRussianModelBundle.Load(
    File.ReadAllBytes("models/ru-RU/ru_RU-ak-cur.lite.res"),
    File.ReadAllBytes("models/ru-RU/ru_RU-lk-text.lite.res"),
    File.ReadAllBytes("models/ru-RU/legacy-engine-profile.json"));
var glyphClassifier = bundle.CreateGlyphClassifier();
// Supply prepared glyph ink, baseline and explicit context/boundary flags.
// Language profile and runtime scale remain explicit caller parameters.
var language = bundle.CreateResourceLanguageScorer(runtimeScale: 1);
var words = bundle.CreateIntegerWordDecoder(runtimeScale: 1);
// words.Decode accepts caller-prepared integer glyph choices plus explicit
// history/category flags, additional path costs, heap budget and beam width.
```

Russian connected handwriting is still experimental. Automatic continuous cuts,
complete group search, native context/default policies and a validated end-to-end
GUI recognizer remain unfinished. Prepared geometry APIs do not infer missing
context or promise recognition accuracy.

Source and detailed evidence: https://github.com/Xekep/BlurMask/tree/main/src/InkRecognition
