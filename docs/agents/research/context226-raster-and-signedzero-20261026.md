# Native226 raster and signed-zero corrections

Two separate numerical differences are now reproduced and corrected in an isolated 945-source candidate. Neither changes models, input coordinates, thresholds or limits.

The raster kernel must choose its raw extent by comparing the rounded float `width + 1` and `height + 1` values. The previous `Math.Max(width, height)` differs when these rounded values tie despite unequal raw extents. The first one-file correction reproduces that native branch.

Its real-method probe then exposes a second, independent discrepancy in the unchanged degenerate branch. Original native instructions load zero and subtract the vertical reference; managed code used unary negation. For a positive-zero reference, subtraction produces positive zero while negation produces negative zero. The separate correction uses `0f - verticalReference`. The native instruction chain and exact PE bytes are pinned; this is not an oracle adjustment or a zero-bit mask.

ROOT composes the two exact source inverses over the same baseline, authenticates all 945 library sources and the probe through PDBs, and reruns the original unchanged 25-case fixture. All 750 float-bit lanes and 750 quantized-short lanes now match. Nine cases have differences from the old implementation. The original library passes its before expectations and refuses the native-after expectations; the corrected library passes after and refuses before. Independent signed-zero controls also pass. No target-derived expected values were generated.

The initial raster-only candidate's signed-zero failure remains preserved. No model or handwriting-quality run has yet been performed on this combined geometry candidate, and the separately verified chronology-factor candidate is not silently included.

Evidence under `E:/BlurMask-build-artifacts-20261014/`:

- `ru-context226-rounded-extent-correction-source-20261026/manifest.json`, SHA256 `7aaeb88e4af0c5c89c222d803527eaf500ac838191afa1e4275a11099e97fed2`.
- `ru-context226-degenerate-signedzero-correction-source-20261026/manifest.json`, SHA256 `6ed8059327746ae63c08dce504fc854c4ee3d53cca5933f952522c0e9fd7dafb`.
- `root-context226-extent-verification-20261026/report.json`: preserved raster-only first failure.
- `root-context226-combined-verification-20261026/report.json`: original-fixture real-method verification.
