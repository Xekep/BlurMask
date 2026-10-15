# Mutable alphabet case scalar streams

The next restored part of Engine04F170 is its scalar/string producer, before
mutable alphabet lookup. It does not replace the stored-alphabet relation maps
or claim the complete mutable alphabet factory is finished.

The default global+68 service is bound by031910 through pointer252188,
descriptor1DD740+20, runtime table1DD7A0. Virtual60 is048190, canonical
decomposition/ordering; virtual68 is048300, which calls048190 followed by
04AA10 canonical composition. `LegacyNativeCanonicalDecomposition` exposes the
former step; existing canonical scratch supplies the latter without changes.
The checked numeric iterator retains the native missing-CCC value−1 and default
writer substitution of surrogate/FFFE/FFFF values to1A. Valid scalar case input
is stricter and refuses surrogates. Custom codecs/malformed UTF8 are outside
this already-decoded input scope.

05F4C0 emits72-byte alternatives: lower pointer/count00/08, upper10/18,
title20/28, language30 and context34.04F170 consumes lower and upper, not title.
`LegacyNativeMutableCaseStreams.Prepare` uses owned05F8E0 context and the
previously proved ordered/language-aware selection. If no row matches, both
writers retain the original scalar; empty selected sequences remain empty.

Both outputs undergo canonical composition into separate scratch. A third stream
is made by canonical decomposition of the **raw uppercase writer**, removal of
**CCC200 through234 inclusive**, then
composition. This is neither titlecase nor general Unicode-mark stripping:
Hebrew sheva CCC10 survives, acute CCC230 is removed. Cyrillic Ё/Й can thus
produce stripped uppercase Е/И as this native alphabet-preparation operation;
this is not a post-recognition replacement rule.

The raw-writer distinction is pinned in assembly:04FB9E (V68 at04FBB7) and
04FCCE (V60 at04FCE7) both load sourceRDX=`RBP-50`/charsetR8=`RBP-40`;
destination is the separate pair at`RSP+68`. No assumption that native
NFD(NFC(x)) always equals NFD(x) is needed.

Later04F170 tests one complete **tailored grapheme cluster**, rather than one
scalar, then performs AL virtual28 lookup.05FA80 sets tailoring flag+40=1.
Native optional recursion checks lower/upper consistency and can invalidate
individual form labels. Label insertion, lookup ownership, recursion, failure
return byte and partial output behavior are not represented by these successful
scalar outputs. They remain the next mutable alphabet stage.

The probe checks23832 independently generated stream triples across all2845
cased scalar inputs plus mixed suffix contexts/flags/languages. The independent
canonical oracle uses recursively expanded lists and stable segment sorting.
All93610 official Unicode9 NFD projections pass except exactly two native
service-policy deviations, isolated037E and0387, whose decompositions are
explicitly refused by05ED00. The official corpus hash is
`2d48d848656b3cf889df59980ab13551988950c4ca8c5190a17c691a17f8b9bb`.
Independent controls retain the old public expected outputs; no recognition
goldens or stored alphabet case maps are changed.

Portable limits are explicit:65536 numeric decomposition inputs,262144 output
scalars,1048576 work visits,32 recursion depth; case context input1..4096.
Ownership, cancellation, gap ordering, substitutions and bounded mark chains
have separate controls. These prove Unicode transport, not handwriting quality.

```
python tools/LegacyNativeCase.Probe/reference_controls.py models/text-unicode/legacy-case-data.json scalar-controls.json
python tools/LegacyMutableCaseStreams.Probe/reference_controls.py scalar-controls.json models/text-unicode/legacy-normalization-data.json stream-controls.json
dotnet run --project tools/LegacyMutableCaseStreams.Probe -c Release -- models/text-unicode/legacy-case-data.json models/text-unicode/legacy-normalization-data.json stream-controls.json NormalizationTest.txt
python tools/handwriting/audit_mutable_case_streams.py static-report.json
```
