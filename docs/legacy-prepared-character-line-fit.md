# Prepared character line iterative fit

`LegacyPreparedCharacterLineFit` ports the primary prepared-item C1A fit at
MyScriptInk `180021110`, using the separately verified moment producer
`180020d70` and constrained solve `180020ed0`. It reconstructs the primary
baseline, x-height and slope calculation before the wrapper's acceptance and
bbox fallback. It does **not** yet produce the required character hypotheses
from raw connected handwriting.

## Static evidence and input contract

The inspected MyScriptInk DLL SHA-256 is
`b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a`.
Read-only decompilation and PE assembly are private audit artifacts at
`D:/BlurMask-build-artifacts-20261011/InkInitialLineC1aFitNext/`.
No vendor executable or DLL is loaded by the implementation or probe.

Configuration setter `180020d00` requires **0x18 = 24 bytes** and copies three
qwords into runtime offsets 10/18/20. The prepared configuration contains:

| Offset | Type | Meaning |
| --- | --- | --- |
| 00,01 | bytes | minimum/maximum item-count slope thresholds |
| 04,08 | floats | lower/upper slope bounds |
| 0C | float | minimum height retained for the separate wrapper |
| 10 | float | maximum width for vertical clipping |
| 14 | float | known-glyph total weight cap |

Each prepared item is **0x248 = 584 bytes**, not 248 bytes:

| Offset | Type | Meaning |
| --- | --- | --- |
| 00,01,02 | bytes | known glyph, clip height to width, marker downweight |
| 04 | uint32 | active candidate count, at most 20 |
| 08..14 | four floats | left, right, top, bottom |
| 18..B7 | 20 float pairs | base weight, coefficient |
| B8..107 | 20 int32 | candidate kind; kind 2 has an alternate profile |
| 108..1A7 | 20 float pairs | main/alternate top profiles |
| 1A8..247 | 20 float pairs | main/alternate bottom profiles |

The public snapshot retains all 20 slots because native code computes all 20
scratch weights, including slots outside the active count. Finite input and
ordered box bounds are an explicit portable safety contract; the vendor setter
itself does not provide these validation guarantees. `CopyPreparedSlots` returns
a copy. The independent typed constructor pads omitted slots with zero.

## Preserved iteration and scoring behavior

The initial parameters are height scale 1, slope 0 and intercept 0. Known-glyph
weight scaling, count blending and optional width clipping use binary32
operations before promotion to double. Candidate base weights are squared;
marker weight uses the promoted binary32 literal 0.01, and the coefficient term
is multiplied by zero in the observed function. The first pass adds both
profiles at half weight for kind 2. Subsequent passes choose the better profile
using reciprocal absolute residual affinity and reweight by half that affinity.

Termination requires at least ten solves and squared parameter change below
`1e-10`. If no convergence occurs, pass index 250 performs the native first-slot
initial-style solve and terminates after 251 solves. The observed fallback pass
uses slot 0 even if an item's active count is zero. Zero total mass or zero
height scale rejects the primary fit; the portable port also rejects arithmetic
overflow. It has an explicit 256-item budget and cancellation checks.

The final public float outputs preserve native arithmetic:
`baseline = intercept - heightScale`, `xHeight = heightScale * -2`, and `slope`.
No smallest/largest glyph-box selection is introduced.

The native final affinity has an unusual implementation that is preserved:
assembly `180021c93..180021ccb` normalizes only the **last item's** top/bottom,
while `180021db2` reads a single scratch weight array that was last filled for
that item. The outer loop then visits every item's profiles without refreshing
either geometry or weights. The port therefore reuses last-item geometry and
all 20 last-item scratch weights, averages over item count, and caps by the
incoming score. `RawProfileAffinity` names this raw diagnostic value and does
not claim calibrated recognition confidence or an intended per-item score.

## Scope needed for connected-word font sizing

Document `180053c60` creates Text 2DA and invokes `1800531b0` before recognition
to populate line records at driver F8/count 100, stride 38. Its helper
`180052e90` invokes this C1A fit and stores baseline/slope/x-height as doubles in
the 56-byte source-line record. `1800531b0` is the genuine upstream producer of
the prepared 584-byte items; its glyph/category profile construction and the
complete caller configuration remain unfinished. This port cannot substitute
a stroke bbox for those profiles, and does not yet remove connected-word bbox
fallback in the application.

Document `180060820` is a separate **postglyph** affine/fraction metric refresh;
its prepared glyph metric port is not evidence that raw connected handwriting
features have been restored. The Ink wrapper `1800221a0` also has a separate
acceptance/minimum-height/bbox fallback policy that is deliberately not exposed
as successful primary inference here.

## Independent controls

`tools/LegacyCharacterLineFit.Probe/reference_controls.py` generates 384 original
prepared cases with an independent NumPy design-matrix solve on each IRLS pass.
It checks known/marker weights, clipped geometry, main/alternate profiles,
bounded slope and force-zero branches, final last-item scoring and incoming
score caps. Fifteen cases exercise the 251-pass fallback. The probe additionally
checks all 20 slots of the 584-byte layout, the 24-byte configuration, a known
baseline 100/x-height 80 result, immutable snapshots, malformed input, arithmetic
overflow, cancellation, zero mass and empty-fit rejection. These are numerical
prepared-stage controls, not handwriting or font-size accuracy measurements.

```powershell
python tools/LegacyCharacterLineFit.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/character-line-fit-controls.json
dotnet run --project tools/LegacyCharacterLineFit.Probe -c Release -- D:/BlurMask-build-artifacts-20261011/character-line-fit-controls.json
dotnet publish tools/LegacyCharacterLineFit.Probe -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/character-line-fit-aot
D:/BlurMask-build-artifacts-20261011/character-line-fit-aot/LegacyCharacterLineFit.Probe.exe D:/BlurMask-build-artifacts-20261011/character-line-fit-controls.json
```
