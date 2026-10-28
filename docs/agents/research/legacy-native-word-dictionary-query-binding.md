# Native word producer to dictionary query binding

This is a static caller audit of the inspected Text engine, not a new native word scheduler. It resolves the important distinction between the dictionary descriptor's constructor field80=1 and the request actually sent by ordinary word startup. No vendor DLL or GUI is executed. Text SHA-256: `18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.

## Startup request is node0

Ordinary word startup `1801fd240` initializes the query at decoder runtime1648 before calling `runtime30.virtual40(query)`. Constructor `18020baa0` supplies the stable shared context-container identity and fixed output-buffer capacity.

| Query offset | Decoder offset | Verified startup value |
| --- | --- | --- |
|00..07|1648|Zero tag, then header byte05=8|
|08|1650|Address of shared context container at runtime2318, set by20baa0|
|10|1658|Parent-context identity zero|
|18|1660|Byte0|
|19|1661|Runtime2602 when startup mode is active, otherwise0|
|1C|1664|Owner input/graph node from startup param3|
|20|1668|Startup span param4, retained as full uint|
|24|166C|4 only when runtime254C and input flags26 bit10 are both enabled; otherwise0|
|26|166E|Input ushort flags param5|
|28|1670|Output32-byte-record buffer at runtime48, set by20baa0|
|30|1678|Output12-byte-cost buffer at runtime1048, set by20baa0|
|38|1680|Accepted-record skip0|
|3C|1684|Capacity128, set by20baa0|

Startup mode itself depends on runtime2601 and a frontend query which can replace that decision with `(inputFlags &3)!=0`; the outer span-limit gate also uses runtime2602. These mode/caller decisions are not model-derived defaults. The table describes the request once the actual startup branch is entered.

The first32-bit query field is therefore **node0**. Source2b7 constructor `180092790` sets its separate field80 to1; `092990` uses that field for a descriptor child-interval probe. Neither startup nor the dispatcher replaces query node0 with that descriptor field. The observed RU node1 is a leaf, but that fact does not prevent native startup from querying node0. The later direct packed query's root-child skip remains a distinct restored operation.

## Aggregate source preserves the request node

The abstract linguistic-source interface is represented here by concrete aggregate28d and dictionary2b7 implementations. Their verified normal-query wrappers are:

* Aggregate28d table `1802e8bb0`, slot40=`180236b30`: zero R9D and tail-jump to `180236910`.
* Dictionary2b7 table `1802b4360`, slot40=`180093f40`: zero R9D and tail-jump to `180093b50`.

`236910` recognizes startup by node0, source-ID byte04=0 and tag-header byte05 bit8. With skip0 it resets the aggregate's selected-source cursor and each child offset marker. It visits the ordered selected-source table, performs that child's `virtual88(ownerNode,0)` eligibility check and forwards the request **without changing its node** to child `virtual40`. It subtracts/adds the child's accepted-count base around the call, preserving aggregate skip accounting. Returned32-byte records receive source-ID byte04=`selectedSourceOrdinal+1`.

For subsequent requests, source-ID byte04 chooses the selected child by ordinal-minus-one. The child eligibility check still applies. The dispatcher stamps that same source ID on its returned records. The complete selected-source list construction, eligibility producer and source-controller mutation are separate upstream boundaries; this audit does not fabricate one-source acceptance.

## Ordinary and alternate continuation differ

The record writers `1f8d60` and `1f9080` receive already selected query/scoring data. They do not themselves run dictionary queries. Their caller chains must supply the following different requests:

* `180201270` copies the chosen immutable parent row's **tag at00** into query00. It clears ParentContext10 and byte18, initializes skip0, sets owner node/span from params3/4, and computes remaining byte19 from the parent's stored length and runtime2602. Its flags24 use the same conditional4; flags26 come from input param5.
* `180204290` also copies a selected parent/anchor row's **tag at00**. Its Parameter1C is the current caller-owned graph node, while Parameter20 is the **sign-extended span byte at parent20**, not an inferred range length. It clears ParentContext10/byte18/skip and sets the conditional flag4/input flags.
* `1801fe740` is a separate alternate-state path. It selects rows using the **opaque/tag value at parent28** and state `(parent21 &3f)`. It copies parent28 into query00, that six-bit state into query18, and uses flags24=`100 | conditional4`. It must not be silently substituted for the ordinary parent00 continuation. Validity/scheduling of this alternate state is not recovered by copying every initial record's FFFFFFFFFFFFFFFF field28 into a query.

Each query page is consumed in native32-byte record order. The actual callers advance skip by the returned count before fetching subsequent pages. Query node, owner input/graph node, source arc and returned lexical label are separate identities. Frontend `152320` (`virtualF8`) forwards scoring-request fields08 (int32),10 (ushort),20 (byte), and the supplied enumeration ordinal to candidate graph `virtual58`; that returned arc is subsequently stored by `1f8d60`. It is not the packed dictionary node from the query's first field.

These chains do not establish a native parent selection, source/category mode default, context-path correction, final row scheduling or winner. The existing initial/continuation writer and original beam tests remain bounded transport checks until those callers are attached.

## Initial symbol-context producer

Dictionary preparation `092990` receives a shared context container as param4 (`local148`). This is **distinct from source+38**, the separately allocated language binding used for descriptor bit1.

For each prepared lexical label, the initializer resolves its exact acoustic/control-token map. When the mapped ushort is neitherFFFF norFFFD and the supplied container is nonzero, it calls `1800918d0` with:

* holder `{globalContext, sharedContainer}`;
* tree-root slot at `sharedContainer+0`;
* key equal to the **mapped acoustic/control ushort**, not the lexical label index.

A newly allocated node's parent-link field00 is set tozero. The resulting node identity enters source68 at the original lexical-label index. Duplicate mapped keys share a node. Unmapped/virtual/replacement entries retain zero as observed in the initializer; that does not make an all-zero initial context array equivalent to the actual startup context factory.

`0918d0` performs unsigned-ushort-key lookup/insertion into an AVL tree. Exact40-byte node fields are:

| Offset | Native field |
| --- | --- |
|00|Symbol-path parent link; caller-managed, distinct from BST linkage|
|08|Root of the next-symbol child tree|
|10/18|AVL left/right child pointers|
|20|Ushort mapped symbol key|
|22|Signed AVL balance byte|

Allocator `180091b30` uses4080-byte pages (`FF0`) with102 nodes at40-byte stride and separate16-byte page descriptors. It searches usable pages before allocating another and reports allocation failure. The portable AVL/pool implementation is not added by this audit; opaque pointer values are not synthesized.

Packed child materializer `0920a0` uses the initial source68 array when query ParentContext10 iszero. With a nonzero parent and shared container, it looks up the mapped symbol in the tree-root slot **ParentContext+8**, allocating from the shared container's pool at+10. On creation it writes the new symbol node's parent-link00 to the parent context.

Startup `1fd240` additionally obtains sentinelFFFA in this shared symbol index and traverses parent links, with `1f6fb0`/`1f7080` path rewrites in enabled branches. The context resolver required by the prepared dictionary query API is therefore a real shared symbol-path factory, not arbitrary invented IDs. Returning zero contexts may test query bytes and pagination but does not prove the downstream native startup/context lifecycle.

## Boundaries for the next executable stage

The caller audit closes startup node0, capacity128, request fields and the normal/alternate continuation distinction. A faithful composed stage still needs ordered selected-source/eligibility state, the shared symbol-context factory and enabled path rewrite rules, exact scoring descriptor/category callbacks and the actual parent/row scheduler. Native initial/continuation writers should consume those produced records, not a manually scheduled map adapter presented as native query orchestration.

Private static exports are in `D:/BlurMask-build-artifacts-20261011/dictionary-word-query-next/` during this investigation. They are not shipped source code or runtime dependencies. The repo's existing `LegacyPreparedDictionaryQuerySource` is not modified by this audit.
