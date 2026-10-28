# Publication CI and app integration candidate, 2026-10-26

This is isolated integration evidence for the production935/meterV2 candidate. It does not admit the unfinished English949/Russian diagnostic work or prove recognition quality. Main HEAD remains a746d25230db4d72d80b59bf78a51c9ddc9f1396.

Candidate `E:/BlurMask-build-artifacts-20261014/root-production935-main-candidate-20261026` applies the reviewed 73-target admission packet a8a05b4d5283805a2202702746b8f8a0cfd0d6601dd9d7465410b833fa1e3825, followed by the one-argument runtime clarification 190f90544c4856dd943d423a671d76aae2756ea5265635d61d0788a5ca373c90. Exact preimages, final library sources and model bytes were verified before testing.

The new CI driver passed in 277.95 seconds: independent default and diagnostic builds, complete PDB/source bindings before any actual recognition, default six-case capture, diagnostic six-case capture with trace off and on, and independent paired audit. The child processes inherited DOTNET_ROLL_FORWARD=LatestPatch; explicit runtime 10.0.12 was used. The separate desktop app Release build passed in 226.83 seconds with AOT disabled; the app was not launched. This is not app NativeAOT/package or cross-platform runtime evidence.

Static PDB refusal controls accepted both authentic cohorts and rejected mismatched PE/PDB pairs in both directions, a missing expected authored file, a foreign generated-source root and reclassification of authored files as generated. These controls did not load any compiled assembly. Shell syntax validation passed 45 Bash blocks/scripts; PowerShell parsing of the native-check script also passed without executing it.

Runtime-policy correction: the original claim that inherited LatestPatch could override an explicit --fx-version was wrong. Official .NET 10.0.12 host source applies SetFxVersion after config/environment parsing and disables patching/roll-forward. The extra --roll-forward Disable is redundant clarification, not a fix for a demonstrated gap. A separate numeric-only observation confirmed selection of installed 10.0.11 without the extra flag and 10.0.12 with it, under inherited LatestPatch. No model ran in that observation. The CI helper validates selected runtime availability, configuration and build artifacts; it does not hash all installed host/runtime binaries.

Evidence roots under E:/BlurMask-build-artifacts-20261014:

- root-production935-main-preparation-20261026/report.json
- root-production935-ci-driver-test-20261026/report.json and flavors/report.json
- root-production935-app-build-20261026/report.json
- root-production935-pdb-refusal-controls-20261026/report.json
- root-production935-runtime-selection-observation-20261026/report.json
- production935-ci-runtime-policy-correction-20261026/manifest.json

The Windows NativeAOT publication probe also built successfully (336.79 seconds). Its actual six-case run completed in 1.07 seconds and the entire 420,315-byte capture matched the reviewed managed default byte for byte: SHA-256 d42bb82a0ca1c6df28822fee9ee86213d6ba98226f96c6143cb2dab1d3e5d77c. The independent paired publication audit passed. Evidence: root-production935-publication-aot-20261026/report.json and root-production935-publication-aot-parity-20261026/report.json. This proves parity for that probe and those inputs, not the whole app, every model path or another operating system. Complete project CI, app AOT/packaging and release checks remain required. No new version has been released from this candidate.
