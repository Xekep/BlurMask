# Actual Math2 processor order and raw C79 mapping prerequisites

This static checkpoint closes the order and empty-cache prerequisites for the
source-position proposal. It executes neither vendor code nor a recognition
model. The coordinate controls are prepared scalar/position cases, not claimed
NN-generated glyph ranges.

## Exact resource order, not a selected example

The fingerprinted raw resource `2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a`
serializes this exact `input_processing_pipeline.preprocessor_array`:

1. `stroke_reorder_cogx` with no configured fields;
2. `scaler_group_extrema` with `quantil=0.5`;
3. `normalizer` with no configured fields;
4. `resampler` with `has_interstrokes=false`, `resampling=0.4`, `version=1`.

Its feature scheme is exactly `symbols, math7, knn6Prox`. This is decoded from
resource bytes and checked structurally, rather than inferred from a tensor name
or a successful sample.

Ink `013460` visits the JSON array by increasing index (`01386A`, `0139D8`).
Its callback `013B50` constructs the processor and appends it at the prior count
(`013C20/013C36/013C39`). The registry maps these names to C7F, C71, C73 and C72;
their concrete V10 targets are `01EEF0`, `01C970`, `015520`, and `017060`.
C6D `014BE0` walks the same B8 array in increasing order and supplies the same
ink and C79 owners (`014E59..014E89`). It does not sort the processor array.

`01EEF0` obtains the actual original stroke point vector through its V88/V90
paths. It sums X in ordered F32 additions, divides by the original point count,
and stores the F32 mean at its 16-byte work row's offset4 (`01F190..01F240`).
Resampling has not run yet. Thus original-mean ordering before normalization
and resampling is correct for this raw profile. Post-resample mean is a different
policy and would produce a real discriminator on nonuniform point sampling.

There is a separate existing implementation discrepancy: native `01ED50` uses
stable insertion for at most20 rows, then a quick partition with actual equal-key
swaps for21 or more. A permanently stable host OrderBy is not equivalent at larger
counts. The source-aware route must consume the separately verified literal
sorter; until that join exists, a bound of20 before inference is sufficient and
must be explicit. This finding is not a reason to silently retain wrong ordering
for large inputs merely to preserve a capture.

## The supported raw cache stays empty

C79 V8=`00F130` resets its source-position/range tree and explicitly destroys and
zeros cache pointer98, count/capacityA0 and countA8 (`00F1AB/00F1B6/00F1C0`).
The current proposed entry has an actual new raw document, complete pen-stroke
selections and no C25 known-text objects or inherited C79 owner.

The fixed pipeline's C79 mutations are bounded:

- reorder V20=`010240` gathers position50, separator60 and source-kind30 arrays,
  rebuilds its position lookup tree70 through `00EDA0`, and invokes the validator;
- resample V18=`00FB30` replaces position50 and separator60 arrays and invokes
  the validator. It does not populate cache98;
- scaler `01C970` and normalizer `015520` do not use their C79 argument;
- the actual host call supplies C6D argument5=true (`047F38`) and argument6=false
  (`047EF1/047F33`). The former bypasses C79 V58's known-source import, while the
  latter takes the source-marker clearing branch instead of unchopping.

`00F7E0` only validates these fields; it does not produce a cache. Therefore the
`010850` empty-cache branch is justified for this new fixed raw-source owner.
This is not a license to apply the same reader to inherited, known-text or
externally supplied C79 state. Such owners remain refused.

## Literal coordinate composition

Source position is an unsigned packed64 value, not just a floating point index:
original source ID occupies the high DWORD; point, fraction and mode occupy the
low DWORD. Reordering moves whole position segments without replacing those IDs.
The resampler emits a temporary coordinate in the preprocessed input's domain;
C79 V18 composes it through the prior positions.

For temporary fraction0 orFF, V18 copies the entire prior position, including
its source ID and mode. For an interior fraction it reads adjacent prior packed
positions, treats prior FF as fractional zero, and evaluates this exact F32 chain:

```text
left  = F32(leftPoint + F32(leftFraction * .005f))
right = F32(rightPoint + F32(rightFraction * .005f))
x = F32(F32(F32(F32(right-left) * temporaryFraction) * .005f) + left)
point = floor(x)
fractionByte = byte(int(F32(F32(F32(x-F32(point)) + .0025f) * 200f)))
```

The store preserves the prior source ID and mode. It does not carry a resulting
fraction200 into the point index. Substituting `LegacyNativeInkPosition.Pack`
would change both the rounding expression and the carry behavior. The independent
controls contain23 float-neighbor rounding-order discriminators and12 resulting
fraction200 cases, including the literal packed result `000000000000C803`.
A one-point input preserves FF3, and a fractional first segment preserves mode3.
Neither can be replaced by an unconditional mode5 endpoint.

The independent standard-library oracle supplies4097 composition cases,
3400 rounding-neighbor cases and64 processed snapshots with20729 native interval
queries. Forty-eight snapshots change source order. It composes packed positions
and then uses the previously recovered separate `010850/00EBE0` expression; it
never invokes the C# implementation. These controls will become prerequisites
for the later managed source producer, rather than expected-label feedback.

## Narrow runtime ownership proposal

The next implementation should use the same raw source clone for both feature
production and original source IDs, then retain the processed position vector
at the shared core's actual writes. It must preserve the current feature payload
for inputs where its numeric operations already match native behavior, and use
the verified literal source sorter for the larger-count discrepancy above.

The proposed existing-file seams are:

- `MathPreprocessor`: shared core with an opt-in private coordinate sink; old
  `Process` still returns its original record shape. New source-aware output
  includes the actual original-to-sorted identity and resampler coordinates.
- `Math2Mode0RawOnlySource`: named source-aware preparation, retaining the exact
  original-document/processed-position owner beside the same NN loop. No new NN
  pass or caller-supplied map is accepted.
- `LegacyNativeInkPositionSnapshot`: an internal owned-processed constructor,
  guarded by the source-bound factory and copied fields, reusing its verified
  empty-cache `ReadRange` consumer. Existing public fresh creation is unchanged.
  Any assembly visibility seam is internal and explicitly scoped to Math.

New math source-position and source-bound-output owners hold private immutable
arrays and source/feature/output identities. The output adapter validates the
same Source/Record/Library/AL and complete output under the existing output gate,
preflights its full additional payload, and resolves the actual mapped runs.
Each nonnegative mapped unit receives the original-document C21 VB0 union of
its source intervals. Negative indices still advance the previous endpoint.
Old semantic output-row shape and opaque-range APIs stay unchanged; a new named
first-copy entry can consume the typed source-bound wrapper afterward.

The original document should reuse the already restored C21 union rather than
introduce a second geometric union. Its current bounded source document can be
used by a narrowly visible internal factory; any4096-point source budget is a
named library safeguard, not a native limit. Unsupported C25/external-page,
nonempty-cache and inherited-source branches must refuse before NN preparation.

Before any new output copy, the owner must check source identity, liveness,
metadata extents and the combined payload budget. Consuming cancellation/fault
poisons the source and preserves logical attachment prefixes; no partial accepted
result or retry is exposed. The independent runtime audit must derive complete
range bytes from actual captured packed positions and real mapped runs, including
reorder, fractional endpoints, repeated/overlapping intervals and source crossings.

`audit_math2_source_mapping_producer.py report.json` verifies12 complete additional
regions,49 instruction pins, all11 registry entries and four constructor/primary/
V10 chains, runs the preceding source-transport proof, and generates the prepared
coordinate controls. It loads only PE/resource bytes. Source DOM tags, freeze
state, external edits and layout transport remain subsequent joins.
