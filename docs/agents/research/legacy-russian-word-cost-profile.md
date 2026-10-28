# Explicit legacy word-cost profiles

`LegacyWordLanguageCostProfile` reads the four scalar profiles in the validated
type2d1/base100/order2/flags3 language object. The caller explicitly selects a
profile and a finite positive runtime scale. This component converts stored
levels into integer costs; it does not select a recognition mode, apply word
history/category rules, or add acoustic and language costs.

Static evidence is the reader `1800fcc10` and cost preparation `1800fe2a0`.
The latter selects a profile with `0x137 - externalMode`; the meaning of that
external mode remains unverified. The reader stores four model-major arrays of
two class offsets at field `_f0`, four word offsets at `_110`, four boundary
offsets at `_120`, and four two-float class slopes at `_130`. Word conversion
uses the order-zero class slope and its separate word offset. The checked
object ends with these 128 calibration bytes followed by 9,498 punctuation
bytes. `LoadProfiles` first validates the complete object using
`LegacyWordLanguageModel`, allocating temporary raw-table validation storage.
Load once and reuse the resulting four small profile objects.

The positive-runtime-scale scalar branch uses this operation order, with a
binary32 rounding at each arithmetic step:

```
scaledSlope = runtimeScale * slope
scaledOffset = runtimeScale * offset
level = (float)storedLevel * (1 / 8192)
weighted = level * scaledSlope
centered = weighted - scaledOffset
cost = truncateToInt32(centered * 8192)
```

The API rejects nonfinite coefficients, nonpositive/nonfinite runtime scales
and costs outside the portable signed-int32 conversion range. Signed slopes
are accepted: this is scalar cost conversion, not confidence calibration.
The native nonpositive-scale branch and alternate-vocabulary paths are
unsupported. Boundary offsets, the additional calibration arrays and
punctuation/history state are not silently folded into this calculation.

For the checked resource under [`models/ru-RU`](../../../models/ru-RU), class offsets
are zero. Both class slopes are `.4`, `.22`, `.22`, `.3` for profiles zero
through three; word offsets are `4`, `1.375`, `1.375`, `1.3`. With the explicit
diagnostic scale of one, the word/unigram costs are:

| Word | Profile 0 | Profile 1 | Profile 2 | Profile 3 |
| --- | --- | --- | --- | --- |
| мир | -28827 / 24788 | -9096 / 13633 | -9096 / 13633 | -7694 / 18591 |
| миф | -20510 / 25424 | -4522 / 13983 | -4522 / 13983 | -1456 / 19068 |

These are separate cost fields, not a demonstrated total score or a reranking
policy. They do not establish that the recognizer now selects `мир` correctly.
Native external mode selection and acoustic-cost combination still need proof.

The vendor-free probe constructs its own complete stored model with distinct
values in every profile field. It checks field layout, positive scalar
conversion, signed truncation, cancellation, nonfinite values and overflow.
An independent Python reference generator rounds every arithmetic operation
using `struct.pack/unpack` binary32 and includes large signed inputs and 768
deterministic random cases (780 total). Managed .NET and Windows Native AOT
pass all controls and produce identical eight actual diagnostic rows.

```powershell
python tools/LegacyWordProfile.Probe/reference_controls.py .artifacts/own-word-profile-controls.json
dotnet run --project tools/LegacyWordProfile.Probe -c Release -- --controls .artifacts/own-word-profile-controls.json
dotnet run --project tools/LegacyWordProfile.Probe -c Release -- models/ru-RU/ru_RU-lk-text.lite.res
```

The component remains headless and is not connected to the application's AI
pen. Connected-stroke cuts, complete history costs and representative Russian
cursive accuracy remain open.
