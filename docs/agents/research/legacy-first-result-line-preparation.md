# First-result720 to main line preparation

`LegacyFirstResultLinePreparation` restores Text `16F6F0`: actual first-result
720-byte rows become 584-byte C1A inputs. `LegacyFirstResultLineProfile` reads
the actual Russian outer-source24B at187937, version103. This is the main source
postprocessing branch after first recognition, not the separate first row's
24A at11421 or its6120 frame. No vendor DLL is executed.

The native chain is24B `170400` → `16FEC0` → `16F6F0` → Ink C1A `0221A0` →
`021110`. The source vector header begins at secondaryC8: its first qword is
the720-row pointer. Descriptor34/38 supplies a half-open source-row interval.
Descriptor24/28 independently selects the inclusive physical-input interval.

|720 source offset|584 prepared offset|Meaning|
|---|---|---|
|14 uint32|00 byte|known flag: nonzero and bit13 clear|
|28 primary label|01/02 bytes|actual marker IDs: width clipping/downweighting|
|18..27|08..17|left/right/top/bottom|
|28..4F ushort labels|04 count|stop at firstFFFF, at most20|
|50..EF pairs|18..B7 pairs|base weight/coefficient|
|140..18F ints|B8..107 ints|profile kind|
|190..22F pairs|108..1A7 pairs|main/alternate top|
|230..2CF pairs|1A8..247 pairs|main/alternate bottom|

Unconsumed prepared slots remain zero even when source slots contain other
data. The transferred bounds belong to the native source row; the physical
input envelope is separately scanned from actual retained parent points.
Neither is invented from a printed character or substituted from a whole-word
box. Owned `LegacyFirstResultSourceRecord` instances can be passed directly.

`170220` resolves F0002,F0003,hyphen,underscore,U+30FC,U+4E00 and U+FFFD in the
parent source alphabet. `LegacyFirstResultLineMarkers` requires those actual
IDs explicitly;FFFF means failed lookup. The exact parent alphabet service
owner/lookup remains unfinished, so a diagnostic `TryGetLabel` fixture is not
presented as the native lookup lifecycle. No language aliases are injected.

The24B resource reader170790 stores slope tangents directly. The unrelated
caller setter16F080 accepts angles and computes tangents; the resource reader
does not repeat that conversion. `PrepareFitProfile` follows170220: source10
physical resolution0 yields1e-6; otherwise `(resolution/25.4)*.75` in float32.
The maximum width comes from a sole explicit source68 line with first uint≥2
and positive third uint converted to float, multiplied by.25; otherwiseFLTMAX.
Known-glyph cap is the actual fresh runtime48 zero. Source physical resolution
and explicit line metadata remain actual caller inputs, not glyph heights.

`LegacyCharacterLineFitCompletion` supplies the exact `0221A0` wrapper around
the existing independently checked IRLS. Positive-affinity primary output is
retained only when its height is at least5% of the physical input height.
Rejected/too-small primary output enters the native midpoint/minimum-height
bounding-box fallback, with slope zero. A distinct public `Basis` labels this
fallback; it is not reported as successful neural inference or calibrated
recognition confidence. Native float32 comparisons and equality boundaries are
preserved. Portable nonfinite guards, cancellation and work budgets are extra
checked-library contracts.

The probe has512 independent byte-transfer controls,128 NumPy-backed full-fit
completion cases,128 native fallback boundary cases and24 metadata profiles.
It also composes the actual pinned RU24B, first sampler categories and alphabet
with189 explicit selected-label/cropped-point source fixtures (183 primary
fits). Those labels and selections are prepared: these checks are not Russian
handwriting accuracy or a native winning-path test.

```powershell
python tools/LegacyFirstResultLine.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/main-first-line-next/controls.json
dotnet run --project tools/LegacyFirstResultLine.Probe -c Release -- D:/BlurMask-build-artifacts-20261011/main-first-line-next/controls.json models/ru-RU/ru_RU-ak-cur.lite.res
dotnet publish tools/LegacyFirstResultLine.Probe -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/main-first-line-aot
python tools/handwriting/audit_first_result_line_preparation.py --output D:/BlurMask-build-artifacts-20261011/main-first-line-next/binding-audit.json
```

The static auditor pins Text SHA18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f
and Ink SHAb553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a.
Full24B lifecycle/16F170 deskew, subsequent24A/24D/29B main processing and the
actual decoded winning glyph/source/alphabet owner are separate gaps. This
bounded port does not claim the main247 frame or the application's connected
word font-size provider is complete.
