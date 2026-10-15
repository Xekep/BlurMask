# Main-source original-symbol confidence calibration

`LegacyMainConfidenceCalibration` restores the EXP scalar branch of13E8E0
using the actual main21D mode1 resource table. It does not build candidate
vectors or select the winning glyph. The full216BC0 result producer remains
unimplemented.

The checked resource stores mode1 at566408, width192 at566412 and float format
0100 at566414. Four active second-only families own192 center/slope pairs each
at[566416,572560). This differs from the180 output neurons. The first180 pairs
of each family match the existing engine profile, but original labels180..191
also have stored coefficients. They must not inherit the coefficients of the
neural alias, or use a default merely because they exceed the neural width.

The helper accepts an explicitly supplied original candidate label, active
family ordinal and quantized EXP probability0..32767. It converts probability
to binary32 with1/32768, promotes that value and the stored float coefficients
to double, evaluates the logistic and casts only the final result to float.
An original label or family outside the stored matrix uses native double0.575
and center0. It does not infer the neural alias, top20 rank, original group,
primitive probability or source-owned channel88. These remain requirements
for the main score-cache adapter.

The independent Python controls read the complete matrix directly from the
hashed resource and generate16,548 scalar references. They cover every stored
label/family,576 extended-label samples and7,332 dimension-fallback samples,
including synthetic quote labels, uint-max family and ushort-max label. The
probe also checks owned coefficient storage, invalid EXP values, cancellation
and corrupt resources. Float results allow one final ULP across platform libm
implementations. A separate static audit pins the getter/loader PE blocks,
unsigned bounds, coefficient indexing/promotions, double constants and the
entire stored matrix; it never executes vendor code.
