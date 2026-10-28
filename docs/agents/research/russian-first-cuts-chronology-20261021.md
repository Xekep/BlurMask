# Actual Russian FIRST cut records and source-key chronology

The complete261 recorded inputs now have an independent scalar audit of their actual FIRST cut records and original-source references. This is a diagnostic negative finding: no additional cut-record or source-key mismatch was found. No runtime, native quota, beam, learned weight, score or recognition threshold changes are introduced.

## Actual retained inputs and results

The new probe uses only the existing public owning FIRST API. It records the actual pre-cut `row.Prepared.Strokes`, fitted baseline/height and per-source enclosure vector, then reads the same owner's complete cut record bits, tags, original point identities, markers, source chronology and graph references. It does not import recognized words or a supplied candidate list. Expected labels are never opened.

The separate scalar191950 oracle composes the existing independently implemented vertical-reversal, loop, horizontal, prefilter, baseline, postfilter, mandatory override, pen-lift pruning and final marker-limiter stages in the recovered order. Captured shortest-decimal source coordinates are first restored to binary32; subtracting their Python double parse values directly would create false differences in segment deltas. Every coordinate/delta/length component is compared as its signed32-bit float representation. Active ordinary first-source policy uses version106 and the absent optional category-owner limit; no new limit is inferred.

All261 recorded inputs pass:82,512 complete records,412,560 float components,247,536 tag/physical/original-point fields and5,430 projected markers. Managed and NativeAOT complete captures are byte-identical (14,072,321bytes), SHA256 `6e1f5946be0fed2fb2b8248bf18a22215018d8bef336064d4d6b352044102108`.

## Independent key and endpoint checks

The second oracle reads only the frozen blind raw coordinates. It independently reproduces167210's adjacent-RAW comparison, restoring binary32 subtraction before the native relative double comparison. It verifies all retained original ordinals, including344 removed adjacent points.

For every cut record it composes actual265 fixed-point200 ticks through the actual297 inserted record's original-index map, computes the ceiling metadata index and complementary remainder byte, maps to original raw key ordinals and independently obtains the ending raw duplicate-run interval. All82,512 public source-key getters match, including37,849 fractional positions.

For every actual graph arc, its selected ordinary candidate has one physical contiguous range in this recorded corpus. The oracle projects that range's start and last-start key/remainder, using the original end-of-run key only for a physical tail. All32,530 actual public source references match, including1,818 tail arcs. This checks the endpoint values against actual selected group ranges; it does not derive the graph's membership from labels or advertise complete native graph-path equivalence.

## Limits and next boundary

One supplied input track does not prove physical pen continuity; the recorder
omits release boundaries. See [recorder provenance](russian-recording-coordinate-provenance.md).

These are single continuous input tracks; multi-range sort/merge and real pen-up handling are not covered. Actual smoothing ticks, fitted frame and graph membership are inputs to these checks, not independently generated native topology. Existing source/float/native-stage proofs establish the recovered kernels, but the vendor SDK is never executed. Neither all-record arithmetic agreement nor the preceding feature/NN audit proves general Russian recognition readiness. Remaining errors can involve model/data limits, native bounded candidate selection or still-unverified producers; none should be corrected by tuning quotas against these expected words.

Large captures remain only at `E:/BlurMask-build-artifacts-20261014/review-russian-first-cuts-audit-20261021`; the frozen manifest records their paths and hashes. The repository receives only probe code, two auditors and this document.

```powershell
dotnet run --project tools/LegacyRussianFirstCuts.Probe -c Release -- models BLIND-COORDINATES.json CAPTURE.json
python tools/LegacyRussianFirstCuts.Probe/audit_output.py REPO-DIRECTORY CAPTURE.json
python tools/LegacyRussianFirstCuts.Probe/audit_chronology.py CAPTURE.json BLIND-COORDINATES.json
```

The next unclosed coverage boundary is native seed/graph membership and its caller ordering, rather than an observed numerical defect in these actual cut/chronology streams. Native constants and ordering must remain unchanged unless a concrete mismatched producer is proved.
