# Owning main DIRECT word pass

`LegacyOwnedMainDirectWordPass` connects the prepared ordinary main source to
the ascending `2144F0` node/span pass. It uses the actual main sampler graph,
four acoustic networks,195-slot caches, per-span acoustic dictionary
permissions, dictionary queries, geometry, alternative records and language
costs. It produces owned stored88 rows and primary40 alternatives; it does not
accept expected text or substitute Latin/Cyrillic lookalikes.

The input policy is `LegacyOwnedFreshMainWordControllerPolicy`, with its actual
selected247 metadata and `151BF0/20C8C0` limits. Its DIRECT source deliberately
does not construct the case-aware `20C9D0` factory. Case-aware span owners are
rejected. Both the original single-glyph source and the new typed
multi-physical/single-descriptor source have concrete constructors. A supplied
multi-physical acoustic scorer must be the exact span owner's scorer.

## Producer/consumer order

* Each node resets the same owned descriptor with `212D80` phase0. Its backing
  allocation survives logical capacity changes and the following language
  phase; inactive allocated bytes are retained.
* The actual sourceV80 epoch occurs before the first node (`-1`) and after
  every completed node. Outer290 resets all normal/auxiliary query owners and
  its next-slot counter before forwarding to the same bound dictionary source.
  Permission refresh alone never resets these caches.
* Spans are prepared in ascending actual frontend98 extent. D0's real cache
  and permission refresh precede every query. The same persistent290 instance
  reads normal pages across spans; no permission epoch invalidation is invented.
* Whole-prefix spans consume all `1FD240` initial queries, including the
  already implemented quote and primitive branches. Other spans visit finite
  primary rows in stored order, apply the native signed word-length68 guard,
  then `2136D0` flag prepass and `201270` parent query/scoring/allocation.
* The checked DIRECT `290.V60` is false, so `1FE740` is not scheduled. This
  follows the source-class getter, rather than suppressing a troublesome page.
* Native ordinal58, root/stored-parent alternative production, literal sorting
  and deduplication precede `212EC0` primary quota and `213410` row writes.
  The shared alternative pool retains its post-merge bytes for the next node.
* The actual graph-boundary `203C90` gate decides whether language runs; the
  final node always runs it. `212D80` phase1 resets the retained descriptor,
  and the real290 auxiliary source feeds `204290` current-terminal injection,
  category/language cost, computed restart and `1F9080` allocation.
* Phase1 ordering/quota precedes language row materialization and `212B10`.
  A false language gate calls `FinishWithoutLanguage`, preserving125/125 and
  bypassing compaction. Both paths perform native primary-alternative cleanup.
* Every50 nodes, the fresh zero99/A8, nontracing branch normalizes a positive
  finite minimum. FrontendA0 forwards to actual graph `1A75D0`; its inclusive
  backward loop covers current node plus the last eight source nodes here.
  Only finite row cost10 changes, including source rows beyond retained-end;
  row14 and other storage bytes survive. The corresponding2520 cumulative
  entries increase with signed32 wrap and future nodes copy the current value.

Source-end23F0, retained-end23F8 and finite primary count remain distinct.
Language preparation uses the checked first-word/root-anchor consumer; a source
requiring another history, protocol, associated-controller or geometry branch
fails explicitly. No returned dictionary page is filtered to pass this guard.
The query and auxiliary work budgets are explicit finite safety bounds.

## Storage and boundaries

`LegacyMainDirectStorageProvider` supplies all allocation bytes and opaque
identities, including row/pool storage and unwritten request/lexical residue.
The implementation never claims that native allocation residue is zero.
Growing allocations must preserve every previous byte. Heap backing is copied
back with its full allocated extent, beyond the current logical capacity.
The auxiliary query buffer is also persistent across nodes.

This is logical source-row ownership. Grouped `1FA300` native allocations,
pointer/ring alias identities, the outer result-selection options and final
publication remain separate producers. Single-use protection is atomic;
cancellation after an allocation callback is observed before use. An entered
failed/canceled pass cannot be resumed. Unsupported scopes are reported rather
than replaced by synthetic metadata or an arbitrary confidence.

## Checks

```powershell
dotnet run --project tools/LegacyMainDirectWordPass.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/main-direct-word-pass-managed -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res models/text-unicode/legacy-case-data.json models/text-unicode/legacy-text-publication-data.json
```

The four-node single-input control compares an independent procedural scheduler
against the reusable pass, including94912 complete row/pool storage bytes and
2624 context fields from separately owned context factories. Literal context
paths are compared instead of assuming managed identities are native pointers.
Lower scoring, geometry, ordering, quota and writer APIs have their own
independent scalar and full-record controls. The procedural oracle uses the
actual zero-terminal direct auxiliary source plus current-terminal injection;
the reusable pass additionally exercises the separately verified290.V48 layer.

This control consumes39 initial and310 parent query rows, with123 flag
allocations. Only its last node runs language, with18 allocations; its source
end143 and retained end141 differ after compaction. The earlier single-input
fixture still has explicitly limited first-parser proposals upstream, so these
counts are integration regression checks rather than a handwriting-quality score.

The optional four extra arguments exercise14 physical trajectories through
actual first-parser/protocol processing,720 source records,24B/24D/29B/265,
434 prepared points,420 main acoustic groups and nonempty main context. The
owning pass visits all56 source nodes and14 language gates:79 initial/9741
parent query rows,13000 flag allocations and823 language query/allocation
events. Its last source/retained ends are156/156. Node49 applies the actual
offset346098 to sources49..41; future cumulative entries retain that offset.
An independent delayed-offset scheduler representation compares all returned
storage, giving1423680 total bytes and39360 exact symbolic fields including
the single-input control. It does not call the implementation's normalization
helper or mutate the reference NodeBuffers in the same way. The test supplies
no expected labels to the main recognizer. First-parser supported-context restrictions,
DIRECT dictionary factory scope and missing final-winner/acceptance calibration
remain explicit; these tests do not enable production Russian AI-pen replacement.

An additional attempted stress input of15 perfectly equally spaced copies
stops upstream in `LegacyNativeOrdinaryGapMixture.Fit`: the29B mixture
intersection is nonfinite. It does not reach the word pass. That geometry
refusal remains unresolved and is not hidden by filtering its input or treated
as a successful60-node decoding check. Further stress coverage is separate.

The old node probe additionally checks128 full-allocation controls,32
false-language whole-storage controls and22 lifecycle guards without changing
its original quote/parent/compaction goldens. Managed and Windows Native AOT
exercise the same controls.
