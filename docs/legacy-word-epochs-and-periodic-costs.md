# Word-controller epochs and periodic costs

These are two exact source/controller stages consumed by the owning DIRECT
word pass. They do not infer final-result options or acceptance confidence.

## SourceV80 epochs

The native controller invokes source30.V80 before its loop with completed
node-1 and after each completed node. `290:2372F0` calls `237080`, which resets
eight normal caches, sets next-slot41BE0 to zero, resets the single auxiliary
cache, then forwards to the selected backend. `236F30` resets each level's
query40/48 backend-owner/cache pointers as well as counts and cache tags.

`ResetForControllerNode` on both delimiter source classes restores their
cache portion. Managed request objects are replaced to preserve the opaque
query-owner reset. Existing outer query objects still refer to their same
normal slot; new outer queries start at slot0. Record backing is not fabricated
as a new native allocation. The owning pass then calls
`LegacyOwnedMainWordSpanPreparation.AdvanceNodeEpoch` on the same backend.
Permission-map D0 refresh does not imply a cache epoch. Failed sessions cannot
be revived by a reset, and resets serialize with reads.

The standalone epoch probe checks normal cache resume versus the auxiliary
parameter-refresh replay, then reset/replay at four accepted offsets. Exact
backend request-object identities and offsets, nextSlot0/1 and failed/canceled
lifecycle guards are checked independently of emitted word text.

## Periodic normalization

After source/retained count publication, `2144F0` copies the preceding
runtime2520 entry, or writes zero at node0. Its nontracing branch executes
every50 nodes when knowledge99/A8 are zero and unsigned `(minimum-1)<3FFFFFF`.
Fresh `00C860` writes99 andA8 to zero; the checked fresh policy has no property
overrides or tracing. The minimum is captured from primary and language rows
actually materialized after their quotas, before stored-row compaction.

FrontendA0 `151FF0` forwards to graphV30 `1A75D0`. For the actual current node
it returns `min(node+1, resource span cap8)`. The inclusive loop therefore
normalizes nine source nodes at the first event:49 down through41. Every
finite stored cost10 inside source-end23F0 loses the positive minimum with
signed32 wrap. Cost14 and all other bytes survive. Source rows beyond retained
end23F8 are included; allocation rows beyond source-end are not. Secondary
alternative-count1 has no separate pool adjustments. Primary alternative
payloads are not shifted by this native branch.

Each affected2520 entry gains the minimum with signed32 wrap. Following nodes
copy the most recent cumulative value. The owning pass preserves mutable
previous rows for subsequent parent/history lookups and returns final snapshots
after any later normalization has modified earlier nodes.

`LegacyNativeWordPeriodicCostNormalization` restores the two bounded byte/vector
operations over explicit owning inputs.512 independently generated controls
check722304 whole88-byte storage bytes and8208 cumulative cells, including
signed overflow, impossible/negative/zero costs, cost14, untouched outside-source
allocation, future vector entries and the inclusive backward endpoint.
Seven invalid-extent/cancellation guards accompany them.

```powershell
python tools/LegacyWordPeriodicNormalization.Probe/reference_controls.py E:/BlurMask-build-artifacts-20261014/word-periodic-controls.json
dotnet run --project tools/LegacyWordPeriodicNormalization.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/word-periodic-managed -- E:/BlurMask-build-artifacts-20261014/word-periodic-controls.json
dotnet run --project tools/LegacyDelimiterEpoch.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/delimiter-epoch-managed -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
```

The actual14-physical/56-node main pass reaches the first normalization and
produces offset346098 at node49. An independent scheduler keeps delayed
per-source offsets instead of mutating NodeBuffers or calling this helper;
it compares all final stored rows, alternative payloads, cumulative state and
symbolic ownership. The former uniform15-physical29B failure was a premature
portable rejection of a nonfinite intermediate before a native variance retry.
The [retry correction](legacy-fresh-main-gap-mixture.md) now verifies that real
15-stroke input through a finite gap and265 output. This does not by itself
establish a successful60-node word-pass/normalization control.
