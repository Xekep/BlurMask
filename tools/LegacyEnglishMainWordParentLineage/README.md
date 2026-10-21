# English word-parent static evidence and numeric diagnostics

See [the ownership proposal](../../docs/english-main-word-parent-lineage-20261021.md).
These are static PE checks and prepared numerical projections, not an executable
word decoder. No SDK DLL is loaded. Whole-word ownership and the outer290/case
wrapper remain separate prerequisites.

`audit_native.py` takes `--repo`, `--sdk`, `--models`, `--output`, and current
`--acoustic-134`, `--acoustic-100`, `--d0-134`, `--d0-100` files. Their producer
audits must already have passed. This script independently checks the geometry,
field masks, scorers and native bytes; it does not replace those producer checks.

The two Managed diagnostic projects reference an explicitly built library via
`-p:InkRecognitionAssembly=ABSOLUTE-DLL-PATH`. `ExistingKernelDiagnostic` takes
an acoustic capture, separation profile and output file; its output is an array.
`BackendDiagnostic` takes the acoustic resource, pooled D0 capture, geometry JSON
and output file. Its geometry input is an object with a `rows` array, as produced
by `reference_geometry.py`; an existing-kernel array can be wrapped in `rows`
after independent equality checks. Do not substitute pristine NN costs for D0.

The recorded JSONs identify the frozen Windows numerical evidence. They contain
no transcription targets. The standalone scalar references and source pins state
the limits of these checks. NativeAOT and genuine leased-row runtime integration
need their own tests; these Managed projections do not establish either one.
