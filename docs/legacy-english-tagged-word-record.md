# English tagged known-word records

The internal `TaggedWordTables` companion reuses the initializer's immutable
English model tables to evaluate known words with zero, one or two previous
classes. It combines the existing sparse unigram/bigram/trigram calculation
with the actual category-pair/triple tables and writes the known fields of a
40-byte language record. This is a numerical component; it issues no source
token, graph history receipt, query lease or recognition candidate.

`TaggedWordInput` is immutable numerical transport. The future live wrapper
must supply its word, category, tagged history, boundary flag, category
suppression/clearing policy and lexical budget from the same real invocation.
`IsFor` checks the exact initializer table owner. Two small reference vectors
are retained once; no code/cost model payload is cloned per word. Evaluation
uses local scratch and does not mutate input history or model tables.

The pinned resource has order3 and casePathVersion0200. Its category history
is the leading nonzero nibble sequence in bits24..27, distinct from class
history in bits0..23. Clearing the first tag occurs before counting that
prefix. Category0 disables it. Ordinary category costs use the pair/triple
table or the original fixed fallback. Suppression contributes no category
cost and preserves the old tag prefix instead of appending a new category.
Class and category histories shift independently, as in native0FFEC0.

Native0FFCF0/0FFEC0 and the known-word branch of100090 were inspected as static
PE bytes. In particular, record+4 is matched order in the known-word branch;
the separate earlier branch writes historyCount+1 and must not be substituted.
The output writes cost, matched order, count, up to two history cells, state-1
and the source word ID. Its write mask preserves allocator residue elsewhere.
The native module was not executed.

The scope is the fixed English resource, ordinary known words, no optional
trace/vocabulary transport and a final-cost lexical budget check. It excludes
unknown-word handling, optional native request modes, native cache/allocation
failure chronology and source-policy selection. Native early pruning is not
reproduced as an observable failure-prefix contract.

The managed probe and a separate Python raw-resource reference agree exactly
on57,096 complete records, including poisoned allocator residue and all
category tags0..12 with the boundary/clear/suppress combinations. They include
43,305 unigram,13,766 bigram and25 trigram matches, nine malformed-input/budget/
cancellation checks and seven rejected corruptions. No model inference is
performed. Full capture SHA256:
`a30448e9286013810b31b9b82f45aecc642a4254ef5eb2903df7f2536815112e`.
This proves the checked numerical records, not English handwriting quality or
integration into the whole-word controller.

```sh
dotnet run --project tools/LegacyEnglishTaggedWord.Probe -c Release -- models tagged.json
python -I -B tools/LegacyEnglishTaggedWord.Probe/audit_reference.py models tagged.json reference
```
