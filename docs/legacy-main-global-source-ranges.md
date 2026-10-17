# Shared main graph source references

`LegacyFreshMainGlobalSourceRanges.Prepare(actualCompactGraph)` binds the same
post-cross/prune [compact graph](legacy-main-compact-ordinary-graph.md), shared
cut chronology and original current main source. It resolves ordinary forward
mode0 `21B.V120` references across actual descriptors. It does not manufacture
global atom IDs, substitute acoustic group ranges for the decoded source path,
or run separate line samplers.

`1A9AE0` starts with the first matching span0 arc at the requested starting
context. `1A9070` follows outgoing selector1 entries in existing compact order,
advancing one node. Failed paths roll back before the next alternative. On the
fresh ordinary kind0 domain this visits source arcs, not merged span candidates.
The native emitter `1A8E60` resolves each selected group's ranges through
source218.V90, VA0 and VC8. All source endpoints and fixed-point residuals retain
their actual original metadata owner after global cut insertions/reordering.

The previous `LegacyOrdinaryGraphSourceRanges` now shares its unchanged private
projection with an additive internal entry. Sorting and reverse coalescing
still use native start/end key order, remainder bytes, original F8 record
adjacency and ending metadata. Existing public APIs and expected results are
unchanged. Metadata key triples remain source keys, not document object IDs.

The new probe executes seven actual raw owning inputs and211 candidate source
callbacks, including five cross-descriptor arcs. An independent recursive
Python implementation verifies the path, sorting, coalescing and both residual
bytes against owned metadata captures. Earlier prepared/deferred raw source
oracle controls remain a regression for the shared projection; no expected OCR
text selects the path or output.

The old576 prepared raw/deferred cases and18 actual first-source cases retain
byte-identical10887 callback captures in Managed and Native AOT. Their separate
oracle verifies8669 reverse merges and2296 duplicate-tail metadata differences.

The supported endpoint is bounded forward ordinary mode0. Reverse contexts,
protocol/type1/type2 source groups and extra graph sources are separate native
branches. The portable path limit is4096 ranges and DFS limit1048576 work steps;
invalid/canceled calls publish no partial result. Global227 contexts,
descriptor-specific normalization/NN and final multiline decoding remain
separate. These checks do not establish handwriting accuracy.

Probe arguments: AK, engine, separation, case, publication, line30, line50,
capture. Python auditors are `tools/LegacyMainGlobalSourceRanges.Probe/audit_output.py`
and `tools/handwriting/audit_main_global_source_ranges.py --output report`.
