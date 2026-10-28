# Current ordinary row acoustic input

`LegacyOrdinaryRowAcousticInput` connects a fresh **already-selected** ordinary
row to its checked current source247 geometry/smoothing profile, source point
keys,297 cuts and the reusable prepared glyph evaluator. Its frame comes from
the resource-backed24a fit, rather than a caller baseline or arbitrary bbox.
It does not perform global row selection or native word winner/acceptance.

## Current-source ownership

In the checked Russian resource, outer source247's processor list has six
items starting6104:249,24b,24a,24d,29b,265. Its24a node187976 is third in that
list, **after**249's text-recognition call. Selecting187976 merely because it
belongs to the outer resource does not prove the NN's baseline profile.

Outer249@6104 contains a source247 at6120. Its four early processor flags are
all zero, so it does not create another2ab/2e3 grouper or2d0 orientation pass.
Its postprocessor list starts11421 and has exactly **24a@11421 then265@11457**.
Source1680d0 runs the owned clone list sequentially through virtual18. Type24a
uses16e850 and source265 uses173c10. The profile reader validates this exact
supported schema by reusing the source247/v10d reader, parsing the two typed
nodes and rejecting early processors or another postprocessor count/order.
It takes an explicit current source-node offset; a global property/controller
factory must supply that provenance when building the whole recognizer.

Orientation's primary249@5477 is a different owner: its source247@5493
contains2ab@5638 and only24a@5789 in its postprocessor list. It is not exchanged
with the current per-row source6120.

Source218 virtual40 at168b90 simply returns state78+40*row index when state80
is nonzero, otherwiseNULL. It is not itself an automatic line estimator.
The earlier16fec0 investigation belongs to29b's later guideline/query path;
it must not be silently substituted for this pointer getter or the24a producer.

## Connected processing order

The supported fresh no-prior/no-associated/no-external-guideline path:

1. Ordinary167210 ingress retains raw point ordinals after adjacent-raw dedup.
2. Source15abe0 computes exact orthogonal/corner ratios **before** deskew.
3. The current24a profile supplies16e0c0's sparse/dense fit. Fresh16a130 flag168
   is zero, so the frame height is the fitted band. The forced-height branch
   used by orientation's temporary249 state is not guessed here.
4. 16e570 deskews the owned coordinates and frame. Guideline metadata does not
   alter the baseline/height in this fresh no-guideline scope.
5. 173c10 uses the existing frame record's height for265. This differs from
   orientation's independent axis scale. Ratios still come from step2.
6. Geometry and source keys at factor200 are produced by the shared spline
   core. The original point records and physical probabilities retain their
   raw input identity; no duplicate-induced timestamp or scalar averaging is
   introduced. The original unresampled helper's convenient factor1 is not
   used to truncate these fractional positions.
7.297 receives the smoothed coordinates, the prepared frame and original
   per-physical enclosure vector. Its inserted point map composes through the
   owned factor200 chronology. The cut result retains a private owner identity.
8. `CreateEvaluator` checks that owner, source kind and frame, then connects the
   existing context/normalization/glyph stage. The evaluator's context/search
   policy remains its explicitly documented diagnostic policy.

## First and second networks are distinct

Source247@6120 also contains first graph297@11506,21b@12727 and ordinary21d
at13082. A static prefix audit finds initial network13103..19094, context
selector2 and **no dynamic257**. It has two network slots:19195..105190 with
input247/hidden200/output180, and105197..187592 with229/200/180; the remaining
five slots have no networks and zero fixed weights. The two active slots have
stored flags1/1 and mode2. These are the no-baseline trajectory/raster widths,
not the four-family second ordinary21d@189734 currently loaded by ModelBundle.
The no-dynamic mode2 forwarding and complete first/second pass binding still
need recovery. The prepared row factory does not pretend they are identical.

The actual probe therefore labels its NN consumer **supplied second model**.
It proves resource/profile→geometry/map→real neural evaluation, while leaving
the appropriate native first-pass policy explicit. It cannot be treated as a
completed SDK recognition pipeline or evidence of Russian cursive accuracy.

## Verification

`tools/LegacyRowAcousticInput.Probe` has a vendor-free source247/24a/265
fixture, truncated-input and order rejection, original endpoint chronology,
owned factor200 mapping and297 insertion checks. Actual mode:

```powershell
dotnet run --project tools/LegacyRowAcousticInput.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
```

Own printed MIR has five physical strokes, with the three I strokes evaluated
together. Two scale/translation variants produce identical complete180-cost
vectors after this pipeline. M's first raw class is Cyrillic`м`, I's first is
Cyrillic`И`; R still prefers Latin`P`, with Cyrillic`Р` fourth. This is a narrow
diagnostic, not a handwriting corpus or success-rate claim. Source ownership
and foreign/frame mismatches are checked. No vendor DLL or local GUI executes.
