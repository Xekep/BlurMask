# Legacy spatial window membership

`LegacySpatialWindows` restores inspected physical-sample membership from
`1800b7ce0` (uniform exponent-10 extents) and `1800b7840` (coordinate quantiles).
It supplies spatial cells for the alternative directional family; it does not
yet supply its16 local statistics, transformed/calibrated input or ensemble.
These memberships are not handwriting character boundaries.

Cells are emitted in row-major Y/X order and retain the original sample-index
order inside each cell. Each sample occurs once. Uniform membership first
divides the fixed extent by the number of bins, then advances equal intervals;
flooring each successive ideal boundary is different. Quantile membership
sorts each coordinate axis independently, takes indices `floor(k*N/binCount)`,
and uses the final sorted coordinate as the last upper bound. Bounds are lower
inclusive and upper exclusive. All unmatched coordinates use the final bin,
including the quantile maximum, a constant axis, negative coordinates and
outside extents. Duplicate quantile bounds can leave empty cells.

The implementation accepts1..255 caller-prepared points, positive dimensions
up to65535, and1..16 rows/columns. The sample cap preserves the native byte
membership/count domain and prevents its overflowing256-point bucket case.
Integer operations stay within the checked32-bit domain. Cancellation is
checked before work and per assigned point. No resource or vendor sample is
included, and no SDK runtime is executed.

Managed and Windows Native AOT pass640 independent own-point controls plus
analytic row-major, boundary, repeated-axis, negative/outside fallback,
malformed-domain and cancellation tests. The uniform reference uses quotient
classification; the quantile reference enumerates matching coordinate intervals.
This confirms the restored arithmetic against an independent implementation,
not recognition accuracy or full SDK parity.

Native `1800b9380` selects uniform spatial membership when feature-mask bit12
is present and quantile membership for bit13 when bit12 is absent. The inspected
RU directional profile uses feature-mask0x1111 and a4×4 first spatial grid;
the4f serializer `1800ba060` reads this first spatial family. Its local helper
`1800b82c0`, channel transform/calibration and score combination still need
separate restoration before driving the real4f network.

```powershell
python tools/LegacySpatialWindows.Probe/reference_controls.py .artifacts/ru-spatial.json
dotnet run --project tools/LegacySpatialWindows.Probe -c Release -- .artifacts/ru-spatial.json
```
