# Owned English-lite first preprocessing

`LegacyOwnedEnglishLiteFirstPreprocessing.Prepare(model, rawRow)` binds the
actual selected `LegacyFirstAcoustic119WireModel`, its immutable source247/110,
and the source's ordered24A/102,24C/100,265/102 child owners. Input is one
already-selected ordinary row of physical strokes, with NEW/no-prior/no-event
source state. It is not a source grouping factory or an English recognizer.
The full cursive profile's additional feature settings are explicitly refused.
No Russian profile, category table, frame or context vector is substituted.

The owner performs ordinary167210 duplicate suppression, original15ABE0
direction metrics, actual English24A estimation and16E570 deskew. It then counts
24C directions over the same post-deskew points. Below20,24C returns unchanged;
265 executes using the actual English smoothing profile and fitted height.
The resulting factor200 chronology resolves back to retained raw stroke/point
keys. All public point/count/key/probability buffers are detached copies; the
internal source and chronology cannot be mutated through recoverable memory.
The source247/119 immutable references and actual child identities are retained.

## Native assignment and branch closure

Fresh163970 clears ingress9C.167210's ordinary kind0/no9C branch writes-1 to
physical1C+4.24A's16E850 writes the separate input descriptor+10 guide index;
its source callback169750 and16E570 deskew do not write physical1C metadata.
Thus the24C physical filter is genuinely-1 even though the descriptor has a
guide. This is derived inside the factory, not supplied by a Boolean.
See [the preceding static plan](legacy-noinitial-first-feature-plan.md).

24C table2DFE20.V18=176A10 reads count44,XY28,prefix38 and physical1C58.
176A7A tests physical+4==-1;176A91 resets signed direction per stroke.
Strict Y-decrease and Y-increase use176AAB/176AC0; equal Y retains direction.
It counts the beginning/change of every strict run and one final endpoint for
a nonhorizontal stroke. Single points and horizontal strokes add zero.
176B0B compares the signed total with20;176B0F jumps to the unchanged return
at176F92. Count>=20 is a **temporary unsupported positive24C branch**, refused
before265. The real angle-selection/shear branch is the next required port.
24C wire176FB0 already stores radians; external setter176980's degree
conversion is not applied to serialized angles.

The actual first ENlite source is8843,24A16842..16878,24C16878..16902 and
26516902..16950. Offsets are audit results, not factory constants. The owner
resolves source children by their actual resource identities/order. English
24A slopes are approximately+/-0.5773503,dense threshold50;265 extra is0.
These are materially different from Russian first profiles.

## Checks and limits

The dedicated probe uses actual119/source247 with96 raw input rows:90 reach
265,6 genuinely cross the post24A24C threshold and refuse. It records2613
prepared points, full raw/deskewed/smoothed geometry and every factor200 key and
resolved raw identity.140 separately marked prepared counter controls include
19/20/21, flat runs, duplicate points and multiple physical strokes. These are
algorithm controls, not evidence of recognition quality.

The portable Python audit parses the real raw247 resource independently, checks
the processor payloads, computes24A using an independent Gaussian normal-system
solver, reproduces float deskew ordering, and solves265 splines with dense
NumPy matrices. It checks exact counts/key maps/raw identities and tight
geometric tolerances. Its spline input is the separately checked float deskew
boundary; this is not a claim that independent IRLS solvers are universally
bit-identical. Managed/Native AOT full capture equality is checked separately.
No vendor binary is executed. The static audit pins18 counter instructions and
composes the prior source/physical-assignment/processor-order proof.

This stage does not normalize float glyph ranges, merge pen-ups, produce
217/199 features or219/201 network inputs, construct activation objects, evaluate
NNs, construct a graph or select words. Those remain separate actual-owner
joins. Portable raw/prepared bounds remain1..256 strokes and4096 points.

```powershell
python tools/LegacyEnglishLiteFirstPreprocessing.Probe/reference_controls.py E:/BlurMask-build-artifacts-20261014/enlite-preprocessing-controls.json
dotnet run --project tools/LegacyEnglishLiteFirstPreprocessing.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/enlite-preprocessing-managed-artifacts -- models E:/BlurMask-build-artifacts-20261014/enlite-preprocessing-controls.json E:/BlurMask-build-artifacts-20261014/enlite-preprocessing-managed.json
python tools/LegacyEnglishLiteFirstPreprocessing.Probe/audit_output.py models E:/BlurMask-build-artifacts-20261014/enlite-preprocessing-managed.json
python tools/handwriting/audit_english_lite_first_preprocessing.py E:/BlurMask-build-artifacts-20261014/enlite-preprocessing-static.json
```
