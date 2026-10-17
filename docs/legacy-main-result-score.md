# Explicit main alternative score channels

`LegacyMainResultScore` restores `2276A4..227750` as an explicit numerical API.
It does not select a terminal, allocate273 alternatives, infer cumulative path
cost or decide whether handwriting should be replaced. Its source count and
kind are caller-owned values, not guessed character or atom counts.

`Evaluate` takes total terminal cost, the main loop's integer reference,
positive source-node count, selected row14 and source kind. It performs signed32
`total-reference` before converting that integer difference to float. The
divisor is native signed32 `sourceNodeCount<<13`, including wrap. Alternative14
is independently `float(selectedRowCost14)/8192`. The normalized path becomes
log mass through multiplier-5 for kind0x135 and-1 otherwise. The inclusive
[-75,75] interval uses double exponential converted to single; lower values
produce0 and upper values the stored single3.73e32. NaN follows the exponential
branch. These are separate path/auxiliary/mass channels, not acceptance confidence.

The native count is controller2310, loaded at22727C; it is not the selected
record's field22, a glyph count or a compact-coordinate index. This explicit API
does not establish that owner relationship for the caller.

The cast order matters: total100000001 and reference100000000 produce integer
difference1, whereas converting both to float first would produce difference0.
The existing `LegacyPreparedResultScore` belongs to225FB0 and intentionally keeps
that other cast order. It is unchanged. Reference generation/flags99/A8 and the
actual cumulative2520 producer remain separate from this helper.

The positive-count check corresponds to the native nonempty branch. Numeric
controls deliberately include large positive counts whose signed32 shift wraps
or becomes zero. The resulting SSE-style infinities/NaNs are preserved rather
than replaced with checked-long arithmetic. Those explicit numeric stress values
are not claims that such source graphs were constructed by the owning pipeline.

`WriteCostChannels` returns one copied40-byte row with only fields08,10 and14
written. Constructor data, row0C, linked WORD/source identities and all other
bytes stay intact. No invented273.V8 constructor or pointer value is supplied.

## Whole-prefix publication

`LegacyMainResultScorePublication.Normalize` copies complete explicit storage,
including any inactive capacity, and processes its actual active prefix. It
implements273.V60=`13CBA0` then `227CCC..227D57`:

- Keep raw mass0 while traversing later rows backwards. Their08 becomes
  `mass_i/(mass_i+raw_mass0)`; first08 uses the final denominator from row1.
  A singleton08 becomes1. This is neither sorting nor softmax.
- Choose minimum row14 with MINSS source-operand semantics, then write
  `expf(-(row14-minimum))` to row0C, without clipping. A singleton0C becomes1.
- Use the **saved raw** first mass for the TOP80 cap: multiply by.9 below1;
  otherwise compute `1-.1/mass`. The unordered COMISS path also takes division.
  Combine with explicit old TOP80 through MINSS semantics, preserving equal
  signed-zero and unordered source selection. Empty active storage changes
  nothing and has no invented raw first mass.

The owner supplies old TOP80. Native13B870/13BB50 initializes it to-1; this helper
does not substitute1 or interpret the sentinel as a usable confidence. Its deep
result storage and raw-first field have no273/source/pool ownership claim.
The existing first-result publication component is unchanged.

## Verification

An independent original Python generator creates896 integer/float cases and430
full-allocation normalization cases.353 controls distinguish integer-before-float
from the wrong float-before-subtraction order. Cases include int32 difference
and shift wrap, both source-kind multipliers, zero divisors from positive counts,
inclusive clamp boundaries, infinities, quiet NaNs, signed zeros, MINSS ordering,
expf overflow, raw-first versus normalized-first caps and inactive/padding bytes.
Ten malformed-domain, budget and cancellation guards accompany copy isolation.

Managed and fresh Windows Native AOT captures match byte for byte on these
vectors. Independent capture audits compare all fields and preserved allocator
bytes. Calculated NaNs are checked by classification rather than demanding a
particular platform NaN payload; exp/expf allow at most2 float ULPs. Other finite
operations, signed zero and nonchannel bytes are exact. Floating exception/status
flags are not an output of this portable API.

`audit_main_result_score.py` reads three pinned PE regions,42 instruction pins,
nine exact data constants and the273.V60 binding. The audit distinguishes
2276BD subtraction from225FB0 rather than copying its semantics. No vendor DLL
is loaded or executed.

```
python tools/LegacyMainResultScore.Probe/reference_controls.py controls.json
dotnet run --project tools/LegacyMainResultScore.Probe -c Release -- controls.json capture.json
python tools/LegacyMainResultScore.Probe/audit_output.py controls.json capture.json
python tools/handwriting/audit_main_result_score.py static-report.json
```

The missing owning joins and next real-source test remain documented in
`legacy-main-result-assembly-roadmap.md`. Complete273 alternative allocation,
selection/exclusion, quote/result normalization ownership and emitted-text
acceptance are not implemented by these numerical helpers.
