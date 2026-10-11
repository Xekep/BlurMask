# Prepared native acoustic candidate score

`LegacyPreparedAcousticCandidateScorer` restores acoustic21d virtual58
(`1801c0fd0`), reached through frontend222 virtual128 (`180152790`). It accepts
an explicitly supplied acoustic class-cost cache, acoustic symbol, composite
category-cost cache, candidate kind, previous path cost and total budget. These
are prepared native inputs, not an automatic raw-ink recognizer.

The source order is significant:

1. Class cost exactly equal to the impossible sentinel `0x4000000` rejects
   before geometry access and writes the sentinel as the resulting path cost.
2. Candidate kinds1,2,3 require class cost at or below their respective runtime
   gates `+F0/+F4/+F8`. Other nonnegative kinds do not apply these gates.
3. Five symbol labels bypass geometry access: initialization `1801b0760` looks
   up exact UTF8 strings U+F0001, U+F0002, U+F0003, U+F0004 and U+F0008 through
   `180018f70`. These are symbol IDs, not categories or neural channels.
4. Otherwise the signed composite category selects geometry-cache slot2.
   The ordinary Russian sampler assigns the same grouping cost to all35
   composite categories; see [group separation](legacy-russian-cursive-group-separation.md).
5. Geometry cost, previous path cost and class cost are added. The updated cost
   is retained even when the strict `total < suppliedLimit` comparison fails.

`ForAlphabet` resolves those five exact strings from the caller's alphabet and
preserves missing labels as65535, matching the lookup sentinel. It does not
assume their numeric IDs across resource versions. The actual Russian alphabet
contains four of the five strings. The component preserves signed prior-path
values; it does not clamp previously applied negative language costs to zero.
Bounds and rejection of signed addition overflow are deliberate checked-port
limits, not a claim that the native engine checks wraparound.

## Resource forwarding

`LegacyAcousticCandidateThresholds.ReadV110Node` reuses the checked single-
profile type21d prefix reader and restores the bounded tail through runtime
field+F8. Reader `1801c2940` reads the three signed integer gates for stored
versions109 and later. Clone `1801b0560` copies raw object fields+8 through+157
into the corresponding runtime offsets without altering these gates.

The committed Russian resource stores them at byte offsets566390,566394 and
566398:20480,24576,32768. The reader consumes through566402 for node189734.
Other tail fields and nested obsolete conversion are not silently interpreted
as class gates; the latter is rejected in this checked v110 schema. Array bounds
are original defensive limits. The loader does not install universal gates for
other models or source modes.

## Verification and remaining scope

```powershell
python tools/LegacyAcousticCandidate.Probe/reference_controls.py .artifacts/acoustic-candidate-controls.json
dotnet run --project tools/LegacyAcousticCandidate.Probe -c Release -- .artifacts/acoustic-candidate-controls.json models/ru-RU/ru_RU-ak-cur.lite.res
```

The independent integer oracle provides1024 candidate controls and64 own
serialized acoustic-tail profiles, including variable lists and group arrays.
Regression checks cover exact kind-gate equality, strict total-limit equality,
early sentinel/threshold rejection before geometry lookup, geometry bypass
before category access, negative previous costs, invalid bounds, overflow and
cancellation. Actual-resource checks confirm all three gates and the four
present native bypass labels. Managed and Windows Native AOT pass.

Neural class costs and special-symbol cache generation must still be supplied
correctly by the upstream producer. This phase does not claim recovered native
word-beam orchestration, automatic source line/category/context selection,
real connected-word accuracy or AI-pen integration. It provides the proven
elementary addition/gating step for the ordered variable-candidate graph.
