# Prepared existing-unit correction

The internal finite040CD0/0409F0 adapter takes explicit existing four-byte units,
initial output units, increasing observable-to-original row indices and the
activated observable segmentation matrix. It does not manufacture a C25 source,
select a request policy or update an actual completed-record owner.

Source entries with byte3 zero must occur as complete adjacent pairs in this
bounded prepared API. Each pair averages its two original matrix columns with
separate F32 addition and multiplication by0.5. The repeated
[column matcher](legacy-math2-column-matching.md) associates observable rows
with these pairs. The first full four-byte source unit replaces the output at
the corresponding original observable row; all bytes are retained.

The final association lookup intentionally uses the compressed matching row
directly in the updated output vector. It does **not** rebase that row through
the observable-to-original map. Each resulting full uint is duplicated twice;
unmatched pairs retain uint FFFFFFFF. Native instructions040ACB..AD5,
040C22..C3D and040F10..F8F establish these different coordinate domains.

Limits are6000 existing units,1000 output units and1000000 input matrix values.
Probabilities must be finite in[0,1]; unsupported pair layouts are refused.
These are named library bounds, not native source or process-memory limits.
Input and output inspection copies do not mutate retained results. Cancellation
produces no accepted partial result. Native allocator failure prefixes are not
represented by these copied arrays.

Managed and fresh NativeAOT captures match SHA256
`7e37c12c561b90b38be85875df39da518737b71541c47646ca47220968b9a03e`.
An independent Python oracle verifies2048 prepared corrections,4996 output
rewrites,3605 cases distinguishing literal compressed-row lookup from rebasing,
2103 unmatched pairs and14 guards. Every average F32 value, selected pair,
output byte and expanded association is checked. A static auditor pins two
complete correction regions,28 instructions and the half constant, and reruns
the four-region matching proof without executing vendor code.

Actual existing-expression provenance, the remaining EF4 insertion/auxiliary
paths, owning record integration and accepted formula output remain separate.
