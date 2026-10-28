# Actual Math mixed107 raw networks and evaluation boundary

This batch provides a portable **raw resource reference**, plus static native loader/dispatch evidence. It does not run a vendor DLL or implement the application's recognizer. The existing architecture12-only English107 owner is unchanged; Math weights are not coerced into its typed source ownership.

## Six real envelopes

The exact committed `models/math/math-ak.res` has SHA256 `8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1`. Its actual13A5 knowledge contains six unframed FEA0/version107 envelopes, whose magic differs from the90FE resource wrappers:

|Containing configured13A5|Absolute start..end|Stored kind|Input columns|Hidden rows|Output rows|
|---|---|---|---|---|---|
|Inside first-vector13A3's13A4|8702..177650|003F|276|200|257|
|Inside base-tail13A4|1007739..1151090|001F|222|200|257|
|Inside base-tail13A4|1151097..1288050|002F|204|200|257|
|Inside base-tail13A4|1289175..1458130|003F|279|200|257|
|Inside base-tail13A4|1459255..1628210|004F|279|200|257|
|Inside base-tail13A4|1629111..1772466|005F|223|200|257|

The header stores hidden width201 and output width258; the matrix readers operate on one fewer output row. These row counts do not assign output labels or prove the full alphabet/result policy. The stored kinds are preserved without inventing language, feature-family or activation names. The equal003F kind in two distinct owners does not make their weights interchangeable.

`math_legacy_mixed107_reference.py` parses each entire envelope, owns immutable header/matrix values, and **reconstructs every byte** from those fields. The pinned-resource entry checks the whole resource size and hash before selecting the six observed offsets. This inventory identifies their enclosing actual13A5 frames; it is not a complete13A5 slot parser or a live native model instance.

## Actual raw owner is215

Abstract Text214 has descriptor2B9E90 and default service2B9E80, whose first function is **0E0AF0**. The actual Engine TypeAPI V30 is **037F50**; it looks up the registered descriptor and reads its+20 default-service pointer. The exact guarded lookup includes both returns and is separately labelled as an inspected extent, not an invented Ghidra body.

0E0AF0 reads the actual stream header. MagicFEA0 selects concrete **215**; FEA3 selects216. It constructs the chosen object, dispatches that object's V10 reader, and completes the enclosing stream operation.215 derives from214 and its V10 is **0EC890**. That wrapper calls the actual header reader **0EC270** and matrix reader **0EC490**, with allocation/read-only stream behavior determined by version and stream mode.

The actual13A5 raw slot reader **0170B0** requests the registered214 service for its present model pointers. Its corresponding writer is018970. The broader frozen13A5 reader invokes this helper over its actual stored slot structures. No prepared tree or expected label list supplies these raw weights.

## Architecture02 is int16 followed by int8

The current header flag byte is02. Header0EC270 stores it at raw215+21/+20 and selects the two matrix backends through **0EEFD0**, using `(flags >> 4) & 7` for the first and `(flags >> 1) & 7` for the second. The actual result is **mode0, mode1**. It does not mean an unrecognised floating network:

- First matrix: signed16 weights, no row-scale vector.
- Second matrix: signed8 weights followed by a row-scale byte vector, including its stored leading byte.

The current extra middle-width fields are zero and the quantizer field is10. The portable reference supports that explicit subset only. It refuses architecture12, other versions, extra middle layers and other quantizers; native supports additional cases. These are library scope refusals, not claims about native malformed-file behavior.

All six actual matrix-backend tables return16 from their slot0 alignment function **083980**. Version107 aligns each matrix start against the **absolute original stream position**, then rounds its column stride up to a multiple of16. First-matrix reader **0ED5A0** consumes signed16 rows and their serialized stride padding. Second-matrix reader **0EE090** consumes signed8 rows and its scale vector. The read-only accessors0ED9C0/0EE6F0 expose the same bounded extents when that stream mode is selected.

For the first real model,33 bytes of header, one byte of absolute alignment padding, `288 × 200 × 2` first-matrix bytes, `208 × 257` second-matrix bytes and258 scale bytes total **168948**, exactly177650−8702. Every other real envelope ends exactly after the same independently derived layout. Copying a107 envelope elsewhere and assuming origin0 loses this alignment contract.

The reference retains original alignment bytes and padded weight columns. It does not silently zero or drop them. All six observed alignment paddings areFF and all observed padded weight columns are zero. The controls deliberately change these bytes and verify their exact reconstruction. A future evaluator that only accepts meaningful input columns must independently refuse nonzero padded weights or own the actual padded input; the raw parser itself does not pretend native rejects those bytes.

The reference requires the leading second-matrix scale to be1 and all scale bytes nonzero, as in these six resources. That extra guard narrows this library reference to the inspected scale domain; native error parity is not asserted.

## Raw215 is not evaluator212

Raw215 owns its serialized/allocated weight storage. Its actual V40 **0ECF90** creates **212**, derived from211, and calls212's extended constructor **0E6780** with the actual backend pointers, stored dimensions, weight pointers, optional buffers, flags and quantizer. Its V48 **0ED250** instead creates **213**, a separate evaluation route. The source model's preparation code00CAF0 can select these factories according to the actual request mode. A future212-only runtime must prove the relevant mode gate rather than quietly substituting it for213.

0ECF90 passes existing matrix pointers into212 and can create/adapt another output matrix when an optional argument requests that behavior. The full copy/borrow/destructor flag policy is not reconstructed here. The raw215 lifetime and the212 scratch/evaluation lifetime therefore remain distinct. A reusable runtime must retain or detach real weight ownership explicitly and define in-flight disposal behavior; successful wire parsing does not create such an execution lease.

The factory's native failure prefixes can already contain constructed objects or changed buffers. This package does not claim atomic allocation, automatic rollback or retry safety.

## Feature and activation boundaries

Actual Math13A5 V30 **015DB0** calls **00FB90** and **010EA0** before using prepared model evaluation.00FB90 follows the model's same retained source ranges and prepares its geometry/point buffers.010EA0 uses configured slot conditions, actual feature-production helpers and the produced evaluation wrappers. Their complete containing bodies are pinned here, but their numerical/feature dependencies are not ported. The visible helper **0327A0** is an actual producer boundary, not permission to fill a vector from a glyph rectangle or expected symbol.

Evaluator212 V88 **0EBB20** copies caller-supplied signed16 feature entries into the actual evaluation scratch. V90 **0EBB60** scales supplied floats by the evaluator quantizer and truncates/narrows into that scratch. Neither function recognises pen coordinates by itself. The reference performs neither quantization nor feature extraction and never appends an invented input bias.

Actual212 V50 **0EAF90** dispatches through its retained matrix backends and numerical helpers. Its hidden branch places the native `1 << quantizer` value in the first hidden slot before the output matrix. Complete constructor0E6780 establishes its quantizer-derived fields. That is not evidence that any arbitrary caller-supplied activation table, current English source policy or confidence conversion is correct for Math.

The inspected SSSE3 backend's mode0 dot **0EF640** uses signed16×signed16 `pmaddwd`,32-bit `paddd` and horizontal reduction, then advances the weight-row pointer. Mode1 dot **0EF6B0** sign-extends signed8 weights, performs the signed16 dot and multiplies the wrapped result by the next unsigned row scale. Their exact loop/return extents are pinned independently because Ghidra reports no body. Other CPU backends have distinct dot entrypoints; their table bindings are recorded, without a new whole-backend numerical parity claim.

The remaining activation/mask/output-score helpers, actual Math feature order and normalization,213 mode, full13A5 owned slot preparation and final label/candidate policy are explicit prerequisites for recognition. No zero vector, expected label or easier model is substituted for them. The two separate Engine5F networks owned by13F8 rescore **already-produced symbol sequences**, as described in the preceding routing proof; they are not these six initial raw215 models.

## Verification and proposed next scope

Run `python tools/handwriting/audit_math_legacy_mixed107.py output.json --native-repo <repository-with-static-bytes>`.

The audit validates18 complete Ghidra functions/regions, five separately labelled exact disassembled scopes,21 descriptor/service/backend bindings and1211 instruction pins. It composes the frozen AK operator/candidate/normalization/resource proofs. The portable raw reference reconstructs all six real envelopes, rejects294 bounded malformed-wire controls and retains12 altered opaque-padding controls. Header/matrix preflight completes before weight slices are copied. No vendor DLL, SDK, GUI or neural inference is executed.

Only the raw reference is implemented in this batch. `productionRuntimeImplemented`, `wholeEvaluatorRestored` and `applicationReady` remain false. Actual source→feature→activation→candidate integration requires the remaining producer/ownership proof first.

## Proposed next runtime checkpoint: owning raw models only

The proposed scope is **NEW files only**: an independent BCL-only `LegacyMathMixed107RawResource` implementation, a standalone Managed/NativeAOT probe and its Python oracle. No old107 architecture guard, source247/119 owner, evaluator212, application provider or settings seam changes. The raw model objects are independent library projections of the observed215 wire format, not vendor SDK instances or fabricated native handles.

1. `LoadActualResource(ReadOnlySpan<byte>, CancellationToken)` first validates the exact bounded resource length and captures an owned copy in cancellable chunks. The caller cannot later change the bytes being parsed. It preflights **all six** enclosing ranges, headers, absolute alignment, both matrix extents and scale vectors before cloning any matrix payload. It then checks the actual whole-resource fingerprint before publishing any owning result. Bounded header failures can therefore be exercised before fingerprint refusal without weakening provenance.
2. The resource owner exposes six privately constructed, immutable typed raw models. Each records original start/end, stored kind, dimensions, quantizer, the same resource fingerprint, and its verified containing13A5 frame role. The API does not accept caller-supplied provenance, model offsets, candidate labels or a fake212 receipt. Exact offsets remain a fingerprint-bound inventory until the full13A5 slot schema is recovered.
3. Each model owns copied little-endian signed16 first-matrix values, signed8 second-matrix values, complete row-scale bytes and retained alignment/padded-column bytes. It must reconstruct the original envelope from typed fields byte-for-byte. It must not retain a borrowed caller span or silently use the old int8/int8 model type. The temporary full-resource capture can be released after construction; model weight storage remains owned by the new resource.
4. Whole-source/model memory and width limits are fixed before copying. All scalar products, byte extents and origin arithmetic use checked calculations. Cancellation is checked during source capture/hash, every envelope preflight and bounded matrix decoding chunks. No partially constructed resource escapes on cancellation, malformed input or allocation failure.
5. Public snapshot/export operations return detached owned values. A private owner gate serializes those copies against disposal. Closing the root invalidates future live model operations; an already-returned historical snapshot remains inspectable. Concurrent readers cannot see partly cleared weights. There is no native-reference/destructor-equivalence claim or one-use claim: immutable raw weights can be inspected repeatedly.
6. A NEW diagnostic-only phase observer may enable deterministic cancellation/throw tests without exposing captured byte arrays, weights or provenance setters. It must be separate from the production load signature and cannot inject a model, dimensions or a late-bound source. Original caller-buffer mutations during capture must lead to the same validation/fingerprint rules, not a substituted resource.
7. The Python oracle independently uses the portable reference to verify every model's raw envelope hash, every decoded signed16/signed8 weight value, scale byte, dimensions, absolute origins, alignment and reconstruction. A complete-value stream digest must cover all matrices in all six models; validating a few sampled weights is insufficient.
8. Managed and NativeAOT captures must agree with that oracle for all six actual models and for structural malformed controls, wrong origins, foreign-resource bytes, each deterministic cancellation prefix, input/snapshot mutation isolation, parallel copies/disposal and GC/lifetime controls. Existing raw107/actual Math probes remain unchanged; run appropriate old raw/resource regressions once after the new isolated build.

No further native dependency blocks this **fingerprint-bound raw wire owner**: the current-version field order, alignment, matrix domains, scales and complete envelope consumption are already proved. The unresolved full13A5 slot parser prevents a generic resource-version promise, so the new loader must remain pinned to this exact resource. Feature extraction, input quantization, bias/activation construction,212/213 execution, score conversion, neural inference, recognition dispatch and candidate acceptance are **excluded** from this checkpoint. They require their own evidence and approval; a raw loader does not improve recognition accuracy by itself.
