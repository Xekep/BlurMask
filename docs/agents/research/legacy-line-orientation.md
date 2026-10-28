# Prepared legacy line orientation

`LegacyLineOrientationProfile`, `LegacyLineOrientationProjection`, and
`LegacyLineOrientationPolicy` recover the numerical and decision stages of
Text's type2d0. They do **not** replace the whole source219 automatic dispatch:
optional processor265's smoothing/resampling and primary processor249's source
callback still have to produce the exact prepared input and child statistics.
The callback in the vendor-free probe is deliberately original and is not a
pretend implementation of249. Russian cursive quality and final native word
acceptance are not established by these controls.

## Checked resource and native path

Static evidence uses `MyScriptText.dll` SHA256
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
No vendor DLL is loaded or executed. In the checked Russian resource,
type2d0/v100 is at5444, primary249/v100 at5477 and secondary265/v102 at6021.
Reader082b80 consumes four float parameters, two optional child nodes, then
`float,byte,float,float,float,byte,byte`. The actual parameters are
`(-90,90,15,.95)`, then `(.5,9,2.5,.2,.3,false,true)`.
Offsets are diagnostic identities, not a byte-pattern search for a model.

Source1680d0 calls source+40 processor7's virtual10 before scale15af40 and
line callback15b260 when its dirty-state condition is satisfied. Type2d0's
runtime table2b39c0 maps virtual10 to081e90. That routine executes:

1. Inclusive candidate angles using float accumulation and the native relative
   `2^-22`/absolute `2^-23` equality tolerance.
   A float step landing slightly beyond the maximum is accepted when it is
   within that tolerance; the final stored angle is the exact configured maximum.
2. 0809c0 extracts **separate X and Y axis amplitudes** between extrema and
   endpoints. It sorts each axis, selects the round-half-away quantile, and
   takes the smaller of the two values. An empty axis has `FLT_MAX`, so the
   other axis wins. Both empty axes reject the input. This is different from
   source219's max-X/Y chord scale. Own printed MIR yields55 here, compared
   with its supplied line height80 in previous diagnostics.
3. Optional265 preprocessing, then012890 rotates the prepared coordinates
   clockwise about the origin: `(cos*x+sin*y, cos*y-sin*x)`. Trigonometry and
   multiplication use double before each coordinate is cast to float.
4. 081240 uses Y bins of width`scale/10`, counts every point, and accumulates
   float `count*float(log(count))`. It does not integrate stroke arc lengths.
   080570 uses a middle-pivot descending quicksort and swaps equal scores.
   Preserving tie order with a stable sort would change the shortlist.
5. Retain at most the resource shortlist count. For more than eight physical
   strokes,080e20 rejects implausible chronological direction/row jumps using
   arc-weighted centers. The qualifying stroke needs at least half-scale
   width or height and at least`.35*scale` height. Row-jump fractions and the
   forward-transition minimum come directly from the profile.
6. 081af0 computes the ratio of sequentially accumulated bbox widths to
   heights, joining the next qualifying bbox whenever its top is no greater
   than the current bottom. It preserves physical order; it is not a generic
   rectangle union. The first-stroke eligibility uses a strict half-scale
   comparison; subsequent ones include equality.
7. Above the coverage threshold, a non-cardinal top angle with at least seven
   strokes wins without the child. Fewer strokes compare zero and that top
   angle. A top angle near±90 retains the available+90 then−90 candidates,
   adding zero for fewer than seven strokes. Below the threshold the shortlist
   is scored, with zero appended for fewer than seven strokes.
8. 081750 invokes primary249 for each selected rotated input and compares
   `int(float(pow(cardinality, exponent))*float(int(unsignedSum/unsignedCount)))`.
   Zero count supplies`INT_MAX` before the signed cast. CVTTSS2SI maps NaN or
   signed32 overflow to`INT_MIN`; the port retains this behavior instead of
   silently clamping. Strictly smaller values replace the winner.

The callback returns exact state fields154 (`uint`cost sum),d0 (`uint`element
count),158 (`ulong`cardinality). Primary249 virtual18 is158bb0, which forwards
to158970 with the intermediate argument zero. That initializes through156200
and156630, resolves the source interface and calls157c60 over ordinary empty
nodes. Orientation's temporary state sets150=1,154=`INT_MAX`,168=1 and
b0=`double(scale)`. Source callback16e850's externally supplied height branch
therefore matters; an arbitrary standalone24a fit does not prove child parity.

## Fresh ordinary configuration evidence

The full orientation stage must be integrated before claiming the following
ordinary source defaults make the complete frame automatic:

* 15a1c0 passes source219+8 to2e3 initialization076ba0. Reader1d4950 zeroes the
  resource object78..22f before1d41e0 populates the config atc0. It never writes
  config+c;1d62d0 and163970 copy it verbatim. The initial quarter-vertical flag
  is zero before property overrides, rather than inferred from a nearby
  serialized byte.
* Fresh unassociated physical dispatch057ae0 reaches the **extended2c1**
  entry29e998→077910. ASM077974 explicitly writes the local physical record's
  half-X byte to zero. Known-record status comes from an independently zero
  input argument; initial group ID is−1. Fresh ordinary physical half-X flags
  are consequently all false. The main2e3 table's+88 is a property method and
  must not be confused with this extended physical callback.
* Constructor076780 sets width multiplier5 and overlap`.75`; initializer
  076ba0 sets internal quantile`.8`. Provider resolution+3c starts zero and
  is assigned by property setter069c10. A source-DPI→provider-property
  forwarding proof is still required; passing source96 without it is a guess.
* Both outer2a9@5325 and nested2a9@5654 contain flags1,0,1 at body+8,+9,+10.
  Thus the nested profile does not prove default2e2 dispatch. 15a1c0 also
  examines prior-state and child+118 conditions. The complete249/source
  dispatch must settle that choice before wiring a guessed2e2 scorer.

## Verification and bounds

Generate independent float32 references and run the headless probe:

```powershell
python tools/LegacyLineOrientation.Probe/reference_controls.py controls.json
dotnet run --project tools/LegacyLineOrientation.Probe -c Release -- controls.json models/ru-RU/ru_RU-ak-cur.lite.res
```

The384 controls exercise4224 projection records,1493 original supplied-child
policy costs, four rotated frames per case, both chronological directions,
quantiles0/.5/.95/1, seven/thirteen angles, shortlist limits, dense/tiny boxes,
unsigned mean conversion, CVTT boundaries, malformed profiles and cancellation.
The scalar oracle uses the native branch form for extrema, not the managed
direction helper. Actual resource mode verifies the complete2d0/v100 profile
and reports its own-ink projection ranking; it deliberately labels this as
**before child265/249**, not actual native orientation accuracy.

Checked library limits are256 physical strokes,4096 total points,129 angles,
one million bins per angle and finite coordinates within±1000000. Nonreachable
angle endpoints are rejected rather than allowing the native unbounded loop.
Copies preserve physical stroke/point order. The source chronology should be
retained by the later whole-source producer; no point identities are invented
by these prepared numerical APIs.
