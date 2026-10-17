# Owned main glyph-refresh scoring

`LegacyOwnedRussianMainGlyphScoring` shares the actual acoustic and geometry
cache constructor used by ordinary `216BC0`. The ordinary producer delegates to
this service; it retains its own symbol-resolution, unit-score, flags, scratch
and partial-failure behavior. This is not another neural model or a parallel
heuristic cost implementation.

The public refresh factory requires the exact
`LegacyOwnedMainGlyphSourceMetadata` owner, pinned main acoustic resource,
materialization confidence, graph, source configuration and engine separation
table. A numerically identical foreign metadata result is refused. Higher source
references retain their target identity while the original base node/arc selects
the real acoustic group. Geometry costs use that group's actual ranges and
source height, not the merged source range's bounding box.

`ReadPreparedSymbol` takes an explicitly prepared normalized symbol from the
`216860` boundary and compact-byte20 bit40. It does not infer a label or convert
a synthetic quotation label. It creates the exact 16-byte `1C0` request: actual
base node/arc, candidate ordinal `FFFF`, normalized symbol, start/end category
geometry index, and zero control/residue bytes. A closed primitive uses the
native geometry bypass. Missing category ownership is an explicit refusal;
there is no `216BC0` unit-score shortcut in this lazy-refresh caller.

The result preserves first-candidate vector bits (`1C1520`), calibrated symbol
vector bits (`1C1300`, actual owner88 channel0), cost (`1C13A0`) and copied request
bytes. Normalized candidate24 field0 is not a substitute for either vector.
`ReadFirstCandidateScoreBits` and `ReadSymbolScoreBits` are separate operations
because native `217670` writes owner1C before symbol resolution, then glyph10
before constructing/calling the cost request. A caller can preserve those
earlier writes when a later operation fails. This service mutates no glyph pool,
descriptor, source reference or string publication.

## Preparation and cache scope

Pinned frontend `152A70` calls graph `V138 -> 1AC0E0`, which checks the sampler
`V100` pointer. The actual fresh v106 sampler slot is null. It then calls acoustic
`V158 -> 1C2530`, whose entire body stores `FFFF` into last-group word3C2 and
returns. In this immutable owned-source scope, it changes no neural output,
mask, category, geometry cache or probability. Portable scoring owns immutable
group caches and selects each group explicitly, so it needs no native
last-group pointer/sentinel. This proof does not authorize arbitrary sampler
preparations or mutable/reused native sources.

## Checks

The new probe runs the actual CASE scheduler, terminal selection, compact
materializer and V140 metadata owner before prepared-symbol scalar controls.
It covers a one-physical raw trajectory, a two-physical source with nonzero
separation costs, and two pinned recorded word tracks. Recorded tracks have no
pen-up metadata and are explicitly treated as unsegmented paths. Expected
transcriptions are not opened. Diagnostic higher-source merges are genuine
source-overlay operations, not claimed selected-path association events.

Per runtime: 726 source reads, 493 higher-source reads, 21,512 successful cost
requests, 1,452 explicit category refusals and 48 guards. The independent Python
auditor reads raw AK category maps and confidence coefficients, recomputes
float32 confidence, separation costs, full request16 bytes and final costs.
The two-physical input exercises 16 nonzero geometry groups. These tests measure
this contract, not OCR accuracy.

Managed and Windows NativeAOT captures match byte for byte. Existing CASE
3,104-control and DIRECT 3,288-control complete scratch/score/full40 captures
also match their original frozen captures in both runtimes, including flags,
synthetic quotes, unit-score branches and retained-prefix refusals.

Probe CLI: AK, LK, engine profile, separation profile, case data, publication
data and output JSON. Append line30, line50, a private pinned corpus manifest
and index `0` or `6` for the recorded controls. To select the two-physical raw
fixture, append line30, line50, `unused`, `twoPhysical` instead. Audit each capture
with `audit_output.py AK separation.json capture.json`.

`tools/handwriting/audit_main_glyph_refresh_scoring.py` pins four native scoring
regions, 49 exact instructions and six virtual bindings. It also runs the
separate scoped preparation audit. Vendor binaries are read statically and are
never loaded or executed. Actual `216860` symbol production, descriptor append,
mutable pool refresh and complete `0213A0/021000` wrapper orchestration remain
separate stages.
