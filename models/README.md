# Recognition resources

Original MyScript resources used by the recovered recognition modules are kept
here, with byte sizes and SHA-256 fingerprints in [manifest.json](manifest.json).
These model resources have MyScript provenance and are separate from the
repository's MIT-licensed implementation code.

| File | Role |
| --- | --- |
| [ru-RU/ru_RU-ak-cur.lite.res](ru-RU/ru_RU-ak-cur.lite.res) | Russian neural classifiers and acoustic alphabet |
| [ru-RU/ru_RU-lk-text.lite.res](ru-RU/ru_RU-lk-text.lite.res) | Russian packed word dictionary and language model |
| [raw-content/raw-content2.res](raw-content/raw-content2.res) | Raw Content2 text/math/drawing classification graph |

Raw Content2 partitions ink into regions; it does not decode Russian text or
solve equations. Export its checked profile into the application's model pack:

```sh
python tools/export-raw-content-model.py models/raw-content/raw-content2.res .artifacts/raw-content.ikrraw
BlurMask --install-raw-content-model .artifacts/raw-content.ikrraw
```

Russian recognition remains experimental. Its reusable word path currently
requires the matching activation/calibration configuration and an explicit source
baseline; connected cursive segmentation and native lexical costs are unfinished.
See [the Russian word diagnostic](../docs/legacy-russian-directional-words.md) and
[Raw Content installation](../docs/raw-content-engine.md).
