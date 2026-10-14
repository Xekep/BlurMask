# First terminal selection

`LegacyFirstTerminalSelection` restores the nonempty terminal scan and scalar
result fields of Text `1f42d0`. It requires owned, complete final merged B8
node tables, with global F8 rows and A4 columns. Primary-only acoustic tables
are not a replacement for this input.

The scan visits rows then columns, stopping a row at its first impossible
cost. It follows E6 trailing-symbol predecessors using packed span bits and
byte9/byteA row/column coordinates, applies F4 to the selected trait record,
and keeps the first strictly smallest signed32 cost. Result identity is the
actual qword18, including zero; equal text is not an identity substitute.
At most A4 distinct identities are returned. Subtraction and trait addition
retain native signed32 wraparound.

When configuration99 and A8 are both false, the first result establishes
reference78; subsequent costs are relative to it. Otherwise the input
reference is retained and raw costs feed the scalar channels. Source kind135
uses multiplier5. Native mass08 is an unbounded exponential of the scaled
relative cost: it can underflow or overflow and is not recognition confidence.
The separate prepared-result score clamp does not apply here.

512 independent Python grids cover1368 emitted choices,151 zero identities,
481 exponential infinities, finite-prefix stops, ties, trailing labels,
predecessor boundaries, signed overflow and snapshot ownership. Managed and
Windows Native AOT agree; exponential finite results allow two float ULPs
between platform math implementations, while zero/infinity must match.

```powershell
python tools/LegacyFirstTerminalSelection.Probe/reference_controls.py D:/terminal-controls.json
dotnet run --project tools/LegacyFirstTerminalSelection.Probe -c Release -- D:/terminal-controls.json
```

This is not the entire1f42d0 method: result273 allocation, character/source
backtracking, frontend callbacks and the empty-source branch remain separate.
The actual B0/B8 producer and subsequent word/linguistic controller are still
required before an application recognizer can use these choices. The probes
are algorithm controls, not Russian or English recognition accuracy tests.
No vendor DLL is loaded. The statically inspected Text binary SHA256 is
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.

The probe additionally consumes an actual raw/NN/B0/B8 captured grid and its
owned RU alphabet, then checks selected cells under two explicit fixture
score policies. Guarded zero traits and absence of E6 prove ranking does not
depend on unproved score-only owner flags in this fixture. The real selected
symbol-context path is still mixed Latin/Cyrillic; no script substitution or
forced reference word is applied. This is not full273 Unicode materialization.
