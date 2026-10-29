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
