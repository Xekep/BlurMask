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
An [immutable library loader](legacy-math-grammar-resource.md) now binds this
configuration to the actual AK alphabet and reconstructs the complete stored
grammar from parsed fields with independent checks.
Every terminal name resolves through the same actual AK alphabet:259 labels,
258 strings and one virtual entry without a string. This verifies resource
membership, not acoustic recognition or grammar search.

The raw MLPs expose257 logical outputs, while initializer00CAF0 obtains259
from the actual alphabet and stores it as the later unified cost-sentinel
index. The [fixed selected-five classifier](legacy-math-unified-classifier.md)
now composes that distinction with the262-slot cost projection. It covers the
fresh unmasked, unguided single-stroke numerical branch; it does not yet bind
an actual13A4 matcher request. Virtual label256 remains distinct from the
unified sentinel259.

Feature writer0327A0 also uses a different configured program from the English
directional feature extractor. Its temporal/grid branches, calibrations and
source callbacks must be selected from the actual slot/profile fields.
The [original XY normalization prerequisite](legacy-math-ak-source-geometry.md)
is implemented separately with bounded input and independent rounding controls.
The [single-stroke unguided temporal prefix](legacy-math-ak-temporal-features.md)
now produces the selected1F model's222 inputs with independent source-derived
checks. The [source-bound1F numerical network](legacy-math-ak-temporal-network.md)
now evaluates those inputs with exact modular arithmetic and network-local
probability/cost channels. One controlled actual forward matched the independent
reference. The [source-bound spatial prefix](legacy-math-ak-spatial-features.md)
now supplies the selected2F model's204 inputs. The
[selected2F numerical network](legacy-math-ak-spatial2f-network.md) also matched
an independent complete integer replay on one fixed actual forward. A
[shared immutable feature profile](legacy-math-ak-feature-profile.md) now keeps
all four calibration pairs once for repeated geometry subsets. The
[selected3F directional temporal features](legacy-math-ak-directional3f-features.md)
and [source-bound3F numerical view](legacy-math-ak-directional3f-network.md)
now also match independent original-geometry and numerical references. The selected4F quantile-grid and selected5F upper-direction features and their networks also match independent original-geometry replays. All five fixed numerical views and their
unified AL projection are checked. The selected source owns five networks;
the six-model raw catalog also includes a separate first-vector source.
The [directional feature bundle](legacy-math-ak-directional-bundle.md) now
prepares the common3F/4F/5F geometry once, preserving all three independent
feature results without retaining intermediate samples in the output.
The [shape299 primitive](legacy-math-ak-shape299.md) now derives30x7 features
from the same private normalized geometry and compares them against the actual
immutable13A6 prototypes. The classifier now applies the selected prototype's
cost adjustment and produces the unified shortlist. Prepared checks cover all39
prototype entries; the one connected actual diagnostic selects a label with
no prototypes, so those two scopes remain separate.
The next connection requires the genuine processed source, matcher bitset,
original-point/range mapping and13A4 subset cache. The separate first-vector
source, remaining multi-stroke/guide branches, grammar matching and tree
materialization are still required. Neither
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
