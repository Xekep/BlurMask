# PlainWord CI expectation after the first-model correction

The managed recognition suite on all three operating systems reached
`LegacyFirstPlainWord.Probe` and rejected its historical total cost `41268`.
The Russian first model now uses the separately proved mixed EXP/sigmoid route;
the probe still expected its previous plain-EXP score. This is a test expectation
change, not a new recognizer heuristic or a reason to weaken the assertion.

The exact same five-stroke fixture and resources were run against old core
`f04d1a15…` and corrected core `d378766d…`. Instrumentation captured results before
the original assertion, which remained intact. The old run passed and the new
run preserved the expected failure. Both selected the same nodes `[3,6,8]`, arcs
`[3,2,1]` and literal symbols `[22,70,25]`: `M\u0418P`, deliberately mixed script.
This fixture is not evidence of Russian word-recognition quality.

Independent raw-resource arithmetic checked 140 trajectory/raster networks across
the two runs, 25,200 Q15 values and 12,600 ensemble costs. Primary recurrence,
B0/B8 context passes, terminal scan/backtracking and WORD scalar fields were
replayed independently. Geometry corrections were retained source-owned callback
inputs; this report does not claim a new independent geometry implementation.

| Selected group | Old acoustic cost | Mixed acoustic cost | Change |
| --- | ---: | ---: | ---: |
| 14 | 6172 | 6172 | 0 |
| 24 | 16233 | 15877 | -356 |
| 31 | 19020 | 18951 | -69 |

Separation costs remain `[0,7100,0]`; transition corrections remain
`[0,-3780,-3477]`. Therefore the independently reconstructed acoustic cost is
`6172 + 15877 + 18951 + 7100 = 48100`, and the total is
`48100 - 3780 - 3477 = 40843`. Both totals change by exactly `-425`; the geometry
contribution stays `-7257`.

The minimal test-only patch changes `41268` to `40843` and adds the exact selected
path assertion after recognition. No expected text, path or cost is passed into
the engine. All 512 existing synthetic scalar/boundary controls and subsequent
ownership/invalid-input controls are retained. The corrected original probe passes
with the mixed core and rejects the old core as a negative control.

Evidence: `E:/BlurMask-build-artifacts-20261014/ru-first-plainword-ci-oracle-next-20261029`,
receipt `1c55530abc3fafebf505d4b8bcc6c3bf97ad520fb13c3dfba981c16ef3324cf4`.
Root also built and ran the updated repository probe as Windows Native AOT:
35.03-second build and 0.078-second run, both exit zero, CPU2, with all 512 controls
and the actual 35-group fixture passing. Logs and pinned inputs are in
`ru-first-plainword-root-native-20261029`. Neither this fixture correction nor green primitive
tests complete Russian cursive, English publication or automatic recognition.
