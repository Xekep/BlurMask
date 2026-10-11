# Default Unicode service and bounded acoustic symbol-range filter

## Proven default global-context binding

Static analysis of MyScriptEngine SHA256
`d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c`
establishes the default chain:

```text
Engine180031910
  service descriptor pointer at 180252200 -> 180202b00
  descriptor +20 -> vtable 180202a70
  writes descriptor +20 to (engine.context & ~1) +78
  vtable +8 -> Engine18005ed00
```

The initializer accepts an optional service-provider callback. The evidence
applies to the default service; a caller can install a different descriptor.
The pair producer Engine180058da0 calls precisely `global+78, method+8`.

Engine18005ed00 is a direct Unicode decomposition lookup, not a learned
Latin/Cyrillic confusability model. Its data pointer at `1802521f0` refers to
`1801df3e0`; the sorted index pointer at `1802521f8` refers to `1801f2bf0`.
The binary-search exclusive bound `0x165A` is **5722 decimal**. An early
5660-record diagnostic accidentally omitted the last 62 canonical CJK records;
the complete audit corrects that count.

Each indexed record contains an unsigned packed header, original UTF32 scalar
and direct UTF32 decomposition. The low header byte is the component count;
`header >> 8` is the decomposition kind. The entire set of **5722** scalars,
kinds and component sequences exactly equals the public
[Unicode 9.0 UnicodeData.txt](https://www.unicode.org/Public/9.0.0/ucd/UnicodeData.txt).
Its SHA256 is
`68dfc414d28257b9b5d6ddbb8b466c768c00ebdf6cbf7784364a9b6cad55ee8f`.
The [Unicode property definitions](https://www.unicode.org/reports/tr44/#Character_Decomposition_Mappings)
explain the decomposition tags. This establishes the decomposition data
version; other service methods have not all been matched to this UCD release.

| Kind | Unicode decomposition tag | Record count |
| --- | --- | --- |
| `0x1` | canonical | 2060 |
| `0x2` | font | 1184 |
| `0x4` | noBreak | 5 |
| `0x8` | initial | 171 |
| `0x10` | medial | 82 |
| `0x20` | final | 240 |
| `0x40` | isolated | 238 |
| `0x80` | circle | 240 |
| `0x100` | super | 152 |
| `0x200` | sub | 38 |
| `0x400` | vertical | 35 |
| `0x800` | wide | 104 |
| `0x1000` | narrow | 122 |
| `0x2000` | small | 26 |
| `0x4000` | square | 285 |
| `0x8000` | fraction | 20 |
| `0x10000` | compat | 720 |

Two service-specific exceptions decline U+037E and U+0387 even though their
canonical records exist in the table. Hangul U+AC00..U+D7A3 is decomposed
algorithmically into L/V and optional T with kind 1. The service produces one
direct decomposition; it does not recursively normalize arbitrary text.

The caller's `kind == 0x800` gate selects only **wide** decompositions. All 104
are single-component and exactly match the constructive mapping used by
`LegacyWideSymbolPairs`: U+3000→space, U+FF01..FF5E→ASCII, U+FF5F/FF60→2985/2986,
and U+FFE0..FFE6→their seven currency/symbol bases. There are no Cyrillic targets
in that selected set. The ordered uint32 little-endian `(original,base)` pairs
hash to `ee6e71209527db0ffea3770d54d1ecff1839af62e29bc7ee0e65061e22fe3a47`.

Latin `m/p` and Cyrillic `м/р` have no direct decomposition. Russian `Ё/ё/Й/й`
have canonical base-plus-combining mappings with kind 1; they do not pass the
wide gate. NBSP has kind 4; micro sign→Greek mu has kind 0x10000. The observed
RU acoustic alphabet has no kind-0x800 labels, so this path supplies no default
RU `(base,wide)` cache pairs. It cannot justify replacing Russian letters with
visually similar Latin ones.

## Executable ordinal category-range stage

`LegacyAcousticSymbolRangeFilter` ports the bounded Text1801b95a0 loop after
Text1801b8eb0's mask production. Text1801be930 supplies working masks from
runtime+2d8/+2e0 and the explicit byte category argument; Text1801bf0a0 also
calls the same helper. The two packed bounds arrays are runtime+148/+150.

For every symbol active in either mask, the helper reads the minimum and
maximum fields. A nonzero minimum rejects a smaller current category; a nonzero
maximum rejects a larger one. Rejection clears **both** masks. Equality is
accepted, zero bounds are unconstrained, and missing either bounds array
disables this filter. Values already in accepted masks retain their original
bytes, including non-boolean nonzero values.

`DecodePackedFields` restores little-endian uint32 bit fields, including values
crossing a word boundary. This bounded API supports explicit 1..8-bit category
fields and at most 65535 symbols. It does not infer a serialized object header.
Argument/storage/alias/cancellation checks and the returned cleared-position
count are original defensive/diagnostic behavior. Bounds cannot alias mutable
masks. Cancellation during an in-place operation can leave a partial result.

Further static analysis resolves this byte as a **source-stroke count**, rather
than a script or letter-case category. Text1801b4e40 resets its output byte, walks
the selected prepared ranges, and increments it only when the corresponding
source record's first int32 identifier has not occurred in an earlier selected
range. Several fragments from the same source stroke therefore count once.
The byte has native modulo-256 arithmetic. Text1801b5cf0 first uses that same
producer, then adds one for each synthetic prefix-template stroke it appends.
The resulting byte reaches Text1801be930 argument 10 and then Text1801b95a0.
It must not be guessed from character identity or replaced by a language enum.

The resource producer is Text1801c2940. For versions at most 0x110 it writes
NULL to both resource+148/+150. For a newer version it reads one presence byte;
zero again writes both NULL, while nonzero calls global-context+98 service
slot18 twice to load the packed minimum/maximum fields. Text1801b0560 copies
resource bytes [0x8,0x158) to the same runtime offsets, including these pointers;
Text1801b0660 performs the equivalent 0x150-byte body copy from an explicit
configuration. The actual Russian ordinary acoustic node at resource offset
189734 is version **0x110**: these bounds are absent and this filter is disabled
for that node. It cannot explain or fix Latin/Cyrillic candidate preference in
the current Russian diagnostic.

The helper still requires supplied bounds and a count. Full newer-version
packed-object decoding and upstream language/case mask routing are separate
unresolved stages; no example-fitted Latin-to-Cyrillic substitution is inferred
from this recovered count constraint.

## Reproduction and verification

The auditor only reads the pinned PE bytes and official public Unicode data;
it never loads or executes SDK DLLs. No full engine Unicode table or new model
asset is distributed by this batch.

```powershell
python tools/handwriting/audit_legacy_character_properties.py `
  .artifacts/myscript-analysis/sdk/build/x64/MyScriptEngine.dll `
  D:/UnicodeData-9.0.0.txt D:/unicode-service-report.json
python tools/LegacySymbolRange.Probe/reference_controls.py D:/symbol-range.json
dotnet run --project tools/LegacySymbolRange.Probe -- D:/symbol-range.json
```

The static audit checks all 5722 public/native records, default binding and all
104 constructive wide mappings. The mask probe passes **408 independent own-data
controls** in managed and Windows NativeAOT builds. A separate byte-bitstream
reference generates packed words; coverage includes all eight widths, field
straddles, non-boolean masks, zero constraints, strict threshold neighbors,
equality and defensive guards. Native vendor code was not executed, so these
checks establish recovered arithmetic and public-data agreement rather than
runtime oracle parity or overall Russian recognition quality.
