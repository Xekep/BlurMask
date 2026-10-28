# English original-line frontend check

This is a frontend diagnostic, not a recognition-quality result or a completed
MAIN/TEXT integration. The source was built from
`fa35565d15f4857a15279f0290dca6f1ba796ccc` in an isolated checkout.

## Input and provenance

The fixed input is writer 0, sample 0 from the original variant of the
[BRUSH research dataset](https://github.com/brownvc/decoupled-style-descriptors#brush-dataset).
The sample was chosen by its numeric identifier before running inference.
Other variants and other samples were not searched for a successful result.
The dataset author's original variant already includes temporal interpolation;
this check does not claim to use raw device events.

The dataset is restricted to noncommercial research. Neither the sample nor its
target text is included in this repository, packages, or release artifacts.
Only one ZIP member was downloaded. Its pickle was inspected as opcodes and
literal byte payloads; no pickle deserialization or embedded callable was run.
The original drawing payload was verified byte-for-byte after conversion.

The inference input contains 10 strokes and 689 points. Author-provided pen-up
markers define the stroke boundaries. Coordinates were not flipped, scaled,
resampled, clipped, split, or combined; the existing Float32 consumer performs
one conversion from the stored Float64 values. Device DPI is unknown, so 96 is
an explicit diagnostic convention, not measured physical metadata.

Input SHA256:
`5900b04db0faa5e9c8401a58d94b1f4502704081e9e638b98ab54ad6eed55129`.
The target text was kept separate and was not read by the inference entry.

## Observed result

One root-owned invocation completed in about 1.13 seconds with two logical
processors, affinity mask 3, a 40-second cooperative deadline and a 45-second
outer watchdog. This is an observation, not a latency guarantee. Source,
resources, runtime and own binaries had 1,122 pinned entries checked before
and after the invocation.

All real frontend children were processed. The input produced one nonflat
child with 48 nodes, 315 groups and 279 remaining candidate arcs; 36 arcs were
removed by the existing candidate preparation. FIRST119 completed for that
child. Parent postprocessing reached `post265`, with one descriptor, one guide
and 16 rows. No input strokes or frontend children were omitted to fit an
entry-point restriction.

Capture SHA256:
`b7bd4e015e91f97c13aea51e0cb9726ff85fed3abc5568cb69b7f21590683a48`.

This invocation did not run MAIN, complete a shared TEXT/document, admit a
recognition result, or compare a recognized sentence with its target. The
multiple-word MAIN publication and shared TEXT transaction still require their
own source review and verification. The observation establishes a genuine
original-stroke input for that work; it does not establish English or Russian
handwriting accuracy.

## Subsequent global MAIN diagnostic

One subsequent invocation passed the same unchanged drawing to the restored
global MAIN path using the Russian lite resources. This exercises genuine
stroke boundaries; it is not an English language-quality comparison. It used
two logical processors/affinity3, an internal30-second cancellation deadline
starting before scope preparation, and an outer45-second watchdog. Source,
models, runtime and own binaries had1183 pinned entries checked before/after.

The invocation failed after about4.16 seconds during `original-source-run`,
before batch source completion or shared TEXT publication. The exception was
`The glyph has no spatial extent` from `LegacyGlyphGeometry.Create`, reached
through the global acoustic scorer's baseline-family feature extraction.
One original `Scope.Run` was entered; the batch validation meter was not yet
created. The input was not altered and the invocation was not retried.

Failure capture SHA256:
`c23438bde275bfa6c5e2bc18eb6c3b8976f0d2653210a357eea1d7201af6e04e`.

The native MAIN handling of collapsed/pointlike candidates must be restored
before this input can verify the later multiword transaction. FIRST degenerate
feature support alone does not prove the different MAIN feature programs.
Discarding a dot or padding its geometry would change the input and is not a
resolution. No successful MAIN/TEXT or recognized sentence is claimed here.

## MAIN point feature correction

The actual2B1 MAIN classifier now has the native pointlike branch for its two
baseline feature families. For the authenticated 1111 profile and baseline
frame (0,100), identical normalized points produce the literal 256/238-input
vectors, including the existing 28 context values and both boundaries.
The original point coordinates are retained. Other frames decline this branch;
generic extractors and nonpoint inputs keep their previous behavior.

Root independently checked the native byte regions and the complete feature
vectors for every signed-short Y: 65,536 positions, 131,072 vectors, 14 complete
checkpoints and seven domain/cancellation guards. The current main library and
probe built with zero warnings/errors; its exhaustive independent Python audit
passed, with 865 source pins unchanged before/after. These checks load no models
and execute no neural networks. The portable probe and audit run in managed CI.

This corrects the missing point-feature branch. Successful sentence recognition
and shared TEXT publication still require the unchanged drawing to pass the
complete owning pipeline; the original failure above remains recorded.

A single subsequent invocation with this source correction and the byte-identical
drawing passed the previous point failure. The original source completed, with
374 cached/current MAIN acoustic evaluations. It then explicitly refused at
`batch-source-and-carried-rank`: `Actual residual217DB0 augmentation association
remains separate`. The batch structural meter had counted 57 loop bodies;
this is not a count of neural forwards or all CPU work. The unchanged-resource
checks, cancellation guards and precompletion TEXT guard were preserved.

This run took about 4.83 seconds, used two logical processors/affinity3 and the
same 45-second outer watchdog, and verified 1,105 source/resource/runtime/binary
pins before/after. Its failure capture SHA256 is
`275eedaf9238ecb8758b6719ed284bc914287bed0659805c40345657c02b1b98`.
The residual association is the next implementation frontier. Neither failure
was replaced, and this result does not claim completed multiword TEXT.

An experimental successor outside main then connected residual association to
the same original session's private overlay. One unchanged-input invocation
reached three residual inputs/nine emitted rows and one merge, with no newly
allocated higher source. That last observation includes either an existing
higher source or a matching base source; it is not exclusively a cache hit.
The completed association reserved 15,900 structural loop units and 13,477
copied-payload units under the existing caps. These are reservations rather
than measured CPU work, CLR allocations or RSS. Validation counted 93 loop
bodies; original cached/current MAIN evaluations both remained 374.

After about 4.81 seconds the invocation refused at the same batch stage:
`Actual emitted candidate source span changed`. Capture SHA256:
`86345425de9bf02018c92f6a52f3de93d62820bf1420db126ebe6ac03623617e`.
All 1,135 source/resource/runtime/binary pins matched before/after, using two
logical processors/affinity3 and the same 45-second watchdog. The earlier
failures remain immutable. Native candidate-span adoption/boundary behavior
must be understood before changing that invariant. This is no successful
sentence, completed multiword TEXT, app/default integration or quality claim.

A further experimental successor corrected that boundary check only for the
private batch path. Native `219580` selects the existing 275 owner during its
count pass, before residual association; its `13FE20` call receives that same
owner rather than a newly derived post-association chain span. The successor
therefore requires the exact current private worker, publisher and preexisting
owner. Count-pass endpoint eligibility, the standalone equality guard and all
source keys remain unchanged. Static PE/instruction checks and 20 prepared
interner cases/200 checks passed; those prepared objects cannot issue a live
source or TEXT owner.

One byte-identical drawing invocation then completed the batch source stage,
including six residual inputs, 17 emitted rows and two merges. No new higher
source was allocated; cached/current MAIN evaluations both remained 374.
Validation counted 772 structural loop bodies, with association reservations
of 30,398 loop units and 25,857 copied-payload units. These still do not measure
all CPU work, allocations or RSS. The invocation took about 4.88 seconds with
two logical processors/affinity3 and a 45-second watchdog; all 1,113 pins matched
before/after.

It next refused at `batch-TEXT-prepare`: `All275 publication currently requires
ordinary base-source glyph owners.` The preserved failure SHA256 is
`0e17f51090cb7e2b0b7c9481e2551e4d35a218491ae1410b3f2c372970fe621a`.
The same-batch completion check and standalone-wrapper/Choices refusal checks
passed before this stop. Associated-source publication into the owning WORD/TEXT
path remains unfinished. These experimental changes remain outside main; this
is not a completed sentence, Russian recognition improvement or app integration.
