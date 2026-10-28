# Real page accessor and request history context

This is the next static owner checkpoint after the cache transport. It
proves a concrete native C81/CBB page accessor that can produce a BCD/BEF
context shape compatible with05F720. It does not prove05F720's registration,
an actual caller assembling its full request, or the library selected INPUT
owning an SDK page. No vendor code is executed.

C81 table209300.V80 and CBB table208F90.V80 both resolve to113E30. That
getter ensures the page's stored DOM reader at inner10+30 exists, then calls
1106F0. The latter resolves an actual existing parent through110660: it
starts at the DOM root and looks up the native `page` element followed
by its `layout` child. A fresh empty kind4 library root does not automatically
supply that existing parent. A fabricated layout node is not added here.

1106F0 constructs the following real wrappers:

| Value | Exact native producer |
| --- | --- |
| underlying DOM reader | input wrapper10 -> inner30, stored at RSP20 |
| context | original CBB, or a new CBB retaining the original page wrapper; stored at RSP28 |
| BEF | parameterized constructor receives16 bytes at RSP20, so its second qword is that exact CBB |
| BCD | receives the new BEF and actual parent from110660 |

The code therefore closes a genuine page.V80 -> BCD.reader -> BEF.context
producer. Engine BEF.V200=00C0A0 returns its retained28, so this route yields
the actual CBB wrapper. It does not justify coercing a raw source ID, selected
C22, pending C4D, library target or arbitrary callback into that context.
It also does not establish that05F720's request.qword0 came from this getter:
that final caller/registration link is still missing.

The history relationships are distinct. CBB's A8 query11B310 routes IDA8
to interface table208F70; its getter115000 returns wrapper10 -> inner88.
C81's A8 interface uses the same getter. C81 constructor1114C0 creates the
actual3D owner at88 as part of its page/store lifecycle. Thus05F720's outer
generic history begin/commit/rollback token would be this actual owner if
its receiver came from this page route. It is not the raw source C4D list
or CB7 merely because all three participate in the surrounding operation.

Separately, C81 constructor1114C0 creates a parameterized CB7 with the
actual C81 pointer, stores it at innerB8 and registers it with that page's
context service. Descriptor20AD70 -> metadata20B2D8 -> constructor20B170+10
binds the CB7 constructor1363A0. It stores the originating page pointer at90,
initializes nesting counter1B4 and state byte1B0 to zero, and creates its
other required buffers. These are actual constructor writes in a real page
factory, not evidence that buffers remain empty after DOM/source callbacks.

CBB.V178=116970 and V180=116990 forward through that exact innerB8 to
CB7.V20=13BCE0 and V28=13C500. This explains the call ABI seen in05F720:
begin gets a zero-length input array plus flagtrue; end gets flagfalse.
13BCE0 increments counter1B4 and processes each supplied item before calling
13B9C0; a zero input count makes13B9C0 succeed without its DOM scan. The
counter increment remains a real operation, not a no-history return.

13C500 optionally invokes13B530, decrements the nesting counter, and at
the outermost level calls13BD90 then13BFD0. Both consume real retained
page/object vectors and can call the page interfaces or fail. It clears
the pending1C0/1D0 counts along the observed outermost success/failure paths.
Nothing in the current library DOM edit proves those vectors are empty or
that the page's source/observer callbacks did not populate them. End cannot
be replaced with constant success or skipped after a successful DOM Apply.

The remaining joins are therefore concrete: the registered request caller
must connect this BCD/BEF/CBB context to the full05F720 request, identify its
separate request+08 selected-source receiver and preserve the actual88/B8
history lifetimes. Only then can source C20 transaction execution and the
outer commit/rollback be ported as acceptance. The explicit library transport
cache intentionally stops before those callbacks and does not fabricate
receiver A.

The auditor checks the page accessor, exact BEF argument stack slots,
CB7 construction/storage, virtual bindings, begin/end order and A8 getter.
It uses pinned Document/Engine PE fingerprints and emits region hashes.
It makes no assertion that native SDK page history is absent and no change
to the existing library target or acceptance state.
