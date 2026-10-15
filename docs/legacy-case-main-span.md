# Case-aware main span ownership

The additive `LegacyOwnedMainWordSpanPreparation` constructors accept an actual
`LegacyFreshCaseRussianDictionaryPreparation` and its exact sampled source.
Existing DIRECT constructors/getters remain unchanged. A case span exposes
`CaseDictionaryOwner`, `CommonModel`, `CommonDescriptor`, `CommonSourceSummary`,
`Contexts`, `CopySourceHeaders()` and `IsCaseAware`. Its legacy DIRECT getter
throws instead of fabricating a second unwrapped dictionary.

The common internal binding uses the same frontend/cache permission and union
producer for both paths. In the case path, raw computed acoustic permissions go
through `Session.RefreshPermissionsAndGetMap` once:234840 then093F80 installs
the map in the same292 backend used by normal/auxiliary queries. The exposed
permission bytes remain the pre-wrapper acoustic diagnostics; `WorkingMap` is
the actual case-expanded installed map, not its old DIRECT/pre-wrapper result.
The145-entry mask and195-byte finite-cost qualification union still come from
the same owned acoustic scores.

Each case map is an immutable owned object. A span retains that exact identity;
updates through the public session or another span owner invalidate it even if
its local epoch did not change. Query/terminal/restart checks repeat atomically
under the existing session gate, closing the check/read race. A stale owner does
not invalidate a different owner's current session just by failing its lease
check. Invalid/canceled permission refresh computes no replacement map and
cannot silently advance the installed identity.

Source68 refresh is not the native query-cache invalidation epoch. On the real
dictionary, eight requests exhausted under an empty map leave even a NEW request
returning0 after allowing permissions. Object identity alone cannot protect
recycled backend slots. The owning bridge refuses reads after crossing a queried
map without an explicit controller epoch. Weak per-request leases additionally
protect request identity; discarded requests are not retained. Public prepared
Session query behavior is unchanged. Guard failure leaves the shared session
usable by another valid owner; the failed span owner keeps its existing
fail-closed behavior.

`AdvanceNodeEpoch(completedNode)` forwards the actual sourceV80 through the same
owned backend. In292,232450 calls inner2B7.V80/092970→092840 first, then231ED0
clears all eight wrapper slots AND nextSlot8378. The latter write is pinned at
232037; the earlier interpretation that the round-robin cursor survives was
incorrect.28D.V80=236DC0 forwards selected children without resetting aggregate
startup bases. The DIRECT common branch resets its same original2B7 backend.
Session's prepared equivalent is `PrepareControllerNode`. Bounds/cancellation
are checked before mutation; the reset also invalidates earlier map/span leases.
Permission refresh never calls this operation.

Native2144F0 invokes V80 with start−1 before the node loop and after each completed
node. The new probe checks that sequence with one persistent request. Its10
independent NN/map snapshots explicitly request a prepared epoch per snapshot;
this is **not** an implementation of2144F0's within-node span scheduler. A guarded
case request crossing maps within one node is still refused until the actual
controller conditions are composed. Thus persistent290 Level.Query integration
and full main controller scheduling are not claimed.

`ReadWordTerminal` forwards full source tags/ending symbol/boundary to234970 and
preserves236D20's sourceU32+0, wordID+4, category+8 and boundary+12 channels.
`ResolveWordRestart` retains233F70 case-bit copying and236B50 selected-source
stamping. No ordinary metadata provider cast is used for case sources.

The shared initial-special and scalar policies consume common model/header
facts. The previously proved DIRECT ring binding explicitly rejects
`IsCaseAware` pending separate ring/whole-loop proof. No case dictionary is
silently admitted to that earlier scope by the generic constructor.

The new probe starts from raw own ink and native first publication through
actual source720/main geometry and NN scoring. It checks10 real span maps,
10 computed acoustic rows,397 full case dictionary records and all195 finite
union entries per span. Every installed map differs from its pre-wrapper map.
Independent Python reconstructs raw permissions from195 NN costs, ordered
stored-case propagation, lexical filtering and complete case expansion/context
records. Stale/foreign/cross-owner/public-refresh/failed-refresh guards and
metadata/restart/layout and cached-empty/reuse checks are separate. The actual
native reset restores315 records for both an exhausted persistent request and a
new request, including after all8 backend slots were exhausted. These tests are not a final290/240
word winner or handwriting accuracy claim.

Probe arguments: `AK LK engine-profile separation-table case-data publication-data capture`.
Independent audit: `audit_output.py AK LK case-preparation-controls capture`.
Static audit: `tools/handwriting/audit_case_main_span.py [report]` pins17 PE
instructions/table entries without loading a vendor DLL.
The older DIRECT span, session and ring probe expectations remain unchanged.
