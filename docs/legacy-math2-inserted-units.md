# Prepared Math2 inserted units

The internal finite041240 adapter takes explicit existing source units, current
output units, segment assignments, observable original rows, activated
probabilities and a replacement U16. It returns copied units, updated assignments
and the initial insertion mask. It does not select a request or authenticate an
actual source/record owner.

The mask starts with zero for every current unit. For an absent assignment the
insertion position is the preceding assignment plus one, or zero at the start.
A zero-byte3 source pair retains its first full four-byte unit, shifts assigned
indices and assigns both adjacent positions to the inserted unit. The bounded
API requires both members of that missing pair to be absent.

For a nonpair source position, the native algorithm sums F32 probabilities over
all nonzero-byte3 columns for each observable row, retaining the first strongest
row. It copies that row's current output through the original-row map. With no
existing source or no observable rows it constructs `01000000 | replacement`.
This branch fills **all** remaining absent assignments, including nonadjacent
positions; it is not restricted to one contiguous missing run. The inserted
unit's mask byte is one, with prior bytes retained in the same order.

The prepared API bounds6000 segments,1000 output units and1000000 probabilities
in[0,1]. Before copying payloads it checks a conservative output envelope:
initial unit count plus absent assignment count must not exceed1000. This can
refuse a larger source whose actual insertion count would be smaller; it is an
explicit library safeguard, not a recovered native limit. Unsupported pair
layouts are refused. Native allocation-failure prefixes are not claimed.

An independent token-based oracle keeps stable unit identities and resolves
indices only at the end, avoiding the production index-shifting algorithm.
It checks4096 cases,4133 inserted units,1402 pair branches,2731 all-absent fill
branches,320 observable-row-map distinctions and14 guards. Managed and fresh
NativeAOT captures match SHA256
`8a119a178de231261d22df67c3c57b62e1c13bd79cdf4497969663d02423178a`.
The static auditor checks the complete041240 region and36 instruction pins
without vendor execution. Remaining work includes actual general-source
provenance,041EC0 auxiliary data and owning EF4 integration.
