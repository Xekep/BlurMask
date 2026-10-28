# Math2 completed-record ownership

This is a static contract for Document037430, not a completed-record runtime or
an accepted formula. `tools/handwriting/audit_math2_completed_record_ownership.py`
checks8 exact PE regions,47 instruction pins and3 descriptor bindings. It reads
Document and Engine bytes; no vendor binary is loaded or executed.

## Constructor and lifetime

034EA0 grows a vector of168-byte records to count+1 through the allocator's
reallocation service. A failed growth returns false without changing its pointer,
capacity or count. Successful growth updates pointer/capacity before constructing
the next slot. The constructor zeros the recorded scalar/vector fields, writes
I32 -1 at60, and constructs a separate owned type3E object at88. Only a successful
3E construction increments the vector count. The Engine descriptor at23CF80 binds
type3E to its concrete primary table; its constructor behavior is not emulated.

The constructor does **not** initialize bytes04..07 or64..67. These are padding,
not zero-valued semantic fields. The auditor derives this coverage directly from
the constructor's stores. A future managed projection must preserve prepared
opaque bytes or avoid claiming pointer/padding identity with a zeroed array.

034D10 releases the record's U16 buffers, nested110-byte tensor payloads, auxiliary
objects and the separately owned3E handle.035000 destroys removed records before
shifting retained complete168-byte records and updating count. Neither the sorter
nor a scalar score alone implements these ownership operations.

## Content written by037430

The selected raw48 row supplies a signed parent ordinal at28. The recorder writes
that ordinal to completed60 and writes its eighth argument to the opaque qword28.
It computes the existing checked normalized F32 score at00. It does not append the
raw EOS symbol to the copied prefix.

| Completed offset | Actual value source |
| --- | --- |
|08 /10 /14| U16 buffer, count and capacity copied from parameter4's selected parent tuple |
|18 /20 /24| Separate U16 buffer, count and capacity appended from parameter5's selected parent tuple |
|40 /48 /4C| Optional U16 sidecar buffer; copying iterates the parameter4 prefix count |
|60| Selected raw48 parent ordinal |
|88| Separately constructed type3E owner |
|90| Optional8-byte allocation only when EEE diagnostics90 is nonnull |

The tuple table stride is16 bytes. Prefix and optional sidecar capacity reserves
precede element copies. Their fallible helper033D60 grows a U16 buffer by
max(required,trunc(oldCapacity*1.5)). Individual count increments occur after each
element write. The separate033DE0 append helper reserves for oldCount+sourceCount,
copies or zero-fills two-byte elements, then increments the destination count.
The optional sidecar producer must establish the compatible length; the recorder
itself is not evidence that arbitrary prepared sidecars are safe.

## Failure is not rollback

After034EA0 succeeds, the new record already counts as an element. Later failure
of the diagnostic allocation, prefix reserve, optional sidecar reserve or copying
returns false without removing the record. In the common content epilogue,
026A80 releases nested tensor payloads referenced by30/count38, then38 is set to0.
It does not free the whole record, erase its copied prefix, or decrement the vector
count. A failure before this epilogue follows its distinct early return.

The actual outer caller must stop on false and eventually destroy retained
records. A managed all-or-nothing result would change the native logical prefix.
Physical allocation and pointer identity will remain explicit adaptations even
when the ordinary owned data path is ported.

## Remaining joins

The [numeric EOS handoff](legacy-math2-eos-handoff.md) currently records numeric
score events only. The [actual tensor state](legacy-math2-mode0-tensor-state.md)
retains decoder state but still takes prepared prefix and metadata sidecars.
The actual outer prefix/sidecar producers, complete type3E use, diagnostics,
maximum-step fallback and completed-record ownership must be connected before
calling this a restored decoder. Final structural acceptance remains separate.
