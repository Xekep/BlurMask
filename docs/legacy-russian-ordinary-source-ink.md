# Ordinary resource → physical source ink → enclosure score

`LegacyOrdinarySourceInk` connects the restored source-stroke enclosure service
to resource configuration and physical float input. It owns the filtered stroke
snapshot, original point ordinals, source chronology and computed per-stroke
probabilities. This is a scoped ordinary source219 ingress, not the complete
source219 lifecycle or automatic line metadata.

## Native resource forwarding

Resource247 reader `1801d4950` reads its134/133 parent prefix and source-kind/mode
fields, followed by source parameters through `1801d41e0`. Resource virtual80
`1801d62d0` copies the112-byte parameter record from resource+C0 into the init
descriptor. `180150d00` passes that descriptor to source219. `180163970` copies
its+40 flag into source runtime+48 and allocates type2ad when enabled.

The loader supports the checked unencoded resource247/v10d, parent134/v201 and
parent133/v100 envelope. It bounds every read and nested envelope, uses the
existing inline alphabet parser, and skips metadata bytes as opaque data. It
does not execute license/activation checks, vendor DLLs or resource callbacks.
No other resource generation is silently treated as equivalent.

In the committed Russian acoustic resource, source kind is0x135; source parameters
start at byte5285, and the enclosure-enable flag at byte6088 is1. That is a parsed
resource value, not a default chosen by the implementation. An explicit profile
can also be supplied by a caller with a separately known source configuration.

## Native ordinary ingress

`180167210` keeps the first physical point. For each following raw point, an axis
is different when

`abs(double(a))+abs(double(b)) < abs(double(float32(a−b)))*8388608`.

It retains a point if either axis differs. The comparison uses the **immediately
preceding raw point**, including points which were dropped. Comparing against
the last retained point changes the output for consecutive small ULP increments.
Assembly `180167627..180167674` confirms float32 subtraction followed by double
absolute values, addition and multiplication. Physical stroke order is preserved.

The ingress point-map stores retained points' original raw ordinals. Ordinary
source chronology is `(0,inputStrokeOrdinal,originalRawPointOrdinal)`; the source
snapshot constructs the existing `LegacySourcePointChronology` with these keys.
This preserves gaps caused by duplicate suppression, unlike renumbering the
retained points from zero. Prepared cut insertion can subsequently compose that
map with its established chronology adapter.

Ordinary physical record+4 starts−1, so enabled source processing calls type2ad
for each ordinary stroke. The component supplies the same filtered float geometry
snapshot to `LegacySourceStrokeEnclosure`. When the explicit resource profile
disables enclosure, its output vector is zero. Inserted/resampled candidate points
are not passed to the service instead of source points.

`Strokes`, `OriginalPointIndices`, `Chronology` and `SourceStrokeProbabilities`
are intended as one producer result. Cut policy and final acoustic-cache consumers
must use this shared vector with the corresponding physical stroke ordinals.
`LegacyCursivePreparedInk.Create(result.Strokes)` starts candidate preparation from
the filtered source; it does not redefine the source enclosure geometry.

## Limits and verification

This endpoint excludes custom/opaque source records, special input kinds,
page/baseline metadata, automatic raw-input record grouping, callback coordinate
orientation and source flag rewriting. Native routines `18015a3b0` and
`1801629c0` before enclosure update item/group/baseline metadata; the ordinary
physical float buffer used by the enclosure service is the filtered ingress
buffer. The endpoint does not manufacture the metadata those routines compute.

512 independent vendor-free ULP/duplicate/coordinate fixtures verify retained
indices, geometry and source keys;12 own resource variants verify source mode,
metadata-marker lengths, all truncations and enabled/disabled forwarding. The
composed square fixture verifies resource→ingress→enclosure analytically. The
actual Russian resource is checked separately; the own printedМИР fixture yields
computed `[0,0,0]` scores. That result is specific to that ink, not a universal
source probability and not evidence of handwriting recognition accuracy.

```powershell
python tools/LegacyOrdinarySource.Probe/reference_controls.py research/myscript/output/RussianAssetsSearch/legacy-ordinary-source-controls.json
dotnet run --project tools/LegacyOrdinarySource.Probe/LegacyOrdinarySource.Probe.csproj -c Release -- research/myscript/output/RussianAssetsSearch/legacy-ordinary-source-controls.json models/ru-RU/ru_RU-ak-cur.lite.res
```
