# Owned ordinary terminal language penalty

`LegacyOwnedRussianTerminalSources` connects the checked selected dictionary
owner to the existing prepared `2257C0` selector. It derives the real source
header, source-kind-selected profile and loaded runtime scale. Invocation
flags, candidate rows, cumulative cost and the no-rescue/single-alternative
selector scope remain explicit; this does not establish a complete main winner.

`092990` passes the dictionary139 field6C and the acoustic knowledge's
virtual58 source kind to language2D1.virtual48, which is `0FE2A0`. Source135
selects stored profile2 by `137-sourceKind`. Its runtime2534 is later read by
the pure getter `0F7A10`, not computed inside the selector.

The stored input to this scalar is the **last unigram codebook level**.
`0FCC10` reads the word codebook into A0/A8, independently of the per-order
codebook counts D0 and level pointers D8. `0FE2A0`, assembly FF5D2..FF617,
loads `D8[0][D0[0]-1]`. It does not select the last word, last decoded class,
maximum observed class cost or the first level. The additive
`GetStoredFinalClassLevel(bool)` exposes exactly that stored endpoint without
exposing mutable arrays.

For a strictly positive finite runtime scale, local128[0] is scale times the
profile's unigram slope and local100[0] is scale times its unigram offset.
The native scalar operation order is binary32 conversion, multiplication by
2^-13, multiplication by that slope, subtraction of that offset,
multiplication by8192 and truncation to int32. The existing checked
`TransformClassCost(..., false, scale)` preserves this sequence. Unsupported
nonpositive/nonfinite scales and out-of-int32 conversions are refused here;
the native nonpositive-scale preparation branch is not silently substituted.

For the pinned Russian pair, the unigram codebook has256 entries and its last
level is77971. Actual loaded139 scale1 and profile2 produce **2534=17153**.
The existing selector's signed float32 times0.8 rounding then contributes
**13722** when its ordinary language/fallback branch is actually taken.
The implicit fresh source0 has header0 and no language-order bits, so the
selector never reads its language penalty. Its stored zero is explicitly
unused, rather than an inferred language-model value.

The owned factory verifies the selected dictionary/model/profile references,
order2/source135 and selected descriptor identity. Copies of its source table
cannot mutate the factory. It does not construct or guess an acoustic source,
model profile, query context, terminal invocation option or runtime override.

Validation uses2072 independent Python scalar/actual-profile controls, a real
owned source preparation and a prepared terminal row checking that the derived
penalty reaches `2257C0`. The first fixture's proposal policy remains diagnostic
as documented by its shared probe; this is not a real handwriting winner test.
Seven cancellation/scale/overflow guards are exercised. The static audit checks
the pinned PE's vtable bindings, caller/reader identities and18 consecutive
publication instructions. Vendor code is never loaded or invoked.

```powershell
python tools/LegacyMainTerminalSources.Probe/reference_controls.py models/ru-RU/ru_RU-lk-text.lite.res E:/controls.json
dotnet run --project tools/LegacyMainTerminalSources.Probe -c Release --artifacts-path E:/build -- E:/controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
python tools/handwriting/audit_main_terminal_sources.py E:/static.json
```
