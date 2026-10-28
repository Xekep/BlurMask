# Prepared Math2 segmentation sigmoid

`LegacyPortableMathSegmentation.ActivatePreparedSigmoid` implements the
036C70 mode0 arithmetic on explicitly prepared contiguous `[symbols,segments]`
attention. It does not obtain attention from a decoder, reconstruct ancestry,
populate completed records or execute EF4. The separate
[producer proof](legacy-math2-postprocessing-producer.md) establishes why that
downstream work is needed for the actual model.

036C70 creates a rank2 F32 output. For each symbol/segment entry,036D20 loads
the source float,036D26 flips its sign bit, and036D29 calls the native EXP
entry1A87C0. ADDSS and DIVSS then calculate `1f / (expf(-x) + 1f)` as separate
F32 operations. The source descriptor stride is110 bytes; contiguous output
order is symbols followed by segments. Ghidra omits the EXP argument dependence
in its scalar decompile, so the assembly is the arithmetic reference.

The port uses `MathF.Exp` as an explicit portable numerical adaptation. Native
1A87C0 can dispatch an FMA implementation and has not been restored here. No
universal vendor bit-equivalence is asserted. Negative finite maximum is valid
masked attention: EXP can overflow to positive infinity, producing zero after
division. This case is preserved rather than refused or clamped arbitrarily.
Nonfinite inputs are outside this bounded adapter. Other036C70 modes are not
silently treated as identity or sigmoid.

Prepared input limits are1..1000 symbols,1..6000 segments and at most one
million total entries. These are portability work bounds, not recovered native
allocation limits. Inputs are validated before output allocation; cancellation
is checked during validation and arithmetic.

Managed and fresh Windows NativeAOT agree on the full capture SHA256
`96cca55b751dd6907fe3aab974c0c348f1df6ebab1b29c6c9100107a683762dc`.
The independent75-digit decimal audit checks1894 values and10 refusal controls,
including signed zero, subnormals, saturation, negative-FMAX masks, transposed
shapes and401 cases distinguishing separate-F32 arithmetic from a folded
high-precision sigmoid. Portable EXP tolerance is2e-7 in output probability.
These prepared controls measure arithmetic, not handwritten recognition quality.

```powershell
dotnet run --project tools/LegacyMathSegmentation.Probe -c Release -- capture.json
python tools/LegacyMathSegmentation.Probe/audit_output.py capture.json
python tools/handwriting/audit_math2_segmentation_sigmoid.py static-report.json
```

The static audit pins the full036C70 region,17 instructions, sign/one constants
and the F32 EXP argument ABI. No vendor DLL is loaded. The preserved Ghidra
export and reference list are under the local research directory documented in
[`research/README.md`](../../../research/README.md).
