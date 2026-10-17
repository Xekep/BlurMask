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
| Main preprocessing | Actual selected first TEXT to source720,24B/24A geometry, deskew and smoothing.24A is joined; a genuine positive empty-first fixture/remaining temporal branches are not established. |
| Descriptor cut source | Sequential cuts, local metadata, cross-source construction/pruning, compact selector traversal and original source-range projection are checked. Global context selection is under further descriptor-to-line ownership review; global NN/controller integration remains separate. |
| Main candidate | One assigned physical line through CASE, first terminal and compact backtrack. Configured count2 now has actual terminal exclusions, reachability and compact2410 publication with owned selected terminal identities. Its backtrack/WORD join is still separate. |
| Word publication | Per-segment residual association, glyph chain and same-owner plain WORD list; no supplied labels or success flags. |
| First candidate completion | Actual selected alternative scores, published-WORD quote cleanup, fresh-configuration glyph-normalization no-op, separate raw UTF8 WORD projections. No joined/accepted transcription. |
| Ordinary glyph alternatives | Actual positive CASE source export/scoring/compact-driven refresh and13A830 ordering are joined. Fresh one-descriptor negative spans have owned source export/refresh. Native failed-refresh retries/general protocol sources remain separate. |
| Text projection | Fresh273.V50 ->213A0/021000 choices and05AFA0 original raw positions are joined. Separate ordered range interning, existing-glyph021760 reuse and pending selection transactions are checked; processed range operators, full024F30 and document assembly remain separate. |
| Long-source publication | Genuine native 120-node gate and prefix/final-append transport checked; a nonempty genuine long-source winner is not established. |

`LegacyFreshMainSingleLinePlainRecognition` joins raw physical strokes to the
bounded first main candidate's WORD list. Its `PublicationCompleted` property
means the word callbacks completed, not that the transcription should replace
ink. Multiple lines are currently refused. The app is not switched to this API.

`LegacyFreshMainRecognitionContext` now shares owned immutable models between
invocations while preserving separate raw documents and mutable dictionary
contexts. Existing diagnostic output is unchanged. The local library-only
[memory comparison](legacy-main-model-context.md) does not establish app memory
use or recognition quality.

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

Both raw main WORD-to-UTF8 and the new completed glyph-choice/source-position
join give4/9 exact words and16 edits
over 33 reference Unicode code points: 48.48% case-sensitive CER. This is neither
a representative nor a held-out benchmark. Allocator-residue repetitions do
not increase the sample count. The new complete join processes all nine tracks
without explicit refusals and preserves identical Managed/Native AOT captures
SHA256 `7aa4e8899fdd12921d6de9cc50f31bac294a29628289c4168eedf24aa2810286`.
This shows component integration, not improved recognition quality. Russian
support must not be described as finished.

`LegacyMainRecordedTextChoices.Probe` reads only track coordinates/identities
and resource data during inference. Its separate `audit_output.py` validates
source positions and selected markers, then reads expected labels for metrics.
The recorded corpus remains external under its original CC-BY4.0 attribution;
its diagnostic manifest pins upstream commitc6629026b1fdef51fd08e3d2fddffd82f97e60a5.
The CLI takes AK,LK,engine,separation,case,publication,normalization,circles,
line30,line50,recorded-manifest,capture. The audit takes capture,manifest,report.

Managed/Native AOT byte parity, independent numeric/byte oracles and static
instruction checks establish the stated component behavior. They do not prove
handwriting accuracy or equivalence to the whole original engine. No vendor
DLL is executed by the library or these tests.

## Remaining joins

- Main alternatives now have actual shared-TOP postcallback list comparison,
  duplicate release and owned score storage. General terminal exclusions/rescue
  and configured secondary winner-to-WORD publication remain separate. Native mass/rank/TOP80 fields must not become a guessed acceptance
  probability.
- Multiple main descriptors/lines through global compact/context/NN, remaining
  genuine empty-result branches and unsupported
  source/protocol/glyph branches. Native wrapped count/allocation boundaries
  need bounded portable handling rather than fabricated empty results.
- General protocol/source refresh and native failed-refresh retry semantics;
  remaining024F30 source-node interning/reuse, metadata and0283B0 document
  orchestration. Actual first-candidate glyph choices now retain an immutable
  original raw document; sampled node/arc keys are not reused as document IDs.
- Representative online handwriting evaluation with real pen-up events,
  Russian cursive, English, mixed text and mathematical notation before enabling
  the new path for automatic app replacement.

Details: [raw main integration](legacy-main-raw-plain-recognition.md),
[first multiword preprocessing](legacy-main-text-input.md),
[shared descriptor cuts](legacy-main-descriptor-cut-source.md),
[bounded graph append](legacy-main-graph-append-stage.md),
[ordinary glyph refresh](legacy-main-ordinary-glyph-refresh.md),
[actual glyph choices and raw positions](legacy-main-text-choices.md),
[postcallback alternatives](legacy-main-alternative-assembly.md),
[cross-source graph](legacy-main-cross-source-graph.md),
[configured secondary transport](legacy-main-secondary-storage.md),
[first candidate completion](legacy-main-first-candidate-completion.md),
[word publication](legacy-main-owned-word-publication.md),
[UTF8 projection](legacy-main-word-utf8-projection.md),
[result assembly gaps](legacy-main-result-assembly-roadmap.md), and
[string wrapper gaps](legacy-main-string-wrapper-roadmap.md).
