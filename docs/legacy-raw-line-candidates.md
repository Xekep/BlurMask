# Ordinary raw-stroke line candidates

`LegacyRawLineCandidateProducer` restores the ordinary non-special branch of
Text `18006f6e0`, raw-moment costs `18006e6a0`/`18006e8f0` and merge gates
`18006eb30`/`18006ee90`. This fills the missing candidate producer before the
[prepared2e2 costs and free row search](legacy-line-group-search.md). It does
not replace source dispatch, prior-row remapping or child249 recognition with
a guessed automatic pipeline. Static evidence uses Text SHA256
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
No vendor code executes.

## Profile and raw cost

The checked unencoded79-byte2a9/v100 wire node has a16-byte header, uint/float,
three bytes and thirteen floats. Actual Russian nodes5325/5654 have the same
profile: limit200, extent10, flags1/0/1 and coefficients
`1.3,.2,3,8,60,.01,5,20,.5,.5,1,4,5`. Native64-byte configuration has padding
after the flags; `06ddd0` copies it into2dd runtime8..44. The portable parser
reads packed bytes directly, never a resource pointer. Other encoded
nodes/versions are excluded. Cluster IDs and primitive flags stay explicit
until the actual source callback is attached.

Raw cost uses **sums of arc-weighted source moments**, rather than a mean of
normalized prepared-group moments. Horizontal grouping uses Y variance;
vertical uses X. Optional regression blends the raw residual without the
later2e2 determinant stability branch. Nonpositive variance uses the configured
zero penalty; positive variance adds its weighted value and capped inverse.

The ordinary positive explicit-scale/nonzero-count branch disables regression
(`externallySuppliedScale`). This is separate from temporal/special scoring.
Fresh2e2 constructor0691a0 zeros18..FF including special-mode property30;
initializer069720 does not assign it. Configured overrides need source binding.

Merge gap/overlap costs, blend, direction coefficient and square retain float
arithmetic before double accumulation. Horizontal primitive overlap adds
vertical separation penalties, doubled when both input primitives carry the
flag. Maximum extent uses the secondary scale and float bbox subtraction.
Two interval unions preserve one-unit float tolerance. The separate chronology
list uses suffix-minimum starts/prefix-maximum ends and a float positive-gap sum.

## Candidate and tie semantics

The suffix DP considers singleton then longer ranges with strict lower-cost
updates. Known conflicts consult the **best candidate's retained cluster**,
not an invented set of trial labels. Unknowns do not clear it. The unassigned
counter is checked before increment, so limit0 permits one unknown extension.
Equal range costs retain the earlier candidate; equal projection costs choose
vertical. Entries preserve range, moments, own/suffix cost, retained cluster
and successor. Native horizontal byte68 maps to prepared `SpecialSingle`;
this is not a primitive flag or learned class. Ordinary line-label6C remains
`INT_MIN`. Selected successor progress is validated.

Actual RU enables only horizontal projection. Synthetic controls explicitly
exercise other projection combinations without inferring a vertical RU default.

## Verification and remaining chain

```powershell
python tools/LegacyRawLineCandidates.Probe/reference_controls.py controls.json
dotnet run --project tools/LegacyRawLineCandidates.Probe -c Release -- controls.json models/ru-RU/ru_RU-ak-cur.lite.res
```

384 controls check2230 full records.256 unlabeled cases enumerate every suffix
partition independently of the managed DP;128 labeled cases exercise best-state
gates. Range costs are reconstructed independently from point moments, sorted
interval components and prefix/suffix chronology. Cases cover projections,
regression, primitive/overlap penalties, budgets, ties, flat/large-coordinate
geometry and cluster IDs. Malformed resources, metadata, successors, overflow
and cancellation are rejected. Fresh Managed and Windows NativeAOT pass.

Own three-stroke geometry with the real profile selects one group with cost
4.086185973249186 (regression) or4.16596443147958 (explicit scale). This is a
geometry diagnostic, not Russian handwriting accuracy or native child249 parity.

Bounds are256 physical strokes,4096 total points,4096 configured unlabeled
merges and finite scales/results. Complete2e2 still needs06aaf0 prior-row/index
remapping and06c7b0 materialization. Temporal06f010 scoring, associated records,
constrained row lattices, child249 statistics and final native word selection
remain separate unfinished boundaries; none is replaced with this helper.
