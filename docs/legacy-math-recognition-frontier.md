# Original Math recognition: remaining producer chain

The portable integer-symbol LSTM and its numerical cost API implement a later
sequence-scoring stage. They do not supply the original recognizer's acoustic
hypotheses or its matched expression tree. The application currently uses the
separate modern Math2 path described in
[experimental AUTO routing](experimental-auto-math2-routing.md).

Static inspection resolves the original same-owner chain more precisely:

1. Processor009500 clones the actual base-tail AK13A4 receiver and passes that
   same clone into the grammar. It is distinct from the first-vector13A3 owner.
2. The terminal matcher02C370 calls the bound13A4.V20=01CD10. Its real subset
   cache and source alternatives lead to13A5.V38=015EB0.
3. That acoustic stage gathers source points, runs the configured per-slot
   feature/evaluation program and converts the results into a bounded shortlist.
   The13A4 receiver applies further geometry/special-label policies and publishes
   the hypotheses consumed by the terminal matcher.
4. Grammar matching029890 then feeds the actual tree issuer02A600. Its output
   pointer becomes worker+A0; only then does13F8 rescore that tree.

The stored grammar's53 children,26 rules and297 terminal names were decoded.
Every terminal name resolves through the same actual AK alphabet:259 labels,
258 strings and one virtual entry without a string. This verifies resource
membership, not acoustic recognition or grammar search.

There is a concrete conversion gap before a classifier can be connected:
the raw MLPs expose257 logical outputs, while initializer00CAF0 obtains259
from the actual alphabet and stores it as the later unified cost-sentinel
index. Directly assigning raw outputs to alphabet labels or calling the
virtual label256 that sentinel would skip native conversion/postprocessing.

Feature writer0327A0 also uses a different configured program from the English
directional feature extractor. Its temporal/grid branches, calibrations and
source callbacks must be selected from the actual slot/profile fields.
The [original XY normalization prerequisite](legacy-math-ak-source-geometry.md)
is implemented separately with bounded input and independent rounding controls.
It does not yet provide feature vectors or classifier output.
The next implementation is that same-source feature/classifier/shortlist
producer; grammar matching and tree materialization remain after it. Neither
modern LaTeX/Math2 DOM nor caller-supplied scores can substitute for those
original hypotheses and source ranges.

The static issuer evidence pins17 complete PE regions and four dispatch tables;
packet SHA256:
`655e9f0b8de3a13b57788724bab8c3b20f309280d3b57e339484c01924df671e`.
The additive feature/label packet pins the complete00CAF0 and0327A0 bodies;
SHA256:
`d9d68cd3adcbbeccbd26ef808368f177d1809c6dd428f7664677bc1bd5c251dd`.
Both use static PE bytes and resource parsing. No vendor code was executed.
These findings identify the missing algorithms and do not establish runtime
equivalence, recognition quality or completion of the original engine.
