# Legacy float input quantization

`LegacyFloatInputQuantization` restores the checked type212 virtual float-input
append operation: table `1802ba2c0`, slot90, function `1800ebb60`. Native multiplies
each float by `float(1 << modelExponent)`, truncates to signed int32 and stores
the low signed16 bits. It neither rounds to nearest nor saturates the result.
The inspected RU313 network has exponent10; its context −.99 becomes −1013,
while −1 and +1 become −1024 and +1024.

The bounded reusable API accepts1..4096 values and explicit exponents0..14.
It rejects nonfinite or int32-unrepresentable products rather than reproducing
the native conversion's indefinite result. The caller is responsible for using
the exponent and float channels of the same verified model profile.

Managed and Windows Native AOT tests pass640 independent Python float32
reference cases, plus explicit context truncation, signed-short wrapping,
int32 minimum, malformed floats/exponents/lengths and cancellation controls.
No neural model, trained calibration or ink asset is distributed by these tests.
This arithmetic pass supplies no context geometry, prior selection or recognition
accuracy by itself.

```powershell
python tools/LegacyFloatInput.Probe/reference_controls.py .artifacts/ru-float-input.json
dotnet run --project tools/LegacyFloatInput.Probe -c Release -- .artifacts/ru-float-input.json
```
