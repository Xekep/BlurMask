# Class 24a prepared-line wrapper and scalar resources

`LegacyAutomaticLineEstimator.Estimate` now combines the recovered sparse and
dense branches of Text `18016e0c0` for an explicit already prepared line. Its
baseline and upper guideline are double global intercepts; its shared slope is
separate. `Line` performs the float conversion used by downstream line records.
It does not select which strokes form a line or reproduce subsequent callback
adjustments and final recognized-text placement.

`LegacyAutomaticLineProfile.ReadResourceNode` reads the original uncompressed
class-24a version-102 node directly from a `.res` byte span at an explicit node
offset. It checks marker, node length, class, flags and version before reading
the 20-byte scalar payload. The exact native reader is `18016eb90`: integer field
at object `+8`, float `+C`, endpoint byte `+10`, slope bounds `+1C/+20`, sparse/dense
thresholds `+11/+12`, and a consumed reserved byte. These serialized slope bounds
are already tangents; the API does not apply degree-to-tangent conversion again.
The first two fields are retained for faithful inventory, and are not used by
the bounded `18016e0c0` stage. The reserved byte is also retained but has no effect,
matching the native reader's treatment.

The original Russian resource has nodes at 5789, 11421 and 187976; the English
resource at 8486, 16842 and 182230. English text profiles use slope bounds near
`±0.5773502588` and sparse/dense thresholds 10/50. Russian text profiles use bounds
near `±1.633123948e16` and thresholds 10/10. Each resource also has a zero-slope
10/10 node. Reading all three preserves their different native configurations;
the caller chooses the relevant node rather than guessing one language-wide
default.

When vertical-run count reaches the sparse threshold, the wrapper calls the
four-component Gaussian stage with the resource's endpoint-inclusion flag. If
sparse threshold is lower than dense threshold, its slope bounds expand linearly
from zero to their full values between those thresholds. The interpolation is
computed in float as in the original code. Slope clamping does not recenter or
recompute the fitted intercepts. Below 20 vertical runs, the clamped slope halves
and central guidelines blend with anchored float Y mean and standard deviation.
At 20 runs and above this additional blend is absent. The original generic
Gaussian defaults remain the separate `18000b300` configuration.

The implementation deliberately rejects unsupported compressed/header versions,
nonfinite or inverted bounds, zero sparse thresholds, descending thresholds and
oversized input. This is a bounded supported-resource API, not a general parser
for arbitrary vendor engine graphs.

## Validation

The independent controls read all six actual original Russian/English scalar
nodes. They use original synthetic strokes and an independently solved 5×5 ridge
system for dense fit, checking 9/10/11, 19/20/21 and 49/50/51 vertical runs. An
additional original control profile suppresses endpoints and has asymmetric
slope bounds. Managed and Windows x64 NativeAOT both pass 273 scalar-node reads,
72 sparse estimates, 201 dense estimates and 96 dense/sparse blends. Corrupt
version, truncation, nonfinite metadata, zero threshold and cancellation are
also checked. No original vendor DLL is executed.

```powershell
python tools/LegacyAutomaticLineEstimator.Probe/reference_controls.py .artifacts/automatic-line-controls.json
dotnet build tools/LegacyAutomaticLineEstimator.Probe/LegacyAutomaticLineEstimator.Probe.csproj -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/automatic-line-build
dotnet D:/BlurMask-build-artifacts-20261011/automatic-line-build/bin/LegacyAutomaticLineEstimator.Probe/release/LegacyAutomaticLineEstimator.Probe.dll .artifacts/automatic-line-controls.json
```

This completes the selected class's prepared-line sparse/dense computation.
Automatic raw stroke grouping, post-estimation callback passes and full font
placement remain separate boundaries; the completed scalar wrapper does not
establish complete recognition-engine equivalence.
