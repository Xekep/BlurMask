# Shared immutable Math feature profile (2026-10-24)

The feature profile owns all selected calibration/program data once, separately from per-geometry original/normalized points. It retains eight private arrays,2386shorts=4772B:legacy412 pair,selected3F279 pair,4F279 pair,5F223 pair. It has no public arrays,ReadOnlyMemory,span-returning getter or mutable collection. ProgramDescriptor is a copied value with24 scalar fields (60 logical wire bytes, not a CLR layout assertion). Caller changes to that value cannot alter the owner.

`profile.CreateGeometry(strokes)` and `selectedModels.CreateGeometry(strokes)` preserve exact profile identity. Each geometry also retains its unchanged private temporal/spatial feature issuer: exact geometry identity implies its exact immutable attached profile; matching fingerprints alone do not issue features. No model/candidate/grammar authority follows. The profile holds no geometry/model collections or callbacks, so creating subsets does not grow an internal cache or retain source arrays. Pure immutable managed ownership needs no disposal or native lease.

## Authenticated startup and resource lifetime

Profile.PrepareActual preflights exact3685881B length, copies privately in16KiB chunks, hashes that same private snapshot in16KiB chunks, then reads all scalar fields and arrays from that snapshot. No source-array read occurs after its authentication. Finally ZeroMemory clears the profile's private full-resource snapshot on success,cancellation,observer fault or format failure. Internal prepared observer receives only four phase names,never data or a success flag. Public factory always suppliesnull observer.

SelectedModels startup deliberately uses its existing raw-six weight factory AND a separately authenticated profile startup snapshot. The latter is not an unchecked reread of the caller buffer: it independently requires the same full fixed fingerprint before reading fields. If caller data changes between snapshots, the second authentication refuses unless it is still the exact authentic resource. Both immutable results are therefore from the same pinned source. This is two bounded startup authentications, not a single-visit optimization; avoiding their extra startup copy would require a new trusted raw snapshot issuer and is outside this minimal change.

After startup, full subset clients use Models.CreateGeometry/Profile.CreateGeometry. Those methods accept points and token only, perform no resource hash/copy, and decode/clone no calibration. The historical SourceGeometry.CreateActual(resource,strokes) remains a standalone convenience: it preserves preflight/count-single-read before profile preparation, but now owns a new full4772B profile instead of only1648B412 arrays. That path is intentionally more expensive per independent call. It must not be used as the repeated-subset path.

Original00FB90 scalar normalization,stroke boundary/duplicate preservation,finite/short guards,cancellation before publication and output copies are unchanged. Legacy temporal/spatial calibration loops read the same profile pair0 through internal scalar accessors; no arithmetic or default table changes. SourceGeometry replaces its two calibration-array references with one profile reference; no new issuer field is added. Existing temporal/spatial feature types remain disjoint and immutable.

## Exact change inventory and compatibility

NEW Profile +NEW probe3 +this doc. OLD4:SourceGeometry factory/owner reference;TemporalFeatures two calibration accesses;SpatialFeatures one calibration access;SelectedModels profile field/startup+CreateGeometry. Exact preimages are main071b45d. The separately frozen2F648 packet adds disjoint BindSpatial/EvaluateSpatial methods and TemporalNetwork conversion forwarding; no2F source is silently included here. Do not overwrite that addition when integrating this SelectedModels delta.

Native provenance comes from immutable STATIC5ba14b85:0173F0 source wire program,0170B0 pernetwork calibration;actual412/279/279/223 dimensions,headers andall coefficient bytes are independently reconstructed by the new Python oracle. Source creation remains fixed resource only,not a generic13A5 version parser. This owner is a prerequisite; no3F279 feature producer/evaluator is introduced yet.

## Logical allocation/traversal ledger

These figures describe array payload/cardinalities,excluding CLR headers,padding,GC/JIT/crypto implementation internals,exceptions/closures and processRSS. They are not latency or CPU-instruction quotas.

Profile preparation:one3685881B private resource snapshot;4772B final calibration payload;two temporary4-int tables32B. Aggregate array payload3690685B. Two persistent4-reference outer arrays add64 logical reference bytes onx64; profile object/program metadata is separate. Resource snapshot clearing traversesK too:copyK+hashK+clearK+4772 calibration byte-equivalent decode positions=11062415 data-pass cardinality,plus fixed field/metadata operations and array initialization. No hidden blanket byte/work ceiling is asserted.

SelectedModels preparation extends its previous aggregate6138974B by that profile snapshot/data/tables:9829659B aggregate array payload,plus outer reference arrays and metadata. Existing761370B weights are unchanged. Final reusable resource payload is761370+4772=766142B,plus descriptors/references/objects. Pergeometry construction adds no calibration data or resource snapshot. Its already-existing original/normalized points and three outer/reference arrays plus ends have logical payload12*P+28*S bytes onx64 (Ppoints,Sstrokes,includes temporary borrowed row references); headers/scalars/rounding temporaries remain separate. Temporary shapes are bounded S<=64,P<=16384. Each source retains its points/profile normally; no claim that caller-retained geometries are automatically evicted.

## Prepared evidence

Build0warnings0errors. New18guards include four deterministic reached+exact-token cancellation boundaries,mutated caller afterprivate capture,foreign calibration rejection,count readonce,detached original/output,128same-profile geometries,eight-array identity conservation,Models geometry attachment,equal-data foreign source-feature refusal,scalar descriptor copies,pre-cancel andparallel create/inspect. Python derives all24 fields andall2386shorts from original bytes;8 corrupted capture controls refuse. No real network forward occurs.

Existing geometry7cases/14guards/6corruptions is byteexact5d6224a5cffdc85c9d3b4702f5327480c0f04f403c5bc3c8aec69fb5419b654a. Existing spatial13vectors/2652features/4guards/6countcontrols/11corruptions is byteexact1bb20225e20c117a4c92213272bbdf91524d0ac00bf5a675f8d23cfc34f57cf9. These also exercise existing temporal calibration through spatial assembly. No actualNN/vendor/GUI/main changes by author.

Commands:
```
dotnet LegacyMathAkFeatureProfile.Probe.dll REPOSITORY FRESH_CAPTURE
python -I -B tools/LegacyMathAkFeatureProfile.Probe/audit_output.py REPOSITORY CAPTURE FRESH_REPORT
```
Prepared builds use CPU2,disable-build-servers,uniqueE artifacts,UseSharedCompilation=false,nodeReuse=false. Root reviewed source/native/preimages, preserved the disjoint2F binding, and repeated the18guards/8corruptions and both old regression captures in main. Their hashes remain exactly those above. One root-only actual2F forward through the resulting library also matched the independent spatial/features/weights/table replay exactly; this does not claim complete mathematical recognition. The author performed prepared controls only.
