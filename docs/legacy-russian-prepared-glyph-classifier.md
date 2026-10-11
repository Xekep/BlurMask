# Composed prepared-glyph classifier

`LegacyPreparedGlyphClassifier` connects the checked components:

1. Caller-ordered prepared float ranges and explicit source line frame.
2. `LegacyPreparedGlyphNormalization` FF/FF scaling followed by truncation.
3. Four-family `LegacyDirectionalGlyphClassifier`, with explicit context,
   clipped boundaries and whether baseline families are included.
4. `LegacyDynamicAcousticWeights` on the selected **float source** ranges,
   using `LegacyDirectionalAcousticWeightConfiguration` from type21d.
5. Resolved native mode0/1/2 weights, explicit enabled slots/classes and
   cost multiplier, producing the integer acoustic channel.

The result retains prepared range provenance, normalized short strokes/baseline,
raw probability scores, geometric weights, resolved family weights and integer
costs. This endpoint requires the checked four-family second-network shape.
It does not generate cuts, determine line frame/order, manufacture28 context
features, choose a lexical category or perform word search. Instance enabled
slots and multiplier remain explicit. A caller must supply matching classifier
and resource configuration; the portable Russian bundle validates resource
identity independently.

Native geometry normalization may need chronological sorting and pen-up merge;
the bounded normalizer's explicit pre-merge range threshold remains in force.
The integer acoustic mixture is separate from calibrated confidence. The result
can feed `LegacyPreparedSequenceWordSearch` with its original scores and resolved
weights, but that endpoint remains an original bounded fixed-sequence beam.

```powershell
dotnet run --project tools/LegacyPreparedGlyph.Probe -c Release
dotnet run --project tools/LegacyPreparedGlyph.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
```

The vendor-free own fixture combines fractional insertion with exact normalized
points, four own uniform NN outputs, mixed stored weight modes, independent
integer contribution rounding, enabled class masks, explicit inactive baseline
slots, provenance and cancellation. Its uniform acoustic cost8993 deliberately
differs from the unweighted cost8995 because contributions are converted before
weight normalization.

Actual-resource diagnostics run own isolated printed М,И,Р,А shapes with an
explicit baseline90/height80, target100, neutral context and enabled first four
families. All16 source scale/translation variants must preserve exact integer
cost vectors. The original vectors rank Latin m/p ahead of Cyrillic м/р for two
similar shapes; И still competes with n/н/u. Cyrillic а is first for the own А
shape. This is an honest diagnostic result, not a representative quality test
or proof that joined Russian handwriting is ready. Native context and variable
cut/word search must be composed and tested before enabling this path in AI-перо.
