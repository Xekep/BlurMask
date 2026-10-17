# Owned Math2 mode0 prefixes and actual per-step sidecars

`Math2Mode0PrefixContext` joins the restored cold tensor state with the primary/secondary prefix and ancestry producers from `038610`. It privately creates its tensor state through the actual `Math2Mode0ColdBundle`. Per-beam information is derived from that exact pending decoder Step's log probabilities, rather than caller-supplied zeros. The no-detection conditional value is derived from the fingerprinted current model set.

This is an internal bounded per-step owner. Requested width, temperature, delta and the decision to run another step remain explicit prepared controller inputs. It does not implement the complete EEE search/termination loop, imported prefixes, configured aliases, optional detection tensors, completed168 record construction, final recognition output or acceptance. No app path is changed.

## Source and ownership

The constructor consumes the existing cold bundle's actual encoder/initializer path and keeps the resulting tensor state private until disposal. The bundle authenticates the raw resource, labels and all three complete graph fingerprints. Its derived force-nonobservable policy is true for the cold raw3 route; a caller boolean does not replace that policy here. Runtime/run/model handle leases remain the frozen tensor-state implementation's leases.

The initial prefix generation contains one active empty primary and secondary vector. Each requested rank's separate I32 ancestry vector starts with explicit `-1`. BOS0 remains the initial NN target; it is not inserted into either U16 prefix list. No external prefix import is performed.

Every pending prefix Step retains the exact tensor Step and immutable generation identity. Candidate creation uses the actual current offset, batch, carried prior costs and root `LegacyPortableMathLogProbabilities` producer once. It derives:

- information qword `10`: first strict maximum of the same Step's unmasked D8 log-probability row, including BOS0 in that scan;
- conditional U32 `18`: `FFFFFFFF`, because the current decoder has no `detection_output`;
- offered cumulative costs: the E8 channel with the actual BOS0 mask applied **after** the separate D8 argmax.

Argmax starts at negative finite F32 maximum with index `-1`; unordered values do not win, ties keep the earlier index, and an all-minimum/unordered row retains `-1`. The existing portable Exp/Log producer remains a managed CPU numerical adaptation. No equivalence with native CPU transcendental dispatch or floating-point exception mode is claimed.

The same costs/sidecars pass through the existing full48-byte heap, raw sorter and delta/EOS handoff. That handoff receives the same generation's old primary prefix for each active parent. Selection retains its private source-Step identity; equal shapes or a copied row pool cannot substitute a foreign context. Selection data returned to callers is copied. A separate diagnostic parent/label selection seam keeps actual same-Step costs and sidecars but explicitly marks its selected coordinates as prepared.

The candidate object also exposes copied completed-score facts. These are numerical EOS eligibility/score observations, not owning `037430` completed records or final hypotheses. A follow-up can attach an owning event lease before Apply, using the retained Step, selected rank, old primary/secondary vectors and ancestry. This batch does not invent that completed-record owner.

## Prefix update before tensor update

Applying a selection validates current context/Step/generation, width and row ownership before it consumes the operation. For each active selected row, it copies `oldPrimary[parent]`, then appends lowU16 of qword `20`, or qword `08` if the whole alternate qword equals `-1`. It copies `oldSecondary[parent]`, then appends lowU16 of qword `10`. Inactive ranks get empty primary/secondary vectors.

Ancestry is different: each **current rank's own time history** appends the selected I32 parent, or `-1` for inactive ranks. It is not gathered from the old parent. The prepared pure-row tests include a non-`-1` alternate whose lowU16 is `FFFF`, to distinguish the full-qword sentinel check from truncation. Those tests do not establish a configured-alias inference producer.

The owned prefix generation is installed before the existing tensor state's `ApplySelected` call, matching the native phase order. The tensor call consumes exactly the same selected48 rows and applies the checked parent/static maps, attention concatenation/force mask, KV gathers and offset/prior-cost update. After success the prefix and tensor offsets/batch counts must agree.

Entry cancellation and pure foreign/stale/shape preconditions reject before consuming a valid pending generation. Any exception or cancellation after the consuming update begins makes the combined context unusable. If a fault occurs after prefix installation, copied failure diagnostics retain that newer prefix generation even though tensor application did not complete. Disposal still releases all owned tensors and leases. This is a copy-based managed adaptation; native allocation order, leaks and in-place partial-allocation effects are not emulated, and no rollback is promised.

The context serializes step, candidate, update and disposal operations. Readback arrays are owned copies. A 1000-update storage limit is an explicit implementation bound, not a restored native maxstep calculation. The exact input-packet `N+4*K` producer and native fallback/termination behavior remain separate.

## Verification

A procedural two-stroke fixture supplies actual model input without expected labels. Managed and fresh Windows NativeAOT execute 20 captured encoder-backed decoder steps:

- three five-step sequences through actual logits -> derived sidecars -> heap/EOS -> same-Step selection -> prefixes and tensor update, with prepared `(width,delta)` values `(1,-1)`, `(3,1000)` and `(3,-1)`;
- five separate explicitly diagnostic parent/label selections over actual NN Steps, including expansion, repeated parents `[2,0,2]`, shrink and permutation;
- 320 prepared argmax vectors, four prepared raw-row prefix trajectories, and 118 ownership/lifecycle/cancellation guards.

The five-step sequences use a fixed **prepared continuation schedule**, including continuations after an all-FMAX handoff. They are not a claim that the native outer scheduler would run every one of those calls. This lets the per-step lifecycle be tested without fabricating the missing termination/fallback owner. The ordinary width3/large-delta sequence also supplies finite non-EOS continuations after EOS observations.

The independent Python/NumPy oracle verifies **1,828,524 exact tensor/input/cache bytes**, complete heap/sort/EOS raw48 storage, actual D8 argmax/conditional sidecars, both U16 prefix planes, rank/time ancestry and the retained failure-prefix writes. It verifies 11 numerical EOS completed-score facts against the exact old primary prefix and F32 divisor. Portable scoring is independently rounded to F32 with a 4e-6 tolerance; observed maximum error is 0. Managed and Native captures are byte-identical, SHA-256 `d350e2ca63956bc524936c40c60664c173130d4c1216c1850ed930c3ca5b3d08`.

The old tensor-state capture remains `81aa9e5b9cbb6a167bfce9e3684d819575ac403088df1030223a680aca0aacf3`, with its14-step independent audit. The old cold-bundle capture remains `44f43ce023af50dd46dca32664ee479673e26a91378014edb399a3dab7a4bc51`. The current old candidate/score-channel capture remains `52dba125a1e40368ca3fb6db76c60d2a267d4f0f9470f96d8b5dde2117636b20`.

Reproduce with the existing export and official pinned CPU ORT1.23.2 package; no vendor SDK or MyScriptMLOrt is executed:

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet run --project tools/LegacyMathPrefixState.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/joined-math2-prefix-build -- E:/BlurMask-build-artifacts-20261014/joined-math2-state-artifacts/model.ikrmath models/math2/math2-unwrapped.res E:/BlurMask-build-artifacts-20261014/joined-math2-state-artifacts/runtimes/win-x64/native/onnxruntime.dll E:/BlurMask-build-artifacts-20261014/joined-math2-prefix-managed.json
python tools/LegacyMathPrefixState.Probe/audit_output.py E:/BlurMask-build-artifacts-20261014/joined-math2-prefix-managed.json models/math2/math2-unwrapped.res
```

Static prerequisites are the [prefix-plane producer](legacy-math2-prefix-planes.md), [force policy](legacy-math2-force-policy-producer.md), [tensor lifecycle](legacy-math2-tensor-lifecycle.md), and existing prepared heap/EOS contracts. The new runtime file is additive; frozen tensor state, OrtRuntime, prepared candidate kernels, application and CI files are unchanged.
