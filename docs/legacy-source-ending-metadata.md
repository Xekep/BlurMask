# Ordinary source ending metadata

`LegacyOrdinarySourceInk.ResolveEndingMetadata` restores the ordinary source's
second metadata owner (`108`), separately from the start-key owner (`F8`). It
uses the actual prepared identity's key-record index, after the existing
fixed-point ceiling and `D8` lookup. Duplicate runs end at the next retained raw
ordinal minus one; the final run ends at the original physical input count
minus one. Original counts are owned snapshots.

Static evidence pins the Text binary SHA-256 and compares all instructions in
`168D00` and `168E10`: addressing is identical except for the final metadata
owner. It also checks the two metadata writes in `167210`. The vendor binary
is read as data and never loaded or executed.

The independent Python controls cover 512 raw inputs, 10,238 retained keys,
30,714 fractional prepared lookups and 4,249 different start/end ordinals.
The managed probe checks ownership, unchanged metadata indices, remainder
bytes, final dropped duplicates and cancellation. Native AOT runs the same
independent controls.

This is not the full `1A9AE0` source-range dispatcher: its graph context and
range ownership remain a separate stage. No recognition accuracy or complete
Russian cursive support is implied by these metadata checks.
