# Library selected-input registry replay

`LegacyLibrarySelectedInputRegistryReplay.Prepare(document)` consumes the exact
successful transport row already retained by an adopted publication document.
The document retains that replay identity. It does not accept a caller command
list, counter, history switch or success flag. `Run` creates an **active library
registry overlay**; `InitialContext` and its original INPUT/C22/counters remain
historical. DOM application is a separate explicit operation. This is neither
mutation of native C20 nor SDK/page acceptance.

The native policy comes from the registered worker, not just a constructor:
Document00D540 calls InkC20.V170 twice with last argument1; Ink06E120 derives
history0 and passes it through063ED0 to0638A0. The full supported selected-copy
06D2C0 path does not install an observer. Consequently06ED30 takes C4D.V40
078090, not the original-history07E210 wrapper. The original fresh C20 is
history-enabled, and its C4D.F8 attachment proof remains separate. See
[the registered callback proof](legacy-registered-text-patch-apply.md).

The supported transaction is the actual same-seed pending CHAR/WORD/TEXT
operation4 prefix and optional trailing operation7 produced before the BE2
transport. It preserves exact requested registry IDs, category100, native order
and every raw16-byte range. INPUT registry ID0 is distinct from original raw
stroke IDs. The cloned selections retain exact source-command and target
selected-context identities. No source-ID remapping or global manager is used.

078090 first clones each temporary C22 onto the target, then calls C20.VE0
(06BE30), verifies returned ID equals the requested ID, releases the temporary,
and stops at failure. C20 validates ranges before C51 registration. Actual
source ranges here originate in the same retained raw document and checked
postprocessing/selection pipeline. Only unused nonnegative explicit IDs are
supported; duplicate/imported registrations and arbitrary commands are refused
before entry. Preflight bounds the transaction to65,536 added selections and
1,048,576 total ranges, with at most65,536 ranges per selection. This bounds
the retained clone data; it does not claim native allocation capacity/layout.

09EE20 updates Max20 and Next18 only after09BB20 succeeds. For an unused
explicit ID, Max becomes max(Max,ID), and Next becomes ID+1 only if Next<=ID.
The native allocator/hash/name/category side effects are not serialized as
invented pointer/padding bytes; `CopyCounterStorage16` contains only Next/Max.
Operation7 requires old/new counters nonnegative. It skips the setter when
current Next>=new. Otherwise06A100 clamps new to Max+1 if needed, and09ED40
stores Next only above Max. The active fresh lineage always has Next>Max.

The document gate serializes replay with DOM apply/disposal. Preentry
cancellation is reusable; after entry, replay is single-use. A command-boundary
cancellation or temporary-C22 clone refusal retains all earlier registrations,
their counters and command count, and poisons that replay. There is no rollback.
The internal clone-refusal control deliberately models failure **before C51**;
it does not claim every possible native C51 allocator failure or observer prefix.
Once the document has claimed a replay, its DOM apply requires that replay to
succeed; pending and faulted prefixes cannot apply. The old no-replay entrypoint
retains its existing behavior. Native115800 similarly calls C20.V188 before its
final DOM.V218, although this library guard does not port page-history rollback.
Closing the source document/scope refuses live replay without consuming it.
Immutable state snapshots survive close while
retaining their provenance owners; they are not owner-free DTOs.

The independent probe reconstructs successful and failed prefixes from captured
actual pending commands, not expected recognition labels. It covers ordinary,
multi-physical, reordered and rotated raw inputs, exact source/target identity,
historical snapshots, defensive copies, scope/document close and one-use guards.
Separate scalar controls exercise explicit ID allocation and conditional counter
handoff. Static auditing pins the worker-selected policy dependency and the
native command/counter instruction branches without executing vendor binaries.

Verification: Managed and NativeAOT match all167,862 capture bytes, SHA256
`db74443f1631d14fb68515e74880cc2c62016880665d064a07d9f22f84a10f79`:
10 actual command streams,24 retained selections,40 raw range records,
4 entered refusal/cancellation prefixes and91 guards;4,096 explicit-ID and
2,048 counter controls. The unchanged publication-document probe still passes
on both runtimes with stable SHA256
`0f4a0c56d13e1a16b55c90fff5287353026d79848e76aea60d8615027fa7f13c`.

Run `python tools/LegacySelectedInputRegistryReplay.Probe/run.py PROGRAM CAPTURE`
with the probe DLL or native executable. That runner loads committed resource
files and invokes the independent command oracle. Native evidence is checked
separately by `tools/handwriting/audit_selected_input_registry_replay.py OUTPUT`:
8 regions/5 virtual bindings/53 instruction pins, with registered-worker,
fresh selection-command and counter-append producer dependencies.
