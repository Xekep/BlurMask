# English periodic pending-TEXT: first runtime boundary

2026-10-26. The isolated coordinator builds, but its first actual run does **not**
complete pending TEXT or positioned transcription. It is not admitted to the app.

## Source and build

The source-only packet
`english949-periodic-pending-text-source-20261026/manifest.json` has SHA-256
`1bab0b0b1fb83430324a2f6c591eaa852a4a78851317467fd62e9a76f29fda92`.
It replaces two existing production files over the authenticated e233949 stage,
adds no production file, and adds a separate one-file test entry.

The coordinator requires the authentic original-selected input context prepared
before neural processing. It requests TOP, all word choices, CHAR, geometry,
normalization, pending TEXT and source coverage before the one existing periodic
loop. Cleanup uses nested `finally`: release the original input context even if
CHAR disposal throws. The previous e233 route body remains unchanged.

Both exact source inverses and four unknown-path/changed-byte refusal controls
passed. The library and entry compiled; portable-PDB verification bound all 949
library documents and the one entry document to the reviewed source. The entry
references the cached DLL, and its copied library bytes match that DLL.

The initial preparation driver failed before compilation because a source pin
included a `role` metadata field in an exact three-field comparison. The second
driver strips metadata for comparison; the path, length and hash requirements
remain unchanged. The initial driver is retained.

## First actual failure, retained

The original raw array hash, index 4, three original points, recorded resolution
96, existing seven resources and their hashes remain unchanged. The eighth,
explicit normalization resource is 57,864 bytes with SHA-256
`3cc1d91269959a13b5700b7463d3db7442b83b593ba70435a94665d6531226f2`.
Expected labels are not passed to inference.

The run passed original-input preparation, FIRST, parent postprocessing, local
MAIN and periodic source preparation. It failed during `owning-periodic-graph-run`
in `GraphWordPublicationContext` before its `CaptureSpan` allocation. The actual
error is `Node0 cumulative payload exhausted before allocation.` Cancellation
was false; pending capture/receipt were false. This does not establish successful
CHAR, TEXT or coverage publication, or complete disposal correctness.

A second, explicitly observational entry reads scalar ledger counters only in
the existing failure catch. It uses the exact same compiled library and the same
inputs; the library was not rebuilt. All original failure fields except elapsed
time matched, and per-site sums matched the aggregate counters:

| Counter | Value |
| --- | ---: |
| Charged cumulative payload before refusal | 9,409,841 bytes |
| Existing payload cap | 9,437,184 bytes (9 MiB) |
| Remaining | 27,343 bytes |
| Next fixed reservation | 65,536 bytes |
| Copy units | 47,305,713 |
| Visit units | 16,665,234 |

These are conservative logical cumulative counters, not live memory, RSS, CPU
instructions or a memory leak measurement. The failed reservation precedes the
associated allocation. No cap, input, expected label or acceptance gate changed.

## Next work

Review the allocation bounds across the connected raw/source and rich word
publication chain. The failing capture uses a fixed maximum-span reservation;
an existing allocation-free live TOP scalar reader could support a bound based
on the authenticated actual span size. That alone is not yet a sufficient budget
proof for the later CHAR/TEXT work. Audit all associated allocations and copies
before changing accounting; avoid lowering a charge merely to pass this sample.

After rich publication works, a live selected positioned exporter is still
required. It must use the actual selected ordinal and final canonical CHAR
records and adjusted markers, before StringPool disposal. Earlier historical
choice arrays and the raw single-head facade are not winner authority. Corpus
quality, English app integration and RU/EN arbitration remain unfinished.

Evidence is under `E:/BlurMask-build-artifacts-20261014/`:

- `root-english949-pending-text-verification-v2-20261026/report.json`
- `root-english949-pending-text-actual-20261026/report.json` (expected failure)
- `root-english949-pending-text-failure-ledger-20261026/report.json`
- `root-english949-pending-text-owning-frozen-manifest-20261026.json`

## Progressive accounting successor: later failure preserved

The four-file SOURCE successor `5196d13bbf92bd4f32f70908eca39260d30267426411f59076f96e980634013a`
has now passed independent source review, exact inverses, eight changed-byte/path
refusals, compilation and PDB binding (949 library files, one numeric entry and
the unchanged one-file actual entry). It keeps existing limits and outputs.

Its new rich-route accounting charges the actual selected backtrack steps and
emitted rows rather than a maximum per every compact candidate. It bounds span
capture from the authenticated sole-head span count, and stops charging for a
late bootstrap/import when that branch only reuses the already authenticated
pre-neural input. Default and e233 source branches retain their old charges;
runtime regressions of those branches are still separate work.

The first numeric execution matched the separately pinned Python reference:
4,096 RAW bounds, 4,095 span bounds, six input-reuse bounds, eight actual prepared
publisher/capture cases, nine invalid cases, cancellation and 20 corruption
controls. These synthetic records do not establish recognition or native parity.

The first recognition run used the unchanged input and eight resources. It
passed the previous payload refusal but failed later at the precharged compact
predicate scan in `CompleteGraphGlyphChoiceRows` (line 82 in this isolated stage):
`Node0 visit allowance exhausted before source access.` A failure-only observer
again used the exact same DLL and preserved all old failure fields except time.

| Counter before the new refusal | Value |
| --- | ---: |
| Logical cumulative payload | 7,954,673 |
| Copy units | 44,387,873 |
| Visit units | 16,750,242 |
| Remaining visit units under the unchanged limit | 26,974 |

These totals occur at a later execution boundary than the previous failure;
their difference is not a benchmark or physical memory saving. Pending TEXT and
the selected positioned exporter are still not complete. Next work must examine
genuine repeated glyph/publication work and the full-route work budget, rather
than merely lowering the next constant to make this fixture pass.

Additional artifacts under the same root:

- `root-english949-progressive-budget-verification-20261026/report.json`
- `root-english949-progressive-budget-numeric-20261026/report.json`
- `root-english949-progressive-budget-actual-20261026/report.json` (failure)
- `root-english949-progressive-budget-failure-ledger-20261026/report.json`
- `root-english949-progressive-budget-owning-frozen-manifest-20261026.json`

The last manifest SHA-256 is
`3780e4ee19b86f6f6a94c428d4f359dc4ae514227c9f82b59d975aedf56c6495`.
