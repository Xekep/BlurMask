# Remaining Russian result-production dependencies

The first/later case-aware word consumers, compact predecessor cursor and
prepared glyph finalizer are separate pieces. They do not yet form a complete
case-aware main pass or a recognized-text pipeline. AI pen is not switched to
these prepared components.

## Symbol resolution216860

The ordinary RU path can be bound to the actual acoustic alphabet and the same
case preparation owned by the fresh dictionary. Its wide-pair list is empty
and its Arabic shaping array is absent. These facts must come from the actual
resource/preparation; substituting caller-selected booleans would not establish
the branch.

Engine29.V40/04EA40 selects width forms, not uppercase/lowercase letters.
AL.V80=052D40 reads the wide-pair countC8 and V88=052D50 reads its pairsB8/C0.
AL.V68=052640 and V70=052CD0 expose the distinct Arabic-shaping data. Neither
relation is the stored-case triplet table used by dictionary292. With both
branches absent, ordinary216860 emits the original label, independently of
knowledgeBC. Its setter00D180 supports0..3; a constructor zero alone does not
prove a generic default-mode binding.

The output is exactly one ushort AL/synthetic label and a normalized-original
label. It is not a string of Unicode characters. ExtrasALCount+1/+2 represent
apostrophe/quote: the caller's conversion byte determines whether to retain
the extra ordinal or emit the corresponding ordinary AL label. Ordinary
21FE30 calls218560 with that argument literally0;219580 uses1 on another
reconstruction route. FFFD needs the real frontend1A8 fallback producer.
Multi-outputFFFA/B/C belongs to216BC0/frontend1A0, not216860.

## Main-source score ownership for216BC0

The exact72-byte backtrack row provides source node00, arc04, label3A,
mismatch3D and likelihood3E. The first raw score is byte3E/255. Quote extras
are remapped for score lookup even when the emitted label remains synthetic.

Frontend1B8=152F50 forwards classifier21D.V80/1C1300;1C0=152F90 forwards
V88/1C13A0. The latter consumes a16-byte request with node0, arc4,
ordinal6=FFFF, label8, geometryA and zeroB/C. Geometry derives from the main
sampler's start/end composite mappings, with the separate closed-primitive
case. `LegacyNativeGlyphScoreCache` already implements the scalar getters.

The missing adapter must retain the actual main group/node/arc and NN/cache
owners. It must derive the selected vector channel, top20 cutoff, related
label aliases, original-label calibration/fallback and geometry costs. The
actual main source uses confidence mode1 (resource566408), four families,
and no individual channels in flags21400. The first-source mode0 confidence
adapter cannot replace it. Extended aliases need original-label calibration;
indexing a mapped label's confidence alone is insufficient.

After this adapter and216BC0, the remaining route still includes full207CB0
materialization,21FFD0 row/protocol/callback production,217DB0 residual
association,218560 glyph-chain acquisition/linking and final quote/text
publication. The full case main loop must also supply genuine203690 context
policy, row extents and completion scheduling. Prepared controls establish
individual calculations, not Russian cursive accuracy.
