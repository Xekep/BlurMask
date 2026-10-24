The plain publisher now has an internal13FE20 entry point independent from
279 list-node publication. `InternPlainWord` preserves the older public
`Publish` behavior, including exact64-bit head-reference deduplication,
complete120-byte payload ownership, all zero-span linked references and
publication-flag aggregation. It does not create another279 node when an
alternative is appended into an existing275 span pool.

`CaptureSpan` captures one existing exact-span entry and actual selected279
nodes. The caller supplies active same-TOP heads in native traversal order;
fork creation order is never substituted for TOP order. Heads are validated
against the identical publisher/fork owner, and overlapping or cyclic279
lists are refused. The snapshot exposes read-only actual WORD records,
their immutable flat-reference sidecars and original selected ordinals.
Native278's142A50 lookup returns an existing owner only when both signed
First and Last equal the requested key. Its end-group skip links do not admit
containing spans. See [the exact-index correction](legacy-main-span-exact-index.md).
This does not establish the remaining multiword ownership and ranking lifecycle.

`ApplySpanRanking` accepts the separately verified ranking kernel's full
120-byte output, payload-source permutation and old-index map. Every payload
byte follows its source ordinal; only the native special cost sentinel at16
may differ. Source flat-reference vectors follow payload swaps too. The
method rebinds every captured279 node to the selected new ordinal and then
replaces the275 list. It does not rederive vectors from a possibly changed
glyph chain, compare source labels or infer native winners.

`ApplySpanScores` is a second controlled write after ranking: only bytes16
through23 may change, with unchanged payload order, flat-reference identities
and selected ordinals. It does not calculate140B90. Static registration
shows that13D360's final callback is275.V08=140B90, not a no-op. The separate
owning ranking bridge must still execute that native finalizer.

An apply attempt consumes its snapshot before entered validation. A stale,
malformed or otherwise failed attempt cannot be retried using that snapshot;
there is no rollback claim. Already-canceled calls refuse before entering.
The old immutable records can remain readable as snapshots. Native276.V08
13F080 frees a removed WORD's own flat/auxiliary vectors, not shared274 glyph
records. In this plain no-auxiliary lineage, GC owns those vector allocations
and the same TOP glyph pool survives ranking unchanged.

Managed and Windows NativeAOT checks use1024 prepared exact-span owners,
6698 complete WORD records,2823 selected279 nodes and1954 removed payload
sidecars. Independent Python checks every120-byte row, output flat-reference
identity and selected ordinal. Controls cover exact-head deduplication after
changed builder scalars, append without extra279 nodes, fork ownership,
payload permutations, score-only updates, canceled-before-entry calls,
stale snapshots, invalid payload bytes and poisoned retries. They do not
claim genuine219580 candidate generation or full TOP.V68 completion.

The existing public TextChoices path is byte-identical in Managed and
NativeAOT after the extraction (seven real raw candidates,26 glyphs,64
alternatives,52 selected bytes,35 guards; SHA256
`eb2c197d59781b75805cdcf029a84baf069f97fed0012978880bc219b730ebfc`).

```powershell
dotnet build tools/LegacyMainWordSpanStorage.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/span-storage-build
dotnet E:/BlurMask-build-artifacts-20261014/span-storage-build/bin/LegacyMainWordSpanStorage.Probe/release/LegacyMainWordSpanStorage.Probe.dll E:/BlurMask-build-artifacts-20261014/span-storage-managed.json
python tools/LegacyMainWordSpanStorage.Probe/audit_output.py E:/BlurMask-build-artifacts-20261014/span-storage-managed.json
python tools/handwriting/audit_main_word_span_storage.py E:/BlurMask-build-artifacts-20261014/span-storage-static.json
```

The pinned PE auditor confirms actual275/276 registration,13FE20 full-payload
copy/head identity,13F080 vector destruction and13D360's279 ordinal rewrite.
It reads vendor bytes only; no vendor binary is loaded or executed.
