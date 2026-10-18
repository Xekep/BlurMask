# Prepared source-aware assignments

The internal041020 adapter accepts explicit source units, an existing partial
assignment vector, observable original rows, activated probabilities, a segment
count and a separately named all-columns flag. It complements the earlier
fresh assignment kernel; caller flags do not establish a real request policy.

When the assignment count already equals the segment count, the native helper
returns before examining the source-dependent scan. Otherwise all columns are
computed for a nonzero all-columns flag or an empty source. In the ordinary
source-aware branch, zero-byte3 columns retain their existing assignments;
nonzero-byte3 columns insert the original index of their first strongest
observable row at the matching source-column position. Ties retain the first
row. Retained values may include absent index-1 from unmatched correction pairs.

The explicit copied-array contract validates finite probabilities in[0,1], up
to1000 observable rows,6000 segments and1000000 values. A partial input must
contain one assignment per retained zero-flag source column. An observable row
is required for every newly computed column. These bounds and refusal cases are
library safeguards; native allocator prefixes and arbitrary malformed source
states are not claimed.

An independent direct-composition oracle checks4096 cases,569 complete fast
paths,5651 retained indices,5922 tied-column occurrences and13 guards. Two
additional cross-helper controls connect existing-unit correction, source-aware
assignment and insertion: one has an original-row map with a hole; the other
contains an unmatched source pair requiring insertion. They distinguish
original output indices from symbol DWORD values and from compressed row IDs.
Managed and fresh NativeAOT captures match SHA256
`89cb19d00c09d80abbfedfacca950734b5ceded64e71ba6090adf2b098c45e0c`.

The static auditor checks two complete helper/caller regions and28 pins,
including the actual042710..72F bindings for source, filtered tensor,
observable-row map, partial assignment vector and zero final flag. It never
executes vendor code. Actual general-source ownership and full EF4 integration
remain separate work.
