# Restored text engine status

The full text engine is not restored. Russian cursive and automatic replacement
are not ready for general use. The restored main pipeline is not the app's
default recognizer.

## App and library are different stages

The optional **Experimental text engine** setting shipped in 1.2.8-rc.7 uses the
bundled, bounded first-stage single-word decoder. Its native candidate score is
not calibrated recognition confidence. Enabling it does not enable the main
CASE dictionary pipeline described below. The ordinary English recognizer and
the separate formula/solver modules retain their existing paths.

The reusable library now has these checked stages:

| Stage | Current boundary |
| --- | --- |
| First-stage inference | Same-owner NN, graph, selected single or multiple plain words; unsupported traits/terminal columns remain explicit refusals. |
| Main preprocessing | Actual selected glyph records to source720, line fitting, deskew and smoothing; empty first-result continuation remains incomplete. |
| Descriptor cut source | Sequential ordinary and pending-separator steps share one XY/source chronology. Append/remap/root stitching is checked, with an actual adapter limited to whole-physical single-atom descriptors; cross-source construction/pruning remain incomplete. |
| Main candidate | One assigned physical line through the CASE dictionary, scheduler, first terminal and actual compact backtrack. |
| Word publication | Per-segment residual association, glyph chain and same-owner plain WORD list; no supplied labels or success flags. |
| First candidate completion | Actual selected alternative scores, published-WORD quote cleanup, fresh-configuration glyph-normalization no-op, separate raw UTF8 WORD projections. No joined/accepted transcription. |
| Ordinary glyph alternatives | Actual nonnegative CASE source export, original-group scoring, compact-driven pool refresh and13A830 ordering are joined. Negative/span sources and native failed-refresh retry remain separate. |
| Text projection | Raw ordinary glyph UTF8 and sampled source keys; final document/string wrapper remains separate. |
| Long-source publication | Genuine native 120-node gate and prefix/final-append transport checked; a nonempty genuine long-source winner is not established. |

`LegacyFreshMainSingleLinePlainRecognition` joins raw physical strokes to the
bounded first main candidate's WORD list. Its `PublicationCompleted` property
means the word callbacks completed, not that the transcription should replace
ink. Multiple lines are currently refused. The app is not switched to this API.

`LegacyFreshMainFirstCandidateCompletion` now joins that actual publication to
the first-alternative snapshot, quote cleanup, the proven fresh-configuration
normalization no-op and per-WORD raw UTF8 projection. Completion is single-use
once mutation starts; failed prefixes are retained. It does not manufacture
spaces between WORDs or expose accepted replacement text.

## Quality evidence

A diagnostic subset contains nine short Russian word tracks from three writers.
The source has no pen-up metadata, so each track is explicitly treated as one
continuous path. Expected labels are read only after all inference captures
exist; they do not enter scoring, decoding, aliases or normalization.

At the raw main WORD-to-UTF8 stage, the subset gives 4/9 exact words and 16 edits
over 33 reference Unicode code points: 48.48% case-sensitive CER. This is neither
a representative nor a held-out benchmark. Allocator-residue repetitions do
not increase the sample count. These results demonstrate why Russian support
must not be described as finished.

Managed/Native AOT byte parity, independent numeric/byte oracles and static
instruction checks establish the stated component behavior. They do not prove
handwriting accuracy or equivalence to the whole original engine. No vendor
DLL is executed by the library or these tests.

## Remaining joins

- Main alternative construction, duplicate/exclusion handling and final score
  ownership. Native mass/rank/TOP80 fields must not become a guessed acceptance
  probability.
- Multiple main descriptors/lines, empty-result continuation and unsupported
  source/protocol/glyph branches. Native wrapped count/allocation boundaries
  need bounded portable handling rather than fabricated empty results.
- Negative/span source metadata and remaining glyph-refresh branches, native
  failed-refresh retry semantics, and final UTF8/document-source wrapper
  orchestration. Ordinary positive CASE refresh/selection now has an owning
  adapter; sampled node/arc keys are still not document point IDs.
- Representative online handwriting evaluation with real pen-up events,
  Russian cursive, English, mixed text and mathematical notation before enabling
  the new path for automatic app replacement.

Details: [raw main integration](legacy-main-raw-plain-recognition.md),
[first multiword preprocessing](legacy-main-text-input.md),
[shared descriptor cuts](legacy-main-descriptor-cut-source.md),
[bounded graph append](legacy-main-graph-append-stage.md),
[ordinary glyph refresh](legacy-main-ordinary-glyph-refresh.md),
[first candidate completion](legacy-main-first-candidate-completion.md),
[word publication](legacy-main-owned-word-publication.md),
[UTF8 projection](legacy-main-word-utf8-projection.md),
[result assembly gaps](legacy-main-result-assembly-roadmap.md), and
[string wrapper gaps](legacy-main-string-wrapper-roadmap.md).
