# Math CB8 temporary patch application

This is a static owning-join proof, not a runtime implementation or native
original-page acceptance. It follows CB8.V10 through actual diff production,
ordered payload cloning, callback filtering and temporary BCC mutation before
late tag resolution. Vendor DLLs are read as bytes only.

## Produced patch and real retained owners

CB8.V10=033040 calls the SUPPLIED temporary BCC.V220 with that field's existing
mathResult, the actual CB8-owned candidate mathResult and CB8-owned candidate
BCC. The optional caller property/context is passed unchanged. These are the
two distinct DOM arenas from the preceding schema proof. CB8's retained Ink
is the third C20 clone; the temporary DOM and candidate DOM share the actual
retained name owner. None is reconstructed from arbitrary output strings.

BCC.V220=0EFBB0 dispatches to registered BE2.V28=0E2250. The wrapper supplies
the old temporary BCC, candidate BCC and a final null optional writer.0E2250
checks that the old root belongs to the supplied old DOM and that root kinds
match. It allocates a24-byte batch and retains the OLD temporary BCC as its
writer because the optional writer is null. The internal32-byte action-list
header and separate action/payload storage are distinct owned allocations.
It then calls0E70A0 with both actual roots and this same batch context.

This writer identity determines insertion payload cloning.0E2090 creates an
opcode2 action and stores the actual old parent. If the candidate writer and
retained batch writer differ, it recursively clones the ACTUAL candidate
subtree through the batch writer's V88 into the old temporary BCC arena.
It retains that clone through V98, stores it in action+20 and stores the
insertion ordinal at+28. Only then is the action linked to the pending list.
The original candidate subtree stays in its own arena; it is not moved into
the old field, and a caller's detached node table is not a replacement.

## Bounded fresh root and default child matching

0E70A0 compares element names in the same name domain. For a same-name kind3
mathResult it processes old attributes, candidate attributes and then child
alignment through0E7550. The current fresh source has no prior mathResult;
028070's lookup/create branch creates an empty temporary mathResult. Both
roots have no root attributes. The identity raw scope creates exactly THREE
candidate root children: sequence, symbolSegmentList and mathTerminalNode.
All symbol and metadata attributes live inside these actual child subtrees.

The native child-aligner first asks the old DOM for an attached matcher
through V58=0ED270. It can find an inherited attachment through0F2460. A
non-null custom attachment must not be silently replaced by the default.
The explicit new library fresh-root scope owns no such attachments and admits
only this proven default branch; arbitrary imported SDK pages are excluded.
When lookup is null,0E7550 constructs a BE0 matcher with the retained old
name owner. The registered BE0 service table21CA70.V28 is0D33F0. Its object
field+08 holds that SERVICE, assigned by0D2C30, not its unrelated primary
class virtual table21CA20. Using the latter would select a different method.

0D33F0 enumerates the real old/new child chains and performs its float edit
distance alignment. With oldCount0, the matrix is a single row of exact
integer FLOAT32 costs0..newCount. There are no paired-node cost callbacks or
old-child substitutions. Reverse backtracking produces `(null,newChild)`
for every candidate child and reverses them into original new-child order.
0E7550 turns those into opcode2 actions with ordinals0,1,2. The scoped result
is three recursive subtree insertions, not guessed attribute changes or a
single flat text-node replacement. The generic nonempty-old matcher, schema
override callbacks, alignment tie behavior and prior-result mutation are
outside this bounded runtime proposal.

The static auditor includes65 PREPARED reductions for empty-old lengths0..64,
checking the one-row cost/backtrack and resulting insertion ordinals. These
are an independently derived reduction of pinned native control flow. They
are not vendor execution or genuine NN captures, and do not prove arbitrary
nonempty-old matching has been ported.

## CB8 filtering acts on produced actions

After diff construction,033040 calls temporaryBCC.V238=0EFCD0, which dispatches
to BE2.V40=0E25C0. It iterates the actual produced pending actions and invokes
the actual032800 callback with the SAME CB8 and literal `tag` name. A callback
true result removes and releases that action from the live list; false keeps
it. This is a destructive filter, not a detached report over arbitrary tags.

032800's special branch requires opcode7 and a matching tag-attribute name,
parses old/new values, requires a negative new temporary tag, finds that CB8's
actual pending entry, and checks the SAME third C20's registered selection
and name. Its selection predicate is the pinned native C22 test. An
equal-looking Document, unrelated registry or caller-supplied old tag table
cannot authorize the branch. Other operations return false.

The fresh root's three produced actions are ALL opcode2, so the callback
keeps all three. Deep cloned attributes are part of inserted payloads, not
separate opcode7 actions. General prior-root tag-reuse filtering needs its
own positive producer and is not claimed from this fresh reduction.

## V228 applies the payloads; V230 cleans the remaining batch

028070 next calls temporaryBCC.V228=0EFC40, which dispatches to BE2.V30=
0E2560. This is actual DOM mutation, not merely edit registration.0E2560
calls0E4B90 on the retained temporary BCC and pending action-list header.

For opcode2,0E4B90 locates the action's child ordinal on the actual old
parent. If an existing sibling occupies it, V B8 inserts before that sibling;
otherwise V A8 appends the SAME cloned payload node. Applied in list order
to the actual empty mathResult, ordinals0,1,2 append the three cloned
subtrees in native order. No second clone is produced during this application.

Other internal opcodes dispatch mutations1..7; the default switch rejects an
unknown opcode. Most underlying DOM mutator return values are ignored by this
native loop. A later portable checked mutation may deliberately preflight and
poison failures, but must not claim that native V30 reports every DOM mutation
failure, automatically rolls back an applied prefix, or validates a complete
external patch ABI. The actual native header is not the separate exported
40-byte BE2 transport array used in the Text pipeline.

Only when0E4B90 returns success does0E2560 free the action-list header and set
the batch header pointer toNULL. It does not dispose the batch's separate
action/payload storage or retained temporary writer at this point. CB8.V40
now walks the TAG ATTRIBUTES OF THE INSERTED TEMPORARY-FIELD NODES, then
V48(false) gathers their resolved tags and V50 processes stale old tags.
This lifetime must span the whole late-registration sequence.

028070 eventually calls temporaryBCC.V230=0EFC90, which dispatches to BE2.V38=
0E1B00. It destroys any still-live action list, releases remaining payload
storage, releases the retained writer and frees the batch. This cleanup is
used after success and after late resolver failures. If V30 refused before
its header-free step, V230 still owns that header. A portable owner must
release its own payloads once while keeping inserted DOM nodes reachable;
it must not erase an applied tree or third-registry prefix to fabricate an
atomic rollback. Native refcount/allocator ABI is not recreated by managed
object retention.

The later original selected DOM.V208 edit, selected Ink.V178 edit and pending
receiver.V18 transfer remain separate operations. A candidate/temp-tree join
alone is not an original-page/history transaction or application integration.

## Proposed actual runtime scope

After review, add a NEW Math-specific publisher with one narrow existing
field accessor under the actual field gate. Accept only a produced scoped
field/copy receipt and create its outer vector internally from that exact
copy. The accessor clones the private selected C20 registry/C22/Next into
the third registry. It supplies the retained selected field/name owners and
source C23/full INPUT; caller rows, tags, categories, counters and success
flags are not production inputs.

Own the cloned temporary field/mathResult and CB8 candidate BCC/mathResult
as separate arenas with a shared real name owner. Build the actual typed
candidate nodes, allocate pending temporary IDs from2, produce the three
ordered opcode2 payload clones, apply them to the retained temporary field,
then resolve its actual negative tag attributes against the third registry.
Preserve INPUT0 and source Next1, all source C21 ranges and first alternatives,
new assigned IDs/counter, ordered old/new vectors and poisoned failure prefixes.

Scope the implementation as an explicit library semantic patch with actual
produced/cloned payloads and observable mutation stages. Do not expose native
patch bytes or pretend all imported/source-cache/alignment/history behavior
is complete. Preflight extents/budget/cancellation before copying payloads;
check source receipt, exact field/copy and liveness again at mutation
boundaries. After failure, retained diagnostics may inspect the prefix but
normal completion/replay must refuse. Source metadata remains immutable.

Verification must use actual official-ORT/full EE4 outputs, reversed physical
order, source-bound first-copy/outer data, all node/attribute types and bytes,
every range and registration ID, all three cloned subtree identities/order,
pending/registered counters and per-stage throw/cancel prefixes. Foreign
receipts with the SAME Document, closed/disposed/consumed owners and exact
budget boundaries require negative controls. Independent Python reconstructs
the whole candidate/temp trees and ordered patch; checking only strings or
caller-supplied success flags would miss the central join. Old Math captures
must remain unchanged. No runtime code is part of this static batch.

Run `audit_math2_temporary_patch_application.py REPORT [--native-repo REPO]`.
It pins18 complete contiguous bodies,435 instruction anchors,17 registered
bindings and the65 explicitly prepared reductions; it composes the existing
Math clone/schema/CB8 and generic insertion/transport proofs. Full vendor
hashes and all checked bytes remain fixed. No vendor code is executed.
