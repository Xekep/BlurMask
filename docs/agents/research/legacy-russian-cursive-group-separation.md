# Ordinary cursive candidate grouping cost

`LegacyCursiveGroupSeparationCost` restores the ordinary branch of sampler297
`180193860`, forwarded by `180195520`. This is an elementary geometry score for
an explicitly supplied, ordered candidate group. It is not a complete native
word search or evidence of connected Russian handwriting accuracy.

## Confirmed source path

The frontend222 virtual128 (`180152790`) delegates to acoustic21d virtual58
(`1801c0fd0`). That scorer reads a class cost from group-cache slot0 and a
composite-category geometry cost from slot2. The producer `1801be930` fills slot2
through source21b virtual1b8 (`1801ad9b0`), which forwards to sampler297 virtualc8
(`180195520`). The latter calls `180193860`, caps its result by sampler+118 and
writes the same result to every composite category (35 in these Russian
profiles). Consequently this cost affects alternative grouping, although it
cannot change class rank within a fixed ordinary group. It is not one of the
seven neural output channels. The acoustic scorer has special-symbol bypasses;
those caller rules are outside this component.

For each boundary between supplied ranges the function unions all preceding X
bounds and all following X bounds. It computes the larger of the two signed
separations, divides by the explicitly resolved source line height, subtracts
sampler+68 and multiplies by minus sampler+108. Negative separation means the
unions overlap; spatial and chronological orders are not interchangeable.

The transformed value uses three branches: above4 gives zero, below−4 uses the
unclamped linear value, and the interval[−4,4] indexes the256-float lookup at
`1802e1090` with `truncate((x+4)*255+.5)>>3`. The negative log value is multiplied
by8192. For fewer than9 ranges native `18024d8a0` rounds float bits with halves
away from zero and preserves already integral floats above2^23. That branch
saturates each float sum comparison at0x4000000. The branch for9 or more ranges
uses float±.5, truncation and signed addition. This checked port rejects signed
overflow rather than reproducing wraparound. The final score is capped by the
explicit profile maximum.

## Resource and API

`LegacyCursiveSeparationProfile.ReadV106Node` supports the proved uncompressed
type297 version106 layout. It delegates the absolute-aligned old201 category
reader, then reads the grouping fields from the configuration that follows it.
Reader `180196500` places the serialized fields at runtime+68, +108 and +118;
initialization `18018d920` preserves them. Both nodes in the committed Russian
resource (`11506` and `188157`) contain gap offset0.2, slope3 and cap0x4000000.
The API does not infer or hardcode them as universal defaults.

`Evaluate` accepts prepared float ink, ordered half-open ranges, positive
resolved line height, those resource fields, and an explicit256-entry lookup
snapshot. The engine lookup is not included in the `.res` or supplied by this
class. An analytic log-sigmoid curve is not byte-exact: several native table
values were rounded during generation. Vendor-free controls deliberately use
their own analytic table. A second local diagnostic uses an ignored static-byte
snapshot of the confirmed native table; no vendor DLL is loaded or executed.

The native nonordinary group-kind branch returns zero. Native special source
metadata and missing/current/previous line resolution remain outside this API;
the caller must supply an applicable ordinary candidate and its resolved line
height. Bounds256 ranges and checked finite inputs are defensive limits of this
port, not recovered SDK limits. Caller range order and composite-category
selection are not guessed here.

## Verification

```powershell
python tools/LegacyCursiveSeparation.Probe/reference_controls.py .artifacts/group-separation-controls.json
dotnet run --project tools/LegacyCursiveSeparation.Probe -c Release -- .artifacts/group-separation-controls.json models/ru-RU/ru_RU-ak-cur.lite.res
```

The independent Python float32 oracle generates768 cases spanning both range
branches, prefix/suffix overlap, table and linear/zero tails, and caps. It also
constructs64 own serialized sampler profiles at all four absolute alignment
positions. The probe verifies version/compression/category/truncation guards,
nonfinite or invalid coefficients, cancellation, actual resource forwarding,
and the high-magnitude rounding regression8388609. Managed and Windows Native
AOT both pass these controls and the optional static native lookup variant.
This phase does not automatically connect grouping costs to the AI-pen UI,
does not recover complete beam/context policy, and makes no full-word or
cursive-quality claim.
