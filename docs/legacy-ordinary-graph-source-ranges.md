# Ordinary graph source-range callback

`LegacyOrdinaryGraphSourceRanges` restores the successful ordinary forward
mode-zero branch of `21B.virtual120 / 1A9AE0`. It owns both graphs generated
from the supplied ordinary seeds. A selected candidate supplies graph context
IDs; an ordered seed-path search (`1A9070`) finds the first matching path.
Normalized acoustic group ranges are not substituted for raw membership.

`1A8E60` maps each prepared range through the source's metadata. Start,
ending-key and ending-start-key owners remain distinct. The physical-tail
flag selects which ending owner is returned. Rows use the native sort order
and reverse coalescing, including the next `F8` key record rather than a
guessed raw ordinal plus one. This preserves dropped duplicate runs and
can merge adjacent physical strokes into one inclusive source range.

The independent Python auditor implements recursive graph search, a bubble
comparator and reverse coalescing. Managed code uses an iterative search and
stable sorting. It checks 5,831 callbacks from 512 independently generated
raw inputs and 18 actual Russian resource/frame/cut cases, including duplicate
points: 5,289 merges and 2,296 different duplicate-tail keys. Native AOT runs
the same inputs and exports a separate audit for independent verification.

The static auditor pins the Text binary SHA-256 and five callback bindings.
It reads the binary and decompilation without executing vendor code.
Reversed contexts, protocol source records and exceptional source kinds
remain unimplemented and are refused. This stage does not choose the final
recognition winner or complete the main text controller.
