# Word completion language-category transport

The checked first and later dictionary completion producers now use the actual source70 token field at byte8 for100090's language category. They previously passed the geometry-derived word category. These are distinct native fields. This fix does not make the unfinished decoder or application OCR complete.

Static proof uses the pinned MyScriptText SHA256 `18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f` and executes no vendor code:

- 204D9E calls source70;204DA5 loads its complete16-byte return intoXMM7.
- 205368 copiesXMM7 toRBP188 with request baseRBP170: the token begins at request18.2061D4 does the same for first/fallback requestRBP1C0 -> RBP1D8.
- 205BCD's history copy is equivalent;205BF6 overwrites only boundary byte24 from the previous word flags. It preserves token category at request20.
- 1003E4 passes request20 as0FFCF0's category;100DB0 passes the same field to0FFEC0 for tagged class-history output.
- Plain094040 clears both return qwords and writes word ID/boundary/suppression elsewhere. Its category iszero. Case292 metadata can replace it with a different category; no geometry fallback is inferred here.

`audit_word_completion_category.py` checks these pinned instructions and decompiled contracts. The first-language probe deliberately varies16 nonzero geometry categories against a real terminal token with category0, comparing the complete40-byte LM record. The later formatter oracle independently reads model tables and uses the corrected input contract for256 full40/full128 outputs; its geometry values still determine the separate native boundary/path correction.

The generic `LegacyNativeKnownWordLanguageRecord.Evaluate` keeps its explicit prepared category argument. It represents100090's request and does not infer caller layout. The two completion callers now transport the source token category explicitly. Existing category-policy validation and all unsupported-scope guards remain.

Validation: Managed andWindowsNativeAOT probes, plus actual DIRECT4/56/60-node passes. Their M-shaped fixtures do not exercise a changed language category: the60-node whole-storage hash remains `1f538d72437fbcbf68adcae9a7f42ad18db1ae717414192b1729ea805966943f`, and its selected pre207CB0 row remains ordinal127, relative110955+cumulative346098=457053. Matching scheduler oracles alone cannot prove the LM category contract because they share the completion primitive; the pinned assembly and separate nonzero-category/full-record controls provide that evidence.

Case-aware completion's full-tag/source-owner lease integration is a separate next stage. No completed text or final winner/materializer claim follows from this correction.
