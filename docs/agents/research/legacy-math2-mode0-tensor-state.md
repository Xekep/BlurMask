# Math2 mode0 tensor state over official ONNX Runtime

`Math2Mode0TensorState` is an internal executable numerical adaptation of the checked EF1 mode0 tensor lifecycle. It runs the fingerprinted encoder, initializer and decoder graphs through the existing official ORT C API bridge. It is separate from `StrokeMathRecognizer`; the application's greedy algorithm is unchanged. The SDK and MyScriptMLOrt DLLs are never executed.

The underlying producer/axis proof is [the EF1 lifecycle audit](legacy-math2-tensor-lifecycle.md). `audit_math2_tensor_state_route.py` composes it with the EEF score-channel and BOS-mask proofs. Runtime arithmetic uses CPU copies for concat/gather; it does not reproduce SDK allocations, pointers, in-place permutation, native transcendental dispatch or partial failure order.

## Ownership and stages

`Math2Mode0TensorModels` validates all three current graph SHA256 values before creating its own encoder/initializer/decoder sessions. Session handles are private. The model owner holds a synchronized additive lease on `OrtRuntime`; disposing the runtime while that lease exists rejects. Existing callers never acquire this new lease and retain their old behavior.

`RunEncoder` returns a typed `Math2Mode0EncoderOutputs` owner. It holds the actual five output handles, the exact models and the same live `OrtRun`. `Prepare` checks model identity and policy before consuming the encoder tuple. Cancellation at entry leaves it reusable. Once consumed, either preparation succeeds or all taken handles and its transferred run lease are released. Encoder tuple and state owners prevent early model disposal. Encoder inference and preparation hold operation leases, so model disposal cannot race these operations.

Preparation uses the actual encoder memory lengths and INT64 outputs; it does not infer them from feature frames. The current model shapes require `[L,1,256]` memory/segments and four heads. The default-zero/no-prefix branch creates I64 target0/offset0, F32 `batch_size[1]=0`, empty previous attention and KV caches. The initializer runs once. Its cross-K/V/projected-key output handles move to the input state. Original encoder handles are disposed after preparation; decoder memory_seg is a separate copy. Reading just encoder memory metadata avoids an unnecessary full memory payload copy.

`RunStep` always uses the same decoder and same leased run. It returns one pending typed step with actual four decoder outputs. Running again without applying that step rejects. The selected-row lease retains that exact step identity; equal batch shapes in another state are insufficient. After application, all step output handles are consumed and stale reads, selection creation and replay reject.

`ApplySelected` implements the two maps from `04C930`: actual parent ordinals for attention and KV, identity-or-zero for static memory/cross-K/V/projected keys. Attention concatenates along axis2 before parent gather along axis1; selected nonobservable labels can overwrite the last column with finite `-float.MaxValue`. The flattened beam/head cache dimension expands to rank5 before parent selection and flattens afterward. Old inputs and pending outputs are released exactly once after the next state is constructed. Temporary copy arrays are not retained as an additional history of states.

The force-nonobservable flag remains a named **prepared policy** input. This implementation does not claim its actual factory/configuration producer. The observable classification is bound to the exact raw Math2 resource. Arbitrary model families, prefixes, dynamic optional ports, custom allocator residues and mode variants are refused or outside this API.

## Same-step numerical candidate composition

`step.CreatePortableCandidates` takes actual current logits and the state's actual carried candidate-row costs. It uses the shared `LegacyPortableMathLogProbabilities` helper, then applies the proven bundle BOS0 F32Max cost mask and calls the existing unrestricted heap/raw48 ordering/EOS helpers. The returned `Math2Mode0ScoredCandidates` privately retains both the originating step and the handoff; it cannot accept a caller-created foreign `EosHandoff` merely because dimensions match. Its selection is valid only for that exact pending state generation.

Requested width, temperature, delta, prefixes, source-information and conditional sidecars in this composition are explicitly prepared inputs. The current checks use width3/temperature1/delta1000 and zero sidecars. This closes an actual-logits -> portable scores -> prepared native numerical kernels -> actual tensor-state bridge. It does not establish the outer EEF/EEE controller, configured restriction map, per-beam sidecar producers, completed-hypothesis ownership, maximum-step loop or final recognition/acceptance.

`BindPreparedRows` remains a separately marked diagnostic selection seam for coordinate coverage. It validates complete 48-byte rows, one to sixteen selected records, current parent bounds and target0..296. It is used to exercise repeated/permuted parents and shrink; these are not claimed observed beam choices. All parsed row arrays and record snapshots are copies. The two provenance routes never manufacture an older terminal or decoder step.

## Failure and bounds

The new runtime/run leases synchronize acquire/dispose and reject acquisition after disposal. State methods serialize decoder/update/dispose operations. The actual OrtRun cancellation registration is unchanged. After an inference, numeric-update error or cancellation inside `RunStep`/`ApplySelected`, the state becomes unusable; disposal still releases all handles and leases. Pure argument/owner preconditions reject before mutation, allowing valid reuse. No rollback or native partial-allocation observation is claimed. Native early leak behavior is not emulated.

The copy path validates known ranks/types, selected row extents and limits input/output copies to the existing 64M-element bound. Negative/overflow/oversized or foreign coordinates reject before allocating their destination. Native INT64 and F32 tensor enums remain distinct. The additive INT64 reader shares the existing output extent bound and rejects a different element type or disposed handle.

## Verification

A procedural two-stroke geometric fixture supplies real feature input without expected transcription. Both Managed and fresh Windows NativeAOT execute:

- two five-step paths with explicit force policy false/true: B1->3, repeated parents `[2,0,2]`, shrink3->2, permutation2->2 and shrink2->1;
- four additional actual decoder steps through portable score -> heap/EOS -> state selection;
- five prepared nonuniform/zero-extent gather controls, needed because real static memory is identical across initial beam duplicates;
- 105 owner/lifecycle/shape/cancellation guards, including same-shape foreign state selection, stale/replayed rows, early runtime/model/run disposal, disposed owners, changed graph fingerprint, failed initializer preparation and failed decoder inference using actual terminated ORT runs.

The independent NumPy oracle verifies **1,707,680 exact tensor/input/cache bytes**, all14 captured decoder steps and complete heap/sort/EOS record transport. It separately rounds portable exp/log arithmetic to F32 with a 4e-6 tolerance; the checked runs observed error0. Native SDK exp/log equivalence is not inferred. Managed/Native captures must be byte-identical. The full three graph fingerprints and the raw-resource fingerprint are present in each capture.

The old candidate-ordering/heap/completion/EOS capture remains SHA256 `03f46bfcf337fe0fc69ccdbb3c6c2baebf76671aa3325c088dc5a6cef53b256a`, with the original independent audit. Existing runtime tensor/Identity/termination, five runtime/model guards, 84 numeric and17 formatter/model checks also pass in Managed and NativeAOT.

Reproduce with official pinned CPU ORT1.23.2; all generated assets stay outside the repository:

```powershell
python tools/fetch-ink-math-runtime.py win-x64 --output E:/BlurMask-build-artifacts-20261014/joined-math2-state-artifacts
python tools/export-ink-math-model.py models/math2/math2-unwrapped.res --output E:/BlurMask-build-artifacts-20261014/joined-math2-state-artifacts/model.ikrmath
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet run --project tools/LegacyMathTensorState.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/joined-math2-state-build -- E:/BlurMask-build-artifacts-20261014/joined-math2-state-artifacts/model.ikrmath models/math2/math2-unwrapped.res E:/BlurMask-build-artifacts-20261014/joined-math2-state-artifacts/runtimes/win-x64/native/onnxruntime.dll E:/BlurMask-build-artifacts-20261014/joined-math2-state-managed.json
python tools/LegacyMathTensorState.Probe/audit_output.py E:/BlurMask-build-artifacts-20261014/joined-math2-state-managed.json models/math2/math2-unwrapped.res
python tools/handwriting/audit_math2_tensor_state_route.py E:/BlurMask-build-artifacts-20261014/joined-math2-state-static.json
```

Static audits require the preserved local research PE inventory; portable numerical tests require only the raw resource, exported model pack and official runtime. An isolated worktree may set `PYTHONPATH` to the original read-only `research/myscript/scripts` directory. No application acceptance threshold, UI behavior or full beam equivalence is asserted.
