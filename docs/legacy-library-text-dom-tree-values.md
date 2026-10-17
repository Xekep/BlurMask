# Owner-free DOM content values

Existing `LegacyLibraryTextDomNode` snapshots have immutable content but retain
their Arena, Seed and publication scope. Keeping such a tree reachable can
therefore retain a recognition model graph until the scope is explicitly
disposed. It is not an owner-free serialization value. Explicit scope disposal
still releases its resources even when an old snapshot remains reachable.

`LegacyLibraryTextDomTreeValue.CopyFrom(node)` adds a separate value-only tree:
scalar identity/kind/name/tag, copied value bytes, attributes and children. It
contains no Arena, Seed, scope, input document or model references. Immutable
name strings are safe to share. Every byte inspection returns a copy.
The existing node/snapshot APIs and recognizer output remain unchanged.

Before copying value payloads it checks the complete immutable input against
limits of65,536 nodes,4 MiB of stored value bytes and128 levels. Callers may set
lower limits. Metadata allocation and caller inspection copies are not included
in that value-byte budget; it is not an RSS limit. Invalid bounds, exhausted
budgets and cancellation throw without returning a partial value tree.

The headless probe creates two genuine configured recognition/document
publications. With only the new value tree reachable, the first scope becomes
unreachable and is collected after its finalizer. An original-node negative
control retains its scope through Arena/Seed, as expected. The probe then
explicitly disposes that control. This tests the ownership difference, not
recognition accuracy or total application memory consumption.

Managed and fresh Windows NativeAOT pass full content equality against the
original trees (82 nodes/362 value bytes), mutable-copy checks and11 refusal
guards. Their UUID-normalized capture hash is
`3eedd08f5e6cb6713f51a96b799318186163f65341f8dc97653c7045ca902fed`.
The existing document probe remains the independent native-protocol content
oracle; the new probe specifically checks copying and actual GC reachability.
