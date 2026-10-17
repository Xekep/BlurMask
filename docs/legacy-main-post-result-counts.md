# Main24D/29B empty-interval count policy

`LegacyFreshMainPostResultCountPolicy.Prepare(resource, actualLines)` binds the
actual fresh main0 line/source720 owner to its24D@188012 and29B@188050 profiles.
It validates assigned guides and fresh unknown-source720 metadata before reading
the descriptor counts. The immutable result contains scalar counts and initial
blend values; it does not execute shear geometry, EM, smoothing or a text winner.
The existing24D/29B APIs and the TEXT wrapper's empty-result refusal are unchanged.

Native177280 and175350 accumulate `end-first-1` in32-bit unsigned registers for
each assigned ordinary descriptor. An empty interval contributes UINT_MAX.
24D converts the zero-extended counter to float before its1/5 threshold blend.
This is the initial blend, before related-symbol weights; it is not the final
shear.29B applies its strict unsigned per-descriptor10 threshold before summing
the qualified contributions, then uses the10/30 initial mixture blend.

175491 tests the wrapped total for zero before either observation allocation.
The zero branch publishes the actual profile's1.5 value, even if the qualified
counter has wrapped. For example, prepared counts `[0,2]` have total0 and
qualified UINT_MAX, but take this zero shortcut. Simply skipping the empty
descriptor would incorrectly produce a nonzero observation count.

When the total is nonzero, native calls the allocator with item size4 and the
wrapped total and qualified counts. This is a separate boundary from the number
of observations actually produced by the later nonempty row walks. `[0]` asks
for UINT_MAX floats. `[0,12]` declares10 slots while the row walk produces11
observations. The portable policy distinguishes oversized allocations from
insufficient declared capacity and refuses both before any such allocation.
It does not reproduce huge allocations, write beyond declared capacity, drop
descriptors, clamp counts or replace an unsafe case with the zero-gap result.
Allocator-specific padding is not treated as valid observation capacity.

`Counts.RequireBoundedGapAllocation()` enforces that refusal. Only
`Counts.ReadNativeZeroGap()` exposes the exact total-zero value; it refuses other
branches. The portable observation-count budget is4096. No array length comes
from a native wrapped counter. Descriptor snapshots are capped at512 complete
88-byte records and source rows at4096 before evaluation.

## Verification and limits

The probe checks7386 explicitly prepared descriptor count states, including
unsigned underflow, strict threshold neighbours, permutations, kind3 skips,
unassigned-line skips and the prepared ordinary kind6 count branch. A Python
integer/NumPy oracle reads the actual thresholds and zero-gap bits directly from
the acoustic resource and checks every scalar and branch. Static PE pins cover
the32-bit decrement/add, unsigned comparison, zero-extended float conversion,
zero-shortcut store and both allocator argument sets.

Seven actual raw TEXT preparations exercise the owning API with their real
source720 counts; all remain nonempty. Prepared empty intervals are not claimed
to be native first-NN empty-result fixtures. The owning factory deliberately
requires assigned fresh kind0 descriptors; it does not generalize its prepared
kernel to recognized/reused source state. Full24D geometry for empty intervals
and complete empty-first postprocessing remain unsupported. In particular, a
correct24A guide alone does not make that whole path ready.

The probe takes AK, engine profile, separation profile, case data, publication
data, line30, line50 and capture path. Its independent `audit_output.py` takes
**AK then capture**. The static audit is
`tools/handwriting/audit_main_post_result_counts.py --output <report>`.
