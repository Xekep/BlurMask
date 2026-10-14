# Native scalar case context and row selection

`LegacyNativeCaseContext` owns a nonempty UTF32 sequence and complete eight-byte
records from default Engine `05F8E0`. It reuses the already fingerprinted517
combining-class ranges, which are byte-identical to the table read by that
function. Missing combining-class entries remain?1. Invalid Unicode scalars and
unbounded contexts are rejected before any later consumer can use them.

The native backward writes affect byte4 on a following CCC230, stopping before
CCC0, and byte5 on a following U+0307, stopping before CCC0 or230. These are
literal native conditions, including what happens on missing CCC entries. They
are not replaced with framework casing context rules.

A static proof establishes a less obvious property of bytes6/7: native
initializes them from the FIRST scalar only, clears them on CCC0/230, and never
re-enables them in the scalar loop. U+0049 and every one of46 native soft-dotted
scalars have CCC0 in the exact selected table. Consequently bytes6/7 are zero
for every valid input sequence in this default service. The port uses this
proved simplification instead of adding an unnecessary second data model.
The optional audit verifies both the table and native clearing instructions.

`Select` implements the scalar-choice subpass of `04F170` using the OWNED scalar
and OWNED context record. It keeps original special-row order and opaque
language IDs. A requested nonzero language is used only if that scalar has an
alternative with that language; otherwise language0 is used. Context masks0,
1,2,4,8,16,32 retain the exact native predicates; notably mask1 accepts a clear
request bit1, while mask2 accepts a set request bit2. No selection signals that
the later string writer must retain the original scalar.

The independent Python oracle derives the two backward flags through forward
suffix dependencies rather than repeating the native backward walk. It checks
6,522 entire eight-byte records and130,440 ordered selections across request
flags and language IDs, plus copy isolation, cancellation, invalid scalars and
context bounds. Random mixed sequences include Cyrillic, Greek, Turkish
specials, combining marks, CCC gaps and multi-mark chains.

The normalized lower/upper/title string builders, symbol insertion/lookup,
recursive form consistency checks, alphabet map refresh and stateful292 word
query remain separate missing consumers. These scalar helpers do not establish
Russian handwriting accuracy or complete the application integration.

```powershell
python tools/LegacyNativeCase.Probe/reference_controls.py models/text-unicode/legacy-case-data.json D:/case-scalars.json
python tools/LegacyCaseContext.Probe/reference_controls.py D:/case-scalars.json models/text-unicode/legacy-normalization-data.json D:/case-contexts.json
dotnet run --project tools/LegacyCaseContext.Probe -c Release -- models/text-unicode/legacy-case-data.json models/text-unicode/legacy-normalization-data.json D:/case-scalars.json D:/case-contexts.json
python tools/handwriting/audit_legacy_case_context.py --output D:/case-context-static.json
```
