# Explicit integer acoustic-cost ensemble

`LegacyAcousticCostEnsemble` restores the inspected scalar portion of
`1801b9e20` for one ordered seven-slot acoustic profile. Each nonnull slot
supplies quantized probabilities and an explicit nonnegative family weight.
The caller also supplies a finite positive cost multiplier. These parameters
are not inferred from model kind or recognition mode.

In native code, model weight comes from the slot's kind: zero selects the
slot's own float at offset `3c`, one selects the recognizer float at `1b8`,
and two selects the float at `1bc`. The weight sum uses these original values.
If the optional multiplier branch at `1a0` is enabled, the individual
contribution weight is subsequently multiplied by the float at `c`.
`Combine` exposes the resolved weight and multiplier directly rather than
guessing those runtime values. The native outer loop can activate multiple
profiles; this component intentionally handles only one explicit profile.

For each enabled class, the scorer clamps zero probability to one and obtains
a cost through [`LegacyAcousticFixedLog`](legacy-russian-acoustic-fixed-log.md).
An individual weight exactly equal to one adds that integer cost directly.
Other weights multiply the cost as binary32 and truncate to integer before
adding it to the running integer sum. The sum is normalized after all slots:

- A weight sum of one preserves the accumulated integer value.
- A weight sum of two uses an integer right shift by one.
- Other positive sums multiply by a binary32 reciprocal and truncate.
- A zero sum or excluded class produces `0x4000000`.

Final costs are capped above at `0x4000000`. The multiplier affects the
contributions, while the normalization denominator still contains original
family weights. Combining calibrated class confidences first and then taking
a logarithm would produce a different score.

The bounded API accepts exactly seven ordered slots and matching output widths
one through 512, with optional matching class-mask bytes (zero is excluded).
It validates probabilities zero through 32,768 and finite nonnegative weights,
supports cancellation, and rejects float or integer overflow instead of
depending on unchecked native overflow. Negative-weight native behavior,
alphabet special-label exclusions, related-form penalties, optional active
profile gating, and complete language-cost addition are unsupported.

The vendor-free probe checks scalar analytic examples, the distinct one/two
normalization branches, numerator-only multiplier, masks, zero sum, caps,
invalid dimensions/probabilities/coefficients, overflow and cancellation.
An independent Python generator checks 640 original bounded randomized
profiles with step-wise binary32 arithmetic. Managed .NET and Windows Native
AOT pass all 640 controls and the analytic/invalid-input checks.

```powershell
python tools/LegacyCostEnsemble.Probe/reference_controls.py .artifacts/own-cost-ensemble-controls.json
dotnet run --project tools/LegacyCostEnsemble.Probe -c Release -- .artifacts/own-cost-ensemble-controls.json
```

This component is not connected to the application's AI pen. It establishes
integer acoustic score arithmetic, not Russian handwriting accuracy. Native
mode selection, complete history/category handling and connected-word cuts
remain open.
