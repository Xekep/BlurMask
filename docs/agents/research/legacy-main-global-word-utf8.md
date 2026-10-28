# Global published words to raw UTF8

`LegacyFreshMainGlobalWordUtf8Projection.Project` reads an actual successful
global glyph-execution result. It retains the same materialization, association,
glyph pool and result-query lease. It does not construct an old single-source
result or choose new labels. Successful zero-span callbacks produce an empty
projection; a failed callback cannot be projected as a word.

Each flat WORD275 reference is resolved to its complete pool40 record, relative
compact descriptor and matching actual associated72-byte row. Ordinary sources
retain their original scored global arc and stored sampled ranges; merged
range ownership stays separate from that original neural source. Protocol keys
are preserved as protocol spans, without inventing an ordinary graph arc.
Returned records, row maps, ranges and UTF8 are copied.

The ordinary scalar/token emitter uses the actual acoustic alphabet. A nonempty
control descriptor or an alternate alphabet remains an explicit refusal until
its distinct owner is joined. Sampled range coordinates are not raw document
endpoint IDs. This API exposes raw glyph bytes, before0213A0/021000 filtering
and final document acceptance.

The default headless probe runs eight genuine global inputs,32 explicit caller
allocation/record/stack variants per owner:256 published words,256 zero-span
callbacks and1,024 cancellation/null controls. The independent raw-AK oracle
checks all record bytes, reading order, canonical source keys, associated-row
matching, sampled ranges and emitted bytes. The same core emitter/reference
oracle is used for the older source API. Native audits read pinned binaries;
vendor code is never loaded or executed.

An isolated committed1338a3c checkout plus this adapter passes Managed and
Native AOT with identical capture SHA256
`8ed3ebcaa1cb14602d02b54ea94c5f590760ba584ae8016b9d0ec085df92db0a`.
The optional recorded-input mode also matches across both runtimes.

```
LegacyMainGlobalWordUtf8.Probe AK engine separation case publication line30 line50 capture LK
python tools/LegacyMainGlobalWordUtf8.Probe/audit_output.py AK capture
python tools/handwriting/audit_main_global_word_utf8.py static-report.json
```

An optional `recorded-manifest word-index` pair selects one of the nine pinned
coordinate recordings. The loader checks commit, track hash, bounded points and
the explicit absence of pen-up metadata. It passes one continuous stroke per
track and does not read expected text. Default CI fixtures require no private
recordings. Recognition metrics, evaluated after inference, remain separate
from byte/ownership verification.
