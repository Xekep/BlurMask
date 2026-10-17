# Prepared Math2 column matching

The finite prepared kernel restores040270,0403E0 and040670 under the repeated
0409F0 matching driver. Each column first selects its strongest row. Duplicate
rows retain the strongest column, keeping the first on ties. Selected rows and
columns are masked with negative FMAX, then each selected pair is restored to
positive FMAX before the next iteration. Pair order follows the first occurrence
of a row, which can differ from the winning column's order.

This is repeated greedy matching, not an optimal assignment solver. The new
internal API accepts a copied row-major matrix with at most1000 rows,
3000 columns and1000000 values. Caller values must be finite and strictly
inside both FMAX sentinels. A zero dimension returns an empty match. Payload
limits are explicit library safeguards, not native request limits or RSS.
Cancellation returns no accepted partial result. Native allocation failure
prefixes and arbitrary NaNs/sentinel-bearing caller matrices are not claimed.

Managed and fresh NativeAOT produce the same capture SHA256:
`7ea5c9ffb2dd1b37766f4c63b6c17f398ecfc4d6a9266983f6f12e30daee927e`.
The independent Python oracle groups columns instead of using the production
duplicate-suppression loop. It verifies4100 matrices,2999 repeated matches,
56741 tied-column occurrences and11 input/cancellation guards. All matrix
lanes, pair identities/order and iteration counts are checked.

`tools/handwriting/audit_math2_column_matching.py` pins four complete native
regions,33 instructions and the negative-FMAX constant from the hashed PE.
It reads bytes only. The full040CD0 pair averaging, source-unit correction,
actual nonempty C25 producer and owning completed-record integration remain
separate work. This component does not establish formula acceptance or improve
measured handwriting accuracy by itself.
