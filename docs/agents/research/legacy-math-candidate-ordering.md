# Prepared native Math2 candidate ordering

`InkRecognition.Math.LegacyPreparedMathCandidateOrdering.Sort` restores the
bounded numerical036810 sort called by the native EEE beam driver03A260.
It copies complete168-byte candidate records and compares their first float32
cost in ascending order. All other bytes remain opaque caller storage; no
native pointer owner or tensor state is fabricated.

The native last pointer is inclusive. Byte distance below0xD20 means at most
20 records and uses strict stable insertion. Larger ranges use a middle-record
pivot and Hoare partitioning, including native tie permutations. Equal-cost
20-record input stays in place;21 records reverse;42 records rotate by21.
Generic stable sorting would change those native results. COMISS/JBE stops
scanning for equal and unordered values, so NaNs and signed zeros are handled
without imposing a framework comparison order or rewriting NaN payload bytes.

An explicit stack preserves native left-first partitions while avoiding process
stack exhaustion. Portable limits are65,536 complete records and64 million
work steps. Input is privately copied, cancellation is checked throughout, and
a failed/canceled attempt exposes no completed sorted result. This does not
claim native allocation-failure or in-place partial-failure behavior.

The independent Python transcription verifies181 controls over12,787,320 full
record bytes, including nonfinite costs, manual20/21/42 tie goldens,4,752 rows
and a65,536-record limit case. Managed and Windows Native AOT captures agree,
SHA256 `64004db2ac41a19c61de5b12c1356f749058db246a07a8363942df34b7671a6f`.
Static PE checks pin the whole native body,23 instructions and exact pivot
selection for every portable count.

This is a **prepared kernel**, not a joined beam decoder. Candidate production,
tensor state, constraints, scoring/pruning ownership and the rest of03A260
remain separate. `StrokeMathRecognizer` still uses its existing greedy path;
no improved formula recognition, Russian quality or full engine completion is
claimed. See [the actual decoder route](legacy-math2-beam-route.md).

The probe takes only a capture path. Run its `audit_output.py capture.json`
for independent bytes, or `audit_math_candidate_ordering.py report.json` for
read-only private-PE evidence. No SDK DLL is loaded or executed.
