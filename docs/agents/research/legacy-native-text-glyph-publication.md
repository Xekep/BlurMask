# Ordinary text-source glyph publication

This bounded executable stage closes the default ordinary glyph UTF8 emitter → native string owner → Text021000 record flags/byte count. It does not select a recognized winner, create stroke endpoints, manufacture a source-object index, or claim Russian recognition accuracy.

## Proven producer and owners

Text01c010 constructs source2DA field118 through Engine factory88 type3E. Its descriptor is Engine23cf80 and instance table23cfc8; virtual10 is0315e0. This is a separate owner from result273 primary/control/alternate alphabets.0315e0 computes the explicit interval length, hashes it with02ffc0, and calls030e60. Equality is length plus bytewise memcmp, including embedded NULs. New content is copied and followed by a terminating NUL; identical intervals share an identity until owner destruction031390/031150.

`LegacyNativeUtf8StringPool` preserves those observable byte/identity semantics with opaque owner-local objects. It does not synthesize native pointer values or expose native hash bucket traversal order. Owner disposal invalidates reads; copied output cannot mutate the interned bytes. Allocation failures are represented by explicit portable budget exceptions rather than pretending that native allocation succeeded. The owner is unsynchronized and belongs to one source lifecycle.

021000 assembly reads the emitted begin pointer from stack argument5, source118 into RDX, and forms end=begin+byteCount in R9; optional insertion-status argument5 is NULL. It writes record0=0, copies glyph274 word6 low10bits into flags8 preserving higher bits, writes byteCount20 and interned pointer10.019030 receives the interned string and the original byte count. If that helper does not classify the first cluster,018b40 sets or clears flag10000 from its NUL-terminated literal lookup. A successful special classification leaves that prior bit untouched. `LegacyNativePublishedTextGlyph` exposes the authoritative fields; remaining40-byte record geometry/ownership is deliberately not fabricated.

## Default first-cluster service

Engine+78 table202a70 virtual30=05f290 returns native **script IDs**, not Unicode general categories.716 ordered12-byte ranges begin at1fb6d0; gaps return0. Native IDs7/8/9 mean Han/Hiragana/Katakana and set40000; Hangul10 sets80000. First codepoints3000..303F also set40000 unless the Hangul branch already applied.

Default iterator virtual48=05fa80 sets tailored mode1. Virtual58=05faf0 returns the complete scalar cardinality of the **first grapheme cluster**, although019030 only supplies storage for the first two scalars. It does not count the entire string and does not cap its return at2. Thus a leading newline followed by ordinary text still receives the newline flag, while `a` plus a combining accent is a two-scalar first cluster. Default charset5 consumes one initial UTF8 BOM while the interned interval and byte count retain it.

The port restores the actual scalar property producer05f140, narrow basic Hangul ranges,19 Control/225 Extend/11 Prepend ranges,77 native ZWJ joining ranges, CRLF, regional-indicator pairs, Lam-Alef and three-script virama tailoring. It preserves explicit ZWNJ and Arabic shadda breaks and Thai SARA AM exclusion. It uses the previously portable exact native CCC data, not host text-element segmentation.

019030 flags:1000 whitespace,2000 newline,4000 soft hyphen,8000 ZWNJ,20000 bidi controls,40000 East Asian,80000 Hangul. CRLF is the two-scalar special case. Ghidra shows a duplicated unreachable CRLF condition; assembly019142 is `cmp ecx,ecx` and the duplicate LF test cannot add another reachable case.

018b40 is an exact relative-bound UTF8 trie of120 records at Text30e160, not a Unicode punctuation property and not equivalent to a dictionary of all stored literals. Its third-byte binary search selects one matching row and then checks the remaining suffix.17 supplementary literals sharing a three-byte prefix are unreachable in the pinned implementation. The runtime preserves the bounds and this behavior. Lookup stops at an embedded NUL; the intern pool does not.

## Portable data and verification

`models/text-unicode/legacy-text-publication-data.json` contains the numeric ranges and all120 trie records with relative bounds. Source PE SHA pins are Text18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f and Engined31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c. Numeric table SHA isdd431ff067120a92b87cb35cb4d34253516740e4e0e9fedf841f8d258317edcc; asset19024bytes SHA6daddbc9ce3d1d3d2e9e179ce008db60887f2c6a13c52ab8bb47dcb215890673. Runtime does not require a vendor PE.

Original controls cover826 cluster/flag cases,1024 interval-length/owner/lifetime cases, malformed Unicode/budgets/cancellation/corrupt model guards, and188 actual pinned RU alphabet labels passed through the real ordinary emitter into source records. These labels are resource facts, not hand-labeled winning OCR outputs.

Independent [Unicode9 Scripts](https://www.unicode.org/Public/9.0.0/ucd/Scripts.txt) audit covers128237 assigned codepoints: one public/native script-name exception is ALM061C, nativeArabic5 versus publicCommon. Native reserved/future ranges remain intact. Scripts SHA isfba415952f5654145acad220dc2b878f815c673474d2bb4928934e3ba6ccca1d.

Independent [Unicode9 GraphemeBreakTest](https://www.unicode.org/Public/9.0.0/ucd/auxiliary/GraphemeBreakTest.txt) first-cluster audit matches683 of744 valid scalar cases;61 differences follow the native property/tailoring rules, notably restricted Jamo/ZWJ and broader emoji modifier extension.78 corpus rows contain surrogate values and are outside valid Unicode UTF8 scope. The corpus is audited without adding case-specific corrections. Corpus SHAc76e7ed44a602f85161758ec54d2bcbcb7baf38f97720605a3e81ce02af47061.

The read-only auditor checks source/table binding, literal trie behavior and public script correspondence. It never loads a DLL. The exporter is optional preparation tooling; the checked portable JSON loader is usable independently in managed and NativeAOT libraries.

## Reproduction

```powershell
python tools/LegacyTextGlyphPublication.Probe/reference_controls.py models/text-unicode/legacy-text-publication-data.json D:/controls.json
dotnet run --project tools/LegacyTextGlyphPublication.Probe -c Release -- models/text-unicode/legacy-text-publication-data.json models/text-unicode/legacy-normalization-data.json D:/controls.json D:/GraphemeBreakTest.txt models/ru-RU/ru_RU-ak-cur.lite.res
python tools/handwriting/audit_legacy_text_publication_binding.py models/text-unicode/legacy-text-publication-data.json D:/Scripts.txt --output D:/publication-audit.json
```

Malformed/legacy five- or six-byte UTF8 decoding, custom charset/property services, native memory exhaustion/bucket enumeration, circled conversion upstream and accepted winner/source-reference publication remain separate boundaries. No whole-word bounding box is passed off as a native line metric.
