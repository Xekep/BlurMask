# Owned main-source glyph confidence

`LegacyOwnedRussianMainGlyphConfidence` connects actual main0 sampling,
linguistic masks, neural scoring and final class cache to the ordinary
`1BCBD0` confidence-vector producer. The factory accepts an actual
`LegacyFreshCaseRussianDictionaryPreparation` or
`LegacyOwnedRussianDictionaryPreparation`; each uses its own existing
`CreateAcousticScorer`. A DIRECT invocation retains the DIRECT owner and
does not construct a parallel case dictionary. `Model`, `Graph`,
`CaseDictionaryOwner` and `DirectDictionaryOwner` preserve those identities.

There are overloads for the owned singleton source and for an actual
`LegacyFreshMainSingleLineContext`. The latter must belong to the same
sampled source. It supports multiple physical strokes in one assigned source
descriptor; multiple assigned descriptors/lines remain outside the existing
sampler contract. The public entry accepts no replacement mask, candidate
list, neural probability, baseline, source-probability vector or score-channel
argument. `ScoreGroup` invokes the actual scorer internally.

The first20 final candidates receive confidence; later candidate vectors are
zero. The source-derived insert-only `3A0` map selects a neural output, while
the ORIGINAL candidate label selects the full192×4 calibration matrix.
For example, underscore186 uses prototype140's neural probability but
underscore186's stored coefficients. The four calibrated binary32 values
are summed in native order1,0,2,3 followed by the absent zero families,
divided by4 and capped at1. These are distinct from integer acoustic costs,
cost-derived exponentials and normalized candidate24 field0.

The special F0001 label191 has no neural prototype in this source. A retained
top20 instance uses the original source218.V60 float indexed by the FIRST
STORED graph range's physical ID. It does not use chronological range order,
average several strokes, apply a logistic or clamp the source float. Missing
symbols and candidates after20 return0. `ReadFirstCandidateScore` corresponds
to the first vector channel0. Public candidate/probability snapshots are
defensive copies; the actual score owner remains internal for the following
geometry/cost producer.

The supported source policy is fresh ordinary240 configuration without
external property events or reused container state. NEW00C860 zeroes88 in
its84..8B word. Frontend152F50 reads that byte and passes it to1C1300. The
registered52-property copy path and its table are pinned separately; this is
not an assertion about arbitrary configured native objects. The actual
main policy disables individual-network channels. Fresh21D350=-1 disables
the subsequent1BE1A0 pruning branch;1BE530 changes candidate24 field0 rather
than confidence vectors. Ordinary1BE930 calls1BCBD0 with flag0, selecting D8.
The static audit checks these paths without executing vendor code.

The headless probe exercises both owner kinds on one physical stroke and on
two physical strokes in one assigned line:92 actual NN/cache groups and908
confidence rows. These fixtures have no retained extended labels, primitives
or candidates beyond20. Separately,128 explicitly prepared numerical cases
(3200 rows) exercise extended original labels, synthetic quotes, raw primitive
floats (also-.125/1.125 to distinguish direct transport from clamping) and the
top20 cutoff through the same internal kernel. They are not
additional handwriting observations. The probe checks36 ownership, scope,
index and cancellation failures plus defensive copies. Its existing first
submission fixture is prepared upstream; this test does not establish a new
native whole-controller winner.

The independent Python oracle reads the pinned raw resource, its actual
zero-count knowledge groups, header/prototype mappings and all calibration
coefficients. It checks original-versus-alias indexing, float addition order,
primitive transport and cutoff separately for real and prepared data. It
allows at most two final float ULPs across platform libm implementations;
the local Managed and Native runs match the oracle exactly.

Probe arguments are `AK LK engine-profile separation-profile case-data
publication-data line30 line50 output.json`. Audit with
`python tools/LegacyOwnedMainGlyphConfidence.Probe/audit_output.py AK
output.json`; static audit takes one output JSON argument at
`tools/handwriting/audit_owned_main_glyph_confidence.py`.

This restores owned main scalar confidence, not216BC0 glyph classification,
geometry publication, final text selection or complete Russian recognition.
