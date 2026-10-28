# Math2 selected-prefix and per-beam metadata producers

The unrestricted `038610` branch has three distinct U16 prefix planes and a separate I32 parent-history table. The primary plane follows the selected candidate label or its alternate. The secondary plane follows a **per-beam argmax of log probabilities**, rather than the selected candidate label. An optional third plane follows detection metadata. The actual raw3 bundle has no detection output, so that third plane is absent and candidate `18` is `FFFFFFFF`.

This audit supplies the missing producers for the currently explicit prepared sidecars. It does not run a vendor DLL, execute a network, implement a search loop, or turn the prepared sidecars into actual inference data.

## Bound input and fresh seed

All addresses below belong to the pinned Document PE, SHA-256 `17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f`. The auditor composes the existing tensor-lifecycle audit for the three actual ORTM model fingerprints, their IO names, and the default allocator proof. The allocator conclusion is restricted to the standard Engine V18 route ending in Windows `HeapAlloc` with flag8. Custom allocation is excluded.

`038610` allocates two pairs of width-sized descriptor arrays: one current/scratch pair for primary U16 vectors, and one pair for secondary vectors (`038974..038A05`). A descriptor is 16 bytes: pointer at0, U32 count at8 and U32 capacity atC. Their initial empty state follows the default V18 zero-fill contract. There is no BOS token appended to these prefix vectors. The NN target tensor's initial BOS0 is a separate object and contract.

The optional auxiliary pair is allocated only if signed EEE `3C >= 0` (`038A21..038A2D`). This field is the detection-output index:

| Field chain | Producer |
| --- | --- |
| EF1 `120` | Reset to `-1` by `04A1F0`; `049630` changes it only for decoder output `detection_output` (`04A0C8..04A0F3`). |
| EED `9C` | Factory reads EF1 `120` at `045289` and writes EED `9C` at `04538C`. |
| EEE `3C` | EEE copies the packet at EED `70` into instance `10`; packet `2C` therefore becomes instance `3C`. |

The actual decoder outputs are `reco_output`, `attn_seg`, `state_out_k`, and `state_out_v`. Thus current fresh EF1 `120`, EED `9C`, and EEE `3C` remain `-1`. Do not confuse this field with **EF1 `D4`**, the encoder `max_pred_counts` index used for K, or **EF1 `D8`**, the optional `classification_output` index forwarded to EEE `4C`. The actual encoder has `max_pred_counts`; it has no `classification_output`.

Initial active counts are explicitly one: EEE `98=1` in `03A260` and EEF `110=1` in `03BD40`. EEF prior cost `F0[0]=0` is explicitly written at `03BE31`. Unused capacity slots are not extra active beams.

The parent-history table at EEE `88` is distinct from all U16 prefix planes. Its allocation has a real prerequisite: `035D60` allocates width×16-byte descriptors if EEE `38 != 0` or EEE `5C > 0`, then explicitly clears each descriptor. The current decoder's `attn_seg` sets EF1 `40=1` (`04A137`), which factory `0450C0` forwards through EED `98` to EEE `38`. Hence the actual bundle takes that allocation branch. `03A260` clears its counts before the ordinary driver call; `038610` appends **I32 `FFFFFFFF`** as each rank's initial ancestry item (`038B32..038B38`). These seed values are explicit writes, not an assumption about allocator contents.

The empty U16-prefix conclusion requires that the driver does **not import an external prefix**. Its separate `037790` branch can populate those lists when its prefix input and EEE flags permit. This audit does not prove that every external caller always omits such an input, or implement that import route.

## Actual per-step sidecars

EEF `03CAC0` returns two separate score pointers: its `D8` log-probability rows and `E8` cumulative-cost rows (`03D1BA..03D1D5`). `038610` uses the latter for heap offers. It calls `036250` on the **former** once per active beam at `039119` and stores its returned I64 ordinal into every admitted candidate's `10` field (`03918E`). This is data from the same beam and step. It cannot be replaced by a constant zero or inferred from the chosen candidate's symbol.

`036250` uses the vocabulary count from EEE `18`, whose low I32 is sign-extended for the scan bound. It starts with best value negative F32 maximum and index `-1`. Its unrolled and tail `COMISS/JBE/CMOVBE` comparisons preserve the first strictly greater value, skipping unordered values. Consequently ties retain the earlier index; an all-negative-maximum or all-unordered row can retain `-1`. The pinned initial literal is F32 bits `FF7FFFFF`. A conventional argmax initialized to index0 would differ on those boundary rows.

For the current raw3 bundle, EEF has no detection tensor, so candidate `18` is written with **U32 `FFFFFFFF`** (`039121`, `039192`). If EEE `3C` were nonnegative, the driver would instead compare two coordinates from that optional output and write0/1 (`03912C..039150`). That other-model branch is pinned but not supplied with a fabricated actual tensor. Candidate `1C..1F` remains the heap helper's initialized padding.

The primary symbol at candidate `08` is the offered vocabulary ordinal. In the unrestricted branch candidate `20` is explicitly qword `FFFFFFFFFFFFFFFF`; configured alias expansion can provide another value and remains separate. BOS0 masking applies to cumulative costs in EEF `E8`; it does not turn the independent `D8` argmax into a chosen non-BOS label.

## Selected update and ownership

For every active selected raw48 record, `038610` uses **signed I32 parent ordinal at `28`** (`03958D..039592`). It clears all scratch-vector counts across the requested width, then processes only active selected rows. The source arrays remain the previous generation during all copies; repeated parents therefore read the same previous prefix.

| Destination plane | Exact operation |
| --- | --- |
| Primary | Copy `oldPrimary[parent]`; append lowU16 of qword `20`, unless the **whole qword** equals `-1`, in which case append lowU16 of qword `08`. |
| Secondary | Copy `oldSecondary[parent]`; append lowU16 of qword `10`. |
| Auxiliary, if present | Copy `oldAuxiliary[parent]`; append lowU16 of U32 `18`. |

The alternate test is at `0395AD..0395B8`. A value whose lowU16 is `FFFF`, but whose whole qword is not `-1`, is a real alternate and does not fall back. The secondary read is `0395BD`; the auxiliary read is `0396BD`.

`033DE0` appends the previous U16 sequence and advances its count, growing capacity as needed. `033D60` reserves the additional scalar, which the driver writes before incrementing the count again. All three planes use this same helper pair. The plane pointers swap at `03973D..03976C` after the selected-copy phase. Allocation failure can leave partially modified scratch vectors and take the native cleanup path; this is not transactional rollback.

The separate EEE `88` ancestry vectors are indexed by **current rank over time**, not by copying the old parent vector. After the plane swap, every rank receives the selected I32 parent at that step, or `-1` for an inactive rank (`0397B2..0397B8`). This retains the backtracking relation across rank permutations. It must not be replaced by a flat sequence of chosen symbols.

Finally EEF `V40=03DA20` receives the **same raw48 candidate owner**. It copies candidate costs into prior costs, invokes EF1 selected-parent state/attention reindexing, and records selected lowU16 labels. The prefix update and tensor-state update must consume one selection together; shape equality alone does not establish this ownership. Failure after either partial update must render a managed combined session unusable rather than resuming with an old prefix generation.

The driver also uses the secondary plane to compare common-prefix positions and the primary plane for the ANY-observable EOS test. Neither plane can be reconstructed later from only the final chosen text.

## Verification and bounded next join

```powershell
python tools/handwriting/audit_math2_prefix_planes.py E:/BlurMask-build-artifacts-20261014/joined-math2-prefix-planes/report.json
```

The auditor verifies 12 complete PE regions, 116 instruction pins, five vtable bindings, the actual IO names and allocator dependency. It checks 19,607 prepared comparison vectors, including ties, negative maximum, infinities and NaNs; four prepared multi-plane updates expose repeated parents, shrink/permutation, alternate sentinel width, and U16 truncation. These are independent static/layout discriminators, not captured native execution or recognition results.

A coherent next runtime block can be new files only: a same-Step sidecar factory derives `10` from that Step's retained log-probability rows and `18=FFFFFFFF` from the exact no-detection model binding; an owned prefix context starts with one active empty row and consumes a state-owned selected-row lease. It creates fresh copied primary/secondary vectors and rank/time ancestry, validates the exact Step/selection generation, rejects foreign or replayed selections, and becomes unusable on a failed combined update. The prepared seam remains explicit for multi-parent controls. Any optional detection or imported-prefix variant must refuse until its producer is bound.

This leaves the actual maximum-step input-packet dimension `N`, `4*N+1` bound and encoder `K` layout, completed168 ownership/recording, configured alias expansion, fallback/output selection, and whole search-loop lifecycle as separate work. Cold requested width1 still does not justify a greedy first-EOS stop, native CPU exp/log equivalence, or recognition confidence.
