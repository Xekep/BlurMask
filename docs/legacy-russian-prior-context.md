# Legacy prior context selection

`LegacyPriorContextSelection` restores the checked `1801ca5f0` filtering of
precomputed accepted observations. Caller supplies target line/scope identity,
target physical indices and previously accepted groups with inclusive physical
index ranges, mean X and28 neural probabilities.

It keeps observations on the same line and optionally in the same context
scope. The normal mode excludes every observation whose inclusive physical
range intersects the target ink. The alternate `excludeGroupStartOnly` mode
excludes an observation if its start index matches any target physical index,
preserving the inspected native zero/nonzero selector
behavior. Boundary targets have no selected prior context. Output retains
caller observation order and composes with `LegacyContextWeighting` and
`LegacyFloatInputQuantization`. Empty accepted context becomes28 fixed −1024
channels.

The bounded implementation accepts at most1024 observations and4096 target
indices0..65534; accepted group ranges begin at1..65534. Native window enumeration
uses stroke0 as the earlier X reference and starts observed windows at1, so a
target containing physical index0 remains valid and cannot overlap those windows.
It rejects invalid group ranges or neural probabilities.
Sorted target indices and binary searches implement the native selection
semantics without enumerating every integer in each prior range. These bounds
are an original defensive API limit, not an asserted SDK or product limit.

Managed and Windows Native AOT controls pass640 independent Python comparisons
whose reference uses set/range intersections. Analytic tests cover inclusive
endpoints, alternate first-index mode, unordered/duplicate target IDs,
line/scope/boundary exclusion, weighting and fixed conversion, malformed input,
work bounds and cancellation. No vendor model or ink asset is included.

```powershell
python tools/LegacyPriorContext.Probe/reference_controls.py .artifacts/ru-prior-context.json
dotnet run --project tools/LegacyPriorContext.Probe -c Release -- .artifacts/ru-prior-context.json
```

This component does not enumerate or accept previous30-network windows, discover
line/scope identities, fit reference geometry, produce cursive glyph cuts or
establish recognition accuracy. Those remain explicit inputs or further work.
