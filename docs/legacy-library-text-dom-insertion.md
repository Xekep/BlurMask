# Library-owned DOM insertion and replacement

`LegacyLibraryTextDomTarget.CreateFresh(sameSeed)` creates an explicit new
parameterized-BCC/no-history target. Its actual constructor root has kind4,
no name or parent and empty child links. This is a new library DOM root,
not an SDK textField, page or accepted C20 document. The
[target proof](legacy-library-text-dom-target.md) and
[MAIN transport contract](legacy-main-document-be2-transport.md) remain
separate static prerequisites.

`LegacyFreshMainDetachedTextDom.PrepareForTarget(actualSources,target)`
requires the exact same Seed and Document. It creates a separate writer
sharing the target's retained BEE name owner. The existing `Prepare` route
continues to create an independent detached arena and preserves its old
captures. Both routes use the same exact-Sources CWT: switching entrypoints
does not permit a consumed formatting invocation to run again.

For a fresh root the new builder constructs031880 and then queues opcode2.
For this target's one previously applied textResult it first queues opcode4
and copies that previous payload into the writer, then constructs031880 and
queues opcode2. This matches061510's ordering. The fresh native old-reader
slot is null, so V70's previous-payload clone is necessary. The new subtree
already belongs to the writer, so V60 retains it instead of cloning it at
that point. No ready candidate-local trees are merged after publication.

`LegacyLibraryTextDomInsertion.Prepare(actualBuiltResult).Export()` owns the
V50 export. Opcode2, or ordered opcodes4/2 on replacement, retain empty paths
and child ordinal0 derived from this privately owned target. Payloads are
recursively copied into the writer. `Rows` are immutable, with40-byte scalar
snapshots, empty paths and typed payload identities. Pointer-shaped20 is a
zero serialization placeholder for the actual typed payload sidecar; it is
not represented as a native address. Default allocator zero lanes and
assigned scalar lanes are distinguished from those sidecars.

`target.Apply(actualExport)` performs the separate V218/V10 ordinary edit
branch. It removes/releases the exact old child for opcode4, recursively
copies the exported opcode2 payload into the target arena and appends it to
the root. Build, export and applied nodes retain distinct `(arena,identity)`
pairs while sharing the exact name owner. Public before/after trees and
values are deep immutable snapshots; replacement does not modify an older
snapshot. No XML parser, escaping or expected recognition label enters this
path.

The target supports only its fresh root and the single textResult produced
by this API. Arbitrary existing parents, additional children, supplied edit
records, nonempty paths and unrelated payloads are not accepted. Repeated
tests create a new source-formatting invocation from the same retained
handoff. This is explicit library replay of immutable recognition sources,
not a second inference or a hidden exception to source single-use.

The portable epoch prevents stale builders/exports from mutating a target
that has advanced. It is not a fabricated native C8F version number. Exact
target, Seed, Document and BEE references are checked, and export/apply are
single-use. A pre-entry cancellation leaves a valid call available. An
entered apply exception or cancellation poisons the target and preserves
the actual mutation prefix. In particular, failed replacement can leave
the old textResult removed; there is no invented rollback.

Prepared internal service faults cover queue4/queue2, export header/array,
recursive copies and cancellation. They represent fallible service return
points, not Windows allocator addresses or OS fault timing. Queue2 failure
follows completed031880 and therefore retains its unattached subtree;
031880's own normal-false cleanup is not incorrectly run afterward.
Failed V50 export exposes only diagnostic completed rows and arena prefix,
not a fabricated successful native partial header. It cannot be applied.

Native V10 does not guard a null recursive clone before Append(null). This
undefined branch is explicitly refused. The target is poisoned after entry,
including after an earlier removal, and its diagnostics remain readable.
Managed storage retains source/queue/export arenas and native-unattached
failed nodes until their library owners are collected. It does not emulate
native pool addresses, freed-memory bytes or observer/history application.

The probe uses four actual inference-owned CHAR/WORD/TEXT/line sources,
including reversed physical input and rotation. The independent Python
oracle reconstructs trees from raw40/48/ref24/TEXT32/line56 records, then
independently executes the native recursive-copy and root-edit algorithms.
It checks all scalar40 rows, ordered payloads, full pointer-free arena/link
states,24 successful copy stages,90 fault prefixes and cancellation prefixes.
It uses no expected labels. Stable parity normalizes only the actual
environment-dependent retained UUIDgen36 value.

The retained C4D commands, selection registry, source payloads and older
snapshots are unchanged. C8F result-cache transfer, native C20 observer
history/acceptance, external SDK parent ownership and full0283B0 remain
outside this explicit library target API.
