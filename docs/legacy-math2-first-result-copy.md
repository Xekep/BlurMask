# Math2 first-result copy and lifetime contract

The fresh raw-only first-result copy is implemented and checked. Outer result
aggregation, DOM publication and formula acceptance are separate unfinished joins.
`audit_math2_first_result_copy.py REPORT` checks eight complete native regions,
79 instructions and the registered EED20 service. Vendor DLLs are read as data.

## The first target is inside the recognition row

029290 constructs48-byte work items and schedules027590. That worker reads
the source from item00 and the recognition row from item08, then passes them
as the third and fourth arguments to EED20=047C50. The recognition row has
strideF0; it is not the local completed-hypothesis vector's168-byte element.

047C50 retains the fourth argument at rsp60. After successful formatting it
copies the first local completed record to that row plus20, through043AC0.
026F00 initializes the enclosing row: record score00 starts0 and parent60
starts-1, not0. The caller tests the actual return byte before swapping the
separate external full-record vector. Ghidra's inferred void return for043AC0
does not override that instruction-level return contract.

## A new string pool and copied source ranges

043AC0 resets target counts, destroys old symbol alternatives and ranges,
releases the old Library88 and creates a new3E library. It copies score00,
parent60, primary/secondary U16 vectors, frontier28, optional tensors,
coordinates40, full U32 units50, eight-byte runs68 and symbol rows78.
Optional90 is another distinct branch. Neither the old-to-new formatter map
nor raw EOS/ancestry storage is part of this copy.

043740 receives the target vector at record78. Its adjacent vector+10 is
therefore the **new Library88**. Each nonempty source UTF8 interval is interned
through that library's V10. Length and scalar bits are separate fields in each
16-byte alternative; row flags20 and metadata24 are copied separately. Merely
copying the old text reference would retain the wrong pool lifetime.

After copying a row's alternatives,043740 clones its source range through the
global range service V88, using the source row00 pointer and08 count. These
range fields are not optional padding. The fresh row initializer creates an
empty range, but later processing populates it. The named source-aware factory
now retains the actual original document and processed C79 positions through
the same feature preparation. Full EE4 output attaches mapped-run intervals
through C21 union. The first-copy entry accepts only that exact completed
attachment owner, and clones its real per-unit ranges after alternatives.

028A80 subsequently reads these first-copy rows at F0+98/A0. Its026780 helper
again uses the destination's adjacent pool and clones the source ranges.
Multiple recognition rows may be wrapped in table/row-separator symbols.
This is a second owning transport, not concatenation of alternative strings.

## Runtime and failure boundaries

`Math2Mode0FirstResultCopy.Create` consumes the registered source attachment's
actual first retained result, while holding the same decoder/output gate. It
creates an independent pool, clones primitive vectors and full row fields,
then clones each row's typed source ranges. It retains the immutable original
document for the range-ID domain, but no decoder, source record, model, history,
event lease or old string pool. The result survives decoder disposal and GC.
Its own disposal releases its pool exactly once; caller inspection cannot
mutate retained arrays or ranges. A second copy claim is refused.

The combined logical payload envelope is checked before claim or payload copy:
source attachment reservation +96 bytes for fresh record/library +4 per
primary element for both U16 prefixes +68 per unit for U32/row/alternative/
range-vector storage +8 per run +16 per copied range +unique UTF8 pool payload
and peak interning work (5*length+1 retained, 5*length peak). This conservative
16 MiB envelope excludes CLR headers and caller inspection; it is not RSS.

Managed and fresh Native AOT captures agree byte-for-byte for four actual
inputs: cold width1, explicitly prepared widths3/16 and repeated M/M ink.
Width3 reverses physical stroke order, proving that source IDs are not mapped
segment ordinals. Independent tracked Python functions reconstruct native
MeanX ordering, resampling positions, C21 range unions and resource AL bytes.
The oracle checks new-pool aliases, exact budgets,22 failure/cancellation
prefixes and80 guards. Positive and negative GC controls discriminate a
detached copy from a retained source-backed result. No expected text labels
or vendor execution are used. The oracle runs with Python isolated mode.

Run `LegacyMathFirstResultCopy.Probe PACK RES OFFICIAL_ORT CAPTURE`, then
`python -I tools/LegacyMathFirstResultCopy.Probe/audit_output.py CAPTURE RES
SOURCE_POSITION_CAPTURE [OTHER_CAPTURE]`. The source-position capture comes
from the separately checked `LegacyMathSourcePositions.Probe`.

The supported input is fresh raw-only, no C25/inherited cache or auxiliary
tensor/coordinate/optional90 branch. One actual first result is copied; retained
search alternatives are not multiple F0 source groups. The outer new-pool
copy and math-specific source/DOM registration remain unimplemented here.

Native043AC0 may fail after resetting the target or copying a prefix. Fresh
portable target poisoning/disposal is a cleanup adaptation; it does not prove
generic existing-target rollback or native allocator-failure order. Formula
acceptance, DOM publication, layout and recognition quality remain separate.
