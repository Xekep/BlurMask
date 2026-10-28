# DOM patch preflight and the library target boundary

This read-only checkpoint pins `DOM.V210 -> Engine0EFA70 -> BE2.service8
0E2870`. It proposes the next owning library join; it does not implement native
CBB acceptance. The actual original page DOM, worker-selected INPUT DOM, and
the publication's new library target are separate owners.

## The missing algorithm

`LegacyLibraryTextDomTarget.Apply` already checks the exact edit/target,
successful export, captured epoch and retained previous child. It then claims
the edit and performs its operations on the live target. It does not first
clone the target and simulate the edits. Those existing identity checks are
not a substitute for native DOM preflight.

The registered receiver `Document115800` calls original Ink.V180, original
DOM.V210, checks both execution returns, then checks both compatibility bytes.
Only after all four checks does it execute Ink.V188 and DOM.V218. Pins
115B38/B3D, 115B6B/B70, 115B76/B84, 115C5E and 115DAC establish this order.
`Worked=false` and `Worked=true,Compatible=false` are distinct results.

For our actual private document factory the target is a fresh kind4 root and
the genuine edit contains one operation2, empty path, child index0. A structural
incompatibility cannot arise from this factory without a foreign/stale owner
or a prepared counterexample. Do not claim actual inference produced one.
The missing native work is scratch allocation, target copying, complete path
resolution and edit simulation before registry mutation, including their
defined failure behavior. Repeated direct target replacement uses actual
operation4 then operation2 and can exercise the scratch-copy path with a
nonempty previous target.

## Exact supported native algorithm

Both BCC and BEF tables bind V210 to0EFA70 and V218 to0EFB10. V210 first calls
0EBD50, which locks the reader context and searches its real32-byte observer
rows for BE2; this is not a constant-null getter. A null edit reports
Worked1/Compatible1. A nonnull edit is passed unchanged to BE2.service8.

0E2870 reads reader+10 (BEE name owner). A found BE2 observer with nonzero50
causes an early Worked0 return **without writing the compatibility output**.
The explicitly parameterized no-history library BCC can support the absent
observer branch; this says nothing about a real page's CB7 observer.

For a nonempty edit:

1. A null root argument is replaced by reader.V20. Construct a separate BCC
   with byte0=0 and the same reader BEE name owner (16-byte constructor payload).
2. For a kind4 root, create the scratch constructor root and deep-copy each
   existing child through V88 then VA8. The native sibling count includes a
   encountered kind0 terminator; current library trees have no such child.
   Other root kinds use V88 on the root itself and are outside the proposed API.
3. Allocate one8-byte mapped-parent slot per actual40-byte edit row.
4. Resolve **all** row paths against this initial scratch tree before simulating
   any row. Path entries are32-bit sibling ordinals. Missing parent sets
   compatibility0; the function still returns Worked1 after cleanup.
5. In row order, operation1/2 clones payload through V88. An existing child is
   preceded by an insertion via VB8; otherwise current child count must equal the requested
   signed child index before VA8 append. Operation3/4 requires an existing
   child and detaches it via VE8. Missing child or invalid append index means
   compatibility0. Unknown operation codes simply advance in this function;
   the proposed production API still accepts only its actual owned2/4 rows.
   VB8=0EADD0 is insert-before, not replace: it links new.next=old and
   old.prev=new without unlinking the old node. A replacement is the caller's
   explicit operation4 removal followed by operation2 insertion.
6. Free the map and destroy scratch BCC, then write compatibility. The return
   reports whether scratch setup/map allocation worked, separately from that
   compatibility byte. Scratch-constructor, initial-existing-tree clone, or map
   allocation failure returns Worked0 with compatibility1.

At0E2C60 the simulation payload clone result is moved directly to R9; the next
test is `test rdi,rdi` (existing child), **not a null test of the cloned payload**.
Native unchecked clone-null is not a defined Worked0 branch. A portable
implementation must explicitly refuse that boundary, retain its diagnostic
prefix, and poison its entered one-use service; it must not continue with an
invented node. Managed scratch cleanup is an adaptation and never publishes
the scratch tree as an accepted target.

## Owner mapping and proposed consumption

Native CBB.V78 supplies original inner38 Ink and inner30 DOM. The worker's
selected C20 copies and selected request DOM are produced separately. Existing
`LegacyLibraryTextProcessingContext` owns a selected request tree
`kind4 -> textField -> input(tag=actual retained INPUT ID)` and its own BEE.
The result Seed is adopted from this context before recognition. The document
publication instead creates `LegacyLibraryTextDomTarget.CreateFresh(sameSeed)`:
a new kind4 library root and its own BEE, shared with its detached writer and
export clones. Same Seed/document provenance does **not** make those DOM roots,
name owners or trees identical to the original page DOM.

The next bounded API can therefore be named
`LegacyLibraryTextDomPatchCompatibility.Prepare(actualDocumentReceipt)`.
It retains that receipt's exact transferred BE2 edit, target, captured epoch,
Seed/document and pending transaction identity. It accepts no caller rows,
booleans, target replacement, revisions or page handles. Claiming it must occur
before any selected-registry replay has entered and before DOM apply. The
receipt retains the claimed service, so its result is not an orphan checksum.

`Run` would hold the document gate then target operation gate throughout
scratch cloning, path mapping and simulation. Once claimed, both replay and
DOM application must require its exact immutable result with Worked1 and
Compatible1; the existing optional original-INPUT compatibility gate remains
independent and required when claimed. Pending, failed or incompatible claimed
services block both consumers. Pre-entry cancellation remains reusable;
entered cancellation/failure retains the completed prefix and is single-use.
Old unclaimed APIs retain their current outputs and behavior.

Use named preallocation bounds for edit rows, payload/initial-tree nodes,
attribute/value bytes and depth. Check the complete bound before allocating
scratch. Record actual copied prefix, mapped-path prefix and simulated-row
prefix; genuine structural incompatibility may be unreachable in the fresh
receipt and should be covered only by honestly named prepared controls. A
successful compatibility result does not claim later live Apply cannot fail:
its payload clone has another allocation boundary. Existing live failure-prefix
and poison semantics remain separate from native page history rollback.

No CBB revision, original page DOM, native CB7 mutation/finalization, receiver
range/ID remapping, post-apply input-tag validation or accepted-C20 receipt is
provided by this proposed service. Library transport rows retain their explicit
-1/-1 versions and are not recast as registered applicable native rows.

## Static evidence

`audit_library_dom_patch_preflight.py OUTPUT` checks the full pinned Engine and
Document PE hashes, six virtual/service bindings, exact instruction tuples,
and full-region hashes, with the existing original receiver/history audit as a
dependency. Key complete function bytes:

| Region | SHA256 |
| --- | --- |
| Engine0E2870..0E2D70 | 10104e3d56789792d363c1a3afed16392e072aa4de741fc61d41aad16d19dbe7 |
| Engine0EFA70..0EFB10 | 0ca1c7d6a6e3dad3c5867a1cc21f19d66a793e484823750d2c56de82888c7706 |
| Engine0EBD50..0EBE40 | 3549bdafe28d3f22320c543ecdbfdda3d8a77ee14e1b9bab21b5ef77dd1e13c2 |

This checkpoint executes no vendor code and changes no runtime.
