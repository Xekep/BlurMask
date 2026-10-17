# Math2 maximum-step and temperature producers

The native cold Math2 path does not use a fixed 128-step loop. It derives a maximum position from EED configuration, the actual input tensor dimension, and an encoder-produced `max_pred_counts` value. Its temperature is carried through the same EED -> EEE -> EEF ownership chain as the score model. This audit establishes those producers and consumers without running inference or implementing the search loop.

## Fresh defaults and inherited state

All native addresses below refer to `MyScriptDocument.dll`, image base `0x180000000`.

| EED field | Cold constructor `044080` | Configuration producer | Consumer field |
| --- | --- | --- | --- |
| `70` | Signed DWORD `1000` | Retained by EED copy/raw-input packets; no maximum-step JSON writer was found in the inspected initializer/callback route | EEE `10` |
| `74` | F32 `4.0`, bits `40800000` | Model JSON `max_len_factor`, callback `044AC0`, store `044C57` | EEE `14` |
| `D0` | F32 `1.0`, bits `3F800000` | Reset to `1.0` by `045460` before model metadata; conditional model JSON `model_temperature`, store `044E9F` | EEF `20` |

The model callback frame points to EED `68`. Consequently, callback stores at frame `+0xC` and `+0x68` write EED `74` and `D0`. The temperature branch is guarded by frame qword `+0x108`, which is EED `170`, the optional fourth model. A zero optional model skips that branch. `max_len_factor` is outside that guard.

The actual resource has three model blobs and no fourth model. Its decoder metadata sets `max_len_factor = 4`; none of the three model metadata maps contains `model_temperature`. Cold initialization therefore produces factor `4` and temperature `1`. Request configuration in the inspected `045460` route handles other named settings; it has no `max_len_factor` or `model_temperature` dispatch. This is not an inventory of every external host mutation of a prepared source.

EED copy constructor `044180` copies scalar state and can retain an existing EED `220` decoder. Raw initializer `044630` copies the `0x170`-byte scalar payload into EED `68..1D7`, including these fields. Later `045460` resets the EED temperature before metadata, but it skips decoder reconstruction when `220` is nonnull. The retained EEE/EEF child may therefore keep previously captured fields. A new wrapper around a warm source does not prove cold defaults reached that child.

## Maximum position passed to the decoder

EEE `03A260` sign-extends the DWORD at `EEE10`, loads the F32 factor at `EEE14`, converts that factor to an integer **before** multiplying, then reads the first input tensor's field `+0x10`. In the ordinary finite nonnegative domain this is:

```text
base = unsigned_min(sign_extend(EEE10), uint64(truncate(EEE14)) * N + 1)
```

The exact source of `N` in `03A260` is `*(uint64 *)(*(uint64 *)(inputPacket + 8) + 0x10)`. It is a dimension of the native first input tensor, not an original stroke or raw-point count. The multiply and increment are native 64-bit operations. The full unsigned-conversion sequence, including its `2^63` adjustment and `CVTTSS2SI`, is pinned. This audit does not provide a portable general conversion policy for NaN, infinity, negative factors, or overflow.

For the cold values, the base is `min(1000, 4*N + 1)`. Truncation order matters: with factor `4.5` and `N = 10`, native conversion-before-multiplication gives `41`; multiplying in F32 first and then truncating would give `46`.

The base is not always the final maximum. EEE calls its actual B8 encoder's `V18 = 04A640` with a fifth output pointer. The assembly preserves that pointer through `04A640` into `04A4B0`. The latter validates the configured `max_pred_counts` output as a one-element INT64 tensor and writes its qword payload through that pointer. EEE uses a nonzero returned value as another unsigned upper bound. A zero value selects the separate optional-output path instead.

The data-only FlatBuffer audit reads the actual encoder Graph outputs:

```text
output, output_len, output_seg, counting_output, max_pred_counts
```

`049630` records `max_pred_counts` at EF1 `D4`; its actual output ordinal is 4. The optional `max_pred_len` output is absent, so the inspected cold route leaves that index at `-1` and EEE's corresponding optional index `40` disabled. Other models can expose it: EEE then has additional typed-tensor and count-based bounds, with its own validation and rounding. Those branches are retained as distinct prerequisites, not silently treated as zero.

Thus, for the fingerprinted cold model and ordinary input domain:

```text
maximumPosition = min(1000, 4*N + 1)
if encoder max_pred_counts K != 0:
    maximumPosition = unsigned_min(maximumPosition, K)
```

`K` is computed during encoder inference. Its actual value cannot be obtained from the metadata or output name alone. This audit does not substitute a constant for it.

## Driver termination and completed hypotheses

EEE passes the selected maximum as the fifth argument to `038610` (`03A91B..03A920`). In the main driver loop, the previous position is compared to that unsigned maximum before the next score callback (`038CCD..038CE5`). A prior-prefix path can initialize the position above zero, so this field is an absolute bound on that counter rather than always a count of newly generated tokens.

If supplied, the progress callback receives the incremented position and maximum. Returning false enters the explicit error path (`4000005`). Score callbacks, allocations, and later decoder callbacks can also fail; reaching the numeric maximum alone is not such a callback failure. The last-step path can attempt to collect a completed hypothesis when no completion has yet been stored.

EOS handling is not a first-EOS stop. The driver examines candidate prefixes, and any observable prefix leaf can trigger `037430` completed-hypothesis collection; structural tokens remain valid parts of those prefixes. Collected hypotheses are length-normalized and retained while non-EOS candidates can continue. The [EF3 classification](legacy-math2-observable-token-gate.md) and [restriction-map provenance](legacy-math2-restriction-map.md) do not replace that consumer. The native completed-hypothesis ranking, all exit paths, prefix ingestion, and optional model branches are not implemented here.

## Temperature application

EED `D0` reaches EEF `20` through the EEE raw packet. `03C0D0` performs `DIVSS` on `1.0f / EEF20`, then passes that F32 result to Engine service `54.V48` for log-softmax. It is not double-precision division. The cold factor is exactly `1.0f`; a warm child or enabled optional-model configuration requires its own proven producer values.

There is no positive-finite temperature guard at this division site. No additional acceptance or clamping policy is introduced by the audit. The Engine CPU exp/log dispatch remains unported; proving its input scale is not proof that a substitute numerical implementation is native-equivalent.

## Reproduction

```powershell
python tools/handwriting/audit_math2_step_temperature.py E:/BlurMask-build-artifacts-20261014/joined-math2-step-temperature-static.json
```

The audit checks thirteen whole regions, 55 instruction pins, the concrete encoder service binding, exact configuration strings and F32 constants, actual graph input/output vectors, and the raw resource fingerprint. It composes the existing bounded metadata audit, including its model-blob hashes. Six simple expression checks and a fractional-factor counterexample expose the order of arithmetic; they are static checks rather than native execution. No runtime or CI files change, and no vendor SDK, ONNX Runtime session, GUI, or inference is executed.
