# Math2 force-nonobservable factory policy

For the pinned three-model Math2 bundle, a successful **cold mode0** preparation sets the active EF1 `need_force_nonobs_positions` byte to **true**. This follows the model metadata and factory packet, rather than the resource mode or a prepared caller flag. The standard Engine service retains the **same EF1 pointer** in EEF `B8`; it does not clone the decoder at that handoff.

This is a static producer proof. It adds no runtime policy factory, search loop, native execution, or acceptance behavior. `Math2Mode0PreparedPolicy` remains an explicitly prepared policy until a separate owning bundle factory consumes this contract.

## Provenance and precedence

The Document PE is SHA-256 `17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f`; Engine is `d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c`. The resource is `models/math2/math2-unwrapped.res`, SHA-256 `2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a`. The auditor composes the existing raw ORTM metadata reader and checks all three models, their hashes, and resource mode0. Tensor dimension names are not configuration evidence.

1. EED constructor `044080` clears `68..1D8` (`044125..044136`) and clears its child pointer `220`. Thus bare EED `188` begins zero. Raw constructor `044630` first calls this constructor and then copies the raw packet's `18..188` into EED `68..1D8`; inherited packet values cannot be treated as cold defaults.
2. In `045460`, the four metadata owners are collected in the order EED `1C0`, `1C8`, `1D0`, `170`. Before their configuration callbacks, **mode0** resets EED `180=4` and byte `188=0` (`045587..04559B`). This reset, rather than an inherited raw byte, supplies the initial value for this preparation.
3. Each present model's metadata is enumerated with callback `044FB0`. The callback frame is explicitly `{JSON service, EED+68}` (`0455EA..045618`). `044FB0` requires the `config` property to be a string, parses that JSON, and enumerates it through `044AC0`, forwarding the original frame (`04504C..045069`). A failed preparation is outside the conclusion.
4. `044AC0` recognizes the literal `need_force_nonobs_positions` (`044BCF`), obtains the JSON boolean through service `V70` (`044C03..044C07`), and writes **frame+120** (`044C0A`). Since the frame base is EED `68`, the destination is **EED `188`**, not EED `120`. The adjacent `num_heads` write is frame `118` -> EED `180`.
5. The actual encoder and initializer configs omit this boolean. The decoder, slot2, supplies `need_force_nonobs_positions: true` and `num_heads: 4`. Slot3 is absent. Ordered metadata processing therefore ends with EED `188=1`. Later configs could overwrite an earlier value in other bundles.
6. The subsequently inspected request-config block handles only `disable_iso_pass`, `beam_size`, `language_model.alpha`, `language_model.beta`, and `delta_cost_max`. It does not supply a force override. This does not establish the behavior of arbitrary external property setters or replacement metadata owners.

`045460` calls factory `0450C0` only when EED `220` is null (`045938..04594A`). A warm EED can refresh its outer metadata byte while retaining an existing child; the outer byte alone then says nothing about that child's force policy. Copy constructor `044180` also has inherited owner/state behavior. The conclusion deliberately requires a cold child and the pinned raw3 metadata.

## Packet and active EF1 ownership

For mode0, `0450C0` constructs a 64-byte EF1 packet. The two qwords EED `180/188` are copied together to packet `18/20` (`0451EF..045203`); the packet begins at the stack address passed by `04525B`, with size `40` at `045260`. Type `EF1` is selected at `04523B`.

EF1 raw initializer `04A370` requires size `40`. After its EF0 initialization, it copies packet `18..27` into instance `28..37` (`04A3C0..04A3C7`). Thus heads occupy EF1 `28`, and the force byte occupies EF1 `30`. Successful EF1 is stored in EED `168` at `0453A3`.

The outer ownership arithmetic is:

| Handoff | Actual byte address |
| --- | --- |
| EEE raw packet | EED `70` |
| EEF packet within EEE packet | `+50` = EED `C0` |
| EF1 reference within EEF packet | `+A8` = EED `168` |
| Active EEF field | `B8`, assigned at `03B46C` |

EEE `0359F0` passes packet `+50`, size `B8`, when creating EEF. EEF `03B300` reads packet `A8` and calls the Engine context `18.V20` service (`03B451..03B464`). The default service has the following independently pinned binding:

- Engine descriptor pointer `252048` -> descriptor `1DBB00`.
- When no descriptor override callback is present, `031A64` selects this descriptor. `031B4E/031B52` install its field `20`, table `1DBBF0`, at context `18`.
- Table `1DBBF0.V20` is `03A2A0`. It calls `03A100`, which increments the high16 reference count at object `+8` using the allocator CAS operation, retrying on contention. A reference overflow reports failure.
- On success, `03A2A0` returns the **identical input pointer** (`03A2A6`, `03A2B7`, `03A2BB`); null input or failed retention yields null. It allocates no replacement instance and copies no EF1 fields.

Consequently EEF `B8` and EED `168` refer to the same EF1 in this successful default-service route. A custom Engine service override is outside this proof. This handoff is different from cloning an EED through context `10.V18`.

## Consumer and scope

EF1 selected-parent update `04C930` checks byte `30` at `04CE23`. When active, it calls the actual EF3 `V10` predicate on each selected label (`04CE69..04CE98`). For a nonobservable label, it overwrites the last attention-time column for every segment with F32 bits **`FF7FFFFF`**, negative finite maximum. This is not negative infinity, an EOS decision, a grammar rejection, or removal of structural tokens. The existing tensor-state tests distinguish the force-enabled attention writes from the disabled path.

The auditor pins 14 PE regions, 94 instructions, six descriptor/vtable bindings, the actual metadata, and prepared byte-layout/precedence discriminators. The latter are explicit layout checks, not captured native factory executions. They catch the wrong unadjusted EED `120` address, a missing mode0 reset, reversed metadata precedence, and treating a refreshed warm outer byte as the active child byte.

Run from the repository root:

```powershell
python tools/handwriting/audit_math2_force_policy.py E:/BlurMask-build-artifacts-20261014/joined-math2-force-policy/report.json
```

The auditor reads vendor binaries and ORTM data only. It never loads a vendor DLL, runs ONNX Runtime, or modifies a resource.

## Corrections to earlier wording

The following earlier descriptions overstate cloning at this particular handoff. They are listed for a separate correction; this batch does not modify the frozen files or their historical manifests.

| File and current line | Required wording correction |
| --- | --- |
| `docs/legacy-math2-score-channels.md:15` | `03B300` retains the same decoder reference at packet `A8` in EEF `B8` under the default Engine service. |
| `docs/legacy-math2-score-channels.md:51` | Replace “EEF decoder clone” with “EEF same-pointer decoder retention.” |
| `docs/legacy-math2-tensor-lifecycle.md:9` | Replace “selected/cloned EF1” with “selected EF1 retained at the same pointer.” |
| `tools/handwriting/audit_math2_score_channels.py:144` | Correct the report's `route` text from `EEF B8 clone` to `EEF B8 same-pointer retain`. Numeric pins remain valid. |

`docs/legacy-math2-restriction-map.md:20` says EEB clones its EED source. That concerns a different context `10.V18` call and is not corrected by this V20 proof. Its map-adoption discussion also remains unchanged.

## Next owning join and remaining search prerequisites

The next small owning join can replace an arbitrary force boolean with a private policy derived from the exact validated raw3 model set, actual EF3 owner, and **cold** preparation provenance. It should reject changed metadata, a foreign model set, and inherited/warm policies outside this route. It must preserve the existing explicit prepared-policy API for diagnostic controls. No runtime API is added here.

Before an actual default search loop can be claimed, the separate input packet's exact first dimension `N`, `4*N+1` limit and encoder `K` producer/layout, the full completed168 record and sidecar ownership in `037430`, and maxstep fallback/output ownership still need their own joins. Parent work owns the completed-record audit. The already proven cold width1 selection is not a greedy-first-EOS policy: the native driver retains `2*beam` raw candidates, applies delta pruning and the ANY-observable EOS gate, records normalized completed hypotheses, and can continue a non-EOS candidate after observing EOS. This force producer proof does not close those branches or establish recognition confidence.
