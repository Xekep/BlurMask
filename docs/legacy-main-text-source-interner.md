# Actual source-range node interning

`LegacyOwnedMainTextSourceInterner.Prepare(actualRawDocument)` owns the
fresh insertion-only01B4E0 key tree and insertion-order linked list.
`Intern(actualSourcePositions)` accepts only the identical original raw
document and copies the actual ordered05AFA0 position ranges. It retains the
first inserted source-position producer with each node. Equivalent keys return
that same node and an `Inserted=false` result without changing tree/list links.

The cache is document-bound rather than candidate-bound: native2DA+48 source
interning can outlive an individual result. A later full024F30 caller must
separately bind the current candidate, TOP emission and identical string pool.
This component never turns a source key into a recognized label or accepted
text, and does not initialize024D40 source payload fields.

## Exact key and tree semantics

024F30 calls01B4E0 on wrapper+48 with its actual temporary source-position
vector.01B4E0 compares through Ink context20/table8 virtualF8. The registered
C21 descriptor is Ink0E5C90, table0E5AC0; slotF8 targets081870.

081870 compares the **unsigned** first64-bit position, then the unsigned
last64-bit position for each ordered16-byte range. If their common prefix is
equal, it compares unsigned range counts. It neither sorts nor coalesces
ranges, removes duplicates, masks mode/fraction bytes, nor compares only a
bounding rectangle. Two representations with the same geometric union can
therefore remain different source keys.

01B4E0 stores left0/right8 and signed balance10. Key descriptor38/40 refers
to the ordered position vector. New nodes receive native AVL balancing through
019990 and are appended to a separate previous80/next88 list. Existing keys
retain their original insertion identity and position in that list. The
portable implementation uses an independent recursive height formulation;
its exact tree shape, balance bytes and list links are checked against a
separate iterative path/balance oracle, including all six-key permutations.

The native caller transfers ownership of its temporary vector after a new
insertion. The portable implementation instead copies it into private immutable
storage. `CopyRanges16`, insertion-list snapshots and tree snapshots expose
copies or immutable node handles. They do not expose mutable key storage.
Opaque owner-local node identities represent links; they are not native
addresses or fabricated document object IDs.

The native allocator uses144-byte nodes and fresh28-slot4032-byte pages.
Address-sorted page descriptors, free/recycle behavior, deletion and allocator
failure replay are not simulated here. The insertion-only key/list behavior
does not depend on those addresses. Portable limits are65536 nodes and1048576
total stored ranges, with65536 ranges per key. A duplicate lookup occurs before
the new-node budget check. Cancellation is checked before mutation; the AVL
link update has no callback/cancellation point, matching the indivisible
native insertion section.

## Verification

The probe checks976 independent streams: every permutation of six keys plus
256 streams with repeated, empty, multi-range and unsigned64 boundary keys.
It verifies14693 node insertions and22395 duplicate lookups, including exact
root/left/right/balance/insertion-list state.2048 independent comparisons cover
the unsigned lexical contract. These numeric controls are explicitly prepared
keys, not fabricated actual source positions.

The owning entry point is also run on three genuine projected source keys from
an actual completed raw result, each repeated once. A separate byte oracle
checks all six queries, three node identities and final tree/list links.
Foreign-document and canceled insertions are rejected without changing the
tree. Managed and Windows NativeAOT captures are byte-identical. No target
transcription is read and no vendor DLL is executed.

The static auditor pins Text/Ink PE hashes,01B4E0/019990 insertion and list
layout, the fresh01C010 cache state and actual081870 unsigned comparisons.
Source projection itself is the previously restored05AFA0 stage; its raw
document positions preserve original source IDs and native append order.

## Remaining024F30 join

A new source key must invoke213A0 and initialize024D40 metadata. An existing
key must invoke021760 using the same source-node state and string pool. Calling
fresh213A0 for every node and deduplicating its output afterward would change
native empty/smiley/marker and reuse semantics.

024D40 is not a glyph bounding-box estimator. It creates/reuses a C22 source
selection through0210C0/C4D document registration, initializes node metadata,
collects optional prior/cached stroke attributes through021D10, and computes
01FAE0 flags. Those document selection identities/counters and payload writers
need their actual caller ownership. This interner deliberately leaves them
uninitialized instead of inventing document IDs, zero counters or cached data.
