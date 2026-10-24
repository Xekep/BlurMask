# Completed Russian word augmentation to selected TEXT

The original global publication scope can now bind its genuine completed word
augmentation directly to text publication. A private completion capability keeps
the same selected input document, live selected word, glyph pool, original source
references and publication lease. Detached result buffers cannot issue it. The
scope adopts the already completed session without a second completion claim.
Bad resources, incomplete owners and foreign scopes fail before adoption;
adoption failures fault the scope instead of permitting a partially initialized
retry. Disposal invalidates the bound operations.

The old typed NoCandidate APIs and property types remain available. An augmented
owner's legacy completion accessor throws explicitly because that old authority
does not exist. Internal text stages use the private completion capability. No
new global weak-table authority was introduced.

This implements the current selected TOP word route. Its active alternative count
is read from the actual snapshot. All current WORD120 payloads are available
separately as defensive metadata copies; that is not complete native TEXT
enumeration of all word alternatives. The public AI-pen adapter still uses its
earlier path and is the next integration step.

## Verified actual scope

Root ran one unchanged 74-point recording through genuine scope recognition,
owning augmentation and selected text publication. The result was `да`, with two
actual word payloads and one selected TOP. All25 ordered lifecycle checks passed,
including two exact cancellation tokens, early owner/resource/same-document
foreign-scope refusal, repeat/cache behavior and disposal. Acoustic evaluation
count did not increase during text publication.

An independent DATA oracle derived the selected ordinary Cyrillic characters
from the original acoustic alphabet and live word references. It checked both
retained word payloads, existing glyph-record prefixes, selected character
records and UTF8, and all three original-input range exports. The source range
was exactly `[3,4849413]`. Ten corrupted captures were rejected. This single
case does not measure handwriting quality or establish full Russian recognition.

The first structural checker incorrectly required the entire glyph pool to stay
identical. It failed and was preserved. The production refresh legitimately
appends two cached alternatives to each of two selected glyph owners; previous
records and all word payloads remain unchanged. The independent oracle checks
that prefix contract. A separately reviewed structural successor derives the four
added descriptor ordinals from the original compact node intervals and checks
their order, packed counts, flags and initialized scalar/link fields. It binds
the text prestate exactly to the earlier completed augmentation capture. Its
eight corrupted DATA controls are preserved separately. Appended neural score
values are not independently recalculated by that structural checker. No neural
run was repeated to change the outcome.

Evidence: external frozen source manifest `156e00f2…`, actual harness
`67594952…`, root capture `dcc584d5907af1a17767c5b71c6768f79cde887e60a89e73004e0c216fdcef82`
in `root-ru-augmented-selected-text-actual-20261024`. The actual invocation used
CPU2, affinity3, a30-second diagnostic deadline and45-second process watchdog.
Its roughly2-second cold process time is not proof of the public2-second budget.
All frozen source/resource/binary pins were checked before and after execution.

## Reproduction and remaining checks

The prepared probe uses detached source26 DATA and the existing recording. It
checks original range projection, closed constructors, unchanged old property
types, the genuine augmentation overload, no Result ingress and28/28/16 byte
struct strides. It executes no neural model and runs in CI. The old WORD/TEXT
probe also compiles; old q0 runtime compatibility remains unproven on current
flags and is not inferred from historical captures.

The separate actual entry requires explicit `--actual`. Generate a fresh plan
with `python -I tools/LegacyAugmentedText.ActualEntry/write_plan.py REPO PLAN`,
then use `--validate-plan PLAN REPORT` to verify input/table delivery without
model loading. Root-only controlled actual testing uses `--actual PLAN FRESH_DIR`
under a CPU2/affinity3 watchdog. The independent check is
`python -I tools/LegacyAugmentedTextBinding.Probe/audit_actual.py REPO PLAN CAPTURE FRESH_REPORT`.

New augmentation preparation owns a572763-byte acoustic snapshot; source
references are capped at4096 and the existing projection helper caps visits and
output ranges. Optional all-word metadata copies can allocate120 bytes per word
plus arrays and wrappers. These are logical payload bounds, not process RSS or
whole-allocation guarantees. Repeated count-only metadata calls currently copy
the captured span and can be optimized separately.
