# Registered patch acceptance and original history ownership

This static checkpoint continues the
[registered C8F callback proof](legacy-registered-text-patch-apply.md). It adds
no acceptance runtime. The library selected-registry replay and library DOM
application remain different operations from the native C81/CBB page receiver.

## Original and selected source are distinct required owners

CBB.V78=1105E0 ensures and returns its actual inner38 Ink/C20 and inner30 DOM.
Registered00D540 obtains that original pair, then creates the two worker-selected
copies through C20.V170. Last argument1 suppresses their history attachment.
The original's0638A0/0648A0/0788D0 chain instead retains its own history C4D
and writes the exact original C20 into C4D.F8. Those are separate instances,
not two names for the same mutable source.

115800 loads the original inner38/30 **before** its own1101A0 call. The real
00F110 caller's preceding V78 preparation matters; a constructor with null
fields is insufficient evidence of usable owners. The library selected context
cannot replace that original receiver even for its nonnull-command preflight:

1. C20.V180=Ink06EC60 reads C20+58.
2. For nonnull incoming C4D it calls C4D.service20=07E2E0.
3.07E2E0 is exactly `mov rax,[rdx+F8]; ret`: a **target getter**, not a
   command-list getter, clone or history reset.
4. The returned original C20 becomes the target of C4D.service30=077850.

A selected clone's null58 cannot be passed into that getter. The null-incoming
branch of06EC60 is defined separately and does not apply to a real nonempty
pending transaction. The already restored library replay legitimately executes
the different, history-disabled V188 path without calling V180; it does not
provide this original-history receiver.

## Actual acceptance sequence and transaction boundaries

The native chain is C8F.V10 -> resolved parent.V70 -> C8E00F110 -> actual
receiver.V110=115800. Source-name/revision lookup and the incoming40-byte rows
are retained from the registered worker; neither an INPUT ID nor a library UUID
supplies them.00F110 passes field-state98 as the current row version.

115800 first resolves the actual source in its DOM and begins CB7.V20 with
`targets=NULL,count=0,lastByte=1` (11591E/21/24).13BCE0 increments nesting1B4;
the last byte prevents setting1B0.13B9C0's count0 branch returns true. This is
only the entry branch, not a proof that the whole CB7 transaction is a no-op.

The original page factory installs the same live CB7 on both sources:
10FB80 calls Ink.V18 with CB7,0x78,1 before retaining the Ink in inner38;
10FD60 calls DOM.V10 with CB7,0,1 before retaining inner30. CBB11AF60 also
registers its wrapper with CB7.V10=136980.136980 checks actual page-listener
interfaces C9D/E79 and retains their callbacks. Mutations can therefore populate
the CB7 pending watcher lists even when its entry target list was empty.

Each row's Before00 is compared with the evolving current version. Only an
exact match enters preflight; a later version stops the loop, while an earlier
nonempty row records a skipped-payload condition that affects later processing.
The loop reads the actual BE2 at10 and C4D at20, advancing40 bytes. The registered
row/version stream must be restored before a library -1/-1 transport row can be
treated as an applicable native row. Constructor-zero field-state values alone
do not prove the runtime version.

For a matching row, source Ink.V180 and DOM.V210 must both execute successfully
and separately report compatibility. A successful function return with a false
compatibility byte is not application. The mutation path then calls Ink.V188
with the actual C4D and DOM.V218 with the edit and native range/ID mapping output.
DOM.V210=Engine0EFA70 invokes BE2.service8=0E2870, which checks the edit against
a cloned tree. DOM.V218=0EFB10 invokes BE2.service10=0E2D70, which performs the
edits. These two calls must not be conflated.

The tail calls CB7.V28=13C500 with argument0. At nesting0 it may call13B530,
then13BD90 and13BFD0, and clears1C0/1D0 even when one of those finalizers fails.
Those routines iterate actual pending page/listener collections. The outer
history service then commits through global28.V80 only for successful result
and successful post-validation; failure or that validation condition calls
global28.V88. This is native rollback/history work, not the library's current
poison-after-partial-mutation adaptation. Only a successful receiver path lets
00F110 clear the actual C8F rows and continue revision/event bookkeeping.

## Smallest proposed owning library join

The next bounded algorithm that can use existing raw data is the ordinary
operation4/7 compatibility portion of06EC60/077850, tied to the exact incoming
transaction and original INPUT parent of the adopted scope. It needs a checked
original-history-target identity derived from the actual constructor/F8 chain;
it must not reuse the selected replay as an original target.

With the caller's optional mapping arguments null, the ordinary branch copies
the original C21 source ranges and validates each nonempty operation4 C22
through C21.V120=081D90; operation7 does not change compatibility. V120 itself
returns false for an empty request, whereas077850 skips that call for an empty
operation4 range vector. Existing raw range operations implement its underlying
unsigned binary search and FF-fraction handling. Actual same-document INPUT,
raw-object presence and pending command provenance are available; original
history identity transport must be represented explicitly before this is an
owning API. Allocation success and compatibility must remain separate outputs.

A proposed new service would consume the document's retained replay/transaction,
retain the actual original parent and produce a typed preflight result before
any registry or DOM writes. It would not provide a caller success flag, a page
revision, a CB7 replacement or a CBB facade. Before a full acceptance caller can
consume it, the real page/store/history owner, registered row versions, actual
CB7 mutations/finalizers and input-tag post-validation must still be restored.
The current library target's empty kind4 root cannot supply these dependencies.

`audit_registered_text_patch_history.py` pins the three PE hashes, concrete
interfaces, original-F8 preflight and CB7 entry/exit instructions. No vendor
binary is loaded or executed. This file is an ownership and algorithm contract,
not a claim that all of115800 or native page acceptance has been ported.
