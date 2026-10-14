# Fresh first-model literal glyph production

`LegacyFreshFirstGlyphSymbolPlan` restores the already-selected symbol branch
of1F42D0 under actual fresh configuration6C. This flag bypasses alias and case
conversion for every symbol, including primitives. The glyph receives count1
and13AF30 kind0 with the literal selected label. F0001 is not converted into a
guessed letter or circle character.

Before that branch, E2 (alphabet.virtual10 label count192) is replaced with the
actual alphabet headerE label178. That label is **virtual**, not a space.
The first alphabet has space180, newline182, replacement character189,
F0000 start-marker190 and F0001 closed-marker191. HeaderE is filtered by its
separate source-wrapper policy before UTF8 publication. Its missing primary
text is not fabricated here. F0000 still needs the separate circled codec.

A missing category mapping selects geometry index0, not rejection. Mismatch
bit8 compares the primary six-bit category to the raw symbol start map, except
headerE always clears it. Result flags preserve existing bits and add40;
actual replacement-character label also adds8. Selected raw labels and
published labels remain distinct. The plan does not infer confidence, cost,
descriptor identity, source ranges or a word boundary.

1,170 independent controls parse the pinned resource's category and alphabet
tables in Python. They cover195 labels, six primary categories, missing maps,
virtual blank replacement and exact40-byte updates preserving unrelated fields.
An actual raw enclosing contour runs first frame/cuts/neural networks and final
cache; its retained F0001 candidate reaches literal kind0, private UTF8 and a
single-reference WORD275 publication with source-owned confidence. Selection
of that retained candidate is explicit. This does not prove the complete
first273 terminal winner or Russian/cursive accuracy.

```powershell
python tools/LegacyFirstLiteralGlyph.Probe/reference_controls.py models/ru-RU/ru_RU-ak-cur.lite.res D:/literal-glyph-controls.json
dotnet run --project tools/LegacyFirstLiteralGlyph.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res D:/literal-glyph-controls.json models/ru-RU/legacy-engine-profile.json
python tools/handwriting/audit_first_literal_glyph.py --output D:/literal-glyph-static.json
```

No vendor DLL is executed. Native multiword/alternative materialization,
main240 linguistic winner and application recognition remain separate work.
