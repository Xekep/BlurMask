# Owned Russian scalar glyph-symbol resolution

`LegacyOwnedRussianGlyphSymbolResolver` restores the ordinary `Text216860`
scalar branch for the actual fresh Russian case dictionary owner. It returns
one emitted ushort and one normalized-original ushort. It does not publish
text, run the classifier, choose a winner or implement the surrounding
`216BC0` protocol-symbol lifecycle.

The factory requires the owner's exact `Symbols.Preparation` instance and
acoustic alphabet reference. Successful checked case preparation proves that
the pinned script service found no Arabic script; independent raw resource
inspection also checks both Arabic digit ranges. The actual alphabet has no
wide-decomposition entries. Stored case triplets do not imply the separate
Arabic-derived Engine88 array. The resolver computes the wide-pair count;
neither condition is supplied as a caller boolean.

`216878` tests only the low byte of the conversion argument. If nonzero,
synthetic193 maps to actual apostrophe134 and synthetic194 maps to quote129.
Both returned values are the converted label. These early returns precede
the knowledge99 and replacement branches. With a zero low byte, synthetic
values remain unchanged. Other ushort values, including192 and65535, pass
unchanged in this scalar branch. This does not give protocol sentinels the
meaning of ordinary published characters.

Knowledge99 is an explicit prepared argument; after the early converted
quotes its native bypass returns both values unchanged, including U+FFFD.
With knowledge99false, actual U+FFFD requires frontend1A8's unresolved
fallback producer and is explicitly unsupported. No fallback character, source geometry or label substitution is
invented. Previous/following context and BC policy do not affect the proved
no-wide/no-Arabic ordinary branch.

Native provenance is pinned in
`tools/handwriting/audit_russian_glyph_symbol.py`: complete executable216860,
both conversion callers, synthetic-label writers, EngineV68/V80 and actual
raw alphabet/script tables. Primary `21FE30` passes conversion0 through
`218560`; alternate `219580` passes1. The resolver exposes a ulong argument
so256/257 and other upper bits cannot accidentally change low-byte semantics.

The headless probe tests131070 successful combinations covering every
ushort except U+FFFD's label with conversion0/1, plus2709 explicit actual
alphabet/synthetic rows for0,1,2,255,256,257 and ulong maximum with both
knowledge99 values. Fourteen
ownership, unsupported-policy and cancellation refusals are checked.
`audit_output.py` independently parses the pinned raw alphabet, derives
quote/replacement labels and checks both output channels and the exhaustive
SHA256 stream. These are scalar implementation controls, not handwriting
accuracy or end-to-end Russian support evidence.

Probe arguments are `AK LK engine-profile separation-profile case-data
publication-data output.json`. Run its independent audit as
`python tools/LegacyRussianGlyphSymbol.Probe/audit_output.py AK output.json`.
The static PE audit takes `--resource AK --output report.json` and never loads
or executes a vendor DLL.
