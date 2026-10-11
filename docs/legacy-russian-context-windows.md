# Legacy initial context windows

`LegacyContextWindowScorer` composes the checked single-line, fresh-cache subset
of `1801c9870`: windows of whole physical strokes, extended30 geometry,
float-to-fixed input, initial7001 network, EXP class probabilities and strict
nonblank-mass acceptance. Accepted records can then pass through
`LegacyPriorContextSelection` and `LegacyContextWeighting` into the28 appended
directional input channels. No vendor weights or activation tables are included.

The caller supplies integer-coordinate strokes for one line, line height,
vertical reference, per-stroke scope IDs and the matching model/activations.
Windows begin at physical stroke1; stroke0 contributes the earlier X reference.
Each start generates prefixes of at most the caller limit1..3 (inspected RU
profile2), stopping at a changed scope when enabled. The window center is its
point-count mean X, not a bounding-box center or arc-weighted center.

The horizontal reference accumulates across **all earlier physical strokes**.
It is the maximum earlier X in the default geometry mode and minimum earlier X
in the maximum-X geometry mode. Native `fVar41` is initialized outside the
start-loop and is not reset at each start or scope change. A backwards-stroke
regression uses a deliberately geometry-sensitive own network to distinguish
this rule from a last-stroke-only reference in both modes.

The scorer accepts a window when the sum of all output probabilities except
the caller-specified blank class strictly exceeds the nonnegative cutoff. The
inspected RU model has30 inputs,100 hidden units and28 output classes, blank
class27 and cutoff0.1. Probabilities use the checked EXP integer conversion and
the native1/32768 float scale, not independent sigmoid output. Native negative
cutoff mode and inclusion/exclusion variants are outside this API.

Own controls cover window indices, mean centers, cumulative reference in both
modes, scope stops, strict cutoff equality, empty windows for a sole reference
stroke, target-overlap exclusion, weighting/fixed composition, malformed inputs,
zero-extent rejection and cancellation. The implementation bounds lines to256
physical strokes and4096 points; a zero-extent window is rejected rather than
guessing the unimplemented native special branch.

Optional caller-asset diagnostics run the real RU initial context network and
optionally the directional313 network on an original printed **МАМА** fixture.
The two А glyphs each contain a triangle and a separate crossbar, giving six
physical strokes with explicit glyph groups `[0]`, `[1,2]`, `[3]`, `[4,5]`.
With supplied line height/Yreference40, eight context windows are accepted;
target exclusion leaves8/4/6/5 observations respectively. Geometry, fixed
context values and top acoustic classes are printed for inspection.

Under explicit caller baseline/calibration and boundary values1024, А is headed
by Russian а:19188/32768 without context and18961/32768 with context. М is still
headed by н:8727/32768 without context and7441/32768 with context. The expected
М becomes one of the next candidates, but this is not a successful МАМА word
recognition or an accuracy benchmark. Only the restored313 family is evaluated;
the alternative family, combination/candidate policy, automatic source baseline
and connected-character cuts remain unresolved. The experimental scorer is
not enabled in the AI-pen UI, and no SDK activation is bypassed.

```powershell
dotnet run --project tools/LegacyContextWindow.Probe -c Release
# Optional caller-owned inspected assets, then optional274-channel calibration:
dotnet run --project tools/LegacyContextWindow.Probe -c Release -- acoustic.res activations.json calibration.json
```
