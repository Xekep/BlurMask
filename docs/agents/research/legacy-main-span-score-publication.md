# Span score publication

`LegacyPreparedMainSpanScorePublication` restores the prepared finite numeric
portion of275.V08/140B90, called at the end of13D360. Ranking alone does not
complete this stage. The operation takes complete120-byte WORD payloads and
returns owned payloads in the same order with only channels16..23 changed.

A private descending index uses the native binary-insertion tie policy.
Processing from the worst index backwards converts score16 to
`exp(score - bestScore)` with native75 bounds. The best and runner-up masses
initialize normalization; later masses follow the native float32 recurrence,
using the prior raw mass and prior normalized value separately. This is not
normalization by the sum of all alternatives. Once a mass becomes zero the
remaining tail is zeroed. A singleton receives1 in both output channels.

For multiple rows, the rank20 channel uses `expf(-(logRank28 - minimumLogRank28))`.
It has no75 clipping and is distinct from score16. These fields do not establish
recognition accuracy or application acceptance confidence.

The portable boundary accepts at most4095 WORDs and finite incoming score16 and
log-rank28. Other native domains and allocation-failure injection are not
covered. Complete opaque fields are preserved. The numeric kernel neither
mutates actual275 span owners nor declares TOP.V68 complete.

4096 deterministic controls cover197502 complete payloads, binary-insertion
ties, empty/singleton cases, more than60 rows and zero-mass tails. The independent
oracle checks unchanged bytes and index order exactly; the two numerical channels
allow4ULP to account for system exponential implementations. Managed/NativeAOT
captures are also compared byte for byte on the test host. A static audit pins
the full140B90 region,11 instructions and275.V08 binding. The vendor DLL and
its CRT functions are not executed; bitwise identity with every native CPU/libm
variant is not claimed.
