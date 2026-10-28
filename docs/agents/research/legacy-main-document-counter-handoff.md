# Pending document counter handoff

This is the next ordered fresh selected INPUT stage after the actual successful
037E80/026EF0 result. It appends the native counter synchronization command to
the same retained pending C4D transaction. It does not execute that command,
update the live C51 registry, invoke2E7, or accept a document.

Text05F050 snapshots actual C20/C51.Next into tuple10.0283B0 copies that value
into its working pending tuple and saves the original counter separately at
02876A.0210C0 advances the working counter for CHAR/WORD/TEXT registrations.
After026EF0 succeeds,0298AA compares the saved and final numeric counters.
These values are counters, not two C4D pointers. Equality skips the append.

When they differ,0298DC dispatches C4D.V10=Ink07AAF0. That function allocates
an80-byte command, calls077230 with operation7, stores the old counter at20
and new counter at18, and links it after the existing tail.077230 initializes
the command links and assigned body. Only byte0 and the assigned body18..4F
are modeled; padding and pointer bytes are not serialized.

`LegacyFreshMainDocumentCounterHandoff.Apply` derives both counters from the
same seed and WORD publication owner. It requires successful same-seed
037E80 and the actual empty DAF026EF0 branch. The initial counter is the
retained selected INPUT registry snapshot; the final counter is the actual
shared pending selection cursor. Neither is supplied by the public caller.
`LegacyOwnedFreshTextSourceContext` retains the operation7 object in its actual
shared transaction state. `CopyPendingCommands` returns the complete supported
ordered view, with the same original operation4 objects and trailing operation7.
`CopyPendingSelections` continues to return only the unchanged operation4
snapshots, preserving its existing API and records.

The bounded handoff is single-use. Cancellation before entry leaves the list
untouched. An explicit internal allocation-failure control retains the original
prefix and refuses retries. The completed handoff closes the supported new
selection append phase; it does not claim the complete generic C4D API.
Prepared numeric controls cover equality, arbitrary signed64-bit counters and
allocation failures. Negative counters can be stored by07AAF0; validation of
their execution is a separate native branch, not an append-time filter.

In the later command executor078090, operation7 checks both counters are
nonnegative and compares the new counter with actual C51.Next. Only when the
new counter is larger does it invoke C20.VF8=06A100, which also checks the
actual maximum live registry ID before updating C51. This executor is not
called by the owning handoff. Four real inference/publication paths prove
the INPUT registry snapshot and every selection record remain unchanged.

The next boundary is2E7.V0=0658E0. Its fresh branch uses061510 and actual
2DA.V30=031880 to build document nodes, then BE2 transport. A pending command
or a list of inferred strings does not replace those missing owners.

```powershell
python tools/LegacyMainDocumentCounterHandoff.Probe/reference_controls.py E:/BlurMask-build-artifacts-20261014/document-counter-controls.json
python tools/LegacyMainDocumentCounterHandoff.Probe/run.py <managed-dll-or-native-exe> E:/BlurMask-build-artifacts-20261014/document-counter-controls.json <capture.json>
python tools/handwriting/audit_document_counter_handoff.py <static.json>
```
