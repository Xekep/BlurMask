# Legacy directional baseline statistics

`LegacyDirectionalBaselineStatistics` restores the nine raw integer statistics
in the checked `0b9380` temporal directional profile. They are subsequently
transformed into columns 274–282 by `LegacyDirectionalFeatureAssembly`.

The raw channels are width, height and physical arc length divided by baseline
height; mean point Y relative to baseline Y; baseline top and geometry bottom
relative to that height; and the fractions of points above, within and below
the inclusive baseline band. These fractions use the prepared point count.
The implementation preserves signed integer truncation and checked native
numerator overflow, rather than substituting floating point ratios.

`Extract` accepts prepared samples and an explicit positive-height baseline in
source coordinates. It applies the same origin removal and power-of-two scale
as the restored coordinate preparation, including the negative-scale zero-height
guard. It rejects a zero source sampling step: the native degenerate raw-path
branch leaves baseline coordinates unchanged, and that branch cannot be
reliably inferred from the existing prepared-sample object. `ExtractPrepared`
instead accepts explicitly prepared baseline coordinates for that case.

No baseline estimator, unavailable-baseline convention, model, context values
or cursive segmentation are supplied by this API. The older raster family's
special unavailable-baseline behavior is not assumed to apply to this profile.

Managed and Windows Native AOT controls pass against 640 independent Python
integer-reference cases, including overflowing source numerators and extreme
caller baseline coordinates. Own analytic controls also cover source mapping,
translation invariance, negative scaling, inclusive point bands, unavailable
and ambiguous-baseline rejection, and cancellation. These are numerical
controls, not a handwriting accuracy benchmark.

```powershell
python tools/LegacyDirectionalBaseline.Probe/reference_controls.py .artifacts/ru-baseline.json
dotnet run --project tools/LegacyDirectionalBaseline.Probe -c Release -- .artifacts/ru-baseline.json
```
