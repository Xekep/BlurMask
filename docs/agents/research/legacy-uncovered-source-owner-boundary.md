# Uncovered input needs a geometric result owner

This static checkpoint does not implement `238B0` or extend the supported
document-coverage branch. The existing exact WORD/TEXT comparison remains
strict. No vendor SDK binary was loaded or executed.

`0248D0` traverses the actual WORD source tree and unions its original key
ranges. If the union differs from the TEXT key, it copies the TEXT ranges,
subtracts the WORD union and passes the resulting vector to `238B0`. Testing
containment instead of equality, changing the WORD key to the complete INPUT,
or silently discarding this vector would bypass actual native work.

The committed CHAR capture has eleven WORD clips equal to the full INPUT
root and two unequal clips, MIR and II. This is input-membership evidence;
the capture itself does not yet produce a TEXT receipt or a native 2DB owner.

## The next owner is not the existing source interval AVL

`238B0` creates type2DB and invokes its real `V18=050660` with the result2DA
and retained Ink. The source factory and Ink are different operands. The
runtime table is `276740`, linked through the actual type2DB descriptor.

`050660` chooses the actual selected document alternative, reads each WORD24
link and its chosen WORD48, then visits the actual CHAR24 links and chosen
CHAR40 records. It builds WORD64, CHAR80 and line48 index arrays. It retains
the source F8 guide rows, including angle, and computes the rotation used by
point lookup. These arrays are typed views of the selected result, not newly
recognized text or an independently fabricated second alternative.

With the geometry flag set, initialization calls both `04B4C0` and `04E480`.
Only after they succeed can the owner provide its geometric boundary arrays.
The existing source interval AVL and prepared source-line metrics do not
replace these missing boundary producers.

The real lookup methods include `V60=0519C0`, `V68=051AF0` and `V70=051BF0`.
They transform input coordinates with the retained guide rotation and use
strict, ordered comparisons against line and character boundaries. Their
returned line, character and WORD indices are separate domains. Deriving an
index from a raw bounding box or nearest glyph would invent their behavior.

## Source-object dispatch and mutations are also required

`238B0` builds a C23 owner over the retained Ink and walks the supplied packed
source ranges. It obtains actual source objects, distinguishes their dynamic
kind and contiguous versus chunked point access, and consults the 2DB index.
The C25 special path is distinct from the ordinary point paths; ordinary
library input is not permission to relabel every native object as C49.

The branch tracks candidate intervals, ordinary versus special CHAR flags,
and line membership before applying its updates. It invokes `234D0`, whose
source unions can call `339E0` and `36630` to update the actual CHAR and WORD
payloads. These helpers are not equivalent to appending the uncovered vector
to a local array. Mutable source payloads, selection ownership and failed
prefixes must survive in the same typed result owner.

No blanket rollback or guarantee that every remaining stroke becomes text is
inferred from these calls. Their actual lookup and mutation outcomes decide
which branch executes. Original document acceptance remains a later boundary.

## Recorded proof and next implementation boundary

The companion audit checks the pinned Text DLL hash, twenty-one complete
contiguous Ghidra function bodies, all recorded call/condition instructions
and seven descriptor/runtime-table bindings. The body TSV records Ghidra's
actual function extents rather than the first PE unwind fragment. Hashing and
complete linear instruction decoding cover every byte in each recorded body.
The exports were made headlessly from a read-only project; changes made only
in the analysis session were discarded.

A later implementation needs an owning nonempty 2DB index from the real global
WORD/TEXT/line result, actual current-source object dispatch, and typed source
mutation commands. Its budgets must cover all lookup boundaries, point visits,
payload clones and generated command prefixes before live mutation. It must
preserve the existing equality fast path and old configured-result captures.
This checkpoint authorizes no runtime accessor or owner coercion.

```text
python tools/handwriting/audit_uncovered_source_owner.py REPORT.json --capture GLOBAL_CHAR13.json
```
