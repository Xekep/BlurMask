# First result scalar publication

`LegacyFirstResultScorePublication` ports the final scalar writes of Text
`1f42d0` after character/result materialization, addresses1f6a13..1f6c00.
It requires actual full40-byte alternative records and the pre-call count.
Only the newly appended rows change; all prior rows and opaque bytes remain
owned and preserved. If no new result survived, the entire buffer is unchanged.

Mass08 is not softmax. Every new alternative after the first receives
`mass[i]/(mass[i]+mass[first])`. The first receives
`mass[first]/(mass[first]+mass[first+1])`, or1 if it is the only appended row.
The assembly's reverse unrolled loop retains the immediately following pair's
denominator for that final first-row write. Three or more values therefore
need not sum to1. This is a native relative comparison, not calibrated OCR
acceptance confidence.

Acoustic14 remains unchanged. For more than one total result, native MINSS
finds the minimum acoustic14 among new rows, then CRT expf25c840 writes
`expf(-(acoustic14-minimum))` into0C. For exactly one total result0C is1.
No clipping, epsilon or invented finite fallback is applied. IEEE NaNs,
infinities, underflow and source-on-equality MINSS behavior are preserved.
The portable expf may differ by up to two float ULPs from another CRT.

768 independent Python batches cover6123 complete rows,320 unordered outputs,
empty/no-append batches, prior rows, single/multiple results, signed zeros,
subnormal masses, expf overflow/underflow and unchanged opaque storage.

```powershell
python tools/LegacyFirstResultScore.Probe/reference_controls.py D:/score-controls.json
dotnet run --project tools/LegacyFirstResultScore.Probe -c Release -- D:/score-controls.json
```

The primary source is statically inspected Text SHA256
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
No vendor DLL is executed. Result allocation, glyph/source backtracking,
canonical Unicode output and result80 glyph-level minimum remain separate.
This numerical stage does not complete Russian recognition or app integration.
