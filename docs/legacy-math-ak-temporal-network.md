# Source-bound original Math1F network

`LegacyMathAkTemporalNetwork.PrepareActual(source, resource)` authenticates the
actual AK and privately owns its fixed base-tail1F model at1007739. It accepts
only222-feature objects issued by that same genuine geometry source. There is
no public model selector, caller weight array or caller feature-array factory.

The first layer performs modular signed16 dot products, followed by the exact
48-entry integer activation table with shift16. Hidden slot0 is1024. The second
layer uses signed8 weights, unsigned row scales and modular32 arithmetic.
Padding lanes must have zero stored weights. The unsafe native INT_MIN hidden
lookup case explicitly refuses.

`EvaluateRaw` returns200 hidden dots,201 hidden values and257 raw outputs.
`EvaluateNetworkCosts` performs one forward and adds the unmasked EXP/Q15 and
log8192 channels. Actual212.V78 is a no-op for this model; the virtual256 row
remains in the network output. Unified259-label costs still require the other
feature/model families and native combination/geometry policies. These results
cannot issue grammar matches or candidate trees.

The factory temporarily loads all six raw models, then detaches1F. Its retained
weights/scales occupy143314 payload bytes; this is not a memory optimization or
an RSS guarantee. The full classifier should share immutable model weights
across source-subset evaluations. Result accessors return detached arrays.

Prepared controls cover8 synthetic dots,17 activation boundaries and771 numerical
costs, with seven corrupted-output checks. The separate static audit verifies all
48 activation,352 EXP and2049 log entries against original PE bytes. CI runs the
portable mathematical reference without SDK files and without model inference:

```sh
dotnet run --project tools/LegacyMathAkTemporalNumeric.Probe -c Release -- prepared.json
python -I -B tools/LegacyMathAkTemporalNumeric.Probe/audit_output.py . prepared.json report.json
```

One controlled actual forward used synthetic points `[[0,0],[2,1],[4,0]]` and the
genuine source-issued temporal features. An independent scalar reference parsed
the original weights and rederived the features; all200 dots,201 hidden values,
257 raw outputs,257 probabilities and257 costs matched exactly. Cancellation,
equal-data foreign-source rejection and output ownership checks passed. Clearing
the caller's entire1F model frame after preparation did not change the result.
No vendor DLL was executed. This numerical test establishes neither handwriting
accuracy nor the complete classifier/grammar/application path.

Explicit actual diagnostics and independent replay (which itself evaluates the
real weights) require separate deliberate commands:

```sh
dotnet run --project tools/LegacyMathAkTemporalNumeric.Probe -c Release -- --actual . capture.json
python -I -B tools/LegacyMathAkTemporalNumeric.Probe/audit_actual.py --actual-replay . capture.json actual-report.json
```

`audit_native_output.py` checks static PE tables/dispatch from the development
research directory. It reads binary bytes and never loads or invokes the SDK.
See [the remaining producer chain](legacy-math-recognition-frontier.md).
