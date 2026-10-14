# Native first dictionary-word completion

This batch connects a bounded regular first-word branch of Text `204290` to
the actual source70 word metadata, source50 restart tag, language profiles,
full40-byte language result and existing `1f9080` cache writer. It is not the
complete language controller, later-word `203690` context enumeration,
`2144f0` scheduling, a selected final winner, or calibrated acceptance confidence.

## Fresh default history and source ownership

Type2cd descriptor `1802ba7f0` field48 points to runtime initializer `1800f0260`.
This is distinct from raw-table initializer `1800f02d0`. Runtime `0f0260` calls
its138 parent, clears60, writes a qword ofFFFFFFFF at **decimal100/hex64**
(covering64 and68), and writesFFFFFFFF at6C. Type2d1 runtime initializer
`0f0ad0` invokes that2cd initializer through global class slot20. Checked base
version100 reader `0f0310` does not overwrite64. Dictionary preparation
`0fe2a0` reads64 but does not assign it. Thus fresh loaded order2/flags3 resources
have default class64=FFFFFFFF; `100090` request00=1 does not invent preceding
word history in this scope. Configured defaults, clone mutation and other base
versions are separate paths.

Dictionary `2b7.virtual70=094040` returns two explicitly cleared qwords. It
loads the packed node-to-terminal ordinal into output4, stores its explicit
boundary argument at byte0C, and source tag bit42 at byte0D. `2489e0` writes
terminal preorder ordinals before descending into children. The existing
`LegacyLexiconWordIndex` implements that proven traversal and retains dictionary
identity. `LegacyPreparedDictionaryWordMetadata` binds to that same dictionary
and index and accepts terminal nodes only. Nonterminal packed entries can name
a later terminal and are not silently converted into known words here.

The completion constructor takes the selected owned bundle and derives its
acoustic alphabet and main sampler category mapper. Metadata must reference
that bundle's dictionary and word index, and the language record must reference
its language model and source-kind-selected cost and auxiliary profiles. Equal
word counts, identical coefficients, or an independently reloaded copy of the
same bytes do not establish that source70 ordinal ownership. The generic numeric
language-record API still accepts explicit caller-selected profiles; it cannot
be used in this completion bridge with a foreign profile. Restart tags are an
explicit value-type snapshot, including legitimate configured values, rather
than a nullable service. The selected aggregate child source ID and prepared
C00 header remain explicit owning caller inputs; the full native aggregate
source-descriptor object/initialization binding is not inferred by this API.

## Actual language flags and result record

`100090` forwards request01/knowledgeF5 to `0ffcf0`'s **clear first history
category** argument, and request25/source70byte0D to its separate **suppress
category cost** argument. Request24/source70byte0C controls the additional
boundary cost. These flags are not interchangeable.

`LegacyNativeKnownWordLanguageRecord` owns a matched model/profile/auxiliary
scorer. It uses the already checked order2/flags3 numeric paths, then produces
the full40-byte output for the no-vocabulary-map general branch. The temporary
heap residue is an explicit complete input. Only assigned bytes are changed:

- `00`: accepted lexical integer cost;
- `04`: numeric cost-order/history count1 or2;
- `08`: one-byte updated class-history count1;
- `0C`: actual current class low24 bits and the category nibble;
- `18`:FFFFFFFF vocabulary state;
- `1C`: actual source word ordinal.

The distinction at18/1C matters. Flags3 omit optional vocabulary maps80/90,
so `100090` uses the model's WordCount sentinel for its scratch vocabulary ID.
`0ffec0` converts that sentinel toFFFFFFFF at payload10/record18. However,
the general branch writes its source ordinal `local124` to record1C. The fast
mapped-vocabulary branch writes a different field and is not used as a template.
Bytes outside these assignments, including spare history slots and trace
storage20..27, retain the supplied residue. There is no guessed zero allocation.

For order2, class history slides to the current class. With source42 clear,
`0ffec0` appends the current category nibble. With source42 set, it carries the
proven tagged prior prefix, if one remains after knowledgeF5 clearing. The
language result obeys budget equality and rejects lexical cost04000000h;
numeric overflow remains the existing checked scalar API's explicit supported
domain rather than an unbounded native wrapping claim.

## Producer-to-cache bridge

`CreateAlreadyTerminalQuery` restores `204290`'s `local318` cache injection:
record5.bit1 set and record7.bit0 clear select the current terminal. It copies
the actual source tag and current symbol, stores context/cost/increment metadata
zero, and preserves unassigned query24..31 from the supplied cache residue.

`LegacyNativeFirstDictionaryWordCompletion` accepts the actual selected C00h
source header, owned stored word row and query. It resolves the materialized
anchor through that row's native38/3C pair and requires the word to start at
source node0. Current category uses the existing actual sampler mapper; the
starting category supplies the checked8192 correction for categories2/4.
No category is inferred from Unicode or repaired between Latin and Cyrillic.

The live language budget comes from `router.GetPrimaryHeap(sourceId)` and
therefore uses the same actual bucket owner as subsequent insertion. The new
router method returns its existing primary heap; it excludes optional override
and secondary routing and does not create, replace or resize storage. There
is no second caller-provided heap that can supply a foreign budget.

The consumer adds the actual row/query/correction costs, calls source70 and the
resource-selected language model, obtains source50's complete restart tag,
and installs query kind into tag header bits4..6. It passes the resulting
20-byte history payload, stateFFFF, increment14 and computed record28 to
the existing1f9080 producer. The initialized auxiliary pair5C is the literal
204290(-1,04000000h) pair, not a fabricated prior context. The parent/query
inputs are snapshotted before callbacks, callback cancellation is checked, and
exceptions invalidate serialized reuse.

Supported scope is explicit: frontend280=false, nontracing, mode22f6=false,
knowledge.ec<=0, same selected C00h dictionary source, plain symbols, first
word/root anchor, and fresh checked language defaults. Unsupported modes are
refused in the constructor. Auxiliary source48 pagination, later-word history
selection/recombination, source case policy and full fresh main geometry remain
the next controller stages; callers must not label this partial stage a complete
recognizer or invoke it for the language-bypass path.

## Verification

The independent oracle builds on original small language tables and binary32
numeric calculations.640 controls compare all numeric fields and all40 result
bytes, including preserved residue, source42/knowledgeF5 distinctions, history
updates and lexical-budget equality.512 independent controls compare terminal
injection gates and complete32-byte cache assignments. Eight structural and
27 actual consumer guards cover source/anchor extents, cancellation and reuse,
unsupported modes, input mutation during callbacks, foreign full-heap isolation,
actual owner budget rejection, invalid source/bucket lookup, null services and
same-sized foreign dictionaries/indexes/models/profiles/auxiliary data.

The actual resource fixture carries11 NN-derived native parent rows through
the current-terminal producer, source70, the proven source0x135/profile2
language path,0ffec0 and1f9080. It checks real class IDs, source ordinals,
stateFFFF, computed restart tags and payload ownership. The input frame is
still explicitly diagnostic; no full204290 scheduling or winner is claimed.

```powershell
python tools/LegacyMainWordLanguage.Probe/reference_controls.py .artifacts/main-word-language-controls.json
dotnet run --project tools/LegacyMainWordLanguage.Probe -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/main-language-continuation-managed-artifacts -- .artifacts/main-word-language-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

The same probe supports Windows NativeAOT. Vendor binaries are inspected
statically and are not executed.
