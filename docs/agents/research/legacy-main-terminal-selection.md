# Prepared main terminal winner scan

`LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue`
restores the `2257C0` scan with secondary-alternative count0/1 and argument7
disabled. It consumes actual88-byte rows through **SourceEnd23F0**, which is
distinct from the compacted retained extent. It does not truncate the source
extent to hide residual rows.

The caller supplies the selected-source table, its prepared language penalty
values, the cumulative path cost, context owner and literal invocation flags.
The scan preserves finite-cost and27.bit0 eligibility, primary/record5.bit2
classification,24B8's FFF9/FFFB context-symbol exception (including non-root contexts), source category exclusions
and argument4/5 filtering. The first row wins equal costs. Raw row cost14,
published row field22, fallback flag and input/output result flag8 remain separate.

Selected source2D1.V68 is the pure runtime-context2534 getter. Its returned
signed integer is converted to float32, multiplied by stored float32 0.8,
adjusted by positive or negative0.5, then truncated. Arithmetic uses the native
signed32 wrap. The winning relative cost is compared before adding cumulative
path2520. That final sum may wrap or equal a sentinel; the later result
controller still owns its sentinel/acceptance checks.

The independent Python oracle generates1024 full-row scans with signed costs,
source categories, penalties, all invocation gates, sentinel contexts and
unwritten bytes. Additional controls verify stable ties, the full source extent,
nested sentinel contexts, post-selection cumulative wrap, cancellation and malformed extents. The static
auditor pins PE bytes for the getter, SSE operations and float constants.

```
python tools/LegacyMainTerminalSelection.Probe/reference_controls.py controls.json
dotnet run --project tools/LegacyMainTerminalSelection.Probe -c Release -- controls.json
python tools/handwriting/audit_main_terminal_selection.py static-report.json
```

This entry does not restore secondary alternative exclusion lists, the optional
source rescue branch, production invocation options, source penalty preparation,
backtracking or275 publication. It does not establish handwriting accuracy or
application integration. Source selection and the owning ordered decoder remain
required before its prepared result can be used by AI pen.
