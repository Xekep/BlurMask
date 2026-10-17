# Owned raw-only Math2 EF4 stage

This internal entry connects actual raw pen strokes, the authenticated three-model
Math2 cold bundle, completed records, their per-time attention history and the
restricted `042100` byte-one correction branch. It ends after record score/unit/
assignment/run updates. It does not perform the second completed-record sort,
truncation, formula construction, application acceptance or SDK output transport.

## Source and policy boundary

`Math2Mode0RawOnlySource.PrepareColdDefault` owns feature preprocessing, exact ORT
input tensors, stroke ends and the existing cold recording loop. Its private
constructor excludes existing expression objects, C25 selections, imported glyphs
and caller property packets. This is a library-owned raw-only contract. It is
**not** evidence that an arbitrary native fresh EEB selection contains no C25
objects. The general existing-source path remains unsupported.

The cold bundle retains the already fingerprinted encoder, initializer, decoder,
resource and EF3 gate. The source uses official Microsoft CPU ONNX Runtime 1.23.2;
no vendor SDK or MyScriptMLOrt code is executed. Width one uses the existing
proved cold policy. `PrepareWithPreparedSearch` explicitly selects diagnostic
widths three and sixteen and is not another native-default claim.

The new policy auditor pins eight whole native regions and 25 instructions, with
the existing beam/configuration auditor as a dependency:

- `0455C3` resets `EED.B4` to float bits `3DCCCCCD` (alpha 0.1), with adjacent
  segmentation mode zero. The actual three model configurations contain no
  `seg_cost_weight` override; the third selects sigmoid segmentation.
- `02780A` writes fresh `EEB.40=0`. Request begins at `EEB.38`, so request+8 is
  zero; `04539A/04539D` copy it to `EED.BC`. Packet copies lead to `EEE.5C=0`,
  and `039AFC/039B01` skip optional `038340`.
- Fresh request byte four is one, not zero. Existing cold ownership installs no
  fourth constraint model, so optional constraint `038170` is absent here.
- `036FA8/036FAC` skip records whose primary count is zero before attention or
  EF4. Their score bits and empty output remain unchanged, including NaN/Inf.

See [the coordinate proof](legacy-math2-ef4-empty-source-coordinates.md) for the
native ABI, empty-source exclusion and original/filtered/remapped tensor planes.
The EF3 separator at +0A is `FFFF`; it is distinct from replacement label 299.
Opaque argument eight is not treated as an empty-source flag.

## Owning transition

`RunToInitialOrder` records the actual same-context search and raw attention, then
uses the existing initial-sort owner. `ApplyEf4` requires this exact order/history
pair and the private stroke-end array. Value-equal foreign arrays, Source, Record,
Library, vector, factory and stage substitutions are refused.

Metadata, extent and ownership validation precede the consuming claim. A canceled
entry or invalid budget argument leaves the entry reusable. Once claimed, even a
total-budget failure consumes the source. The old initial-order wrapper becomes
stale immediately; it cannot expose a pre-EF4 ordering after scores change.

Each nonempty actual record takes its own recorded Source lease, canonical old
primary and historical attention. The implementation keeps these coordinates:

1. Original attention `[oldPrimary, encoderSegments]`, sigmoid-activated with the
   existing explicitly portable MathF adapter.
2. Observable-only row copy and original-row map for initial assignment.
3. Units initialized from the actual old primary; assignment values identify
   original rows. The supported finite observable scope has no unassigned -1
   entry, so `041240` inserts no placeholder and its initial mask is all zero.
   Auxiliary lanes are empty in this completed-record producer.
4. `041660` deletion and `041BA0` separator compaction remap assignments. The
   deletion mask is separate from the initial insertion mask. The actual JNE at
   `0428BB` rejects deletion of an inserted/nonzero-mask position.
5. `041A30` compresses runs, including possible absent -1 assignments after
   stroke-boundary suppression.
6. `041CF0` uses the **original activated tensor and initial insertion mask**,
   together with the already remapped assignment values. No intuitive matrix
   rebase is applied. Actual record Score is then updated through its owning
   capability; Source and Library references never move or get reconstructed.

The empty-primary path skips reconstruction, EF4 and score stores. Current real
records do not exercise this path; a separate prepared predicate/bit test proves
the skip contract without manufacturing an actual empty EOS event.

The numerical implementation uses existing prepared helpers only after the
corresponding coordinate preconditions are satisfied. New managed arrays and
logical record fields are a portable numerical/memory adaptation. They are not
native ABI pointers, allocator padding or a claim of native allocation order.

## Lifetime, failure prefix and payload budget

Source and loop gates bind the same live lifetime. Observer callbacks are
followed by owner validation and cancellation checks before further mutation.
The source is single-use. Consuming faults/cancellation poison both the stage
and its loop; successful-result reads then refuse. Logical phase payloads and
previous score stores remain visible to diagnostic statistics. There is no
rollback or partial accepted formula. Disposal clears retained EF4 payloads and
releases the existing record Libraries exactly once.

The default limit is 16 MiB of **payload**, not RSS or CLR overhead. A conservative
preallocation envelope includes retained raw per-time history, private stroke
ends, one shared cached historical matrix per distinct Source, retained EF4
units/assignments/masks/runs, and peak numerical workspace. It does not count a
shared cached matrix twice. Tensor shapes and all metadata are validated before
numeric payload allocation. Existing first-reconstruction ownership is required;
a history with prior cached snapshots is refused. The exact envelope succeeds;
one byte below it and zero fail before any snapshot or EF4 payload allocation.
No KV tensors are copied. Model/runtime/input owners and caller inspection copies
are outside this named payload limit.

## Verification

The dedicated probe uses three actual official-ORT runs: width one plus explicitly
prepared widths three and sixteen. It produces 67 actual Source-owned records;
47 original units are removed. No actual suppressed segment or literal-index
counterexample occurs in this fixture. Those cases are not claimed as observed
native inference.

- Independent Python reconstruction consumes captured raw attention/ancestry,
  computes sigmoid/assignment/pruning/runs and the literal original-matrix score.
  It checks every record field and Source/Record/Library/factory identity through
  runtime guards. Maximum observed penalty error is `1.914367686595142e-08`, and
  final-score error is `1.888549803652495e-06` versus the independent arithmetic.
- Nineteen deliberate failure/cancel/budget/disposal cases inspect concrete
  phase payload lengths and unchanged or already-stored scores; 129 guards cover
  foreign ownership, stale ordering, replay, detached copies and lifetime.
- 256 prepared score cases distinguish literal native original-row lookup from
  a tempting remapped-matrix interpretation. Another 256 prepared empty-primary
  controls preserve arbitrary score bits, including NaN and infinities.
- Fresh Managed and NativeAOT captures are byte-identical on this Windows host:
  `11539c01fc1bba66977d6dd4781f627f8f2aa1d34e751bc2f73408e7a176c9ad`.
  This is local evidence, not a universal cross-platform ORT/MathF golden.
- All six earlier recording, completed-record, carried-attention, initial-sort,
  provenance and raw-history probes pass again in Managed and NativeAOT, with
  their independent auditors. Their prior complete captures remain byte-identical,
  including prefix `d350e2ca...`, records `f455ac7a...`, carried attention
  `17ee5e47...`, initial sort `36998e41...`, provenance `e9b954b2...`, raw history
  `31ff6118...` and recording loop `e61c3ecd...`.

Reproduce from the repository root (the four probe arguments are model pack, raw
resource, official runtime library and output capture):

```powershell
dotnet run --project tools/LegacyMathRawOnlyEf4.Probe -c Release -- `
  <model.ikrmath> models/math2/math2-unwrapped.res <onnxruntime.dll> <capture.json>
python tools/LegacyMathRawOnlyEf4.Probe/audit_output.py <capture.json> models/math2/math2-unwrapped.res
python tools/handwriting/audit_math2_rawonly_ef4_policy.py <static-report.json>
```

The static auditor reads preserved vendor bytes only. No formula text, expected
transcription, confidence calibration or application acceptance threshold feeds
this stage. General existing-source correction, inserted placeholders, auxiliary
lanes, configured/warm policies, final ordering/truncation and output transport
remain separate joins.
