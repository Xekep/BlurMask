# Legacy Russian directional profile

The Russian acoustic resource also contains two 313-input networks (types `3f`
and `4f`). The checked configuration uses 17 global statistics, sixteen local
windows with sixteen channels, nine baseline channels, context and boundary
channels. Reusing the already restored 229-input raster vector cannot drive
these additional networks correctly.

The following arithmetic passes have been restored in reusable managed code:

- `LegacyDirectionalLandmarks` restores the checked `0b6d20` horizontal/vertical
  reversal flags and physical-stroke endpoint flags. It preserves the observed
  first-Y carry behavior rather than replacing it with generic local extrema.
  Pen-up paths are not inserted. Landmarks are not character cuts.
- `LegacyDirectionalStatistics` restores the checked `0b7100` seventeen global
  statistics, including normalized shape/center/arc length, dynamically scaled
  signed covariance and direction/intersection-class counts. It deliberately
  requires caller-prepared coordinates, native geometry dimensions, physical
  arc length, intersection classifications and aggregate intersection count.
  Those inputs are not inferred or filled with guessed zeros.
- `LegacyArcLengthWindows` restores `0b7440` temporal membership over cumulative
  physical distances, with rounded strict boundaries, explicit empty windows
  and the observed single-window advance on a large segment. Pen-up distances
  are excluded; unsafe overflow domains are rejected.
- `LegacyDirectionalCoordinatePreparation` restores the `0b4d10` power-of-two
  normalization selected by the largest physical segment and short extent cap.
  `LegacyDirectionalResampling` restores the constant physical step with
  exponent-10 residual/chord handling and the `0b4af0` prepared geometry. It
  preserves repeated rounded samples, original physical stroke boundaries and
  the final endpoint. This differs from the 229-input square-raster profile.
- `LegacyQuantizedSegmentIntersection` and `LegacyDirectionalIntersections`
  restore the fixed-point crossing helper and ordered spatial-grid flags,
  including physical-stroke crossings and nonoverlapping self-loop intervals.
  See [intersection arithmetic and controls](legacy-russian-intersections.md).
- `LegacyDirectionalWindowStatistics` and `LegacyDirectionalFeatureAssembly`
  restore the temporal local16 layout and calibrated geometry274 plus the nine
  explicit raw baseline, twenty-eight context and two boundary channels. Caller
  calibration arrays are required; they are not bundled or guessed.
  See [the temporal channel profile](legacy-russian-temporal-channels.md).

Own analytic tests and independent Python references pass in managed execution
and Windows Native AOT: 641 landmark cases, 512 global-statistic cases,
640 window cases, 655 coordinate/resampling cases and 4,736 crossing cases.
Another 1,152 controls check local statistics and calibrated channel assembly.
The references execute no vendor engine and include no vendor weights or ink.
The comparison confirms the restored arithmetic against the separate reference;
it does not measure Russian handwriting accuracy or demonstrate SDK parity.

Regenerate portable controls in a scratch directory and run:

```powershell
python tools/LegacyDirectional.Probe/reference_controls.py .artifacts/ru-directional.json
dotnet run --project tools/LegacyDirectional.Probe -c Release -- .artifacts/ru-directional.json
python tools/LegacyDirectionalStatistics.Probe/reference_controls.py .artifacts/ru-statistics.json
dotnet run --project tools/LegacyDirectionalStatistics.Probe -c Release -- .artifacts/ru-statistics.json
python tools/LegacyArcLengthWindows.Probe/reference_controls.py .artifacts/ru-windows.json
dotnet run --project tools/LegacyArcLengthWindows.Probe -c Release -- .artifacts/ru-windows.json
python tools/LegacyDirectionalCoordinates.Probe/reference_controls.py .artifacts/ru-coordinates.json
dotnet run --project tools/LegacyDirectionalCoordinates.Probe -c Release -- .artifacts/ru-coordinates.json
```

The [spatial 4f layout](legacy-russian-spatial-model.md),
[explicit-line context](legacy-russian-context-windows.md) and
[mode1 acoustic aggregation](legacy-russian-acoustic-ensemble.md) now have
separate checked components. Automatic line/baseline profiles, the remaining
native lexical/case handling, candidate character cuts and
representative joined-word controls remain unfinished. These passes are not
enabled in AI-pen UI and do not make joined Russian cursive recognition ready.
