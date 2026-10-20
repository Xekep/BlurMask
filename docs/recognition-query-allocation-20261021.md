# Dictionary query allocation check, 2026-10-21

The dictionary, case wrapper and delimiter readers now reuse bounded private
scratch buffers (16 KiB records, plus 6 KiB case costs). Returned page prefixes
remain independent owned copies. Prepared map storage is immutable and exposed
internally as a read-only span; the public snapshot method still copies it.
The existing nonreentrant query contract is unchanged. This adds no thread-safety
guarantee, ranking changes, model changes or confidence-threshold changes.

On the same 261 recorded Russian inputs as the [blind comparison](russian-blind-comparison-20261020.md),
before/after full Glyph40 records, UTF-8, flags and score bits agree exactly.
Median calling-thread allocations fell from626,637,656 to310,674,944 bytes
per input (50.4%). Maximum fell from2,131,730,624 to946,452,640 bytes.
These are cumulative temporary allocations, not retained process memory.
The retained model delta was about23.2 MB in the baseline harness.

This local Release JIT run observed median199.3 versus161.0 ms, but maximum
1206 versus1249 ms; it does not establish a latency or RSS guarantee. Observed
private bytes were130.7 versus132.7 MB and peak working set194.9 versus183.7 MB.
The application has not yet adopted this complete experimental global tract.
Further allocation hotspots, particularly whole alternative-pool copies during
primary-word recombination, remain. This does not resolve all performance work
or establish Russian recognition readiness.

`LegacyQueryScratch.Probe` checks saved-page/cost-table independence, zero padding,
varying capacities, public map-copy isolation and warmed empty-query allocation
bounds under Managed and NativeAOT. Existing independent dictionary5120-operation,
aggregate12288-page, delimiter512-fixture and case10248-control tests also pass.
The complete before capture SHA256 is
`34ed943ba53f3ee25bb12ba11586bcfdb873d8578be71dae57326557618b6e23`;
after is `0bca96bdcbfc2cec9aa4f2a47b6e206096832b5df4e825584f46b273ed3c2432`.
They remain local diagnostic artifacts; no runtime timing constants are inferred.
