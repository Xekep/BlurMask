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

Intermediate publication uses a different internal live lease. Capture requires
the current node's actual D0 map before V80, retaining the existing query slots
and revision. Its own queries use that same backend and advance only its own
revision. A normal query, another D0/map, V80, replacement capture or explicit
completion invalidates the old lease. DIRECT bindings track the same mutations;
the common span token also checks exact active-token identity. Cancellation
during validation leaves a valid lease usable.

`LegacyMainLiveQuery.Probe` checks both actual DIRECT and CASE owners, paginated
same-backend byte parity and14 lifecycle controls. It does not by itself publish
intermediate compact records or prove long-source recognition.

## Paused development checkpoint, 2026-10-15

Development was paused at the user's request. The accompanying WIP checkpoint
preserves compileable work, not a completed engine or a release candidate.

- Live-query ownership: Managed/Native byte parity,14 lifecycle controls;
  existing final-query38-row/10-guard capture remains unchanged.
- Intermediate compact publication: Managed CASE4 regression and independent
  byte oracle pass. Actual124-node input executes the native node119 gate but
  produces zero compact records and no terminal winner. A separate explicitly
  prepared CASE4 node2 invocation produces716 records and continues node3.
  Native AOT, remaining lifecycle controls and CASE V58/233FC0 cache proof are
  still pending for this publication batch.
- Actual glyph flush plans: single fixture and all nine recorded tracks pass
  Managed/Native independent oracles; all ten captures are byte identical.
  Packet metadata still requires the real initial WORD seed. Actual source70
  calls on these fixtures are zero. Owning residual/chain/publication integration
  is not complete.
- Main plain-result pool factory and WORD packet/reset bridge: Managed byte
  checks pass; Native validation of these latest additions remains pending.
- First-source interior delimiter helper: compilation only. Independent numeric
  checks, static audit, multiword text materialization, multiword720 and empty24A
  are unfinished.

The checkpoint library build passed with zero errors and zero warnings.
Russian cursive accuracy, complete automatic recognition, application integration
and a verified next release remain unfinished.
