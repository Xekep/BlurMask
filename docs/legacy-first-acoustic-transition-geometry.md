# First acoustic ordinary transition geometry

This implements the fresh, single raw input descriptor / line0 ordinary branch
between first class23f acoustic records. It does not substitute class240 or the
second RU directional model. It does not provide the final text winner, the
three later A8/B0/B8 text-context passes, cross-descriptor/special/primitive
transitions or a Russian handwriting accuracy claim.

## Ownership and native dispatch

`LegacyFirstTransitionGeometryProfile.ReadSource` first validates the owned
247 source and first no-dynamic model. It then reads the sibling sampler297/v106
and its discrete category201 prototypes. For the committed RU resource, source
247 starts at6120, sampler297 at11506 and category201 at11571. The tail starts
at12513 and ends exactly at12727; the main sampler188157 is a different profile
and must not be substituted.

First resource tail: enabled pair geometry1, scale1, offset0.5, maximum1.5,
default gap1.5, translation threshold0.4, gap slope3. Its36 category margins
and two integer caps are read from the resource, not guessed. Initial class23f
forwarding makes sampler210=1. Fresh source secondary9c is−1, so the resource
gap and unit margin scale apply. Version106 has no extra1d0 network; fresh242=0
leaves custom boundary probability adjustments inactive.

The source descriptor is distinct from physical stroke chronology. Fresh
`LegacyFreshOrdinaryRawPacket` coalesces the ordinary physical strokes into
descriptor0; `LegacyFreshOrdinaryPacketFrame` assigns descriptor line0. Native
19f850 stores this descriptor ordinal in group+00. Compact group+05 is copied
from wide+52 by19c4e0 and contains endpoint nibbles; fresh sourceac/b0 are zero.
Compact group+07 copies wide+07, which is zero for ordinary input. These facts
close the same-descriptor ordinary branch instead of equating descriptor IDs
with physical stroke indices.

Primary recurrence1ee3b0 applies frontend118 then140 for continuation kinds0
and2. The former dispatches1525e0→1ad890→sampler195c40. The latter dispatches
1528c0→1acfa0→sampler195b30 while graph1c8 returnsfalse and fresh graph1dc=0.
The current and previous glyphs use their actual prepared group bounds and
resource category prototype indices. Special labels and omitted branches are
rejected explicitly, rather than receiving zero geometry corrections.

Frontend1525e0 first checks graph188=1a7d10 for kind2: every ordinary physical
range must begin at point0. An internal cut with nonzero start is rejected for
plain predecessors. Its exception for an eligible primitive predecessor is
outside this API. This wrapper guard precedes sampler geometry, so calling
195c40 alone is not equivalent to the frontend callback.

## Cache preparation and arithmetic

`LegacyFirstOrdinaryTransitionGeometry` folds native1941b0 backwards over
immediate graph50/58 successor lists to obtain future minimumX. It then folds
compatible previous arcs forwards for prefix maximumX. Roots retain their own
maximumX and impossible transition caches. Other arcs use their normalized
gap to fill join and6×6 category caches. The last category row subtracts1;
the last column has the native uncapped behavior. The separate3× gap and
kind5 caches are not used by this bounded kinds0/2 API.

1941b0 uses the same pinned256-float table as the existing separation profile,
native VA1802e1090, float-byte SHA256
`b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf`.
Lookup adds4 as float, multiplies255 and adds0.5 as double, truncates and
divides by8. Values above4 use zero; below−4 use the linear tail. Join uses
24d8a0 bit rounding; matrix uses signed nearest rounding via float±0.5 then
truncation. Native195c40 truncates the translation term **before** multiplying
8192. Native195b30 can contribute negative pair costs (resource offset−4096);
these are preserved. Arithmetic outside the checked signed integer domain is
rejected, rather than silently wrapping.

The resource category costs entering ranked acoustic queries are not guessed
seven-family weights: graph1b8→1ad9b0→samplerC8=195520 computes193860 separation,
clips sampler118 and repeats this cost across the composite-category entries.

## Verification

Vendor-free Python controls independently calculate1024 prototype residuals
(172 negative cases) and1024 table/edge/tail cases for both rounding paths.
Managed and Native AOT pass. The actual diagnostic uses three original printed
MIR fixtures at scales1,1.75 and0.7 with translations. Each fixture scores all35
first-NN variable groups, reaches3050 callback118 calls and27 nonroot primary
traces. Its captured graph inputs independently reproduce105 prefix/suffix
arc states and15720 callback118/140 values using Python scalar arithmetic,
including2580 kind2 rejections for nonzero physical starts.
Malformed profile, ownership, cancellation, cumulative acoustic versus total
costs and native raw-source/frame assignment are checked.

Best **primary diagnostic** paths are mixed-script `MИP`, with costs41268,
41140 and42035. This is not a final Russian result: the later text-context
passes and terminal273 selection remain unresolved. These are synthetic
printed controls, not a representative cursive corpus or UI support.

```powershell
python tools/LegacyFirstTransition.Probe/reference_controls.py D:/controls.json
dotnet run --project tools/LegacyFirstTransition.Probe -c Release -- D:/controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json D:/actual.json
python tools/LegacyFirstTransition.Probe/reference_controls.py --audit D:/actual.json
```

The final optional file is a private diagnostic capture of actual prepared
inputs and outputs. The implementation neither loads nor executes vendor DLLs.
