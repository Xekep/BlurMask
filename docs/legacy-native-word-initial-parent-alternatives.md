# Native initial alternatives with a primary stored parent

`LegacyNativeInitialParentWordAlternativeProducer` ports the nontracing `20e6a0` branch where the derived predecessor is an existing node and current slot `1e < controller230a`. Root alternatives, continuation `211eb0`, and higher-arc secondary-parent branches have separate producers or remain unfinished. No winner/confidence controller is inferred.

## Actual parent ownership

The native predecessor is unchecked unsigned32 `currentNode - (uint)(int)(sbyte)slot20 - 1`. The provider receives its exact nonnegative signed counterpart and the current slot's ushort arc `1e`. It supplies an owned stored88-byte row and its actual configured primary/secondary40-byte alternative blocks. Negative/unrepresentable predecessor indices, root `FFFFFFFF`, and higher-arc branches are refused before parent lookup. The checked API supports counts1..16, complete 128-byte heap slots, and explicit pool ordinals; it does not invent limits or heap ordering.

The actual parent's record08 points to a symbol context node. The producer obtains its ushort symbol at node20 from the shared portable context factory. Zero/foreign/stale identities are refused rather than replaced by a dummy token. This source token is distinct from an alternative's mapped/raw context IDs: classification of the parent branch must not use the last symbol of its rendered/output context instead.

## Resetting control tokens

The exact source condition is signed-short `symbol == -5 || (((symbol + 11U) & FFFA) == 0 && symbol != -10)`. Exhaustive enumeration of all65536 ushort values yields only `FFF5`, `FFF9`, `FFFA`, and `FFFB`. `ResetsPrimaryContext` exposes that exact set without assigning undocumented semantic names to the controls.

For these controls, the first primary alternative starts from an empty context, has `current.slot10 - parent.record10` and `current.slot14 - parent.record14`, the current slot count26, and bit5 set. Its mapped/raw contexts use `1f7e30/1f7ff0` on the actual current slot08 suffix. Remaining primary alternatives receive cost and increment `04000000` and count zero, retaining their other prior bytes.

The first secondary alternative has the current slot's complete cost/increment and count, but combines both of its parent contexts through **raw `1f7ff0`** in this reset branch. The mapped callback is not used for that first secondary context. Later secondary alternatives still follow the ordinary per-alternative inheritance rule below.

## Ordinary inheritance and suffix predicate

Suffix appending occurs only if `ushort(slot18) == currentNode` or signed byte `slot05 >= 0`. This is the literal source-node/header condition; it is not inferred from stroke chronology, lexical content, or confidence. An ordinary first secondary alternative retains its parent mapped/raw IDs when this condition is false, and composes them with slot08 when true. Its cost/increment/count always remain the current slot's values. First-row metadata is initialized from slot1e/20/21; byte1b preserves bit7 and clears its low seven bits.

Every ordinary primary alternative, and secondary alternatives1..count-1, independently test the signed parent cost against `04000000`. An available alternative receives unchecked signed32 additions of the current-minus-parent record cost/increment deltas. Primary count1e comes from current slot26; later secondary count1e is zero. Metadata18/1a comes from current slot1e/20, low seven bits of byte1b come from the alternative index, and bit7 retains its prior destination provenance. Mapped/raw contexts are either copied or composed according to the suffix predicate.

Primary state byte1f preserves its old high two bits, takes current slot21's low five bits, and **clears bit5**. Secondary preserves its old high three bits, including bit5, while replacing the low five. Reserved1c..1d and20..27 retain their prior pool bytes.

Unavailable parent alternatives receive both cost00 and increment04 `04000000`, and count1e zero. All other bytes retain their previous candidate-pool values (with first-row headers already initialized). This branch does **not** stop at an unavailable entry: a later available parent alternative is still processed. This differs from the unanchored continuation producer, whose unavailable tail only invalidates cost00 and stops copying. Neither producer clamps negative costs or signed32 overflow.

Cancellation is checked immediately after the owning parent-row callback and
before final return, including an appendfalse branch that makes no context calls.
Two controls cover callback cancellation with suffix appending true and false.

The inputs and parent blocks are copied and owned. Active heap records and inactive slot bytes are unchanged. Source/context access must be serialized; parent snapshots and pool configuration stay stable during mapping. Earlier context insertions can survive cancellation or a later exception. Trace640-byte copying is explicitly rejected; no native pointers or trace identities are manufactured.

## Independent controls

`tools/LegacyWordInitialParentAlternatives.Probe/reference_controls.py` uses a separate byte-slice writer and the frozen independent functional context oracle. Managed and Windows NativeAOT controls cover:

- 512 complete primary/secondary pools with counts1/2/3/16, random reserved/inactive bytes, shuffled/repeated ordinals, signed spans, both suffix predicates, control-reset source tokens, differing mapped/raw prefixes, negative costs, signed32 wrapping, and unavailable entries followed by available ones.
- 1280 exact predecessor-node/arc lookups, checked in physical slot order.
- All65536 source-token values against the original signed arithmetic predicate.
- Root/higher-arc rejection before external lookup, cancellation before state changes, and the both-disabled no-op branch.

An actual RU integration prepares the existing MIR ink, obtains regular NN/final-cache/category scores, and uses the native root producer and initial ordinal writer to create6 actual stored root parents. A second real classification supplies10 accepted acoustic increments. Explicit diagnostic scheduling builds and routes the initial child records, with source span0 and current node exactly one beyond the stored parent node. The new producer receives those actual rows through its owning provider and generates8 default2/2 child pool rows. Per-heap native initial ordering/alternative merge and the stored-row materializer retain6 children. All36 parent/history lookups address the actual stored rows. Their primary context contains the real parent lineage followed by the current suffix, their acoustic cost matches the real path, and primary/secondary bit5 provenance is preserved.

Frames/masks, mapper selection (the proven identity `1f8300` fixture branch), diagnostic enumeration, and controller limits are explicit. This producer/consumer bridge does not prove full native search orchestration or Russian cursive recognition quality. The higher-arc `20e6a0` branches, anchored `211eb0`, rejection prepass, native winner, and acceptance confidence remain required work.

Commands from the repository root:

```powershell
python tools/LegacyWordInitialParentAlternatives.Probe/reference_controls.py .artifacts/legacy-word-initial-parent-alternatives-controls.json
dotnet run --project tools/LegacyWordInitialParentAlternatives.Probe -c Release -- .artifacts/legacy-word-initial-parent-alternatives-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
dotnet publish tools/LegacyWordInitialParentAlternatives.Probe/LegacyWordInitialParentAlternatives.Probe.csproj -c Release -r win-x64 -p:PublishAot=true --artifacts-path D:\BlurMask-build-artifacts-20261011\word-initial-parent-alternatives-artifacts -o D:\BlurMask-build-artifacts-20261011\word-initial-parent-alternatives-native-bin
```

The NativeAOT executable uses the same five data arguments. No GUI, installer, Explorer process, or proprietary native recognition DLL is executed.
