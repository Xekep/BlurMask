# Recognition resources

Original MyScript resources used by the recovered recognition modules are kept
here, with byte sizes and SHA-256 fingerprints in [manifest.json](manifest.json).
These model resources have MyScript provenance and are separate from the
repository's MIT-licensed implementation code.

The catalog contains 16 source `.res` files, including the two line classifiers
extracted from the inspected engine (67,689,808 bytes in total), plus the separate Russian engine data
profile, a separate exact separation-cost table, and the portable character-line
category/profile catalog. Keeping a resource here does not
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
| [line-geometry/line-2e3-30.res](line-geometry/line-2e3-30.res) | Embedded42-input/affine100/BLSTM30 line grouping classifier |
| [line-geometry/line-2e3-50.res](line-geometry/line-2e3-50.res) | Embedded42-input/affine100/BLSTM50 line grouping classifier |
| [line-geometry/character-line-categories.json](line-geometry/character-line-categories.json) | Portable3923-symbol/45-profile table for prepared character-line x-height fitting; no vendor DLL needed |
| [ru-RU/ru_RU-ak-cur.lite.res](ru-RU/ru_RU-ak-cur.lite.res) | Russian neural classifiers and acoustic alphabet |
| [ru-RU/ru_RU-lk-text.lite.res](ru-RU/ru_RU-lk-text.lite.res) | Russian packed word dictionary and language model |
| [ru-RU/legacy-engine-profile.json](ru-RU/legacy-engine-profile.json) | Matching static activations, geometry ranges and acoustic confidence calibration |
| [ru-RU/legacy-separation-profile.json](ru-RU/legacy-separation-profile.json) | Matching 256-value table for costs between grouped handwriting components |
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

`LegacyCharacterLineCategoryModel.Load` reads the committed character-line catalog
directly and verifies its source and full ordered-table SHA-256. It can be reused
with the prepared C1A fitter without a private vendor PE or research directory.
It requires actual per-symbol geometry and recognized alphabet IDs; it does not
recognize raw connected handwriting or replace whole-word bounds automatically.
See [portable character-line catalog](../docs/legacy-character-line-category-model.md).

Raw Content2 partitions ink into regions; it does not decode Russian text or
solve equations. Export its checked profile into the application's model pack:

```sh
python tools/export-raw-content-model.py models/raw-content/raw-content2.res .artifacts/raw-content.ikrraw
BlurMask --install-raw-content-model .artifacts/raw-content.ikrraw
```

Russian recognition remains experimental. `LegacyRussianModelBundle` loads the
two Russian resources and matching engine profile without MyScript DLLs or
private research files. Its reusable word path still requires an explicit source
baseline; full connected cursive candidate generation and native search are unfinished.
Known-word native lexical costs are available with explicit profiles and flags.
See [portable Russian model loading](../docs/legacy-russian-model-bundle.md).
See [the Russian word diagnostic](../docs/legacy-russian-directional-words.md) and
[Raw Content installation](../docs/raw-content-engine.md).

[text-unicode/legacy-normalization-data.json](text-unicode/legacy-normalization-data.json)
is the portable default Unicode9 scalar table for native combining classes,
canonical decomposition and composition. It is fingerprinted in the model
manifest and read without a vendor DLL. Full stream ordering, marker handling
and accepted text publication remain separate. See
[normalization data](../docs/legacy-native-unicode-normalization-data.md).

[text-unicode/legacy-case-data.json](text-unicode/legacy-case-data.json) stores
fingerprinted native scalar case alternatives and opaque contextual flags.
Unicode9 is a comparison reference; native case behavior has retained
reference differences. The independent loader and alphabet predicate require
no vendor DLL. Full case292 query integration remains unfinished. See
[case alternatives](../docs/legacy-native-case-alternatives.md).
