# Actual main compact winner to glyph rows

`LegacyOwnedMainGlyphBacktrackRows.Prepare` consumes the actual
`LegacyOwnedMainCompactWordMaterialization` and its same-confidence-owner
source callbacks. It restores21FFD0's ordinary numeric append events into
72-byte2498 records. The compact pool, selected numeric terminal, alternative,
2478 node starts and sampled graph all come from their actual producers.
No expected character, caller-prepared winner, fake source reference or
whole-word bounding box replaces them.

The stage first performs21FFD0's unconditional frontend1A0 request with the
actual zero-capacity output cursor. The typed ordinary source callback
returns its real zero count/flags before inspecting delimiter kind; these
values are not constructor booleans supplied to the row walker. A nonzero
initial result would require the separate synthetic subtype5 row writer.

Each outer predecessor step uses the restored alternative-aware cursor.
Filtered lookahead uses a separate cursor copy and skips rows whose low
subtype is3+ or whose symbol isFFFF or alphabet headerE. It never changes
the outer selected alternative. The direct predecessor and filtered
predecessor are recorded separately. State tracks the last regular symbol
and the earlier distinct source node/arc, including skipped outer records.
Fresh9A=0 suppresses ZWJ; FFFF/FFFD take their native no-row paths. HeaderE
produces residual source references for the later217DB0 stage.

The72-byte writes are:

| Offset | Native source |
| --- | --- |
| 00 / 04 / 08 | Current source node / arc / compact index |
| 0C / 10 / 14 | Direct predecessor node / arc / compact index |
| 18 / 1C / 20 | Filtered predecessor node / arc / compact index |
| 24 / 28 | Carried earlier distinct source node / arc |
| 30 | Exact216610 source-reference key |
| 38 / 3A | Last regular symbol / normalized current symbol |
| 3C / 3D / 3E | Compact20.low3 / compact20.bit6 / compact21 score byte |
| 40 / 44 / 45 | Zero dword / zero byte / compact07.bit1 |

Unwritten bytes remain explicit72-byte stack residue. They are not inferred
to be zero allocation. Literal scalar normalization preserves the native
special-marker rules; it is not Unicode normalization or a Russian alias
heuristic. The current stage supports ordinary graph subtypes0..3, fresh
nontracing mode0 and no extended markers. Higher/external arcs and further
synthetic writer paths are refused.

Subtype0..2 keys use the source node/arc directly and clear the first-key
flag. Subtype3 uses the actual graph boundary dispatch. Frontend168 reads
FIRST (`StartContextId`); frontend170 reads LAST (`EndContextId`). In the
general fallback, low24 is LAST and the signed arc is FIRST minus LAST,
with bit56 set. It is not a positive width or screen coordinate.

The real four-node raw fixture produces694 compact records. Its selected
numeric path is681 ->637 ->636: FFFF is skipped, followed by one ordinary
row and an FFFA protocol row. Their keys are03000003 and010000FE7D000183.
The second uses LAST387, signed span minus387 and bit56. Its last-regular
field38 correctly retains the preceding ordinary symbol. These labels are
reported as outputs of the actual decoder, not asserted expected text.

The probe repeats that actual path for128 independent stack residues and
checks256 complete72-byte rows, private snapshot ownership and six
null/resource/cancellation/foreign-owner guards. The Python oracle reads
the entire actual compact pool and graph and independently performs the
predecessor walk, filtered lookahead, key construction and every byte write.
The static audit pins native functions, exact writes and source vtables.
No vendor DLL is loaded as executable code.

This is **not complete21FFD0**. It returns numeric append events rather than
word segments. Word-boundary callback packets and cost summaries, residual
217DB0 association, full protocol scoring and218560's duplicate/link/head
lifecycle remain separate. In particular an actual protocol row is retained
as protocol data; it is not passed off as an ordinary recognized character.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet run --project tools/LegacyMainGlyphBacktrackRows.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/backtrack-rows-build -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json E:/BlurMask-build-artifacts-20261014/backtrack-rows-managed.json
python tools/LegacyMainGlyphBacktrackRows.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res E:/BlurMask-build-artifacts-20261014/backtrack-rows-managed.json
python tools/handwriting/audit_main_glyph_backtrack_rows.py E:/BlurMask-build-artifacts-20261014/backtrack-rows-static.json
```
