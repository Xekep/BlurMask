# Actual encoder boundary diagnostic

This tool isolates the first observable numerical boundary of the existing
Math2 owning probe. It does not add a production hook or another inference.
`prepare_math_encoder_boundary.py` copies the current sources into a fresh
directory, adds before/after observations around the actual `Encoder.Run`, and
starts/seals the collector in the existing probe. Main sources remain intact.

The collector records type, shape, element count, payload bytes and SHA256 of
the actual three inputs and five outputs for eighteen existing calls: fresh,
unchanged-prior and changed-input for each of six cases. It keeps no engine
owner or tensor outside the collector. It rejects more than24 calls, more than
4Mi elements per tensor and more than128MiB of observed tensor payload before
copying. These are diagnostic work bounds, not process-memory measurements.

```powershell
python -B tools/ci/prepare_math_encoder_boundary.py .artifacts/math-encoder
dotnet build .artifacts/math-encoder/stage/tools/LegacyMathCurrentSelectedIngress.Probe -c Release --artifacts-path .artifacts/math-encoder/build -p:EmitCompilerGeneratedFiles=true -p:DebugType=portable -p:DebugSymbols=true
python -B tools/ci/audit_math_encoder_boundary.py --prove-build .artifacts/math-encoder/stage .artifacts/math-encoder/build .artifacts/math-encoder/pdb-proof.json
```

The prepared extent is explicitly Core951 / diagnostic Math88 / probe3.
Preparation checks exact insertion sites and round trips; the manifest records
all original and prepared hashes. Build proof rejects changed/unexpected sources
and authenticates all authored documents in the compiled PE/PDB. Generated JSON
sources must be emitted to disk; omitting that setting cannot pass PDB proof.
Use a new output directory for another source revision.

Run the prepared probe with the same six pack/resource/runtime/fixture arguments
as the normal current-ingress probe and a fresh capture path. It writes an extra
`capture.json.encoder.json`. Audit it and same-platform repetitions with:

```powershell
python -B tools/ci/audit_math_encoder_boundary.py managed.json.encoder.json managed.json repeat.json.encoder.json native.json.encoder.json
```

The manual `Math encoder boundary diagnostics` workflow performs preparation,
build proof, two Managed runs and Native AOT on Windows/Linux/macOS. Its result
is diagnostic consistency, not the separate historical-reference release gate.
Cross-platform SHA differences locate a boundary; they do not measure a numeric
error bound or authorize a tolerance/reference exception. No independent neural
mathematical recomputation is claimed.

Root's Windows evidence is under
`E:/BlurMask-build-artifacts-20261014/math-encoder-boundary-20261029`.
All eighteen encoder tuples match between Managed, repetition and Native AOT;
six fresh/prior input/output pairs are exact. The original capture is byte-equal
to uninstrumented Main. `FINAL.json` SHA256:
`cff50f1c801d85b40e4b0c8405a328dfcf87ce3fabefb3c306e81a30886dfd94`.
`TOOLS-FINAL.json` additionally verifies source reproduction, eight trace
corruptions and two prepared-source corruptions. The first build's missing
generated-source PDB failure and its binaries/logs remain preserved.

## Three-platform observed boundary

Run `40843964502` at `3c374f4` passed on Windows, Linux and macOS. All1042
prepared authored sources are byte-equal between platforms and PE/PDB-proved
before execution. Model/resource/fixture pins match; runtime binaries retain
their distinct official platform hashes. Two Managed runs and Native AOT agree
exactly within each platform. On every platform the full recognition capture
also equals its earlier uninstrumented owning capture.

All54 input tensors (18 existing calls ×3 inputs) are bit-identical across the
three platforms. Windows/Linux also agree on all90 encoder output tensors.
macOS differs on40 float output tensors, with every type/shape/count unchanged;
integer outputs remain exact. The first existing fresh call already differs in
`output_seg` and `counting_output`. Thus the first observed difference is inside
the actual encoder execution, before the later C23/prior/history machinery.
This does not identify a specific ONNX operator or establish an error bound.

`CI-BOUNDARY-FINAL.json` SHA256 under the evidence root:
`5740bf0fcdd4c110dd4aa5f9a863aa165751254ea2217c697a11e77218d7d5d3`.
The separate historical Windows-hash gate remains unchanged.

The current runtime requests optimization level99. ONNX Runtime documents
[graph fusion and hardware-dependent layout optimization](https://onnxruntime.ai/docs/performance/model-optimizations/graph-optimizations.html);
this is context, not proof that a particular optimization caused these results.
The [pinned1.23.2 configuration definitions](https://github.com/microsoft/onnxruntime/blob/v1.23.2/include/onnxruntime/core/session/onnxruntime_session_options_config_keys.h)
describe optional approximations/precision changes separately. No such setting,
new tolerance, platform hash exception or library numerical behavior was changed
by this diagnostic.
