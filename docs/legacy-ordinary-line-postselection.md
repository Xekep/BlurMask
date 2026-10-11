# Native ordinary unknown-row postselection

`LegacyUnknownLinePostselection` implements the ordinary, unlocked, empty-guideline
branch of Text `18007a850`. It accepts the physical groups actually selected by
`07c160`, retains unselected physical IDs as -1 and returns the native row order
and updated physical memberships. It does not invent attachment of unselected
strokes, singleton groups or a language-specific row heuristic.

For each nonempty group, the native sort key is the float32 sum of prepared
mean-Y fields divided by its physical-stroke count. `180072550` uses middle-pivot
partitioning with exchanges on equal keys. The adapter preserves these exchanges;
stable sorting would change adjacent groups and subsequent merge decisions.

The native traversal is bottom-up across consecutive sorted groups. It tests
vertical bounding-box overlap and the difference of the original mean-Y keys.
For disjoint horizontal bounds, `18007a460` compares the mean-Y values within
both nearest five-scale-wide group ends. If either end differs from its full
mean by more than a quarter scale, the end-to-end limit is 1.75 scale;
otherwise the full-mean limit is one scale. All limits are strict where the
native branch is strict. IEEE NaN comparisons for an empty end subset are retained.

For the remaining near-mean candidates, `180071ad0` builds the first group's
horizontal interval union with its native one-unit bridging tolerance.
`180071c80` accumulates coverage for every second-group stroke; a single
intersecting union interval returns the entire query width, rather than a
clamped geometric intersection. Multiple intersections use the original first,
interior and last interval arithmetic and relative-zero tolerance. A total
below half scale permits the merge. Surviving group keys are not recomputed,
physical IDs are appended in their original group order, and traversal continues
upward. The final ordinary materialization preserves this native sorted order
and maps physical strokes to the newly compacted output groups.

`LegacyPostselected2e3LineFrames` connects source219/profile scale, prepared30/50
pair networks, unknown-row selection, this postselection, and explicit resource
class24a estimation/deskew. It preserves the earlier07ca80 single-active-stroke
branch. The selected class24a profile, window multiplier and overlap remain
explicit caller choices. This is a bounded ordinary-input endpoint, not a claim
that the full source configuration dispatcher has been restored.

## Verification

The vendor-free Python oracle generates512 independent scalar geometries and
partitions. Managed and Windows Native AOT agree on1,904 final groups, native
equal-key order, bottom-up merged physical order and unassigned IDs. Invalid IDs,
duplicate physical ownership, scalar bounds and cancellation are checked.

The live diagnostic separately runs committed Russian/line resources through
raw ink, resource scale, pair networks, selection/postselection, Gaussian frame,
four-family glyph classifier, final class cache and dictionary search. Fifteen
word/frame checks cover the own printed MIR fixture on one row and two translated
rows, all three resource geometry profiles, and bottom-row-first ingress.
Every expected word remains best `мир` with cost54456. Bottom-row-first input is
returned top-down without a caller sort. Singleton frame and precancellation are
also checked. These are original printed fixtures, not held-out or joined
Russian handwriting, native winner/confidence or production AI-pen acceptance.

Guideline/known/locked source branches, the subsequent vertical proposal sweep,
source configuration overrides, incremental line history and native final
winner/acceptance remain outside this endpoint. No vendor DLL or GUI is executed.

```powershell
python tools/LegacyLinePostselection.Probe/reference_controls.py D:/BlurMask-checks/postselection.json
dotnet run --project tools/LegacyLinePostselection.Probe -c Release -- D:/BlurMask-checks/postselection.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res
```

Using only the controls argument runs the vendor-free kernel checks.
