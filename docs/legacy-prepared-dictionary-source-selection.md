# Prepared dictionary source selection

`LegacyPreparedDictionarySourceSelectionBuilder` ports the bounded, already-prepared child-descriptor part of native `236020` / `236200` / `236490` / `236370`. It produces ordered rows for the aggregate dictionary dispatcher. It does not create child sources, run their preparation hooks, decide their eligibility, or implement the native word winner.

## Proven producer and storage

The static evidence is from MyScriptText.dll SHA-256 `18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`. `236020` appends a source and its selection row, rejecting a 256th source. `236200` removes the matching source and row and decreases the indices of later sources. This port supports fresh selection of all added sources. Arbitrary previously selected subsets are outside its scope.

`236490` first prepares every child, then gets each selected child's descriptor. That upstream preparation is a precondition of the typed API. For descriptor category `Flags & 0x1e00` equal to `0x800` or `0xc00`, a present language model supplies its `virtual40` value; absent models and other categories supply priority 1. It resets each selected row's accepted skip base to `0xffffffff` and sorts unsigned priority descending, then unsigned source index descending. The port retains the exact midpoint quicksort comparison and moves the complete immutable row.

The language resource `2d1` vtable `2babb0 + 0x40` resolves to `0fe000`, which reads `*(uint32*)runtime[0x78]`. Reader `0fcc10` stores language order at `0x60`, word count at `0x70`, class count at `0x74`, and an array of stored n-gram counts at `0x78`. The value is the first array entry, not the language order or word count. In the checked Russian order-two/flags-three resource, it equals class count **1024**; the order is 2 and word count is 439826. `FromObservedOrderTwoModel` uses the strictly loaded model's class count only for this established order-two binding. Other prepared inputs must supply their observed first stored count explicitly.

After sorting, `236490` sums descriptor `Weight24` using binary32 arithmetic in selected order. Only when the sum is greater than 1 does it divide each `Weight24` by that sum. `Weight28`, flags, order, identifier, runtime scale, and context ownership are retained. The portable implementation returns immutable descriptor copies and does not mutate the caller's input. Repeated preparation starts from those original input descriptors.

## Composition contract and boundaries

An input carries a prepared descriptor, nullable first stored n-gram count, and explicit portable context-owner identity. An output carries the original/reindexed source index, priority, reset skip base, descriptor, and context owner. The caller uses `SourceIndex` to retrieve the corresponding child reader; the output list order supplies the aggregate selected ordinal. These two indices are different. Context identities are caller-owned tokens, not invented native pointer values.

The API rejects repeated references to the same descriptor object. Native descriptors are mutable; two aliases can be normalized repeatedly through the same pointer. That alias behavior is outside this immutable distinct-owner port. Equal but separately owned descriptors are supported. Weight bounds reflect the signed fixed-point descriptor encoding and language-presence/order flags must agree. Cancellation returns no partial selected result and leaves inputs unchanged. No default controller flag, guessed Unicode eligibility filter, or invented child preparation success is introduced.

## Reproducible controls

`tools/LegacySourceSelection.Probe/reference_controls.py` independently constructs 384 add/remove traces with 9,216 preparation snapshots. Its list ordering oracle uses Python's tuple sort rather than the native quicksort; weight arithmetic explicitly rounds each operation to binary32. Controls include unsigned priority extremes, equal-priority index ties, category changes, negative and near-unity weights, source removal/reindexing, metadata/owner retention, and input immutability. The managed and Windows NativeAOT probe also check 255 sources, duplicate descriptor aliases, malformed values, cancellation, and exact unity.

The opt-in real-resource path loads hash-checked Russian acoustic/lexical resources and constructs the proven `0xc0a` descriptor and exact 139-key initial context map. It observes priority 1024, weights 0/1, shared context ownership, two distinct descriptor owners, and removal reindexing. It does not claim child-source factory, aggregate scheduling, automatic Russian recognition, or word accuracy.

```powershell
python tools/LegacySourceSelection.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/source-selection-controls.json
dotnet run --project tools/LegacySourceSelection.Probe -c Release -- D:/BlurMask-build-artifacts-20261011/source-selection-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
dotnet publish tools/LegacySourceSelection.Probe -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/source-selection-aot
& D:/BlurMask-build-artifacts-20261011/source-selection-aot/LegacySourceSelection.Probe.exe D:/BlurMask-build-artifacts-20261011/source-selection-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
```
