# Original Math AK source geometry

`LegacyMathAkSourceGeometry.CreateActual` copies bounded original XY strokes
and applies the actual base-tail AK normalization program: modes1/1 and
target8192, authenticated against `models/math/math-ak.res`. It preserves
point order, duplicate points and original stroke boundaries. Returned arrays
are detached copies.

The implementation follows00FB90's ordered floating-point operations and
half-away-from-zero conversion. Scale starts at1 and is recomputed only when
both source extents are nonzero. Horizontal and vertical lines therefore keep
their original scale. Inputs whose normalized coordinates cannot be represented
as signed shorts are rejected; native wrapping is outside this portable subset.
Admission limits are64 strokes and16384 points with finite bounded coordinates.

This is the input prerequisite for the original acoustic classifier. It does
not produce feature vectors, predictions, source13A3 receivers, grammar matches
or expression trees. The application continues to use the separate modern
Math2 path. See [the remaining producer chain](legacy-math-recognition-frontier.md).

The headless probe checks normalization, bounds, cancellation, input/output
ownership and original stroke ends. Its Python reference independently
reconstructs float32 rounding and rejects corrupted captures. These prepared
controls run in CI without loading a vendor DLL or executing a neural model:

```sh
dotnet run --project tools/LegacyMathAkGeometry.Probe -c Release -- . capture.json
python -I -B tools/LegacyMathAkGeometry.Probe/audit_output.py capture.json audit.json
```

Static evidence pins the complete native region00FB90..0102CD, SHA256
`54fb0b7375eb59b7456b0dbd966e19475b71c51daffedd2d4c9008ea91897634`.
The immutable native supplement manifest has SHA256
`7c7620f7ef2f45dc0efe6812e48858d2cbad8fde79904bbfbb12467198baeaac`.
These checks establish the stated geometry subset, not recognition quality or
completion of the original engine.
