# Ordinary main word flag prepass

`LegacyNativeMainWordFlagPrepass` connects an owned post-first main source and
its actual four-network class cache to the ordinary `2136D0` record consumer.
This is the stored-symbol flag prepass before `201270`, rather than the
unresolved outer `292` case-wrapper factory or the complete `2144F0` scheduler.

## Native producer and consumers

Static evidence is the `MyScriptText.dll` export `1802136D0`, plus the existing
`1F8D60`, `1C0FD0`, `1941B0`, `195C40`, `1A82A0` and `1ACFA0` exports. No vendor
DLL is executed by the probes.

`2136D0` returns without work when alphabet header `0E` is `FFFF`, when its
parent byte `05` bit 7 agrees with live controller `22F7`, or when the signed
distance from the parent's first source node reaches knowledge `58`. The
supported owned main source has alphabet count 192 and a present `0E` value.
The remaining two live policy inputs stay explicit.

The frontend `110` query uses the alphabet-count label, length zero and context
zero. The adapter builds its finite-union qualifier from the same owned final
195-entry caches subsequently used by frontend `128`. It iterates the existing
owned successor connection list in native order. Original previous ending and
source arc remain distinct from the ancestor fields in the complete 80-byte
packet.

`LegacyNativeWordFlagPrepassRequestFactory` restores the complete packet,
preserving untouched request residue and its initialized `34 = 0x4000000`
limit. Parent source headers produce request `40/44`; actual category mappings
produce bytes `12/13`; the shared cost cell starts from parent `10`, while
request `30` starts from parent `14`. Callback order is `118`, `140`, `128`,
then `1F8D60`. The resulting 128-byte record preserves its parent's label,
category, first source node, owning context fields and anchors, and forces the
native flag/kind fields. Budget rejection skips allocation and preserves the
session; exceptions or cancellation invalidate it.

## Owned main kind 5 geometry

`LegacyMainBlankTransitionGeometry` reads the main selected sampler's actual
resource tail, including runtime `70 = -0.5` and `10C = 0`. It verifies the
resource against the sampled source owner before preparing any costs.

The `1941B0` cache `10` uses the current minimum X, the maximum prefix X over
all predecessor arcs, the actual post-first guide height, threshold `70` and
slope `108`. It calls the independently checked common
`LegacyCursiveBlankGapCost`. Root arcs and missing descriptor lines receive
the native impossible cost. The bounded singleton source has the restored
same-descriptor, ordinary-protocol `1A82A0` status zero. `195C40` kind 5 adds
this cache without the ordinary translation term. Main runtime flag `50 = 0`
makes `1ACFA0` accept at its graph `192` check before the first-source FFFD or
pair-geometry branches. First-source budget/scratch policies are not imported.

The shared final cache and `195520` separation table then supply the count-label
class cost and actual geometry index to the existing full-request acoustic
consumer. Raw neural outputs are never substituted for its final class cache.

## Ownership and limits

The main source, prepared graph, source guide and class caches are owned by
`LegacyFreshSingleGlyphMainSampled` and
`LegacyPreparedFreshMain240AcousticScorer`. The row lookup must supply a full
88-byte row from the requested owning node/ordinal. Header/source ID extents
and graph adjacency are checked before use; the parent and request residue
are copied. Callback cancellation is checked immediately. Calls on one
prepass instance must be serialized.

The caller still owns the selected word row container, actual prepared source
headers, `22F7`, knowledge `58`, `2609` and live heap descriptor routing. These
are explicit inputs rather than inferred settings. The API does not perform
case conversion, attach decorators, construct the entire scheduler or select
a final word/confidence. A prepared direct dictionary does not prove that the
native outer source factory omits the `292` case wrapper.

The real-resource fixture produces a real first result using explicitly
limited plain proposals, then transports that result through the restored
720-byte source records and main postprocessing. The resulting main geometry
and NN cache are owned. The limited first proposal schedule is diagnostic and
does not claim the full native first winner or production Russian recognition.

## Independent checks

Managed and Windows Native AOT checks cover:

- 1,024 independent Python full-80 packet projections with random parent and
  residue bytes, actual 35-category resource mappings, and immutable inputs;
- six real main min-X/all-parent-prefix/guide-height gap cache rows against an
  independent scalar log/bit-rounding calculation;
- real main source → four NN families → final 195 cache → an eligible actual
  dictionary query → actual owned parent row → kind 5 → full 128-byte output;
- an independent numeric/full-128 output formatter, including nonzero allocator
  residue `78..7F`, anchor/history tail and both accumulated costs;
- 16 guards, including cancellation/lifecycle, request extent/limit,
  row/header/node ownership, polarity/distance no-op and strict/equal/loose heap
  budgets. Rejected candidates preserve all 128 bytes of existing storage.

Generate controls with **one output argument**:

```powershell
python tools/LegacyMainFlagPrepass.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/main-flag-prepass-controls.json
dotnet run --project tools/LegacyMainFlagPrepass.Probe -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/main-flag-prepass-managed-artifacts -- D:/BlurMask-build-artifacts-20261011/main-flag-prepass-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
$env:TEMP = 'D:/BlurMask-build-artifacts-20261011/native-build-temp'
$env:TMP = $env:TEMP
dotnet publish tools/LegacyMainFlagPrepass.Probe -c Release -r win-x64 -p:PublishAot=true --artifacts-path D:/BlurMask-build-artifacts-20261011/main-flag-prepass-native-artifacts -o D:/BlurMask-build-artifacts-20261011/main-flag-prepass-native-bin
& D:/BlurMask-build-artifacts-20261011/main-flag-prepass-native-bin/LegacyMainFlagPrepass.Probe.exe D:/BlurMask-build-artifacts-20261011/main-flag-prepass-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```
