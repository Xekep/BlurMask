# Owned Math2 source positions and formatted-unit ranges

The named source-position route retains the actual original raw document and
produces C79 coordinates alongside the same feature samples used by the official
ONNX Runtime encoder/initializer/decoder. After full EE4 formatting it attaches
source ranges to every actual retained output record. This closes the missing
046AE0 source-field producer for this library-owned raw-only route. It does not
publish an SDK page, a DOM node, an external source tag or an accepted formula.

The earlier [source transport proof](legacy-math2-source-position-transport.md)
and [mapping producer proof](legacy-math2-source-mapping-producer.md) establish
the selected C6D processor order, fresh empty-cache lifecycle, V20 permutation
and V18 composition. The raw resource orders original mean-X reordering before
group-extrema scale, normalization and resampling. The shared core uses the
separately verified literal 01ED50 sorter for all supported 1..256 sources;
partitions above20 are not assumed stable. Raw source IDs remain original
document ordinals after permutation.

## Native details that affect source coordinates

The V18 scalar uses the native float32 add-before-multiply quantization chain.
It is distinct from the initial `Pack` codec. A stored fraction200 is retained
without carrying into the next point. The complete00F7E0 validator has no
numeric fraction<200 predicate: it checks owner/source membership, separator
and cache extents, overlap rules and packed ordering. Constructor00EE90 stores
C79.B0=1 at00EF56. Resource `version=1` is read into resampler+10 at016B6C;
0184B7..0184BD clears B0 only for version<1. The internal processed constructor
therefore requires strictly ascending packed positions within each segment for
this scope. It is not a general C25/nonempty-cache validator.

017060 unconditionally overwrites the last temporary coordinate with the raw
terminal at018367..018387, even when the last XY sample stays interior. The
shared producer applies this overwrite in the coordinate plane without changing
feature values. A singleton's sampled position ends in FF3 while its enclosing
original source range still has the mode5 terminal. The independent interval
oracle models these as distinct values.

046AE0 reads every actual mapped run `[previous, EndSegmentExclusive)` through
the same processed C79 reader. A nonnegative mapped unit receives the native
C21 union of its current collection and the new interval. Negative unit indices
still advance `previous`. Neither reordered feature indices nor whole-stroke
ordinals substitute for original range endpoints.

## Typed owners and copy boundary

`Math2Mode0RawOnlySource.PrepareColdWithSourcePositions` uses the actual cold
bundle policy. `PrepareWithPreparedSearchAndSourcePositions` explicitly supplies
a diagnostic search width. Both retain a private `Math2Mode0RawSourcePositions`
from the same shared feature preparation; there is no caller position matrix,
external source selection, existing C25 vector or warm-source entry. Shape,
finite coordinates, 1..256 sources and at most4096 raw points are checked before
cloning and NN preparation. These bounds are library safeguards.

The weak `MathFeatures` reference authenticates the exact initial preparation
before NN tensors are created. After binding, liveness depends on the private
original document and processed snapshot retained by the actual source, rather
than the feature wrapper staying alive. It does not accept another same-shaped
feature object. The probe forces GC after NN preparation before attachment and
again before reading attached ranges.

`Math2Mode0RawOnlyOutput.AttachSourcePositions` consumes the attachment stage
once and returns `Math2Mode0SourceBoundOutput`. The latter exposes internally:

- `Output`: the exact complete formatted output.
- `Document`: its immutable original raw document.
- `ReadRecordIdentity(record)`: the unchanged actual completed-record identity.
- `CopyRanges(record)`: detached original source-range arrays per actual unit.
- `ValidateForCopy(actualOutput)`: same-output, completed-attachment and live
  source checks. Old opaque-source output refuses attachment before consuming it.

The attachment and formatted-output classes are partial only to support a later
metadata-only first-copy callback under the same gate. No first-copy or DOM
implementation is included here. `Math2Mode0OutputRecord` keeps its existing
semantic shape; the typed source wrapper supplies the missing range collections.

## Payload budget and failure

The16MiB combined envelope is a payload bound, not an RSS measurement. It
includes the existing history/EF4/output reservation, original source XY,
processed packed positions, a conservative128 bytes per original source for
the document/map selected-row copies, references and separators/permutation,
two enclosing ranges (document/snapshot), per-record references, a conservative retained interval bound and peak
reader/union work. Shared existing payload is counted once. Metadata and the
combined budget are checked before claiming or allocating attachment payload.

Before-claim cancellation or budget rejection permits valid reuse. After claim,
allocation/callback/cancellation failure keeps its logical range-write prefix
and poisons the entire recording/output owner. Later accepted reads refuse;
there is no rollback. Detached prefix statistics remain diagnostic. Source
disposal invalidates attached-range reads, and existing libraries are released
exactly once. Source attachment neither repeats NN inference nor mutates record,
library, source lease, semantic units, score, map or run identities.

## Verification and reproduction

`LegacyMathSourcePositions.Probe` takes five arguments: model.ikrmath, raw
Math2 resource, official ONNX Runtime library, prepared position controls and an
output capture. Generate controls with the tracked
`tools/LegacyMathSourcePositions.Probe/reference_positions.py controls.json`.
Run `audit_output.py capture.json raw-resource [same-platform-other.json]
[old-structured.json]`. The auditor uses tracked stdlib/NumPy references only;
it can run with `python -I` from an unrelated working directory without ignored
research files. `reference_features.py` mechanically preserves the independent
research function bodies and records their original file hashes.

The probe covers20 genuine raw feature inputs, including reorder/ties at
19/20/21/22/41/42/43/256 sources, singleton and mixed geometry; 293 fractional
processed positions; 1280 independent processed interval reads; and4097 explicit
V18 scalar controls. Actual NN width1 and prepared-search widths3/16 produce20
retained records and40 source-range collections. Width3 reverses the original
input order while retaining original source identities. These widths do not
change the declared cold factory default.

Eight explicit prepared producer refusals cover foreign permutation, wrong
processing order, invalid point/fraction, incomplete production and duplicate
strict positions. Six injected attachment failures cover throw/cancel at claim,
after a later record's first run and before completion. The complete guard count
is83. The independent oracle derives features, packed positions, source order,
intervals and C21 unions without calling implementation reconstruction or using
expected transcription labels. Same-platform Managed/Native captures are
compared bytewise, without a universal cross-platform ORT hash requirement.

Old public position controls retain10152 pack/4096 canonical/68 snapshot/128
selection/27850 interval checks. All eight original feature fixtures remain
exact, and the old full EE4 output capture remains unchanged. The static auditor
`audit_math2_owned_source_positions.py report.json` checks5 additional PE regions,
34 instruction pins,5 runtime ownership chains and both preceding static
dependency audits. Vendor files are read as data only.

The checked source-aware first-result copy into a new library is described in
`legacy-math2-first-result-copy.md`. Remaining work includes outer aggregation,
source-ID transport and DOM publication. General inherited,
C25 and nonempty-cache source paths are outside this entry.
