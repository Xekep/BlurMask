# Owned fresh singleton main-line preparation

`LegacyFreshSingleStrokeMainLine` binds one fresh main247 input to its selected
first249 child and consumes that invocation's actual selected rich275 word.
The input is one physical pen stroke; it can contain multiple connected letters.
This is a checked intermediate stage, not a completed recognition endpoint.

The native raw descriptor comes from159810/167210, including88 initialized bytes,
ordinary kind0, physical endpoints0/0, source interval0/0, unset line10, page and
auxiliary indices, and the first retained point.161120 immediately succeeds for
one descriptor.15EAA0 does not reorder the fresh boundary−1. The singleton branch
of15F050 submits physical index0 without invoking the multi-stroke group provider;
16AA00 copies its point and metadata owners in identity order. No page lines,
auxiliary rectangles or prior source exist in this entry point.1629C0 therefore
leaves line10=−1 and guidecount0. It does **not** fit a bounding-box guide here.

The helper submits the parent's retained point buffer to the selected first
factory; the original user duplicate indices are not substituted for157C60's
child-to-parent submission map. `ApplyFirstResult` requires reference identity
with that factory, then calls the actual275/pool/graph source materializer. Labels,
individual glyph ranges, geometry, category profiles and weights come from that
selected result, not supplied expected text. The loaded249 category owner carries
the serialized source score exponent (.75 in the pinned RU resource); the raw
first factory's constructor exponent1 is a separate scope.

Actual source720 count produces descriptor34=0/38=count. Existing24B170400 creates
the main guide through16F6F0/C1A and deskews the owned physical points/row boxes.
For a nonempty result it assigns line0, so the following24A16E850 skips this
descriptor instead of fitting a second line. An empty result remains unknown;
the required subsequent24A geometry branch is deliberately not implemented here.
`SourceRows` contains records before24B; transformed rows and points are in
`Output.Deskew`. GuideC/10 preserve the original fitted baseline/height;
guide0/4 carry the deskewed NN frame. They are not modern Document font metrics.

170400 leaves some inactive temporary guide bytes unspecified. The portable
wrapper zeroes those ignored bytes for deterministic storage; it claims exact
defined geometry/indices, not a vendor uninitialized stack bit-pattern.
Actual physical resolution is mandatory:0 is the source constructor value,
not an assumed screen DPI.

Static runtime table binding proves24D uses177280, which estimates shear and
changes physical X/source138/720 bounds;29B uses175350, which estimates normalized
inter-glyph gap/source9C. These later stages,265 resampling, multi-physical15F050
grouping, complete first winner selection, main recognition and production font
integration remain outside this helper. Preserved baseline/height alone does not
justify skipping the later geometry needed by main decoding.

The headless probe runs64 independent Python full88 descriptor controls plus an
actual RU first NN/recurrence result with10 groups and one selected source720 row.
Six raw points (one duplicate) become five retained parent/child points. That
actual C1A primary-profile case gives baseline90 and bodyheight40 while the raw
stroke's box is80 high. This demonstrates the category/body metric in this
bounded path, not handwriting accuracy or native winner equivalence. Foreign
factory ownership, cancellation, nonfinite resolution and snapshot mutation are
also checked. The first recurrence probe uses the existing bounded ordinary
letter/digit request set; complete native candidate enumeration, terminal selection
and calibrated confidence remain unresolved. No expected character is used to
choose or correct the selected result. Native PE auditing is opt-in and static only.

```powershell
python tools/LegacyFreshMainLine.Probe/reference_controls.py D:/probe/controls.json
dotnet run --project tools/LegacyFreshMainLine.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json D:/probe/controls.json D:/probe/main-line.json
python tools/LegacyFreshMainLine.Probe/reference_controls.py --audit D:/probe/main-line.json
python tools/handwriting/audit_fresh_single_stroke_main_line.py --output D:/probe/static-audit.json
```
