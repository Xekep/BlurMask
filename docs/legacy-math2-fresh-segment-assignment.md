# Prepared fresh segment assignment from actual event attention

`LegacyPreparedFreshMathSegments` restores the finite-probability fresh041020
assignment and041A30 run compression as an explicit internal prepared branch.
Its entry consumes an actual event-bound attention snapshot. It derives
observable primary positions from that event's fingerprinted alphabet, applies
the already checked sigmoid, filters those rows and assigns each source
segment to the original primary position with greatest probability. Equal
finite values keep the first observable position. The resulting runs retain
original primary indices and exclusive source-segment ends.

The source is not a caller-provided transcription, probability matrix or symbol
map. Returned arrays are copies. The separately named prepared numeric helpers
are limited to finite activated probabilities in0..1, ordered distinct prefix
indices and bounded complete input. This scope avoids claiming native
nonfinite or invalid-row behavior. The actual snapshot entry has the same
uninterrupted cold-history restriction as its source attention.

11 actual EOS attention snapshots feed the prepared branch.512 additional
numeric controls cover ties, repeated assignments and multiple rows/segments.
An independent oracle uses column argmax and grouped run lengths, and reruns
the previous record/attention/ancestry/sigmoid checks.15 guards cover shape,
index order/range, nonfinite/out-of-range probabilities and cancellation.

This does not select the real request's EF4 policy. It has no existing-source
correction,041240/041EC0/041660/041BA0 path,041CF0 score update, completed50/98
mutation, native allocation-failure behavior or final acceptance. These remain
required before the completed Math2 decoder can use it. The app is unchanged.
