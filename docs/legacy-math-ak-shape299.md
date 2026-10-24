# Selected Math299 geometry and immutable13A6 prototypes

This adds a source-bound numerical primitive, not a classifier result, grammar match, candidate tree or application switch. It uses the actual selected13A6/299 resource and the same privately owned normalized short strokes produced by existing `LegacyMathAkSourceGeometry`. No neural model is executed by this packet.

`LegacyMathAkFeatureProfile` now decodes the actual13A6 prototype owner from its **existing private authenticated snapshot**, before the snapshot is cleared. `SelectedModels.FeatureProfile` and every geometry created from that shared profile retain the same immutable prototype owner. Preparing another subset performs no resource hash, prototype clone, model catalog clone or calibration clone. The existing standalone `SourceGeometry.CreateActual(resource, ...)` convenience still constructs a new profile each time and therefore pays the new prototype decoding/allocation cost. Every profile, including old convenience callers, now has one additional reference and the prototype payload; there is no claim that old allocation cost is unchanged.

## Exact selected source and ownership

Actual13A6 is1772528..1789539; its nested299 is1772544..1772595 in the pinned3685881B `math-ak.res`. Reader03DB00 reads30 rows, window0, iterations4, transform flag1, metric127, weights1.5/3/3/1/1/1/1. Its V18 gives210shorts. Reader00C160 then reads259 ordered label lists,39 prototypes of210shorts,66 target IDs and factors0.75 where targets exist. Empty-target prototypes remain stored alternatives. The owner reads the complete exact source, preserves all values/ordered targets, exposes defensive copies only and retains no raw resource bytes or mutable memories.

The new fixed parser is internal to the authenticated profile path. It is not an arbitrary-version13A6 decoder and cannot grant authority to caller-prepared labels, arrays or prototype rows. Header/kind/version/settings, all count ceilings and complete endpoint1789539 are checked. Target IDs are bounded to AL259/FFFF. Nonzero-window299 is unsupported.

`PrepareShapeFeatures()` accesses the geometry partial class's private normalized short arrays directly. No external arrays or copied diagnostic snapshots are consumed as authority. Its result retains that exact geometry, its existing private issuer and the same profile prototype owner. `OwnsShapeFeatures` refuses equal-data foreign geometry/profile owners. Public diagnostic prototype distances do not classify or admit a label and accept no supplied feature arrays.

## Literal feature path

Native caller15EB0 first invokes00FB90, then passes the resulting same source+1F0 short group to13A6.V10. The new path uses that existing normalized projection, not original Vector2 coordinates or temporal/spatial neural feature arrays.

03BCE0 accumulates F32 physical segment lengths, divides by29 and resamples all physical strokes with one carried remainder. It preserves the literal pending-group behavior, zero-length handling, F32 interpolation, break-before-remainder-subtract and final endpoint condition `remainder/step >0.5`. The final endpoint append belongs to the existing emitted group, including when the final original stroke has emitted no separate sample. Zero total arc repeats the first input point30 times. No pen lifts are inferred.

03C2E0 finds the sampled bounding box, square-centers and scales coordinate channels to8192, writes start/continuation markers, derives centered neighboring direction, then direction dot/cross per emitted physical group. It repeats the final complete row if needed, then multiplies all seven channels by the actual stored F32 weights using the native signed +/-0.5/CVTT/short conversion. Native interpolation instructions03C10F..03C14E are SUBSS/MULSS/ADDSS; the reference does not silently promote them to double or use fused multiply-add.

Portable bounds are the existing geometry64strokes/16384points plus a local30-resampled-row cap. The cap refuses **before** writing a31st sample or fixed210-short feature data. Native3C2E0 checks30 only for the first coordinate pass and its later loops follow sampled count; this library does not reproduce any hypothetical native out-of-buffer execution. All13 prepared cases produce30 rows. The cap's defensive refusal is a declared supported-domain boundary, not a claim of native malformed-input/failure-prefix equivalence or a proof that every geometry can reach it. No counter injection/prepared issuer is added just to manufacture that case.

The normalized coordinates and private sampled square keep coordinate channels0..8192 before weights, direction/dot/cross bounded in signed-short range and intermediate integer products within I32. Producer conversion also explicitly refuses any nonfinite/out-of-short channel rather than wrapping an unproved domain. Cancellation is checked before work, at bounded segment/sample loops, through weighting, and immediately before publication.

## Window0 distance and remaining classifier work

Actual03D110 checks window0 at03D1DE and executes the direct corresponding-row path through03D364. It sums210 absolute signed-short differences and returns F32(sum)/F32(30<<13). Maximum sum13,762,350 is below2^24. DTW traversal and transform-fit callbacks are not used by this selected distance branch. The seven weights are already applied by the feature producer and must not be applied again during distance calculation.

This packet exposes source-owned features and diagnostic distances only. It does not implement13A6 cost adjustment, source masks/eligibility,262-slot unification, native unstable sorting/pruning, stroke-count scaling or top20 materialization. Those must compose against the same private selected models/profile/geometry before a classifier output exists. AL259, neural257 and scratch262 remain distinct.

## Bounds and verification

Once per profile, new array payload is8190shorts(16380B)+66ushort targets(132B)+39F32 factors(156B)+260I32 label offsets(1040B)+40I32 target offsets(160B)+7F32 weights(28B)=17,896B. Six array references and the profile's new owner reference are additional object fields; CLR headers/alignment/GC/RSS are not claimed by this payload figure. Zero-window code need not reproduce the native unused7200B DTW buffers.

Per feature call: fixed30 Vector2 sample scratch240B,30I32 ends120B and210short output420B. Only the420B output is retained by the typed result; the geometry/profile/prototype references are retained identities, not clones. The result adds four references and two I32 counts. Public diagnostic copy methods intentionally allocate their bounded requested copies. A distance request allocates at most10F32 values40B, uses at most2100 component comparisons and returns a fresh array.

Successful producer work has two passes over original edges (at most2*(16384-1) length evaluations), one bounded30-sample publication stream, at most30 groups, bounding/coordinate/direction/dot passes over at most30 rows, at most29 final row copies and210 weighted conversions. Each inner interpolation appends a sample before proceeding, so the30-row guard bounds it before unbounded growth. These are protocol/cardinality statements, not CPU instruction, wall-time or RSS guarantees.

Prepared verification covers13 original-point geometries,2730 independently derived shorts, all8190 prototype shorts/66 targets and507 exactF32 distances,31 guards and17 capture corruptions. Cases include point/zero arc, horizontal/vertical, reversed chronology, repeated points, multiple physical strokes and an endpoint on a short last stroke. Existing Geometry(7 cases), FeatureProfile(all fields/cancellations/128 shared geometries), Temporal222(1554 values), Spatial204(2652 values) and sharedDirectionalBundle(5467 values) probes and independent auditors pass unchanged captures. No C# actual-network entry is present in this probe.

Native supplement pins six **complete original Ghidra function bodies**, eight data bindings and four complete scalar instruction spans. The earlier window0 static packet's03D1DE length0187 ended inside the final unconditional jump; this successor pins the complete span through03D368(length018B). Earlier bytes/evidence are preserved; the arithmetic conclusion is unchanged. The first full-body export attempt stopped on an absent leaf00C800 and is preserved separately; the successful successor exports the six real complete bodies. No incomplete runtime-function prefix is called the whole03C2E0 function.

Commands (fresh output paths):

```
dotnet build tools/LegacyMathAkShape.Probe/LegacyMathAkShape.Probe.csproj -c Release --disable-build-servers --artifacts-path E:/UNIQUE -p:UseSharedCompilation=false -nodeReuse:false -m:1
dotnet E:/UNIQUE/bin/LegacyMathAkShape.Probe/release/LegacyMathAkShape.Probe.dll REPOSITORY CAPTURE.json
python -I -B tools/LegacyMathAkShape.Probe/audit_output.py REPOSITORY CAPTURE.json
python -I -B tools/LegacyMathAkShape.Probe/audit_native.py SDK_REPOSITORY REPORT.json
```

The native command reads original vendor files as bytes only. It neither loads a DLL nor executes SDK code. This primitive is not handwriting-quality evidence or recognition readiness.

Main integration was rebuilt separately with zero warnings/errors. The root replay independently passed all13 cases/2730 channels/507 distances/31 guards/17 corruptions; five rebuilt earlier prepared probes retained byte-identical captures. CI runs the geometry/prototype audit without neural evaluation or vendor binaries. This change follows the RC13 tag and is not included in RC13.
