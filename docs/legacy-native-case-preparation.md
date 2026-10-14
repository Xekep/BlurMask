# Stored-alphabet case preparation

`LegacyNativeCasePreparation` restores the non-Arabic stored-alphabet table part
of wrapper292 `2324E0`. It owns the prepared alphabet ranges and derives the
absence of the Arabic map from the pinned script data: an alphabet containing
any script5 scalar is explicitly refused. `0573B0` cannot allocate its map when
`04E280` finds no script5 string. No caller supplies the Arabic-presence flag.

The two byte tables include all labels from native −29 through AL count−1.
Mode0/1/2 access uses the stored native triplets and mode fallback, not host
Unicode casing. The implementation retains mode1's missing-second repair,
mode2 substitution, the native second-table zero write when mode2 has no first
form, and the dependence on mode0's third field in the mode1 classification.
Hyphen and extended controls receive their native categories; comma, colon,
semicolon, full stop, exclamation and question mark override only the first
table. Associated six-byte form rows are kept independently of classification.

An independent Python raw-payload oracle covers 433 actual/prepared alphabets
and 95,640 extended symbols, including 128 separately generated three-mode
alphabets. Managed and Native AOT run the same comparisons; cancellation and
outside-domain accesses are rejected. The static audit pins the source DLL
hashes, ten literal scalars and the no-Arabic-map branch.

This is table preparation, not a completed wrapper292: restart/query state,
source factory selection, mutable alphabet form production, Arabic maps and
the full word-winner search remain separate. It does not establish Russian
cursive recognition quality or application integration.
