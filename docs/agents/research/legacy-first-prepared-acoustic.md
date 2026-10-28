# Prepared first-model glyphs

`LegacyFirstPreparedAcousticClassifier` composes the owned current row source
with the [typed first acoustic model](legacy-first-acoustic-model.md). Its source
reader additionally requires the ordinary model's normalization flagsFF/FF.
The actual first model stores target0; native reader1c2940 replaces0 with100.
This permits reuse of the restored1b4e40 FF/FF normalization, including float
scaling before short truncation. It is not permission to use a baseline-family
256/238 input in the first247/229 networks.

The checked path is:

1. Already-selected row →24a at11421 →deskew →265 at11457 with200-factor keys.
2. Owned297 prepared cut points and composed raw-point chronology.
3. Caller glyph ranges, sorted by exact chronology including equal-key swaps.
4. Candidate minimumX and native fitted source baseline/height →FF/FF short ink.
5. Native normalized pen-up reduction, with an **explicit caller threshold**.
6. Trajectory217/raster199 prefixes →caller context28 →caller signed boundaries2.
7. First247/229 networks →integer probabilities →fresh mode2 weights1/1.

Native1b75b0 fills each feature prefix and appends the context when slot flags
e0 and the two signed boundary channels when e1 are set. The validated first
profile has both flags set for its active trajectory/raster slots. Native1b7f70
produces boundary signs; callers may use the existing separate boundary policy
with confirmed suppression flags. This endpoint does not choose those flags or
substitute a whole-line context policy for the native context producer.

The result retains normalized geometry, merged geometry, probabilities and
integer acoustic costs. It does not fill native final class corrections/pairs or
perform lexical decoding. Row/cut ownership is checked before evaluation; a cut
result from another prepared row is rejected even if the shapes match.

## Checks

```powershell
dotnet run --project tools/LegacyFirstPreparedAcoustic.Probe -c Release
dotnet run --project tools/LegacyFirstPreparedAcoustic.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json
```

Both Managed and Windows Native AOT pass. The vendor-free mode uses own zero
network fixtures and confirms equal quantized probabilities independent of ink.
Both modes evaluate own printedM/I/R, including a three-physical-strokeI, across
scale1/1.5 and translation. Full cost vectors are identical under those transforms.
Foreign cuts, incorrect context width/boundary signs/range threshold, and
cancellation are rejected.

With explicit neutral context, the actual model's lowest costs are:

| Own glyph | Network label | Integer cost |
| --- | --- | --- |
|M|22|5200|
|I|179|11812|
|R|25|11636|

These are raw acoustic output labels, not a recognized Russian word or an
accuracy assertion. In particular, they must not be fed to the native word beam
as final cached costs. The next missing producer is first-pass context selection
and native final-cache/search orchestration; representative joined Russian
handwriting and native winner/confidence remain unverified.
