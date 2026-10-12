# Higher-arc reset-source alternatives

`LegacyNativeHigherArcResetAlternativeProducer` ports one complete nontracing `20e6a0` higher-arc branch. It connects an actual stored continuation parent to new initial primary/secondary alternatives. The other higher-arc branch invokes native frontend118/140/F0/148/100 callbacks and alternative merge1f87c0; it is explicitly refused rather than replaced with guessed results.

The source branch predicate is exactly `Mode22f6 == 0 && (sourceDescriptorHeader & 1C) > 7 && slotLabel24 != controller24ea`. The descriptor header is read through an explicit callback addressed by the actual current slot04 source byte. `LegacyNativeHigherArcResetParameters` contains explicit controller24ea and uint32 controller22ec values, with numeric names because their complete preparation policy is not recovered here. No default special label or source ID is chosen.

The predecessor is unchecked unsigned32 `currentNode - (uint)(int)(sbyte)slot20 - 1`; current slot1e must be at least230a. The owning parent callback receives that exact nonnegative predecessor and actual arc. It returns an88-byte stored row and its configured secondary alternatives. Root and lower-arc scopes have separate producers. Counts1..16 and per-heap4096 slots are checked subsets, not claims about SDK maxima. Ordinal58, existing pool bytes, source metadata and ownership are explicit inputs. Both-disabled counts1/1 preserve the native no-op without descriptor or parent lookup.

The first primary alternative starts from an empty mapped/raw context through1f7e30/1f7ff0, with unchecked signed32 current-minus-parent record cost and increment. It receives current slot count26. Its state byte1f low five bits come from slot21, high two bits retain their previous pool provenance, and bit5 is selected by exact uint32 source equality: if slot04 equals controller22ec, use slot05 bit0; otherwise set bit5. Comparing a byte to only the low byte of22ec would be incorrect.

The first secondary alternative retains the current slot's full cost/increment/count. Both parent alternative0 contexts are concatenated with current slot08 through **raw1f7ff0**, even the mapped output ID; the mapping callback is not used for these two secondary concatenations. Secondary state1f preserves its high three bits and copies current low five state bits. First-row18/1a copy slot1e/20, and1b preserves bit7 while clearing the low seven bits.

Remaining primary and secondary alternatives receive both cost00/increment04 `04000000` and count1e zero. Their other bytes remain unchanged. First-row reserved1c..1d and20..27 retain their prior pool bytes. Inactive pool rows and full128-byte input heap storage remain unchanged. No pointer-shaped IDs or zero initialization is manufactured: context IDs belong to the shared actual symbol factory, and storage comes from the caller's owned snapshots.

Cancellation is checked immediately after both source-descriptor and parent callbacks, before composition, and before final return. Earlier context allocations are not transactional. Missing parent blocks, unsupported branch predicates and trace640-byte copying are refused.

`tools/LegacyWordHigherArcReset.Probe/reference_controls.py` independently generates512 complete primary/secondary pool controls with counts1/2/3/16, signed spans and32-bit wrapping costs, varying context lineages/control tokens, source-ID equality, random reserved/inactive bytes, and full callback provenance. Another1024 controls exhaust all256 low header values × mode0/1 × matching/nonmatching special label. Focused controls cover cancellation inside either owner callback, unsupported recombination scope, and the both-disabled no-op.

Managed and Windows NativeAOT use real RU resources in a composed fixture: NN/final-cache root/primary-parent costs create6 stored child rows; explicit diagnostic anchored reconstruction creates2 real stored continuation rows; the new higher-arc reset producer addresses those actual parents and creates2 pool rows, followed by native initial duplicate merging/materialization into1 default2/2 stored initial row. Primary cost resets to the real current acoustic increment and its context is the actual current suffix. Secondary context retains the actual stored continuation parent and appends that suffix. Source descriptor8, controller24ea/22ec values, mapping branch, anchors, masks, frames and scheduling are explicitly selected diagnostics; this does not prove complete native orchestration or Russian production acceptance.

Commands from the repository root:

```powershell
python tools/LegacyWordHigherArcReset.Probe/reference_controls.py .artifacts/legacy-word-higherarc-reset-controls.json
dotnet run --project tools/LegacyWordHigherArcReset.Probe -c Release -- .artifacts/legacy-word-higherarc-reset-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
dotnet publish tools/LegacyWordHigherArcReset.Probe/LegacyWordHigherArcReset.Probe.csproj -c Release -r win-x64 -p:PublishAot=true --artifacts-path D:\BlurMask-build-artifacts-20261011\word-higherarc-reset-artifacts -o D:\BlurMask-build-artifacts-20261011\word-higherarc-reset-native-bin
```

The NativeAOT executable uses the same five data arguments. No GUI, installer, Explorer process, or vendor DLL is executed. Full higher-arc callback recombination, required rejection prepass, winner and acceptance confidence remain required work.
