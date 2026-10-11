# Native neural line proposals and pair evidence

`LegacyNeuralLineProposals` connects prepared physical stroke geometry to the
two real embedded line networks. It ports the ordinary horizontal-window
producer `Text180072610`, ordering producer `180073de0`, the neural input
consumer in `18007d7c0`, and pair aggregation `18007bc90`. This is a production
of native pair evidence, not the remaining native row-selection algorithm.

The caller supplies prepared 17-field stroke records, active physical flags,
nominal window width, overlap and scale. The initializer `180076780` sets the
original width multiplier to5 and overlap to0.75, but this bounded API keeps
those explicit: source configuration and the scale sample provider are outside
its contract. It does not derive a recognition baseline from a whole-ink box.

The window producer uses float32 arithmetic, including the0.01 extent padding,
overlapping windows, optional right-anchored tail, and final whole-width window.
Each nonempty local window produces mean-Y, top-Y, bottom-Y and physical-order
sequences; the final whole-width window produces only physical order unless
it is the only window. The middle-pivot sort swaps equal keys, preserving
`180072550` behavior rather than substituting a stable sort.

The first three sequences use the30-state model loaded at runtime+F8. Physical
order uses the50-state model at+F0. The42-channel producer creates a self-pair
first row, but **native inference skips it**: `18007d7c0` sets the descriptor
input pointer to features+42floats and frame count to proposalCount−1. This
omission happens before affine normalization and recurrent inference; removing
a prediction afterwards would retain a different recurrent context.

Engine descriptor5F at`180229f30` points through metadata`18022a0c0` to runtime
table`180229f80`. Slot+C0 is`18010dd00`, which dispatches to`180184130`. The last
argument1 requests top1. It writes the maximum output's index and probability,
retaining the lower index on exact ties. The pair consumer normalizes endpoint
order, keeps first encounter order, and replaces duplicate evidence only for
strictly greater confidence. A dictionary accelerates identity lookup without
changing that output order or tie policy.

The source-frame route is also identified statically:2E3.virtual40=`180078220`
calls2C1.virtual10=`180059940` through descriptor`180276930` and table`180276978`.
The ordinary point dispatcher`180057ae0` reaches the2E3 receiver`180077140`.
This binding does not itself establish all opaque/custom input branches.

## Scope and verification

128 independent geometry cases cover wide/narrow windows, active subsets,
singletons, equal sort keys, endpoint overlap and native tail/global variants.
The NumPy oracle uses independent existing feature/network arithmetic and
constructs only real adjacent pairs, so it never presents a self frame to the
network. Both original own synthetic models and the two real embedded models
are exercised. Original printed MIR physical strokes and a second translated
row additionally check actual resources; these are diagnostic fixtures, not
Russian handwriting accuracy evidence.

Managed and Windows Native AOT pass 130 cases, 5,423 proposals and 104
independently scored aggregated pairs. In the own single-row MIR fixture,
within-row pairs have label zero with confidence above 0.99999. With two
translated rows, within-row pairs retain label zero above 0.99995, while two
cross-row pairs have label one with confidence 0.9711 and 0.9923. Native merging
and row selection are still required to interpret this evidence as final lines.

```powershell
python tools/LegacyNeuralLineProposals.Probe/reference_controls.py D:/BlurMask-checks/proposals.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res
dotnet run --project tools/LegacyNeuralLineProposals.Probe -c Release -- D:/BlurMask-checks/proposals.json
```

Remaining stages include vertical sweep/pruning`18007d530`, pair-to-group
selection`18007be60`/`18007c160`, native scale/configuration forwarding and final
source219 line metadata. Consequently this component does not automatically
replace the manually supplied word-recognizer frame or enable Russian cursive
in the application. No vendor DLL or GUI is executed.
