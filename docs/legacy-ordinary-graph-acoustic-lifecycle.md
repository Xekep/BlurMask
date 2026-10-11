# Ordinary graph selector and acoustic cache lifecycle

`LegacyOrdinaryGraphAcousticPermissionSession` connects the previously restored ordinary candidate graph to the permission producer and working-map consumer. Selection, ordinary kind, successful/computed/reused row status and pool record identity now come from this executable state machine. Callers no longer manufacture per-row `GraphKind`, `Recomputed`, `Succeeded`, special-record presence/flags or physical primitive scope fields for the actual model probe.

This is a bounded **fresh frontend, immutable ordinary same-source graph** path. It does not claim the complete native recognizer orchestration, application settings, native word winner or acceptance confidence.

## Selector and owner binding

The statically inspected Text SHA-256 is `18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.

Frontend222 table `1802de610` slotD0=`1801520e0` calls its candidate graph at+70 and ordinary backend at+78. It first calls graph `virtual180` (graph preparation), then backend `virtual48`=`1801c01b0`, passing frontend pointer and callback `1801520b0`. The new session takes the immutable output of the already restored graph producer; it does not infer `virtual180` success or implement source refresh/mutation.

The decompiler omitted the fifth callback argument. Static disassembly proves `1520b0` saves incoming scope in R9D, loads a ushort ordinal from the original stack fifth-argument slot, forwards anchor/ordinal to graph `virtual70`, then compares the returned value to the scope. Type21b table `1802e1ff0` identifies:

| Slot | Native function | Verified field |
| --- | --- | --- |
|38|`1801a75f0`|Anchor's primary arc count, source248 row+8 ushort|
|68|`1801a77d0`|Resolved arc group ID at arc+0|
|70|`1801a7850`|Resolved arc span byte at arc+4|
|88|`1801a78d0`|Group kind at record+7|
|B0|`1801a7b30`|Special record+8 only for kind1/2|

Native group records use stride60 when graph+254 isfalse and stride38 otherwise. Special low3 permission flags therefore live at record+12; optional `virtual88` output instead reads record+52 (wide) or+5 (compact). These are different fields.

The checked candidate graph already resolves ordinary seed/merge ownership and does not expose foreign-source/token aliases. Its `CandidateArc.Span` is exactly native arc+4. Deferred placement at arc+5 and flags at+6 are separate metadata. The session filters by `Span == requestedScope` in native encounter order; it never compares candidate flags or guesses scope from range membership.

For ordinary seeds, `18019f850` initializes local158 bytes6/7 tozero and copies them into the generated group record. The restored ordinary merger `1801a3b30` also writes zero ushort at record+6. Thus kind at+7 is0 throughout this typed ordinary-only graph. Kind1/2 token branches are excluded by the producer domain, rather than silently assigned default flags.

## Native cache and pool

`1c01b0` has per-group38-byte records in runtime2f0 and pooled10-byte records in runtime310. Each pooled record has group ID, scope byte at+4 and cost-buffer pointer at+8. Per-group record+30 indicates computed data; its+0 points to a pooled buffer. The bounded port models buffer identity, not just a dictionary of immutable costs.

Fresh frontend constructor `1801503f0` zeros its configuration flags at+30. `180150d00` maps flag bit9 into ordinary initializer argument byte+12; `1801b0760` stores that byte in runtime1a2. Consequently a fresh unmodified frontend has1a2=0, enabling the following invalidation branch. The later configured nonzero1a2 branch is not inferred from the model resource and is outside this API.

Before each pass, pooled records whose stored scope equals the requested scope are marked vacant if their group ID is absent from **all** arcs at the anchor. This happens before callback filtering: unselected spans still contribute IDs to the presence set. Other scopes are retained.

For each selected arc:

1. A computed group record may reuse directly when native param5 permits reuse.
2. Otherwise an existing pooled group ID also permits reuse. Param5=false alone does **not** force a neural rerun.
3. Recomputation chooses the first vacant pooled record or appends one, stores group ID/scope and assigns its cost-buffer pointer to the group record.
4. The actual producer computes the row; successful costs and native scale enter that buffer. The computed flag is then retained for later calls.
5. Permission contributions use the cost row at that point in callback order. Successful rows are copied into immutable public snapshots before any later pool overwrite.

Native per-group pointers retain their pooled buffer identity when a pool entry is invalidated/reassigned. The independent controls deliberately cover that alias behavior, including reuse after another group overwrites the same buffer. The session preserves it; it does not silently substitute a safer immutable-cache policy and label that native-equivalent.

A failed producer returns no accepted permissions or source-map refresh. Failed, canceled or throwing preparation invalidates the independent session: callers must create a fresh one. This is an explicit fail-closed boundary around partially prepared native allocation/failure states, not a claim that arbitrary native failed-state retries have been reproduced. Calls must be serialized; native ushort anchor counts, alphabet/cache widths and a16-million pooled-cost bound are checked.

## Resource-backed producer and remaining policy

`LegacyPreparedOrdinaryGraphAcousticProducer` verifies the pinned Russian acoustic resource, creates the actual NN/context group evaluator and strict v110 final-cache profile, and uses the exact disabled-power scale1. Its pair list comes from the recovered unrestricted wide-symbol producer, not manually supplied aliases; the observed RU list is empty.

`LegacyOrdinarySourceEndpointMetadata.BindContext` derives the physical-boundary predicate, endpoint fields and per-physical enclosure probability from the owned raw/deduplicated/inserted source. A caller cannot manufacture those three fields for this producer. Per-row success follows actual production, recomputation follows the cache session, and ordinary kind follows the checked graph producer.

The prepared linguistic mask/controller, guide frame and **remaining external cache policy** (fallback suppression, knowledge/related-form switches, explicit scope penalties) are still upstream inputs. The producer copies the supplied template and replaces only the three proven physical fields. It does not claim those external switches are model defaults. Optional external recognizers, confidence-pruning callbacks, special token groups, cross-source alias records and mutable graph/controller refresh remain unported boundaries.

## Executable evidence

The original Python oracle generates256 independent cache traces comprising6144 passes. It constructs the ordinary no-deferred graph reference directly from logical spans, uses independent object-buffer records and set-union permissions, and covers empty selection, rejected-span presence, param5 reuse, pooled reuse when param5=false, scope invalidation, recycled pointer aliases, repeated group IDs, strict threshold costs and immutable emitted snapshots. Additional probes cover failure/no-forward, cancellation-invalidated sessions, malformed computed width and pooled snapshot ownership.

The opt-in pinned RU checks build real ingress/cuts/native seed and merged candidate graphs, then invoke the session over each anchor/span. No caller supplies row kind, recomputation, success or special flags. Every immediate repeat must preserve permissions and make **zero** new neural calls.

| Diagnostic drawing | Graph nodes/arcs | Successful computations | Immediate row reuses | Context groups |
| --- | --- | --- | --- | --- |
|Own printed geometry|7/28|28|28|2|
|Same geometry with a late horizontal stroke|8/52|47|52|3|

Real final195 costs and their derived permission maps feed the linked graph beam and actual packed dictionary/native lexical totals, producing128 terminals for each drawing. The late stroke reaches the native deferred-placement graph. Prepared per-position maps/cost snapshots preserve the pass state consumed by this diagnostic. This original graph beam and raw-cache cost adapter do not restore all native category/path corrections or begin the native dictionary query lifecycle at its fresh node1.

Managed and Windows NativeAOT pass all traces and both actual model diagnostics with matching counts and word totals. Observed AOT times were16.14ms/27.95ms on this machine. They are transport/cache checks, not representative handwriting accuracy, a portable benchmark or calibrated acceptance confidence.

```powershell
python tools/LegacyGraphAcousticLifecycle.Probe/reference_controls.py D:/graph-controls.json
dotnet run --project tools/LegacyGraphAcousticLifecycle.Probe -c Release -- D:/graph-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
```

Omit the three model arguments for independent controls only. No vendor code, native SDK, GUI, extracted code or new binary model assets are executed/shipped by this batch.
