# Ordinary source endpoint metadata and primitive scope

`LegacyOrdinarySourceEndpointMetadata` restores the ordinary source219 writer's
end-key table and the checked source predicates consumed by Text `1801bb020`.
The graph word endpoints expose the opt-in named argument
`useNativeSourcePrimitiveScope: true`; their default remains false. This opt-in
replaces exactly three fields in the caller's cache context: the boundary/single
normalized-stroke predicate, the endpoint-zero predicate and source probability.
Pruning, fallback suppression, knowledge-rule suppression, related-form policy
and penalty-label memory remain caller inputs.

## Two distinct point metadata tables

Source218 interface table `1802df340` virtual90 points to `180168d00`, which
returns a twelve-byte start key from secondary+f8. VirtualA0 points to
`180168e10`, which uses the same global-index, prepared-map division/ceiling and
metadata-index lookup but returns the twelve-byte **end key at secondary+108**.
They must not be treated as two aliases of the start-key chronology.

Ordinary ingress writer `180167210` creates both tables. The first two fields
are copied from the start key: zero source hierarchy and physical input-stroke
ordinal in the checked unsampled/default branch. Start-key field3 is the
retained original raw point ordinal. End-key field3 is the next retained raw
point ordinal minus one, or the original physical raw count minus one at the
last retained point. Thus each retained point owns an interval of raw input,
including immediately trailing discarded raw points.

The original raw counts are supplied explicitly: they cannot be reconstructed
from retained start ordinals if the final raw points were discarded. The public
constructor checks bounds and retained-ordinal consistency; the graph endpoint
provides counts directly from its validated raw input. The end-key snapshot
passes through the already restored prepared point-map composer. Inserted cut
points retain the same original interval as their preceding filtered point.
No timestamp, glyph category or semantic character class is inferred.

## Native primitive predicates

The closed primitive branch at `1bb020` lines268-286 requires normalized stroke
count1, nonempty ordered ranges, first range start0 and last range end equal to
the source count of its last physical stroke. It then reads source218 virtual60
for the **first** range's physical stroke. The code does not separately demand
one range or identical first/last physical stroke IDs. Accordingly the data-only
`EvaluateOrderedRanges` takes the resolved normalized count explicitly rather
than replacing it with a physical-range count. Pen-up reduction itself remains
a separate producer.

The knowledge-rule branch at lines464-485 repeats the same normalized-count and
boundary guard, reads virtual90 at the first physical point0 and virtualA0 at
the final range's final point, then requires field3 of both returned records to
be zero. The existing context field is named
`WholeStrokeEndpointCategoriesZero`; the recovered data shows these are raw
endpoint ownership fields, not a supplied glyph category. The API name is kept
for compatibility, while the new result calls the condition `EndpointFieldsAreZero`.
For ordinary unsampled input, a complete one-point raw stroke satisfies this
condition. Two coincident raw points normalize to the same retained geometry
but have end field3 equal to1, so the condition is false.

`Evaluate(glyph)` verifies prepared-source identity before binding ranges;
`EvaluateOrderedRanges` is the explicit data-only form with bounds validation.
`BindContext` supplies the exact three source fields and preserves all other
context values. The opt-in graph endpoint uses that binding after glyph
normalization and before the native final-cache scorer. Symbol masks, line frame,
other controller flags, native winner/confidence and automatic Russian acceptance
are still outside this restored source-scope stage.

## Verification

258 original Python controls assign raw repeated-point runs to independent
ownership intervals. Managed and Windows Native AOT compare5128 prepared end
identities and1940 boundary/normalized-count predicates. These cover inserted
points, trailing discarded raw input, distinct start/end tables, partial ranges,
single normalized versus multiple physical ranges, malformed ownership and
cancellation.32 context variants per whole physical range check that only the
three checked fields are replaced.

Actual Russian resources run MIR with two delayed strokes, an isolated loop
and a contour enclosing another stroke through the graph/glyph/final195-cache/
whole-word lexical consumer. Opt-in source binding reproduces a separately
supplied exact source-scope resolver's retained texts and costs. The enclosing
fixture supplies a positive source probability (approximately0.6981), and the
actual cache consumes it for the closed primitive. A raw dot versus two
coincident raw points demonstrates the endpoint-zero minimum-minus-one rule in
the real cache. These verify transport and source semantics; they do not claim
handwriting accuracy or acceptance confidence.

```powershell
python tools/LegacySourceEndpoint.Probe/reference_controls.py .artifacts/legacy-source-endpoint-controls.json
dotnet run --project tools/LegacySourceEndpoint.Probe -c Release -- .artifacts/legacy-source-endpoint-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```
