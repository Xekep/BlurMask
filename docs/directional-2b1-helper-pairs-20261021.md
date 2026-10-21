# Actual2B1 helper seam, V2 blocker and bounded V3 proposal

Update: the [shared BC280 helper and English N/D0 correction](directional-2b1-landmarks-runtime-20261021.md)
are now integrated. Russian V3/full evaluation remain pending. The static proof
and proposed boundaries below are preserved as the preimplementation checkpoint.

This is a STATIC correction proof. No vendor binary is loaded or executed, and
no production code is changed here. V2 captures, source and metrics remain
immutable historical evidence, not a complete native-feature parity claim.

## Concrete landmark defect

The selected2AF mode2 invokes2B1.BEDF0, then BC280. Generic2B0 uses B6D20.
LegacyDirectionalLandmarks and its old independent reference deliberately
reproduce B6D20's first point2 Y comparison against point0. Actual BC280 instead
loads point1Y before advancing to point2. Thus borrowing that helper silently
transferred the wrong native variant, despite correct local/whole extent flags.

Raw assembly pins close the source of the difference:
BC42E sets RBX to point1; BC4D3 loads its X; BC4DA loads its Y; BC4DE advances
RBX to point2; BC4F0 loads point2Y; BC4F7 compares with saved point1Y.
B6F70 loads only point1X before B6F77 advances to point2, and B6F9A still compares
Y with the earlier saved value. The full bodies and10 exact instructions are
pinned. No decompiler variable-name inference is required for this difference.

A tiny static reference control [[0,0],[1,2],[2,1]] yields actual BC280 flags
[81,8,146]. Generic flags differ.125 independent three-point Y orders/equalities
exercise the new literal reference. These are geometry controls, not language
labels or a trained model.

The first real-input flag witness is heldout-0000/group37: two flag locations
change, but both selected feature vectors and NN outputs happen to remain
unchanged because membership/counts hide this shift. The distinct positive
impact witness heldout-0037/group114 changes two flags, zero temporal/two spatial
geometry shorts, and zero temporal/59 spatial actual-model probability entries.
Original V2 vectors and original probability arrays were independently replayed
first; the only counterfactual change is literal BC280 Y initialization. This
is a native-required arithmetic correction, NOT evidence of better words.
Neither witness reads a target. E-only artifacts and hashes are in the manifest.

## Full configured helper review and explicit bounds

The static export pins19 COMPLETE Ghidra bodies/19 regions, including all seven
old/new pairs below. Extents are actual Ghidra bodies, not the first unwind row.
Body hashes are exact-byte evidence; they are NOT a formal equivalence checker.
The following is a manual supported-subset semantic review of the configured1111
producer, in addition to the separately frozen table/flag/default-calibration
and SAT-CALIB-SAT proofs.

|Selected2B1|Earlier2B0|Result for admitted facade|
|---|---|---|
|BC280 landmarks|B6D20|Y point1 initialization differs: NEW helper required. Endpoint and axis masks otherwise same.|
|BB720 intersections|B6270|Same fixed-point B5FA0 predicate and ordered four-bin/nearest-point/first-write/self-loop algorithm; point/class storage widens byte to ushort.|
|BC650 global17|B7100|Body differs only in intersection byte versus short reads; values remain0,1,2,3. Same17 integer formulas/covariance loop.|
|BC9E0 temporal membership|B7440|Same rounded strict cumulative boundaries, once-per-point advance, singleton and trailing windows; counts/indices widen to ushort.|
|BD280 spatial membership|B7CE0|Same divided-then-repeated uniform boundaries, unmatched-last-bin policy, point-order fill; cell counts/indices widen to ushort.|
|BD520 temporal local16|B7F60|Same means, extents, endpoints, reversals, class counts, covariance/rounding; reads ushort membership/class data and carries separate no-unit flag.|
|BD870 spatial local16|B82C0|Same four spatial prefix formulas and remaining endpoint/reversal/intersection/covariance channels; same widening and no-unit flag.|

BC8A0 separately supplies all nine baseline formulas, inclusive within-band
classification and integer division. Existing baseline helper matches the
positive-height branch; nonpositive-height native storage behavior is excluded
by the owning source's positive guide and explicit numerical API guard.
BE270/BC120/BCC20 allocate short membership/class storage. B4D10's in/out pair
at receiver6C/70 is target-count then resulting source stride;70 is NOT a new
zero-default policy. BB720 receives that stride, while its stored scale comes
from the actual profile/BC120. Existing resampling and source baseline mapping
retain their independently pinned shared arithmetic.

Native2B1 supports wider ushort indices. Current borrowed numerical consumers
explicitly REFUSE more than255 prepared samples (intersections, baseline,
spatial and temporal memberships). Consequently V3 can retain a narrower
1..255 prepared-sample facade, with an explicit whole-sample cap after common
resampling and BEFORE those calls/byte casts. This is an honest library subset,
not a claim that native2B1 is byte-indexed. In the supported domain indices and
counts0..255, source-stroke ordinals0..14 and class values0..3 fit their managed
representations without truncation. No global legacy byte API is widened.

Additional guards retain positive signed-short dimensions, nonnegative prepared
coordinates, checked source stride/grid arithmetic, monotonic bounded cumulative
length, finite source normalization and positive baseline height. Native overflow
or allocator-failure domains are not imitated as false success. The configured
upper spatial family remains inactive, so its quantile path is not borrowed.
The table/instruction scan plus this manual helper review is not transitive
alias analysis or a full-program proof for every native feature profile.

## Minimal next runtime proposal, not implemented

Add NEW language-neutral LegacyDirectional2B1Landmarks.Extract returning the
existing owned landmark result. Preserve the historical generic B6D20 helper
and its tests unchanged. Use the new pass only in actual2B1 N and RU producers;
keep the current bounded255 prepared-point facade explicit before consumers.
The NEW literal reference_landmarks.py is shared by their independent producer
references; do not silently replace the generic reference. Both corrected
pipelines must replay full actual geometry/NN/cache corpora, early-Y reversal,
plateau/singleton/multi-stroke/cancel/budget controls and Managed/AOT parity.
No model, beam, confidence or known-word tuning is authorized by this proof.
Final V3 source/captures/oracles must freeze before any fresh posthoc evaluation;
previous V1/V2 labels are already known, so later tests are fixed-dataset replays.

The root separately owns fresh field/CTC frame admission. A profile using2B1
cannot borrow generic landmarks merely because its final vector has same width.
No UI integration, ready Russian recognition or full native engine completion
is claimed here.
