# Math2 EF1 mode0 tensor and cache lifecycle

This is a static producer/consumer audit of Document EF1 for the three checked Math2 ORTM blobs. It adds no decoder runtime and executes neither the SDK nor ORT. The reproducible auditor is `tools/handwriting/audit_math2_tensor_lifecycle.py`; its report contains 21 hashed PE regions, 124 instruction pins, 13 concrete dispatch/data bindings, all 27 actual graph input/output specifications, and four explicitly prepared coordinate controls.

The checked resource SHA256 is `2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a`. Graph dimensions containing strings are symbolic shape metadata. They do not establish the requested beam width or a numerical encoder output length. The width, step-limit, restriction-map and score-channel audits remain separate prerequisites.

## Concrete callback owners

EF1 runtime vtable `0x1801FD150` binds `+18=04A640`, `+20=04A710`, `+28=04B9E0`, and `+30=04C930`. Its model owners at `+10/+18/+20` are encoder/initializer/decoder. EEE's `owner80` is EEF; it is not EF1. EEF holds the selected EF1 at `B8`, retained at the same pointer by the default Engine service; the [concrete ownership proof](legacy-math2-force-policy-producer.md) pins that service. EEF table `1EDA20` binds `V28=03BD40` and `V40=03DA20`.

`03BD40` calls EF1 `V20`, creating the decoder input tuple. `03DA20` consumes the selected 48-byte candidate rows, copies their cost into EEF's prior-cost vector and calls EF1 `V30`. The actual parent ordinal is the signed I32 at candidate `+28`; the next target is the qword at `+8`. EEF subsequently records the low 16 bits of that target in its separate selected-label array. Those are related representations with different widths.

`04A640` and `04B9E0` call each model's `V58`, `V40(input tuple)`, `V48(0,0)`, then `V50(output tuple)`. Decoder output kind 2 selects `reco_output` and optional `attn_seg` by the indices below. The single-tensor output alternative is accepted only when the EF1 attention-output flag is false; it is not the checked current model route.

## Graph-name/index binding

`049630` enumerates each actual model's names and fills these EF1 I32 fields. Unavailable names retain `-1` from `04A1F0`.

| Model | Fields and names |
| --- | --- |
| Encoder inputs | B0 `src`, B4 `src_len`, B8 `downsampling`, BC `strk_ends` |
| Encoder outputs | C0 `output`, C4 `output_len`, C8 `output_seg`, CC `output_seg_len`, D0 `output_counts`/`counting_output`, D4 `max_pred_counts`, D8 optional classification output |
| Initializer inputs | DC `memory`, E0 `batch_size`, E4 `memory_seg` |
| Initializer outputs | E8 `cross_k`, EC `cross_v`, F0 `proj_key_seg_in` |
| Decoder inputs | F4 `tgt`, F8 `offset`, FC `memory`, 100 `memory_seg`, 104 `prev_attn_seg`, 108 `proj_key_seg_in`, 10C/110 `state_in_k/v`, 114/118 `cross_k/v` |
| Decoder outputs | 11C `reco_output`, 120 optional `detection_output`, 124 `attn_seg`, 128/12C `state_out_k/v` |

The checked graphs have no `src_len`, `output_seg_len`, decoder `memory`, classification or detection output. They do have segmented memory, previous attention, projected keys, and KV caches. Enumeration sets flag41 for `output_seg`, flag42 for `prev_attn_seg`, flag43 for `proj_key_seg_in`, flag44 for `state_in_k`, and flag40 for `attn_seg`. The current branch has all those flags set. This is name-driven dispatch; it must not be inferred from a hard-coded output count alone.

## Actual model shapes

The auditor reads ORTM FlatBuffer bytes directly with bounded accesses and fingerprints every full blob. It imports no generated ORT module. ORTM element type 1 means F32 and 7 means INT64; the SDK tensor enum uses 7 and 0xB respectively.

| Graph port | Actual type and shape |
| --- | --- |
| Encoder `src` | F32 `[max_src_len,1,26]` |
| Encoder `downsampling` | I64 `[max_src_len,1]` |
| Encoder `strk_ends` | I64 `[1,max_strk_count]` |
| Encoder `output` | F32 `[max_out_len,Addoutput_dim_1,256]` |
| Encoder `output_seg` | F32 `[max_out_seg_len,LayerNormalizationoutput_seg_dim_1,LayerNormalizationoutput_seg_dim_2]` |
| Encoder `output_len` | I64 `[Castoutput_len_dim_0]` |
| Encoder `counting_output` | F32 `[LayerNormalizationoutput_seg_dim_1,1]` |
| Encoder `max_pred_counts` | I64 `[LayerNormalizationoutput_seg_dim_1]` |
| Initializer `memory` | F32 `[enc_mem_len,beam_size,256]` |
| Initializer `batch_size` | **F32** `[beam_size]` |
| Initializer `memory_seg` | F32 `[enc_mem_seg_len,beam_size,256]` |
| Initializer `cross_k/v` | F32 `[2,beam_x_head,enc_mem_len,Concatcross_*_dim_3]` |
| Initializer `proj_key_seg_in` | F32 `[beam_size,enc_mem_seg_len,256]` |
| Decoder `tgt` | I64 `[label_size,beam_size]` |
| Decoder `offset` | I64 `[1]` |
| Decoder `memory_seg` | F32 `[enc_mem_seg_len,beam_size,256]` |
| Decoder `prev_attn_seg` | F32 `[enc_mem_seg_len,beam_size,prev_label_size]` |
| Decoder `proj_key_seg_in` | F32 `[beam_size,enc_mem_seg_len,256]` |
| Decoder `state_in_k/v` | F32 `[2,beam_x_head,offset,64]` |
| Decoder `cross_k/v` | F32 `[2,beam_x_head,enc_mem_len,64]` |
| Decoder `reco_output` | F32 `[label_size,beam_size,297]` |
| Decoder `attn_seg` | F32 `[enc_mem_seg_len,beam_size,label_size]` |
| Decoder `state_out_k/v` | F32 `[2,beam_x_head,tot_label_size,64]` |

The runtime encoder lengths/counts require inference. This audit does not replace any of them with raw point/stroke count or a symbolic dimension string. The model config declares `num_heads=4`; the general cache code obtains heads by dividing the actual flattened dimension by EF1's previous beam count. A future adapter must validate the configured/runtime shape relationship rather than fix the general code to four.

## Initializer and first decoder input

`04A710` first sets EF1 `130=1` and allocates the decoder input array as 0x110-byte SDK tensor descriptors. The no-prefix target is I64 `[1,1]`. A supplied prefix instead creates `[prefixCount+1,1]`, explicitly writes BOS 0, and widens each supplied U16 to I64. The checked offset route creates an I64 scalar vector `[1]`. Previous attention starts F32 `[S,1,0]`; state K/V start rank4 `[layers,heads,0,width]`. Their zero-element buffers are null. Projected keys are initially an empty rank3 placeholder.

The no-prefix target and offset have no explicit data write after `049360`. Their zero values are proven only for the default SDK allocator, rather than assumed from managed arrays:

1. Engine `033C70` selects configuration `252000` when its allocator argument is null, creates a root context with parent zero, and copies the callback block to context `48..70`.
2. `034010` installs the allocator descriptor `252030 -> 1DB660 -> table1DB5A0`; V18 is `037410`.
3. Root `037410` calls context `+50`, forwarding element count and size. Default `252008` is thunk `1BA950 -> 1C71C0`.
4. `1C7210` loads HeapAlloc flags 8 before the actual imported HeapAlloc call. Thus this default allocation is zero-filled. A custom allocator's callback is not covered.

The initializer's `batch_size` is a zero-filled F32 vector of length one under this same default contract, not an I64 scalar containing one. Its *shape* establishes the current batch extent. `04A710` transfers encoder `output` and `output_seg` into the initializer input tuple by swapping complete descriptors, executes the initializer, and consumes/releases those payloads with the temporary initializer inputs. The encoder output slots receive the previous empty descriptors; this route does not restore their original payloads afterward. Separately, decoder `memory_seg` is copied by `033ED0`, which allocates and copies payload/shape/strides; it is not an alias of that transient initializer input record.

Initializer output must have tuple kind2; cross K/V require rank4 and projected keys, when present, rank3. Full descriptor swaps transfer their allocated buffers into the decoder input slots. Temporary tuples then release the old empty descriptors. This is ownership movement, not copying an opaque pointer into two independently disposable objects. The model bridge and future ORT adapter must bind the destination port explicitly rather than treat a moved source descriptor name as a second owner.

## Selected-parent update

Let `B` be previous EF1 `130`, `N` the selected row count, `P[i]=I32(candidate[i]+28)`, and `Z[i]=i` when `i<B`, otherwise zero. `04C930` allocates both I64 maps. It checks `tgt` is SDK INT64, rank2, batch dimension B. There is no native ownership argument permitting a foreign row pool or a new beam context here.

For the checked offset model, target batch is resized to N, each qword candidate label becomes its next target, and the shared I64 offset increments by one. The no-offset route instead gathers full prefixes on axis1 and appends a time row; it is a separate model variant.

| Input carried to next step | Native action and map |
| --- | --- |
| `memory_seg` | axis1 gather/resize using Z |
| `prev_attn_seg` | **concatenate latest `attn_seg` on axis2**, then axis1 gather using P |
| `proj_key_seg_in` | axis0 gather/resize using Z |
| `state_out_k/v -> state_in_k/v` | reshape rank4 `[L,B*H,T,D]` to rank5 `[L,B,H,T,D]`; gather axis1 using P; reshape to `[L,N*H,T,D]`; move complete descriptors into input slots |
| `cross_k/v` | analogous rank5 expansion and axis1 gather using **Z**, not P; flatten again |

After the K/V descriptor swaps, `04D359..04D36E` exchanges the name pointers back separately; cache payload ownership moves while the destination/source names retain their prior identity.

Engine54 V68 is concretely `187440`, `cat_3D_dim2`, with type/rank3/equal other dimensions checks. It adds destination and source axis2 lengths. Treating that callback as ?copy latest attention? loses the attention history. Engine54 V70 is `1879A0`, the in-place index-permutation helper; V78 is `187FA0`, the resize helper. Document `049520` grows before gathering and shrinks afterward, so repeated parents and expansion must not be implemented as an unsafe forward in-place copy.

When EF1 `30` (`need_force_nonobs_positions`) is active, the EF3 predicate classifies each selected label. For a nonobservable selected label, the last attention-time column of every segment is overwritten with **F32 bits FF7FFFFF**. This is negative finite maximum, not negative infinity. Structural/nonobservable labels are still decoder labels; this write does not reject the entire prefix or treat every token as observable.

The four prepared coordinate controls distinguish parents `[2,0,2]` from Z `[0,1,2]`, shrinking, permutation, and initial duplication from B=1. They verify flatten/expand/gather coordinates and attention concatenation. They are not captured neural inference or execution of Engine tensor helpers.

## Failure and disposal boundaries

`049360` owns allocated data via descriptor byte64 and data pointer68; zero extents have null data and no owned-data flag. Native tuple cleanup frees an owned payload, its name, and tuple storage. EF1 `04A420` separately frees its six model-metadata arrays `50..A8` using `049180`, then delegates to EF0 destruction. The model metadata owner and live per-step tensor tuples must remain distinct.

Native preparation and update are not transactional: descriptor swaps, attention extension, partial cache changes and model/candidate updates can precede failure. `04C930` writes EF1 `130=N` on its cleanup exit even when an update failed; invalid target geometry can return before that cleanup. EEF `03DA20` likewise stores its new beam count and clears its selected-label count after a callback failure. Some early native error paths retain temporary allocations. This report does not promise native leak behavior or strong rollback. A managed port can dispose its own acquired handles safely, but must mark a failed recognition session unusable instead of continuing with old shapes after partial mutation.

## Next coherent OrtRuntime join

The existing `OrtRuntime` has owned output handles, borrowed run inputs, and constructors for F32/I64 tensors. Its F32 reader copies data and rejects nonfinite values; it has no I64 output reader or cache gather/view API. The current application greedy batch1/BOS0/128-step/first-EOS loop remains an independent algorithm.

A bounded next implementation can be an internal, single-recognition `Math2Mode0TensorState` with these operations:

- `Prepare(actual encoder output owner, checked model bundle, explicit/default-allocator policy)` validates the actual named outputs/ranks and INT64 `max_pred_counts`, runs the initializer once, and owns the current input state. Its first target/offset are justified by the default policy above. Arbitrary supplied prefixes or custom allocator residues require separate explicit coverage.
- `RunStep(run)` uses that same state and decoder session, returning owned named output tensors plus borrowed score tensor provenance. It does not independently choose a label or run a beam algorithm.
- `ApplySelected(actual prepared raw48 owner/count, EF3 owner)` validates P and N, performs the exact two distinct maps, attention concat/mask and KV reshape/gather, then transfers the output buffers. It must not substitute Z for P or rerun the encoder/initializer per selected child.
- `Dispose()` releases each unique handle exactly once and keeps model/runtime owners alive until dependent tensors are gone. A copy-based CPU implementation may reproduce numeric data while explicitly excluding native allocator/failure-order equivalence.

Before calling this a native decoder join, tests must use actual encoder/initializer/decoder outputs for multiple steps, an expansion B1->N>1, repeated parents and a shrink/permutation, with independent coordinate gathers, exact named input shapes, selected labels/offset and attention-tail writes. Preserve original raw48, completed168, delta pruning and ANY-observable EOS behavior. CPU exp/log dispatch equivalence, actual restriction maps, completed-candidate ownership and final structural acceptance remain separate work. This audit supplies the producer contract; it does not claim those stages are restored.

Reproduce from the repository root:

```powershell
python tools/handwriting/audit_math2_tensor_lifecycle.py E:/BlurMask-build-artifacts-20261014/joined-math2-tensor-lifecycle-static.json
```
