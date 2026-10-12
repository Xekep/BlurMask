# Prepared character line equation kernel

`LegacyPreparedCharacterLineEquations` restores MyScriptInk **020d70**'s ten-moment producer and **020ed0**'s native three-parameter solve. These are the actual producer/consumer used inside line-fit **021110**, called by service **C1A** callback **0221a0**. They consume prepared boundary/profile observations; they do not infer those observations from raw ink or choose script/category candidates.

The statically inspected MyScriptInk.dll SHA-256 is `b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a`. MyScriptDocument.dll is `17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f`. No vendor library was loaded or executed. Private exports are under `D:/BlurMask-build-artifacts-20261011/InkInitialLineC1aLinearNext` and `InkInitialLineC1aFitNext`.

## Ordered moment producer

Each native call receives X center, top/bottom Y, prepared top/bottom profile values and a weight. Define `y=(topY+bottomY)/2`, `q=(topProfile+bottomProfile)/2`, `dy=bottomY-topY`, `dq=bottomProfile-topProfile`. The ten double moments retain native addition/multiplication order:

```text
W  = sum(weight)
X  = sum(x*weight)
Y  = sum(y*weight)
Q  = sum(q*weight)
XX = sum(x*x*weight)
YX = sum(y*x*weight)
QX = sum(q*x*weight)
YY = sum(y*y*weight)
YQ = sum(y*q*weight + dy*dq*weight)
QQ = sum(q*q*weight + dq*dq*weight)
```

The implementation also preserves the native span contribution after each accumulated midpoint term. It does not aggregate observations in a different order or substitute a covariance formula. Input arithmetic is double precision; upstream native code has its own float-to-double preparation, not silently reproduced here. Finite values, nonnegative weights, an explicit observation budget and cancellation are checked. Moment storage uses a cleared stack span, avoiding per-iteration heap allocation.

## Native solve and output

For a regular unconstrained case, the independent normal-equation matrix in `[intercept,slope,heightScale]` order is:

```text
[ W   X          Q  ]       [ Y  ]
[ X   XX+1e-12   QX ]       [ YX ]
[ Q   QX         QQ ]       [ YQ ]
```

The port retains **020ed0**'s exact eliminated expressions, absolute determinant threshold `1e-10`, optional force-zero branch, and minimum/maximum slope clamp order. It then computes height scale using the native zero-slope or nonzero-slope denominator branch, with strict tolerance `max(abs(denominator)*2^-51,2^-52)`. Intercept follows the weighted-mean equation.

The native clamp is followed by its particular eliminated equations. It is not claimed to be a general constrained least-squares optimum. Even force-zero is followed by slope bounds; a positive minimum can subsequently clamp it away from zero. Singular prepared inputs can produce a zero height scale. The library does not invent a positive height or confidence for them.

**021110** finally narrows `intercept-heightScale`, `-2*heightScale`, and slope to float baseline Y, x-height and slope. `ToPreparedFitOutput` exposes that conversion separately from the wrapper's acceptance policy and rejects float overflow. Negative or zero fitted x-height is retained as a candidate value; acceptance/rejection is upstream. Intermediate overflow is rejected explicitly, preventing arbitrary finite malformed moments from silently passing through NaN comparison/clamp branches.

## Initial source-line binding and unresolved stages

Document **052e90** and **0531b0** call service **C1A** and promote its three float outputs into Text2da source line record **+10 baselineY**, **+20 x-height**, **+18 slope**, stride **0x38 = 56 bytes**. C1A factory configuration is a **0x18 = 24-byte** record. The observed flags `0x1402` provide two count bytes 2 and 20, followed by slope bounds and additional scalar limits.

The primary **021110** consumer uses **0x248 = 584-byte** prepared item records, candidate categories, top/bottom profile alternatives and weights. It iteratively reweights candidates and calls this equation kernel, with native convergence and a maximum of 251 passes. The complete prepared-item producer and iterative consumer are **not part of this batch**. The outer **0221a0** has its own failure/small-height bounding-rectangle branch; this port does not substitute that branch for the primary fit or present it as a learned x-height producer.

Consequently, the native initial source-line caller is identified, and its numeric equation kernel is executable, but automatic connected-handwriting preparation is not declared complete. No neural network, lexical alias, glyph category, probability or acceptance confidence is invented.

## Independent checks

`tools/LegacyCharacterLineEquations.Probe/reference_controls.py` forms independent midpoint and span design rows, multiplies weighted matrices with NumPy and solves them with `numpy.linalg.solve`. It does not use the native eliminated expression sequence. **512** regular weighted matrix cases pass in managed and Windows NativeAOT, compared within `1e-10` scaled tolerance for different solver arithmetic order.

Additional tests check clamped-slope native residual equations, force-zero and independent reduced-height covariance, the exact determinant threshold and adjacent double values, known baseline90/x-height80 output semantics, malformed weights/values, cancellation, input/intermediate overflow and output narrowing. No actual handwriting/model accuracy is inferred from these synthetic equation controls.

A prepared-profile regression combines an ordinary80-unit body, a160-unit
ascender and a2-unit detached mark. Their explicitly supplied profiles describe
the same baseline90/x-height80; the fit retains those metrics rather than using
the minimum or maximum physical height. Translation and uniform scaling retain
the same relative result. This checks the equation consumer with known profiles,
not automatic categorization of the mark or ascender from raw strokes.

```powershell
python tools/LegacyCharacterLineEquations.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/character-line-equation-controls.json
dotnet run --project tools/LegacyCharacterLineEquations.Probe -c Release -- D:/BlurMask-build-artifacts-20261011/character-line-equation-controls.json
dotnet publish tools/LegacyCharacterLineEquations.Probe -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/character-line-equation-aot
& D:/BlurMask-build-artifacts-20261011/character-line-equation-aot/LegacyCharacterLineEquations.Probe.exe D:/BlurMask-build-artifacts-20261011/character-line-equation-controls.json
```
