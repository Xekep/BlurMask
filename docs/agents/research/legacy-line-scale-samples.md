# Legacy Y-extremum scale samples

`LegacyLineScaleSamples` ports the prepared-stroke sample producer
`180068fa0`. The upstream estimator classes `2e2` and `2e3` invoke it through
their input operations `180069c80` and `180077140` respectively.

For each strict Y turning point it measures the chord from the previous
turning point or start: `max(abs(dy), abs(dx))`. An explicit native record flag
changes the horizontal term to `.5 * abs(dx)`. Flat Y plateaus use their last
point when followed by a reversal. The final endpoint contributes only when
its Y differs from the previous turning point; a purely horizontal stroke
produces no samples. A horizontal endpoint plateau can still change the final
chord's X extent when the endpoint's Y differs from the anchor.

`Extract` accepts prepared float32 coordinates. `PrepareAndExtract` additionally
ports upstream removal of coordinates nearly equal to the last accepted point:
each coordinate's tolerance is
`max(max(abs(a), abs(b)) * 2^-22, 2^-23)`, with a strict comparison of float32
coordinate difference against that tolerance. Either coordinate exceeding its
tolerance keeps the point. The first point is kept. Input is not mutated.

The half-horizontal flag remains an explicit caller input. Choosing it requires
upstream item/record associations; it is not inferred from glyph shape or a
recognized character. Stroke grouping, associated record counts, the estimator
profile and line clusters remain separate. The `219` geometry scale selector
is not assumed to consume this exact sample list without restoring its own
producer links. In particular, the `2e2` estimator's own scale selector
`18006b8c0` uses a different resolution floor and record-count weighting from
`219`'s `18015af40`.

Own independent Python controls retain the decompiled conditional ordering,
while the library uses an equivalent strict-extremum predicate. Managed and
Windows Native AOT runs pass 916 raw/prepared comparisons. Additional analytic
checks cover turning-point plateaus, excluded horizontal endpoints, half-X
weighting, unchanged caller input, finite-coordinate rejection and cancellation.
There is no SDK execution or automatic-baseline accuracy claim.

```text
python tools/LegacyLineSamples.Probe/reference_controls.py .artifacts/legacy-line-samples-controls.json
dotnet run --project tools/LegacyLineSamples.Probe -c Release -- .artifacts/legacy-line-samples-controls.json
```
