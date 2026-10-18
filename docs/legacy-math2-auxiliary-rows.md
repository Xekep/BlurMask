# Prepared Math2 auxiliary rows

The internal041EC0 adapter inserts auxiliary blocks for the explicit initial
insertion mask. A nonzero mask entry inserts `width` rows at `unitIndex*width`,
preserving the existing tail. The first new row contains the output unit's
low U16 label, zero padding and score bits00000000. Remaining new rows contain
symbol0 and score bitsFF7FFFFF (negative FMAX).

Their middle U16 padding is **unknown**. Native041EDB writes only the low word
of the stack template,041EEB writes its score,041F40 reads the entire QWORD,
and042088 copies that template. There is no defining explicit write to the
padding word in this function. It must not be inferred to be zero from the
earlier destination memset. The portable row keeps nullable padding; a complete
native image is refused while any row has unknown padding. Existing known or
unknown padding and all score bits are preserved. This is a semantic field
representation, not an allocator or native stack image.

Prepared inputs must have consistent initial blocks: one block per uninserted
unit. Width0 returns an empty block sequence; the named library bounds are
width0..128 and at most1000 units. Those limits are not recovered native limits
or process RSS. Inputs and inspection results are copied. Cancellation produces
no accepted partial result; native allocation-failure prefixes are not claimed.

An independent block-composition oracle checks2048 cases,7222 inserted blocks,
30078 unknown padding fields,355 fully defined images and7 guards. Production
uses incremental insertion; the oracle composes final blocks directly. Managed
and fresh NativeAOT captures match SHA256
`00343d98aa89820f6e6a1577cde21cbe2173df0429b2c66de00eec301e66a021`.
The static auditor checks the complete041EC0 body,24 instruction pins and every
explicit fixed-frame stack write against the template padding interval without
executing vendor code.

Actual auxiliary98/A0 source ownership and full general EF4 integration remain
separate. This prepared component does not provide a new formula result.
