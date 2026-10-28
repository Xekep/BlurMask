# Class 24a postcallback line geometry

Two more explicit prepared-line stages are reconstructed:

* `LegacyLineGuidelineAdjustment.Compute`: Text `180169750`, the class219
  interface method at offset `0x88` invoked by `18016e850`.
* `LegacyLineDeskew.Apply`: Text `18016e570`, called immediately afterwards on
  the selected group's prepared stroke range.

Class219 descriptor `1802df150` references its interface table `1802df0a0`.
Table slot 17 (`+0x88`) is `180169750`. Static disassembly confirms user guideline
baseline/pitch loads are **float** `movss`; the decompiler's `int*` cast and
integer-to-float-looking expressions in this helper are misleading.

## What the four fitted levels become

The native class24a callback `18016e850` resets the generic Gaussian defaults
with `18000b300(param2+0x28)`, estimates the line, and creates a 40-byte record:

| Record offset | Exact source |
|---|---|
| `+0` | Float conversion of central Gaussian guideline 1 / sparse baseline |
| `+4` | Float conversion of guideline 1 minus guideline 2, or caller-provided scale when secondary `+0x168` flag is set |
| `+8` | Float conversion of fitted/clamped/blended shared slope |
| `+0xC` | Copy of the original `+0` value |
| `+0x10` | Copy of the original `+4` value |
| `+0x14`, `+0x18` | Alignment metadata computed by interface `+0x88` when enabled |
| `+0x20`, `+0x24` | Guideline/row identifiers, initialized to `-1` / `int.MinValue` |

Outer Gaussian levels 0 and 3 are not stored as this record's baseline or height.
The height is the **central guideline band**, rather than the full ink bounding
box. Calling it the final font point size or claiming a learned text-placement
network would go beyond this evidence. Conversion from this recognition geometry
to final rendered font metrics remains a separate boundary.

## Guideline metadata

The interface helper preserves the record's actual baseline, height and slope.
It first resets guideline/row identifiers. If the explicit metadata-enable flag
is false, it returns. Otherwise it evaluates the original baseline at minimum
and maximum X of the prepared points. With no user guidelines its alignment
baseline is the mean of those two values. With a one-row first user guideline it
uses that guideline's baseline and row zero. With a multiple-row first guideline
it uses the caller's row, or infers one by round-away-from-zero of the baseline
at minimum X relative to the guideline baseline/pitch. This is intentionally not
the minimum projected Y for negative slopes. If the selected guideline lies
below the midpoint of the two evaluated baselines, alignment height increases
by the difference.

The API takes that native enable flag and first user guideline explicitly. It
does not infer application configuration or substitute a whole-ink bounding box
for missing user guidelines. Alignment metadata uses the preserved original
height, and does not replace the Gaussian band's stored height.

## Deskew

The native helper constructs cosine from `sqrt(double(float(slope*slope))+1)`
and signed sine from `sqrt(double(float(1-cosine*cosine)))`. It rotates around
the midpoint of the X extent and the fitted baseline at that X. Prepared points
and the record's actual baseline/height are transformed in float; the separately
stored slope remains unchanged. Thus the preserved original fields and metadata
are distinct from the transformed main fields.

When transformed height is within native relative/absolute float tolerance of
zero, the helper uses `width*0.15f`, floors height at `0.1f` and shifts the baseline
up by half of the **unfloored** fallback. This exceptional native fallback is
ported literally; it is not a guessed general font-size rule. The public API
returns transformed copies of the prepared strokes instead of mutating caller
arrays in place.

## Validation and remaining boundaries

Managed and Windows x64 NativeAOT pass 189 own affine/guideline cases and 3189
transformed points. Controls include zero/tiny/positive/negative/large slope,
zero/tiny/negative input height, one/multiple/no user guideline, absent or
explicit row, half-integer row ties, finite/bounds checks, cancellation and input
ownership. The Python oracle also independently composes homogeneous affine
matrices to verify the scalar point transforms. Neither controls nor runtime
execute the original vendor DLL.

```powershell
python tools/LegacyLinePostprocessing.Probe/reference_controls.py .artifacts/line-postprocessing-controls.json
dotnet build tools/LegacyLinePostprocessing.Probe/LegacyLinePostprocessing.Probe.csproj -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/line-postprocessing-build
dotnet D:/BlurMask-build-artifacts-20261011/line-postprocessing-build/bin/LegacyLinePostprocessing.Probe/release/LegacyLinePostprocessing.Probe.dll .artifacts/line-postprocessing-controls.json
```

The class24a path is now reconstructed for an explicitly selected prepared line,
including the concrete metadata/deskew postcallback. Native candidate grouping,
recognition-result geometry adjustment (`18016f170` / `18016f6f0` and their callees)
and final output typography remain separate unfinished stages. This bounded path
does not establish complete engine equivalence.
