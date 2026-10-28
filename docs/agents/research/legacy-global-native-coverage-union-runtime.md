# Actual global0248D0 canonical WORD coverage correction

This bounded correction follows the separately frozen `global-native-coverage-union-plan-static-frozen-manifest-20261021.json` proof (SHA256 f014e6c06c134c642dfaa78418fb379ada381aec81d523cffa214d5e5ba543c6). Native0248D0 builds a local C21 by appending each WORD key through VB0, then compares its canonical union with exact TEXT through VE8. Comparing the unmerged raw key vector incorrectly rejected MIR and II. The new runtime uses the already verified same-Document C21 Union implementation before that comparison.

Only `LegacyFreshMainGlobalDocumentCoverage.cs` changes production behavior. Raw WORD key, interner identity, `CopyWordRanges()`, selection ranges and the ordinary-or-raw CHAR subtraction fallback are preserved. A distinct immutable normalized coverage view is returned through `CopyNormalizedWordCoverageRanges()`, with the same existing lifetime validation and a defensive copy. It grants no source ownership or mutation authority. There are no old selected/index/cleanup/transform/counter production edits and no app/default/DOM/acceptance integration.

The union has a conservative n² envelope, computed in long and rejected above16,777,216 before union execution; the accepted envelope is also charged against the existing cumulative CHAR-subtraction budget. Negative/unbounded scalar counts and the inclusive4096 versus4097 boundary have explicit internal numerical controls, without manufacturing production parents. The old retained-range and comparison caps remain. Union and subtraction operate only on local copies and pass cancellation through to the existing kernels.

Both Managed and Native AOT complete all13 exact coverage cases with37 forward CHAR subtractions and84 guards. MIR's raw3 ranges and II's raw2 ranges canonically join their respective TEXT singletons. All13 therefore take native equal-fast-skip here: no previously initialized nonempty2DB owner is borrowed and no compute-first policy is introduced. The standalone timed2DB component and its V2 shared-metadata behavior are unchanged.

Later stages admit12 owners and explicitly refuse MIR when its actual second CHAR reaches the literal01FD40 C25 raw-object lookup. No source class is inferred from bounds. The new probe catches only that precise NotSupportedException message, verifies unchanged Seed identity, string-pool count/bytes, source scalars/keys/record bytes, WORD/line bytes, pending commands and counter, and records `C25`. Other unsupported exceptions still fail. II genuinely runs through selected, indices, cleanup, transforms and the existing counter handoff.

Independent Python oracles compose the unchanged actual13 CHAR/WORD/TEXT byte/source proofs, canonical linear C21 operations and exact lazy selected predicates. New source flags, ordering, AVL intervals/pages/memberships and failure prefixes are checked numerically; expected recognition labels are never inference inputs. Actual results:

| Stage | Cases / numerical outputs | Guards |
|---|---|---|
| coverage |13 ready,37 subtractions,16 raw WORD entries→13 canonical entries|84|
| selected |12 ready +1 C25 refusal,34 CHAR,34 ordered writes|80|
| indices |12 ready,46 source rows; prepared40 AVL cases unchanged|201|
| cleanup |12 ready,34 CHAR,74 records;5 cancellation prefixes|186|
| transforms |12 ready,13 identity matrices;5 cancellation prefixes/3 failed exports/2 late invalidations|128|
| counter |12 ready,58 original selection commands +12 trailing operation7 commands|75|

The static proposal predicted12 matrices by counting owners. The independent runtime oracle corrects that prediction: II's one canonical TEXT interval spans two original raw objects, so raw point-count evidence yields two fragments and two matrices. No transform production code changed. The new oracle derives those fragments from original per-object counts and source interval endpoints rather than captured matrices.

Coverage tests validate cancellation before preparation without changing the full original-owner snapshot, defensively copied normalized/key/line arrays, raw WORD identity, stale/disposed normalized access, poisoned counter state and scalar union-budget boundaries. Existing downstream stale/dispose/cancel/entered-failure controls are retained. Guard increases are derived in the oracle comments from the extra successfully admitted owner and actually executed guard calls, rather than unexplained golden edits. Metadata and command history remain immutable after read-only factory refusals.

Fresh wrapper13 and WORD/TEXT13 producer captures are byte-identical to the prior frozen evidence, in Managed and Native. For each of the six changed stage captures, all11 formerly successful per-case objects remain exact (excluding the newly added normalized field only for coverage). Historical prepared/alias/failure collections remain unchanged. `audit_global_coverage_union_regressions.py` checks these comparisons and all six Managed/Native byte hashes. Source-index prepared controls still cover19,960 AVL nodes,14,200 memberships and all rotation directions.

The runtime static auditor now checks normalized comparison, separate raw fallback/getter, additive budget and unchanged provenance contracts. Its selected-source composition resolves the sibling runtime coverage auditor from the same checkout while taking vendor-native proof dependencies from `--native-repo`; this avoids accidentally checking another checkout's obsolete raw comparison when the vendor DLLs are stored separately. Native binaries are inspected read-only, never loaded or executed.

Build inputs are isolated at base df8a04c84d532f8022d60aeba14f0b63c27d903f. The source manifest lists the one production edit, six probe/oracle pairs, two static-auditor edits, new regression comparator and this document. Evidence records fresh captures, composed oracle output, static dependency outputs and exact old11/prior13 regression reports. No main/CI edits or commit are part of this package. MIR C25 service, actual unequal0238B0, full recognition quality and final DOM acceptance remain separate incomplete work.
