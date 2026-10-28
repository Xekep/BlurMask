# Actual legacy Math request and selected XY admission

This is static evidence for the older MyScriptMath/2D route. No vendor DLL is loaded or executed. It complements the frozen typed-candidate and normalized-clone proofs; it does not implement recognition, a source observer, AK operators or an application switch.

## Three distinct owners

The outer processor is class138B, derived from1F45. Its service callback06C680 returns resource service98 at09BE30 and C9E at09BE08. Its13EF answer is Boolean1, not an interface pointer. C9E contains recognition06BBC0 and factory06C4B0. The factory constructs **13BA**, not1388.

Class13BA derives from generic base4. Its constructor007750 constructs an actual1388 worker and reference-acquires it into context+10. The context also owns its selection header+18, auxiliary storage+28, and0x50-stride entries+40. Reset007840 clears these context selections/entries and dispatches the actual worker's V10. Destructor007900 releases the worker and owned storage. The outer138B, context13BA and worker1388 must never be represented as interchangeable aliases or one inherited object.

The actual worker1388 derives from1F40; its13EC service is09A570. Its primary V18 is034540, which forwards unchanged XY inputs to base1F40. Its primary V110 is0408C0. The previous search-owner proof pins06BBC0's use of the actual context+10 worker. This package does not replace that request with a caller-created expected tree.

## Document cache and field provenance

Document classCBD has create020B00 and clear020A60. Create queries the actual processor's C9E. Its field-root name comes from the actual root kind0/+10 or kind3/+30. With no selection it first clears the cache and creates a context through C9E+8.

With a selection, reuse requires both equal root-name content and the same retained field pointer CBD+28. A miss clears the old cache, copies the root name into+20, reference-acquires the field into+28, constructs and owns the new context into+18, and constructs C23 into+30 using the actual selection's retained Ink pointer at selection+10. A hit calls the actual C23 C31 update hook before continuing. The actual C23 V48 then consumes selected ranges, and C31 registers the CBD observer; only successful calls return the context and expose the cache flag CBD+14.

Clear releases field+28, context+18 and C23+30 and frees the copied name+20, clearing their slots. Native failures are not atomic: a failure after storing one owner can leave a partial cached prefix. The cache identity proof does not recover C23/C31 observer implementation, all invalidation callbacks or their concurrent lifetime policy. Those are explicit remaining dependencies, not permission to substitute a detached snapshot.

## Actual attached knowledge and preparation

138B constructor0680A0 zeros processor+380/+388 and cache+398 and sets+390. Successful attach068190 recognizes the actual1389 AK resource, storing its alias at+380, or a base1F48 grammar, storing its alias at+388. The optional13EA branch remains distinct. It then calls the actual base1F45 resource-attach service already pinned by the search-owner proof. Alias assignment precedes the base call; native attachment failure can therefore leave a changed alias. Retained resource ownership must not be inferred from an unsuccessful attach.

Detach068300 requires actual pointer identity with an accepted resource alias, clears the corresponding slot, and then delegates base detachment. These aliases are knowledge resources, not font-size resources.

Presearch068580 rejects missing+380 with03050000 and missing+388 with03050001. When actual worker+154 is zero it dispatches that worker's V110, then calls base1F45 V20, the genuine009500 search. Worker V1100408C0 resets count+160, releases and zeros normalized cache+168, then calls base1F40 V110004090 to release and zero the inherited raw result+A0. This gate clears both raw and normalized result owners; it does not call constraint reset003FC0, which is a different base slot V80. This is the actual presearch cache gate. It refines the older normalized-clone proof's deliberately narrower V10 reset evidence:0344B0 alone was conditional and did not establish unconditional cache invalidation.

## Selected objects, not guessed source points

The inherited C21 callback084D10 answers C38 with table0E5C78. C37 is Boolean1, not a table. Both actual C22 and C23 primary tables use retained Ink getter036530 and selected-span getter085EF0. Those short getters are pinned through their actual returns, without pretending a Ghidra body exists.

C38 count083970 iterates actual16-byte selected intervals and sums inclusive stroke-index extents. C38 indexed get0839C0 reads the retained Ink and these same spans, resolves the selected ordinal through081ED0, then calls the retained Ink owner's V90. It does not read an independent expected coordinate list.

081ED0 walks the actual intervals in order, selects the stroke index, decodes the compact endpoint fields and produces the actual partial-selection descriptor. It preserves whole-end sentinels and the byte-to-fraction conversion. Its output's booleans and endpoint values are consumed by C20 V90 **06BCB0**, which forwards the selected descriptor to06B7F0.

06B7F0 examines the actual Ink object's source and partial-view cache. A matching cached C25 is returned through its own branch; matching interval descriptors can reuse cached views. A containing cached interval or a fresh partial selection dispatches the actual default Ink view factory through context+20. The complete ordinary fresh branch can return the original source object. The existing raw-stroke geometry proof identifies that actual factory as0914E0, producing C4A integer and C4B fractional views; C4B endpoint XY interpolation is already independently ported by LegacyNativeRawStrokeGeometry. C4C supplied transformations and cached provenance clones are additional branches, not silently equivalent to an ordinary polyline. This package pins the complete containing06B7F0 body but does not claim every factory/cache branch has been newly implemented.

## Genuine Float32 XY admission

Math068680 asks the actual selection for C38, enumerates its actual objects, temporarily reference-holds each result, and inspects the object's actual kind. C1E/C26/C27 use global Document DAF.V0 **02FD10** with the worker's actual V18 callback. A null object or false callback stops the loop, releases the current object and later frees temporary DAF storage. Other kinds are not thereby proven recognized; the native loop does not explicitly reject every unknown kind.

C25 has a separate branch: it reads eight geometric corner coordinates, computes a bounding rectangle and invokes13EC.V10 with the object's stored data. That is not evidence of pixel OCR or an interchangeable XY source.

02FD10 obtains the actual stroke point count through V78. If V80 reports contiguous points it obtains the actual pointer through V88; otherwise it reserves an8-byte-per-point temporary buffer and uses actual V90 copy-out. Its callback receives **x=base, xStride=8, y=base+4, yStride=8, count**. No timestamp or pressure channel is generated. This older 1F40 path must not be confused with the separate Text timing protocol or modern EE4 raw input.

Worker034540 forwards to base002900, which first appends through034810. That routine copies each32-bit XY value from the supplied independent pointers/strides, appends the cumulative stroke end and increments the stroke count. Reserve034740 grows end-index storage in groups of8 and point storage on a64-element boundary using the actual growth constant.002900 then appends original stroke/fragment mappings, anFFFFFFFF mapping sentinel and selection bits.001A20 appends4-byte elements despite its decompiler's misleading32-byte pointer type. No interpolation, timestamps or AK inference is inserted at this copy boundary.

Native append failure is not a transactional rollback: XY or earlier mapping vectors may already have grown before later allocations fail. The source adapter still needs an explicit owning budget/cancellation/failure contract before any runtime work is approved. Nothing here permits reproducing unsafe native failure behavior as success.

## Verification and remaining boundary

Run `python tools/handwriting/audit_math_legacy_worker_admission.py output.json --native-repo <repository-with-static-bytes>`. The auditor hashes complete Ghidra body regions, checks instruction pins and exact descriptor/constructor/service bindings, and composes the existing normalized-clone/candidate/resource proofs. Short getters and all-return resolvers are separately labelled exact disassembled extents, not first-unwind or invented full Ghidra bodies.

The frozen report keeps sourceProviderComplete, runtimeImplemented and applicationReady false. A bounded ordinary source implementation can reuse the already-owned raw geometry path only after exact source identity, requested selected intervals and resource aliases are validated under one owning request. C23 observer mutation, generic reference modes, transformed/cached object factories, actual AK feature/model operators and029890 grammar matching remain required before claiming the entire native route or application recognition is restored. This evidence does not provide a neural font-size model, Russian model or a bridge from modern EE4 markup into the older typed tree.
