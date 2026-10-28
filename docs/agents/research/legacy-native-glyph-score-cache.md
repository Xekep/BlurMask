# Native glyph score callbacks

`LegacyNativeGlyphScoreCache` restores the three ordinary acoustic callbacks
that first-result materialization reads. It owns an actual prepared group's
candidate24 records, their separate float vectors, geometry costs and scalar
fields. It does not create confidence vectors from a word's score or resolve
a graph group from a physical stroke number.

The pinned Text frontend table2de610 routes158/1B8/1C0 through1529C0/152F50/
152F90 to ordinary21d table2e4180 slots90/80/88. These resolve to1C1520,
1C1300 and1C13A0 respectively. Each calls its actual graph180.virtual68,
then addresses runtime2F0 plus returned group index times38.

1C1300 scans candidate24 rows for the first matching label8. Its return is
the raw float at row.vector10[actual frontend-owner config88]. Missing labels
return zero. NativeASM1C1382 returns this throughXMM0. It is neither the
candidate24 normalized probability at field0 nor TOP273 mass8.1C1520 returns
the first candidate's vector10[0], including rawNaN/sign bits; an empty group
returns zero. The first materializer copies these into different result
fields, so the channels must stay separate.

1C13A0 takes the actual16-byte request after group resolution. Explicit
ordinal6 selects a candidate irrespective of label8;FFFF scans by label,
except the actual blank label uses group18 directly. Gate byteB rejects
candidate indices20 and above. Nonzero byteC uses MULSS(group1C,40000) then
CVTTSS2SI, including integer-indefinite overflow. Finite costs below4000000
receive the geometry cost selected by the signed byteA, except the exact
eight caller-owned bypass labels. ADD preserves signed32 wrap. A missing
pointer/index is refused by this portable API rather than dereferenced.

The complete candidate records retain opaque fields and pointer bytes as
data; float vectors are owned separately and no native pointers are
manufactured. Bounds, cancellation and copied outputs are checked. The
independent Python oracle tests1024 groups, duplicate labels/first-match,
explicit ordinal versus label, rank gates, blank and scaled fallback,
overflow, rawfloat payloads and ownership. Managed and NativeAOT execute
the same tests. The static auditor checks six callback bindings and the
critical actual machine instructions without loading a DLL.

```powershell
python tools/LegacyGlyphScoreCache.Probe/reference_controls.py D:/glyph-score-controls.json
dotnet run --project tools/LegacyGlyphScoreCache.Probe -c Release -- D:/glyph-score-controls.json
python tools/handwriting/audit_legacy_glyph_score_cache.py --output D:/glyph-score-audit.json
```

Upstream1BCBD0 confidence-vector production, configuration88 selection,
graph68 owner construction and final273/276 winner assembly remain separate
producers. These checks do not claim Russian cursive accuracy or enable a
new recognition provider in the application.
