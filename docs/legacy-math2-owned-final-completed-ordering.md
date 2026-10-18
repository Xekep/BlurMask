# Owned success-only Math2 final record ordering

`Math2Mode0RawOnlySource.OrderFinal` consumes the exact successful raw-only EF4
token. It sorts and truncates the actual completed vector at the `039BF2` tail of
`038610`. It preserves each actual Record, recorded Source lease, Library3E and
its post-EF4 units/assignments/masks/runs. It does not construct or accept a
formula, transfer SDK output, or claim the general configured/native SDK route.

This follows [the owned raw-only EF4 stage](legacy-math2-owned-raw-only-ef4.md).
The original initial-order, EF4 and unopted recording APIs retain their behavior;
once this new entry claims the EF4 stage, its former inspection token refuses
instead of exposing an obsolete order and a released tail.

## Native boundary

The existing whole-driver/initial-order auditor pins `039BF2..039C2B`:

- Count zero/one bypasses this final sort and truncation block.
- Count greater than one calls exact `036810` over inclusive 168-byte records.
- Only if sorted count exceeds the same saved beam width does `039C26` call
  `035000(width, count-width)`.

The new auditor additionally hashes the complete `035000..035110` and
`034D10..034EA0` regions and pins 30 instructions. In the full-tail branch,
`035060/035066` call the destructor starting at the width ordinal, `03506B`
advances by 168 and `035076` repeats in ascending order. `035078` stores the new
count **after all destructors**. This branch performs no memmove. The separate
middle-range erase/memmove branch is not used by this endpoint.

`034D10` frees primary +8, secondary +18, nested tensor data, +40, units +50,
runs +68, sidecars +78, Library +88, and auxiliary +98/+90. The bounded producer
owns no nested/auxiliary/constraint lanes. Its relevant logical release order is
primary/secondary, EF4 payload and Library. This is a managed ownership/memory
adaptation, not a native pointer image or allocator/padding equivalence.

The native caller enters this tail even on earlier failure exits. The new API
deliberately requires a **successful** EF4 token. Native failure-exit tail
orchestration is still a separate join. Injected managed faults below demonstrate
cleanup ownership, not native throwing-destructor behavior.

## Identity, budget and failure ownership

Preflight verifies the same source lifetime, registered stage, every actual
record/library membership, width from the same loop, cancellation and payload
reservation. The 65,536-record cap is a named library safeguard, not a native
count limit. Invalid budget/canceled entry is reusable; the source is claimed
only after those checks.

The 16 MiB payload-only default conservatively reserves the preceding EF4
history/payload envelope plus `count*(2*168+17)` bytes for numeric projections
and ordinal/position/seen arrays. It intentionally retains the earlier peak
workspace reservation rather than pretending to measure live RSS. CLR object,
reference-container and dictionary metadata are excluded from this data-payload
bound. No NN call, new model owner or KV copy occurs here. An exact reservation
succeeds; one byte below and zero fail before claim.

A complete original-reference array stays alive through every move and release.
The already audited numeric kernel receives a portable score-plus-ordinal 168-byte
projection; a full byte/identity permutation is validated before moving actual
Record references. EF4 payload lookup uses private exact Record identity, not
score equality or an invented reconstructed record. Unique-reference swaps
preserve the full owner set through an injected move failure.

The actual tail is released in ascending sorted ordinal order. Count remains
unchanged through all those releases, then one `RemoveRange` commits the count.
If a managed fault occurs after a release, all original references remain owned
for cleanup, including already released Libraries. Library release is idempotent;
subsequent disposal releases each original exactly once. There is no rollback.
After a consuming failure, the loop is poisoned and no final rows can be read.
Diagnostic counters report the retained logical prefix only.

The successful token exposes detached copies and the validated sort permutation.
It remains tied to the same source/loop lifetime and refuses after disposal.
No caller-selected width, foreign stage, fake final token or no-winner coercion
is accepted. The result is a sorted/truncated record owner, not a formatted
formula or confidence/acceptance decision.

## Verification

The dedicated probe runs the actual official-ORT raw-only pipeline at cold width
one and explicitly prepared search widths three and sixteen. Its 67 real EF4
records produce 20 retained owners and 47 actual tail releases. These are real
positive truncation cases, distinct from pure prepared sorting controls.

An independent Python oracle transcribes the existing native `036810` index
algorithm. It verifies every permutation, retained post-EF4 payload, actual
Source association, intermediate owner swap, ascending release sequence, and
single count commit. Eighteen deliberately injected fault/cancel boundaries
cover numeric ordering, owner moves, release and count-commit phases; cleanup
must retain every original Library exactly once. Another 256 prepared whole-row
cases cover ties, NaN/infinity, zero/singleton ranges and native short-sort
boundaries. They do not manufacture actual inference records.

Fresh Managed and NativeAOT plus the independent oracle pass with 108 guards.
Their local Windows capture SHA is
`603a2b493e530b6cc9cdb07151c904a1486cdaee3541c398cdeded3ec95183e7`.
All seven earlier raw-only EF4, recording, record, carried-attention, initial-sort,
provenance and raw-history probes pass again in both runtimes with independent
auditors and unchanged full captures/sidecars. The EF4 capture remains
`11539c01fc1bba66977d6dd4781f627f8f2aa1d34e751bc2f73408e7a176c9ad`.

All tests use real model outputs without expected formula feedback. Captures are
checked numerically/structurally; any Windows-local complete SHA is evidence only,
not a universal cross-platform ORT golden.

```powershell
dotnet run --project tools/LegacyMathFinalCompletedOrdering.Probe -c Release -- `
  <model.ikrmath> models/math2/math2-unwrapped.res <onnxruntime.dll> <capture.json>
python tools/LegacyMathFinalCompletedOrdering.Probe/audit_output.py <capture.json> models/math2/math2-unwrapped.res
python tools/handwriting/audit_math2_final_completed_ordering.py <static-report.json>
```

The static tool reads vendor PE bytes only. No vendor SDK execution or GUI is
used. Remaining joins include failure-exit tail orchestration, non-raw-existing
source policies, final output transport and application acceptance.
