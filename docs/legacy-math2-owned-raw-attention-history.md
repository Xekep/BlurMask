# Owned Math2 raw attention history

The opt-in cold recording route now retains actual per-time decoder attention
and reconstructs a completed record's matrix after recording and initial
ordering. It uses the same loop, prefix, Step generations, candidate sources
and official ONNX Runtime inference. Existing recording, carried-state
attention and ordering APIs remain unchanged. This is an internal stage;
EF4, completed-formula publication and acceptance are not joined here.

## Entry and ownership

`Math2Mode0ColdRecordingLoop.RunWithRawAttentionHistory()` runs the existing
recording loop once and returns its privately registered history owner.
`RunWithPreparedRawAttentionBudget(bytes)` names a prepared lower budget.
Only the private loop observer appends frames, before recording and Apply.
No caller can install raw arrays or assert successful completion.

Each saved frame is the actual output ordinal 1, F32 `[segments, beams, 1]`,
copied while the prefix and tensor-state lifetime gates remain held. Shape,
element type, stable segment extent, active beam count and total budget are
checked before allocating its payload. The scope is segments 1..6000 and
beams 1..16. There are no KV copies or whole decoder-tuple copies. Actual cold
force-nonobservable policy and the fingerprinted EF3 gate are retained by
identity; diagnostic alternate histories are refused by the existing sticky
history guard.

`history.CaptureCompleted(source)` accepts only a consumed Source lease in the
same actual recording vector, with its canonical immutable generation and
parent. This works before or after initial sorting, which moves actual record
references intact. It requires complete successful recording and does not
access a live Step. A foreign same-shape/same-input source is rejected before
consumption. Previously returned wrappers cannot be used after a later
history failure or loop disposal.

The separate historical snapshot exposes a detached `CopyAttention()` only.
Repeated capture of the exact Source returns the same cached wrapper without
charging another payload. Inspection arrays cannot mutate private frames,
generations or matrices. Disposal clears private raw frames and the payloads
held by cached wrappers, including wrappers retained by an inspecting caller.

## Native trace and managed adaptation

The existing static proofs are composed by
`tools/handwriting/audit_math2_raw_attention_history_join.py`:

- EEF `03CAC0` saves each step's last `attn_seg` time frame into F8 using
  Engine `185A50`, axis 2 and last-frame index.
- `036F20` starts at `ancestry[parent][oldPrefixCount]`, walks time downward,
  reads `raw[time][:, ancestor, 0]`, and updates
  `ancestor = ancestry[ancestor][time]`.
- A nonobservable old-primary token writes F32 `-Max`, bits `FF7FFFFF`.
  The cold root must end at ancestry `-1`.
- The actual EF3 table is `[2, 190, 261, 268, 276, 65535, 242, 296]`.
  Observable IDs are 2..261 plus 268, 276, 296. Raw model force policy is true.

The current EOS/fallback frame is saved as native F8 is saved, but that
record's matrix contains only its OLD prefix. The EOS frame itself is
excluded. Raw output capture and ancestry reconstruction are tested
independently of the previous carried-state snapshot shortcut.

`LegacyPreparedMathAttentionHistory.WritePrepared` is the bounded numerical
backward trace. Its explicit readers confer no recording or inference
ownership. The owning history supplies private frames and immutable source
generation readers. Managed copying and logical budget accounting are
portable adaptations, with no native pointer, allocator-order or ABI-padding
equivalence claim. The composed proof has overlapping regions; its component
counts are not disjoint native coverage.

## Budget and failure

The default is 16MiB of retained F32 payload per history. It includes raw
frames plus lazily cached reconstructed matrices. It excludes metadata,
ordinary model/tensor storage, inspection copies and diagnostic reference
captures. It is neither an RSS bound nor a native configuration property.

Reservation occurs before each private payload allocation. Statistics
distinguish lifetime reserved bytes, current raw/matrix bytes and allocation
attempts. A rejected budget check does not increment allocation attempts.
There is no automatic per-EOS matrix allocation. An explicit reconstruction
can exhaust the remaining budget; it then poisons the whole history and loop.

Entry cancellation and invalid prepared budget are checked before claiming
the loop and allow a later valid run. Once consuming work starts, unsupported
metadata, budget exhaustion, cancellation and allocation/callback faults
leave the owner unusable. Counted recording/raw prefixes remain observable as
statistics, including hashes, while raw payload or matrix inspection is
refused. Earlier snapshot wrappers are also invalidated. This is not
transactional rollback or partial accepted output.

## Verification

The new probe runs the actual bundled encoder, initializer and decoder using
official Microsoft CPU ONNX Runtime 1.23.2. Cold default width 1 and explicit
prepared widths 3/16 produce three history runs and 67 actual record-source
matrices, 13,208 bytes. All matrices match both the old carried-state snapshot
captured before Apply and an independent reconstruction from captured raw
outputs and ancestry. Private raw frames total 6,396 bytes across the three
owners. NativeAOT and Managed captures are byte-identical locally:
`31ff6118f8d5f6fc47dc40ea7c0d8302e55ad1039519ada2e4b3394e344e50a8`.
This SHA is local same-platform evidence, not a cross-platform assertion.

The 128 prepared branching histories cover 350 matrices, 467 repeated-parent
cases, 231 shrinks, 242 permutations and 1117 nonobservable selections. The
Python oracle independently checks forward gather/append/mask and backward
ancestry. These controls are explicitly prepared numerical cases, not
additional actual inference. Seven budget/failure cases check refusal before
payload allocation, retained successful prefixes, cached-snapshot poisoning,
and callback throw/cancel after a real recorded event. The capture includes 129
ownership, stale, foreign, replay, cancellation and lifetime guards. No
positive actual last-position fallback occurred; supported future fallback
sources use the same actual-record membership guard.

The existing recording, completed records, carried attention, initial order
and shared-provenance probes are rebuilt and independently audited against
their previous same-platform captures. No universal inference SHA is added to
the new auditor.

Run from the repository root with model pack, official runtime and output
paths supplied explicitly:

```powershell
dotnet run --project tools/LegacyMathRawAttentionHistory.Probe -c Release -- model.ikrmath models/math2/math2-unwrapped.res onnxruntime.dll capture.json
python tools/LegacyMathRawAttentionHistory.Probe/audit_output.py capture.json models/math2/math2-unwrapped.res
python tools/handwriting/audit_math2_raw_attention_history_join.py static-report.json
```

The pack SHA is
`08226987e872d1d81b4c691659286ca4fe4c3e8ce55dda699fddd597340d3ed7`;
raw Math2 SHA is
`2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a`.
The official runtime fetcher and existing cold bundle/state factories verify
the underlying model/runtime provenance.

Required `036F20` ownership is now represented by the historical matrix, but
its downstream native processing is still incomplete. In particular, the
fresh request's EF4 branch byte is not inferred from graph metadata and is
not substituted here. No sigmoid, assignment, deletion, replacement, optional
driver transformations, final sort/truncation or completed output is invoked.
