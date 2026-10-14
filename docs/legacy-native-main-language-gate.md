# Ordinary main language-stage gate

`LegacyNativeWordLanguageGate` connects Text `203c90` to the actual ordinary
candidate-graph boundary flags, retained resource span cap and stored word rows.
It decides whether that language stage runs. It does not choose a winner, create
row storage, initialize descriptors or execute the full `2144f0` scheduler.

## Graph producer ownership

Type21b raw table `1802e1e00` reader `1801ad9d0` clears the scalar region and
reads its first serialized byte into raw offset8, then the second into offset9.
Runtime `1801a6980` receives the cloned raw object, preserving that scalar cap.
Main resource node189379 contains8/1; first node12727 contains7/1. The existing
span-profile reader recovers the same first byte. Candidate graph generation
now retains its passed `MaximumAtoms`; the gate requires that value to equal
the selected bundle's actual main cap. The internal graph constructor accepts
the retained cap for subsequent owned pruning/remapping. Generation, arc order,
flags and group ownership otherwise retain their existing behavior.

Graph table `1802e1ff0` virtual188 is `1801acf70`. Static assembly resolves its
ordinary arc and returns `(arc.byte6 >> 2) & 1`. This is the existing
`CandidateArc.Flags` bit2, distinct from group kind/flags. Native seed `1a1ef0`
sets this bit; physical-overlap `1a4f10` clears it where a physical source crosses
the boundary; merger `1a37b0` carries the left arc's bit. The existing ordinary
graph producer therefore supplies the actual predicate rather than a callback
that always claims a boundary.

Frontend222.virtual100=`152350` scans current-node arcs in encounter order. It
accepts an arc when `current - span - 1 == previous` and graph188 is true.
Frontend.virtual108=`152400` searches following nodes, starting at previous+1.
Graph.virtual30=`1a75d0` returns `min(node+1, raw8 cap)` for nonnegative nodes,
so the following search extent is `min(previous+2, cap)`. It returns false on
reaching graph.NodeCount. This scope uses the ordinary primary graph; source
joins, supplemental node overrides and foreign token aliases are not inferred.

## Controller and stored-row consumer

`203c90` first accepts controller22e8 bit3 without accessing graph rows. Otherwise
param4=-1 uses frontend108 and scans primary rows0..230A. A nonnegative param4
uses frontend100(param4,param3) and scans secondary rows230A..storedCount[param3].
The gate takes the actual primary count, complete packed88 rows and source
descriptor headers. It neither guesses primary/secondary boundaries nor derives
headers from a symbol's language.

The scan stops on cost10 **equal** to04000000h. A larger signed cost does not
trigger this exact sentinel check. A visited row accepts for the selected
alphabet's U+FFFD label, or if signed32 wrapping `storedNode - anchor38` is at
least knowledge54 and its source kind `(header & 1E00h)` lies in0200h..0A00h.
The literal wrapping comparison and native first-accept encounter order are
preserved. Only visited source IDs need valid descriptor ownership; native
global/graph short circuits do not access unrelated invalid row storage.

Controller flags, knowledge54 and descriptor headers remain explicit owning
caller snapshots. Full source preparation and scheduler lifecycle are separate
stages. In particular, a raw generated graph may still require the native V68
eligibility/pruning stage before it is the final scheduler graph. This gate
does not silently treat an unpruned diagnostic graph as that completed stage.

## Independent and resource checks

The original Python oracle constructs512 independent physical-overlap layouts
and4096 gate decisions. The controls cover ordinary span flags, following
lookahead bounds, primary/secondary ranges, source kinds, replacement symbols,
sentinel equality, values beyond the sentinel, signed overflow and global
short circuits. Thirteen guards check model/cap/node/extents/source ownership
and cancellation. Managed and Windows NativeAOT run the same controls.

The real resource consumer derives a whole first physical-stroke arc from the
owned main graph rather than using its first partial atom. Actual finite NN
costs, qualification, prepared dictionary pages and native initial records
produce10 parent rows. Eight terminal rows pass the real following-boundary
gate, source70/profile2 language evaluation, source50 restart and `1f9080`.
The actual descriptor refresh has aggregate bit3 clear, so the physical graph
boundary is required; the preceding partial physical stroke fails that boundary.
The acoustic frame remains an explicit diagnostic fixture. No selected final
word, complete main row/frame orchestration or acceptance confidence is claimed.

```powershell
python tools/LegacyMainLanguageGate.Probe/reference_controls.py .artifacts/main-language-gate-controls.json
dotnet run --project tools/LegacyMainLanguageGate.Probe -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/main-language-gate-managed-artifacts -- .artifacts/main-language-gate-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

Vendor binaries were inspected statically and were not executed.
