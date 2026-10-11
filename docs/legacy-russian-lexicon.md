# Legacy Russian lexical backend

`LegacyPackedLexicon` is a reusable, headless loader for the checked type2b4,
version100, flags0 packed trie format. It accepts caller-supplied resource bytes;
no dictionary, weights, vendor code or activation material is bundled. This API
does **not** recognize pen coordinates and is not connected to the application UI.

The main legacy RU LK resource contains two kinds of lexical data. Nineteen
type13a containers hold type23c/version109 graphs for static grammars such as
numbers, dates, email and out-of-lexicon alphabet rules. The principal Russian
word dictionary is instead a type2b4 graph inside a type13b container, alongside
a type2d1 language-cost object. Parsing only type23c does not recover that word
dictionary.

The restored type2b4 payload starts with flags, node count and maximum word
length. A packed array contains alphabet labels and the high terminal bit. The
next byte declares child-offset bit width; a second packed array stores child
counts. A final uint stores terminal count. Packed-array version 0x01000000 uses
an unaligned nine-byte header followed by little-endian 32-bit words. Nodes use
breadth-first numbering: the root's children begin at 1, each following child
range begins after all preceding ranges, and child-count sum is node-count−1.
The reusable loader preserves bit packing for labels and relative child offsets.

Static evidence in the inspected MyScriptText DLL is reader `180248d90`, child
range calculation `180091ce0`, word enumeration `180248730` and type13b reader
`18010c5f0`. Engine packed-array reader `18008f980` supplies the inline array
layout. The checked parent dictionary uses its default terminal-bit convention;
alternate parent terminal encodings, graph flags, aligned array versions and
language-cost formats are unsupported. The loader is bounded to two million
nodes and 256 label positions, rejects truncated data and inconsistent counts,
and validates unique increasing sibling labels before binary-search lookup.

The locally supplied main RU dictionary has 895,571 nodes, 439,826 terminal nodes
and maximum word length 36. Both native field counts and complete object
consumption agree with the independent static parser. Loader-owned packed
storage occupies 2,910,612 bytes, excluding the caller's source buffer and
temporary load buffers. Ten reference lookups pass, including terminal paths
for **мама, привет, кот, дом, мир, тест** and rejection of the nonword **абвгде**.
These inputs are strings mapped to dictionary labels, not handwritten samples.

`LegacyLexicalDecoder` adds an original bounded beam search over caller-supplied
per-glyph label candidates and nonpositive log scores. It filters impossible
prefixes, merges duplicate candidate labels, and returns terminal word paths.
It has no native dependencies, supports cancellation and bounds beam width to 256
and choices per glyph to 512. Input segmentation, mapping acoustic labels to the
lexical alphabet, confidence calibration and language-model costs belong to the
caller. It does not restore cursive segmentation or native SDK decoding.

`LegacyInkAlphabet` restores the embedded type2b version103/104 alphabet payload,
including UTF-8 labels, lookup-table spans, reverse slots and optional triplet
tails. The checked RU acoustic alphabet has 192 slots and 189 text mappings;
the main lexical alphabet has 139 mappings. Independent controls confirm all 328
mappings and exact consumed lengths (5,134 and 2,900 bytes). Callers can translate
AK→LK labels through their matching text; equal numeric IDs do not mean equal
characters across the two resources. Own Cyrillic fixtures check both versions,
truncated payloads, invalid UTF-8 and nonmonotonic string offsets.

Run the vendor-free transition, terminal, bit-crossing, malformed-input and
ambiguous-candidate tests:

```powershell
dotnet run --project tools/LegacyLexicon.Probe -c Release
```

With a locally supplied dictionary and independently prepared controls:

```powershell
dotnet run --project tools/LegacyLexicon.Probe -c Release -- --controls resource.res 609980 controls.json
```

The graph offset identifies the actual nested type2b4 object. A control file has
`{"cases":[{"labels":[94,82,94,82],"terminal":true}]}`. Label IDs depend on the
parent lexical alphabet and must not be copied from the acoustic alphabet.
Managed .NET and Windows Native AOT pass the same checks. Private controls and
static exports remain ignored under `.artifacts/myscript-analysis/output`.

Full Russian handwritten words and joined cursive remain unfinished. The next
required work is calibrated acoustic ensembles, stroke segmentation, baseline
inference, the remaining 313-input features, virtual-label rules and type2d1
language costs; the dictionary loader alone does not establish recognition
accuracy.
