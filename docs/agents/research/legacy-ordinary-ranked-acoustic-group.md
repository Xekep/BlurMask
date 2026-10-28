# Ranked ordinary group consumed by the first acoustic-only controller

`LegacyOrdinaryRankedAcousticGroup` implements the bounded ordinary21d
virtual88/1c13a0 query and1be530 top20 cost-normalized likelihood. The supplied
cache must already represent the caller's corrected/pruned native shortlist;
this component does not invent a confidence threshold or run a word winner.
It is reusable with either model when the caller proves the applicable branch.

## Producer and consumer proof

The inspected static DLL has SHA256
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
First no-linguistic-source controller23f initializes in1edde0: it checks the
frontend's virtual1C0 presence and stores that result in its byte28. Class222
descriptor2de898 resolves through info2de5d8 to runtime table2de610; virtual1C0
is152f90. Its raw instructions forward to frontend78 ordinary21d virtual88.
That runtime table2e4180 slot88 is1c13a0, so the relevant1ee3b0 path queries the
**ranked group record**, not just the underlying195-element class-cost vector.

Offsets and native addresses below are hexadecimal. The ordinary cached group
is0x38 bytes. Its fields20/28 address an ordered
24-byte shortlist and its count. An entry stores normalized likelihood at0,
cost at4 and acoustic label at8. The group fallback cost is18; native scale is
float1C; geometry/category costs are an int array at10.

After1bc680 constructs and signed-cost sorts this list,1bcbd0 writes calibrated
class channels separately,1be1a0 optionally prunes them and1be530 computes the
stored cost likelihood. These are distinct values and mechanisms. Constructor
1b0330 writes confidence threshold350=-1 and size penalty354=0; no initializer
write to350 was found. The fresh diagnostic therefore retains the existing
shortlist. This constructor observation does not authorize external/reused
controller configurations. Their pruning remains a caller precondition.

## Literal query and likelihood behavior

1c13a0 requests use symbol at8, shortlist ordinal at6 (FFFF means label search),
signed category atA, top20 restriction atB and primitive-cost flag atC.

* Primitive cost is trunc(group.scale*40000).
* FFFF ordinal with symbol equal to acoustic alphabet count selects the group
  fallback. It bypasses the ordinal/top20 test but still reaches geometry.
* Otherwise lookup or supplied ordinal selects entry cost. A supplied ordinal
  deliberately does not check whether entry.label equals the requested symbol.
  Missing/out-of-list ordinals and a top20-restricted ordinal above19 return
  impossible04000000. Finite signed negative costs are retained.
* Geometry is added except for the five exact private primitive labels
  U+F0001/2/3/4/8 and the source alphabet header fields4/5/6. The direct ordinary
  virtual58/1c0fd0 API bypasses only the five named primitives; its policy must
  not silently replace this first-controller virtual88 branch.

1be530 writes likelihood1 for a one-entry list. Otherwise, at most the first20
entries receive float exp(-(float)cost/(scale*8192)), accumulated in order in a
float sum and divided by that sum. Later entries receive0. There is no invented
max-cost stabilization. This bounded implementation rejects zero/overflow sums
outside the valid finite arithmetic branch. Scale is explicit: it is not the
calibration slope, glyph size or acceptance confidence.

First controller1ef070 reads frontend virtual150/1529a0, forwarding to ordinary
virtual78/1c1260. That accessor finds the requested label and returns its entry0
likelihood (or0). The controller then stores trunc(likelihood*255), clipped to
1..255. `GetFirstControllerLikelihoodByte` preserves that lower bound even for
missing symbols. This byte is not evidence that a transcription is accepted.

## Checks and actual composition

`tools/LegacyOrdinaryRankedGroup.Probe/reference_controls.py` generates512
independent scalar float references for ordered lists of0..30 signed costs and
five scales. The portable probe checks likelihoods, byte values, positional and
label access, top20 limits, explicit ordinal/symbol disagreement, missing
labels, primitive cost and fallback geometry. Constructed source-header bypass
controls preserve a negative class cost without reading a category; malformed
duplicate/unsorted lists, invalid scale and cancellation are rejected.

The actual-resource mode joins `LegacyFreshFirstGlyphSession` raw row/24a/265/
297/context/NN/cache with the **first** sampler297@11506 category profile201 at
11571, its resource separation fields and the published engine separation
table. Disabled-power first profile has group scale1. On own printed M/И/Р:

| Glyph fixture | Leading cache entry | Separation | Likelihood | Stored byte |
| --- | --- | --- | --- | --- |
| M | Latin M6172 | 0 | .5686 | 144 |
| И | Cyrillic И16233 | 7100 | .5114 | 130 |
| Р | Latin p14405 | 0 | .2382 | 60 |

The real first ranked-query costs agree with existing direct ordinary scoring
on each fixture's shared nonspecial branch. This confirms the consumer bridge,
not word accuracy. Masks, pairs and candidate ranges are still explicit; the
Latin/Cyrillic mistakes remain visible. Full23f sequence search, final result,
confidence and representative joined Russian corpus validation remain open.

```powershell
python tools/LegacyAcousticCache.Probe/reference_controls.py cache-controls.json
python tools/LegacyOrdinaryRankedGroup.Probe/reference_controls.py ranked-controls.json
dotnet run --project tools/LegacyOrdinaryRankedGroup.Probe -c Release -- ranked-controls.json cache-controls.json
dotnet run --project tools/LegacyOrdinaryRankedGroup.Probe -c Release -- ranked-controls.json cache-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```
