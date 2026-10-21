# Actual Russian FIRST candidate arithmetic audit

A new headless diagnostic replays the frozen261 recorded coordinate inputs through the **existing public owning first-source API**. It adds no old runtime seam, source tap, model/beam change or expected-word input. Its purpose is to test an actual error-producing pipeline before attributing remaining recognition errors to a numerical transfer bug.

## What is actually observed

For each owning first invocation, the diagnostic reads the actual retained prepared cut coordinates, fitted acoustic frame, normalized/merged short geometry, selected source ranges, class mask and original `GroupScores` NN probability/cost arrays. Context/sign readouts are recomputed through the same public methods against that retained owner; they are not advertised as a newly exposed private tensor buffer. All261 recorded inputs are one continuous physical track, so their candidates have one contiguous physical range, empty prior-window context and no pen-up merge. This evidence therefore does not cover a multi-stroke context or discarded real pen-up events.

The independent Python audit recomputes subtraction/multiplication/division with explicit binary32 rounding and native truncation, checks each normalized short, verifies actual range endpoint signs and one-track context, then independently computes the complete217/199 native feature arrays from the captured merged geometry. Separate int8 matrix arithmetic, hidden LUT/EXP conversion and seven-slot integer-cost arithmetic must match every **original owning candidate's** probabilities and costs. It does not claim that private first-stage feature buffers were captured directly. Direct217/199 buffer parity for the four new zero-extent candidates is separately tested by the preceding degenerate-feature runtime milestone.

## Complete recorded result

- 261 inputs,32,530 actual candidate groups.
- 1,827,230 selected normalized points match independent binary32/native-short arithmetic.
- 13,532,480 independently computed feature channels feed the output oracle.
- 11,710,800 original integer NN probabilities and5,855,400 original costs match.
- Four collapsed candidates use the separately proven native degenerate assignments.
- Observed short coordinate range -494..1279; no upper saturation or lower wrapping-domain boundary is reached.
- Managed and NativeAOT complete captures are byte-identical:112,211,070 bytes, SHA256 `7248be1049aa35f64b8c441ac63bae1445fd3435916837b6a2fbf4a8fa6cfc98`.

No expected labels were opened by this diagnostic or oracle. Both use pinned resource SHA256 `7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978` and blind input SHA256 `c8fe150f3ca960cf7216da88c1dd90e1045aed2ef4652a8fa66c9e7a85353dce`.

This is a rigorous negative finding: after the degenerate repair, no further FIRST normalization/feature/NN/cost mismatch was found on these actual candidate streams. It is not a running-vendor comparison, full native-engine equivalence or a claim that all Russian text works. Cut placement/graph coverage, downstream pruning/ordering, language choice and model/data limitations remain separate. Correct candidates sometimes losing a genuine native bounded heap is not permission to enlarge its quota or change weights.

The captures remain only in `E:/BlurMask-build-artifacts-20261014/review-russian-first-candidate-audit-20261021`. The frozen manifest records their hashes and dependencies; no large coordinate capture is added to the repository.

```powershell
dotnet run --project tools/LegacyRussianFirstCandidateArithmetic.Probe -c Release -- models BLIND-COORDINATES.json CAPTURE.json
python tools/LegacyRussianFirstCandidateArithmetic.Probe/audit_output.py REPO-DIRECTORY CAPTURE.json
```

Next investigation: actual CUT/graph-key/range-chronology producer fidelity. Preserve native limits and avoid output-label tuning. Native default0 to100 is separately proven and unchanged.
