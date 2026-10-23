# Math AK integer-symbol sequence primitive вЂ” source checkpoint

`LegacyMathAkSymbolSequence` is a reusable model reader and next-symbol
distribution API for the fixed resource
[`models/math/math-ak.res`](../models/math/math-ak.res). It does not change
application routing. The existing `NativeNetwork` numerical mode remains the
default; only this new reader selects the separate scalar policy.

## Connected implemented source

`LegacyMathAkSymbolSequence.Read` snapshots the exact 3,685,881-byte resource, verifies its complete SHA before reading fields, then owns one actual record's thirteen FP32 tensor blocks and its 290-entry ordinal mapping. This is an explicitly fixed-resource reader, not a new generic model converter. The resource has two real 5F106 networks: kind1 forward and kind2 reversed interior. Both have input lookup241x50 (stored241x56), a unidirectional kind3 LSTM100, and output241. No English resource identity or context is coerced into this owner.

`Prepare` accepts **legacy symbol ordinals**, not Unicode, strokes or caller-selected model output. It owns `[240, mapped ordinals, 240]`; kind2 reverses only the interior. There are n+2 IDs, n+1 input frames, and n+1 next-position targets. The last sentinel is target-only. Maximum4095 source symbols is a new bounded library subset, not a recovered native universal limit. Empty input prepares one boundary frame/target; admitting empty recognition candidates is outside this API.

`Evaluate` accepts only its same model's typed prepared owner, copies the actual50 embedding channels per input frame and invokes the existing owned managed LSTM. Results expose scalar reads/defensive copies; neither arrays nor ReadOnlyMemory escape. Every call has private scratch, allowing concurrent immutable model reuse. There is no disposal-lifetime claim: managed immutable weights remain with the owner and prepared/result references. Snapshot buffers are cleared in finally; cancellation can interrupt tensor decoding, preparation and evaluation, with no partially published model/result.

The optional internal `SymbolSequenceScalar` policy changes only sequential FP32 softmax summation and removes the old artificial sigmoid clamp. All existing constructors still select `ExistingPortable`; their previous reduction and sigmoid remain unchanged. Native199470 accumulates exponentials sequentially; native199280 directly computes1/(1+exp(-x)). This is a portable scalar policy, **not** a complete native numerical-equivalence certificate: CRT exp/tanh approximations, MXCSR FTZ/DAZ, native SIMD dispatch/reduction, NaN payloads and FP status flags remain excluded. Native109DE0 explicitly sets MXCSR; the helper does not emulate it. Nonfinite managed recurrent state/invalid softmax retain the existing explicit refusal.

## Native relationships

Math00F3C0 is a post-produced-candidate consumer. It maps actual source ordinals, appends sentinel240, passes only n+1 integer frames to Engine primaryV10=109DE0, and reads n+1 output rows through VA8=10DAC0. It selects each row's probability using the **next** ID. Engine17E660 embeds IDs using lookup base+288 and padded stride+280, appends bias, and dispatches kind3 to181980. Engine106DD0 supplies the FP32 input gate g/i/f/o matrices, recurrent gate g/i/f/o matrices and i/f/o peepholes, output matrix, then lookup. Initializer10E410 supplies rows/columns/stride and activation dispatch. The exact original tables/V10/VA8 and Math caller assembly already have main's `audit_math_legacy_ak_operator_routing.py` certificate. New extracted complete decompiler bodies are pinned separately; copied C text alone is not fresh assembly proof of every transitive helper.

Mapping offsets592004/1004804 follow a **four-byte1160 length word** at592000/1004800. Parameter payloads end before this word. The parser's payload cursor bound and mapping offsets are distinct. The pure-auditor drafts initially assumed a short envelope header and adjacency without that length word; both failed before reporting success, were corrected against raw bytes, and are preserved as failed source evidence. Production source used the correct independent offsets and was not modified by those diagnostic fixes.

## Bounds and work scope

All model counts and resource bytes are authenticated/fixed before tensor copies. Source ordinal count is admitted before snapshot, every ordinal validated before ID allocation, and frame widths are checked by the existing executor. At4096 frames, input floats204800, hidden floats409600, output floats987136. The fixed tensor payload is411488B (retained peepholes omit48B padding); ordinal map1160B. Source IDs require16388B and the temporary ordinal copy at most16380B. Feature+hidden+output floats total6,406,144B. Private gate/state/scratch arrays, model and IDs remain below a conservative8MiB **logical payload** per single evaluate path, excluding independently requested defensive copies, allocator/object headers, JIT, caller concurrency and RSS. Read additionally snapshots3,685,881B. These are cardinalities, not CPU or latency guarantees.

The fixed scalar affine multiply-add loop count is at most364,764,544:4096*4*100*56 +4095*4*100*104 +4096*241*104. Activation/nonlinear and loop overhead are additional. No work-budget setting or implicit recognition admission is introduced. Cancellation checks use the existing per-frame/per-row boundaries; SHA itself is one bounded, non-interruptible managed call.

## Evidence and remaining boundary

`audit_raw_sequence.py REPO OUTPUT` uses the versioned parsing-only helper
`tools/handwriting/native_network_reference.py` to partition all26 tensor
blocks, validate both mappings/padding and check ten prepared sequences,
including all290 ordinals, empty input and4095 symbols. It needs no ignored
research directory and performs zero model forwards. Prepared-mode C# checks
passed29 guards and7 negative controls. Synthetic transcendental comparisons
allow one ULP for the tested MathF/NumPy values; they do not establish native
CRT equivalence. Existing synthetic float/int16/int8 BLSTM goldens still pass
(max error4.4703e-08), including parallel reuse and85 CVTT controls.

Both real records were separately evaluated on source ordinals `[0,1,2]`:
four input frames and964 outputs per record. All1928 probabilities were finite
and normalized; input/next-target IDs matched the independently read mappings.
An independent NumPy LSTM with unclamped sigmoid and sequential softmax agreed
within absolute+relative tolerance (3e-7 +1e-5*reference). Maximum absolute
differences were7.45058e-7 and5.66244e-7; all eight row argmaxes agreed. Its
BLAS affine reductions differ from the managed scalar reductions. This is a
short numerical diagnostic, not native bit equality or recognition quality.

```sh
dotnet run --project tools/LegacyMathSymbolSequence.Probe -c Release -- models/math/math-ak.res prepared.json --prepared
python -I -B tools/LegacyMathSymbolSequence.Probe/audit_prepared.py models/math/math-ak.res prepared.json prepared-audit.json
python -I -B tools/LegacyMathSymbolSequence.Probe/audit_raw_sequence.py . raw-audit.json
```

The separate `--actual` mode runs both real models on the fixed short sequence;
outputs must use fresh paths. CI runs the prepared/raw controls.

The owned numerical cost API is described in
[`legacy-math-symbol-sequence-rescore.md`](legacy-math-symbol-sequence-rescore.md).
Actual candidate ownership from00D9F0, the grammar/search pipeline and integration with
modern Math2/AUTO remain incomplete. The primitive returns distributions and
numerical costs for explicit prepared symbol sequences, not OCR confidence or
an application-ready recognition result.

See [the original Math recognition frontier](legacy-math-recognition-frontier.md)
for the distinct acoustic, grammar and matched-tree producers still required.
