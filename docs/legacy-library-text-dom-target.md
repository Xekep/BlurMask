# Explicit fresh library DOM target

This read-only contract supports a new, explicitly library-owned empty DOM
root. It does not reconstruct an SDK textField, page, request parent or an
accepted C20 result. The concrete MAIN request/BE2 contract remains in
[the transport map](legacy-main-document-be2-transport.md).

## Constructor-owned root and names

Engine BCC parameterized constructor0EC260 requires16 bytes and forwards
byte0 plus the name-owner qword at8 to0F1E40. With byte0 zero and a null
name owner,0F1E40 creates a fresh BEE owner and a0x150-byte arena/root block.
It assigns kind4 at0F1F5C, null parent8 at0F1F84 and empty first/last-child
lanes20/28 at0F1F9C/0F1FA4. The reader's V20 getter,0x18003EFB0, returns
that exact BCC+18 root. It does not create a textField element.

The constructor clears observer storageD0/countD8 and exclusion storageF8/
count100. Its final byte0 check skips default BE2 observer creation. This
is a constructor-owned no-history domain, not an inference from absence of
previous recognition text or a claim about an arbitrary external DOM.

A separate writer must share this target's exact BEE owner. The actual
05F200 producer passes reader+10 as the writer constructor's name-owner
argument and passes zero as its history byte. Recreating names in a second
unrelated pool would lose this identity contract. Existing detached builds
create their own name owner and must retain that API's separate scope.

## Kind4 is a valid insertion target

DOM.VA8=0EAAF0 writes child.parent8, parent.firstChild20 and
parent.lastChild28. These lanes exist in the constructor's kind4 root.
There is no kind3-only parent test in that operation. Root attachment puts
the mutation in the root observer domain; the native observer loops are
empty because this constructor actually established countD8=0.

061510 reads the target's firstChild20. On this precise new constructor
root it is null, so the helper skips the old-textResult removal and retains
insertion ordinal0. It derives neither outcome from source-prior being
null. An existing parent, including one with only non-textResult children,
requires the real scan/ordinal policy and is outside this fresh-root API.

BE2.V50=0E35C0 exports an empty path when action.parent is exactly its
target argument:0E38E1/0E38E4 bypass the ancestor count loop and0E3947/
0E394A bypass path insertion. No synthetic root name or root ordinal is
added. The exported insertion still retains its real child ordinal0.

BE2.V10=0E2D70 resolves a zero-length path to the exact supplied parent.
At0E2F59 it accepts child traversal for parent.kind>=3, including4. Empty
root.firstChild20 leaves the selected child null and0E2FDE chooses VA8,
rather than insert-before VB8. Thus the kind4 constructor root is valid
for the native root-target/empty-path/ordinal0 branch. No element name
test or SDK textField identity is invented by this branch.

## Recursive payload copies and failure limits

The original031880 result, V50's exported payload and V10's applied payload
are distinct node identities. V50 retains the supplied writer and invokes
writer.V88(recursive=true) to clone the subtree. Apply invokes the target
reader's V88(recursive=true) again, then appends that new payload. Passing
the exported node directly to target.Append would alias/move native state
instead of restoring this algorithm.

V88=0EBE40 handles the actual supported node kinds0/1/3. Element names and
attribute names are resolved against the destination's BEE owner. The
element branch creates a new element, copies ordered attributes through
0EB9C0/0EB3B0, recursively copies each child in order and appends it through
VA8. A failure while cloning an element releases its already cloned prefix.
The kind1 literal-text copy has its separate bare-release failure rule.

0EB9C0 preserves value tags and assigned numeric lanes, and copies string
payloads using their native storage representation. A portable logical tree
must retain actual literal bytes and numeric raw lanes; it must not claim
that its byte arrays reproduce native pointers or unknown inline padding.
No XML parsing or serialization belongs to this copy operation.

The native insertion apply branch does not test V88's return before calling
Append. A null clone would therefore reach an invalid native operation.
A bounded managed implementation must explicitly refuse that prepared
fault boundary, retaining its diagnostic prefix, instead of claiming a
successful rollback or an invented native false return.

DOM.V218=0EFB10 obtains the current BE2 observer through0EBD50 before calling
V10. The new parameterized target has an actually empty observer array, so
that lookup returns null and no observer history transaction is started.
This conclusion does not cover the C20/C4D observer chain or05F720's outer
commit/rollback. Those remain separate from the library DOM edit target.

## Proposed narrow API checkpoint

The next runtime can add `LegacyLibraryTextDomTarget.CreateFresh(seed)` with
private constructor ownership, retained exact Seed/Document, one BEE pool
and its constructor-created kind4 root. This public name must explicitly
say that it creates a new library target; it accepts no SDK parent pointer,
caller-supplied no-history flag, invented tag or replacement registry.

A named `LegacyFreshMainDetachedTextDom.PrepareForTarget(actualSources,
actualTarget)` can create a separate no-history writer sharing that exact
BEE owner. The existing `Prepare(actualSources)` API and captures stay
unchanged. The result must retain the exact target identity so a later
insertion cannot consume an unrelated detached result, even if it belongs
to the same raw document or has equal tree contents.

An owned fresh insertion can then implement V60/V50's opcode2, empty path,
ordinal0 and recursive payload export, followed by the separate V218/V10
recursive copy/apply to that same target. It must retain deep immutable
before/after snapshots, node/name identity sidecars, single-use leases and
actual failure prefixes. Prepared faults remain internal controls.

This API would be an explicit fresh-root portability adaptation. It would
not assert native05F720 entry registration, actual request-reader lineage,
SDK parent identity, C8F result-cache transfer, C20 acceptance or complete
0283B0. No incoming binding of05F720 was found in the current static direct
call/pointer scan; its body proves a caller contract, not public invocation.
Runtime implementation waits for review of this scoped target contract.

`tools/handwriting/audit_library_text_dom_target.py` checks both complete PE
hashes,12 regions,6 virtual bindings and52 instruction pins for the root,
names, path, kind4 apply branch and recursive copies. It is static only.
