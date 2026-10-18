# Cold Math2 EE4 structural formatter: native inventory

The ordinary owning output stage deliberately refuses any retained structural
rule. The next coherent join must normalize the **entire** actual completed
vector through EE4, then retain the same Source, Record, Library, unit/run and
alternative owners. A different ordinary winner cannot replace a structured
alternative. This document and its auditor are static proof before that runtime
join; they do not implement or accept a formula.

## Caller and concrete owner

The complete output-route proof already binds fresh request byte five to EE4,
the SAME source AL in formatter+110, and V18=`025980`. The restricted library
raw-only source supplies no existing expression or external eligibility map.
`046AE0` initializes its formatter mode byte and external map to zero; it only
derives a different map/mode when EED+230 is nonnull. That general configured
source route remains outside the library raw-only factory. A tensor feature
shape alone is not evidence that the native EEB selected only raw input.

`025980` is a parser/repair pass, followed by `0251E0` compaction and U16 map
construction, followed by a new `024A20` grammar check. A valid-tag identity
shortcut does not implement malformed-prefix repair, nested-step matching,
fence validation, table padding or the returned old-to-new map.

The native 14-rule order and type values are:

| Rule | Type | Parts |
|---|---:|---:|
| square root | 3 | 1 |
| fraction | 9 | 2 |
| slanted fraction | 10 | 2 |
| subscript | 4 | 1 |
| superscript | 5 | 1 |
| sub/sup | 11 | 2 |
| under | 12 | 2 |
| over | 13 | 2 |
| under/over | 16 | 3 |
| pre-sup | 6 | 1 |
| table | 14 | 3 |
| vertical operator | 15 | 3 |
| fence | 7 | 1 |
| U+2402/U+2403 control | 8 | 1 |

Names and label IDs come from the actual 72-byte PE rules and the fingerprinted
299-entry source alphabet. The decoder/output ordinary limit remains the
separate serialized 297 field. The control rule can therefore be tested with
explicit prepared syntax while its two control tokens are not claimed as actual
297-output decoder selections.

## Parser storage and helpers

`023FA0` opens a 64-byte frame. `02405F..02406A` explicitly clear all 64 bytes,
then `02406E` increments count. Its known semantic fields are:

| Offset | Meaning |
|---|---|
| 00 / 01 | signed rule ordinal / signed current step |
| 08 / 10 / 14 | owned U16 input-position vector / count / capacity |
| 18 | owned AST node reference |
| 20..22 | per-part presence bits |
| 23..25 | fence kind, first-body-seen and allowed-delimiter flags |
| 26 | table flag, type 14 |
| 28 / 2A | table row/column counters |
| 2C..32 | first/second content row-column pairs, initialized to -1 |
| 34 | maximum columns, initialized to -1 |
| 36 | table mismatch flag |
| 38 / 39 | vertical-operator flag and separator-seen flag |

Typed native references at +08/+18 are owners; they must not become fabricated
pointer bytes in a portable representation. `023D60` destroys a frame's owned
position vector before either tail count adjustment or shifting later frames.

The shared AST node producer `0234F0` also matters. On a successful nonnull node,
`023687` zeroes its complete eight bytes, `02368A` writes kind zero and `023690`
writes low U16 unit `FFFF`. Thus a new rule node, whose opener writes only kind,
has a known unit word `0000FFFF` when `023E40` examines it. An opening nested
rule used as a fence's first body is consequently not an ordinary delimiter.
Neither allocator zero-fill nor an invented unit zero is needed to establish
this branch. Actual ordinary leaves subsequently store their full unit word.

`023E40` updates the current frame and ancestors. Its presence predicate is
literally **AST kind 2 or 3**, or an already set presence bit. It is not the EF3
observable-token classifier used by beam EOS and attention masking. For a fence,
the first body unit is additionally checked against the constructor's delimiter
list. That list is `(`, U+27E8, U+2308, U+230A, `{`, `[`, `)`, U+27E9, U+2309,
U+230B, `}`, `]`, `|`, U+2016, retaining only labels actually present in the same
AL. There is no Unicode-category substitute.

`0241B0` matches a step against the current signed step and special table or
vertical state. `024310` handles a closer; when it fails against the top frame,
it searches preceding frames backward and records the native diagnostic instead
of silently treating that token as an ordinary leaf. Native missing-presence
exceptions are type 9 or the bits in mask `C408`, namely types 3, 10, 14 and 15.
These exceptions and the separate close/pop predicates must remain distinct.

`024780` checks rule openers first, then step/closer labels; otherwise it creates
an ordinary AST leaf and propagates its presence. `024900` pops the frame and
updates parent table content locations only under its native completion/presence
conditions. This work must preserve nested current-step identity.

## Repair, compaction and map order

`025980` calls `024EC0` during scanning and again at end of input. Recovery can
append a missing closer, or leave a frame unclosed so its recorded positions are
marked with low-U16 `FFFF` tombstones. Those stores preserve upper metadata bits.
It cannot be replaced by rejecting every malformed sequence or appending guessed
closing strings.

`024C60` pads table rows using the native maximum-column count. Insertions move
whole 32-bit units and adjust later entries in the U16 input-position map.
The map indexes input positions and current unit positions at different points;
rewriting them as logical table coordinates would change the algorithm.

`0251E0` consumes the original and repaired vectors. It copies full 32-bit units,
handles tombstones and the actual formatter special labels, and creates the
returned U16 map. The raw-only mode is zero; the other mode's substitutions are
a distinct caller policy. `025980` then clears its AST arena and invokes
`024A20` on the resulting units only if normalization succeeded. The final check
uses the same token/step/pop helpers, so reusing only the initial scanner is
insufficient.

The existing `046AE0` caller remaps every non-absent run index through this final
map, then runs the actual decimal rule and SAME-record `045C60` interval output.
Score and Source/Library references must remain attached to their original
record throughout; this stage does not re-rank or perform NN inference.

## Actual next test and owning join

The already captured actual width-sixteen completed vector includes
`M,<sub>,1,</sub>` and `M,<fence>,|,</fence>` alongside ordinary alternatives.
These labels are read only after inference and are not decoder feedback.
The new factory must process that whole retained vector. All 14 rules, nested
and wrong-ancestor closers, missing components, fence delimiters, table widths,
literal metadata and map changes need explicit prepared controls in addition
to those actual positive records.

Planned runtime files are a new `LegacyMath2Ee4Formatter` and a named full-format
owning entry, sharing the existing output owner/decimal/Library core. The old
`FormatOrdinary` domain and capture remain unchanged. Both named factories share
one final-token claim; callers cannot restart either facade after consuming
failure. The full factory still excludes existing-source byte-two units and
optional auxiliary alternatives rather than manufacturing those owners.

Preflight must reserve the existing history/EF4/final payload plus formatter
input/output/map, frames/positions, possible table insertion workspace, unit
rows and UTF8/key payload. Bounds are explicit portable payload safeguards, not
native allocator/RSS equivalence. Failures after claim preserve logical write
prefixes, poison the same source and expose no partial result. All libraries
remain available for exact-once DisposeAll; no rollback or native allocator
failure-order claim is made.

`audit_math2_ee4_formatter.py` hashes 13 complete parser/normalizer regions,
checks 45 semantic field/branch pins and all 70 callsites, and depends on the
existing complete output-route proof. It reads the actual delimiter literals
and resource bindings and checks 14 frame initialization and 30 presence
controls. It executes no vendor code. The runtime join and independent full
procedural normalization oracle remain the next work.
