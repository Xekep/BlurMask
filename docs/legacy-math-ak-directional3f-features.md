# Source-owned selected3F279 directional temporal inputs

This port adds the configured A111 temporal input producer proved by STATIC5ba14b85. It is a feature primitive, not a model evaluator, candidate classifier,13A3 cache or grammar-tree issuer. Raw first3F276 at8702 is not accepted or substituted; returnedModelStart is selected1289175.

`source.PrepareUnguidedSingleStrokeDirectionalTemporal()` starts from the geometry's private00FB90-normalized points. It uses actual2B1 coordinate preparation/resampling100,false,BC280 landmarks,checked byte-domain intersection/global/local helpers,16 arc windows and supplied selected3F calibration family1 from the immutable shared profile. There is no caller-feature/membership/calibration ingress and no resource hash or calibration array clone in this per-source path.

The layout is bias1+global17+16x16local=274, first short saturation,calibration1..273 with the source279 min/max pair,second saturation,then first3 absent-guide baseline outputs(-1024 each),then two whole-source boundaries+1024. The neutral existing283-column assembler computes six additional unused baseline channels before the fixed277-prefix copy; these are library glue work, not claimed native callback stores. Final279 values are the exact native configured channels. No English default274 table is used: the7 independently derived vectors differ from that alternative at272/273 positions each.

Private typed features retain exact source,existing privateissuer andexact immutable profile identity. `OwnsDirectionalTemporalFeatures` checks allthree. Temporal222/spatial204/directional279 types are disjoint even though they reuse the privateissuer object; callers cannot construct a matching issuer through supported APIs. Public outputs are cloned arrays; no prepared-sample/flags/window/mutable helper object escapes. Intermediate legacy helper spans/memories remain private locals, not caller-visible retained aliases.

## Bounds and lifetime

Admission requires one whole physical stroke,2..4096 original points and nonzero normalized extent. Existing resampler arithmetic domains and1..255 sample ceiling apply; the explicit byte-domain guard is BEFORE landmarks/intersections/window consumers. Unsupported degenerate,multistroke,point-count/dynamic-arithmetic branches refuse. This does not assert every geometry below4096 is supported. No automatic guide,contextual boundary/cut policy or source eligibility is inferred.

The profile's new INTERNAL composer is fixed tofamily1 and passes read-only spans of its private prefix arrays straight into the existing neutral assembler; no calibration buffer is copied or exposed publicly.9zero baseline ints use a cleared36B stack buffer. Only279 outputshorts (558B) and scalar/ref metadata remain in the result; source/profile ownership is retained deliberately. SourceGeometry itself and the immutable resource/profile creation paths are unchanged by this packet.

Logical array-payload inventory (not CLR headers/RSS/latency or a whole controller admission): P<=4096 originalpoints,N<=255 preparedpoints,one stroke. Input LegacyInkStroke and coordinate preparation allocate16P payload bytes in aggregate (inputcopy4P,coordinate temporary+owned8P,cumulative4P). Prepared sample copies/cumulative allocate12N. Landmarks addN+4;intersection flags/segment storage addN+24N plus<=10000 int grid heads(40000B). Membership ranges total<=N bytes plus16reference slots128B.16 local calls each flatten ALLN points,adding64N bytes even for empty windows; all16 results add1024B and the private local reference array128B. Global17 adds68B;assembly reference snapshot128B,temporary283shorts566B,final279shorts558B. Three one-stroke reference arrays add24B. Existing List sample accumulation also has runtime backing-array growth; current .NET doubling capacities4..256 aggregate2032B (it refuses at sample256). Under that concrete capacity schedule the aggregate logical payload envelope is16P+103N+44660<=136461B. This is a current helper/runtime accounting explanation, not a promised portable allocator quota; object headers/collection wrappers/exception paths/stacklocals are separate.

Traversal dimensions: P sourcepasses;N resampling/landmark/global points;16 window validations each traverseN plus memberships whose total isN. Intersection linked-list checks are bounded by4*(N-1)^2;each successful same-stroke loop can scan/mark<=2N flags. This conservative cubic worstcase is bounded but not a latency claim. Existing integer overflow/admission guards stay in force. No hidden zero-allocation or per-subsetNN-cache performance claim is made.

## Source and tests

NEW DirectionalFeatures +NEW probe3/reference +doc; OLD1 onlyfixed-family composer in FeatureProfile. Base05760c1 plus the exact root-imported profile overlay is pinned separately. Profile preimage is the exact root-imported9c71d689 source encoding (rawSHA recorded); normalized code equals immutable original. Canonical difference adds the helper and normalizes CRLF to LF.2F SharedModels BindSpatial additions remain present and untouched. All earlier freezes stay immutable.

Build0/0;7vectors1953channels equal BOTH the fresh independent raw-geometry reference and every previously frozen STATIC5ba value.9guards cover same-profileforeigngeometry,equal-dataforeignprofile,copiedvalues,cancellation,point/degenerate/multistroke/over4096 refusal and caller-resource clearing after profile creation.8 numeric/protocol corruptions refuse. Existing7geometry output5d6224a5... and13spatial output1bb20225... remain byteexact. No actualmodel dot,neural forward,vendor execution or main mutation by author.

CLI:
```
dotnet LegacyMathAkDirectional.Probe.dll REPOSITORY FRESH_CAPTURE
python -I -B tools/LegacyMathAkDirectional.Probe/audit_output.py REPOSITORY CAPTURE FRESH_REPORT
```
The model-weight owner is reused only by future evaluated views; this feature stage does not load it. Actual3F network execution requires separate root review.
