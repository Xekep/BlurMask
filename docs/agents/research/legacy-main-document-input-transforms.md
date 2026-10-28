# Selected INPUT result seed and transform-vector handoff

This stage continues the library-owned fresh selected textField branch after
the actual `023260` control cleanup. It does not apply the pending selections,
invoke final2E7, import an SDK page, or produce an accepted document.

`LegacyFreshMainWordSourceChoices.PrepareSelectedInput` creates a
`LegacyFreshMainTextDocumentSeed` in its shared state before any WORD/CHAR
publication. The seed retains the exact selected C20, INPUT tag, C22 ranges,
pending C4D context and newly created3E string pool. Every later string in the
shared WORD source path belongs to that same3E. The old OriginalID entrypoint
keeps a null seed and its existing behavior.

The seed also creates and retains its actual initial108 vector owner before
publication.037E80 replaces that exact seed field, and the returned stage
retains the same new vector owner. The separately retained initial snapshot
does not change. No local detached empty vector substitutes for this lifetime.

The native NEW2DA constructor `01C010` creates3E at118 and initializes108/110
to null/zero. Input loading `02D8C0` resets the result and reads the actual
INPUT tag into30. `05F200` reads DAF.V38 INPUT span rows before2DA.V18. Here
those rows are empty because the privately created addItems INPUT has no span
children; no empty vector is inferred from raw-object count or transform count.
Only the assigned source30, initialD8, initial110 and initial114 scalar bytes
are exported by `CopyInitialScalarStorage20`; pointer lanes are not invented.

`LegacyFreshMainDocumentInputTransforms.Apply` requires the exact successful
cleanup owner and the seed created before its WORDs. The native tail calls
023260, resolves source30 through C20.V128 to retained C22, then calls037E80
with writeDom=false. The actual document-alternative countD8 is two. It is
distinct from the post025E40 source-line count100, which is one in this scope.

037E80 calls DAF.V30 with identity exclusion. The actual complete ordinary C49
current views resolve one identity matrix per selected raw object, so none is
appended to the transform80 vector. The following026EF0 reads the independently
produced DAF32 count and returns true before any matching write when it is zero.
The prior source lines, CHAR/WORD records, selections and seed snapshot remain
unchanged. One seed lifetime can be entered once; cancellation before entry
does not consume it. An entered failure cannot reset the lifetime by preparing
another object.

Failure ordering is deliberate: at037F0A export returns status,037F1C saves it,
and037F2D replaces108 regardless of status. The previous vector becomes local
cleanup ownership. Only the outer029861 status gate skips026EF0 on false.
Internal prepared controls test this swap with arbitrary preserved64-byte
scalar tails and typed range sidecars, including nonempty failed output
prefixes. They are explicitly separate from the real current-view exporter,
which supports successful identity-excluded output only. Native pointer bytes
and a nonidentity80-byte matrix codec are outside this port.
Four additional internal fault attempts use separate genuine seed lifetimes:
an explicitly prepared nonempty failed prefix replaces the seed's real vector,
the result retains that same owner,026EF0 is skipped, and a retry cannot clear
the retained prefix. Public callers cannot supply export status or records.

The probe runs four genuine configured raw inputs through inference, shared
WORD/CHAR publication, TEXT registration, source-line processing, source
indices and cleanup. It checks the seed before publication, exact string-pool
ownership, two document alternatives versus one line, all identity matrices,
empty actual transform/DAF output, unchanged earlier snapshots and old API
refusal. Independent Python controls cover1024 swaps, including512 failed
exports. Static PE checks pin NEW2DA, tag loading, ordered tail calls and the
unconditional swap; vendor binaries are never executed.

Commands from the repository root (all output paths on E):

```powershell
python tools/LegacyMainDocumentInputTransforms.Probe/reference_controls.py E:/BlurMask-build-artifacts-20261014/document-input-transform-controls.json
python tools/LegacyMainDocumentInputTransforms.Probe/run.py <managed-dll-or-native-exe> E:/BlurMask-build-artifacts-20261014/document-input-transform-controls.json <capture.json>
python tools/handwriting/audit_selected_document_input_transforms.py <static.json>
```
