# Prepared Math2 candidate heap

The internal `LegacyPreparedMathCandidateHeap` restores the owned0351E0
48-byte max-heap helper. It retains the lowest offered costs; equal costs are
rejected once full. During downward movement equal children select the right.
COMISS/JAE branches use ordered greater-or-equal: NaNs fall through instead
of being ranked by a generic comparer. All48 bytes move with each parent or
child, then the returned slot is fully zeroed and receives the offered cost.
The count increments before upward movement. Capacity zero refuses offers.

The constructor takes explicit initial capacity storage, so bytes in unused
slots are neither invented nor mistaken for initialized candidates. The
portable capacity bound32 corresponds to the supported maximum16 beam width
and the caller's2-times-width candidate buffer; it is not an arbitrary
configuration factory. Cancellation is checked before this bounded helper.
The separate internal prepared payload write models a caller writing after
the helper returns; it does not restore that caller's input producers.

The actual unrestricted branch in038610 invokes0351E0 for every decoder class
of each beam, then writes beam ordinal28, symbol8, metadata10, conditional18,
and all-ones20. Its candidate costs come from EEF's separate cumulative-cost
channel. The configured C8 branch has additional alias/constraint processing;
neither full beam route is implemented by this helper.

The existing math ordering probe adds64 heap scenarios,4096 offers, comparing
complete capacity bytes both before and after each prepared payload write.
They include capacities0 through32, full replacements, parent/child movement,
equal scores, infinities, signed zero and NaN payloads. An independent raw-byte
oracle replays both states. Existing181 completed-record ordering controls
and4096 default completed-score controls remain separate. This is algorithm
verification, not handwriting accuracy or a decoder replacement.

`tools/handwriting/audit_math2_candidate_heap.py report.json` checks the exact
whole DLL fingerprint, helper/caller regions and34 instruction pins. It reads
PE bytes only and never executes the vendor SDK.
