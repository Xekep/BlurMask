# Typed temporary-writer kernel

502 frozen prepared cases cover insertion/moves, deletion by the current ordinal,
same-parent attribute no-op, replacement by canonical name, removal by identity,
typed scalar/string writes and retained prefixes on traversal termination. Nine
additional guards include foreign nodes, cycles, cancellation and tight list
scan/shift reservations. `audit.py` compares every node, scalar, adjacency list,
freed marker and prefix against the frozen Python reference.

```
dotnet run --project tools/LegacyMathTemporaryWriter.Probe -c Release -- tools/LegacyMathTemporaryWriter.Probe/fixtures.json .artifacts/temporary-writer.json
python -I -B tools/LegacyMathTemporaryWriter.Probe/audit.py .artifacts/temporary-writer.json
```

`fixtures.py` is the authoring generator; checked-in fixtures and their fingerprint
are used during verification. It refuses to overwrite an existing fixture file.
CI checks managed and Native AOT outputs. Both this probe and the separate prior
diff probe use the existing Math friend assembly name in separate directories.

Scope is the default library-created temporary writer with no observers and valid
unique canonical attribute names. Operations 2/4/5/6/7 are implemented. Operations
1/3 are outside this produced diff and explicitly refuse. Unknown operations return
failure; earlier operations remain applied. Managed node/lifetime accounting is
not the native allocator, linked-pointer ABI, observer system or transaction
rollback. A freed marker does not recreate native memory reuse. This prepared
kernel has no current document, revision, owning apply or publication authority.
