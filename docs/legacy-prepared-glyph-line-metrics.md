# Prepared glyphs to source line metrics

`LegacyPreparedGlyphLineMetricProducer` ports the scalar value producer in MyScriptDocument **060820** and its concrete Text2da line setter path. It takes actual prepared affine glyph geometry and optional normalized glyph-line metadata, averages baseline and x-height contributions in native arithmetic order, and produces source line geometry. It does not estimate a handwriting line from raw points or manufacture glyph metadata from stroke boxes.

## Proven producer and setter

Document **060a20** traverses existing recognized-line/range records, skips range flag21 and negative source-line IDs, resolves referenced input items, and collects concrete glyph type **C25** by source line before calling **060820**. This is a prepared glyph metric refresh. Raw stroke type C26 is not silently treated as a converted glyph. The selection/traversal and partial mutation lifecycle are upstream of the scalar library API.

For each collected glyph, **060820** queries the geometry service for six floats: origin and two affine edge vectors. It uses origin Y and the second vector's two components. It promotes the vector floats to double and evaluates native **19dd00**, the statically linked `_hypot` implementation. It queries optional normalized baseline and x-height fractions through glyph interface **C3D+18**. These are the actual payload fractions already covered by `LegacyGlyphGeometryPayload` and the rendered-font geometry builder.

Let `v` be the second vector length, `Y` its origin Y, and `(b,h)` the optional baseline/x-height fractions. Native updates the double sums in this exact order:

```text
fractions present: baselineSum = (baselineSum + b*v) + Y
                   heightSum   = heightSum + h*v
fractions absent:  baselineSum = (baselineSum + Y) + v
                   heightSum   = heightSum + v
```

The absence branch uses the actual prepared affine glyph's vector, not a substitute handwritten bounding box. Empty input is a successful native no-op, represented by a null result. Nonempty means divide each sum by the glyph count.

Document **053c60** creates Text class **2da** and calls **0531b0** to prepare its source line array at runtime **F8**, count **100**, stride **38**. Table **Text275550+1E0 = 0376c0** updates source line position; **+1E8 = 0378e0** updates angle/height. **060820** supplies fourth argument zero and the mean as fifth argument to each setter, producing record **+8 = 0**, **+10 = mean baselineY**, **+18 = 0**, **+20 = mean x-height**. The native property names are `x`, `y`, `a`, `b`, and `h`; the height setter removes the old `x_height`/`x-height` aliases before setting `a`/`h`. Property-tree side effects, allocation failures and first-setter-success/second-setter-failure partial updates are not replaced by a fictional atomic native success. The library returns owned values rather than reproducing that object lifecycle.

Document **177590** reads a prepared result line object's double+20 and narrows it to SDK `lineInfo.float+18`. Interface **CA8+78 = 0b6810** and iink **4c2660** transport the metric to success payload+1c, used by the font-size policy. `ToSdkSourceLineXHeight` restores this narrowing and rejects nonfinite overflow; `ToFontSizeSample` carries the resulting `SourceLineXHeight` alongside explicit recognized-range coordinates. The full source-line-to-result-line object binding remains upstream, and this does not claim that any currently used raw handwriting decoder supplies the metric.

## Exact finite float-vector norm

The inspected native **19dd00** returns the absolute other input for zero, computes `sqrt(x*x + y*y)` when the unsigned exponent test `(ex - ey + 0x36) < 0x6d` holds, and otherwise returns `x+y`. Its rescaling branches use double exponent thresholds **20B** and **5F4**. Finite nonzero binary32 inputs promoted to double have exponents **874..1150**, so none of those double rescaling/subnormal/error branches can occur. The library ports that complete bounded branch with `Math.Sqrt`, preserving native order and unsigned arithmetic. NaN/infinity inputs are rejected at the public boundary.

The source line is double precision; it can remain finite when its later SDK float is too large. That narrowing is checked separately. Zero x-height is retained rather than invented as a positive font size.

## Independent controls and scope

`tools/LegacyGlyphLineMetric.Probe/reference_controls.py` creates **540 cases / 4109 glyphs**. Its independent norm oracle forms exact float32 squares using 200-digit Decimal arithmetic, rounds their sum to binary64, and applies square root; it contains no native exponent/zero/rescaling branches. Float32 products are exact in binary64, and far-exponent float pairs round to the same larger norm. Both baseline and x-height doubles, plus SDK float outputs, are compared bitwise in managed and Windows NativeAOT probes.

Controls cover absent/present fractions, rotated/signed vectors, zero vectors, float subnormals and exponent-difference boundaries **53/54/55**, small/large magnitudes, ordered averaging and deliberate cancellation cases that distinguish the native addition order from reassociation. Other checks cover the empty no-op, malformed/partial metadata, cancellation, bounds and SDK narrowing overflow. The existing rendered-glyph builder, actual payload readback, line producer and handwriting font-size policy are composed in a known-value round trip.

This is a verified **prepared glyph → source line metric** stage. Initial connected-handwriting line inference, automatic glyph/range selection, native result ownership and final text placement are not declared complete. In particular, existing stroke boxes or source-frame heights are not relabeled as this line x-height.

```powershell
python tools/LegacyGlyphLineMetric.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/glyph-line-metric-controls.json
dotnet run --project tools/LegacyGlyphLineMetric.Probe -c Release -- D:/BlurMask-build-artifacts-20261011/glyph-line-metric-controls.json
dotnet publish tools/LegacyGlyphLineMetric.Probe -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/glyph-line-metric-aot
& D:/BlurMask-build-artifacts-20261011/glyph-line-metric-aot/LegacyGlyphLineMetric.Probe.exe D:/BlurMask-build-artifacts-20261011/glyph-line-metric-controls.json
```
