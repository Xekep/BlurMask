# Math2 first-result copy and lifetime contract

This is a static checkpoint, not a completed runtime result transport.
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
empty range, but later processing can populate it. The library's current
raw-only interval rows do not yet carry the C79 source mapping, so they cannot
establish that the corresponding native result range remains empty.

028A80 subsequently reads these first-copy rows at F0+98/A0. Its026780 helper
again uses the destination's adjacent pool and clones the source ranges.
Multiple recognition rows may be wrapped in table/row-separator symbols.
This is a second owning transport, not concatenation of alternative strings.

## Runtime and failure boundaries

An isolated draft demonstrates the metadata/string subset: three actual NN
sources, one repeated interned interval pair,20 failure/cancellation prefixes,
62 guards and positive/negative source-collection controls pass Managed and
NativeAOT. That draft is deliberately **not integrated as043AC0**: it omits
the actual symbol source ranges. Passing its lifetime tests does not close
this producer/transport gap.

The next join must carry original source indices and fractional point positions
through the actual preprocessing and C79 mapping, then retain and clone those
same typed ranges. A resampled segment ordinal is not an original ink-stroke
index. No guessed empty range or whole-stroke replacement is authorized by
the present proof.

Native043AC0 may fail after resetting the target or copying a prefix. Fresh
portable target poisoning/disposal is a cleanup adaptation; it does not prove
generic existing-target rollback or native allocator-failure order. Formula
acceptance, DOM publication, layout and recognition quality remain separate.
