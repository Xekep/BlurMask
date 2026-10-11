# Native acoustic wide-symbol pairs

`LegacyWideSymbolPairs.CreateUnrestricted` restores the unrestricted
Engine `180058da0` pair producer. It scans acoustic label IDs in ascending
order and accepts only a single complete Unicode scalar whose decomposition
descriptor has kind0x800 and an existing exact substitute in the same alphabet.
The pair is `(baseLabel,wideLabel)`. Native `1be930` later fills an impossible
sibling with the other's class cost plus1. It does not generally merge scores.

This is the Unicode **wide** decomposition (`<wide>`), not case folding,
typographic similarity, or a Latin/Cyrillic substitution policy. The helper
constructs its104 mappings from simple Unicode intervals and seven currency/
symbol mappings, without embedding the full engine character table. It rejects
multi-scalar label text, missing base labels, and all other decomposition kinds.
The restricted native branch uses additional label eligibility; that branch is
not inferred by this method.

Static binding: Engine initializer `180031910` installs service pointer
`180252200` → descriptor `180202b00` → descriptor+20 vtable `180202a70`.
Its slot+8 is `18005ed00`, the decomposition query used by the producer.
That query searches the5722-entry index at `1801f2bf0` with records rooted at
`1801df3e0`, reached through `1802521f8`/`1802521f0`. Record high bits encode
kind, low byte encodes UTF32 substitution count. All104 kind0x800 entries match
the independently generated public Unicode wide mappings. No vendor code was
executed for this comparison.

The independent character-property audit additionally matches all5722 native
records to official UnicodeData9.0.0. The104 wide mappings are unchanged in
the probe's public UCD15.1. Their ordered little-endian scalar/base uint32
bytes have SHA-256
`ee6e71209527db0ffea3770d54d1ecff1839af62e29bc7ee0e65061e22fe3a47`.

The observed192-label Russian acoustic alphabet has no wide characters.
Consequently this producer gives an **empty pair list**, independently of
restricted/unrestricted eligibility. Its existing decompositions are NBSP
kind4 to space, micro sign kind0x10000 to Greek mu (absent), and Ё/Й/ё/й kind1
to base-plus-combining marks. None enter the kind0x800 branch. Latin `m/p` and
Cyrillic `м/р` have no decomposition record. Their different neural scores
therefore remain different through this native C0/C8 mechanism. Other later
alphabet mutations and language scope are distinct policies.

`tools/LegacyWideSymbolPairs.Probe` passes104 exhaustive wide references and
1333 scalar controls from Python's public UCD15.1, plus original serialized
alphabets checking existing bases, complete scalar consumption, no script/case
folding, ascending pair order and cancellation. The actual Russian resource
gives zero pairs. Managed and Windows Native AOT results agree. These controls
establish symbol-pair behavior; they are not handwriting accuracy evaluations.

```powershell
python tools/LegacyWideSymbolPairs.Probe/reference_controls.py controls.json
dotnet run --project tools/LegacyWideSymbolPairs.Probe -c Release -- controls.json models/ru-RU/ru_RU-ak-cur.lite.res
```
