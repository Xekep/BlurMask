# Ordinary source chronology and context geometry corrections

Ordinary source construction now retains the native factor of 200 at both ingress sites. The generic prepared-source default remains 1. The chronology regression preserves its generic-factor case and separately verifies ordinary-factor ticks, keys and remainder metadata.

Context raster scaling compares the rounded width-plus-one and height-plus-one before choosing the unrounded extent. Equal rounded extents retain width. Degenerate context normalization uses subtraction from positive zero, preserving the native positive-zero bit pattern.

The combined 935-source library passed the original 25-case context fixture (750 float-bit lanes and 750 quantized lanes), eight ordinary chronology cases, the existing chronology regression, and two existing 512-case floating-context regressions. The application compiled without launching its GUI. Nine unchanged Russian requests completed; three were accepted and six still failed the unchanged glyph-score policy. These changes do not establish Russian cursive quality or complete the engine port.

Run the focused synthetic regressions from the repository root:

```powershell
dotnet run --project tools/LegacyContext226RoundedExtent.Probe -c Release -- tools/LegacyContext226RoundedExtent.Probe/fake-inputs.json after
dotnet run --project tools/LegacyOrdinaryChronologyFactor.Probe -c Release -- tools/LegacyOrdinaryChronologyFactor.Probe/fake-maps.json after
```

The fixture expectations were independently prepared before compiling the corrected library. Neither test uses model resources or executes vendor binaries.

The historical D0 budget auditor retains its original source hashes and arithmetic. It reconstructs the two historical chronology source files only from these exact reviewed successors, verifies the reconstructed full hashes, and reports both identities. This is a historical source view, without a claim that its old runtime budget proves the corrected runtime. Both original Git preimages and both successors pass; eight corrupted-source or incorrect-target controls are refused.

## Core951 composition regression and repair

Commit `11c647a` inadvertently dropped the explicit `sourceFactor:200` argument
from physical `LegacyOrdinarySourceInk.Prepare` while importing its private
metered metadata methods. `CreateOrdinary` still used200; the mixed200/1 state
therefore failed the existing after-mode chronology test. Git baseline49045312
and the parent of11c647a both contain the argument. This is a composition error,
not a change to the prepared-source API or recognition policy.

An independent read-only review reauthenticated36 frozen source/native files,
the original Text PE/full decompilation and all five scoped PE spans/table joins.
Native secondary16A130 stores200 at+128; the actual219/218 initialization and
V30/V90 chain retains/consumes that same field. The corrected physical factory
is limited to its existing source kinds135..137. Generic `CreatePrepared` still
defaults to1. Only the missing argument is restored; private metering remains.
The corrected canonical-LF source SHA is
`4acecbd2dfb380ea9995cccc57776ea346a710736fffad4016fb525006d79d3d`.

Root reproduced the failure with the actual pre-fix Core951 and unchanged
original probe, then verified the one-argument candidate. Original expectations
pass:8 cases/11 stages/56 points,1024 unsigned-wrap map kernels,768 prepared
sources,42,823 identities and768 signed/tie range orders. Native AOT outputs
equal Managed. The Native factor entry changes only its final JSON serialization
to an explicit equivalent formatter; every numeric assertion and fixture is
unchanged. No model invocation occurs in these numeric controls.

Actual Main default951 and diagnostic953 were rebuilt and PDB-authenticated.
All33 strict source controls and three paired publication calls pass. The
checkpoint14 additions/67 overrides,13 protected files, Frame26 revision and
935-source baseline remain pinned. A separate one-file revision anchors the
corrected body to its exact imported checkpoint; the erroneous old body is not
accepted as current. Historical D0 source authentication separately removes only
the exact private-meter delta before its existing factor-unit inverse. It does
not claim that old runtime budgets cover the corrected source.

Actual Main also passes the original numeric entries and genuine owning EN/RU/
Math checks: the three-point English capture is identical after validating its
fresh UUID, all nine original two-second Russian calls retain complete data
apart from wall time, and Math87 retains its byte-identical1156-control capture
and all seven independent audits. This repairs chronology metadata; no cursive
accuracy improvement is established by these narrow controls.

Evidence root: `E:/BlurMask-build-artifacts-20261014/ordinary-factor-regression-20261029`.

| Receipt | SHA256 |
| --- | --- |
| `MAIN-GATE-FINAL.json` | `072a1cbd053f98eb916f6ef13833934aadc6b62332bed2e324e2f94b927dacb9` |
| `main-numeric/FINAL.json` | `acefc93d7135a894cfbefe6d7462a952713e048d5e52a956188d9ec79e601b08` |
| `main-owning-regressions/FINAL.json` | `c8165ab82bb3a93353d309c5033f8b191cabe21fb825583ba25fde0eb6efa51f` |
| `native-verification/FINAL.json` | `fdd45835ea207e742180541c73217273c7cdf2ce196f3716b90ca4530d3da716` |

This repair is after the separately published1.0.0 tag; it does not complete
public English admission, Russian cursive or full original-result application.
