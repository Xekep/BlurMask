# Anchored continuation alternatives and reference-pair reconstruction

This batch ports the nontracing anchored branch of Text `211eb0` (`slot3e != FFFF`) with actual stored-row owner callbacks. It adds `LegacyNativeAnchoredContextProjection` and `LegacyNativeAnchoredWordAlternativeProducer`. The unanchored branch has a separate existing producer. Native production of the anchor and replacement-index inputs, rejection-prepass internals, higher-arc `20e6a0`, full scheduling, winner and acceptance confidence remain incomplete.

## Reference-pair proof

The C export alone omits a decisive comparison after scanning a path to null. Disassembly at `18021222a..180212278` establishes:

1. Load `referenceParent = reference->00` before scanning.
2. Scan the current mapped path backwards, comparing its identity with both reference and referenceParent, collecting ushort symbol20 keys when neither matches.
3. After the scan reaches zero, compare that zero result with reference and referenceParent too (`212269` and `21226e`). A zero referenceParent is therefore a valid match.
4. Only if neither comparison matches, replace reference with `referenceParent->00`, stepping two ancestors, and repeat.

The implementation follows this literal pair traversal. It does not substitute a generic lowest-common-ancestor query. A null reference before step1 would be dereferenced by the native function; the bounded API refuses this input rather than inventing a zero node. Foreign identities, cycles and paths beyond4096 are also refused. These are supported-input limits, not recovered SDK maxima.

The returned keys are in parent traversal order: last/current symbol first. Native0918d0 replay inserts them in reverse. Replay is direct ushort index insertion, without mapping callbacks or the six-control composition table. `FFFC` followed by a retained `FFFA` stays as two nodes during replay; `1f7ff0` would fold that pair. New nodes receive their actual symbol-path parent link. Allocation-bound null results are retained and subsequent direct insertions continue from that zero state.

## Actual stored-row lineage

The producer addresses four owning row snapshots in this order:

- Current parent: explicit function parent node and current slot1e.
- Anchor: current parent's stored record38 node and ushort3c arc.
- Reference: predecessor node `anchorNode - signed(anchorRecord20) - 1`, and anchorRecord1e arc.
- Replacement: that same predecessor node and current slot3e arc.

It never infers these addresses from glyph geometry, IDs, timestamps or lexical content. The provider returns actual `LegacyNativeStoredWordRecord` objects. Each lookup takes an owned snapshot of its88-byte record and alternative blocks. The predecessor must fit a nonnegative signed node index; unsafe wrapped pointer indices are refused. Configured counts1..16 are a checked subset. Actual default2/2 is retained in the fixture, but it is an explicit prepared configuration; constructor defaults do not establish all native prepared sessions.

The current parent's first secondary mapped context is scanned against the reference's first mapped context using the pair projection. Every available replacement alternative then supplies its own mapped prefix. The retained literal keys are replayed onto that prefix, followed by current slot08 suffix composition through mapped `1f7e30` when nonzero.

Raw output is deliberately different: every replacement alternative uses **current parent's first secondary raw context**, optionally composed with the current suffix through raw `1f7ff0`. It does not use replacement alternative[k]'s raw ID.

## Costs and selective metadata

For each replacement alternative whose signed cost00 is at most `03ffffff`:

- Cost00 is unchecked signed32 `current.slot10 - replacement.record10 + replacement.alt00`.
- Increment04 is unchecked signed32 `replacement.alt04 - replacement.record14 + current.slot14`.
- 18 contains current parent arc; ushort1a is `00FF`, so byte1b is zero for every alternative.
- 1c/1d are FF;1e is zero;1f preserves the old high three bits and copies current slot21 low five bits.
- 20 contains current slot3e replacement arc;22 contains the alternative index as a byte.
- 23..27 preserve the supplied previous pool bytes.

The first unavailable replacement alternative stops copying. Only cost00 of remaining destination alternatives becomes `04000000`; old increments, identities and metadata remain untouched. Negative costs and signed32 wrapping are preserved. Inputs, reserved bytes and inactive pool rows remain unchanged.

The same exact rejection predicate as unanchored211eb0 controls an explicit external preprocessing callback. Missing required preprocessing refuses the branch. Cancellation is checked immediately after that callback, covering true/false results and count1 no-op returns. Anchored source3e checks occur before external mutation. Trace640-byte cloning is rejected. Context and external owner mutations are not transactional.

Continuation ordering uses only secondary merging and transports these rows through the existing secondary-only materializer predicate (`destination >= 230a`). No one-alternative production configuration or dummy native pointer identity is introduced.

## Verification

The independent Python oracle models canonical symbol paths by tuples and uses pair-depth scans, then a separate byte-slice anchored writer. Managed and Windows NativeAOT verify:

- 1024 reference-pair controls, including a100-key current tail that exceeds the native50-key inline buffer, null scan matching, and233 explicit null-reference-invariant refusals.
- 512 complete anchored pools with counts2/3/16, differing source/raw/replacement contexts, reserved/inactive bytes, control symbols, unavailable tails, signed32 extrema and wrapping.
- 2048 exact current/anchor/reference/replacement node+arc lookups.
- Literal control-key replay, foreign identity refusal, cancellation before replay and four callback cancellation branches (true/false × secondary count1/count2).

The actual RU fixture prepares/scorers the existing MIR ink and carries real regular-model NN/final-cache costs through native root and primary-parent producers to6 stored child rows. It supplies explicit diagnostic self-anchor node/arc ownership for those actual rows; automatic source-anchor and slot3e production are not claimed. Four anchored candidates address actual stored parent/reference/replacement rows. The new producer replays their real child symbol tails onto replacement prefixes, preserves current raw lineage, and transports the resulting default2/2 secondary pools through native continuation merging/materialization to2 stored continuation rows. Expected fixture symbol paths are computed independently from the actual root-symbol relationship. No winner quality or production Russian AI-pero acceptance is inferred.

Commands from the repository root:

```powershell
python tools/LegacyWordAnchoredAlternatives.Probe/reference_controls.py .artifacts/legacy-word-anchored-alternatives-controls.json
dotnet run --project tools/LegacyWordAnchoredAlternatives.Probe -c Release -- .artifacts/legacy-word-anchored-alternatives-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
dotnet publish tools/LegacyWordAnchoredAlternatives.Probe/LegacyWordAnchoredAlternatives.Probe.csproj -c Release -r win-x64 -p:PublishAot=true --artifacts-path D:\BlurMask-build-artifacts-20261011\word-anchored-alternatives-artifacts -o D:\BlurMask-build-artifacts-20261011\word-anchored-alternatives-native-bin
```

The NativeAOT executable uses the same five data arguments. No GUI, installer, Explorer process or vendor DLL is executed.
