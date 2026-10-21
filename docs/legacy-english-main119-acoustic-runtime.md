# Actual English MAIN119 acoustic checkpoint N

**Review correction,2026-10-21:** the native2B1 producer proof is incomplete.
Further static review identified default calibration in BF090/BFF40 and the
raw88-dependent local-extent policy. The current implementation and its
independent reference both omit these branches. The captures below reproduce
the current implementation; they do not establish native feature equivalence.
Correction and renewed evaluation are in progress. This scorer is not the
application's default English recognizer.

This checkpoint evaluates the four genuine MAIN107 families for an actual completed English119 single ordinary source/local graph and the explicit independent main13B-only masks. It does not select a linguistic word, accept text, replace the application recognizer, or claim English handwriting is complete.

`LegacyEnglishMainOnlyAcousticScorer.Prepare(masks)` requires the actual M owner and retains its exact Source identity. `ScoreArc(node, ordinal)` resolves the actual immutable local graph's group and span. It returns defensive copies of 220 costs, a shortlist, fallback cost, span-power and negative-baseline metadata. The detached scorer has a private once-published result per live group; it does not claim the native mutable group-pool/epoch ABI. A group used with multiple spans is explicitly refused before evaluation.

The owned model checks the exact 1111/1111 variant2 profile, real boundary signs and four aligned107 wire objects (228/210/285/285 inputs, 200 hidden, 182 outputs). It copies their matrices once per actual Source identity (360,960 logical payload bytes), not per glyph. Equal-byte foreign Source instances remain distinct. No first-stage or Russian model/header is relabeled as MAIN.

The feature join uses the actual stored ranges for float257 dynamic weights and boundary signs, and native signed source-key ordering for integer normalization. Actual FF/FF100 gives baseline0/height100. The pen-up cap is10. The 226/208 trajectory/raster inputs include their real baseline9; directional temporal/spatial vectors use actual2B1 full-int32 transforms followed by signed-short saturation. The degenerate00A770 branch retains its native literal condition order, including the unreachable second comparison under the admitted baseline0/100 configuration.

All four actual slot modes are2 and use the actual257 mode2 weight. Unmasked mixed EXP/sigmoid uses special180; the two actual212 normalization slots are the pinned RET0 no-op. Related-case80 relaxation runs before division of the weighted integer sum. Ordinary primitive/fallback costs, B8 size penalties, whole-stroke dot rule, seven ordered positive groups, quote slots, native heap pruning, actual span+1 power100, ranking and soft-hyphen copy retain their proved order.

The distinct physical-source count is retained separately from span. Native1B95A0 is a real no-op for this resource: its entire body requires BOTH receiver148/150 pointers. The actual119 reader1C2940 initializes bothNULL and leaves themNULL when the raw presence byte at545671 is0; only its present branch creates them. The typed wire and N model both reject presence. FF/FF are normalization flags, never count bounds. Native power's actual FC byte at545658 is1, so its count is the requested arc span+1, not the distinct-source count. The N native audit additionally verifies both complete regions of the previously reviewed M reader.

Production retains only immutable small scores; full vectors/NN intermediates are transient internal diagnostics. Group/span arc pairs, current ink/probability rows and mask/group data are privately snapshotted once. Cache hits resolve arcs in O(1) without copying a whole adjacency list. Public snapshots cannot alter the model, masks, owner, cached output or source720.

The conservative copied-payload envelope reserves 11,599,872 bytes at maximum admitted dimensions (16MiB ceiling) before large N copies. This covers retained graph/range/cache copies plus one in-flight workspace, not process RSS or cumulative allocation across all group evaluations. Bounds are actual completed source: at most256 physical strokes/4096 points,4096 groups/nodes/candidates,131072 certified graph range/adjacency entries,15 selected ranges and4096 selected points per glyph. Independent concurrent calls have separate bounded transient workspaces; publication elects one immutable result. Cumulative computation is separately bounded by the finite graph and cached groups. Cancellation is checked during source/group loops and NN row blocks. Internal diagnostic callbacks execute outside gates; canceled work publishes no entry.

Validation uses fixed raw blind141 inputs (134 actual single owners;7 explicit multiline refusals) and the separately recorded PluColl100 multi-stroke coordinate set (100 actual single owners). No targets are read. All original4,066 groups/16,264 NNs and PluColl11,330 groups/45,320 NNs are reconstructed by an independent Python oracle: source normalization/order/merge, full trajectory/raster/baselines, independent directional coordinate/landmark/intersection/window statistics and saturated2B1 assembly; raw aligned matrices, mixed probabilities, dynamic weights, case/size/groups/pruning/power and all220 cache entries/ranking. Sixteen artificial saturation controls are labeled prepared controls and do not count as handwriting accuracy.

The probe also checks foreign equal-byte Source separation, cross-source kernel refusal, wrong vector width, admission arithmetic without large allocations, defensive snapshots,12 deterministic cancellation stages and a two-thread publication barrier. Managed and NativeAOT outputs must be byte-identical for each input set. Old P/L/A/parent/source/plain/V2/M captures and Russian lexicon/query goldens remain exact. No old implementation file or guard is changed.

Portable checks (SDK is read as bytes only):

```powershell
python tools/LegacyEnglishMainMasks.Probe/reference_actual.py models raw-main.json
python tools/LegacyEnglishMainAcoustic.Probe/reference_profile.py models raw-profile.json
python tools/handwriting/audit_english_main119_acoustic.py --models models --sdk SDK_DIRECTORY --output native-audit.json
python tools/LegacyEnglishMainAcoustic.Probe/audit_output.py models/en-US/en_US-ak-cur.lite.res raw-main.json capture.json
```

Unsupported normalization overflow/geometry domains and unproved INT_MIN hidden-activation inputs explicitly refuse; this is not a promise that every possible source geometry is admitted.

The next missing join is the actual English sparse request/history/controller search and its real word/terminal lifecycle. These acoustic costs alone do not establish a word-level accuracy improvement, native-equivalent full recognition or Russian readiness.
