# Owned single-glyph main sampler ingress

`LegacyFreshSingleGlyphMainSampler` continues the owned single-physical,
single-source720 endpoint after loaded249,24B,assigned24A,zero-gap24D/29B and265.
It uses the selected **main**247 sampler/category/graph/acoustic resources,
not the first6120 frame or an explicit example baseline. It accepts the actual
first selected rich275 word and graph range owner from its own first factory.
It does not select a main word winner or make Russian handwriting production-ready.

The committed RU main owns sampler188157, categories188222, graph189379 and
acoustic21D189734. The first selected child uses different owners even where
scalar sampler options happen to match. The checked cut prefix is validated
before using the restored Russian106 stage interpreter. Altered settings are
rejected, rather than silently ignored. Main21D's stored FF/FF normalization
and positive target height are read from that exact adjacent acoustic node.

## Native producer and ownership

* Reader196500 **version below107** writes134=0 and optional network138=NULL.
  The three earlier serialized prefix bytes are not this flag. Consequently
  18D920 disables242, and192C80 returns success without evaluating a closure
  network. Reader's source-name1D0 extension is also absent before108.
* Clone18D590 calls18D400 and copies only configuration8..1A7. Runtime244 lies
  outside that copied area and remains constructor zero.18D920 does not write
  it. This restores the ordinary cut branch, rather than guessing the boolean
  `wholeStrokeOnly` from a drawing's shape.
*193380 reads the actual source descriptor, current source218 point buffer and
  guide **0/4** baseline/height. Physical record+C probability survives the
  postprocessor transforms;2AD is not rerun over resampled points.
*193380 invokes191950, then source218.virtual30=168B40. Assembly168B40 forwards
  into16AA00 with map factor1 and the caller's **null order**. Cut insertions
  therefore replace current source points and compose the previous265 factor200
  map. They do not regenerate original point identities or remap source720
  publication boxes/ranges. The API preserves those domains separately.
* Ordinary physical metadata starts with byte1B/1A zero and boundary18 minus1.
  The sealed fresh no-prior postprocessor path preserves them. This source70
  fact follows physical-field ownership; it is not inferred from an empty text
  pool after first recognition.19F850 initializes ordinary group6/7 zero;
  19EF90 receives zero endpoint nibbles; same-descriptor1A3B30 merges preserve
  the ordinary kind. Generated arc flags remain the real producer's flags.
* The graph cap comes from main21B, and the existing exact sampler68 eligibility
  pass removes/renumbers arcs. Candidate source references retain the original
  parent raw metadata through both265 and sampler insertion.

`NormalizeGroup` handles the one-range branch1B4E40: candidate minimumX is
scanned from its actual selected prepared points; baseline/height come from
the main source guide; scale target is the stored main21D value. Floating
operations precede truncation into shorts. The native lower-short wrap branch
remains rejected by the existing bounded normalizer.

`GetSourceLineIdForArc` restores frontend152280: graph68 resolves group,
graph78 resolves descriptor ordinal, source21810 resolves descriptor88, and
**INT32+0x10 is line ID**. It is neither a physical boundary field nor a byte
at decimal10. All valid ordinary singleton groups address descriptor0, whose
24B-assigned line is0. Missing/unknown graph endpoints are outside this API.

## Explicit remaining boundaries

Closed physical strokes are rejected: native191430 can use an upstream
category-dependent limit that this owning endpoint has not restored. No null
limit is silently used for that branch. Multi-source720, multi-physical grouping,
prior/source-property events, special protocols and other sampler versions
remain outside the endpoint.

`CreatePrepared240TransitionGeometry` is explicitly the **prepared ordinary240
caller branch**.150D00 tests the owner's actual controller at+60 for23F;
1A6980/18D920 forward that result into sampler210. For240 it is zero and the
main gap/matrix scales apply;23F uses another branch. This helper transports
the owned source facts into that scoped consumer, but does not prove live
controller240 creation/selection. It must not be cited as full orchestration.
Source secondary150 is a separate score-mode boolean, not the line geometry.

Main acoustic family gates, main context/cache ownership and final language
winner/confidence are further integration stages. No source guide height is
relabeled as modern Document font x-height, and no fixture label corrects the
first inference output.

## Verification

The headless probe uses actual first model inference and its selected275 output,
including the resource-owned first cap7 sampler prune pass. Its selected result
produces one genuine720 row, baseline90/height40,31 points after265, and the
main cap8 graph with10 groups. Independent Python executes the complete191950
stage sequence, then verifies each prepared row,200 map composition and166
normalized points across those10 groups. The same probe checks main model
ownership, source line IDs, malformed scalar prefix, cancellation and invalid
group access. The fixture's raw height80 is not substituted for guide height40.

```powershell
dotnet run --project tools/LegacyFreshMainSampler.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json D:/main-sampler.json
python tools/LegacyFreshMainSampler.Probe/audit_output.py D:/main-sampler.json
python tools/handwriting/audit_fresh_single_glyph_main_sampler.py --output D:/main-sampler-static.json
```

The static auditor verifies pinned PE/resource hashes, interface slot bindings,
version/clone policy, source-map factor and the152280 line-field read. It reads
the proprietary binary as data; no vendor DLL is executed.
