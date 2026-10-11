# Native normalized pen-up reduction

`LegacyNormalizedPenUpMerge` restores the checked signed-int gap path of
`1b4bd0`, with descending quicksort`1b4ae0`. For a caller-supplied positive
maximum stroke count, it keeps the largest `maximum−1` squared pen-up gaps.
Other stroke boundaries are removed. Every normalized short point stays in
the same global order and repeated endpoints remain; no resampling occurs.
The result includes original boundary indices in increasing physical order.

The quicksort's equal-gap swaps are retained, rather than replacing the native
selection with an assumed stable sort. For two equal gap slots0/1, native
partition swaps them and a maximum of2 retains boundary1. Overlarge squared
gaps outside signed-int32 are rejected; the native wrapped arithmetic for such
out-of-domain geometry is not claimed supported. Source/result geometry is
bounded to256 strokes and4096 points, with cancellation.

Native runtime computes the maximum from enabled acoustic profile+1c divided
by3 and takes a minimum across profiles with nonzero feature mask+14. The
actual single Russian profile stores30, giving10. This component takes the
resolved maximum explicitly and does not derive runtime enabled profiles.
It is independent of sampler atom/group range composition: it changes only
the stroke boundary array after short normalization.

The current frozen `LegacyPreparedGlyphNormalization` and classifier endpoint
still enforce their explicit pre-merge bound. This standalone checked stage
does not silently expand that endpoint or imply native chronology resolution.

```powershell
python tools/LegacyPenUpMerge.Probe/reference_controls.py .artifacts/myscript-analysis/output/RussianAssetsSearch/legacy-penup-merge-controls.json
dotnet run --project tools/LegacyPenUpMerge.Probe -c Release -- --controls .artifacts/myscript-analysis/output/RussianAssetsSearch/legacy-penup-merge-controls.json
```

512 independent own unique-gap fixtures compare retained boundaries against
descending mathematical selection and verify unchanged global point order.
Own controls cover an exact equal-gap swap, no reduction below threshold,
reduction to one stroke, invalid threshold, squared-gap overflow and cancellation.
No Russian handwriting accuracy, dictionary search or GUI integration is claimed.
