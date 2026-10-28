# Actual single-line document consistency

`LegacyFreshMainDocumentLineConsistency.Apply(actualText)` ports the bounded
ordinary single-assigned-line path of `025E40`. The input is the successful
typed pending TEXT registration after the actual shared WORD/CHAR source
choices and `021AB0` source-line publication. It requires the exact current
CHAR/WORD/TEXT command prefix, validates every actual WORD view and preserves
its document, source and TOP owners. No source IDs, chosen labels, line
heights, success flags or caller-created WORD/CHAR records are supplied.

The native caller registers TEXT at02962A before calling025E40 at029647.
The new stage enforces that order. It does not apply pending selections to
an external document or claim a complete0283B0 accepted result.

Native025E40 visits each alternative's WORD links, skips virtual WORD
sources, chooses payload40 when nonnegative or otherwise the279 selected
WORD index, and reads the last CHAR's source line at payload60. The actual
fresh WORD producer initializes payload40 to minus-one at028EB7 and exposes
no setter. The adapter therefore consumes the real selected279 ordinal.
It refuses virtual, empty, foreign-owner or unassigned/multiple-line inputs;
these are explicit unsupported branches, not discarded output rows.

For every actual WORD choice, every CHAR payload must reference the same
assigned source line0 and raw document. The producer checks the complete
choice array, not only the selected WORD. This proves all last-CHAR line
comparisons equal, making024A90 WORD splitting unreachable. When WORD
flag1000 is clear,025940 still executes its scans. It walks adjacent CHAR
links backwards in each WORD choice; only unequal non-minus-one line IDs
with both CHAR4000 flags clear reach range allocation, flag writes and
inserted-character construction. Equality jumps directly to the next pair
at0259D1. The owning stage reproduces those scans and counts; it does not
implement or silently bypass the unequal-line insertion branch.

After the visits,025E40 truncates source2DA.F8/count100 after the greatest
signed selected last-CHAR line plus one. The preceding actual021AB0 calls
append two56-byte rows while resetting the wrapper write index for each
alternative: row0 contains the guide, row1 is zero. The actual line0 visits
therefore produce a one-row result. The new result owns its updated56-byte
storage and immutable line list. The earlier source-line publication,
WORD48 records, CHAR24 links, source flags, selected ordinals and pending
command objects remain unchanged. It does not invent a bounding-box height
or alter any guide value.

The headless probe runs four genuine raw configured inputs: one stroke,
two ordered three-stroke groups, the same groups in reversed input order,
and a rotated stroke. All use the real NN, dictionary, compact result,
publication, source completion and shared CHAR/WORD producers. Eight
alternative visits perform24 actual adjacent comparisons and four zero-tail
trims. An independent oracle replays the selected-index traversal, native
025940 predicate, entire48/24-byte preservation and full56-byte truncation.
Sixteen guards cover cancellation, missing TEXT registration, intervening
commands and disposed source owners. No expected transcription is read.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet build tools/LegacyMainDocumentLineConsistency.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/document-line-consistency-build
python tools/LegacyMainDocumentLineConsistency.Probe/run.py E:/BlurMask-build-artifacts-20261014/document-line-consistency-build/bin/LegacyMainDocumentLineConsistency.Probe/release/LegacyMainDocumentLineConsistency.Probe.dll E:/BlurMask-build-artifacts-20261014/document-line-consistency-managed.json
dotnet publish tools/LegacyMainDocumentLineConsistency.Probe -c Release -r win-x64 -p:PublishAot=true --artifacts-path E:/BlurMask-build-artifacts-20261014/document-line-consistency-native-build -o E:/BlurMask-build-artifacts-20261014/document-line-consistency-aot
python tools/LegacyMainDocumentLineConsistency.Probe/run.py E:/BlurMask-build-artifacts-20261014/document-line-consistency-aot/LegacyMainDocumentLineConsistency.Probe.exe E:/BlurMask-build-artifacts-20261014/document-line-consistency-native.json
python tools/handwriting/audit_main_document_line_consistency.py E:/BlurMask-build-artifacts-20261014/document-line-consistency-static.json
```

The optional auditor reads only the pinned PE images. No vendor binary is
loaded or executed. Generic multi-line splitting, inserted line markers,
later document postprocessors and final acceptance remain separate work.
