# Portable native symbol-context factory

`LegacySymbolContextFactory` restores the bounded shared symbol index used by the native dictionary query path. It composes ushort AVL lookup/insertion (`1800918d0`), single/double rotations (`1800917c0`),102-node page allocation (`180091b30`) and the initial dictionary source68 map written by `092990`.

The inspected Text SHA-256 is `18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`. Static vendor exports are investigation evidence only; the executable library/probe does not load or execute a vendor DLL.

## Identity and native field ownership

Portable identities are opaque nonzero monotonic tokens, globally distinct across live factory instances. Zero is the native null-data counterpart. These tokens do not reproduce, encode or dereference native pointer addresses.

Each live node exposes the verified symbolic fields of the native40-byte record:

| Native offset | Portable field |
| --- | --- |
|00|`ParentLink`, the previous symbol in the context path|
|08|`ChildTreeRoot`, root of this parent's next-symbol AVL tree|
|10/18|`Left`/`Right`, structural AVL child identities|
|20|Unsigned ushort `Symbol` key|
|22|Signed balance: right height minus left height|

The factory also tracks page/slot and tree ownership as independent bookkeeping, without presenting those extra fields as native record bytes. AVL rotations change structural left/right links and balances; they never rewrite symbol-path parent links or child-tree ownership.

`ResolveInitial(key)` indexes the shared root symbol tree. `ResolveChild(parent,key)` indexes the separate tree at that parent+8 and reports whether the symbol was created. `LinkCreatedContext(child,parent)` applies the native caller's separate write of node00 after successful creation. It validates that the child belongs to that parent's tree. Equal symbols under different parents receive different node identities; repeated lookup under the same parent reuses the same identity.

## Exact AVL insertion and rotations

Native lookup compares unsigned16-bit keys. Existing keys return the same node with Created=false and do not allocate or rebalance. The port records the search path and performs the equivalent bottom-up balance update until height stops changing or the last nonzero pivot rotates. The native search uses32 direction bits; the bounded index refuses paths beyond that width.

`0917c0` supplies the exact balance assignments for LL/RR and LR/RL rotations, including its zero-child-balance assignments. The ordinary insertion controls exercise all reachable insertion rotations. They do not claim to exercise a deletion algorithm or to synthesize otherwise unreachable zero-child-balance states; deletion is not implemented.

## Page allocator and snapshot restoration

`091b30` allocates4080-byte pages with102 node positions of40 bytes. It first uses the active page if space remains, otherwise scans existing pages from last tofirst before adding one. Page-descriptor capacity grows to max(new count,trunc(old capacity×1.5)). Page/slot assignment is retained in node diagnostics.

The portable node bound is1..1,000,000, default131,072. When the configured allocation bound is reached, a new lookup returns identity0/Created=false without mutation; an existing-key lookup still succeeds. This models explicit allocation failure data, rather than inventing a context or marking recognition accepted.

`Capture` represents the saved root, node fields, page used counts and allocator cursor written by the context snapshot branch in `20d8f0`. `Restore` applies the symbolic counterpart of `1f6e50`: restore the saved tree/page contents, discard nodes/pages beyond that checkpoint and retain the current descriptor capacity. Saved node identities and an initial source map referencing them survive restoration. Identities allocated after capture become stale and are rejected; future reuse of the same physical page slot gets a fresh portable token. This deliberate token-safety boundary does not assert that native allocator addresses change on reuse.

Snapshots are opaque, immutable to public callers and restricted to their owning container. Cancellation is checked before any insertion commits; a committed bounded insertion completes rebalancing without interruption. Capture/restore construct their temporary copies before committing, so canceled restoration preserves the previous live tree. A canceled map batch returns no map but may retain its earlier completed insertions, matching the component's explicitly nontransactional batching boundary. Callers serialize access.

## Initial source68 map and query callbacks

`CreateInitialSourceMap(exactDictionaryMap)` follows `092990`: preserve each lexical position's mapped acoustic/control ushort; skipFFFF/FFFD; obtain a root-tree node for every other key; write a zero parent link for newly initialized root nodes. Duplicate mapped keys share context identities. No Latin/Cyrillic aliases, language probability or dictionary state is inferred.

The shared factory connects to the existing query API without modifying that API:

```csharp
var factory = new LegacySymbolContextFactory();
ulong[] initialContexts = factory.CreateInitialSourceMap(exactDictionaryMap);
var source = new LegacyPreparedDictionaryQuerySource(lexicon, workingMap, initialContexts);
var query = new LegacyPreparedDictionaryQuery
{
    Node = preparedNode,
    Capacity = 128,
    ContextRoot = factory.ContainerIdentity,
    ParentContext = preparedParentContext
};
var page = source.Read(query, auxiliary: false,
    resolveContext: (parent, symbol) => factory.ResolveChild(parent, symbol),
    linkCreatedContext: factory.LinkCreatedContext);
```

`preparedNode` and `preparedParentContext` must come from the verified caller/source state. Initial queries use ParentContext0 and the initial map. Continuations use the matching factory's parent symbol node. Neither this binding nor a direct prepared child query invents a native parent scheduler, source eligibility or application controller state.

## Independent and actual-model verification

The original Python oracle computes tree heights recursively and balances by height, independently of the port's stored-balance/path update. It covers every insertion permutation of up to7 distinct keys, duplicate lookups, unsigned key boundaries0/32768/65535 and256 randomized64-element sequences.

Managed and Windows NativeAOT pass 6170 tree references and 52,793 exact node/left/right/balance checks. Additional controls verify independent parent trees, stable duplicate identities, unchanged parent links after rotations, exact initial-map skip/shared-key rules,102-node page boundaries, partial-page recycling after restore, retained descriptor capacity, stale/foreign identities, allocation-bound failure and cancellation.

The opt-in pinned Russian model probe obtains the actual exact lexical→acoustic/control map. It produces 139 root symbol contexts for 139 mapped lexical positions, using two pages. The restored packed node0 query returns 132 records in two128-capacity pages, each with its actual nonzero mapped-symbol context. A real child query at node4 creates one next-symbol context under its actual parent, verifies the node00 parent link and duplicate lookup identity, and reaches 140 total contexts. These are resource identity/query transport checks, not handwriting accuracy or a native word-startup result.

```powershell
python tools/LegacySymbolContext.Probe/reference_controls.py D:/symbol-controls.json
dotnet run --project tools/LegacySymbolContext.Probe -c Release -- D:/symbol-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
```

Omit the three model arguments for independent controls only. The native aggregate's selected-source/eligibility producer, context path rewrites `1f6fb0`/`1f7080`, real word-parent/row scheduling, winner and acceptance confidence remain separate unfinished stages. No binary assets or vendor code are added by this batch.
