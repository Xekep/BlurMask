# Original Math single-stroke temporal inputs

`LegacyMathAkSourceGeometry.PrepareUnguidedSingleStrokeTemporal` prepares the
actual base-tail1F input:222 shorts, including30 resampled points, temporal
directions/turns, global fields, three absent-guide zeros and two whole-source
boundary fields. It admits one nondegenerate whole stroke of2..4096 points.
Multistroke, guided and degenerate branches remain separate.

The same geometry owns the412-channel minimum/maximum calibration tables read
from its authenticated Math AK snapshot. This adds1648 retained payload bytes
per geometry. The result carries private source/issuer identity and returns
detached feature/sample copies. It has no predictions or candidate authority.

The arithmetic follows the selected02ED20/02F3D0/02FB40/030020 prefix and0327A0
calibration. In particular, native05EAC0 starts the large-input downshift at2.
Its16384-byte lookup table equals rounded square roots for every entry, but its
downshift policy differs from a generic integer norm (32000 maps to180).
Spatial raster work is omitted only for this successful temporal prefix;
native allocation/failure-prefix equivalence is not claimed.

Static PE checks pin13 complete function regions and the entire norm table.
Supported grid coordinates stay within0..127; bounded source length, resampling
and interpolation keep the stated intermediate integer products below I32.
These are arithmetic bounds, not latency or process-memory guarantees.

Seven synthetic source geometries produce1554 checked features. The independent
Python reference derives normalization, all sample coordinates and all features
from original points and raw calibration bytes. Seven guards, five norm controls
and nine corrupted-output controls cover ownership, supported input and the
distinguishing norm policy. The earlier geometry capture remains byte-exact.

```sh
dotnet run --project tools/LegacyMathAkTemporal.Probe -c Release -- . capture.json
python -I -B tools/LegacyMathAkTemporal.Probe/audit_output.py . capture.json report.json
```

These CI controls execute no neural models. The fixed1F network, other feature
families, unified259-label costs and grammar matching still need their own
source-bound implementation and verification. The actual grammar caller's
guide selection is not established by this explicit unguided API. See the
[remaining recognition chain](legacy-math-recognition-frontier.md).
