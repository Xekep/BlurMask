# Fresh first word endpoints and glyph symbol production

`LegacyNativeGlyphSymbolUpdate` restores pool274.virtual30 at `13AF30` using the already owned record pool. It changes only flags04 low3bits and label08. Descriptor00, publication flags06, classifier scores and geometry pointers remain intact.

The external-variant byte takes precedence and produces kind4 with the supplied label. Otherwise count0 produces kind1/label1, even if the supplied symbol is FFFF or an ordinary label. These records contribute zero span through WORD276.virtual20 and remain present in reference count, reading order and publisher275's flat reference vector. They must not be replaced by a guessed space.

With nonzero count, macro symbols FFF9..FFFC produce kind1. The low two label bits retain `(symbol-1)&3`; the rest encode `((componentFlag<<13)|(count&1FFF))<<2` with native U16 truncation. The rejection at `13AFC3` is a **signed** count comparison against8192. Negative counts therefore remain accepted and masked exactly in this low-level API; this is not a policy allowing arbitrary negative counts from a recognizer. On rejection the record is unchanged. Other labels produce kind0.

`LegacyFirstPlainWordDelimiters` covers only endpoints of the checked ordinary no-protocol graph. Frontend table2DE610+1A0 dispatches152DA0 through graph21b.virtual130 to1AAE70. Ordinary no-custom source flags give19EB10 contextFFFF; seed1A1EF0 transfers compact group04 to arc18, and merge1A37B0 preserves it from the left arc. Graph reset1A7230 clears fallback metadata count208. Thus leading previousNode=-1 and trailing currentNode=NodeCount enter the empty-metadata endpoint branch: flagsC/D become0, the token buffer is untouched and capacity is unchanged. This derives **emittedcount0**, rather than substituting a token label. The API validates the other endpoint's actual graph cell. Interior separators, custom token metadata and geometry-driven punctuation remain unsupported.

The fresh23f constructor1EDB60 has an explicit I32 write at **1EDC52** clearing object.FC, separately from its U16F8 initialization. `FreshDescriptorIdBase` is therefore0 for the fresh constructed first controller. Descriptor identity remains independent from the symbol label and selected alternative. WORD276.virtual30/13F370 forwards its actual last flag to pool.virtual20/13ACC0 (force duplicate); virtual28/13F410 supplies0. Materializer policy and subsequent source-specific identity assignment remain the caller's responsibility.

Reproduce without running vendor code:

```powershell
python tools/LegacyGlyphSymbolUpdate.Probe/reference_controls.py controls.json
dotnet run --project tools/LegacyGlyphSymbolUpdate.Probe -- controls.json models/ru-RU/ru_RU-ak-cur.lite.res
python tools/handwriting/audit_first_plain_delimiters.py --output audit.json
```

1694 independent packing controls cover all macro labels, flag overflow/truncation, signed count extremes, zero-count and external-variant precedence, plus84 rejected unchanged records.880 ordinary graph endpoint checks and a composed pool→WORD276→publisher275 control verify3 retained references/1 actual symbol. Nine own printed MIR scale/translation inputs additionally exercise the actual6120 resource frame/cuts/ordinary graph and factory-owned capture/order flags for18 zero-span endpoint updates. These last fixtures do **not** test glyph classification, Russian accuracy or the main240 linguistic decoder. Portable capacity/ownership and cancellation checks are retained. Managed and Windows Native AOT results agree.
