# Prepared native group costs and free line search

This adds checked pieces of the legacy Text class-2dd / class-2e2 geometry chain. It does **not** complete automatic page clusters, baseline fitting, font placement, or the newer class-2e3 group search. It does not supply guessed native configuration values.

## Recovered stages

`LegacyLineArcMoments.ExtractStroke` implements the moment portion of `18006e080`. A segment contributes its length as weight and its midpoint to X/Y/XX/YY/XY moments. Midpoint addition and coordinate subtraction retain native float precision; length and moments use double. The last point contributes an additional unit weight. The bounds include every input point. Zero-length segments remain valid; a one-point stroke has unit weight. This is not the unweighted mean in the separate 17-channel class-2e3 feature producer. Stroke grouping, temporal metadata and language-specific flags are separate native stages and are not inferred here.

`LegacyPreparedLineGroupCost.Score` implements `180068950` (X projection) and `1800682f0` (Y projection), on explicit native-style 0x90 group records. Those records contain start/end stroke indices, weight and six moment fields, axis bounds, and caller-provided flags/labels. The native class-2dd search `18006f6e0` produces such candidates; that preceding search is not yet ported.

The cost includes:

- a base or explicitly marked singleton cost;
- positive projected gaps, with a separate weight for adjacent original stroke IDs;
- a projected variance cost with a capped inverse-variance term;
- optionally, a blended linear-regression residual with the native determinant stability check;
- a competing regression cost that adds a squared gap penalty from the union of orthogonal intervals.

Interval union uses the original one-coordinate-unit tolerance in float arithmetic (`180067fd0`). Its gap accumulator retains float differences and the backwards batch order (`180068230`). Profile fields correspond, in order, to class-2e2 offsets D0, D4, D8, DC, E0, E4, E8, EC, F0, and F8. The F8 value is the effective regression blend length; the native caller may copy/disable F4 before inference.

The full C export incorrectly reports both cost functions as `void` and drops their return-value calculations. Static x64 disassembly establishes the missing behavior:

- `1800689c0..1800689d2` / `180068360..180068372`: singleton/base selection;
- `180068ad3..180068b25` / `180068473..1800684c5`: gap and adjacency accumulation;
- `180068b7f..180068d03` / `18006851f..1800686a2`: normalized variance and blended regression;
- `180068d0e..180068d9a` / `1800686b8..180068744`: min of fitted and variance-only costs;
- `180068ee5..180068f42` / `180068894..1800688f1`: simple-variance fallback and XMM0 return.

The inspected DLL is pinned to SHA-256 `18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`. No native DLL is executed by the runtime or these probes.

`LegacyFreeLineGroupSearch.Search` implements the **free row-count** branch of `18006bc00`, with prepared moments and explicit labels. It keeps strict lower-cost updates, the native backwards candidate order, row/cluster boundary restrictions, and the unusual native rule where an unknown label does not clear a previously assigned best-state label. It returns ranges and cost. It does not return a baseline. The alternate branch with a supplied row lattice and monotone row-index search remains outside this API.

## Remaining chain

The known chain is:

```text
points -> 2dd arc moments                 [ported]
       -> 2dd candidate merging/search    [not ported]
       -> 2e2 projected costs             [ported]
       -> free row-count DP               [ported]
       -> groups/cluster fitting          [not complete]
       -> prepared cluster baseline data [materializer already ported separately]
```

Calling the free search on one independently extracted record per stroke supplies an explicit candidate list; it does not reproduce the missing native candidate merging. The newer 2e3 neural boundaries already have a separate checked feature/inference path, but their downstream candidate merging and baseline calculation are also incomplete. No separate learned font-placement network is established by these findings.

## Validation

`tools/LegacyLineGroupSearch.Probe/reference_controls.py` uses 160 independently generated moment sets, both projections, singular/constant geometry, different scales and enabled/disabled regression. It checks 320 costs against a separate scalar moment calculation, and free-search minima against exhaustive enumeration of every partition (up to eight groups). It also independently prepares 1,440 arc-moment records from float input points. Explicit probes cover ties, known equal/different labels, cluster restrictions, malformed finite input, overflow, and cancellation.

```powershell
python tools/LegacyLineGroupSearch.Probe/reference_controls.py .artifacts/line-group-search-controls.json
dotnet run --project tools/LegacyLineGroupSearch.Probe -c Release -- .artifacts/line-group-search-controls.json
dotnet publish tools/LegacyLineGroupSearch.Probe -c Release -r win-x64 --self-contained true -p:PublishAot=true -o .artifacts/line-group-probe
.artifacts/line-group-probe/LegacyLineGroupSearch.Probe.exe .artifacts/line-group-search-controls.json
```

Managed and Windows Native AOT probes pass. Cost and search accept bounded explicit inputs (512 and 128 groups respectively), check finite arithmetic, and never silently substitute another grouping rule on overflow. These are independent reference comparisons to the statically recovered algorithm, not end-to-end comparisons with a running vendor engine.
