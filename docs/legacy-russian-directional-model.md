# Experimental temporal directional model diagnostics

`LegacyTemporalDirectionalFeatures` composes the restored coordinate preparation,
target100 resampling, landmarks, grid10 intersections, global17, sixteen temporal
local16 windows, caller274-column calibration and baseline9 into the checked
temporal313 input. Context28 and two boundary values are explicit caller data.
It supplies no baseline/context estimator, model assets, character cuts,
alternative4f window layout or SDK ensemble.

`tools/LegacyDirectionalModel.Probe` has a vendor-free mode using own printed
four-point И/П shapes, own synthetic calibration and a zero-weight network.
It checks source translation with matching baseline translation, complete input
layout, preserved caller context/boundaries, numerical network probabilities,
invalid caller shapes and cancellation. These controls also pass Windows Native AOT.

Its optional three-file mode runs the inspected legacy RU lite temporal model
313→200→180 on eight original printed shapes (И and П with translations and
scales). It accepts caller-owned acoustic resource, activation tables and trained
calibration; none are included in the repository. The inspected network span is
367899..467094 in that specific resource, not a generic resource parser.
The diagnostic explicitly chooses source baseline Y at the shape bottom and
height equal to the shape height, context28 all −1024 and boundaries +1024.
These caller choices are **not** recovered automatic SDK context or baseline.

The actual diagnostic currently gives weak class results: unscaled И chooses
н with 16069/32768, with и third at 3149/32768; unscaled П chooses Latin r with
6413/32768. Translated copies have identical scores. Windows managed and Native
AOT output matches exactly. The program reports these scores and does not
assert that accurate recognition passed. Geometry numerical controls and a
successful real network forward pass alone cannot establish a correct full
recognition profile.

```powershell
dotnet run --project tools/LegacyDirectionalModel.Probe -c Release
# Optional, caller-owned inspected legacy assets only:
dotnet run --project tools/LegacyDirectionalModel.Probe -c Release -- acoustic.res activations.json calibration.json
```

The next requirements are verified automatic baseline/context profiles, the
other model family's channel layout/combination policy, and representative real
writer samples with character/word accuracy measurements. Joined Russian
cursive and AI-pen UI integration are not complete.
