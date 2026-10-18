# Math CB8 tag and selection ownership

This is a static checkpoint, not an implemented math DOM publisher. It closes
the temporary tag, late registration and stale-selection lifecycle beneath
the existing028070/027E20 contract. Vendor libraries are read as data only.

## Constructor and distinct ID domains

Registered CB8 metadata1ED890 points to descriptor1ED8D8, virtual table1ED830
and constructor interface1ED910. Its parameterized constructor032C00 requires
exactly40 bytes and non-null DOM and Ink owners. It retains those owners,
creates a separate BCC with parameter byte0=0 and the retained DOM name owner,
optionally clones/attaches the source subtree, then allocates its72-byte state.
That state starts its temporary counter at2. Old/new tag vectors initially
have count0; their capacities use the actual retained Ink registry size.

The028070 caller supplies its temporary BCC and a clone of the actual selected
Ink, with descriptor name `mathResult`, category102 and actual source context.
This is another Ink clone, not the original registry and not a textField
context inferred from similar row sizes. Source object IDs, INPUT IDs,
temporary CB8 IDs and registered math-selection IDs remain distinct domains.

## Temporary registration is not final Ink registration

CB8.V28=033560 allocates a40-byte pending entry, assigns the current temporary
ID and increments the counter immediately. It sets resolvedID=-1 and retains
the exact supplied name. A nonempty range constructs a C22 against the retained
Ink with the actual source pointer/count and copy flag0. The entry is linked
into the pending list even when that construction returns null, after which
the call returns-1. A failed range construction therefore consumes an ID and
leaves a cleanup prefix. It does not roll back the counter.

A count-zero entry has null C22 and can return its temporary ID. The symbol
caller027E20 only registers its nonempty-range branch and negates the returned
temporary ID for the DOM `tag`. Starting at2 avoids confusing a normal
placeholder with the numeric-attribute parser's -1 sentinel. These placeholders
are not raw stroke indices or final registered selections.

## Late resolution and failure prefixes

V40=0337E0 walks `.//*/@tag` through callback0336C0. The parser's invalid
sentinel is INT64_MAX. A valid nonnegative tag passes through. A negative tag
must match the negation of a pending entry's temporary ID. Missing entries
fail; an equal-looking range or convenient empty entry is not a replacement.

If that entry's resolvedID is still negative, the callback invokes the SAME
retained Ink.VE0=06BE30 with its stored name/C22 and constructor category102.
It stores the returned ID in the pending entry before checking success. A
nonnegative result is then written into the actual DOM tag attribute as typed
numeric value4. Multiple attributes referencing that placeholder reuse the
cached registered ID. If the DOM write fails after registration, that
registration and resolvedID remain; the method does not undo them.

CB8.V10=033040 builds an edit from its retained temporary BCC/root against the
supplied actual DOM, then uses032800 for old/new negative-tag matching. That
callback checks the exact retained Ink's range and name lookups, and native
range equality. A table of arbitrary caller names and ranges cannot stand in
for this owner. This audit hashes the full callback region; it does not claim
its complete edit/receiver integration has been ported.

## Old and new selection vectors

V48=033920 walks the same tag attributes into one of two ordered vectors.
Flagtrue chooses state+28, flagfalse chooses state+38. The actual028070 caller
collects existing tags before constructing the new math subtree, resolves the
new negative placeholders through V40, and collects the replacement tags with
flagfalse. Callback033870 appends each parsed ID and preserves duplicates;
these are not mathematical sets.

V50=0339D0 iterates the first vector in order. If an ID appears anywhere in the
second vector it is preserved; otherwise the SAME Ink.VE8=069630 removes it.
The ID remains in R8 at the indirect call even though the decompiler omits
that argument. Processing stops on the first failure. Duplicate old IDs are
not silently deduplicated, and a failed deletion cannot be reported as a
successful whole-vector rollback.

## Remaining owning join

The checked raw source/first-copy and single-source outer-copy stages supply
actual text intervals and typed source ranges. They do not supply the original
math field, selected C23/Ink clone, CB8 edit roots or native pending receiver.
Those must be retained through a math-specific scope before this registration
can become a genuine runtime publisher. The existing text kind4 target and
text category100 are not substitutions for the native math path.

The next scope must derive its raw source document from those actual owners,
keep the counter/resolution and DOM-write failure prefixes above, and separate
library-owned transport from native original-page history acceptance. No
application switch, confidence conversion or complete formula output follows
from this static checkpoint.

Run `python tools/handwriting/audit_math2_cb8_tag_owner.py REPORT`. It verifies
both PE hashes,10 full focused regions,43 instruction pins,13 registered
bindings and literal initial counter2. Complete focused decompiler bodies,
disassembly and SHA provenance are retained locally under
`research/myscript/output/math2-cb8-tag-owner-20261018/`, excluded from Git and
releases. No vendor DLL is loaded or executed.
