# Math2 common-secondary frontier and completed-record argument

`038610` supplies a computed common-secondary frontier to `037430` argument8. `037430` stores this qword at completed168 offset `28` (`0374E5..0374FC`). It is not a cold constant0. Even with one active beam, an empty imported prefix and two prior secondary items, the frontier becomes2 before EOS recording.

This is a static producer audit, not execution of a vendor DLL or a completed-record runtime implementation. It covers the ordinary no-import, no-detection source used by the owned prefix context. The separate imported-prefix route is pinned but remains outside that runtime scope.

## Driver producer

The complete pinned Document `038610..03A260` region has SHA-256 `180a1bcc3d913b2a19469e34a2b8a184e5a083354ef8f59414bed3d913e2716a`. All offsets below are hexadecimal.

| Storage | Meaning and producer |
| --- | --- |
| `[rbp-40]` | Signed scan-start index, initially0 (`0386DF`). Imported-prefix branch changes it to imported count minus1 (`038C1D`). It is distinct from the saved frontier. |
| `[rbp-30]` | Saved qword frontier, initially0 (`03895B`); imported-prefix branch can replace it (`038C27`). |
| `[rbp-80]` | Previous-generation secondary-vector table. It contains argmax-derived U16 sequences, not chosen primary labels. |
| `[rsp+62]` | Current common flag. The driver resets it true at each scan entry (`038D0F..038D16`), then passes its address to EEF.V30 (`038E03..038E0C`). |

The scan at `038D20..038D79` begins at the scan-start index, not at the previous frontier. For each existing secondary position, it first assigns that index to the frontier, then compares the first active beam with every other active beam. The first value is **signed I16** (`038D33`); other values are zero-extended U16 (`038D50`). The ordinary actual argmax range0..296 does not expose that signedness difference; `FFFF` prepared controls do.

At a mismatch it stops with common=false and the mismatching position retained. If all positions match, it leaves the last visited index. Empty input retains the old saved frontier. After successful EEF.V30, the driver increments the frontier by1 if the returned common flag is true (`038E46..038E50`). An empty cold scan followed by one-beam successful scoring therefore produces1, not0.

The EOS path loads the saved frontier at `0392F6`, writes it to argument8 stack slot `38` at `039309`, and calls `037430` at `039325`. The same saved value is also used by a separate later recorder path. This audit does not implement that outer fallback path.

## EEF.V30 refinement uses a separate tensor view

EEF.V30 is `03CAC0`. It obtains the same decoder `reco_output` tensor through EF1.V28, requires F32 type7, rank3, active batch EEF `110`, and vocabulary EEF `10` (`03CB84..03CBAD`). For one active beam it skips the multi-beam refinement and preserves the incoming common flag (`03CBB3..03CBB7`).

For multiple beams it creates a rank2 descriptor:

- Preserve original data pointer `68`.
- Copy dimensions `18/20`, dropping the leading time dimension.
- Copy strides `78/80` and per-axis offsets `C8/D0`, dropping the leading time stride and offset.
- Leave no additional data-pointer displacement to the last time frame.

The copies are at `03CBC3..03CC9D`. Thus a contiguous zero-offset `[T,B,V]` tensor is inspected at **frame0**. This must not be replaced by the portable score adapter's last-frame selection when `T>1`. Current bounded actual model steps with `T=1` cannot distinguish these routes; the audit includes a prepared two-frame counterexample.

Only when the incoming common flag is true does `03CD19..03CD7C` compare a per-beam argmax. The caller passes axis1 and the current beam ordinal (`03CD44..03CD5B`) to Engine54.V38. Different argmax indices clear common; matching indices preserve it. This occurs before `03C0D0` transforms logits into log probabilities (`03CD99`). It is not a comparison of candidate symbols, prior costs, normalized EOS scores, or the secondary sidecar from `036250`.

Engine54.V38 points to `185D20` through table entry `22A238`. The F32 jump-table entry points to `185EE3`: `COMISS` followed by `SETA` updates only on an ordered strictly greater value. The helper initializes its best value and index from the first element (`185E43..185E7D`), rather than from negative F32 maximum and index-1. Ties retain the earlier index; a later NaN is ignored; a first NaN leaves index0. Empty axis or failed scalar access returns-1. `036250`, the secondary-sidecar producer, has a different initial state and must remain a separate helper.

## Verification and next owning join

```powershell
python tools/handwriting/audit_math2_common_secondary_frontier.py E:/BlurMask-build-artifacts-20261014/joined-math2-frontier/report.json
```

The static auditor checks six complete PE regions, 83 instruction pins and three bindings, plus 3,905 prepared argmax controls and 1,170 prepared frontier controls. These cover one-beam advancement, mismatching old secondary prefixes, unequal current argmaxes, ties, NaNs, signed-U16 disagreement, empty old input and frame0 versus last-frame discrimination. The checks read PE bytes only.

The next bounded implementation should extend the owned prefix context with a same-Step frontier fact. It must scan the previous secondary generation before selection, inspect that Step's original logits using the pinned rank2-view semantics, and retain the resulting frontier with the candidate generation. An EOS event lease must bind context, Step, generation, raw48 rank, previous primary/secondary rows, rank/time ancestry and this computed frontier. It must be one-use and become stale after selection or failure. Public numerical score facts alone cannot create that ownership.

The completed recorder can then consume the lease before `ApplySelected`, store the frontier at completed offset `28`, and preserve the already-proved constructor/content-failure order. Imported prefixes, nonzero tensor view offsets, optional detection/constraint models, native ABI pointer/padding captures, completed-record fallback selection and the full search loop remain separate work. No fixed frontier0, fabricated EOS owner, native CPU numerical equivalence or recognition confidence is claimed.
