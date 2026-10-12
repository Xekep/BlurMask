# First acoustic-only records and ordinary recurrence

This is a scoped port of the first, no-linguistic-source controller **23f**, not
the dictionary controller240. It is currently a library/headless diagnostic,
not a completed Russian UI recognizer or the SDK's final whole-text winner.

Static provenance: `MyScriptText.dll` SHA256
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
The binary is read, never executed. The reusable code contains no vendor DLL.

## Record insertion

`LegacyFirstAcousticRecordTable` ports **1ee030** over owned40-byte slots:

| Offset | Meaning |
| --- | --- |
| 00/04 | signed total/acoustic costs |
| 08 | likelihood byte, filled separately by1ef070 |
| 09/0a | previous alternative / later continuation-table selector |
| 0c | acoustic symbol ID |
| 0e | bit0 path choice, bits1..3 span, bits4..9 composite category |
| 10 | source graph arc ordinal |
| 18 | opaque symbol-context identity |
| 20 | five Arabic contextual traits; upper three bits retained |

With a nonzero identity, insertion scans active records backwards. An existing
cost no greater than the new cost keeps the old record. A lower cost replaces
that slot **in place without sorting it again**. Without a matching identity,
signed-cost insertion preserves equal-cost order and clips to the supplied
capacity. The packed upper bits and unwritten bytes retain destination data.
The separate640-byte diagnostic buffers are not represented.

## Primary ordinary recurrence

`LegacyFirstOrdinaryAcousticRecurrence.Run` composes graph50/58 successor
queries with these records, for ordinary same-source labels with all five
native Arabic traits zero. Input requests must already have a confirmed
regular symbol, composite category, ranked group cost and consistent native
likelihood lookup. Primitive/fallback labels and special symbol transitions
are outside this API. Beam width and native request enumeration order are
explicit. Capacity is bounded to256, nodes4096 and total requests1,000,000;
span0..7 is the representable40-byte branch. Arithmetic overflow is rejected.

For a request at nodeN/spanS, previousNode=N-S-1. Initial requests enumerate
root successors for selectorS and insert a source-root record. Continuations
enumerate finite previous records and their selectorS successor lists. Native
**frontend118 runs before frontend140**, separately for kind0 and kind2. A
rejection short-circuits the second callback. Kind2 is selected only when its
cost is strictly smaller; equality keeps kind0. The best previous alternative
for one current request is retained with native strict comparison. Group cost
is then added, and the independent acoustic subtotal omits geometry deltas.

Packed bit0 holds the chosen kind2-versus-kind0 path, while byte0a remains0 at
this stage; they must not be conflated. Initial bit0 is1 and previous byte09
isFF. `TracePrimary` backtracks this primary record path for diagnostics. It
does not stand in for final273 output materialization or acceptance confidence.

The native zero-Arabic-trait branch is not a guessed Unicode case/script
penalty: initializer1edde0 selects its zero-trait thunk1eddb0 when the acoustic
alphabet has no contextual Arabic arrays. The separate controller transition
penalty40960 is therefore not arbitrarily charged for Cyrillic/Latin changes.

## Mandatory corrections and remaining gap

Both correction callbacks are required, with cancellation checked before and
after each invocation. No zero-cost default is supplied. Static dispatch is:

* frontend118/1525e0 ->21b198/1ad890 ->297A8/195c40 for ordinary requests;
* frontend140/1528c0 ->21b190/1acfa0, because21b1c8/05a400 returnsfalse.

Fresh first-controller graph descriptor byteA isfalse, hence graph1DC does not
disable the second updater. Sampler virtual78/194170 explicitly supplies
graph flags190=1,191=1,192=runtime297byte50 and additional geometry settings.
These producers cannot simply be replaced by zero. Their complete resource
and prepared-geometry bridge, the later A8/B0/B8 text-context merging passes,
and final1f42d0 winner/result materialization remain unfinished.

## Reproducible controls

```powershell
python tools/LegacyFirstRecurrence.Probe/reference_controls.py <controls.json>
dotnet run --project tools/LegacyFirstRecurrence.Probe -- <controls.json>
```

The independent Python oracle poisons opaque bytes and checks512 insertion
cases/15,360 operations, including negative/equal costs and identity updates.
For384 small linear ordinary graphs, it expands compatible prefixes,
coalesces requests and independently sorts/truncates candidates. Managed
controls compare5,015 complete primary paths and scalar metadata, callbacks
with distinct negative deltas/rejections, cancellation and copied ownership.
The reference data and input strokes are our own, without vendor weights.

Optional real-resource invocation:

```powershell
LegacyFirstRecurrence.Probe.exe <controls.json> models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

This prepares our printedMIR stroke fixture through owned247/24a/265/297,
first float context, actual NN/raw180/final195 cache, ranked query and first
shift-zero graph. **Only source-root requests** are submitted: five variable
groups reach25 native initial records. Unknown continuation callbacks throw
if called. Category geometry here is explicitly the existing separation-cost
diagnostic, not a claim that the entire native category producer is restored.
There is no accuracy assertion or claim of completed Russian joined writing.
