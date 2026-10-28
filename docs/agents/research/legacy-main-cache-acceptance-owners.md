# MAIN cache transfer and acceptance receivers

This read-only checkpoint follows the explicit library BCC target. It
separates a proved C8F row transfer from the still missing request/history
owner. It does not execute vendor DLLs or introduce an SDK textField, page,
history flag or accepted C20 result.

## Exact C8F producer

Document descriptor C8F is209630. Metadata2095F8 names object table2095B0
and constructor table2095E0. Parameterized constructor11B450 is that latter
table's slot10. The nearby11BB20 is a CA7 interface test, not this constructor.
For the actual16-byte argument branch,11B450 retains argument.qword0 into
object10 and requires exact retain identity. It initializes object18 to
null, derives flags20.bit0 from argument.byte8, and clears string28 and
vector38/count40/capacity44. Text05F720 supplies the receiver returned by
its DOM reader's V200 and byte8=1. It does not supply an inferred raw INPUT
or a fabricated page object.

Append11B860 reserves exactly count+1 records of40 bytes when required.
Allocation failure returns before changing vector/count. It clears all40
bytes, increments count, then writes these lanes in native order:

| Lane | Native argument |
| --- | --- |
| 10 | Exported BE2 edit result |
| 18 | Reverse BE2 edit result |
| 20 | C4D transaction |
| 00 | Before version |
| 08 | After version |

The05F200 call supplies before=-1, after=-1, its actual exported result,
null reverse result and its actual pending C4D. Only after append succeeds
does05F200 clear the local C4D holder and context.result20. The append does
not clone, retain again, execute the transaction or apply DOM edits. Before
that success the caller still owns both resources. The flag1 destructor
11B6C0 destroys each nonnull BE2 lane through BE2.V18 and C4D through V50,
then frees the vector and releases the retained receiver. A copy constructor
11B530 clears ownership bit0; a nonowning shallow row copy is a separate
contract and cannot be substituted for the actual owning parameterized row.

## Three acceptance receivers, not one C20

The following distinction corrects the earlier shorthand naming every
receiver in05F720 as the same source C20:

| Receiver | Exact producer and calls |
| --- | --- |
| A | request.qword0 -> +10 DOM reader -> V200; retained in R15; queried for A8; V178 begin, C8F constructor.qword0, V180 end |
| B | request+08, reloaded at05F874; V188 receives the row's actual C4D |
| C | original DOM reader, retained in R13; V218 receives the original parent and row's BE2 export |

These may be related by a real native factory. Their identity or type is
not proved by equal virtual slot numbers. In particular, Ink C20.V180 is
06EC60. With R8=0 it writes byte1 through its R9 output pointer.05F720 sets
R8=0 and uses R9 as the V180 call target. Binding receiver A to this Ink
C20 implementation would therefore contradict the observed call ABI.
This is positive evidence against that shortcut, not proof of which other
factory produced A. The separately typed library selected INPUT context
does not establish that factory merely by retaining its C22 and registry.

Ink C20.V188=06ED30 is a concrete possible implementation of receiver B,
once B's type is established. A null incoming transaction succeeds. A
nonnull transaction selects either direct C4D.V40 when C20.history58 is
null or C4D.V38=07E210 when that owner is retained. The latter begins a
history service, requires its retained F8 transaction, invokes078090 and
performs its later history call. The portable pending-command list has not
yet restored those observer/history effects. Empty incoming history or a
successful DOM target Apply cannot be used as their replacement.

The reader distinction is also executable at the static binding level.
Fresh parameterized BCC table227080.V200=031900 returns null. Our explicit
fresh library target therefore cannot simply be passed to05F720. Engine
BEF table2273A0 instead has V200=00C0A0, which returns object28. BEF's
0F0520 constructor retains its explicit second qword into28 and shares
the supplied underlying reader's18/10. It does not invent a context for
that second argument.

Document CBB gives a concrete further candidate chain: table208F90.V178
and V180 are116970/116990. They load wrapper10 -> innerB8 and forward to
that object's slots20/28. CBB constructor11AF60 retains its explicit source
wrapper, aliases its inner10 and registers with the actual B8 service. Its
underlying C81 constructor1114C0 owns a much wider page/store lifecycle.
This identifies real context interfaces to trace next; it is not proof
that05F720's request used CBB or that the library raw document owns that
page/store lifecycle.

## Entry provenance and next bounded stage

The audit scans all6793 Text exception-directory functions for decoded
immediate or RIP-relative references to05F720, all bytes for an absolute
QWORD pointer to it and the PE export table. No such entry reference is
present in the pinned image. The direct05F858 call to05F200 is present,
but a body calling another body is not its own registration proof. This
bounded negative result says nothing about arbitrary computed addresses
or an external SDK caller. It cannot justify labelling05F720 the currently
registered main request entrypoint.

The first missing native producer is the request binding that connects a
real DOM reader, its V200 context, request+08 selected-source receiver and
the original parent, together with the entry registration/caller. Closing
that owner precedes executing V178/V188/V180 and calling the result
accepted. The existing explicit library root deliberately has no such
implicit context. No acceptance runtime is added at this checkpoint.

A separately named library-owned C8F transport cache could faithfully
retain the already typed BE2 export and exact pending C4D, preserving the
row layout and transfer/failure semantics above. It would close transport
only. It must not expose an acceptance result or bypass the still missing
observer/history/request join. The next useful native trace is BEF's
second constructor argument through the concrete request factory and
Document CBB's retained B8 service, rather than another guessed zero flag.

`audit_main_cache_acceptance_owners.py` pins all four PE hashes,13 regions,
18 virtual/constructor bindings and89 exact instructions, then performs
the bounded entry-reference scan. Its JSON report records each region
hash and the negative-search scope. Existing target/runtime captures are
not modified by this static checkpoint.
