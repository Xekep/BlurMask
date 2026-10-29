# Prior Math candidate and default DOM diff

Runs six genuine pipelines through an owning initial application, prior selection,
unchanged current C23, a new neural result, alias-preserving THIRD clone, temporary
old writer, candidate writer, default diff and CB8 filtering. Also tests 961 frozen
prepared alignment cases, typed values, source ownership, cancellation, exclusive
borrows, ancestor closure, one-shot claims and retained diagnostic prefixes.

```
dotnet run --project tools/LegacyMathPriorDomDiff.Probe -c Release -- .artifacts/math-mode0.ikrmath models/math2/math2-unwrapped.res PATH_TO_OFFICIAL_ORT tools/LegacyMathPriorDomDiff.Probe/alignment-fixtures.json .artifacts/prior-diff.json
python -I -B tools/LegacyMathPriorDomDiff.Probe/audit_nn.py .artifacts/prior-diff.json models/math2/math2-unwrapped.res
python -I -B tools/LegacyMathPriorDomDiff.Probe/audit_candidate.py .artifacts/prior-diff.json
python -I -B tools/LegacyMathPriorDomDiff.Probe/audit_diff.py .artifacts/prior-diff.json
```

The actual cases produce three tag edits each, all filtered by CB8. They prove no
kept-action application, changed-input recognition, outgoing update or public
adapter. Prepared alignments confer no document ownership. The executable retains
the existing friend assembly name `LegacyMathFreshPublisher.Probe`; its project
and output directory are separate. CI runs both managed and Native AOT versions
using `tools/ci/check-math-prior.ps1`.
