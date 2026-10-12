# Recognized source items to line font metrics: actual dispatch audit

This audit identifies the concrete Document recognition dispatcher and the
actual owner boundaries needed for live font-size samples. It does not claim
that the decoder winner, source alignment or application integration is ready.
Vendor code is never executed during inspection or probes.

## Source recognizer and decoded item transport

Document **0540d0** prepares a text field's existing2DA state and invokes
**0526c0**, scheduling **027590** for each prepared source block. `027590`
queries interface **EED** and invokes **virtual20** with the field, block,
statistics and explicit caller context.

EED's descriptor is **1fd090**, packed kind **0x13300000eed**. Its dispatch
table is **1fd0d8**, and **+20 = 047c50** is the recognition operation. The
adjacent **+28 = 0485b0** only copies/resets statistics and is not recognition.
`047c50` obtains the field's raw input snapshot, prepares module+218 inputs,
and delegates decoded output to **module+220.virtual28**.

Module+220 is concretely constructed by **0450c0** as class **EEE**, with a
**0x108 = 264-byte** prepared parameter record. EEE's descriptor is **1ed9a0**;
runtime table **1ed930+28 = 03a260** invokes main decode **038610**. Its child
at **+80** is an EEF processor created by **0359f0**. Decoder language/beam
preparation includes **EF1/EF2** selection, plus configuration names
`beam_size`, `language_model.alpha/beta`, `src_len`, `downsampling` and
`strk_ends`. Those names do not establish resource compatibility with the
separately restored old Russian source family135.

Decoded result records have stride **0xA8**. Their **+78/count+80** owns the
40-byte source item vector. On success, `047c50` uses **043ac0** to copy the
selected output record into **prepared block+20**; consequently the block's
source item vector is **+98/count+A0**, as consumed by **04fab0**.

`04fab0` concatenates recognized block items and explicit paragraph separators
into the final source vector. **043740** clones each40-byte item, preserving:

- the actual source identifier vector at+0/count+8 through the ink collection service;
- UTF8 candidates at+10/count+18, each16-byte text/length/flags record;
- item flag+20 and int32 kind+24.

The source vector becomes argument6 of **053c60 ->0531b0**. Existing item text
is resolved through the actual resource alphabet; prepared category bytes and
vertical profiles feed C1A. **04ffa0** has an additional contextual symbol
rewrite branch when driver mode+24 equals1 and alphabet features permit it.
It must not be replaced with a manual Unicode alias map.

The concrete EEF selected-path -> source-ID/position alignment construction
remains unfinished. The dispatch trace does not turn expected text labels,
arbitrary candidate records or a whole-word rectangle into genuine symbol
source regions.

## Proven result owner boundary

The separate [prepared result-line metric port](legacy-prepared-result-line-metrics.md)
closes a real source ownership gap: Text **050660** writes result2DB's line
records at+58/count+60/stride30, with each source pointer equal to
`source2DA.F8 + lineIndex*38`. Document **177590** follows that pointer and
narrows its **double+20** into SDK lineInfo **float+18**. The SDK/iink path then
uses this native source-line x-height in the prepared font policy.

Its exact one-step forward character-source-ID repair and cursor ranges are
ported and exhaustively tested. This metric is not source219/24A classifier
frame height; the confirmed source owner, rather than a similar field name,
establishes its semantics.

## Reproducible static check

`tools/handwriting/audit_recognized_source_line_pipeline.py` reads SHA-pinned
Document/Text PE files as bytes and verifies ten concrete descriptor/dispatch
table entries. It records the inspected producer chain and its remaining gaps
without loading either DLL. Private focused decompilation evidence is under
`D:/BlurMask-build-artifacts-20261011/DocumentEedDispatchNext`,
`DocumentEedDecoderOwnerNext`, `DocumentEeeDecoderSourceNext`,
`DocumentEeeMainDecodeNext`, `DocumentActualSourceItemBoundaryNext`,
`DocumentSourceItemCopyNext`, and `TextResultLineOwnerNext`.

```powershell
python tools/handwriting/audit_recognized_source_line_pipeline.py .artifacts/myscript-analysis/sdk/build/x64 D:/BlurMask-build-artifacts-20261011/recognized-source-line-pipeline-audit.json
```

The reusable metric/model APIs require only committed portable data, not these
private PE files. This auditor is optional preparation evidence, not a runtime
dependency or a native winner/recognition accuracy test.
