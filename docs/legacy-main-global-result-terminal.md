# Global result source70 metadata lease

The completed global pass now provides an internal
`ReadResultTerminal(fullTag, rawSymbol, boundary, token)` entry for the existing
source70 metadata action. It forwards its private result-query lease through
the same global source and the same selected292 dictionary session. Both
source and session check that exact active lease while holding their existing
serialization gates.

Final `2144F0` V80 resets query slots and retains the preceding D0 permission
map. A live D0 span is stale at this point. The new entry therefore checks the
result lease, not a fabricated D0 map or a newly constructed dictionary. It
does not reset queries, refresh permissions, alter the installed map or advance
the query revision. Actual result V40 reads can still advance that same lease;
external reads, map/epoch changes and replacement lease identity revoke it.

The old public `ReadTerminal` and internal map-bound entry retain their previous
map checks. Their unchanged metadata action is now a private shared helper:
selected source byte4 must equal one, the complete tag/symbol/boundary reaches
`LegacyNativeCaseWordMetadata`, and the returned token's uint32 zero is stamped
with selected source one. No case/source bits are stripped from the input tag.

Native `236D20` selects a child from input source byte4, forwards the full tag
to child70 with the raw ending symbol and boundary, copies all16 bytes and
stamps the selected source ID. `234970` delegates to the same underlying word
metadata and adjusts its category at byte8 using case state/classification.
The native metadata action does not issue normal/auxiliary dictionary queries.
The read-only audit pins both byte regions, 24 PE instruction locations and
the managed gate/lease/action order. Vendor DLLs are never executed.

## Evidence and precise coverage

`LegacyMainGlobalTerminalMetadata.Probe` runs the existing eight genuine global
input pipelines, performs the actual main pass/terminal/compact publication,
and reads4,754 terminal tags (after the actual213C00 join) and raw labels already present in those compact
pools through the new post-final lease entry. An independent raw-LK preorder
and case-classification oracle checks every complete metadata16 token. It
derives the complete expected terminal set from every compact record and the
raw lexicon, rather than from captured metadata IDs or a fixed historical count.
Removing all captured rows for one terminal is rejected. The
probe checks repeated/detached results, same-backend equivalence with the old
source70 action and96 source/cancel/foreign/replacement-lease/query-revision
guards. It also verifies that no acoustic model is reevaluated.

These calls are explicitly **prepared metadata consumers on actual compact
terminal tags**. All eight current compact pools have zero rows with the
native `compact+36 != -1` metadata marker. Consequently they produce no native
`21FFD0` metadata events, and this batch does not claim to have observed that
guarded flush branch. It supplies the necessary owned source70 reader for a
future real event; it does not manufacture a metadata marker or winner.

The existing selected-session and result-query regressions cover the preserved
old source70 and query behavior. There is no text publication, automatic
acceptance, confidence calibration or full-engine claim in this adapter.

```powershell
python tools/LegacyCaseAlphabetRanges.Probe/reference_controls.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res E:/BlurMask-build-artifacts-20261014/main-global-result-terminal-case-ranges.json
python tools/LegacyCasePreparation.Probe/reference_controls.py E:/BlurMask-build-artifacts-20261014/main-global-result-terminal-case-ranges.json E:/BlurMask-build-artifacts-20261014/main-global-result-terminal-case-controls.json
python tools/handwriting/audit_main_global_result_terminal.py E:/BlurMask-build-artifacts-20261014/main-global-result-terminal-static.json
dotnet run --project tools/LegacyMainGlobalTerminalMetadata.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/main-global-result-terminal-managed -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json models/text-unicode/legacy-case-data.json models/text-unicode/legacy-text-publication-data.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res E:/BlurMask-build-artifacts-20261014/main-global-result-terminal-managed.json models/ru-RU/ru_RU-lk-text.lite.res
python tools/LegacyMainGlobalTerminalMetadata.Probe/audit_output.py models/ru-RU/ru_RU-lk-text.lite.res E:/BlurMask-build-artifacts-20261014/main-global-result-terminal-case-controls.json E:/BlurMask-build-artifacts-20261014/main-global-result-terminal-managed.json
```
