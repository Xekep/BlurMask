# Math2 mean-X ordering

`MathPreprocessor` now uses the literal Ink01ED50 sorter through
`LegacyMath2MeanXOrder.CopySourceOrder`. It computes each mean from the original
points before scaling, normalization and resampling, as before. The changed
behavior concerns ties in larger stroke groups.

The old unconditional stable OrderBy/ThenBy was incorrect for at least21
records. Native01ED50 receives inclusive first/last pointers to16-byte records.
When `(last-first)/16 < 20`, it insertion-sorts and shifts only strictly greater
means. Equal means retain order in those partitions. Otherwise it copies the
middle key, scans from both ends, swaps complete records including equal keys,
then recursively sorts the remaining partitions. It does not use original ID
as a secondary comparison key.

Twenty equal keys stay in input order;21 equal keys reverse. With42 equal keys,
the exact resulting IDs are21..41 followed by0..20. Sorting every group stably
would change the source permutation and could bind positions to the wrong
processed chronology. Original source IDs must survive this permutation.

The internal sorter preserves raw mean bits and complete opaque payloads. Its
prepared API accepts arbitrary float words, including NaNs and infinities, to
check native unordered comparisons. Actual feature input retains the existing
finite coordinate/work limits. Prepared opaque payloads are portable storage
controls, not fabricated native object pointers.

Independent Python replays the native inclusive byte-pointer algorithm on all
16-byte records.4224 controls cover530496 records,495 nonfinite inputs and2550
counterexamples to stable sorting.257 source-order projections and nine actual
feature inputs verify that the production preprocessor uses the new order,
including the20/21 threshold. Four entry-bound guards check limits/cancellation.
Managed and fresh win-x64 NativeAOT captures are byte-identical,23,426,371 bytes,
SHA256 `23e3ba82e3e89b2bfa663fa221e48dfd61042f843a5e63f357196607a4df98ab`.

All eight earlier feature fixtures still have zero numerical error. The actual
structured-output probe retains its complete old capture,20 NN-derived records
and1614 syntax controls, including its failure prefixes. The change does not
assert improved recognition accuracy or close the missing C79/result transport.

`audit_math2_mean_x_order.py REPORT` checks two complete PE regions and38 exact
instructions for the mean producer and sorter. It reads the DLL as data only.
Run the portable probe with one capture path, then its `audit_output.py` with
that path and optionally a same-platform comparison capture.
