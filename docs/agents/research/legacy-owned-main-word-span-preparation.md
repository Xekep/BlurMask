# Owned main word span preparation

`LegacyOwnedMainWordSpanPreparation` restores the ordinary main frontend
`98/D0` bridge into dictionary queries and the existing `1FD240` consumer.
It owns its resource-bound main acoustic scorer, direct dictionary maps,
context container and aggregate query ring. Callers do not supply successful
cache callbacks, an eligibility mask or a manually prefiltered query page.

Static `1520E0` delegates ordinary cache/source preparation before `1C0F60`
rebuilds frontend `88` eligibility. `1C01B0` forwards the produced permissions
through source `68` to `093F80`, which restores dictionary `58` into `50` and
filters it. The new class composes the existing independently checked
`LegacyNativeWordSymbolPreparationSession` and its native pool/permission
lifecycle, then installs the resulting working map into its actual query
source. The same actual final 195-entry class rows and `195520` separation
costs subsequently feed frontend `128`. Raw NN outputs are not final caches.

For `98`, frontend `151FE0` delegates graph `1A7580`, which scans every owned
arc's byte `04` and returns the maximum span plus one, or zero for an empty
row. `2144F0` iterates spans in ascending order and computes the original
previous ending as `current - span - 1`. The span extent includes intermediate
empty spans; it is distinct from maximum atoms, arc count and frontend `C8`'s
125-row source budget. `Prepare` uses native D0 `param5=false`; the existing
pool still independently decides when a computed group is reused.

Each result carries an owner and preparation epoch. Preparing another span
changes the actual dictionary map, so stale results cannot query or access a
cache as though the old source state remained active. Foreign span owners and
out-of-span acoustic callbacks are rejected. Query/preparation/cache-access
calls are serialized. Failure or cancellation invalidates the owner. Returned
cost rows and eligibility snapshots are owned immutable copies from the
existing native-bound components.

The dictionary is **explicitly prepared directly**. Native `290/292` factory
selection, case-normalizer population and special query decorators remain
separate. Selected word rows, full `2144F0` scheduling, language continuation,
final winner and acceptance confidence are not inferred. The shared probe
first-result fixture uses its clearly limited first proposal schedule; its
real 275 result then owns post-first 720/source/frame transport and main NN.
This is not a production Russian-language claim.

## Actual resource checks

Managed and Windows Native AOT probes traverse all ten spans of the owned main
fixture and compare:

- ten selected rows / 1,950 actual main class-cache values;
- selected ordinal order, an independent pool invalidation/reuse simulation,
  repeated D0-false reuse, finite-cost union and independently computed source
  permission bytes;
- restored `58`/filtered `50` dictionary maps, complete unfiltered root edge
  order, source IDs, labels and actual context ownership;
- 39 full refreshed root query records into 43 real-NN native initial
  128-byte candidates, without expected-label or plain-symbol filtering. Costs,
  categories, arc/first-node ownership and FFFA context lineage are checked;
- seven stale/foreign/domain/cancellation/failed-session guards.

The current fixture's root output includes Latin, Cyrillic and `(`. It does not
activate quote/primitive-extra startup branches. Those `1FD240` branches still
need their own exact consumer; this probe does not suppress or declare them
covered. Its 39 root records and 43 active candidates are unrelated to the
actual capacity 125.

The probe links the frozen `LegacyMainFlagPrepass.Probe/MainFixture.cs` to reuse
the previously checked upstream diagnostic fixture rather than duplicate it.

```powershell
dotnet run --project tools/LegacyMainSpanPreparation.Probe -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/main-span-managed-artifacts -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
$env:TEMP = 'D:/BlurMask-build-artifacts-20261011/native-build-temp'
$env:TMP = $env:TEMP
dotnet publish tools/LegacyMainSpanPreparation.Probe -c Release -r win-x64 -p:PublishAot=true --artifacts-path D:/BlurMask-build-artifacts-20261011/main-span-native-artifacts -o D:/BlurMask-build-artifacts-20261011/main-span-native-bin
& D:/BlurMask-build-artifacts-20261011/main-span-native-bin/LegacyMainSpanPreparation.Probe.exe models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```
