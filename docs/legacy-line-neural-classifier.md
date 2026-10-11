# Embedded neural line-boundary classifiers

The Text class `2e3` has two serialized neural models embedded in the DLL.
Configuration `180076ba0` loads them into object offsets `f8` and `f0` from
addresses `18029e9d0` and `180277280`. They are independent of the external
language `.res` files:

| Embedded address | Resource bytes | Architecture |
| --- | ---: | --- |
| `18029e9d0` | 85,812 | input42 → affine100 → BLSTM30 → output2 |
| `180277280` | 161,332 | input42 → affine100 → BLSTM50 → output2 |

Both resources have type `5f`, version `106`, flags zero, int16 quantized
weights and the checked activation sequence `[0, 1, 1, 3]`. The affine100
stage uses tanh and the two output logits use softmax. Both have a 336-byte
tail with 42 float32 feature means followed by 42 scales. The normalization
consumer `180076250` applies `(feature - mean) * scale`.

`PreparedLineBoundaryClassifier.Load` reads these bounded resource profiles
directly. It reconstructs all matrices, row quantization metadata, peepholes
and feature normalization, validates shapes, padding, finite values and full
resource consumption, and rejects other profiles. It uses the shared portable
`NativeNetwork` executor with an optional affine/tanh input stage. Existing
English and Raw Content networks continue without that optional stage.
Each inference call owns its feature copy and recurrent scratch state.

The network inputs are prepared stroke-pair features. The feature consumer
`1800759c0` uses two stroke records' bounds, means, lengths, direction evidence,
sequence relation and explicit group scale. Consumers `18007d7c0` and
`18007e960` use the predictions when rearranging or splitting group lists.
The native output labels and the full grouping search are not yet reconstructed.
These two outputs are not exposed as baseline, font size or screen placement.
Baseline and line-height records are materialized by the separate geometric
pipeline described in [prepared line geometry](legacy-line-geometry.md).
This does not establish the absence of other learned layout models in the SDK.

The pinned DLL SHA256 is
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
`tools/extract-embedded-line-models.py` performs read-only bounded PE inspection
and validates both original model hashes. It neither executes the DLL nor
changes activation, account or licensing checks. Once extracted, the `.res`
files do not require the DLL or Python at application runtime.

Managed and Windows Native AOT probes pass 32 independent NumPy controls using
the two real networks and two own nonzero synthetic networks. Controls cover
one, two, seven and sixteen frames with and without feature normalization.
They additionally verify repeated and concurrent inference, probability sums,
input immutability, malformed/truncated resources, finite input bounds and
cancellation. Output tolerance is `2e-4` because host float math and quantization
boundaries are not claimed to match all native instruction paths bit for bit.
The unchanged generic ink probe also passes its float/int16/int8 BLSTM golden
checks. Generated pair features are numerical controls, not handwriting accuracy
tests, an automatic-baseline benchmark or complete SDK parity.

```text
python tools/extract-embedded-line-models.py path/to/MyScriptText.dll --output .artifacts/embedded-line-models
python tools/PreparedLineClassifier.Probe/reference_controls.py .artifacts/line-classifier-controls.json .artifacts/embedded-line-models/line-2e3-30.res .artifacts/embedded-line-models/line-2e3-50.res
dotnet run --project tools/PreparedLineClassifier.Probe -c Release -- .artifacts/line-classifier-controls.json
```
