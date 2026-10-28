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
