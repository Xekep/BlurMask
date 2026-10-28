# First-result source line deskew

`LegacyFirstResultLineDeskew` ports Text16F170, called after the main24B
first-result720→C1A fit. It takes the actual40-byte guide, descriptor physical
24/28 interval,720-row34/38 interval and retained parent point/range owners.
It does not reuse the first6120 frame or old16E570 tolerance fallback.

Slope is read from guide8. Native computes float32 slope², promotes it to
double for `sqrt(1+slope²)`, rounds the root to float, then computes reciprocal
cosine. Sine is derived from rounded `1-cosine²`, with its sign negated only
for positive slope. The rotation center uses the horizontal midpoint of the
selected physical points and `centerX*slope+guideBaseline`. Translation and
point transforms retain binary32 operation order, including zero terms.

Only selected physical buffers rotate. The copied-guide flag skips guide0/4
updates but still rotates points and remeasures source geometry. Otherwise
guide baseline is rotated and height is the rotated lower coordinate minus
that baseline, clamped to actual runtime minimum40. This differs from the
16E570 near-zero-height15%-width fallback. Slope8, pre-deskew baseline/height
C/10 and all other guide metadata remain byte-for-byte unchanged.

Each active720 row with source ranges obtains its new bounds from actual
retained points after rotation. Inclusive24-byte ranges can cross physical
buffers, include cropped endpoints, overlap and include unrotated buffers
outside the selected physical interval. They are not merged or replaced by a
whole-word envelope. A native row with no source ranges instead rotates its
two stored opposite corners, keeping their order even when it no longer forms
an ordered axis-aligned envelope. The port does not silently sort those values.
All nongeometry source bytes and source range identities are retained.

`LegacyFirstResultLineRow.FromFirstResult` connects the owned native720-row
producer directly. Portable rows snapshot720 bytes and independently owned
ranges; no native pointer is dereferenced or invented. Result points, rows and
guide are immutable snapshots with defensive-copy accessors. Cancellation,
finite arithmetic checks,65,536 total retained ranges and1,048,576 range-point visits are portable safety
limits. Native allocator behavior is not emulated.

The probe independently generates384 original binary32 affine/range cases:
copied-guide flags, zero slope, both slope signs, empty physical points,
different physical/source intervals, cropped and overlapping memberships,
zero-range opposite corners and unchanged fields. Float results and full
guide/source buffers are compared by bits. It additionally composes actual
pinned RU categories and24B profile with189 selected-label/cropped-point
source720→C1A→deskew fixtures. Glyph labels and selected alternatives are
prepared, and the absent parent alphabet marker service is explicitly disabled
in those fixtures; these are not inferred native winners or OCR accuracy.

```powershell
python tools/LegacyFirstResultDeskew.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/main-first-line-next/deskew-controls.json
dotnet run --project tools/LegacyFirstResultDeskew.Probe -c Release -- D:/BlurMask-build-artifacts-20261011/main-first-line-next/deskew-controls.json models/ru-RU/ru_RU-ak-cur.lite.res
dotnet publish tools/LegacyFirstResultDeskew.Probe -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/main-first-deskew-aot
python tools/handwriting/audit_first_result_line_deskew.py --output D:/BlurMask-build-artifacts-20261011/main-first-line-next/deskew-binding-audit.json
```

The auditor statically pins Text SHA18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f.
Actual parent source/alphabet owner,24B lifecycle and the following24A/24D/29B
main processors remain separate. This closes the verified point/bounds stage,
not the whole main247 frame or the application's connected-word font metrics.
