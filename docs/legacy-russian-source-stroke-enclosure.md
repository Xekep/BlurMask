# Native ordinary source-stroke enclosure score

`LegacySourceStrokeEnclosure` restores the scalar geometry service used by
ordinary source219 to populate each physical stroke's float field at record+C.
It accepts a bounded explicit float geometry snapshot and returns one score per
stroke. It does not infer special source records, source enablement, ingress
filtering or upstream resampling. No model weights, vendor execution or activation
are needed by this component.

## Source binding and correction of the previous hypothesis

Text source218 virtual60 at `180168c00` reads float record+C from secondary120+58,
with physical-record stride0x1c. Source219 init `180163970` copies initdescriptor+40
to runtime+48; if enabled it allocates type2ad through the engine factory. That
descriptor is `1802b3d80`, service table `1802b3d00`.

Ordinary ingress `180167210` sets record+4 to **−1** after point-map creation.
This is not the physical point count: opaque and special kind6 input have separate
branches assigning other values. Process `1801680d0` resets the service, appends
source float geometry, commits it, and calls service virtual20 for physical
records whose +4 is negative. Nonnegative +4 records receive zero.

Virtual20 at `180088970` calls `1800880f0` with a zero-initialized float output;
on success it returns that float in XMM0. The original automatic decompiler
misidentified the return type and printed zero in both branches. Assembly at
`180088992` explicitly loads the output with `movss xmm0,[rsp+0x48]`.
Therefore **ordinary input is not entitled to a constant zero probability**.

## Restored geometry

`1800880f0` constructs float32 cumulative arc lengths and cumulative x/y reversal
counts. It scans endpoint pairs backward, using the first/last quarter of the arc,
strict closure-distance/length tests, at most ten local reversals, and an area to
closed-perimeter-squared threshold of0.0159. The best candidate uses
`(1−closurePenalty)*area/perimeter²`. Its polygon comprises points `[start,end)`;
the endpoint at `end` participates in the closure tests, not the polygon area.
The separate `180087930` area calculation and native accumulation order are
preserved. Target strokes over500 points return zero.

The score becomes nonzero only when **strictly more than half of the points of
at least one other physical stroke lie inside the best contour**. The native
helpers `180087e80` and `180087a80` implement a bounding-box rejection and nonzero
winding number. The public result is multiplied by12.566, exactly as serialized
in the engine code; no additional clamp is invented.

This is a contour enclosing ink, similar to a lasso gesture. It is not a generic
circle confidence and not a baseline confidence. An isolated ideal circle or
square returns zero. A10×10 closed square enclosing another stroke returns
`12.566/16 = 0.785375`. Exactly half of another stroke's points inside still gives
zero. The geometry service's output is consumed by the cut-policy and final
acoustic-cache paths; those consumers must use the same explicit source snapshot.

## Controls and scope

The vendor-free Python reference enumerates endpoint pairs, independently uses
shoelace polygon area and ray crossings, and generates384 varied ellipse/partial
loop fixtures, direction/source-order variants, random inside/outside strokes and
partial closure. Managed and Native AOT compare all per-stroke outputs within
0.0002, allowing float32 versus the independent area calculation. Additional
analytic tests verify the exact square result, isolated/open/repeated strokes,
strict majority, source ordering, the native500-point limit, malformed coordinates,
invalid ordinals and cancellation.

```powershell
python tools/LegacySourceEnclosure.Probe/reference_controls.py .artifacts/myscript-analysis/output/RussianAssetsSearch/legacy-source-enclosure-controls.json
dotnet run --project tools/LegacySourceEnclosure.Probe/LegacySourceEnclosure.Probe.csproj -c Release -- .artifacts/myscript-analysis/output/RussianAssetsSearch/legacy-source-enclosure-controls.json
```

These controls verify the restored geometry primitive, not Russian handwriting
accuracy or a complete source219 implementation. Automatic source configuration
and exact processed-source point preparation remain separate integration proof.
