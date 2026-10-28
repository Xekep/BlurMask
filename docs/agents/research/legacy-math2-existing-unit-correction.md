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

The final association lookup maps the compressed matching row through the
observable-to-original map. Each resulting original output index is duplicated
twice; unmatched pairs retain uint FFFFFFFF. These are indices, not four-byte
symbol values. Caller0426F0 binds the map as040CD0 parameter7;0426F9 binds the
separate output vector as parameter6. Instructions040ACB..AD5,040C22..C3D and
040F10..F8F must be interpreted with these actual caller bindings.

Limits are6000 existing units,1000 output units and1000000 input matrix values.
Probabilities must be finite in[0,1]; unsupported pair layouts are refused.
These are named library bounds, not native source or process-memory limits.
Input and output inspection copies do not mutate retained results. Cancellation
produces no accepted partial result. Native allocator failure prefixes are not
represented by these copied arrays.

Managed and fresh NativeAOT captures match SHA256
`582aad09e908d747d11f393a277deeefdd7de336e02c0c7ebfb5908531fc1960`.
An independent Python oracle verifies2048 prepared corrections,4996 output
rewrites,3605 original-row index discriminators,
2103 unmatched pairs and14 guards. Every average F32 value, selected pair,
output byte and expanded association is checked. A static auditor pins three
complete correction/caller regions,44 instructions and the half constant, and reruns
the four-region matching proof without executing vendor code.

Actual existing-expression provenance, owning record integration and accepted
formula output remain separate. Prepared
[insertion](legacy-math2-inserted-units.md) and
[auxiliary rows](legacy-math2-auxiliary-rows.md) are checked separately.

The first local implementation misidentified parameter7 as the output vector
when reading the isolated helper. Caller binding analysis corrected that
interpretation before application integration or push. The independent oracle
and static evidence now include the caller, not just the helper bodies.
