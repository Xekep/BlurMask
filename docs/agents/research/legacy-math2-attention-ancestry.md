# Math2 attention ancestry and a bounded memory-saving identity

This checkpoint proves the indexing needed before segmentation postprocessing.
It does not add runtime attention ownership, EF4 output units or a final decoder.
The vendor DLLs are read as data only.

036F20 starts from completed.parent60 and the old prefix length. Its first
ancestor is `history[parent][length]`. Working backwards from length-1 to0,
it takes that time's saved attention at the current ancestor and then changes
ancestor to `history[ancestor][time]`. History is a table of time-indexed ranks;
following a single final parent's row through every time would be incorrect.
Nonobservable primary symbols receive negative finite F32 maximum instead.

The saved attention producer is EEF03CAC0. With30 enabled, it appends an owned
tensor atF8 and copies the last time frame using Engine54.V30=185A50,
axis2/index-1. Backward reconstruction uses Engine54.V50=186650 twice: remove
the singleton time axis2 at0, then select beam axis1 at the current ancestor.
The static auditor pins the actual bindings and native instruction regions.

The existing EF1 tensor state already concatenates raw attention and gathers
the chosen parents at each step. With force-nonobservable enabled and the same
observable status for primary symbols and selected raw symbols, induction over
this gather shows that `previousAttention[:,parent,:].T` equals the backward
reconstruction. No second per-time tensor history is needed for this case.
This is a portable implementation identity, not a claim that the native engine
used that optimization.

The capture auditor checks all34 active parent prefixes in the existing20-step
official CPU ORT capture, including11 actual EOS events. All reconstructed F32
bytes agree. Independently generated controls check9436 prefixes,4582
nonobservable selections,1062 parent permutations and1009 beam shrinks, with
repeated parents and changing widths. Inputs deliberately encode time, source
segment and parent so incorrect ancestry cannot hide behind identical beams.

The identity has an explicit limit: raw48.alternate20 can change the primary
symbol while EF1 masks raw48.symbol08. A prepared counterexample with different
observable status fails equality as expected. Runtime integration must track
or reject that history, and bind an attention snapshot to its actual live
event before Apply invalidates the tensor step. Unforced nonobservable history
also cannot use this identity without separately applying the required mask.

```powershell
python tools/handwriting/audit_math2_attention_ancestry_static.py E:/BlurMask-build-artifacts-20261014/root-attention-static.json
python tools/handwriting/audit_math2_attention_ancestry.py E:/BlurMask-build-artifacts-20261014/root-owned-math-records.json E:/BlurMask-build-artifacts-20261014/root-attention-ancestry.json
```

Next remain actual event-bound snapshot ownership and its lifetime, bounded
segmentation conversion, EF4 and final candidate ordering/output. These checks
do not establish improved handwritten-math accuracy or readiness in the app.
