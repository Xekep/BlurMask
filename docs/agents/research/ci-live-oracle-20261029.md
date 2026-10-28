# GLOBAL LIVE reference: restore the language path

General CI failed the unchanged single-stroke case at compact-reference offset28.
The reference omitted its optional GLOBAL language context; production already
supplied it. It made6 LIVE backend calls instead of23, omitted17 language queries
and produced a different716-record pool. Native Text207780/207B00 includes the
source/language calls, LM-cost addition and candidate-heap path.

The [tools-only fix](../../../tools/LegacyMainGlobalLivePublication.Probe/OriginalGlobalLiveSchedulerOracle.cs)
supplies the reference policy's own live lease and explicit zero allocator,
preserving previous storage. Production code, byte/context comparisons and
assertion masks remain unchanged. This procedural reference still shares the
publication kernel; it is not a new fully independent numerical implementation.

Evidence: `E:/BlurMask-build-artifacts-20261014/global-live-physical1-review-20261028/`.
The full original31/35/60/61/1 probe passes86 guards,25650352 bytes and628004
symbolic fields on the same935 DLL. The unchanged independent Python raw-AK/LK
auditor checks all four captures, including716 LIVE and1057 final records.
Production compact bytes before/after the reference fix are identical. Fresh
native evidence verifies3950 PE instructions. This corrects a stale reference
gate, without establishing any Russian recognition-quality improvement.
