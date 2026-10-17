# Outer Math2 loop and last-position completion

This static contract extends the [original-input bound](legacy-math2-input-frame-bound.md)
and [ordinary EOS handoff](legacy-math2-eos-handoff.md). It does not implement
the outer loop, the last-position recorder or final result acceptance.
Addresses refer to the fingerprinted Document PE at image base180000000.

## Position and callback ordering

038CCD preserves the previous position, increments the current position and
stores it at `[rbp-38]`.038CE2 compares the **previous** position to the unsigned
maximum. If previous>=maximum, execution leaves the loop before invoking its
progress or score callback. Otherwise progress receives the incremented position
and the maximum. A false progress result enters error4000005, not a successful
early recognition result.

Ordinary EOS collection happens while scanning the current2*width raw48 pool.
Every eligible EOS row is masked with finite FMAX. A second035F70 sort follows.
This operation does not itself terminate the loop. Active count starts at1,
including the all-FMAX case. Existing completed hypotheses do not implement
first-EOS stopping or an unproven score-based early exit.

## Separate last-position fallback

039384..0393A6 enters fallback only when the current operation succeeded,
incremented position equals the maximum, and the completed vector was empty
at the entry test. It scans ranks0..width-1 of the already re-sorted pool.
For each rank it requires cost different from FMAX and completedCount<width,
then looks up that row's old parent at raw48+28. There is a distinct unordered
path: `0393E4 JP 0393EC` precedes the equality skip.
NaN therefore enters the parent/prefix checks; only ordered equality with finite
FMAX skips the row. This prepared IEEE case does not assert that the supported
actual finite-cost producer emits NaN.

As with ordinary EOS, eligibility requires **any observable symbol in the old
primary prefix**. The current selected label alone cannot make an empty old
prefix eligible. On the first observable old symbol it calls037430 at0394A6
with the current sorted raw48 row, old primary and secondary planes, ancestry
and the actual common-secondary frontier. There is no EOS-label test here.
The existing count test remains inside the loop, so the initially empty vector
can collect multiple fallback records up to width.

037430 stores the old prefix; this branch does not append the current non-EOS
label before recording it. Its candidate cost nevertheless comes from that
current raw48 row. A future fallback lease must be distinct from a lease that
proves an ordinary EOS event. It cannot be manufactured from public numerical
completion facts or by changing an EOS flag.

The whole038610 region contains exactly three direct037430 calls:0390B4
(supplied-prefix path),039325 (ordinary EOS) and0394A6 (last-position fallback).
Only the no-import route is currently being joined to the owned prefix runtime.

## Completed ordering is followed by optional work

039A96..039AB3 sorts the full168-byte completed records with036810 when there
are at least two. On success it then has three distinct optional branches:

- EEF.C0 nonnull and EEF.B6 zero:038170 tensor/history reconstruction;
- EEE.5C positive:038340 auxiliary reconstruction;
- EEE.38 nonzero:036F20 structural/segmentation postprocessing using input
  `strk_ends` when present.

Their actual cold producer values must be independently established before a
new search entry skips them. Absence of a caller option alone is insufficient.
The current ordinary EOS recorder does not close these branches.

At039BF2, including failure exits, the vector is sorted again if its count>1.
If count exceeds width,035000 destroys the tail and moves the remaining full
records. An implementation must keep each retained buffer/3E owner attached to
its record while reordering; sorting only score or text arrays loses ownership.
A successful038610 return is not by itself accepted text or a formatted formula.

## Verification

```powershell
python tools/handwriting/audit_math2_outer_driver.py report.json
```

The auditor pins the complete038610 region,44 instructions and all three direct
recording sites. It reads PE bytes only. It executes no vendor DLL or inference
and supplies no prepared labels, acceptance scores or simulated successful
native search. Final output transfer through03A260 remains a separate join.
