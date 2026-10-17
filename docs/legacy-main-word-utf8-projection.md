# Owned WORD275 UTF8 and source-key projection

`LegacyOwnedMainWordUtf8Projection.Project(actualPublicationResult)` consumes
the successful owning21FE30 callback result. Its native275 reading-order vector
provides actual pool references; every40-byte glyph is copied from the same
result's274 pool. The real descriptor drives the existing ordinary token/UTF8
emitter using the selected symbol producer's acoustic alphabet. No expected
word, replacement label, supplied glyph bounds or guessed source ID is read.

For each glyph, canonical pool key and stored descriptor ID are matched to
the current associated72-byte rows. The latter ID is the compact index minus
the actual2478 start for its source node, as in218560. A prebuilt lookup avoids
repeatedly scanning the segment for every glyph. Duplicate matching producers
are retained; an ambiguous ordinary/protocol binding is refused.

Ordinary source rows must resolve through the same residual overlay to exactly
their original scored node/arc. Their owned sampled ranges are copied, including
any genuine higher-range source; NN geometry identity is not replaced with a
merged box. Protocol span keys remain opaque and distinct from graph arcs.
They carry no fabricated ordinary NN reference or sampled ranges.

The returned UTF8, glyph record, matching row ordinals and source ranges are
owned snapshots. A successful zero-span callback has no published word and
therefore produces an empty projection. Failed publication is refused.

## Deliberate boundaries

This is **raw ordinary glyph UTF8**, before0213A0 wrapper filtering, smiley
replacement or021000 accepted text-source publication. It is not a complete
2271F0 text result. The primary symbol alphabet, dynamic result273+18 control
alphabet and alternate byte storage are distinct: nonempty kind3 and kind4
are refused until their actual owners are supplied. The existing ordinary
emitter also refuses circled-start input rather than pretending to normalize it.

Canonical source keys and sampled stroke ranges are not document object IDs,
legacy24-byte input references or original raw point ordinals. This component
does not instantiate those types, invent whole-word geometry or claim the
remaining source-wrapper bridge is complete.

## Verification

Managed and fresh Windows NativeAOT probes run the single-source fixture and
all nine pinned recorded tracks. Each runtime checks320 actual published-word
projections,1024 glyph/source mappings,320 zero-span callbacks and1280
null/cancellation guards over32 distinct allocator/scratch residues per input.
The independent oracle parses the raw acoustic alphabet and actual pool40
records, expands their descriptors and compares every UTF8 byte, canonical
key, matching row ordinal, sampled range and final published-node reading order.
All ten managed/native capture pairs are byte-identical. No expected labels
are used in these correctness checks. Actual positive protocol-span words,
nonempty residual events and alternate/control emission remain unexercised by
these particular selected traces.

```
dotnet build tools/LegacyMainWordUtf8.Probe/LegacyMainWordUtf8.Probe.csproj
```

Probe positional arguments follow the owned flush probe:

```
AK.res LK.res engine-profile.json separation-profile.json
legacy-case-data.json legacy-text-publication-data.json output.json
[line-2e3-30.res line-2e3-50.res private-recorded-manifest.json word-index]
```

```
python tools/LegacyMainWordUtf8.Probe/audit_output.py AK.res output.json
python tools/handwriting/audit_main_word_utf8_projection.py static-report.json
```

Static verification pins13AE90 lookup/key normalization,139C90 scalar token
conversion and13C840/13C920 repetition/emission. It corroborates the compact
descriptor/source identity and explicitly separate control/alternate owners;
no vendor code is loaded or executed.

## Separate accuracy diagnostic

`tools/handwriting/measure_main_word_corpus.py` is a metrics-only evaluator.
It first opens all nine completed inference captures and verifies coordinate
provenance and identical outputs across allocator residues. Only then does it
read expected strings from the private manifest. It does not invoke inference
or feed labels/corrections back to the decoder. Character error rate is ordinary
Levenshtein distance over raw Unicode code points, with case preserved and no
stripping, NFC, transliteration or aliases. Repeated allocator controls are not
counted as extra independent samples.

```
python tools/handwriting/measure_main_word_corpus.py PRIVATE_MANIFEST CAPTURE_DIR PRIVATE_REPORT
```

`--prefix word-utf8-native` selects native captures instead of the default
`word-utf8-managed`. Coordinate samples and reports with expected/actual strings
remain private build artifacts and are not committed. These nine words come
from three writers; pen-up information is absent, so one unsegmented track per
word is an explicit diagnostic assumption. This is not a representative or
writer-held-out accuracy benchmark, and does not claim stock-engine equivalence.
The current diagnostic has16 edits over33 reference code points (CER48.48%) and
4 exact words out of9. Byte parity is therefore not an assertion of good OCR.
