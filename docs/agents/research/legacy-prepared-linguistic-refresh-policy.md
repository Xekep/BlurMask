# Prepared linguistic source refresh policy

`LegacyPreparedLinguisticRefreshPolicy` restores bounded deterministic decisions
inside Text `20c1c0`, `20d8f0` and `20c8c0`. Its inputs describe an explicitly
prepared source chain. It does **not** perform the complete native refresh,
allocate decorators, initialize their dictionaries or manufacture source flags.
The [prepared binding](legacy-linguistic-mask-binding.md) remains its downstream
consumer.

The pinned native evidence is `MyScriptText.dll` SHA256
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
Only static code/PE bytes were inspected. Private decompilation excerpts and
accessor disassembly are saved under
`D:/BlurMask-build-artifacts-20261011/linguistic-source-refresh`.

## Verified producer and consumer boundaries

`20c1c0` sets controller24b8 when container+C4 is true, active source kind is
0x137, or its acoustic alphabet contains the exact four-byte UTF8 token
`U+F0009`. The native constant at `2eb3a0` is `f3b08089`. `20d8f0` strips the old
decorators back to the base source+40, configures its class290 delimiter from
active profile.virtual78, then applies class296 if24b8 is true and class28f if
container+C4 is true, in that order. Profile class247.virtual78 is `1d62c0`, an
exact ushort getter of profile+BC. The delimiter is therefore explicit.

These built-in wrappers delegate symbol enumeration through `23bd10`; this
does not prove their preparation or all resource286 extension behavior. The
wrapper plan records only their selection/order. The actual RU alphabet lacks
U+F0009 and has source kind0x135, so an explicitly normal container mode selects
neither optional built-in wrapper. That observation supplies no default for the
live container mode.

After source.virtual10 preparation succeeds, `20d8f0` obtains source count via
virtual20 and each source descriptor via virtual28. Base class28d.virtual20
`236820` returns stored count+1. Class28d.virtual28 `236830` returns its implicit
descriptor at source+10 for index0 and the stored descriptor pointer at
source+50+(index-1)*24+16 for later indices. Dictionary source class2b7.virtual28
`093aa0` returns its descriptor at source+20. Wrapper virtual20/28 delegate through
`199bc0`/`1af6c0` respectively. Thus the descriptor list includes the implicit
source, not just the application's dictionary sources.

For each ordered uint header, `20d8f0` builds controller22e8 as follows:

| Aggregate field | Exact prepared descriptor operation |
|---|---|
| bits0..2 | Maximum `(header >> 2) & 7` |
| bit3 | OR of `(header >> 2) & 8` |
| bit4 | At least one `header & 0x20`, excluding category `(header & 0x1e00)==0x800` |
| bit5 | At least one category0xA00 |
| bit6 | OR of `(header >> 1) & 0x40` |
| bit7 | OR of `(header >> 1) & 0x80` |
| controller22EC/22F0 | Last encountered index of category0x800/0xA00 |

It clears aggregate22e8 before enumeration. It **does not reset22EC/22F0** during
this stage: a missing category retains the preceding index. Constructor
`20baa0` sets both fresh indices to-1. The public API requires those preceding
values explicitly, including the fresh-1 case.

When controller2609 enables FORCE_ALIGNMENT, preparation accepts only count2
and null controller2300. Otherwise `20d8f0` returns failure after descriptor
enumeration. On success `20c8c0` clears aggregate bit3 if controller2300 is null;
that postprocessing is skipped after a rejected refresh. The summary exposes
both the resulting flags and the explicit acceptance result. Native consumers
include `203c90`/`204290` for bit3 and multiple word-search branches for low3 bits.
This helper does not assign script aliases or supply the rest of those consumers.

`BuildPreparedMasks` carries explicit source-preparation success and this
FORCE_ALIGNMENT check into the bound controller. A rejection makes controller
preparation fail, allowing the ordinary producer's native fallback. Explicit
language-controller bypass remains a bypass. This is an executable
producer→consumer connection, not an assertion that the native factories work.

## Input-method budget

`20d8f0` calculates controller2602 from live container byte8D and controller
bytes2603/2604. Alternate mode uses `min(container8D / 50, alternate2604)`.
Normal mode uses `min(container8D / 50, normal2603)` when the **post-extension**
controller2601 is active, otherwise zero. Integer division truncates.
Resource286 extensions can affect the intervening controller state, so the
post-extension flag is supplied explicitly rather than inferred from the wrapper
plan. The budget has no inferred recognition-confidence semantics.

## Controls and remaining work

An original set-based Python oracle generates 512 prepared descriptor controls,
including reused source positions. Managed and Windows NativeAOT probes pass
them, four built-in wrapper predicates, 72 budget checks around every multiple
of50 through255, and cancellation/malformed input checks. FORCE_ALIGNMENT
acceptance/failure is also checked through prepared binding and the final
acoustic cache: a rejected configuration cannot accept a partial successful mask.
These original fixtures do not measure real handwriting accuracy.

```powershell
python tools/LegacyLinguisticRefresh.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/linguistic-refresh-controls.json
dotnet run --project tools/LegacyLinguisticRefresh.Probe -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/linguistic-refresh-managed -- D:/BlurMask-build-artifacts-20261011/linguistic-refresh-controls.json
dotnet publish tools/LegacyLinguisticRefresh.Probe -c Release -r win-x64 -p:PublishAot=true --artifacts-path D:/BlurMask-build-artifacts-20261011/linguistic-refresh-aot -o D:/BlurMask-build-artifacts-20261011/linguistic-refresh-native
& D:/BlurMask-build-artifacts-20261011/linguistic-refresh-native/LegacyLinguisticRefresh.Probe.exe D:/BlurMask-build-artifacts-20261011/linguistic-refresh-controls.json
```

Full dirty22F4/pending22F5 lifecycle, source initialization, source-weight
normalization/sorting in `236490`, dictionary descriptor production in `092990`,
resource286 extension callbacks, allocation failures and cached word data remain
outside this prepared policy. In particular `093f80` filters the dictionary's
working map+50, whereas symbol enumeration `093af0` uses preserved map+58. They
must not be treated as the same acoustic mask update. Restoring that working-map
producer/consumer chain is separate work.
