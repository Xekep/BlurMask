# Owning temporary application

Consumes a genuine completed default diff through an exclusive synchronous
borrow. The new bridge selects only the next actual kept action under a short
private grant; copied diagnostics and assembled action parameters are not inputs
to this entry. Existing raw arena/views/pure kernels remain trusted internal APIs,
not a protection boundary against hostile code inside the assembly.

```
dotnet run --project tools/LegacyMathTemporaryApply.Probe -c Release -- .artifacts/math-mode0.ikrmath models/math2/math2-unwrapped.res PATH_TO_OFFICIAL_ORT .artifacts/temporary-apply.json
python -I -B tools/LegacyMathTemporaryApply.Probe/audit_output.py .artifacts/temporary-apply.json
python -I -B tools/LegacyMathPriorDomDiff.Probe/audit_nn.py .artifacts/temporary-apply.json models/math2/math2-unwrapped.res
python -I -B tools/LegacyMathPriorDomDiff.Probe/audit_candidate.py .artifacts/temporary-apply.json
```

Six genuine model pipelines have **zero kept actions** after CB8. They exercise
successful empty traversal and header consumption, decoder release, one-shot
claims, revoked/reentrant borrows, pre-cancel and exact budget boundaries. Eight
fault/cancel cases and nine ancestor/self closures verify readiness and retained
header facts. The ActionCompleted stage is deliberately absent from these real
cases; no action is invented to exercise it. Expected total: 203 controls.

Header consumption is not release of the whole diff batch. Snapshots identify the
last observed action boundary; a later partial operation is not labeled completed.
This is not kept-action mutation coverage, original-document application,
current-input C23/NEW NN, outgoing update or completed repeat recognition.
CI runs this probe managed and as Native AOT in the Formula ABI OS matrix.
