# English: failure before the first glyph refresh

The experimental 950 build includes the reviewed traversal meter and scalar marker-flags change. Compilation and synthetic controls pass. Running the unchanged original three-point input with the same resources still fails its existing work allowance in `GraphGlyphChoices`; this is an unresolved failure, not a recognition success.

A separate observation of the same failure shows zero completed word choices. The failure occurs before the first cold glyph refresh. The new character flags, descriptor batching and source traversal meter are not reached on this example, so they cannot be credited with fixing its current bottleneck.

Observed cumulative charges are payload 8,026,613, copied bytes 44,542,369 and visits 16,766,242. The attempted refresh requests another 20,992 visits and is refused. These values match the earlier compact-index failure. No input, expected label, recognition threshold or work limit has been changed to obtain a passing result.

Evidence under `E:/BlurMask-build-artifacts-20261014/`:

- `root-english950-marker-flags-verification-20261026/report.json`: compilation and synthetic flag controls.
- `root-english950-marker-flags-actual-20261026/report.json`: preserved genuine failure.
- `root-english950-marker-flags-failure-progress-20261026/report.json`: diagnostic observation of the failure boundary, not a passing recognition result.

The separately proposed scoped empty-auxiliary output allocation change preserves the existing visit charge. It therefore has no established claim to resolve this visit refusal. English cursive readiness remains unproven.
