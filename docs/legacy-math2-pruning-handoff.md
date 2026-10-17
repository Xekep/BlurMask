# Math2 raw-candidate pruning and ordering handoff

The actual model metadata `delta_cost_max = -1` enters native pruning; it is not a disabled-pruning sentinel. Decoder `038610` sorts full 48-byte candidate records, masks costs, handles EOS, sorts those records again, and only then selects active continuations. Completed 168-byte hypotheses use a separate sorting helper. This is a static producer/consumer audit, not an executable beam decoder or an actual hypothesis trace.

## Delta producer and ownership

EED initializer `045460` stores F32 `1000` at EED `94` before model metadata (`0455B9`). Metadata callback `044AC0` receives frame EED `68`; its `delta_cost_max` branch writes F32 at frame `2C`, hence EED `94` (`044E47`). Unlike `model_temperature`, this branch is outside the optional-fourth-model guard. The fingerprinted decoder JSON supplies `-1`, exactly representable as F32 bits `BF800000`.

The optional request configuration can overwrite EED `94` afterward (`045806`). A fresh EEB request's configuration pointer is null. EEE raw construction copies EED `70` into EEE `10`, so EED `94` becomes EEE `34`, the field read by the driver. The cold unconfigured route therefore captures `-1` in EEE `34`.

EED copy/raw-input packets can carry previous scalar settings, and copy construction can retain an existing EED `220` decoder. Warm `045460` can update EED fields while skipping child reconstruction. Consequently, neither a fresh wrapper around a retained source nor a refreshed EED scalar alone establishes the child's current pruning value. This audit does not claim an exhaustive inventory of external host setters.

## Two record formats and sort ranges

| Stage | Helper | Format/range |
| --- | --- | --- |
| Raw expansion candidates | `035F70` | 48 bytes (`0x30`), float key at byte 0; driver calls inclusive ordinals `0..2*beam-1` |
| Completed hypotheses | `036810` | 168 bytes (`0xA8`), float key at byte 0; driver passes first and last actual completed records, only when count exceeds 1 |

The raw driver sort range is the allocated candidate **capacity**, not heap active count. The actual unrestricted vocabulary has 297 classes and supported beam widths are 1 through 16, enough to fill a `2*beam` heap from at least one active input row. A generic prepared input with fewer offers must not silently shrink the native sort range or invent initialized remaining rows. The native allocation does not establish zero-filled inactive bytes.

Raw sorter `035F70..0360B0` is 320 bytes. It selects the middle record's F32 cost using signed index arithmetic, partitions in place, recursively sorts the left partition, then iterates the right partition. Left and right scans use strict ordered comparisons (`COMISS`/`JA`); equal or unordered comparisons stop scanning. It exchanges all three unaligned 16-byte blocks of a record, preserving its other 44 bytes. Equal keys are not stable, and NaN has no total-order position. Replacing this with a platform generic sort would need a separate equivalence proof.

`036810` has its own 168-byte movement and small-range/partition rules. The existing [candidate-ordering audit](../tools/handwriting/audit_math_candidate_ordering.py) pins that separate helper. It must not be substituted for `035F70` by changing only a record stride.

## Driver sequence

Within a successful expansion step, `038610` performs the following handoff:

1. Build raw candidate capacity `2*beam` through the actual heap/expansion route, then call `035F70` over that entire capacity (`0391EA..0391F2`). The native call precedes the following producer-success test; this audit does not create a different failure-order runtime contract.
2. Load EEE `34`. If `delta < 1000`, compute **one F32** `threshold = bestCost + delta`. For ordinals 1 through `2*beam-1`, write exact F32 maximum `7F7FFFFF` when candidate cost is ordered greater than or equal to that threshold (`039208..039243`). Ordinal 0 is excluded.
3. Scan all `2*beam` records for the raw EOS class: record qword `+8` equals sign-extended EEE DWORD `28`. Exact F32-maximum cost skips it. For a candidate ranked below `beam` with a nonempty selected parent prefix, scan that prefix until **any** token passes EF3; that first positive result triggers completed-hypothesis recorder `037430` (`0392D6..039325`). No all-prefix-token predicate is used.
4. Mark every matching non-sentinel EOS candidate's cost as F32 maximum, including candidates outside the first `beam` or prefixes that had no observable leaf (`039347`). Other record bytes remain unchanged.
5. Sort the full raw capacity again with `035F70` (`039377..03937F`). The delta threshold is not recomputed after removing EOS.
6. At the maximum position, if no completed hypothesis is stored, a separate fallback scans the first `beam` non-sentinel candidates while the completed count is below `beam`. Again, a nonempty prefix with any observable token can call `037430`. This fallback does not perform the same EOS cost-marking operation.
7. Initialize active count to **one**, then scan raw ordinals 1 through `beam-1` until an exact F32 maximum is encountered (`0394EC..039527`). It does not first require the record at ordinal 0 to have a finite or non-sentinel cost.
8. Carry the selected records' parent ordinals, raw labels, alternate semantic labels, and sidecars into prefix construction and decoder-state continuation. Full-record identity matters throughout the two sorts.

Completed records are subsequently sorted with `036810` at `039AB3`, and again at `039C0C` after intervening completion processing. Both calls use the actual completed count and `0xA8` last-record arithmetic; they are not raw candidate-capacity sorts. Collection, normalized completed scores, and continued non-EOS search are distinct from the raw-cost order.

## Floating-point branches

The outer comparison is `COMISS(1000, delta)` followed by `JBE`: delta at least 1000 or an unordered comparison skips pruning. Negative `-1` enters it. The inner comparison is `COMISS(candidate, threshold)` followed by `JB`: an ordered smaller value or an unordered comparison avoids the sentinel write; ordered equality is masked.

EOS and active-cut tests use `UCOMISS` with an explicit unordered jump. A NaN cost is not treated as exact F32 maximum: it can enter the EOS-prefix path or be counted by the active scan. These are instruction-branch facts, not a claim that current inference produces NaNs or that an arbitrary floating-point exception environment has been restored.

## Conditional width-one consequences

For the proven cold width-one route, ordinary sorted finite raw costs, and delta `-1`, the second of two candidates is masked. If the first is non-EOS, it remains the only unmasked raw candidate. This does not establish greedy/native equivalence: prefix state, normalization, completed candidates, and later callbacks are still separate producers and consumers.

If the first candidate is EOS and has a qualifying observable prefix, it can be recorded and then masked too. A two-record equal-key `035F70` partition exchanges the records. For example, this explicitly **prepared** handoff:

```text
before delta/EOS:  [EOS1 cost0, leaf5 cost1]
after delta/EOS:   [EOS1 FMAX,  leaf5 FMAX]
after re-sort:     [leaf5 FMAX, EOS1 FMAX]
active count:     1
```

The static report includes both complete 48-byte records for the equal-FMAX swap, with explicit raw-class fields and residues. This is a consequence of the pinned two-element branch, not an observed native decoder output or a manufactured actual inference fixture. It demonstrates why immediate first-EOS termination and stable identity assumptions are unjustified.

## Reproduction and boundaries

```powershell
python tools/handwriting/audit_math2_pruning_handoff.py E:/BlurMask-build-artifacts-20261014/joined-math2-pruning-handoff-static.json
```

The audit checks seven whole native regions and 77 instruction pins, composes the frozen width/model-metadata proof and separate completed-ordering audit, and checks eight prepared numeric pruning cases plus complete equal-FMAX record transport. It fingerprints the actual model delta rather than treating the string `-1` as a generic sentinel convention.

No native SDK, ONNX Runtime session, GUI, or inference is executed; runtime and CI files are unchanged. The prepared heap port and completed-record sorter remain separate bounded components. Full prefix/sidecar production, configured alias expansion, native CPU exp/log, completed-hypothesis ownership, and the actual reusable decoder loop remain unfinished joins. This audit makes no recognition-quality or complete-engine claim.
