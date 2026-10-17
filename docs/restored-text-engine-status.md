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
| Descriptor cut source | Sequential cuts, cross-source construction/pruning, compact traversal and raw source projection are checked. Global context, four-family NN/cache, dictionary source epochs, stored-range group geometry and line-aware transition caches are joined. The shared global controller now joins logical word rows, terminal selection, compact publication and numeric glyph backtracking; global WORD/document publication remains separate. |
| Global main numeric result | Same owned graph/context/NN/cache and source epochs through first/later language dispatch, exact primary recombination, terminal selection, ordinary compact207CB0 publication and numeric21FFD0 rows. The eight-input compact/backtrack checks preserve cached NN counts. Intermediate global publication and the213C00 line-increment guard are not closed. |
| Main candidate | One assigned physical line through CASE, first terminal and compact backtrack. Configured count2 joins actual raw preprocessing, exclusions, compact2410 publication, backtracking, same-TOP WORD publication, duplicate comparison and score publication. Rescue and final document processing remain separate. |
| Word publication | Per-segment residual association, glyph chain and same-owner plain WORD list; no supplied labels or success flags. |
| First candidate completion | Actual selected alternative scores, published-WORD quote cleanup, fresh-configuration glyph-normalization no-op, separate raw UTF8 WORD projections. No joined/accepted transcription. |
| Ordinary glyph alternatives | Actual positive CASE source export/scoring/compact-driven refresh and13A830 ordering are joined. Fresh one-descriptor negative spans have owned source export/refresh. Native failed-refresh retries/general protocol sources remain separate. |
| Text projection | Fresh273.V50 ->213A0/021000 choices and05AFA0 original raw positions are joined. A bounded first-WORD driver joins the219580 source prefix, clipping, overlap processing, ordered range interning, fresh024D40 metadata,021760 reuse and pending CHAR registrations. Bounded ordinary/configured no-bit8 TOP.V68 completion and actual shared275 WORD-array choices are joined, including genuine CHAR-key reuse, one shared UTF8/selection owner and pending CHAR+WORD commands. Actual NULL-reference raw-source projection now feeds pending kind2 TEXT registration. Actual021AB0 source-line publication retains separate append/write offsets; bounded025E40 validates every WORD/CHAR line and trims the unused guide tail after pending TEXT. General augmentation and0283B0 document acceptance remain incomplete. |
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

## Latest integration evidence (2026-10-17)

The global compact publisher and the old DIRECT/CASE/configured/live adapters
share one numerical207CB0 kernel. The actual global selected compact pool now
feeds a shared21FFD0 predecessor/row kernel. Ordinary global glyph scoring is
bound to the same cached acoustic/category/geometry owners; its consumer rows
remain explicitly prepared, and this is not yet accepted global text.

A separate nine-track global diagnostic reads coordinates and track hashes,
without expected labels: 140 nodes,868 groups,362 language queries/allocations
and16,557 finite ordinary anchors. Every anchor resolves to the first-word
branch; there are no observed later anchors. These are the same limited
single-path word recordings described above, not a larger accuracy benchmark.

The newly connected later-history branch has independent prepared history,
source70 token/epoch and unsigned heap-capacity checks. The current real inputs
do not establish later-history recognition. The line-increment213C00 native
branch is a newly identified remaining condition:391 adjacent parents satisfy
its line/flag guard, and125 in one20-node input also satisfy actual frontend110
soft-hyphen eligibility. Its118/140/128 geometry, allocation and actual insertion
still require examination. Qualified guard matches are not emitted candidates.

Details: [global compact](legacy-main-global-compact-materialization.md),
[global numeric backtracking](legacy-main-global-glyph-backtrack.md),
[global ordinary consumers](legacy-main-global-ordinary-glyph.md),
[global protocol consumers](legacy-main-global-protocol-glyph.md),
[global flush packets](legacy-main-global-glyph-flush.md),
[global width and packet channels](legacy-main-global-source-width.md),
[global residual/source metadata](legacy-main-global-residual-source.md),
[post-result metadata](legacy-main-global-result-terminal.md),
[later history dispatch](legacy-main-language-dispatch.md),
[shared WORD choices](legacy-main-shared-word-source-choices.md),
[source line publication](legacy-main-owned-source-lines.md),
[pending document TEXT](legacy-main-document-text-selection.md),
[bounded document line consistency](legacy-main-document-line-consistency.md),
[live source gates and exact WORD coverage](legacy-main-post-text-source-coverage.md).

## Remaining joins

- Main alternatives now have actual shared-TOP postcallback list comparison,
  duplicate release and owned score storage. The scoped configured count2 loop
  is joined to raw input; rescue, general properties and final document processing
  remain separate. Native mass/rank/TOP80 fields must not become a guessed
  acceptance probability.
- Multiple main descriptors/lines through the complete global controller (its
  graph/context/NN/dictionary-source components are now joined), remaining
  genuine empty-result branches and unsupported
  source/protocol/glyph branches. Native wrapped count/allocation boundaries
  need bounded portable handling rather than fabricated empty results.
- General protocol/source refresh and native failed-refresh retry semantics;
  complete TOP.V68 source augmentation, actual span WORD pruning and0283B0
  document orchestration. The bounded same-pool13D360 ranking, comparer and real span mutation are
  joined; general candidate expansion remains separate. Actual
  first-candidate glyph choices retain an immutable original raw document;
  sampled node/arc keys are not reused as document IDs.
- Remaining document postprocessors and retained C20 result-cache/final
  transport. Fresh source2DA.22Afalse is now derived through the actual
  frontendV250/context facet/source219 property chain, not its constructor.
  The scoped single-WORD027670 early return and exact0248D0 WORD-union/TEXT
  equality are checked. Pending CHAR/WORD/TEXT commands, trimmed source guides
  and zero residual association do not establish accepted document content.
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
[configured raw alternatives](legacy-main-configured-raw-composition.md),
[processed source wrapper](legacy-main-text-source-wrapper.md),
[prepared WORD ranking](legacy-main-word-ranking.md),
[cross-source graph](legacy-main-cross-source-graph.md),
[configured secondary transport](legacy-main-secondary-storage.md),
[first candidate completion](legacy-main-first-candidate-completion.md),
[word publication](legacy-main-owned-word-publication.md),
[UTF8 projection](legacy-main-word-utf8-projection.md),
[result assembly gaps](legacy-main-result-assembly-roadmap.md), and
[string wrapper gaps](legacy-main-string-wrapper-roadmap.md).
