# Native heap-slot to stored word-row bridge

`LegacyNativeWordRowMaterializer` ports the nontracing `MyScriptText.dll`
`180213410` consumer. It converts a complete 128-byte heap slot to an owned
88-byte stored word record and, when enabled by explicit runtime counts, the
corresponding blocks of 40-byte alternative records. It does not select or sort
the slots to materialize and does not claim a native winner.

## Producer and consumer

`213410` copies bytes `00..57` into the destination row
`runtime+23e0[node][destinationArc]`, stride `0x58`. If the copied ushort at
`3c` is `ffff`, it is replaced by that destination arc. Existing anchors remain
unchanged. Field `38` remains inherited; the destination row's node index does
not overwrite it. This connects the initial `1f8d60` missing-anchor record to
the exact anchor/history lookup consumed by `1f9080` continuations.

For primary alternative count `2402 > 1`, the source block is copied only when
`destinationArc < runtime230a`. Source `runtime2428` is indexed by the signed
slot dword `58`, then alternative count, with 40-byte stride. For secondary
count `2404 > 1`, source `2430` uses the same ordinal/count/stride and copies
without the primary arc-limit condition. The API returns owned blocks rather
than copying into caller-owned destination tables. No block is read when its
native count/limit condition is inactive. Missing negative ordinals, truncated
blocks and signed32 ordinal/count overflow refuse instead of inventing data.

The ordinary controller `2144f0` assigns slot dword `58` ordinals across the
heap descriptors before its additional scoring passes. It then sorts,
deduplicates and prunes before calling `213410`. Whole-record heap moves
preserve that ordinal and thus the alternative source identity. These ordering,
scoring, deduplication and pruning stages are not recovered by materializing
the heap's current physical order. The destination order remains explicit.

Trace mode `2368` and its 640-byte trace/alternative copies are outside the
nontracing API. Runtime alternative counts are explicit; no AK-resource field
is guessed to choose them. The actual-resource test supplies counts one/one,
which proves the inactive-table branch without claiming the native controller
uses those counts for production recognition.

## Independent and actual-resource checks

Managed and Windows Native AOT passed 512 independent complete-prefix controls
and 113 primary/313 secondary alternative blocks. The independent oracle
performs an immutable prefix rewrite and indexed matrix-block selection.
Controls cover absent/present anchors, zero/one/multiple alternatives, primary
limit boundaries, destination ushort endpoints, ownership, missing ordinals,
truncated tables and cancellation.

Real RU source/cuts/graph/context/neural/cache/native acoustic scores produce
six retained initial heap records. Materialization replaces each missing anchor
with its actual stored arc. Ten independently classified second-glyph acoustic
increments then drive 60 explicit diagnostic continuations, 27 allocations,
13 successful promotions and eight retained records. The continuation consumes
the materialized root anchor: its full initial span proves no previous context,
instead of treating the parent record as previous text. All 87 parent/anchor
lookups address the owned supplied rows. Materializing a second row and creating
a third continuation preserves the original anchor node/arc and consumes exactly
the parent plus that root. This closes the slot-to-row-to-continuation lifecycle
gap; explicit scheduling, line frame and state/payload costs are not replaced by
a fabricated native search controller.

```powershell
python tools/LegacyWordRow.Probe/reference_controls.py .artifacts/legacy-word-row-controls.json
dotnet run --project tools/LegacyWordRow.Probe -c Release -- .artifacts/legacy-word-row-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

Remaining native work includes the controller's scoring/sort comparators,
duplicate handling, dynamic heap capacity/pruning, native search scheduling,
winner output and calibrated acceptance confidence.
