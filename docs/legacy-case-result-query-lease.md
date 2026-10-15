# Case dictionary queries during result reconstruction

The native207780 result reader runs after the final controller V80 reset.
That reset clears query slots but retains the last D0 permission map. The
ordinary span lease is already stale, so reusing it or constructing a new
dictionary would select the wrong lifecycle.

`LegacyPreparedCaseDictionarySession` now has an internal result-query lease.
Capture requires the completed controller, a retained actual owning-span map,
and untouched query slots after the reset. Reads use the same292 backend and
normal/auxiliary query services. They retain map, controller epoch and owner
identity, while updating only their own query revision. External reads, map
refreshes, another lease or controller movement invalidate the lease. Failed
backend reads invalidate the session; rejected foreign leases do not.

The headless `LegacyCaseResultQuery.Probe` compares38 paginated32-byte records
against queries on the same backend and map. Ten controls cover premature
capture, interleaving, cancellation, map changes, foreign owners and stale
leases. Managed and Native AOT captures are byte identical. The existing
case-session independent oracle still checks5,742 variants over28 states.

Arguments: `AK LK engine separation case-data publication-data capture`.
This verifies result-query ownership, not full compact materialization or
recognition accuracy.
