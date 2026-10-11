# Legacy dictionary terminal word IDs

`LegacyLexiconWordIndex` derives zero-based terminal preorder IDs from a loaded,
checked type2b4 dictionary tree. This is a vocabulary index, not recognition or
a language scorer. Breadth-first graph node numbers are not word IDs.

Native `180248be0` builds the graph's packed ordinal table at object+0x30.
Recursive helper `1802489e0` traverses each parent's ordered children, stores the
current terminal counter, increments it for a terminal prefix, descends, and
passes the resulting counter to the next sibling. Native iterator `180248730`
independently enumerates the same terminal-before-descendants order. The new
component preserves that order without recursion, bounds depth by the declared
word length, supports cancellation and verifies complete node/terminal coverage.

`TryGetWordId(labels, out id)` follows caller lexical-alphabet labels. The state
overload accepts a validated graph state. Root, nonterminal prefixes and absent
words have no terminal word ID. Alphabet aliasing, other graph encodings and
runtime vocabulary permutations are outside this API.

Vendor-free controls use an original tree containing terminal prefixes and
nonterminal prefixes; preorder IDs deliberately differ from BFS state IDs.
Invalid states, empty/absent words, incorrect declared depth and cancellation
are checked:

```powershell
dotnet run --project tools/LegacyWordIndex.Probe -c Release
```

On the inspected RU dictionary the component enumerates all 439,826 terminals.
Seven independently generated LIFO DFS references pass through both managed
.NET and Windows Native AOT. These then index the raw type2d1 tables via
[`LegacyWordLanguageModel`](legacy-russian-word-language.md):

| Stored dictionary spelling | Terminal word ID | Raw word cost | Class ID |
| --- | --- | --- | --- |
| мама | 197565 | 9851 | 643 |
| мир | 198893 | 9851 | 435 |
| миф | 200417 | 30643 | 568 |

These raw cost differences are evidence of distinct word-level language data,
not a handwritten-word accuracy result. Profile weighting/offsets, word history,
runtime permutations and acoustic cost combination still need their native
rules; no fixture-specific reordering is implemented.

The component retains 3,582,284 bytes for the inspected dictionary's state index,
excluding graph storage, source buffers and temporary traversal buffers. This is
not application memory usage. A caller can reuse the index across requests.

Independent actual controls require a caller-supplied resource. Generated
resource-derived control output remains private:

```powershell
python tools/LegacyWordIndex.Probe/reference_controls.py models/ru-RU/ru_RU-lk-text.lite.res .artifacts/private-word-index-controls.json
dotnet run --project tools/LegacyWordIndex.Probe -c Release -- models/ru-RU/ru_RU-lk-text.lite.res .artifacts/private-word-index-controls.json
```

Vendor-free tests need no resource. The Russian models under `models/ru-RU` were
added separately at the user's explicit request; this index/probe does not run
their native engine or any activation/authentication code.
