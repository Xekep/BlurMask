# Temporary glyph-chain cleanup

Internal `LegacyNativeGlyphRecordPool.RemoveCreatedChainSegment` restores
274.V40/13B1D0 for the existing no-geometry pool.219580 calls this operation
after an emitted chain matches an existing WORD and a new-reference boundary
was reported. It is separate from13F080, which frees a WORD's own vectors and
does not destroy274 glyph records.

The operation follows the actual inclusive linked segment from the last boundary
to the first, reverses its links and traverses the reverse chain. A row is removed
only if its ordinal is currently the last ordinal of its source owner. Native
capacity, descriptor-dedup state and owner nodes survive. Non-tail rows retain
their changed links and bytes; no compaction, renumbering or assumed reclamation
of every temporary record is introduced.

The portable boundary preflights references, the4096-record work limit and
absence of native geometry pointer18 before mutation. Invalid, cyclic or
unsupported input and entry cancellation leave the pool unchanged. The linked
mutation has no callbacks or cancellation points. The owning219580 caller must
provide the actual218560-reported boundaries; it must not pass an arbitrary
published WORD chain or report the entire source pass complete.

1024 deterministic pool controls cover shared source owners, flag56, linked
subsegments and retained non-tail records. An independent byte-addressed oracle
checks2814 removed rows,4059 retained non-tail visits and561120 remaining record
bytes, including owner count/flags and retained empty nodes. Additional guards
exercise cycles, geometry refusal, cancellation and reusing ordinal0 after an
owner becomes empty. Managed/NativeAOT captures are compared byte for byte.
A static audit pins the complete13B1D0 region,14 instructions and274.V40 binding.
The library does not execute vendor DLLs.
