# Prepared symbol categories to C1A line features

`LegacyPreparedCharacterLineCategoryCatalog` and its stateful item producer
restore the bounded Document **04efe0 -> 0531b0 -> Ink021110** input bridge.
Catalog entries, actual alphabet strings, resolved symbol IDs and symbol source
bounds are explicit prepared inputs. This stage does not recognize handwriting
or split a connected word into character regions.

## Exact producer evidence

The inspected MyScriptDocument PE SHA-256 is
`17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f`.
Private read-only exports are under
`D:/BlurMask-build-artifacts-20261011/DocumentCategory98StoresNext/` and
`DocumentPreparedCharacterProfilesNext/`. No vendor code is executed.

Document **04efe0** obtains the actual recognition resource's alphabet from
resource+1D8 and retains it as driver+A8. It creates a text-key hash catalog from
**3923** fixed 16-byte entries at **244c50**. Each entry is a UTF8 text pointer
and two signed category bytes; existing-key visits overwrite those bytes. It
iterates alphabet symbols through virtual30, performs exact text lookup, and
writes parallel signed-byte arrays at driver+98/+A0. Missing text maps to
`(-1,-1)`. Neither case folding nor Latin/Cyrillic aliases are introduced.

The library takes an explicit catalog and alphabet snapshot. `BindAlphabet`
performs the same exact lookup by valid Unicode string, with explicit finite
size limits and strict UTF8 validation. Native allocation, hash bucket identity,
failure side effects, resource preparation and alphabet virtual calls are
outside the owned portable value bridge.

Document **0531b0** resolves each optional source item's text through **022d30**
and driver+A8. The resulting alphabet ID selects the two category bytes. FFFF
is the unresolved ID sentinel. Source bounds come from the field/ink service's
virtual178 for that item's actual source identifiers; the port does not replace
those bounds with a whole connected-word box.

Boundary categories index a **45-row**, 12-byte float table at **244a30**.
The first two floats are baseline offset and vertical span; the reserved third
float is not consumed. Given row `(offset,span)`, observed binary32 operations
produce `bottom=-1-offset*2`, then `top=span*2+bottom`. Known main category gives
kind1; an alternate gives kind2. Main=-1 or unresolved ID gives kind0.

The copied 584-byte item has exactly one candidate, base weight1/coefficient0,
and three false item flags. These fields are the actual template initialized
by 0531b0, rather than guessed acoustic weights or recognition confidence.

## Template state is preserved

Native 0531b0 zeroes the complete template **once**, before iterating source
items. Missing category/ID resets kind to0 but retains prior main/alternate
profile floats. A known main without an alternate retains old alternate floats.
Ink021110 uses the main profile for kinds other than2, including0, so resetting
those fields on each item would change native numerical results.

`CreateItemProducer` therefore creates a fresh zeroed template;
`PrepareNextItem` updates it sequentially and snapshots each returned item.
The template also survives native source-line flushes. A new producer is needed
for a new 0531b0 invocation, rather than every line or symbol. Portable malformed
geometry/profile arithmetic rejection occurs before committing the state update.
Concurrent mutation of one producer is outside its sequential contract.

## Verification and application gap

Independent controls create **256** original catalogs/alphabets and **3072**
source items, including duplicate catalog keys, case differences, missing IDs,
missing main/alternate categories and retained template fields. A Python
binary32 producer builds the candidate features; the independent NumPy C1A
reference then checks the complete composed fit. Managed and Windows NativeAOT
also check malformed UTF8, category ranges, cancellation, state independence
and invalid-input mutation.

An explicit mixed body/uppercase/descender geometry example fits baseline90 and
x-height40 although its combined vertical box spans10..130. The existing
prepared font-size policy then maps style16/reference-x-height8 to size80 in
model units. This demonstrates the restored prepared stages without selecting
the tallest or smallest symbol box. It is not a raw handwriting measurement.

The optional pinned PE auditor extracts the actual **3923-symbol/45-profile**
catalog privately and checks its normalization/default-template constants.
There are 122 Cyrillic catalog entries. Diagnostic mappings include Latin
`a/A/p` and Russian `а/А/р` to body/upper/descender categories, while `ё/й`
have category4 with alternate0. The actual catalog is composed with explicit
mixed Latin/Russian prepared geometry and the line/font policies. Expected
labels are never inserted into an OCR output, and no vendor catalog asset or
handwriting sample is shipped by these files.

Connected-word production in the application still requires actual recognized
character/source binding and per-symbol geometry. The current whole-word CTC
bbox does not satisfy that contract. The native wrapper's acceptance and bbox
fallback also remain a separate stage. This batch does not claim automatic
initial raw x-height or native font placement has been integrated into the UI.

```powershell
python tools/LegacyCharacterLineCategories.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/character-line-category-controls.json
python tools/handwriting/audit_character_line_categories.py research/myscript/sdk/build/x64/MyScriptDocument.dll D:/BlurMask-build-artifacts-20261011/character-line-category-audit.json
dotnet run --project tools/LegacyCharacterLineCategories.Probe -c Release -- D:/BlurMask-build-artifacts-20261011/character-line-category-controls.json D:/BlurMask-build-artifacts-20261011/character-line-category-audit.json
dotnet publish tools/LegacyCharacterLineCategories.Probe -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/character-line-category-aot
D:/BlurMask-build-artifacts-20261011/character-line-category-aot/LegacyCharacterLineCategories.Probe.exe D:/BlurMask-build-artifacts-20261011/character-line-category-controls.json D:/BlurMask-build-artifacts-20261011/character-line-category-audit.json
```

The first probe argument is sufficient for CI without private vendor assets.
