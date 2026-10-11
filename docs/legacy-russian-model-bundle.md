# Portable Russian model data

`LegacyRussianModelBundle.Load(acousticBytes, lexicalBytes, engineProfileBytes)`
loads the checked Russian lite pair together with its separate engine data
profile. The library retains copied parameters, four directional acoustic
networks, the initial context network, both alphabets, the packed dictionary,
terminal word IDs and stored language model profiles. No vendor DLL is loaded.

The resource pair is checked by exact size and SHA-256 before parsing. The JSON
profile is bounded to128KiB and depth8; dimensions, finite coefficients, geometry
intervals and activation shape/ranges are validated. Cancellation is checked
before parsing and between expensive phases. Retained data is independent of the
caller buffers. Only this inspected pair is supported; this is not a generic
reader for every MyScript resource version.

All input data now lives in `models/ru-RU`. `legacy-engine-profile.json` contains
176 integer activation entries, temporal/spatial274-column ranges, and four
180-class center/slope arrays. The static activation and geometry tables are
absent from the `.res` resource and came from the inspected local engine image;
the confidence coefficients came from the acoustic resource itself. Their
provenance is separate from the MIT implementation. Original data fingerprints
are in the model manifest.

The exporter reproduces the profile from the exact local resource/engine pair:

```sh
python tools/export-legacy-russian-profile.py --acoustic models/ru-RU/ru_RU-ak-cur.lite.res --lexical models/ru-RU/ru_RU-lk-text.lite.res --engine-pe /local/MyScriptText.dll --output .artifacts/legacy-engine-profile.json
dotnet run --project tools/LegacyRussianBundle.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
```

The exporter only reads files and does not execute or activate the vendor engine.
The runtime consumes the prepared data directly and does not need this exporter,
Python or the source engine image.

`CreateWordRecognizer(relatedFormMode:0)` constructs the existing experimental
separated-word pipeline with an explicit original alias policy. Its diagnostic
checks sixteen own printed-word/profile combinations after overwriting all
caller buffers. Stored `мир` terminal198893, class435 and wordcost9851 are checked
against independent previous enumeration. Malformed profiles and corrupted
resources are rejected. Managed and Windows Native AOT match the previous
separate-file diagnostics: four-family `мама` rank1, `мир` rank2 behind `миф`.

This loader does not infer source baseline, generate cuts inside connected
strokes, combine the complete native language search or preserve written case.
It does not enable Russian AI pen in the application. Those are separate engine
dependencies; loading data successfully is not handwriting quality evidence.
