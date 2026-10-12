# Fresh ordinary raw packet and descriptor identity

`LegacyFreshOrdinaryRawPacket` restores the **fresh, unscoped, direct float
input branch** of source219/167210. It constructs descriptor ownership from
raw physical strokes, rather than accepting a caller-made descriptor map.
Explicit semantic packet events164a10, custom callbacks, opaque inputs,
begin/end tags and reused source state are outside this entry point.

Static provenance is `MyScriptText.dll` SHA256
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
The DLL is inspected, never run. Resource/model data is supplied separately.

## Source producer

Constructor163970 initializes scope field88 to-1, signed scope counter8c to0,
pending descriptor flag8e to0, physical chronology ordinal90 to0, line fields
94/98 to-1, source identifiera0 to`int.MinValue`, auxiliarya4 and boundarya8
to-1, and endpoint tagsac/b0 to0.

For the checked resource247/v10d, reader1d4950 clears resource78..230 before
loading the source parameters. Reader1d41e0 does not write init bytes68..6a;
constructor163970 copies these zero values to flags83/85/86. This proves the
fresh branch is not initially paused. `PrepareFirstSource` validates the
first model/source ownership and uses the same ordinary ingress reader,
duplicate suppression, source probabilities and source chronology as the
existing raw input component.

167210 appends a new58-byte descriptor when there is no previous descriptor,
pending8e is set, or the previous descriptor's low kind nibble is neither0 nor6.
159810 clears all88 descriptor bytes. For this fresh direct API, the first
stroke creates ordinary kind0 with flagbit4. It clears pending8e. Each later
ordinary physical stroke extends the same descriptor: start physical ordinal
is preserved, last ordinal advances. Consequently a fresh unscoped packet
containingN pen-up-separated strokes produces **one descriptor**, spanning
physical0..N-1. No semantic BeginPacket event is manufactured.

Defined fields include the initial source/line identifiers, first raw X/Y,
horizontal direction1 without a custom direction callback, and four absent
endpoint categoriesFFFF. Physical record end-tag byte1a receives zero nibbles
from sourceac/b0. Line IDs remain-1 at this raw stage.

`LegacyFreshOrdinaryPacketFrame` continues this same ingress object through
the existing class24a/16e570/265 row preparation via `PrepareOwnedSource`.
Native16e850 visits ordinary kind0/6 descriptors with current line10<0,
appends a40-byte fitted frame, writes descriptor10=frameCount-1, invokes
callback169750 and deskews its physical range. With one fresh descriptor and
an empty frame collection, this yields line0. The framed result retains both
the raw descriptor (line-1) and its updated frame descriptor (line0). This
assignment follows the fresh allocation sequence; it does not provide a
general default for reused/page input. The existing `Prepare` entry point
delegates to the same core and retains its previous geometry/key behavior.

This is the native direct-ingress branch selected by the API, not a claim that
every SDK invocation or GUI event sequence uses one descriptor. Explicit
164a10 events can create new/special descriptors. Their inbound event adapter
and reused source lifecycle still need restoration before general UI use.

## Why physical IDs are different

The readonly source interface218 `virtual10` is168ad0:
`secondary18 + descriptorOrdinal*58`. Its physical stroke records instead
live at secondary58 with stride1c. The two collections must not be conflated.

Native19f850 receives descriptor ordinalparam3, obtains that descriptor through
virtual10 and writes the ordinal into geometry group+00. Its range list carries
physical stroke IDs separately. Graph21b.virtual78/1a7920 returns group+00.
The group descriptor pointer later controls1a7f20/1a8670 continuation branches.

The new component therefore resolves all owned physical members/groups to
descriptor0 in this fresh branch, while source chronology's second component
still increases for every physical pen lift. The range accessor validates
physical membership; resampled point bounds and coordinate ownership remain
the responsibility of the prepared-geometry producer. It does not substitute
raw geometry for265/297 coordinates or normalization ordering.

## Category-cost producer proof

A separate earlier gap is now closed statically. Ordinary1be930 ends by calling
graph21b.virtual1b8 with the final group's category-cost array, node and arc.
1ad9b0 forwards to sampler297.virtualc8, which resolves to195520. That producer
computes native193860 separation cost, clips it to sampler118, and repeats the
same integer across the profile's composite-category count.

Thus the existing first ranked-group checks using uniform native separation
costs represent this ordinary producer; they are not an invented seven-family
category estimate. This does **not** complete the additional118/140 pair and
transition updates or the three later text passes/final winner. In particular,
first297@11506 has geometry-enable field50=1; the later sampler's field50=0
cannot be substituted to disable its pair correction.

## Checks

```powershell
python tools/LegacyRawPacket.Probe/reference_controls.py <controls.json>
dotnet run --project tools/LegacyRawPacket.Probe -- <controls.json>
```

384 own append-event fixtures check every input prefix, including repeated/raw
duplicate points. They verify11,648 physical ownership cases, descriptor range
extension, first-point retention, constructor identifiers and independent
pen-up chronology. No vendor binary or weights are used for these controls.

Optional resource check:

```powershell
LegacyRawPacket.Probe.exe <controls.json> models/ru-RU/ru_RU-ak-cur.lite.res
```

Our five-stroke printedMIR fixture passes resource ownership and actual raw
ingress/service probabilities,24a/265/297 preparation, shift-zero seed graph
and35 candidate groups. The groups preserve fresh descriptor0/line0, and
physical chronology still has five separate ordinals. The frame shares the
packet's original ingress object and checks old row geometry/key equivalence,
foreign profile rejection and cancellation. This is a producer/consumer
diagnostic, without a handwriting-accuracy assertion or final text winner.
