# Recognition resources

Original MyScript resources used by the recovered recognition modules are kept
here, with byte sizes and SHA-256 fingerprints in [manifest.json](manifest.json).
These model resources have MyScript provenance and are separate from the
repository's MIT-licensed implementation code.

The catalog contains all 14 `.res` files currently available in this project's
research assets (67,442,664 bytes in total), plus the separate Russian engine data
profile. Keeping a resource here does not
automatically enable its profile in the application or include it in installers.

| File | Role |
| --- | --- |
| [en-US/en_US-ak-cur.res](en-US/en_US-ak-cur.res) | English handwriting acoustic model; source for the current `.ikrpack` exporter |
| [en-US/en_US-ak-cur.lite.res](en-US/en_US-ak-cur.lite.res) | English lightweight handwriting acoustic model |
| [en-US/en_US-ak-superimposed.res](en-US/en_US-ak-superimposed.res) | English superimposed handwriting acoustic model |
| [en-US/en_US-ak-superimposed.lite.res](en-US/en_US-ak-superimposed.lite.res) | English lightweight superimposed handwriting model |
| [en-US/en_US-lk-text.res](en-US/en_US-lk-text.res) | English dictionary and language resource |
| [en-US/en_US-lk-text.lite.res](en-US/en_US-lk-text.lite.res) | English lightweight dictionary and language resource |
| [math/math-ak.res](math/math-ak.res) | Earlier Math acoustic resource |
| [math/math-grm-standard.res](math/math-grm-standard.res) | Earlier Math standard grammar |
| [math2/math-sr.res](math2/math-sr.res) | Original Math2 resource; source for the current `.ikrmath` exporter |
| [math2/math2-unwrapped.res](math2/math2-unwrapped.res) | Examined, unpacked copy of the same Math2 resource |
| [ru-RU/ru_RU-ak-cur.lite.res](ru-RU/ru_RU-ak-cur.lite.res) | Russian neural classifiers and acoustic alphabet |
| [ru-RU/ru_RU-lk-text.lite.res](ru-RU/ru_RU-lk-text.lite.res) | Russian packed word dictionary and language model |
| [ru-RU/legacy-engine-profile.json](ru-RU/legacy-engine-profile.json) | Matching static activations, geometry ranges and acoustic confidence calibration |
| [raw-content/raw-content.res](raw-content/raw-content.res) | Earlier Raw Content classification resource |
| [raw-content/raw-content2.res](raw-content/raw-content2.res) | Raw Content2 text/math/drawing classification graph |

`math2-unwrapped.res` is a different container representation, not a separately
trained model. The exporter accepts the original `math-sr.res` directly.

Prepare English and formula packs from the repository resources:

```sh
python tools/export-ink-model.py --resource models/en-US/en_US-ak-cur.res --output .artifacts/en-US.ikrpack
python tools/export-ink-math-model.py models/math2/math-sr.res --output .artifacts/math2.ikrmath
```

Check the resource catalog with `python tools/verify-model-resources.py`.

Raw Content2 partitions ink into regions; it does not decode Russian text or
solve equations. Export its checked profile into the application's model pack:

```sh
python tools/export-raw-content-model.py models/raw-content/raw-content2.res .artifacts/raw-content.ikrraw
BlurMask --install-raw-content-model .artifacts/raw-content.ikrraw
```

Russian recognition remains experimental. `LegacyRussianModelBundle` loads the
two Russian resources and matching engine profile without MyScript DLLs or
private research files. Its reusable word path still requires an explicit source
baseline; connected cursive segmentation and native lexical costs are unfinished.
See [portable Russian model loading](../docs/legacy-russian-model-bundle.md).
See [the Russian word diagnostic](../docs/legacy-russian-directional-words.md) and
[Raw Content installation](../docs/raw-content-engine.md).
