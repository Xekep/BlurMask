# Word startup request and acoustic128 bridge

This batch connects the literal ordinary startup scoring request to the final
acoustic cache, then to the existing native record writer and heap. It adds
`LegacyNativeWordStartupRequestFactory` and
`LegacyNativeWordAcousticRequestScorer` without changing frozen scalar APIs.
It does not claim the complete `1fd240` scheduler or replace missing qualifiers
with always-true callbacks.

## Request producer

The factory ports the known ordinary, nontracing request stores in
Text `1801fd240`. The caller supplies an actual query tag, length/flags,
returned graph arc, selected composite category, prepared descriptor header,
permission byte, base cost and unknown 80-byte stack residue.

Known startup fields are current node00, previous source/ending nodes04/08=−1,
current graph arc0c, previous arcs0e/10=FFFF, geometry index12, previous
geometry13=FF, initial first IDs14/18=−1, kind1c=`queryFlags & 7`, span20,
current symbol24, previous symbol26=FFFF, portable cost-cell identity28,
accumulator30=0, limit34=impossible, query length38, zero39, permission3a,
source kind40=`preparedHeader >> 9 & 15`, zero44, tag flag48 and zero49.
Unwritten padding stays caller residue. The cost cell receives the actual
query base cost before scoring. Portable identities are never vendor pointers.

Native startup also prefixes ordinary initial contexts with sentinel **FFFA**.
For an actual one-symbol source context with parent0, the endpoints passed
to `1f6fb0` are both0, so it returns the sentinel context. `0918d0` then
resolves the current-symbol child under that sentinel and links the newly
created child's parent. `GraftOrdinaryInitialContext` implements this bounded
producer using the shared existing context container. Repeated grafts share
the same node. Complex paths, control tokens, enabled case rewrites and the
general `1f7080` path remain explicitly unsupported.

## Correct full128 field semantics

Frontend `180152790` delegates to acoustic `1801c0fd0`. Its threshold selector
is the **unsigned source kind at request40**, not the transition byte at1c.
Source kinds1/2/3 use rawF0/F4/F8 thresholds respectively; equality passes.
Other kinds do not use those three gates. A class cost exactly equal to
`0x4000000` is always impossible.

Geometry cost uses the **signed byte at request12**, which is the geometry
index derived from the sampler's composite category. It is not the original
composite index. Five actual alphabet-resolved primitive labels bypass that
geometry lookup. `LegacyNativeWordAcousticGroupCosts` owns final class costs
and the geometry table for an actual node/arc; raw180 neural outputs are not
a substitute for the final cache.

The literal cost addition wraps signed32 exactly as the native instructions.
It compares the result strictly against request34. The separate existing
scalar scorer intentionally checks overflow; this full-request API preserves
the broader native arithmetic contract. Negative class and geometry costs
and costs above the positive sentinel remain data in the literal operation;
an invalid array index is refused instead of dereferencing a vendor pointer.

On accepted calls frontend128 adds the actual cost delta to existing
accumulator30. On rejected calls it stores impossible there. If the explicit
frontend suppression flag is enabled, graphE8 is queried first. Suppression
sets the cost to impossible and returns **before** touching accumulator30 or
reading the class-cache provider. Cancellation is checked after callbacks.

## Verification and boundaries

The independent Python byte writer reads both original RU category profiles
and generates 1,024 complete request/scoring controls. Threshold selectors
include source kinds1/2/3 independently of transition kind. Nonuniform
geometry tables produce **518** differences if a composite index is wrongly
used in place of geometry index. **188** controls exercise signed32 wrap.
Expected full80 buffers, cost values, suppression ordering and provider
counts are checked in managed and Windows Native AOT runs. Eight provider
cancellation guards and64 sentinel graft/dedup/unsupported-path controls also
pass.

The real-resource consumer prepares actual RU ink and final NN cache, loads
the real dictionary source and typed query pages, and feeds their produced
tag/context/symbol/base-cost fields into the request/scorer. Two pages yield
132 real cache calls, ten accepted sentinel-prefixed contexts, and six owned
native heap rows that materialize intact. Prepared frame90/80, permission7,
record qualification and remaining writer fields are scoped diagnostic
inputs. The producer does not silently claim the general frontend110
qualifier, special-symbol startup alternatives, dynamic source policy,
complete pass ordering, winner or calibrated Russian acceptance.

This stage specifically prevents two coincidental successes in earlier RU
transport tests: source kind0 and dictionary kind6 both bypass the three
thresholds, while the observed RU geometry cache repeats one value across
its categories. The new controls vary both independently.

From the repository root:

```powershell
python tools/LegacyWordAcousticRequest.Probe/reference_controls.py .artifacts/word-acoustic-request-controls.json
dotnet run --project tools/LegacyWordAcousticRequest.Probe -c Release -- .artifacts/word-acoustic-request-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
dotnet publish tools/LegacyWordAcousticRequest.Probe/LegacyWordAcousticRequest.Probe.csproj -c Release -r win-x64 -p:PublishAot=true -o .artifacts/word-acoustic-request-native
.artifacts/word-acoustic-request-native/LegacyWordAcousticRequest.Probe.exe .artifacts/word-acoustic-request-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

Static evidence: private `research/myscript` Text exports at `1fd240`,
`1f6fb0`, `1c0fd0`, `152790`, and confirmed category builder `0103e0`.
No vendor DLL is executed and private exports are not runtime dependencies.
