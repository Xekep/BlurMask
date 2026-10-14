# Acoustic confidence prototype indices

`LegacyAcousticConfidenceLabelMap` restores the insert-only index map built in
Text1B0760 and consumed in1BCBD0 before the neural D8 probability getter. It
changes the index used to calculate a candidate's confidence; its output
symbol, script, case and integer acoustic cost stay separate.

For each positive-weight knowledge group in serialized order, the native
producer finds its first member outside the extended interval
`[networkWidth,alphabetCount)`. Checked resource members lie in the alphabet,
so that member is a neural label. Every extended member maps to that first
neural label.09E020 reports whether a key was newly inserted; only a new key
receives a target. Earlier groups therefore win. This is a one-step mapping,
not recursive alias resolution.

The dash-prototype fallback then inserts `-`, fullwidth hyphen, `_`, fullwidth
underscore, soft hyphen, prolonged sound mark, CJK one and two private
primitives, in native order.1B0760 clears the prolonged-sound-mark fallback
when its label is already inside the neural width. Finally, two extra labels
map to apostrophe and quotation mark. A missing quote target stays FFFF and
cannot become a neural probability.

The actual first6120 source has192 alphabet labels and180 neural outputs.
Its checked v110 profile has zero knowledge groups and no alphabet wide pairs.
The resulting confidence-only mappings are:

| Output label | Neural index | Meaning |
| --- | --- | --- |
|185|140|hyphen|
|186|140|underscore|
|183|140|soft hyphen|
|187,188|140|private primitives F0002/F0003|
|193|134|extra apostrophe|
|194|129|extra quotation mark|

Soft hyphen183 is outside180; it is not an internally remapped neural label.
The previous72 real-group fixtures have none of these keys in their retained
top20, so their12,960 score-bit goldens remain unchanged.504 additional
controlled retained-candidate checks reuse those actual neural channels and
verify every mapping with an independent Python scalar reference. Those
checks validate index transport, not handwriting classification accuracy.
512 independent synthetic maps additionally cover group order, first-wins
conflicts, one-step chains, inside-width prolonged marks and label bounds.

The existing `LegacyFirstOrdinaryGlyphScoreProducer.ReadSymbolScore` now uses
this actual source-owned map before neural lookup. The selected output symbol
is preserved. The closed primitive F0001 has no stored prototype in this
source: native1BCBD0 calls graph21B sampler90 to obtain the group range-vector,
then source218 virtual60 on its first physical stroke. That getter reads the
source enclosure probability directly. The source-aware bridge is a separate
stage; this index map has no source geometry owner. Other resources can require the separate
alphabet80/88 wide-pair path. EXP0EBD30 has no bounds guard, so the portable
producer refuses an unresolved retained label rather than reading outside
the neural output or assigning a guessed score. This phase supplies no
Latin/Cyrillic substitutions and does not enable Russian recognition in UI.

```powershell
python tools/LegacyConfidenceLabelMap.Probe/reference_controls.py D:/confidence-map-controls.json
dotnet run --project tools/LegacyConfidenceLabelMap.Probe -c Release -- D:/confidence-map-controls.json models/ru-RU/ru_RU-ak-cur.lite.res
python tools/handwriting/audit_acoustic_confidence_label_map.py --output D:/confidence-map-static.json
```
