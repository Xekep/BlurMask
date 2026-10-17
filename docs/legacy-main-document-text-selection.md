# Fresh document source key and pending TEXT registration

`LegacyFreshMainDocumentTextSelection.Prepare(actualSourceLines).Register()`
closes two related fresh0283B0 steps: the NULL-reference source projection
which builds source2DA.C0 and its later kind2 TEXT selection command. It
consumes the actual shared WORD/source-line result, the same raw document,
the same private string/selection lifetime and the preserved CHAR/WORD
registration prefix. It does not take prepared source IDs, selected labels,
success flags, caller-created context counters or a concatenated string.

NULL source references are not a nonnull vector of zero references. Native
`05AFA0` tests the pointer first. Its NULL branch walks every actual selected
input row, with stride40, and calls Text2C1.V18=`053F90` without coordinate
overrides. The raw document's private creator already retains its original
selected objects, point counts, boundary fractions and closure flags. The
producer reads these exact rows through `CopySelectedInputs`; model sampling
or physical reorder never substitutes current stroke ordinals for object IDs.

This branch also differs from the normal per-glyph output append. `053F90`
calls InkC21.VA8=`0802E0`, which suppresses mode4 and otherwise tail-calls
`07FAF0` normalized append. The producer therefore projects each real row
with the existing ordinary range kernel and folds it through the existing
native C21 union/append implementation in original row order. That operation
can join consecutive complete objects. The tested13 actual selected rows
yield three final root ranges across three documents; a single whole-document
interval is not invented as an input and a per-object append vector is not
substituted for the normalized result.

The raw-key projection is pure for this immutable ordinary/no-C25/no-cached
owner. Native computes it before WORD processing; this bounded join derives
the same key from the retained inputs after WORD/source-line publication.
It does not claim replay of native allocation failures at that earlier call
site or expose a partially accepted document. The full orchestration must
retain the original native call order when such side effects are modeled.

At029605..02962A, the caller passes source2DA.C0 as the key, kind2, and
source2DA.B0 as the output selection ID to `0210C0`. The new typed
`CreateTextSelection` path reuses the same fresh C51/C4D context as the
previous CHAR and WORD commands. Its counter starts from the real original
object IDs; TEXT consumes the next ID after that exact prefix. It returns
the actual typed pending command, including name TEXT, operation4, enabled1,
priority100 and copied root ranges. It does not apply that command to an
external document or claim that a source2DA native pointer has been created.

Registration is single-use per transaction. Another wrapper object cannot
register a second TEXT command in the same context. Unexpected intervening
registration commands refuse before consuming the TEXT counter. Cancellation
before entry preserves retry; an entered failure poisons that registration
owner. Earlier commands are retained and no rollback is claimed. Existing
public CHAR APIs and explicitly prepared numeric controls remain unchanged.

The actual probe runs three configured raw inputs with two real alternatives
each through source completion, all-WORD shared CHAR/UTF8 publication and
021AB0. An independent oracle derives the whole-input key from the original
selected-row fields and independently tested C21 rules, checks every pending
command's ID/links/metadata and verifies TEXT follows the exact CHAR/WORD
prefix. It reads no expected labels or selected text. Twenty-one guards cover
cancellation, pre-registration access, repeated owner/transaction use and
disposed upstream owners. An independently owned guard transaction inserts an
extra genuine CHAR command; TEXT then refuses without consuming its counter.

Native next calls025E40, followed by other document postprocessors, selection
application and final publication. Those stages are not performed here.
Pending TEXT is not an accepted string, OCR confidence, font metric or a
complete0283B0 result.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet build tools/LegacyMainDocumentTextSelection.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/document-text-key-build
python tools/LegacyMainDocumentTextSelection.Probe/run.py E:/BlurMask-build-artifacts-20261014/document-text-key-build/bin/LegacyMainDocumentTextSelection.Probe/release/LegacyMainDocumentTextSelection.Probe.dll E:/BlurMask-build-artifacts-20261014/document-text-key-managed.json
dotnet publish tools/LegacyMainDocumentTextSelection.Probe -c Release -r win-x64 -p:PublishAot=true --artifacts-path E:/BlurMask-build-artifacts-20261014/document-text-key-native-build -o E:/BlurMask-build-artifacts-20261014/document-text-key-aot
python tools/LegacyMainDocumentTextSelection.Probe/run.py E:/BlurMask-build-artifacts-20261014/document-text-key-aot/LegacyMainDocumentTextSelection.Probe.exe E:/BlurMask-build-artifacts-20261014/document-text-key-native.json
python tools/handwriting/audit_main_document_text_selection.py E:/BlurMask-build-artifacts-20261014/document-text-key-static.json
```

The optional static auditor reads the pinned Text/Ink images. No vendor
library is loaded or executed by this implementation or its probes.
