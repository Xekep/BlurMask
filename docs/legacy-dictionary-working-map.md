# Prepared dictionary working map and real neural diagnostic

`LegacyPreparedDictionaryWorkingMap` restores the bounded, explicit map-refresh
and child-transition gate of Text type `2b7`. It keeps native producer inputs
explicit and does not infer a language permission policy from Unicode scripts.

## Static evidence

The type `2b7` descriptor is `Text1802b42f0`, with vtable `1802b4360`.
Its slot `+68` is `180093f80`. That function copies preserved map `source+58`
to working map `source+50`, then filters each entry below the acoustic symbol
count (`source+74`) using the supplied permission byte. Header category
`(source+20 & 0x1e00) == 0x800` requires both bits `0x01` and `0x04`; all other
categories require `0x01`. A rejected entry becomes `0xffff`. Control values
at or above the acoustic count are unchanged. Every refresh rebuilds from the
preserved map, so subsequent permissions can restore previously removed IDs.

Vtable `+40/+48` are `093f40/093f50`, forwarding ordinary/auxiliary selection
to `093b50`. Ordinary enumeration uses working `+50`, except query byte `+36`
bit `0x40` selects preserved `+58`; auxiliary enumeration selects `+60`.
The dictionary child interval comes from `091ce0`. The materializers
`0920a0`, `0922f0`, and `092540` suppress `0xffff` mappings and, for auxiliary
enumeration only, suppress nonterminal edges. Their low output flags encode
root-origin `1` and terminal `2`. The port delegates native packed edge
decoding to the independently restored `LegacyPackedLexicon`, retaining
edge order and exact next-node identity.

The port uses immutable owned map snapshots instead of native mutable pointers.
It bounds alphabets to 65,535 labels and immediate dictionary fanout to 512,
rejects absent auxiliary data, and propagates cancellation. The separate
[prepared dictionary query source](legacy-prepared-dictionary-query.md) now
restores the eight-slot ring, single-packed pagination and32-byte records,
including the additional root-child skip. This lower-level map API does not
implement those query operations, native context allocation or word decoding.

`MapAcousticCosts` is an **original diagnostic adapter**: it uses the selected
map to route explicit integer acoustic costs to lexical IDs. It adds no case,
related-form or script aliases. It has no dictionary state and therefore does
not perform the auxiliary terminal-edge gate; that gate belongs to
`GetTransitions`. It must not be presented as native candidate scoring/search.

## Reproducible controls

```powershell
python tools/LegacyDictionaryWorkingMap.Probe/reference_controls.py D:/working-map-controls.json
dotnet run --project tools/LegacyDictionaryWorkingMap.Probe -c Release -- D:/working-map-controls.json
```

384 independent set-based Python controls cover category-specific required
bits, map selection, special tokens, restore-after-restrict, and owned copies.
A small original packed dictionary independently exercises real edge decoding,
terminal gating, root/nonroot flags, edge order and preserved-map override.
Malformed input, cancellation and missing auxiliary data are rejected.

Optional actual-model diagnostic:

```powershell
dotnet run --project tools/LegacyDictionaryWorkingMap.Probe -c Release -- D:/working-map-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
```

This loads the pinned Russian bundle and tests 133 actual packed root edges.
Two original stroke fixtures pass through actual neural glyph/context/cut
inference, the restored selected map, the existing original variable word beam
and native terminal language-cost evaluator. Each fixture runs with all exact
labels and a separately explicit exclusion of every Latin lowercase label.
The exclusion changes permissions only; Cyrillic IDs are never substituted.
Every retained terminal is validated against the actual word index and every
retained label against the selected map.

Managed and Windows NativeAOT controls passed. Native diagnostic timings on
2026-10-12 were approximately 30–39 ms per fixture/scope; these are small warmed
diagnostics, not an application performance guarantee. The printed fixture's
top five candidates were `то`, `ты`, `мы`, `ж`, `но`; the joined zigzag's were
`ты`, `мы`, `мм`, `ни`, `шт`. The explicit Latin restriction did not change
these leading candidates. Expected words are not used during inference and
these results do **not** establish Russian handwriting accuracy.

Source descriptor category `0xc00`, baseline `90`, height `80`, source
probability `0`, and language scale `1` are explicit diagnostic inputs. The actual
descriptor and permission producer are now separate
[checked components](legacy-acoustic-dictionary-permissions.md). Automatic frames
and the [ordinary neural line sweep](legacy-ordinary-neural-line-sweep.md) are also
separate components. Final native beam/winner/confidence and complete ordinary
application orchestration remain unfinished.
