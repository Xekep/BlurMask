# Older Math: the actual 13EC normalized clone

This static continuation explains the required stage after the matched-root candidate tree. It adds this document and an auditor, with no recognition runtime, model selection or app changes. The report deliberately keeps `applicationReady=false` and `normalizationImplemented=false`.

The new evidence covers 91 complete Ghidra functions/regions, twenty exact straight return leaves, one exact name-reset tail thunk and one separately labelled guarded primary-API lookup extent. It pins 1,789 instruction locations, 43 data bindings and 22 literal strings. None of the leaf/thunk/lookup extents is counted as a complete Ghidra function. The preceding candidate-production and actual grammar audits are composed. Vendor DLLs were never loaded or executed.

## Actual owner and API dispatch

The retained Math1388 worker derives from2D1F40. The preceding proof establishes its actual13EC service and038890 result method. The grammar output lives at the base +A0. A different cached clone lives at derived +168; scratch pointers/count/capacity occupy +158/+160/+164.

The native Type API V28 is037EF0. Its guarded class-registry lookup obtains the actual registered descriptor, follows descriptor +18 to class metadata, then reads metadata +8 for the primary methods. Missing registry/class/metadata paths return null. This exact disassembly extent includes both real returns and every internal branch; it is not labelled as a complete Ghidra body. The six actual Math class method tables and three actual2D base tables are pinned separately. Therefore the indirect typed operations below are tied to real registered classes, not inferred from adjacent public ABI callbacks or a coincidental string result.

138D/138E represent terminal/nonterminal-root alternatives;138F owns ordered expression children;1393 derives from138F and carries table dimensions.1391 and1392 derive from138E and have distinct cell/border coordinate operations. Their actual descriptors, constructors and primary tables are included. These lenses are intentionally distinct: for example,1393's specialized table methods shift its later slots relative to generic138F, while1391/1392 expose their own coordinate getter at +58.

## Clone before rewriting

038890 returns +168 if a normalized result is already cached. Otherwise it obtains +A0 through the real base1F40 V40 getter, clones that object through the actual object API and acquires its reference. The original +A0 tree is not rewritten in place. If the clone is138E, it executes seven calls in this exact order:

1. 0370A0: table/columnar structural preparation.
2. 037A60: carry-up/down rewriting.
3. 037E60: partial-fraction rewriting.
4. 0386A0: collapse `identity list` wrappers.
5. 0386A0: collapse `identity vertical pair` wrappers.
6. 037680: rebuild nested `vertical list` structures.
7. 0386A0: collapse remaining `vertical list` wrappers where its guarded rule permits.

These are five distinct entry functions, not seven unrelated algorithms. A138D clone skips that138E-only sequence. Any false status prevents successful caching; the owner releases the clone and returns zero. On success it stores that clone at +168. No CAS evaluation or equals-triggered calculation appears in this stage.

The deep-copy path is concrete. Math040AF0/00C6E0 copy their derived138D/138E metadata and delegate the actual1F44/1F42 base copies.1F42 copy004560 copies name bytes, selected scalar and both12-byte range vectors, allocates a separate alternative-pointer array and clones its child objects through their actual registered classes.1F43 copy004BB0 similarly owns its copied name/children and stored scalar/flags.1F44's nested candidate/symbol deep copy remains covered by the preceding storage audit.

Math040970 copies138F metadata before its base1F43 copy. Table copy00CBC0 copies its table dimensions with that base; cell/border copy00BF60 copies their coordinate fields with1F42. These are class-specific copy rules, not a blind memcpy of a generic object size or proof that every possible SDK subclass is supported. The actual generic allocation/custom-provider/string/reference internals are still separate from this bounded tree proof.

## Structural passes and direct helper closure

The twenty-four scoped structural functions form a closed **direct** algorithm graph apart from the two common memory primitives08E100/08E4C0. The report records the graph. This does not imply that arbitrary indirect class/allocator/string providers have all been recovered.

| Function group | Actual role in this stage |
| --- | --- |
| 0370A0,037A60,037E60,0386A0,037680 | The five ordered entry passes, with recursive traversal |
| 036500,036B50,036EB0 | Prepare table/columnar children and resulting dimensions |
| 037370,038230 | Gather/rebuild list children and collapse a guarded named wrapper |
| 0348D0,034990 | Construct named expression and alternative-root wrappers through actual typed constructors |
| 035490,035010 | Gather actual row/border-related nodes using retained structural metadata |
| 035990,035DA0,036200,036780 | Aggregate ranges, combine row/cell structures and construct table/border pieces |
| 033B10,034710,034AE0,034B70,034C00,035B50 |12-byte vector append, selected terminal resolution, line-name checks, coordinate sorting and typed child insertion |

0370A0 traverses all alternatives of an138E node and ordinary expression children recursively. Table class1393 is handled separately, including the actual name distinction between `table` and columnar operations.036500 gathers actual nodes for each row, creates cells/borders and later036EB0 obtains their coordinates to compute table dimensions. The current resource/search/request evidence still determines which of these branches can actually be reached; the existence of a branch is not recognition-quality coverage.

037A60 renames `carry up` to `overscript` and `carry down` to `underscript`. It takes the existing first child, clears the old child list, and installs a null base placeholder followed by that child before recursion. The empty slot is significant. It must not be replaced by an invented token or discarded as redundant.

037E60 rewrites `partial fraction numerator` to `fraction` with a missing denominator. For `partial fraction denominator` it preserves the original pieces in the native reordered children and inserts the missing numerator slot. It then recursively processes the resulting child structure. This is structural normalization of already supplied recognition data, not integral evaluation.

038230 collapses a matching named expression only under its single-child guard. It keeps cell/border parents1391/1392 as explicit exceptions. It follows the actual selected alternative of the child root, transfers the selected child into the parent's alternative slot and preserves the wrapper's raw score field in the resulting selected terminal candidate or expression scalar. It does not select a more convenient recognition alternative by an expected label, and it does not calibrate normalized weights as confidence.

037680 gathers nested vertical-list members into the worker scratch array, protects their references, releases/clears the old children and rebuilds the list in native order. It then recurses through rebuilt children. Traversing only the visually selected leaf or sorting all children alphabetically would change this algorithm.

## Copy, adoption and scratch lifetimes differ

The base APIs have materially different insertion contracts:

| Actual base operation | Native effect |
| --- | --- |
|1F42 V48 =0049C0 | Append a newly cloned supplied alternative, then acquire its reference |
|1F42 V50 =004AD0 | Append the supplied pointer, without cloning or acquiring it |
|1F43 V30 =004F20 | Append a newly cloned child, then acquire its reference |
|1F43 V38 =005020 | Append the supplied child pointer, without cloning or acquiring it |

The latter operations implement adoption/transfer within these callers. They must not be replaced by a universal clone-and-retain helper. Conversely, returning a borrowed scratch array as an owned result would lose lifetime authority.037680's explicit reference protection before releasing the old children is essential to its subsequent adoption rebuild.

The scratch array at +158 contains actual temporary node pointers; entries are not independently owned merely because they are present in that array. Particular passes reset +160, append/gather nodes and arrange their own reference transitions. Destructor034430 frees the scratch **array**, releases normalized root+168 and delegates base destruction, which releases +A0 separately. The two roots and scratch storage have different cleanup responsibilities.

0344B0 resets scratch count and releases/clears the normalized cache only while the inherited +154 flag is zero. The first forwarded13EC mutators034540/0345A0/034600/034650 simply call their actual1F40 base operations; they do not establish an unconditional cache-invalidation policy.0346A0 tests whether either cached+168 or base result exists. Base reset003FC0 clears the associated constraint counts and resets +154. Initial flag/cache values and later active source state cannot be silently replaced with a generic “always fresh” assertion.

## Coordinates are table bounds, not font predictions

The actual cell getter00C6A0 copies +78/+7C into its output row bounds and +70/+74 into output column bounds. Border getter00C290 copies the same four stored scalars in its own defined order. Table getter00CE10 reads dimensions+48/+4C. These are independently labelled exact leaves, not approximate inferred struct layouts.

036500/035DA0 group nodes using their retained metadata, assign cell bounds and compute table dimensions through036EB0.034C00 sorts the gathered pointers by the actual coordinate keys. These positive uses explain the row/column role of the stored fields in this stage. They do not establish a neural font-size/baseline model or justify reusing them as application text size.

The original stroke/source range vectors remain a separate concern.035990 collects both real12-byte vectors through the typed node traversal;033B10 appends twelve-byte elements. Native `undefined1[32]` pointer notation is not a32-byte stride. Existing source-position interpolation and original/selected registry identity requirements remain unchanged.

## Failure and integration limits

Some native helper failures occur after pointers, vectors or children have already been modified. Both helper and caller can issue release operations on false paths, and the generic reference-mode details are not fully established here. No atomic rollback, safe retry, complete native allocation-error parity or recovered observer/history acceptance is claimed.

A future owning implementation must retain the actual produced tree/request/source authority, distinguish clone/copy/adoption, preflight whole budgets and define a poisoned failed entry with single logical cleanup. That is a separate library adaptation requiring the original source and matcher joins; a synthetic tree, prepared labels or a coerced modern EE4 receipt would not prove it.

The auditor verifies exact native bytes, complete recorded body regions, explicit leaves/thunk/lookup, all class/API bindings, direct call graph and the seven-call order. It composes the candidate-production auditor and all preceding actual resource/data checks. It is static verification, not execution of a vendor implementation or a runtime recognition oracle.

Run `python -I tools/handwriting/audit_math_legacy_normalized_clone.py REPORT.json --native-repo SDK_REPOSITORY`.

Still open are full AK operators, matching/scoring and actual request/source admission, the source-specific route into this worker, complete generic Engine reference/allocation/string semantics, owning runtime normalization and final page acceptance. This stage is now described and statically pinned; it has **not** been implemented in the independent runtime. Russian recognition, app readiness and the whole engine remain incomplete.
