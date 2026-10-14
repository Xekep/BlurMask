# Selected first glyph allocation and no-word result

The outer1F42D0 guard is separate from the later fresh6C literal symbol plan.
It allocates a glyph only when `(selected!=E2 && selected!=E4) || owned99`.
Actual first E2 is192 and E4 is191/F0001. Fresh invocation99 is false, so those
selected cells do **not** create a glyph, link, symbol update or score read.
The later E2→headerE conversion occurs inside that guard and cannot bypass it.

The pinned instructions are1F4DF5/4DFF/4E0D. The skip at1F4E1E jumps1F62B9,
then the predecessor backedge1F62CA→1F4D00 continues the complete B8 source walk.
It also bypasses that cell's word-boundary/publication and leading-endpoint
branch. A hidden selected root can therefore leave allocated non-root glyphs
in the pool without publishing a word. An all-hidden selected path has only
the initial detached trailing zero-count endpoint. No leading endpoint is
invented, and no source span is obtained from a nonexistent word owner.

`LegacyFreshFirstSelectedGlyphPublication` obtains E2, E4 and invocation99 from
one owned factory. The materializer uses this decision before allocation and
before unsupported interior-word publication checks. It keeps every selected
cell and original graph/source traversal. Existing strict alphanumeric policy
still rejects unsupported selected symbols. Its literal word-only API returns
an actual nonempty word or refuses the entire no-word result. The additive
`MaterializeNativePlainResult` represents nullable word/leading endpoint and
retains the complete selection, pool and source ownership. Its computed
unadjusted scalar diagnostics are not written into a nonexistent WORD275.

The historical primitive-word diagnostic was wrong at this outer boundary:
actual B8 remains labels134,191 atcost12111, but first275 contains only134.
The selected F0001 confidence/model provenance remains available independently;
it is not published or replaced with a guessed letter. MIR remains mixed-script
`MИP` at41268. This correction does not claim a full Russian language winner.

Four own actual fixtures exercise57 NN/cache groups and unchanged selections.
Twenty-four INTERNAL prepared selected-path fixtures cover all-hidden E4, E2/E4
mixtures, skipped boundary bits and a hidden root with an allocated non-root
glyph, and an ordinary root with hidden later cells. They are publication controls, not evidence that native geometry or
the NN selected those artificial blank paths. Actual geometry can reject such
paths; the controls explicitly use owned copied tables and prepared costs.
No public owning context producer accepts a primary/cost override.

The independent Python oracle checks selected allocation order, exact keys,
descriptor IDs, symbols, likelihoods, zero-count endpoint records, nullable
word/leading-root policy and binary32 scalar diagnostics. It distinguishes the
four actual captures from the twenty-four prepared controls. Static proof reads
vendor bytes/decompiled text only; it does not execute a vendor DLL.

```powershell
LegacyFirstSelectedPublication.Probe AK.res engine-profile.json separation-profile.json captures.json
python tools/LegacyFirstSelectedPublication.Probe/audit_output.py captures.json
python tools/handwriting/audit_first_selected_glyph_publication.py --output static.json
```

Traits, Arabic/case/alias shaping, interior separators, multiple words, all273
alternatives and the main240 linguistic winner remain separate work.
