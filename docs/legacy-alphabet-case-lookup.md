# Native alphabet case lookup and consistency

`LegacyNativeAlphabetCaseLookup` completes04F170's successful scalar-result
lookup and optional consistency path for an owned readable alphabet. It leaves
the stored case maps unchanged. It does not insert missing symbols or claim the
native mutable alphabet constructor, allocator-failure ABI or all map refresh
branches have been ported.

The inspected stored-readable interface1DE1B0 resolves V28 to0527E0 exactUTF8
hash lookup, V30 to052A80 text append and V38 to052650 virtual reverse-slot test.
The initial050400 predicate examines decoded input scalars, script!=24 and
Unicode case alternatives. It deliberately does not use stored case-map
membership. Virtual and uncased input returns threeFFFF labels. Default charset
iteration consumes one initial BOM; missing native source text is an error.

After the proved lower/upper/stripped-uppercase producer, each string must
contain exactly one complete native tailored grapheme cluster. A cluster can
contain several scalars, such as`i`+U+0307. Conversely uppercase`ß` yields`SS`,
which is not accepted even if the alphabet contains a literal`SS` label. Exact
lookup retains missing labels asFFFF. No Unicode confusable substitution,
host casing or label creation is performed.

Optional native consistency is bounded: if the input is its own lowercase form,
query its uppercase once without further recursion; an inverse-lower mismatch
clears both uppercase and stripped-uppercase labels. Otherwise, if input is its
uppercase form, query lowercase without recursion; inverse-upper mismatch clears
only lowercase. The no-recursion wrapper04FFB0 explicitly writes stackarg6=0.
Long-s/dotless-i and virtual inverse targets exercise these branches.

One native table quirk is preserved: the alternating range01C4..01C4 takes
precedence over lowerSingle01C4,+2, producing01C5 as lowercase of01C4. Therefore
the inverse consistency test of01C6 can invalidate its uppercase. The older
independent scalar oracle already verifies this table precedence; the new
manual lookup controls encode that native behavior, rather than modern Unicode
expectations.

The probe has64 independently specified label triples over a synthetic actual
UTF8-hash alphabet. It covers Cyrillic Ё/Й accent forms, title-vs-uppercase,
multiscalar graphemes, multi-cluster refusal, recursion failures, virtual/missing
forms, BOM and cancellation. Its three hash levels use the same native hash
bits16..19,6..10 and0..5, with byte-sorted collision buckets. This is Unicode
lookup/ownership evidence, not a handwriting corpus or final word winner.

Run `tools/LegacyAlphabetCaseLookup.Probe` with case-data, normalization-data and
publication-data JSON paths. Run `tools/handwriting/audit_alphabet_case_lookup.py`
with a report output path for the pinned PE-only proof.
