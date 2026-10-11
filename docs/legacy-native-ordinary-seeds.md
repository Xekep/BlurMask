# Native ordinary seed producer and recognition bridge

This component restores the ordinary left-to-right geometry branch of Text
`18019f850`. It consumes the owned prepared ink and atoms produced by the
existing Russian sampler297.version106 cut controller. It preserves the source
atom identity and prepared range of every seed. It also records deferred seed
insertion slots and displacement; it does not silently turn deferred input
into a linear candidate graph.

`LegacyRussianCursiveWordRecognizer.RecognizeNondeferredNativeSeedsWithFinalCache`
connects this producer to the restored ordinary chain/lattice, chronological
range normalization, four neural families/context, native195-entry final cache,
35-category geometry, native acoustic scoring and resource lexical totals.
The supplied line frame, source flags and acoustic source-scope callback remain
explicit. Context framing and the bounded dictionary beam remain original
orchestration. This endpoint is a diagnostic producer/consumer bridge; it does
not claim full SDK recognition, native terminal selection or acceptance
confidence.

## Proven resource and source dispatch

The observed acoustic resource contains a complete type21b.version0x10a node at
byte189379. Its355-byte serialized node starts with the native eight-atom span
cap. Two6x6 scalar tables follow the first two payload bytes. The geometry
flags at node-relative306..308 are `1,1,0`: reorder enabled, insertion shift1,
optional alternate geometry branch disabled. Relative329 stores the backward
height tolerance0.3; relative345 stores the insertion fraction0.6. The relevant
position adjustments at333,337,341 are allzero. `1801ad9d0` is the reader for
these runtime fields. The reader here checks the complete node envelope and
rejects unsupported flag/adjustment variants.

The sampler297.version106 reader `180196500` leaves its newer serialized
reorder-network selector zero. Its initializer `18018d920` and virtualE0 getter
`1801964a0` therefore expose a false reorder-network flag. The ordinary raw
source descriptor virtual10 getter is `180168ad0`. The input descriptor's
byte48 is initialized to1 by `180167210`, unless a custom source provider
supplies an override. The actual ordinary automatic provider2e3.virtual70 is
`18004a2c0`, an assembly `mov al,1; ret`. This is the left-to-right geometry
axis selector; it is a different field from the source service enable flag.

The physical source virtual70 getter `180168c40` reads record byte1b bit0.
New records initialize it to0, but the directional postprocessor can change
it. VirtualF0 exposes the physical boundary ID: source initializer `180163970`
initializes its source value to−1; `180167210` copies that value into the
physical record; `180167ce0` can override it. This component therefore requires
the caller's actual direction flags and boundary IDs, checks every direction
flag is0 and every boundary ID is−1, and rejects unsupported source scopes.
It does not infer these values from stroke IDs or Unicode script.

## Native geometry and lineage

For an ordinary single-range atom, `19f850` obtains both the atom's prepared
bounds and its entire physical stroke's bounds. With the checked zero height
adjustments, the reorder comparison uses the **whole physical stroke maximumX**,
not the atom maximumX. The preceding accepted-geometry anchor is the maximum
atom maximumX observed sofar. A seed is deferred only when the whole-stroke
maximumX is strictly less than `anchor − height×0.3`. Equality stays ordinary.
The shared cut endpoint remains owned by both neighboring atoms, as produced
by `1935e0`; coordinates and point maps are never reordered or replaced.

Deferred placement uses the weighted atom position
`(1−fraction)×minX + fraction×maxX`, with native float operation ordering.
Candidate insertion slots are the leading exterior, gaps between neighboring
ordinary atom boxes, and trailing exterior. Overlapping gaps collapse to the
rounded midpoint. The first slot with the strictly smallest distance wins.
The insertion-slot array is **integer metadata**: assembly at`1801a131c` adds
the selected four-byte slot directly to the insertion shift, despite the
decompiler's misleading float-pointer declaration.

Insertion visits the greatest original slot first. For equal slots, ordinary
LTR seeds with the greatest weighted position are inserted first; strict
coordinate ties preserve the first selected encounter. The actual index is
clamped to the original ordinary count. The one-byte insertion displacement
is `originalSlot − actualIndex`, confirmed at`1801a1412..1801a1419`. The returned
snapshot retains original atom IDs, deferred flags, slots, displacements and
prepared atom bounds. It does not invent timestamps or physical source IDs.

## Why deferred input is not passed into recognition

`1801a1ef0` expands deferred seeds into additional seed arcs and skip paths,
including neighboring placement alternatives. Later source graph mutations
operate on those paths. A simple chain of the reordered snapshot would erase
those alternatives and change native path ownership. `CreateNondeferredOrdinaryChain`
and the new recognition endpoint therefore reject deferred input before
classification. Existing source-order diagnostic APIs retain their original
behavior; this new endpoint is separately named and does not replace them.

Still missing: deferred seed-arc expansion, its graph mutation/join consumers,
automatic complete source direction/scope production, native terminal winner
and posterior policy, and calibrated Russian cursive accuracy. Reordering the
seed snapshot alone does not close those gaps.

## Independent verification

The original Python oracle models insertion as distance to closed intervals,
then applies the separately reconstructed descending-slot insertion ordering.
It executes no vendor code.771 independent controls cover ordered/deferred
ink, multiple atoms per physical stroke, overlapping boxes, exact ties, strict
threshold boundaries and disabled reorder.531 cases deliberately exercise
deferred input and verify refusal of incorrect chain expansion.

Real-resource checks connect native nondeferred seeds through the entire
existing final-cache word pipeline: an owned printed Russian MIR fixture
scores28 groups; an owned joined zigzag fixture scores44. These are diagnostic
transport fixtures, not an accuracy corpus. MIR's expected dictionary word is
still fourth (`мир:55698`), behind `то`, `мне`, `мы`; this is evidence that the
Russian app recognition target is unfinished. A real late cross-stroke
activates deferred placement and refuses before any neural classification.

```powershell
python tools/LegacyNativeSeed.Probe/reference_controls.py .artifacts/legacy-native-seed-controls.json
dotnet run --project tools/LegacyNativeSeed.Probe -c Release -- .artifacts/legacy-native-seed-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

Managed and Windows Native AOT run the same independent controls and actual
resources. The existing variable-word probe also remains a regression check
for the preserved source-order endpoints.
