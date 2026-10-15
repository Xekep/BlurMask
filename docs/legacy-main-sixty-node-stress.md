# Fifteen physical strokes through the sixty-node DIRECT pass

`tools/LegacyMainSixtyNodeStress.Probe` checks the formerly blocked uniformly
spaced15-stroke input after the [29B retry correction](legacy-fresh-main-gap-mixture.md).
It links the existing immutable `MainLineWordFixture` and independent
`OriginalDirectSchedulerOracle`; no runtime implementation, fixture points or
scheduler reference were changed for this test.

The actual raw trajectories pass through2E3 grouping, source split/reordering,
first acoustic/protocol/native275/source720,24B/24D/active29B/265 and the main
sampler/context/four-family acoustic caches. The first fixture's proposal mask
remains explicitly diagnostic; the main source owns its actual dictionary
permissions. No expected text, chosen label or terminal row is supplied.

The resulting main graph has452 groups and60 nodes. An owning `RunOwned()` and
a separately prepared source/context owner running the existing procedural
oracle produce identical counts, source/retained extents and final storage:

- 1,423,680 complete row/primary-alternative scalar bytes.
- 39,360 symbolic owner fields, compared by exact symbol/parent paths across
  separate context factories. Native pointer values are not invented.
- 79 root queries,9,773 parent queries,13,359 flag allocations and826 language
  queries/allocations. All15 source-end language stages execute.
- One persistent heap backing allocation; native allocation residue is
  explicitly supplied zero by the probe, not inferred as a universal default.

The independent scheduler uses delayed per-source cost offsets rather than
calling the runtime's periodic normalization helper. Node49 applies346098 to
the inclusive range49..41. Nodes0..40 retain zero cumulative cost; future
nodes50..59 copy346098. Every final row, alternative payload and cumulative2520
entry is compared after those mutations.

The owned terminal bridge consumes the actual final SOURCE23F0 extent and2520
value. A separate literal fresh-invocation scalar scan chooses pre-207CB0
ordinal127, relative cost110955 plus346098 cumulative, total457053; its entire
88-byte record matches. Repeated selection remains stable after mutating
public copies. Four guards check Run/RunOwned single-use, foreign controller
identity and cancellation. This establishes a numeric beam selection only.
`207CB0`, symbolic result reconstruction, case-aware completion and acceptance
confidence remain separate unfinished stages.

The output JSON records counts, all60 cumulative values, selected88 bytes and
a digest of canonical full storage plus symbolic paths. Managed Release and
fresh Windows NativeAOT are compared byte for byte. The references share the
already verified lower native components; this is independent orchestration
and storage coverage, not a second independent neural-network implementation.

```powershell
dotnet run --project tools/LegacyMainSixtyNodeStress.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/main-sixty-stress-build -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res models/text-unicode/legacy-case-data.json models/text-unicode/legacy-text-publication-data.json E:/BlurMask-build-artifacts-20261014/main-sixty-stress-managed.json
```

Use the same nine arguments for the NativeAOT executable. Build artifacts and
both TEMP/TMP belong on the task's external build drive. No vendor DLL, GUI,
installer or production server is invoked.
