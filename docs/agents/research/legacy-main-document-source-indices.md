# Fresh document source indices

`LegacyFreshMainDocumentSourceIndices.Prepare(actualSelectedSources).Run()`
implements the bounded fresh `020920` stage after the actual
[01FD40/020000 source overlay](legacy-main-document-selected-sources.md).
It retains that exact source/document lifetime. It does not publish or accept
a document, run `023260`, or complete the remaining `0283B0` postprocessors.

The native `2DA` registration binds constructor `01C010` and `V18=0283B0`.
The constructor initializes key roots `120/148`, interval collections
`170/1B0`, their pools and the `1F0` ready flag. This API models only a fresh
library-owned invocation, without prior indices, external source mutations,
deletions or free-page recycling. A weak-table lease is keyed to the actual
`WordChoices.Owner`, so rebuilding an earlier immutable overlay cannot create
another empty index over the same invocation. A canceled pre-entry `Run` can
be retried on its existing owner; an entered run is single-use and preserves
its prefix on failure. Snapshot access does not reset the owner.

`020920` traverses CHAR nodes through the insertion list `50/88`, then WORD
nodes through `80/88`, skipping payload flags `200000`. CHAR AVL in-order
traversal from the preceding stage is intentionally different. The producer
requires the complete actual CHAR insertion list and the current complete
singleton WORD list. It indexes actual CHAR original keys (`header20`),
refusing a used prior range owner; WORD uses its actual ordinary vector
(`header30`) when nonempty, otherwise its original source key (`header20`).

Each `0184E0` interval insertion preserves the raw 16-byte first/last pair.
Equal pairs share an AVL node, but every occurrence allocates a membership
and prepends it to the node's linked list. There is no range union, sorting,
endpoint normalization or membership deduplication. The dirty flag is
committed only after the whole membership loop succeeds. `017EF0` compares
unsigned first then last; `01A520` uses the exact C21.F8 ordered-vector
comparison. Equal vector keys retain the latest actual source and range-header
owner and clear their cached-vector count. The two native AVL rotators
`017DE0/019990` have the same complete 78-instruction algorithm after adjusting
their balance-byte displacement. Balance is right height minus left height.

Snapshots retain typed index/source owners and immutable copied keys. Local
node identities and page creation identities are not native addresses.
Fresh interval nodes use 64 slots per 4096-byte page; membership nodes use
256 slots per 4096-byte page; vector nodes use 73 slots per 4088-byte page.
Native page directories are sorted by allocated addresses and can move their
directory indices. The exposed creation identity plus slot preserves fresh
allocation identity without inventing those address-dependent indices.
Unknown interval cached-bound bytes and native padding are not fabricated.
The separate CHAR and WORD namespaces are retained.

`IsReady` models the native `2DA+1F0` write only after both passes succeed.
Historical immutable snapshots remain readable, while the internal current
consumer gate revalidates source lists, selections and source lifetime.
Portable limits are 65,536 nodes per tree, a 32-entry insertion path, 65,536
ranges per source and 1,048,576 total ranges per index. Native allocator
failure/recycling and reused/prior index state are outside this implementation.
The earlier raw C25 getter boundary is unchanged: C4A wrappers may recursively
refer to other object kinds, so their class alone cannot prove non-C25.

Verification uses four genuine configured raw inputs, including reversed
physical order and a rotated connected M: actual inference produces 12 CHAR
payloads and four WORD payloads. One input's insertion order differs from AVL
in-order order. Independent Python recomputes recursive heights rather than
copying the runtime's iterative stored-balance propagation. Forty separately
prepared numeric cases check all four rotations, exact full tree shapes,
latest equal-vector owners, raw duplicate memberships and all page boundaries
(19,960 nodes and 14,200 memberships). These numeric cases are not claimed as
inferred document sources. Managed and NativeAOT captures are byte-identical;
84 guards cover cancellation, same-root reset attempts, complete WORD-list
ownership, repeat runs, strict CHAR/WORD getters and disposed source owners.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet build tools/LegacyMainDocumentSourceIndices.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/document-source-indices-build
python tools/LegacyMainDocumentSourceIndices.Probe/run.py E:/BlurMask-build-artifacts-20261014/document-source-indices-build/bin/LegacyMainDocumentSourceIndices.Probe/release/LegacyMainDocumentSourceIndices.Probe.dll E:/BlurMask-build-artifacts-20261014/document-source-indices-managed.json
dotnet publish tools/LegacyMainDocumentSourceIndices.Probe -c Release -r win-x64 -p:PublishAot=true -p:StripSymbols=true --artifacts-path E:/BlurMask-build-artifacts-20261014/document-source-indices-native-build -o E:/BlurMask-build-artifacts-20261014/document-source-indices-aot
python tools/LegacyMainDocumentSourceIndices.Probe/run.py E:/BlurMask-build-artifacts-20261014/document-source-indices-aot/LegacyMainDocumentSourceIndices.Probe.exe E:/BlurMask-build-artifacts-20261014/document-source-indices-native.json
python tools/handwriting/audit_main_document_source_indices.py E:/BlurMask-build-artifacts-20261014/document-source-indices-static.json
```

The static audit reads the pinned PE files only. Its optional private research
dependency is not used by the clean-checkout probe or independent output
oracle. No vendor DLL is loaded or executed.
