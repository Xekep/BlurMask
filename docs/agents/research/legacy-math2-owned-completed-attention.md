# Actual completed-event attention snapshots

The carried-state identity established in the attention ancestry checkpoint is
now connected to an actual completed-event lease. This remains an internal
library stage, before EF4 and final mathematical output; the app recognizer is
unchanged.

`CaptureAttention` accepts no copied score, caller parent or arbitrary tensor.
It uses the event's retained raw48 parent and old primary prefix while that
exact context/Step/candidate generation is live. It verifies the cold policy
forces nonobservable positions. An uninterrupted actual-selection history is
required; any diagnostic selection permanently refuses this shortcut. This
avoids claiming equivalence when alternate20 and symbol08 have different
observable status. Cancellation before capture leaves it reusable.

The snapshot contains the transposed `[symbols,segments]` attention for that
event and its exact source identity. Repeated capture during the live step
returns the same immutable snapshot. Apply or disposal prevents a new capture,
while copied snapshot values remain valid independently of the disposed ORT
state. Caller copies cannot mutate it. Sigmoid conversion uses the separately
checked portable036C70 arithmetic.

Copying reads only the selected beam directly under the tensor-state lifetime
lock. It allocates neither a full multi-beam attention tensor nor any decoder
KV cache. Validation precedes output allocation, with at most1,000,000 F32
values per snapshot (4MB). This is an explicit bound for this adaptation,
not a native memory-limit claim. Snapshots are opt-in at this stage; they are
not yet collected by the outer recording loop.

The headless probe preserves the old full20-step prefix and owned-record
captures, adding an attention sidecar. It checks11 actual EOS snapshots,
copy isolation, cached identity, cancelled capture, stale generations, disposed
owners, and refusal after a diagnostic selection. Four distinct prepared
coordinate tensors test the direct selected-beam transpose, including16 beams.
An oversized-output control verifies rejection without allocating its output.
An independent Python oracle reads the actual captured tensors, reconstructs
backward ancestry separately, and checks each sigmoid against high-precision
decimal exponentiation. The earlier prepared sigmoid tests still cover F32
rounding and overflow boundaries.

Remaining work includes fallback-event integration, loop-owned snapshot
collection and total memory budgeting, EF4 output units/auxiliary data,
ordering/truncation and final recognition acceptance. These tests do not
establish better handwriting accuracy or a complete formula decoder.
