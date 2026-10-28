# Owned per-flush residual and WORD seed preparation

`LegacyOwnedMainGlyphFlushSession` closes the actual21FFD0 flush-plan to
ordinary217DB0 and initial WORD276 seed boundary. It retains the exact
`LegacyFreshMainPlainResultOwnership`, its compact materialization, geometry,
CASE source policy, glyph pool and internally created working word. It never
accepts a supplied word label, residual event, segment boundary or publication
success flag through its public preparation API.

## Order and ownership

`Prepare(ownership, plan, allocatorResidue120)` creates the working word through
the actual result owner and captures its post13F110120-byte storage. Opaque
WORD44..53 bytes remain the explicit allocator residue; no zeroed native
metadata or fabricated pointer is assumed. `CopyInitialWordStorage()` copies
this provenance snapshot.

`PrepareNext()` resolves every packet byte-origin dependency against that
initial snapshot. It then calls the actual word's packet-field writer **before**
217DB0, matching21FE30. Association receives only the current flush rows and
the previous live residual prefix plus this flush's real residual additions.
The whole backtrack trace is never passed as one word. Unused storage tail is
not mistaken for a live residual prefix or native allocator capacity.

Source services use the same owned geometry and a privately created
`LegacyOwnedRussianMainResidualSource`. Higher references keep their merged
source ranges while resolving NN/geometry identity through the original last
base source. No new neural group or guessed box is created. Fresh knowledge99
and collectionC1 values come from the existing bounded factory/source owners,
not caller switches.

`LegacyOwnedMainAssociatedGlyphFlush` retains its exact session, original
flush, resolved immutable packet, association status, copied72-byte rows and
copied residual storage. Its status describes217DB0 only. The internal working
word remains in the same pool as the actual result owner.

The next flush cannot run while a callback is awaiting acknowledgement.
`CompleteCurrent` accepts only the typed
`LegacyOwnedMainWordPublicationResult` for the exact current association.
Association publication is single-use through an internal atomic claim.
Cancellation or an exception after entering mutation stops further preparation
and preserves existing packet/overlay prefix effects. A false callback result
also stops the session. No rollback, successful callback, accepted word or full
2271F0 result is inferred by this component.

The separate owning callback is responsible for glyph construction, symbol-span
inspection, publication and normal WORD reset. Internal friend-only controls
acknowledge explicitly **prepared** callback outcomes and simulate that reset;
they are not a public substitute for the real callback. Another friend-only
hook exercises prepared nonempty residual references with the actual service
bindings; those references never become events in a real selected trace.

## Verification

Managed and fresh Windows NativeAOT captures match byte for byte for the
single-source fixture and all nine pinned recorded coordinate tracks. Each
runtime checks640 different initial WORD allocator residues,1280 genuine
per-flush association calls and270 ownership, callback, cancellation, stale
token and mutation-prefix guards. An independent raw compact/graph oracle
recomputes segment boundaries and packet seed dependencies, then checks every
resolved packet byte, all120 WORD bytes and the explicitly prepared reset.

All current genuine selected traces have zero residual events and no source70
metadata calls. This limitation is preserved. A separate2560 prepared-reference
supplement per runtime exercises the actual geometry/guide/category/range
service bindings and2040 active higher-range merges. Its independent oracle
uses raw source facts, float32 operations, range normalization and graph/context
lookup, comparing all row and retained-vector storage. It does not assert that
these prepared references occurred on a real winning trace.

The static auditor reads pinned PE bytes without loading vendor code. It pins
21FE30,218560 and217DB0, the packet-before-association order, association failure
gate, initial WORD seed offsets and the callback-success branch.

Build `tools/LegacyMainOwnedFlush.Probe/LegacyMainOwnedFlush.Probe.csproj` and
pass:

```
AK.res LK.res engine-profile.json separation-profile.json
legacy-case-data.json legacy-text-publication-data.json output.json
[line-2e3-30.res line-2e3-50.res private-recorded-manifest.json word-index]
```

The optional word index is0..8. The private dataset has no pen-up information;
one unsegmented track is an explicit diagnostic assumption. Expected
transcriptions are never read and these checks do not establish OCR accuracy.

```
python tools/LegacyMainOwnedFlush.Probe/audit_output.py AK.res output.json
python tools/handwriting/audit_main_owned_flush.py report.json
```

Genuine nonzero residual/source70 events, final alternatives, quote/result
normalization and the complete native text engine remain separate work.
