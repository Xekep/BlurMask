# Prepared fresh segmentation score

The finite fresh/no-deletion041CF0 numeric stage now consumes the actual
attention and prepared fresh segment assignment. Its base score comes from
the same event's normalized raw48 cost; neither labels nor a replacement
confidence value are supplied by the caller. The segmentation coefficient
remains explicitly prepared, since real request-policy ownership is separate.

For each assigned segment, the native arithmetic clamps its selected
probability to F32 `0x2EDBE6FF` (approximately1e-10), subtracts its logarithm
from a F32 accumulator, then divides by the number of nonabsent assignments.
All-absent prepared input has zero penalty. It multiplies by the coefficient
in F32 and separately adds the base score. The portable implementation uses
`MathF.Log`; native1A8C30 CPU-dispatch equivalence is not asserted.

The existing fresh-segment probe adds a separate score sidecar, preserving all
old captures.11 actual events and4096 numeric controls cover zero and tiny
probabilities, absent assignments, coefficients including0 and negative values,
separate rounding and13 refusal/cancellation guards. An independent oracle
uses75-digit decimal logarithms with an explicit portable tolerance, checks
the base against actual EOS facts, and requires exact separate F32 multiply
and add using the captured portable penalty.

The helper returns numerical facts. It does not mutate completed records,
delete output units, remap a correction mask, select the real request branch,
write native diagnostic fields or enable final recognition acceptance.
Those remaining joins must precede treating this as a complete EF4 score.
