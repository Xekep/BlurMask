# Heap and context projection after secondary recombination

`LegacyNativeSecondaryRecombinationProjection.Project` ports the nontracing
consumer in `MyScriptText 18020e6a0` after an accepted `1801f87c0` merge. It takes
an actual 128-byte heap slot and configured primary/merged-secondary storage,
then resolves actual owning stored parent rows. It does not select an eligible
higher arc or produce the adjusted incoming scores passed into the merger.

The accepted-merge body copies secondary alternative zero cost `00` and increment
`04` into heap `10/14`, its byte count `1e` into heap `26`, and its previous arc
short `18` into heap `1e`. With enabled primary storage, primary alternative zero
index `1b` takes the chosen secondary's low seven bits and retains its own high
bit. The native secondary-zero index assignment is a self-copy and changes no
bits. All other heap/primary bytes and inactive capacity remain unchanged.

The subsequent final loop visits each secondary alternative in configured index
order. Costs at or above `04000000` retain every byte and do not query a parent.
Each available alternative uses its own short `18` and low-seven-bit index `1b`
to select the parent **secondary** alternative, at the predecessor node derived
from the original current slot's signed span `20`. Mapped and raw parent contexts
are combined with the **current slot `08` suffix**, using literal `1f7e30` and
`1f7ff0` and the explicit runtime `2360` mapper. It does not retain incoming raw
context left untouched by `1f87c0`, or append the incoming candidate's suffix.
The loop replaces alternative `1f` low five bits from current heap `21`, preserves
high three bits, and restores alternative span `1a` from current heap `20`.

The checked API owns copies of the full heap and both supplied pools. Source
rows are owning snapshots, and parent callback cancellation is checked before
reading returned fields. Context-factory state is shared and live; callback
cancellation stops further composition, while already created context nodes
from earlier successful iterations are not rolled back. Caller heap/pool bytes
are preserved. Nonnegative actual parent nodes, complete 88-byte parent records,
exact configured secondary blocks, valid indices and counts up to 20 are
required. Native tracing and its `280`-byte side tables are explicitly refused.

`tools/LegacyWordSecondaryProjection.Probe/reference_controls.py` independently
projects byte fields and evaluates functional parent/suffix symbol paths. Its
512 controls compare the full 128-byte heap and complete primary/secondary
capacity. They cover enabled/disabled primary storage, count-20 secondary lists,
signed spans including `ff`, unavailable costs, preserved state/index high bits,
ordinary and control-symbol mapping, and 2076 exact parent lookups with 2953
mapper calls. Owning-parent and mapper cancellation are tested separately.

Actual Russian neural/final-cache scored roots and children provide real context
identities, costs and parent indices. All 36 pairs of six child rows pass through
secondary recombination, this projection (67 owning parent context lookups), the
native ordinal writer and row materializer. Expected paths use each chosen
parent plus the current slot suffix. The resulting stored heap and both pool
blocks match exactly. These incoming pairs are diagnostic: source scheduling,
higher-arc compatibility callbacks, incoming score adjustment, final winner and
acceptance confidence still need separate native controller integration.

Generate controls with `reference_controls.py output.json`, then run the probe
with controls JSON, Russian acoustic `.res`, lexical `.res`, engine profile JSON
and separation JSON. Managed and Windows Native AOT checks require no GUI or
vendor DLL execution.
