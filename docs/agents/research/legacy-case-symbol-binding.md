# Case-aware dictionary symbol and permission binding

`LegacyPreparedCaseDictionarySymbolBinding` connects the already prepared292
symbol and permission phases to the same initial2B7 dictionary maps. The initial
maps now retain their acoustic/lexical alphabet owners; case preparation must
reference that exact acoustic object. `LegacyInkAlphabet.CopySerializedHeader`
returns an independent snapshot of its eight actual serialized fields.

Enumeration runs lexical labels through preserved58 exactly once, then through
the232C80 case callback. The emitted source contains acoustic/control tokens
with no second dictionary map. Native callback order and duplicates remain
observable. This source can feed the existing linguistic mask controller.

Permission refresh runs the ordered234840 byte propagation before093F80. Each
new working50 map starts from preserved58, so earlier filtering cannot suppress
later refreshes. The acoustic-permission entry composes1C01B0 with this chain;
failed preparation forwards neither partial permissions nor a working map.
Both required-bit1 and category800 required-bits1|4 branches are exercised.

The real Russian AL/LK fixture emits1083 callbacks. An independent raw-payload
oracle verifies their complete order and controller seed,416 permission/map
refreshes, and24 acoustic permission cases including failures and graph-kind
state. Tests check owner identity, cancellation and header snapshot isolation.
Managed and Native AOT run the same fixture. The static audit pins DLL bytes
and the preserved58→case callback→permission→working50 linkage.

The binding assumes the caller's owning source factory already selected292.
It does not invent a container case setting, change live query epochs, bind
the application renderer or establish Russian cursive recognition quality.
