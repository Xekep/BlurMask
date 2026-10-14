# Source-owned closed-primitive confidence

`LegacyFirstOwnedGlyphConfidence` closes the source branch in first-model
Text1BCBD0 for F0001 when its alphabet has no neural prototype. The existing
source-free `ReadSymbolScore` still refuses that retained label.

The object's170 field is the source forwarded from initdescriptor4;178 is its
registered source218 interface, not a knowledge-model service. Graph21B
virtual90 (`1A7970`) copies the ordinary candidate's range-vector. The consumer
dereferences its first physical-stroke ID and calls source21860 (`168C00`),
which reads original physical record+C.1BCBD0 writes that raw value to the
candidate vector without sigmoid, neural-family mean, averaging or clamp.
Record+C is produced by the existing2AD cross-stroke enclosure service. It
describes ink enclosing other ink, not an isolated circle.

The reusable endpoint owns the fresh selected-source factory, its cuts,
candidate graph and actual first NN/cache groups. It verifies source
fingerprints, prepared-ink identity and each normalized range against the
graph and chronology. Probability comes from **the first graph range**, whose
native order can differ from normalization chronology. Missing or rank20-and-
later candidates receive zero before that source branch. Other candidates
use the source-derived neural mapping and calibration unchanged.

The plain-word materializer now uses this endpoint for its candidate vectors;
NaN primitive slots and refusal solely because of a primitive first candidate
are removed. Selected glyph conversion stays restricted to plain letters and
digits. This does not implement literal F0001 decoding, split words, case/alias
conversion or the main240 linguistic winner.

32 own raw-source fixtures pass ingress deduplication, actual first frame,
smoothing, cuts,108 graph groups, networks and final caches.28 actually
retained F0001 candidates preserve raw source probability. Controlled retained
candidates test that transport on all groups; they do not measure handwriting
classification accuracy. Rectangles vary scale, translation, direction and
duplicate raw points. Isolated contours return zero; enclosing contours match
an independent area/perimeter oracle. Foreign frames/cuts/groups, cancellation
and rank20 cutoff are guarded. The mixed-script primary fixture remains
`MИP:41268`, not evidence of successful Russian recognition.

```powershell
dotnet run --project tools/LegacyOwnedClosedConfidence.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json D:/owned-closed-audit.json
python tools/LegacyOwnedClosedConfidence.Probe/reference_controls.py D:/owned-closed-audit.json
python tools/handwriting/audit_first_owned_closed_confidence.py --output D:/owned-closed-static.json
```

No vendor DLL is executed.
