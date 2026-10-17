# Actual Math2 postprocessing producer

The current three-graph model requires the036F20 post-loop stage. Its presence
must not be guessed from caller options or skipped because the recognizer can
already concatenate labels.

The [tensor lifecycle audit](legacy-math2-tensor-lifecycle.md) independently reads
the fingerprinted decoder's actual outputs, including `attn_seg`.049630 compares
that name and stores its output ordinal at EF1.124, then writes1 to EF1.40 at
04A137.0450C0 reads that byte at045277 and stores it at EED.98 at045385.

EEE0359F0 receives the scalar packet beginning at EED.70. Its packet offset28
therefore contains that byte.035ADA/035ADE copy packet20..2F to EEE30..3F,
preserving it at EEE.38. The outer driver checks this exact byte at039B23 and,
on a successful search, invokes036F20 at039BE7. For the exact current model,
this is a required route, including when there is no fourth constraint model.

036F20 reconstructs per-symbol attention through retained rank/time ancestry,
handling observable and nonobservable symbols separately. It then calls036C70
and the EF4 service. EF4's type descriptor1FCEB0 points to interface table1FCEF8;
its first callback is042100. This binding is separate from EF3's observable-token
predicate and from037430's completed-record construction.

The freshly exported042100 body writes downstream four-byte output units at
completed50/count58, and operates on the separate98/countA0 auxiliary lane.
The original U16 primary prefix at08/count10 is a different field. A completed
EOS record with those downstream lanes still empty is not the result of EF4.
Native CPU transcendental functions and EF4's helper algorithms are not restored
by the binding audit.

The current owned prefix context preserves the decoder's gathered attention
history, but the ordinary EOS record stage does not yet retain native per-time
attention payloads. Whether a copied parent slice of that gathered history is
equivalent to native ancestry reconstruction requires a separate proof, including
time indices, structural tokens and forced attention masking. No such shortcut
is assumed here.

```powershell
python tools/handwriting/audit_math2_postprocessing_producer.py report.json
```

The static auditor verifies five complete Document regions,19 instruction pins,
two EF4 bindings and actual model outputs through the independent tensor-layout
audit. Ghidra exports of042100/036C70 are analysis inputs, not runtime code or
evidence that those callbacks execute in the application. No vendor DLL is loaded.
