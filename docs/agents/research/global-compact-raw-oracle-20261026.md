# GLOBAL compact raw-resource oracle verification

ROOT reviewed and tested frozen source packet `326a02da…`, which changes thirteen existing tools/CI files and adds three tools-only files. Production recognition C# and model resources are unchanged. The new calculation reconstructs source70, empty-history language costs, the per-ordinal publication heap and compact writes from raw resources and captured allocator inputs. Observed language decisions and expected text are not inputs to its expected-value calculation.

The 935-source production library was reused with its authenticated PDB. The eight-case probe was compiled with source authentication for its program and new allocator-input recorder. After removing the five new input-metadata fields, every original captured value matches the preserved earlier run exactly. The new independent raw-resource audit passes all eight full compact buffers, beam rows, alternatives and node allocation counts. Its execution takes 26.528 seconds within the existing 45-second envelope; the probe takes 7.393 seconds.

This closes the specific previously preserved `(0, 43, 29)` compact-byte mismatch without changing production behavior or expected compact bytes. The old oracle constructed fallback candidates but omitted the already-connected GLOBAL language/candidate-heap path. Source controls pass 236 checks; synthetic controls pass 116 positives and 24 refusals. None of these results establishes Russian handwriting accuracy or whole-engine completion.

## LIVE and final publication remain unresolved

Additional LIVE validation initially failed to compile because ROOT's isolated harness omitted existing local and linked test helpers. Those two failed harness reports remain false. The third harness preserves the original project properties and all five authored C# inputs and compiles successfully.

The actual LIVE probe then reaches a genuine comparison failure: `Reference complete scalar storage differs` at its `Compare` function, called from the prepared short case. The preceding 124-, 140- and 240-node native-gate cases complete, and the 244-node second-cycle refusal passes. This failure is not an app or Explorer launch: only the bounded headless probe runs.

Source review identifies three older GLOBAL C# reference call sites that omit the language context. This is an explicit reference-coverage problem to investigate; the eight ordinary GLOBAL cases do not alone prove every intermediate LIVE result. The old references additionally check producer inputs, per-node metadata and query-cache counters, which the new compact-byte oracle does not fully replace. Their assertions must be migrated through independently checked phase state, not removed or broadly masked. No successful LIVE/final or whole-CI claim is made.

Evidence under `E:/BlurMask-build-artifacts-20261014/`:

- `root-global-compact-language-oracle-verification-20261026/report.json`: eight-case compilation, unchanged original capture, raw-resource audit; PASS.
- `root-global-live-final-oracle-verification-20261026/report.json`: first harness build failure, preserved.
- `root-global-live-final-oracle-v2-verification-20261026/report.json`: second harness build failure, preserved.
- `root-global-live-final-oracle-v3-verification-20261026/report.json`: successful corrected build followed by actual comparison failure; overall FAIL.
- `global-final-reference-migration-plan-source-20261026/manifest.json`: source review and assertion coverage plan, not an implemented fix.

Both unchanged legacy modes have now also been compiled with authenticated sources and exercised against their actual resources under the new shared auditor: ordinary prepared and fresh CASE modes pass. Evidence: `root-old-compact-oracle-mode-verification-20261026/report.json`.

The raw-oracle packet is committed and pushed as `c65da2ce61b9a85a2ade004420fa6acead0ac238` on draft PR #1. The remote SHA is verified; main HEAD and its index remain unchanged. The PR description explicitly retains the LIVE blocker. An initial Git admission refused the PowerShell CRLF preimage against the existing LF blob; the subsequent exact EOL relation proves that the new source bytes are already the intended LF blob and that its only normalized changes are three added audit argument pairs. No successor bytes or expected outputs were changed to pass admission. Evidence: `root-global-oracle-git-eol-v2-admission-20261026/report.json` and `root-global-compact-oracle-v2-commit-20261026/push-report.json`. No new release is justified by this verification alone.

Remote CI for this commit now passes the earlier GLOBAL compact audit on Windows, Linux and macOS, then fails the same LIVE scalar-reference comparison reproduced locally. The separate source-geometry workflow succeeds. This verifies the previous gate correction across those systems; it does not make the main CI green. Observation: `root-c65-ci-live-frontier-20261026.json` under the artifact root.
