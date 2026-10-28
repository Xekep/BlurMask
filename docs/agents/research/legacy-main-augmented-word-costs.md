# Augmented WORD cost transfer

`LegacyPreparedMainAugmentedWordCosts` ports219580's post-chain cost calculation.
It reads compact+14/+18 from the actual selected row, subtracts the last direct
parent when it is not−1, retaining signed32-bit wrap, and uses sequential
float32 conversion/division/multiplication. The primary denominator is
`(lastSourceNode + 1 - firstSourceNode) * 8192`. Source kind135 uses negative5
for WORD+10; other kinds use negative1. WORD+1C uses the unnormalized secondary
delta divided by8192. Negative costs are retained, including signed zero.

This is a prepared calculation, not candidate eligibility,218A30/218560 success,
275 insertion, matched-WORD replacement or accepted recognition. The later
matched-word score decrement1e−5 is deliberately not applied here: its native
branch and existing-WORD comparison must be joined by the owning orchestrator.
Inactive compact allocation bytes are preserved and cannot be named as rows.

The independent Python oracle checks all12 result bytes for16384 controls,
including integer wrap, parent−1, float32 rounding beyond2^24, source-kind
selection, negative deltas and span extents. The static auditor pins the full
219580 region,20 instructions and all4 SSE constants. No vendor code runs.
