# MAIN document context and BE2 transport boundary

This is a read-only contract beyond the detached031880 subtree. The concrete
MAIN caller is Text05F200, reached by05F720. Document053C60 builds a similar
context for the separate2DA.V20 caller-UTF8 route; it is not evidence for
the MAIN2DA.V18 route. No vendor code is executed by this analysis.

## Context producer and retained identities

Text05F200 obtains a DOM reader from `(*request)+10` and a target parent node
from `(*request)+18`. It creates a new parameterized BCC with16-byte arguments:
byte0 is zero and the qword at8 is the reader's retained name owner at10.
This is a shared native name-pool identity, not a UTF8 string or the raw ink
document. The caller initializes the stack context at RBP-31 as follows:

| Offset | Exact producer |
| --- | --- |
| 00 | Retained request DOM reader, written05F3DA |
| 08 | Existing request parent node, written05F3F7 |
| 10 | Retained new BCC writer, written05F353 |
| 18 | New24-byte BE2 batch owner, written05F37F; initialized by BE2.V20 |
| 20 | Output edit result, initially zero; written by0658E0 only on export |
| 28..48 | Additional cached/prior branch vectors, initially zero |

At05F47C..05F4A1 this exact context is the seventh argument of the actual
2DA.V18 call. The source table275550+18 resolves to0283B0. Its final2E7.V0
call passes this same context to0658E0. The separate05F050 tuple retains the
actual source C20, C4D transaction and original registry counter; it does
not provide a substitute DOM parent.

The current portable selected INPUT owner models original raw geometry,
C20/C22 selection and the C51 registry. It does not yet own either the
native request DOM reader or its parent node. A source registry ID, raw
object ID, INPUT tag or source-line ID cannot be reinterpreted as either.

## Fresh source branch is not an empty-parent branch

0658E0 calls061510 when the prior2DA pointer is null or its D8 count is zero.
061510 nevertheless scans the actual parent's linked children at20. It
compares each child name with literal `textResult`, retains the last match
and its ordinal, and queues BE2.V70 removal when a match exists. An empty
parent would produce insertion ordinal0, but the source-prior guard alone
does not establish that parent state.

After any removal queue succeeds,061510 invokes actual source.V30 with
context.writer10. A nonnull subtree is queued with BE2.V60 at the chosen
child ordinal under the same concrete parent. This does not immediately
append the subtree to that parent. The queue is an edit plan.

The local BE2 call context retains the batch owner, current action tail,
old-reader identity and new-writer identity. V60=Engine0E2090 creates opcode2
through0E4440, stores the target parent at action18, subtree at20 and child
ordinal at28. When the call context's writer is the batch writer it retains
the actual node through DOM.V98; otherwise it first clones through V88.
Neither node pointer is a portable serialized source ID.

## Exported BE2 result and disposal

On successful queueing0658E0 invokes BE2.V50=0E35C0 with the batch, writer
and concrete target parent. It retains the supplied writer:0E3625 forwards
the exact R8 writer and0E3637 calls the retain service. It does not create a
replacement independent DOM arena. Its output header is24 bytes, containing
the retained writer, action count and40-byte action array.

Each exported row has these assigned lanes:

| Offset | Meaning |
| --- | --- |
| 00 | U32 operation code; this insertion is2 |
| 04 | No explicit writer assignment |
| 08 | Path-storage identity, represented separately from portable scalar bytes |
| 10 | U32 path count |
| 14 | U32 path capacity |
| 18 | U32 child ordinal |
| 1C | No explicit writer assignment |
| 20 | Cloned payload-node identity, also a typed sidecar |

Paths are assembled by walking the action's parent chain to the export
target and prepending each sibling ordinal. If action.parent is exactly
the export target, this path is empty. V50 calls writer.V88 with recursive
copy enabled for payloads; insertion payload identity therefore differs
from its input subtree while remaining in the retained writer/name domain.

The writer itself does not set row04/1C. Under the standard Engine root
allocator these fresh bytes are zero:033C70 default configuration252000,
034010 allocator registration252030/1DB660, V18=037410, default thunk1BA950
to1C71C0 and imported HeapAlloc with flags8 at1C7210. Custom allocation is
outside that conclusion. Native pointer bytes are not replaced with made-up
addresses even in the zero-filled default scope.

0658E0 stores a nonnull export at context20, then destroys context.batch18
through BE2.V38 and clears that field on both success and failure. A failed
export is not a completed transaction. Unattached subtree/queue failure
prefixes and exported clones have distinct ownership; later adaptation
must preserve that distinction rather than disposing one node twice.

## Acceptance is later and has separate failure points

After2DA returns successfully,05F200 calls request[4].V18 with before/after
values both-1, exported BE2 result, null reverse result and the exact C4D
transaction. The actual request[4] is a Document C8F owner created by05F720.
C8F.V18=11B860 appends a zero-initialized40-byte row containing those five
fields. Only after this append succeeds does05F200 clear the local C4D and
context20, transferring ownership. On failure the local destructors still
release them. This append records transport; it does not apply DOM edits.

05F720's single-row branch then calls request+08.V188 with the retained C4D,
zero and false, followed by original DOMreader.V218(parent,BE2result,null).
These are separate operations with separate failure returns. Ink C20.V188 is
06ED30, a possible C4D/history consumer only after that receiver's actual type
is established. DOM.V218 on a BCC is
Engine0EFB10, which invokes BE2.V10=0E2D70; the latter resolves paths,
recursively clones insertion nodes and executes the DOM mutations. It is
not the earlier V50 exporter.

Finally05F720 closes V180 on the distinct object returned by DOMreader.V200
and commits or rolls back the outer history transaction. This object cannot
be identified as Ink C20 merely by its virtual slot number; its observed ABI
contradicts that shortcut. See [the receiver/entry proof](legacy-main-cache-acceptance-owners.md).
A successful031880 subtree, V60 queue, V50 export or
C8F append alone proves none of those later outcomes. Current portable
counter handoff does not stand in for native observer/history execution.

## Smallest next supported ownership boundary

An actual native-parent adapter must retain the request's reader, parent,
name owner, same source C20/C22 and batch owner before source publication.
It must capture real child order, including prior textResult nodes, and
produce the two independent result/transaction transfers. That producer is
not yet represented by the current raw/selected INPUT owner.

A new explicit library-owned DOM edit target can be a separate portability
scope. Its parent must be created by a proven constructor/field-insertion
path in the same arena/name domain; it must not be labelled as an existing
SDK page or silently seeded from a selected raw INPUT. Choosing a fresh
empty root would need an explicitly named new-root API and constructor
evidence, rather than an inferred no-history or no-prior flag. That proposal
requires the next parent/root producer audit before runtime integration.

`tools/handwriting/audit_main_document_be2_transport.py` checks all four full
PE fingerprints,18 regions,15 bindings and60 instruction pins, including
the MAIN context stores, queue/export lane writers, transport append,
separate apply callbacks and the scoped default allocator closure. It does
not execute the vendor DLLs or claim0283B0/accepted-document completion.
