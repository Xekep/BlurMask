# Ordinary source scale and prepared neural line frames

`LegacyOrdinaryLineScaleSource` closes the ordinary source219 scale producer
and resource forwarding. `LegacyPrepared2e3LineFrames` then composes that owned
source with the existing geometry/pair networks, native unknown-row grouping,
class24a fit and deskew. The source, inferred scale, groups and line records are
available together, rather than requiring a caller to guess a baseline from
the ink bounding box.

## Exact scale producer and resource fields

Resource247/v10d reader `1801d41e0` reads four floats into the112-byte source
configuration record at+10/+14/+18/+1C. Its forwarding `1801d62d0` and source
initializer `180163970` copy them into source219+18/+1C/+20/+24. These are the
quantile, sparse quantile, blend numerator and associated-record weight used by
`18015af40`, already implemented by `LegacyLineScale.Select`.

In the original Russian resource, the bounded existing source loader locates
these floats at byte5285: approximately0.96,0.85,3,10. The new parser reads that
validated offset, not a fixed byte offset or language-wide guessed profile.
It preserves all four values, checks their scalar bounds and does not execute
resource callbacks or license/activation logic.

Ordinary source ingress `180167210` calls `180159e00` on each physical stroke's
already filtered point interval. Its arguments are explicitly halfX=0 and
maximumTurns=-1. This unlimited unweighted extremum/chord branch matches
`LegacyLineScaleSamples.Extract`; it does **not** use `PrepareAndExtract` and
therefore does not perform an extra last-accepted-point duplicate filter.
Samples are concatenated in physical ingress order. Every ordinary physical
record+4 remains-1, so the associated-record count consumed by `18015af40` is
zero. Opaque/custom/associated records and limited-turn branches remain outside
this API's scope.

The caller supplies input resolution. The native resolution/25.4 floor and
no-sample fallback remain in the existing scalar selector. No DPI is guessed
from a desktop, model locale or letter size. The factory computes scale from
the source model profile and points before invoking line grouping.

## Connection to2e3 and geometry

Source `18015b260` forwards primary and secondary scales through2e3.virtual38.
The concrete `180077120` setter requires positive primary scale, stores it at
runtime+190, sets byte+12, and ignores the secondary argument. The composed
factory uses that primary float for pair features, horizontal window width and
native unknown-row grouping pitch.

Window-width multiplier and overlap stay explicit. The unchanged2e3 constructor
`180076780` values are5 and0.75, as used by the diagnostic, but arbitrary source
property overrides are not silently treated as those defaults. The caller
also selects the class24a resource profile; the factory does not guess which
of the three nested original profiles is bound to a particular source callback.

For one active ordinary physical stroke, `18007d7c0` dispatches to `18007ca80`.
The latter creates one group and copies active physical IDs using the+150 byte
vector's count at+158. No adjacent pair exists and no pair network is run.
The new factory preserves this upstream singleton branch, separate from the
frozen07c160-only selector. This is necessary to give a single joined physical
stroke line geometry; it does not prove recognition of joined handwriting.

For multiple ordinary unknown strokes, the factory runs actual30/50-state
networks with the native skip-self consumer and native unknown-row selection.
Physical IDs are sorted before the geometry callback as in `18015b260`.
`LegacyAutomaticLineEstimator` supplies the selected resource's sparse/dense
fit; `LegacyLineDeskew` supplies the restored24a postcallback transformation.
All calls accept cancellation, own their returned scratch/copies and leave the
raw caller points unchanged. Unassigned strokes remain explicitly-1.

## Verification and remaining boundaries

384 independent raw-source controls check5,526 samples and primary/secondary
scales, including repeated points, flat/single-point strokes, resolution floors,
different quantiles and sparse/dense blends. Original resource reads, malformed
scalar fields and cancellation are checked. The two real line models then run
live through the complete prepared source/frame compositor into the existing
Russian four-family glyph/final-cache/word diagnostic.

The original own printed MIR fixture derives scale80 from the real0.96 source
quantile; no baseline, height or scale is supplied for recognition. One row and
two translated rows, each tested with all three original24a profiles, retain
`мир` as the best word at cost54456. The former explicit90/80 frame had it fourth.
This remains one original printed fixture, not a held-out corpus, actual joined
Russian accuracy or production/UI acceptance.

Full07a850 attachment/reordering, vertical sweep/pruning, source-driven2e2/2e3
selection, custom/guideline/associated input, incremental scale hysteresis,
native word winner/confidence and final typography remain separate boundaries.
Other cache flags and the diagnostic dictionary beam still have explicit
original policies. No vendor DLL or GUI is executed.

```powershell
python tools/LegacyOrdinaryLineScale.Probe/reference_controls.py D:/BlurMask-checks/source-scale.json
dotnet run --project tools/LegacyOrdinaryLineScale.Probe -c Release -- D:/BlurMask-checks/source-scale.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res
```

The generator and probe with only the controls path are vendor-free. Optional
resource arguments enable the explicitly scoped live model/word diagnostic.
