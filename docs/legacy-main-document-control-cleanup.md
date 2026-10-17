# Fresh023260 no-control source pass

`LegacyFreshMainDocumentControlCleanup.Prepare(actualSourceIndices).Run()`
consumes the successful current [020920 owner](legacy-main-document-source-indices.md).
It scans every actual WORD48 choice, including the unselected choices, then
the complete CHAR insertion list. The source/document/selection lifetimes
remain exact. It does not serialize, apply, accept or publish a document.

The flags are live producer outputs, not constructor defaults. The shared
WORD renderer ORs the flags of the selected actual CHAR40 records into each
WORD48 row8. The CHAR payload retains the publication/reuse aggregate with
the native metadata mask, followed by the01FD40/020000 overlay. The runtime
reads those actual WORD row flags and CHAR flags. It neither uses guessed AL
categories nor infers flags from expected strings.

For a WORD row with `100000`, native023260 calls022960. That service is not
part of this stage; the owning runtime refuses before calling an unavailable
producer. Once all rows of a supported WORD have been checked, native clears
`100000` in that WORD payload before visiting CHAR sources. The result stores
this flag overlay independently, preserving the preceding source snapshots.
All currently supported actual fixtures have no `100000` rows.

An active CHAR source requires the native reverse40-byte-record scan. For
valid emitted UTF8, its four-byte predicate identifies F3 B0 80 80/81
(U+F0000/U+F0001); the assembly uses a signed byte comparison at02336E.
Deletion precedes034D90 and its indexed-source updates when020920 ready is
set, or linked-WORD marker adjustment otherwise. This runtime refuses that
whole active branch rather than fabricating strings, deleting records without
their source updates, or claiming a successful native service call.

The CHAR pass uses insertion-list order, not AVL in-order. A typed lifetime
lease prevents constructing another cleanup owner to reset a started stage.
Pre-entry cancellation can be retried on the same owner; an entered failure
retains the modeled counters/WORD-clear prefix and cannot be retried. An
unsupported branch is refused before the unavailable call, so it is not
claimed as a simulation of that call's native failure writes. Historical
CHAR40/WORD48 bytes and selection commands remain unchanged. The final
current gate checks both completed passes and the same020920/source owners.

Managed and NativeAOT each verify four genuine raw inputs, eight complete
WORD48 rows, twelve CHAR owners, thirty-two CHAR40/string records and twenty-eight
guards. The independent oracle reconstructs every WORD row's flag OR from
its actual selected CHAR records, checks valid UTF8 and the exact control
predicate, compares all before/after40/48 bytes and checks insertion order.
No expected labels are used. These cases demonstrate the real no-control
branch; no active022960/034D90 invocation is claimed.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet build tools/LegacyMainDocumentControlCleanup.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/document-control-cleanup-build
python tools/LegacyMainDocumentControlCleanup.Probe/run.py E:/BlurMask-build-artifacts-20261014/document-control-cleanup-build/bin/LegacyMainDocumentControlCleanup.Probe/release/LegacyMainDocumentControlCleanup.Probe.dll E:/BlurMask-build-artifacts-20261014/document-control-cleanup-managed.json
dotnet publish tools/LegacyMainDocumentControlCleanup.Probe -c Release -r win-x64 -p:PublishAot=true -p:StripSymbols=true --artifacts-path E:/BlurMask-build-artifacts-20261014/document-control-cleanup-native-build -o E:/BlurMask-build-artifacts-20261014/document-control-cleanup-aot
python tools/LegacyMainDocumentControlCleanup.Probe/run.py E:/BlurMask-build-artifacts-20261014/document-control-cleanup-aot/LegacyMainDocumentControlCleanup.Probe.exe E:/BlurMask-build-artifacts-20261014/document-control-cleanup-native.json
python tools/handwriting/audit_main_document_control_cleanup.py E:/BlurMask-build-artifacts-20261014/document-control-cleanup-static.json
```

## Remaining fresh postprocessor boundaries

The exact0283B0 order is020920 at0297EA,023260 at029801,037E80 at02985C,
then026EF0 at029876. No latter call may be skipped merely because the input
contains raw ink and no caller text.

* **037E80 transforms:**0283B0 initially reads source30 from the prior/source
  owner at `param3+10`, then copies it to the new result at028763. Near02980E
  a non-minus-one value requests the actual C20.V128 owner and its range
  vector. The new2DA constructor's minus-one default alone proves nothing
  about this live copied value. Empty retained source ranges or documentD8=0
  take the cleanup branch; otherwise Document serviceDAF.V30 exports a new
  vector. The native call has its final low-byte flag false. The bounded
  [selected INPUT seed and transform handoff](legacy-main-document-input-transforms.md)
  now joins the genuine private fresh textField owner, including failed-export
  swap semantics. The old OriginalID route and imported/prior results do not
  obtain that seed.
* **026EF0 input spans:**05F200 calls DAF.V38 before invoking0283B0. The actual
  Document service is descriptor1ED7E0, table1ED790, V38=030FB0. It walks the
  document's `input`/`span` nodes, preserving their order. Each32-byte row
  stores the actual `eol` byte and originating node. A supported `field`
  becomes owned external UTF8 with byte1=1; an ordinary `tag` resolves through
  the actual C20.V128 owner and copies its source ranges with byte1=0.
  `LegacyOwnedRawInkDocument` currently has immutable original ink objects and
  point counts, not this Document node tree or its source-ID registry. It
  cannot by itself justify an empty span vector. The selected INPUT seed now
  retains DAF32 output from the actual private addItems INPUT before WORD
  publication: that input has no span children. Only this proved fresh branch
  reaches026EF0's count-zero return. Imported or setInputSpans ownership is
  still unsupported.
* **Prior/cache and transaction replacement:**0283B0 next conditionally calls
 027B60 or02BBF0/02BE60/02D440 from the actual prior result and its D8 count.
  These require their actual owners; no null pointer is inferred from raw
  stroke input. In the selected INPUT seed branch, its actual initialD8=0
  bypasses nonempty-prior processing. The later0298AA comparison is between
  saved and current pending counters, not C4D pointers. C4D.V10=07AAF0 queues
  operation7 with old/new counters; it does not itself apply the selections or
  replace the transaction. That ordered counter handoff remains separate.
* **Final2E7 handoff:**Text descriptor276C20/table276C68 selects0658E0. Null
  prior result or priorD8=0 reaches061510 before BE2.V50. The nonempty-prior
  branch additionally uses061820/062E60/0633E0/064BB0/064D40/065650. This is
  document-node/BE2 transport, not a scalar success flag or concatenation of
  inferred strings. It is not yet implemented by the immutable raw document.

These bindings are from pinned static PE reads and read-only Ghidra exports.
Document DAF.V28=030D40 and V30=030E20 are distinct from V38. The full native
document/C1E/C4A lineage remains a separate ownership task, including the
actually reached C25 getter branch. No vendor binary was loaded or executed.
