# Legacy prior context selection

`LegacyPriorContextSelection.SelectNative` restores the checked `1801ca5f0` filtering of
precomputed accepted observations. Caller supplies target line/scope identity,
target physical indices and previously accepted groups with inclusive physical
index ranges, mean X and28 neural probabilities.

It keeps observations on the same line and optionally in the same context
scope. Native runtime selector `+28` chooses these distinct policies:

* `0` retains an observation only when **every** physical ID in its inclusive
  range belongs to the target ink (`AllContained`). Partial overlap is rejected.
* `1` retains only observations with no physical IDs in the target (`Disjoint`).
* Other native selector values retain all matching line/scope observations;
  the checked API names this branch `SameLineAll` (`2`).

Candidate kind `1` has no selected prior context in any mode; the boolean
`targetIsBoundary` means this resolved native kind, not every special candidate.
Output retains
caller observation order and composes with `LegacyContextWeighting` and
`LegacyFloatInputQuantization`. Empty accepted context becomes28 fixed −1024
channels.

The bounded implementation accepts at most1024 observations and4096 target
indices0..65534; accepted group ranges begin at1..65534. Native window enumeration
uses stroke0 as the earlier X reference and starts observed windows at1, so a
target containing physical index0 remains valid and cannot overlap those windows.
It rejects invalid group ranges or neural probabilities.
Sorted deduplicated target indices and binary searches implement the native
selection semantics without enumerating every integer in each prior range. These bounds
are an original defensive API limit, not an asserted SDK or product limit.

Managed and Windows Native AOT controls pass640 independent Python comparisons
for all three native modes; their reference uses set intersections and subset
checks. Analytic tests cover inclusive endpoints, holes inside a contained
interval, unordered/duplicate target IDs,
line/scope/boundary exclusion, weighting and fixed conversion, malformed input,
work bounds and cancellation.

The older `Select` API is retained for callers: its default matches native
mode `1`; `excludeGroupStartOnly=true` is an original start-only exclusion
heuristic. Earlier documentation incorrectly attributed that heuristic to the
native zero-selector branch. Raw instructions `1801ca93f..1801ca9fd` show mode
`0` exits on the first **absent** target ID and retains only when the complete
interval was traversed, while mode `1` exits on the first **present** ID.
The `seta` at `1801ca9fd` determines inclusion, not exclusion.

The inspected Russian resource stores selector **2**, byte `195767`: reader
`1801c2940` expands this byte into acoustic raw `+60`; initialization
`1801b0760` forwards raw `+60` into the type227 descriptor `+20` and constructor
`1801c95d0` copies that to context runtime `+28`. Therefore the actual RU
configuration uses `SameLineAll`, including overlapping windows. The acoustic
prefix loader exposes both `StoredContextSelector` and `ContextSelectionMode`.
These resource values do not determine automatic line or scope identities.

```powershell
python tools/LegacyPriorContext.Probe/reference_controls.py .artifacts/ru-prior-context.json
dotnet run --project tools/LegacyPriorContext.Probe -c Release -- .artifacts/ru-prior-context.json
```

This component does not enumerate or accept previous30-network windows, discover
line/scope identities, fit reference geometry, produce cursive glyph cuts or
establish recognition accuracy. Those remain explicit inputs or further work.
