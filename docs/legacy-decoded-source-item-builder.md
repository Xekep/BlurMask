# Actual decoded source-item construction boundary

This static audit follows the modern Document sequence decoder into the source
items consumed by the restored character-line fitting stage. It establishes
the missing postdecode caller and source-identity service. It does **not** claim
an executable Russian winner, raw symbol segmentation or live native font fit.

## Concrete producer order

Document **047c50**, the EED recognition callback, first invokes decoder
EEE.virtual28 (**03a260 ->038610**). Only after success does its direct call at
**04844c** run **046ae0**. That function iterates the decoder's **0xA8-byte**
records and calls **045c60** at **046da2**. The later direct call **04846f**
copies the selected record through **043ac0** into the prepared block.

Thus the source-item vector **record+78/count+80** is constructed by the
postdecode callback; it is not an output vector already emitted directly by
EEF's beam operation. EEF.virtual40 resolves to **03da20** and merges decoder
state/labels; treating that callback as the source-item constructor is incorrect.

`045c60` initializes40-byte items through **043650**. Its proven inputs are:

- packed tokens **record+50/count+58**, low16 bits label, byte2 branch selector,
  byte3 alternate-candidate eligibility;
- optional item kinds from **record+40**, gated by decoder+3c;
- up to decoder+5c alternate slots from **record+98/count+A0**, each8 bytes
  containing a ushort label and float32 at+4;
- the actual alphabet at EED+1d8 and its explicit alphabet limit+1e2;
- separate byte-string offset data EED+1f0/+200/+208 for the nonalphabet branch.

Alphabet labels strictly between0 and the limit are resolved through
alphabet.virtual30. Label0 emits no candidate. Out-of-range nonzero labels
emit UTF8 U+FFFD (`EF BF BD`), length3, metadata0. The byte2 branch instead
resolves a byte-string interval through the prepared offset vector. Text
candidate records are16 bytes: owned text pointer, byte length, and opaque
32-bit metadata. The latter is copied directly from alternate float storage;
this audit does not label it calibrated confidence or reinterpret it as flags.

The constructor leaves each item's source-identity collection empty, sets
item flag+20 to0, and sets kind+24 from the optional kind vector or−1. It clears
the used alternate count+A0 after processing. Candidate construction alone
therefore supplies no glyph rectangle or pen source IDs.

## Concrete source-position owner

After candidate construction, **046ae0** can map record+68/count+70 pairs into
the source collection. Each pair contains item index+0 and position endpoint+4.
It starts the previous endpoint at0, invokes **C79.virtual30** for the previous
and current endpoint, and appends the resulting identities to item+78 when
the signed item index is nonnegative. The previous endpoint is updated even
for a negative item index. This branch requires the supplied C79 owner and
decoder flag+51; those conditions must not be guessed from application mode.

The pinned Ink C79 descriptor is **0dec30**, its service table **0dec80**, and
virtual30 is **010850**. Its position-range mapper owns:

- original source identities at+10/count+18;
- prepared position identities at+50/count+58;
- sorted separator positions at+60/count+68;
- a prepared range-to-source-collection cache at+98/count+A0.

A full0..count range returns the original source collection. Equal endpoints
return empty. Other ranges have explicit separator adjustments and cached
range expansion, rather than a simple stroke-index slice. Raw pen points,
bounding rectangles and classifier frame heights cannot stand in for this
prepared position owner. The actual snapshot construction remains to be
restored before exposing a default raw-ink adapter.

## Resource compatibility

EED's **0486b0** reader identifies its layout as `SequenceRecognizer`. It reads
an ancestor133 header, alphabet, a nested pipeline processor, a constrained
mode0/1, and separately present interface50 model objects. Fresh construction
**044630** likewise receives explicit model references; it does not extract
them from the old Russian source automatically.

Ink C6D serialized reader **0150c0** loads a `pipelineDescription` through
service BC1 and derives processor nodes. EF0's **03dc50** prepared constructor
requires exactly24 bytes with three explicit model references. EF1's
**04a370** requires64 bytes and then **049630** binds named transformer
encoder/decoder inputs and outputs, including `src_len`, `downsampling`,
`strk_ends`, `output_seg`, `attn_seg`, `cross_k` and `cross_v`.

The committed Russian acoustic root is247/v10d, with a separately recovered
old source135/classifier/lexicon decoder layout. A shared133 ancestor and
alphabet API do not prove that resource satisfies the modern SequenceRecognizer
reader or named tensor interfaces. Absence of modern node headers inside247
also does not disprove a separate SDK adapter: that adapter's caller and
resource transformation have not been located. Both a direct old135 mapping
and an asserted automatic rejection of every possible old model would exceed
the current evidence.

## Reproduction and remaining work

```powershell
python tools/handwriting/audit_decoded_source_item_builder.py .artifacts/myscript-analysis/sdk/build/x64 D:/BlurMask-build-artifacts-20261011/decoded-source-item-builder-audit.json
```

The auditor verifies12 SHA-pinned direct-call and descriptor/table bindings,
using only Python's standard library. It never loads a DLL. Detailed private
read-only exports are in `DocumentActualDecodedItemBuilderNext`,
`DocumentDecodedSourceItemsProducerNext`, `InkC79DecodedSourceIdsNext`,
`InkC6dCompatibilityNext`, and `DocumentEefSourceEmissionNext` under the same
private artifact directory.

The next integration boundary is actual winner/position generation and C79
snapshot preparation, followed by source-ID-derived per-symbol geometry into
**0531b0** and C1A. Existing prepared catalog/IRLS/result-owner ports do not
make the current connected-word bounding-box fallback a native line x-height.
