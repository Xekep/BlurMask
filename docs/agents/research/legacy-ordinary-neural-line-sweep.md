# Whole ordinary2e3 neural line inference

`LegacyOrdinaryNeuralLineInferenceFactory` composes the ordinary135, unknown,
unlocked, empty-prior and empty-guideline branch of Text `18007d7c0`. It takes
an owned source219/scale snapshot, explicit input resolution, source-record
half-horizontal flags, the shared quarter-vertical flag, internal quantile,
window parameters,30/50-state networks and a selected class24a geometry profile.
It returns the inferred frames, physical row mapping, actual internal neural
scale, vertical strip count, tiny-stroke metadata and aggregated pair evidence.
These are explicit caller settings, not guessed Russian constructor bindings.

## Source scale and internal sampling

The native single-active-stroke dispatch `07ca80` occurs before `078df0`.
Accordingly a one-physical-stroke input uses its source scale, no pair network,
no internal quantile override and no newly constructed tiny-stroke list.

For multiple physical strokes, `078df0` reads the receiver's last-accepted-
filtered points. It invokes `0720c0` per active stroke. More than nine resulting
samples triggers `078ca0`, even when source219 has already supplied a positive
scale. Its index rounds float32 `(count-1)*quantile` half away, clamps to the last
sample, and applies its float resolution/25.4 floor. The shared flag reduces
interior vertical displacement to0.75 of its original value only when the physical record's half-horizontal
flag is set; the endpoint branch keeps the full vertical displacement. The
ordinary initializer's quantile0.8 is used explicitly by the resource diagnostic;
the API does not silently choose a caller's configuration overrides.

The tiny-ID list is formed using physical height <=0.1*scale or arc length
<=0.25*scale. These IDs are supplied to the42-channel feature producer. This
metadata does not delete their strokes from proposals, geometry or final rows.

## Exact sweep and deferred continuation

`072610`/`073de0` produce the initial horizontal windows and ordered proposals.
The vertical interval starts at minimum Y. Each strip has float32 height40*scale
and overlap0.2*height; its regular next start is previous end minus that overlap.
When the next end reaches `maximumY-overlap`, the final end is maximumY.
The exact `07d530` consumer filters each proposal by inclusive vertical overlap.
Future/near-bottom strokes mark a shared per-strip byte mask, including when a
filtered proposal leaves fewer than two strokes. Single-ID original proposals
are not inspected. The first self-pair feature row is skipped before inference.

`07bc90` retains undirected first-encounter identities with strictly larger
confidence replacement. The bounded implementation aggregates while emitting
NN rows, preventing repeated proposal occurrences from creating a large
intermediate pair list. Deferred prior pairs precede current strip evidence.

`LegacyUnknownLineSweepSelection` implements ordinary `07be60`/`07c160`
continuation. First-ID sorting happens before the cost table is constructed.
On a non-final strip, a flagged visited pair is skipped; when an unflagged pair
opens an accepted first-ID block, the rest of that block is sorted by second ID
and copied **without repeating the future-mask test**. Confidence sorting then
mutates that same list. `07c160` independently defers any pair with a future
endpoint before interpreting its class. Previous physical groups survive into
the next strip. The final strip processes all pairs. This block behavior is
intentional; replacing it with a simple global mask filter changes native costs.

Successful `07e792` then appends a singleton group for each still-unassigned
physical stroke in ingress order, including flat global Y ranges that have no
NN strip. This is the actual upstream singleton producer. It occurs before
`LegacyUnknownLinePostselection`'s07a850 mean-Y order/adjacent merge; the latter
never invents singleton attachment. Sorted physical IDs then feed the selected
class24a estimate and restored deskew callback.

## Verification and remaining boundaries

512 independent Python continuation controls exercise prepared previous groups,
future endpoint masks, first-ID cost-block behavior, equal-confidence histories,
final/non-final interpretation, pending evidence and physical memberships.
Sixteen additional controls run raw-source filtering/sampling, internal scale
selection, own synthetic30/50 networks, native strip predictions, tiny metadata,
continuation, zero-strip/singleton fallback and final postselection. They check
both scalar choices and actual numerical NN outputs against an independent
NumPy model/feature reader. No vendor assets are needed for these controls.
Malformed ownership/configuration and precancellation are checked. Managed and
Windows Native AOT pass the same controls.

The optional committed-resource diagnostic runs raw own printed MIR through
source scale, real30/50 pair networks, full strips, groups/singletons/postselection,
Gaussian/deskew frames, Russian four-family glyph classifier/final cache and
bounded dictionary search. The previous fifteen own word/frame goldens remain
best `мир` at cost54456. Three additional far-separated rows enter six vertical
strips, giving nine more word/frame checks across the three original geometry
profiles; every expected word remains best `мир`. With large Y translations,
dense Gaussian/deskew float arithmetic changes some diagnostic costs (53582
instead of54456); no bitwise translation invariance is claimed. The existing
small-translation goldens remain unchanged.

This is one own printed fixture plus independent numerical controls, not a
held-out corpus, joined Russian handwriting quality or final UI acceptance.
Source configuration/property dispatch, input record flag binding, known/locked,
guideline/custom/incremental branches, source-driven selection of the geometry
profile and2e2/2e3 provider, native word winner/confidence and production AI-pen
integration remain separate. No native vendor DLL or GUI is executed.

```powershell
python tools/LegacyLineSweep.Probe/reference_controls.py D:/BlurMask-checks/sweep.json
dotnet run --project tools/LegacyLineSweep.Probe -c Release -- D:/BlurMask-checks/sweep.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res
```

Only the controls argument runs vendor-free; the generator creates its own two
network files beside the JSON so those paths remain valid during the probe.
