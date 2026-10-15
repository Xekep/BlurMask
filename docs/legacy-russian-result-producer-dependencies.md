# Remaining Russian result-production dependencies

The first/later case-aware word consumers, compact predecessor cursor and
prepared glyph finalizer are separate pieces. They do not yet form a complete
case-aware main pass or a recognized-text pipeline. AI pen is not switched to
these prepared components.

The actual root-anchor case main loop is now connected by
`LegacyOwnedMainCaseWordPass` ([scope and controls](legacy-owned-main-case-word-pass.md)).
It covers genuine4/60-node source passes; the later/protocol language routes
and full result-production chain are still not established by these controls.

## Symbol resolution216860

The resource-bound no-wide/no-Arabic scalar branch is now restored in
`LegacyOwnedRussianGlyphSymbolResolver`
([scope and controls](legacy-russian-glyph-symbol-resolution.md)). This does
not supply recognized text. The enclosing ordinary subtype0 producer is now
restored separately for actual CASE and DIRECT owners
([ordinary controls](legacy-owned-russian-ordinary-glyph-production.md),
[DIRECT integration](legacy-direct-russian-ordinary-glyph-production.md)).
Protocol output and full word reconstruction remain separate.

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
This applies when knowledge99 is false; its explicit true scalar bypass
retains the input after the early converted-quote branch.
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

The owned adapter retains the actual main group/node/arc and NN/cache
owners. It must derive the selected vector channel, top20 cutoff, related
label aliases, original-label calibration/fallback and geometry costs. The
actual main source uses confidence mode1 (resource566408), four families,
and no individual channels in flags21400. The first-source mode0 confidence
adapter cannot replace it. Extended aliases need original-label calibration;
indexing a mapped label's confidence alone is insufficient.

The scalar mode1 table/getter is now separately restored by
`LegacyMainConfidenceCalibration` ([scope and controls](legacy-main-confidence-calibration.md)).
The table has192 original-label rows per family, despite180 neural outputs;
labels180..191 have stored coefficients. The full group/vector adapter above
still needs to bind the neural alias to the original-label calibration and
the actual source-owned group/channel. That owned scalar vector layer is now
restored by `LegacyOwnedRussianMainGlyphConfidence`
([scope and controls](legacy-owned-main-glyph-confidence.md)), for genuine
DIRECT and case owners and their single-/multi-physical source scorers.
Its closed-primitive channel retains the original source float. Geometry/cost
requests and ordinary216BC0 are now separately restored by the owning ordinary
producer. Full protocol/multi-output handling remains separate work.

The complete-lookahead DIRECT207CB0 route now has actual owned compact
materialization, with separate1BE530 field0 weights; these are distinct from
1C1300 channel0 confidence. Its numeric21FFD0 row walker and one ordinary
216BC0 integration are verified without expected text. Older-prefix
materialization, general CASE publication, full21FFD0 segment/protocol/callback
production and218560 acquisition/linking remain incomplete.
The217DB0 residual controller and21FBF0 quote normalizer are separately restored
over explicit service/word owners. Genuine residual merge/reference ownership
and an accepted end-to-end text result remain required. The full case main loop
must also supply genuine203690 context
policy, row extents and completion scheduling. Prepared controls establish
individual calculations, not Russian cursive accuracy.
