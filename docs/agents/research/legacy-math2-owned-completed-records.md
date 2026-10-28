# Owned ordinary Math2 EOS records

`Math2Mode0CompletedRecords` joins actual same-Step EOS events to the ordinary `037430` record lifecycle. It consumes the actual raw48 row before EOS masking, copies the previous primary and secondary prefixes without appending EOS, records the computed common-secondary frontier, and creates a distinct typed empty Library3E for each counted record.

This is an internal bounded producer and owner. Width, temperature and delta are still explicit prepared controller inputs. It does not implement outer search termination, the non-EOS maximum-step fallback, post-loop attention/EF4 processing, final formula output or acceptance. The existing application recognizer is unchanged.

## Exact source and event ownership

The context is created through the existing authenticated cold bundle: raw resource SHA-256 `2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a`, vocabulary297 and the three already-pinned encoder/initializer/decoder graphs. It accepts no imported prefix, optional detection or constraint model. It uses the existing official Microsoft CPU ONNX Runtime1.23.2 path, not MyScriptMLOrt or the vendor SDK.

Each context can create one completed-vector owner. `Capture(candidates,eventOrdinal)` accepts an event only from that context's current Step and immutable prefix generation. It reads the row at the numerical event's rank from the retained **pruned-before-EOS raw48 pool**. Numerical score facts, copied rows or a foreign same-shape context cannot substitute that source.

The one-use lease retains the exact candidates owner, Step/generation, rank, raw48, old primary/secondary prefixes, complete previous rank/time ancestry and computed frontier. Capturing the first event binds that candidate generation for the Step. A second `CreateCandidates` result cannot inject another version of the same event, and `ApplySelected` must use the exact bound candidates. Every captured lease must be consumed before selection. A repeated capture, replay, foreign recorder, stale Step or entry cancellation rejects before record allocation or count changes.

The completed record also retains its source lease **out of band**. This is portable provenance, not an extra native168 field. Copied prefix/source arrays cannot mutate the record. The lease does not retain usable per-time attention tensors after tensor application; the original TensorStep becomes stale as before.

## Frontier before recording

The [frontier producer](legacy-math2-common-secondary-frontier.md) is independent from the secondary-sidecar argmax. It scans old secondary U16 vectors from index0 in this no-import scope, with signed I16 from the first beam and unsigned U16 from other beams. EEF.V30 then checks whether the current per-beam logits argmaxes agree.

The pinned EEF rank2 view drops the time dimension while preserving the original data pointer, so contiguous copied `[T,B,V]` tensors use **frame0**. It does not use the last-frame log probabilities used by the cost/sidecar adapter. Engine185D20 initializes argmax from the first element and uses ordered strict greater comparisons: ties keep the first index, a later NaN is ignored, and a first NaN leaves index0. A successful common flag advances the saved frontier by1, even when the cold old prefix is empty.

The same-Step frontier is computed once and cached with cancellation checked even on a cached read. Cold nonempty histories have length equal to the generation offset; a missing/imported history is explicitly refused. The actual eleven EOS events in the regression have frontier2,3 or4, rather than a fabricated constant0.

## Vector, record and Library3E lifetime

The native prerequisites are [034EA0/037430 ownership](legacy-math2-completed-record-ownership.md) and the [concrete type3E constructor](legacy-math2-completed-library-owner.md). The managed join preserves their logical ordering:

1. Grow vector capacity to count+1 before constructing the new record. Failed growth leaves the old count/capacity; later constructor failure retains the successful capacity change.
2. Construct a distinct logical24-byte Library3E owner and its distinct empty logical72-byte pool; adopt the child.
3. Add the record and increment count only after successful child construction/adoption.
4. Write signed parent from raw48 `28`, frontier at completed `28`, and the existing no-constraint F32 normalized score at completed `00`.
5. Reserve/copy the old primary U16 prefix, incrementing count after each element. Reserve/copy the separate secondary sequence afterward. No EOS label is appended.
6. Clear nested tensor count `38` in the common content epilogue. No nested tensors exist in this supported append route.

The Library3E pool represents the known zero semantic qwords at `00/08/18/20/28/30/38/40` and DWORD`10`. Its unwritten padding`14..17` is absent from the representation. Library and pool identities are distinct across records and are released once, including an unadopted base/pool after construction/adoption failure. Context disposal also disposes its completed owner.

This uses managed objects, lists and copied arrays. Reported vector capacities are the native **logical** capacities; the backing `List` may reserve additional physical memory. No native registry emulation, physical allocation/reallocation order, ABI pointer bytes, constructor padding`04..07/64..67`, or byte-identical native168 image is claimed.

After the consuming operation begins, failure or cancellation poisons both recorder and prefix context. A record already counted remains counted, with its written header, score and copied primary prefix retained. Failure before adoption retains only the prior records/capacity and releases the unfinished child. Entry cancellation leaves the same pending event reusable. The diagnostic callback seam observes explicitly prepared failure boundaries; it is not evidence that the official runtime or a native allocator actually failed there.

## Verification

The new probe executes the actual official-ORT decoder fixture through the existing three five-step prepared continuation schedules and the separate diagnostic parent trajectory. Eleven real EOS observations feed owned records: one from width1/delta-1, nine from width3/delta1000, and one from width3/delta-1. These are network-derived events, not native outer-loop stopping decisions or accepted formula candidates.

The independent auditor verifies literal event raw48 bytes against the retained sorted/pruned pool, complete prior prefix/ancestry identity, F32 score/divisor, frontier and every semantic record/count/capacity/library field. It separately checks28 prepared failure/cancellation paths, including pre-growth, base/pool/adoption, counted-before-content and partial primary-copy boundaries. There are512 prepared frontier controls and327 additional lifecycle/ownership guards.

The local Windows full prefix/tensor capture remains SHA-256 `d350e2ca63956bc524936c40c60664c173130d4c1216c1850ed930c3ca5b3d08`, including1,828,524 independently checked tensor/cache bytes and the original118 guards. A separate `.records.json` contains the new records and failure-prefix evidence. Managed and fresh NativeAOT are byte-identical: the separate241,132-byte record capture has SHA-256 `f455ac7a4b99e892cb5a827bb955a28d73e3bdf4ed0350f74c122eecaf0a621d`. The old tensor-state, cold-bundle and candidate-ordering regression hashes also remain `81aa9e5b9cbb6a167bfce9e3684d819575ac403088df1030223a680aca0aacf3`, `44f43ce023af50dd46dca32664ee479673e26a91378014edb399a3dab7a4bc51` and `52dba125a1e40368ca3fb6db76c60d2a267d4f0f9470f96d8b5dde2117636b20`.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet run --project tools/LegacyMathCompletedRecords.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/joined-math2-completed-build -- E:/BlurMask-build-artifacts-20261014/joined-math2-state-artifacts/model.ikrmath models/math2/math2-unwrapped.res E:/BlurMask-build-artifacts-20261014/joined-math2-state-artifacts/runtimes/win-x64/native/onnxruntime.dll E:/BlurMask-build-artifacts-20261014/joined-math2-completed-managed.json
python tools/LegacyMathCompletedRecords.Probe/audit_output.py E:/BlurMask-build-artifacts-20261014/joined-math2-completed-managed.json models/math2/math2-unwrapped.res
python tools/handwriting/audit_math2_completed_recorder_join.py E:/BlurMask-build-artifacts-20261014/joined-math2-completed-static.json
```

The static join auditor composes five previously pinned prerequisites; their regions overlap, so their counts are not presented as unique PE coverage. It executes no native library.

The current actual decoder has `attn_seg`, making post-loop036F20/EF4 a real future requirement. This append stage leaves completed output units `50/count58` and auxiliary`98` constructor-empty. Reconstructing the required attention from history or carried attention state, final-step non-EOS recording, sorting/truncating/releasing complete owners, transport and acceptance remain unjoined. This API must not be presented as a complete Math2 decoder.

Cross-platform CI derives record expectations from captured tensors and compares the prefix capture with the old probe on the same platform when its optional baseline argument is supplied. The Windows capture hashes above are local regression evidence, not universal ORT numerical goldens.
