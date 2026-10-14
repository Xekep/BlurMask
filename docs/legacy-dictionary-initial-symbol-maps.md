# Initial dictionary symbol maps and shared contexts

`LegacyDictionaryInitialSymbolMaps.Prepare` restores the normalization-disabled
symbol portion of Text092990, composing it with the recovered shared context
factory and native dictionary reader. It produces initial working50/preserved58,
auxiliary60 and initial contexts68 from the same acoustic/lexical alphabets.
It does not create a language binding, supply descriptor flags, initialize the
complete source factory or restore refresh20d8f0.

The nonvirtual branch initializes auxiliary60 toFFFF. Exact ordinary symbols
leave that value unchanged. Exact ZWNJ/ZWJ copy their acoustic lookup into60;
the post-conversion comparison also copies every primary value at or above the
acoustic alphabet count, including control tokens andFFFF. This differs from
an allFFFF auxiliary map. Initial preserved58 copies working50 before any
permission filtering. Symbol enumeration093af0 consumes58, not filtered50.

The virtual branch initializes primary50 toFFFF but **does not write its
auxiliary60 entry**. Its allocator residue is therefore a required explicit
input for an alphabet with virtual entries. A caller cannot replace those
native unspecified bytes with implicit zero storage. Nonvirtual alphabets
overwrite every entry and need no residue. Context68 skipsFFFF/FFFD, creates
initial root contexts for the other keys and shares exact duplicate keys.

`CreateWorkingMap` connects the three maps to the existing permission/query
consumer. `CreateQuerySource` also connects initial68 contexts to the recovered
eight-slot native reader. `CreateSymbolSource` publishes preserved58 for the
linguistic mask consumer. Public arrays are copies; caller residue and earlier
map snapshots remain unchanged by subsequent permission filtering.

Static evidence: Text092990 at the mapped-symbol branch initializes60 toFFFF,
handles E2808C/E2808D before the ordinary lookup, performs the unsigned
alphabet-count comparison after control conversion, and bypasses auxiliary
initialization for virtual labels. Text093b50 selects60 for auxiliary queries;
093f80 copies58 back to50 before applying permissions. The inspected Text DLL
SHA256 is `18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
No vendor binary is loaded or executed.

Original Python controls specify512 lexical-map cases, including301 virtual
allocation-residue cases, special control tokens, absent mappings, joiners,
distinct Latin/Cyrillic labels, permission filtering and shared context keys.
The actual Russian resources have139 lexical entries:138 acoustic mappings
and the space controlFFFB. Auxiliary60 has exactly two nonFFFF values: joiner
184 andFFFB. The actual shared context factory produces139 nodes. The owned
maps feed132 ordinary root dictionary records and zero auxiliary root records.
That last count follows the actual terminal/map gate; it is not a general
claim that later auxiliary queries cannot return records.

Managed Release and Windows NativeAOT verify these controls. The older exact
map helper remains unchanged. General normalization fallback, foreign resource
factories, full controller scheduling and Russian handwriting accuracy remain
outside this producer.

```powershell
python tools/LegacyDictionarySymbolPreparation.Probe/reference_controls.py .artifacts/legacy-dictionary-symbol-controls.json
dotnet run --project tools/LegacyDictionarySymbolPreparation.Probe -c Release -- .artifacts/legacy-dictionary-symbol-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res
```
