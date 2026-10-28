# Native word context composition and root alternative creation

This batch connects the root branch of Text `18020e6a0` to the existing owned alternative pools, duplicate merges, and stored-row materializer. It does not implement the stored-parent branches of that function, continuation alternatives (`180211eb0`), automatic callback selection, ordinal allocation, or winner/confidence selection.

## Symbol paths: `1801f7e30` and `1801f7ff0`

`LegacyNativeWordContextComposition` uses the same `LegacySymbolContextFactory` that implements the `0918d0` ushort AVL index and its node allocator. Stable managed identities identify nodes in that factory; they are not native pointers or fabricated addresses.

Both functions first recurse through `suffix->parent` at node offset `00`, then process its ushort symbol at `20`. The prefix is the recursion's base result. The C# implementation captures that parent lineage and processes it in reverse, preserving the recursion's order without sorting identities or modifying the source chain. A checked 4096-node path bound refuses larger inputs. Native recursion has no recovered equivalent limit; this is a supported subset.

Mapped `1f7e30` invokes the explicitly supplied controller `2360` callback on ordinary insertions. Raw `1f7ff0` inserts the original symbol. With an empty prefix, mapped composition skips `FFF7..FFFC`; raw composition inserts them. An incoming `FFFD` preserves a nonempty prefix, but an empty prefix still inserts it (mapped after the callback). Callback results are inserted directly: a callback returning `FFFD` for an ordinary input does not trigger the incoming-control skip.

When both the preceding output symbol and incoming symbol are in `FFF7..FFFC`, the native table drives replacement:

| Previous / incoming | FFFC | FFFB | FFFA | FFF9 | FFF8 | FFF7 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| FFFC | -1 | -2 | -1 | -2 | 3 | 3 |
| FFFB | -1 | -1 | -1 | -2 | 3 | 3 |
| FFFA | -2 | -2 | -1 | -2 | 3 | 3 |
| FFF9 | -1 | -1 | -1 | -1 | -1 | -1 |
| FFF8 | 3 | 3 | 3 | -2 | 3 | 3 |
| FFF7 | 3 | 3 | 3 | -2 | 3 | 3 |

`-1` keeps the previous node. `-2` pops it, then inserts the incoming symbol. `3` pops it and inserts `ushort(-(short)3 - 4)`, i.e. `FFF9`. These branches bypass the mapping callback. The native guard accepts six descending symbols (`FFFC - symbol <= 5`, unsigned). The address expression is `1804a7b60 - 4 * (incoming + previous * 6)`; its first listed cell is `1802e7bd0`. The 36 signed int cells, serialized in this table's row-major order, have SHA-256 `ad45c41cff7e88ad53dac9d8c09ba8cd86d89eeeb18d0fa835432024e71ac9a6`.

New child nodes receive the actual path parent through `LinkCreatedContext`; the AVL left/right and child-tree linkage are separate. An allocation-bound failure returns identity zero and traversal continues. A later token can resolve an existing root after that intermediate zero. Foreign/stale identities are rejected. The factory requires serialized access, and a callback must not mutate its source lineage. Source paths must have their actual native-style parent links; the composer does not repair caller-created orphan nodes.

The four `2360` callback variants are selected by `2144f0`, including `1f8150`, `1f81d0`, `1f8290`, and `1f8300`. This batch requires that selection/mapping explicitly. `1f8300` is confirmed by disassembly as `movzx eax, dx; ret` (identity mapping). No default callback is chosen for a resource or recognition session.

## Root alternatives: `18020e6a0`

`LegacyNativeRootWordAlternativeProducer.Create` accepts complete 128-byte heap storage, active count, unsigned current node, explicit prepared controller configuration, the shared context factory, mapping callback, and preexisting primary/secondary pool bytes. Pool bytes are owned copies, preserving their prior contents. It does not invent zero initialization or alternative ordinals.

For each active slot, the native predecessor is `unchecked(currentNode - (uint)(int)(sbyte)slot20 - 1)`. Only predecessor `FFFFFFFF` is supported. All root/ordinal checks precede changes to the shared context index. Signed span arithmetic is preserved, including the negative-byte / unsigned-node wrap case. Slot `58` is the caller-supplied signed alternative ordinal. The checked alternative count range is 1..16, covering the real fresh `20baa0` defaults 2/2 without claiming an SDK maximum. When both counts are one, the native creation branch is disabled: no context callback or root-parent processing occurs.

The root calls `1f7e30(prefix=0, suffix=slot08)` and `1f7ff0(prefix=0, suffix=slot08)`. For each enabled pool's first 40-byte alternative:

- `00/04` copy the actual slot cost/increment at `10/14`.
- `08/10` store the canonical mapped/raw context identities.
- `18` copies the previous arc ushort from slot `1e`.
- `1a` copies the span byte from slot `20`.
- `1e` copies the count byte from slot `26`.
- `1f` receives the low five state bits from slot `21`. Primary preserves high two bits and sets bit five; secondary preserves high three bits, including its existing bit five.

Remaining alternatives receive cost and increment `04000000`, and count byte `1e = 0`. Their other bytes remain unchanged. First-alternative byte `1b`, bytes `1c..1d`, dword `20`, trailing bytes `24..27`, untouched rows, and inactive heap bytes retain their explicit provenance. The 128-byte heap slots are never changed. Repeated ordinals preserve native iteration order: the later active slot overwrites the same pool row. Trace mode is rejected; no 640-byte trace clones are synthesized.

## Verification

`tools/LegacyWordContextAlternatives.Probe/reference_controls.py` is an independent functional tuple-path oracle and byte-slice pool writer. It generates:

- 1024 symbolic controls, including all 36 control pairs in both raw and mapped modes; checks exact callback invocations and canonical repeat identity.
- 256 complete pool controls with counts 1/2/3/16, random reserved and inactive bytes, shuffled/repeated ordinals, differing flag bits, empty/multinode suffixes, and signed-span wrapping.

The probe also checks null allocation/recovery, foreign IDs, pre-cancellation, disabled creation, unsupported stored-parent scope, and trace rejection. Expected symbolic paths are independently constructed in the same index before comparing whole pool bytes; they are not derived from the output being tested.

An actual RU resource integration classifies the existing owned MIR ink with the restored prepared evaluator, final acoustic cache, and category scorer. It routes the accepted records through the native heaps, inserts one explicitly diagnostic duplicate, assigns explicit ordinals, and supplies actual interned symbol contexts. `1f8300` is selected explicitly for this fixture. The new root producer creates 7 default2/2 pool rows; the existing native duplicate merge retains 6 records, and the existing materializer transports their complete 80-byte primary and secondary alternatives into stored rows. This connects the producer to its consumers with real NN/cache costs. It does not claim that fixture enumeration, source context selection, ordinal assignment, or callback selection reproduces the full native controller.

Commands from the repository root:

```powershell
python tools/LegacyWordContextAlternatives.Probe/reference_controls.py .artifacts/legacy-word-context-alternatives-controls.json
dotnet run --project tools/LegacyWordContextAlternatives.Probe -c Release -- .artifacts/legacy-word-context-alternatives-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
dotnet publish tools/LegacyWordContextAlternatives.Probe/LegacyWordContextAlternatives.Probe.csproj -c Release -r win-x64 -p:PublishAot=true --artifacts-path D:\BlurMask-build-artifacts-20261011\word-context-alternatives-artifacts -o D:\BlurMask-build-artifacts-20261011\word-context-alternatives-native-bin
```

Both managed and Windows NativeAOT runs must use the same five data arguments. No GUI, installer, native vendor DLL, or Explorer process is executed.

Russian cursive recognition still lacks the complete native search/winner/acceptance controller. These APIs do not expose a guessed recognition confidence or enable production replacement in AI-pero.
