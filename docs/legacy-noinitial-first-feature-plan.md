# Actual no-initial first features: ownership and next join

This read-only plan concerns the actual selected English cursive-lite249→247/110→21D/119 source. It does not alter the Russian APIs, frozen119/107 readers/evaluator, application or defaults. `audit_noinitial_first_feature_plan.py` composes the prior raw119 inventory with ten additional pinned Text byte ranges and three dispatch bindings. Its JSON retains the actual ordered processor payloads and the profile-derived vector widths.

## Source and preprocessing order

`1680D0` walks the resource-configured processor list in order and dispatches each processor'sV18. The actual English first list is:

| Processor | Raw envelope | Actual parameters |
| --- | --- | --- |
| 24A/102 | 16842..16878 | native first0, second100, endpoints1, slope±0.5773503, sparse10, dense50, final byte0 |
| 24C/100 | 16878..16902 | serialized angle bounds approximately±0.5235988 |
| 265/102 | 16902..16950 | minimum.05, maximum.075, extra0, remaining direction/corner/spline settings retained verbatim |

These differ materially from Russian first24A/265: the Russian24A dense threshold is10 and its final byte is1, while its265 extra is.3. English source processing must not reuse a Russian frame or profile. Reader176FB0 loads24C's stored float values directly. Setter176980 performs the external degree→radian conversion; it is not the wire decoder.

Fresh direct ordinary ingress uses163970→167210, with no events, prior source, semantic kind6 or explicit context.163970 clears ingress9C. In167210, the kind0/no9C path writesFFFFFFFF to each physical1C record's+4. This is distinct from the88-byte input descriptor's+10 guide index. A future owned factory must preserve that narrow lineage instead of accepting a caller-supplied “unassigned” Boolean.

24A.V18 resolves to16E850 at table2DF6B8+18. It estimates a guide with16E0C0, assigns **descriptor+10**, calls source219.V88=`169750` through2DF0A0+88, and deskews the XY view with16E570. The source callback updates the guide's corrected fields; deskew updates the actual XY points and guide0/4. Neither operation receives/writes the physical1C+4 vector. Thus the fresh physical−1 assignment survives24A even though the descriptor acquires a guide. GuideC/10 was copied before deskew and must not be casually equated to guide0/4.

##24C unchanged branch

24C.V18 is176A10 at2DFE20+18. It reads physical count at source44, the shared XY/prefix view at28/38, and physical1C records at58. It only counts records whose+4 is−1. On the fresh scope above, those are the actual post24A points of every physical stroke.

For each eligible physical stroke, direction starts0. A strict Y decrease starts/continues direction+1; a strict increase starts/continues−1. Count increments when the strict direction changes or starts; equal Y preserves the previous direction. After a multi-point stroke, a final nonzero direction adds one more count. A one-point stroke or entirely horizontal stroke adds zero. The counter accumulates across strokes, while direction restarts for each stroke. The native comparison is **signed count<20**, not point count<20, number of strokes<20, or an approximate-Y test.

Below20 it returns success before any24C point mutation or trigonometry. At20 and above it scans eligible descriptor geometry, applies the configured angle selection and performs real geometric transformation. The trigonometric branch is not yet ported and must remain an explicit temporary refusal in the first bounded owner. It is a required subsequent implementation, not a permanent short-glyph restriction or evidence that24C is generally inert.

265 then consumes the same updated point owner/guide and original-source direction metrics, producing its true smoothed XY and factor200 point chronology. The current shared kernels for24A estimation/deskew and265 smoothing/keys are reusable with the **English** profiles. Calling the old Russian row factory is not appropriate because its resource guard and processor list describe a different owner.

##219/201 feature layout

The actual English-lite acoustic feature profile stores14=1011hex,18=3Fhex,1C=30 samples,1E=16 bins,20=8,22=10,28=0 and seven zero float settings. Native00AC70 constructs that feature object.00AEF0 invokes007070/007470 geometry preparation,007B20 resampling,008290 trajectory channels,008770 statistics and the raster services.00B270 confirms210 trajectory channels plus seven statistics. The mask's raster branch produces twelve16-bin banks,192 channels, plus those seven statistics.

| Vector | Positions | Meaning |
| --- | --- | --- |
| trajectory | 0..5 | directional summary |
| trajectory | 6..35 /36..65 /66..95 | pen states / X / Y over30 samples |
| trajectory | 96..124 /125..153 | adjacent normalized DX / DY |
| trajectory | 154..181 /182..209 | adjacent tangent dot / cross terms |
| trajectory | 210..216 | seven shape statistics, including the constant1024 channel |
| trajectory | 217 /218 | actual start/end signs |
| raster | 0..6 | the same seven shape statistics |
| raster | 7..198 | twelve16-bin projection/contour banks |
| raster | 199 /200 | actual start/end signs |

The shared integer feature kernels use exponent10 (unit1024), the native128-square normalization,30 arc-length samples and raster Bresenham/projection rules. Source FF/FF float→short normalization to target100 precedes this integer feature normalization; these are separate operations. Native pen-up reduction also belongs before feature extraction. A guide cannot be omitted merely because a later square normalization removes translation/scale.

`1B0760` derives runtimeA0/E0 from the initial context object at258 and raw first flagsB8/E8, and copies runtimeA1/E1 from raw second flagsB9/E9. English first has no initial object and first=false, second=true. `1B75B0` copies geometry into the network input, conditionally appends context only underA0/E0, and writes its two final boundary channels underA1/E1. Consequently219=217+0+2 and201=199+0+2. There are no borrowed28 Russian channels and no inserted zero context vector.

`1B7F70` supplies the actual boundary signs. Its existing scoped policy resolves the first range's start point and last range's end point against their physical point counts, after native chronology ordering, with ordinary constructor suppression bytes initially zero. A future owned kernel must derive these signs from the same selected range/count owner. A caller-prepared sign pair is not evidence of a source-bound producer.

The first English297/108 feature/cut scalar prefix (49 bytes after its envelope header) is byte-identical to Russian first297/106. This does not identify their alphabets, category tables, child ownership or whole samplers: English has217 AL labels and a separate category201 payload plus108-tail flags. The typed sampler reader already retains those differences. No version constant may be widened to obtain a fake English sampler.

## Activation/runtime construction boundary

Native21D construction `1B0760→0ECF90→service212 initializer0E6780` binds the real networks, input storage, hidden bias, quantizer and feature objects. The hidden activation249DE0 and EXP table249DB0 read module-global tables; the prepared evaluator's48-entry table is independently pinned, and its352-entry EXP reconstruction is checked against all native entries. A source-bound portable constructor for these activation/runtime objects is still absent. Supplying the Russian-named profile JSON's globally identical hidden table is an explicit prepared table boundary, not an English source/model substitution.

Mixed special-class selection is separate from vector production:1B86F0 reads same-source AL+E, actual180, and selects the mixed sigmoid/EXP path. The prepared107 evaluator restores that numerical branch, but it does not close this source/feature construction chain.

## Smallest next implementation

Add a **new** owned English-lite first preprocessing type retaining the selected119 model, same raw ingress, actual24A/24C/265 profiles, fitted guide, postdeskew points, exact24C run count, smoothed points, metrics and factor200 chronology. It constructs the fresh kind0/no-event assignment internally; it accepts no caller success, assignment or geometry flags. Initially it succeeds only on the proven24C count<20 branch and explicitly refuses the positive branch. Tests must include counter19/20 boundaries, horizontal/equal-Y and multi-physical cases, actual English profile differences, cancellation, ownership/copy isolation and oldRU parity.

Next, add a separately tested no-initial217/199+two-sign feature kernel using the exact shared integer feature math, followed by an owned selected-range/chronology/normalization/pen-up/boundary adapter. The current old normalizer's range-count and pen-up limits are still caller-confirmed, so their actual producer and the108 candidate/range owner must be established before exposing a source classifier. The source classifier remains absent until those owners are coherent. After the short-frame join, the actual positive24C geometry branch is the required next port. None of these stages changes application defaults or claims English/Russian recognition readiness.
