# Prepared boundary retention and costs

`LegacyCutBoundaryPolicy` restores the decision/cost portion of native
`1800c2fb0`. Its actual table is `1802b7f70`, whose metadata pointer identifies
sampler **type252, parent241** at `1802b8510`. The neighboring type2b1 descriptor
belongs to a different feature extractor and must not be substituted for this
sampler. Type-table metadata addresses are not public object vtable addresses.

The routine evaluates successive prepared-ink positions, obtains a neural cut
probability and adjusts the threshold using adjacent context weights. If the
current weight is exactly`-1`, threshold remains unchanged. Otherwise native
float32 arithmetic is `base * (previous + current) * 0.5`, in that order.

A current or preceding geometry hint promotes the effective probability to the
threshold. The raw NN probability is stored before this promotion and remains a
different quantity. A cut is retained only if its effective probability is
positive and at least the threshold, including equality. Its cut cost is
`round(-log(p) * 8192 * runtimeScale)`; retained cuts also get a separate join
cost `round(-log(1-p) * 8192 * runtimeScale)`. Nonretained joins have cost0.
Zero cut probability and unit join probability use the native sentinel
`0x4000000`. The subtraction`1-p` is float32 before its double logarithm.
Native signed nearest rounding is explicit; these costs are not truncated
acoustic confidence costs or word probabilities.

This API consumes neural probabilities and caller-provided geometry hints.
The NN feature producer, stored model, native geometry tests, long-gap
refinement and mapping prepared positions back onto original stroke points
remain separate. These positions must not be described as arbitrary original
point indices: preparation can create internal ink components. No default
threshold, scale or trained cut model is invented or enabled in the GUI.

The independent implementation bounds probabilities and thresholds to finite
`[0,1]`, requires positive finite runtime scale and rejects costs outside the
checked signed-int32 domain. Its control generator independently applies
float32 arithmetic, sentinel rules and logarithmic rounding for2048 cases,
including subnormal probabilities, equality and zero/unit edges:

```powershell
python tools/LegacyCutBoundary.Probe/reference_controls.py .artifacts/legacy-cut-boundary-controls.json
dotnet run --project tools/LegacyCutBoundary.Probe -c Release -- .artifacts/legacy-cut-boundary-controls.json
```

These checks do not execute the vendor DLL and do not measure cursive accuracy.
