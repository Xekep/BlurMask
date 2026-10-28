# Next live global ordinary glyph refresh and emission

This read-only checkpoint follows successful
`LegacyFreshMainGlobalNoCandidateSourceCompletion`. No runtime or model changes
are included. It does not reinterpret imported control2A IDs as acoustic labels.

## Exact live owners

The same successful source completion retains its pre-NN scope and input binding,
Document, selected actual global result and completed callback session. The next
owner must retain these identities together:

| Native role | Existing actual global owner |
| --- | --- |
| TOP273 original274 pool | `Source.Session.Ownership.GlyphPool` |
| frontend/240 and current compact2460, count2470, starts2478 | `Source.Selection.Publication` and its exact Policy |
| V140 source keys/ranges and originalLast | `Source.Session.Associations.Metadata` and `.SourceOverlay` |
| prepared glyph geometry and primary category owner | same Ownership.Geometry/Policy/Graph |
| frontend158/1B8/1C0 | `LegacyFreshMainGlobalGlyphScoring`, already used by that session's ordinary producer |
| live primary AL | that Policy.Owner.Model.AcousticAlphabet, identical to owned CasePreparation.Alphabet |
| selected input and3E | same pre-NN scope Context/InputBinding/SharedState/Seed |

No old configured CandidateSource/RawResult, SingleLine graph, independently
sampled source or detached imported pool is necessary. Global group allocation
IDs, descriptor IDs, graph context keys, raw document object IDs and selected
C51 IDs remain separate domains.

Native273.V40=13C6E0 normalizes the source key, looks up TOP48's actual owner,
tests owner18 bit4000, invokes actual controller.V90=217670 if needed, and ORs
4000 even when that callback returns false. The failed first call does not run
13A830. A later native call may select the partial owner because4000 is set.
Existing portable refresh APIs deliberately poison failed epochs instead; the
new route must state that same bounded refusal rather than claim native failed
refresh replay. A native64-byte owner header is not a40-byte glyph row.

## Ordinary217670 writes

Preparation152A70 calls graph138 and acoustic158. In this exact actual v106
source, sampler100 is null and1C2530 only invalidates last-group3C2=FFFF. Existing
immutable cached group values can therefore be reused; there is no new NN call.

For nonnegative source keys the genuine global V140 metadata provides both the
target owner and original base key. Count/fill owns24*N source references plus2*N
residual bytes. Higher source ranges remain distinct from originalLast's NN and
compact identity. Metadata must come from the exact finished session overlay;
reconstructing a new overlay would lose selected residual ownership.

Read first-candidate vector10[0] via frontend158 and retain it as source-owner1C.
Scan only the base node's complete current compact interval in stored order.
Participating rows require compact20 bit20, low subtype0 and FULL compact08
matching the base key. Descriptor ID is current compact index minus node start,
not a symbol or a group number. Append/dedup before resolution, preserving
allocated record40 residue and existing chain links.

Resolve216860 with conversion0, knowledge99 from the scoped fresh factory and
both neighboring labelsFFFF. The global ordinary producer already proves no
wide pairs/no Arabic script and implements the exact replacement callback.
Reuse that scalar resolver without216BC0's unit-score shortcut. Emitted symbol
updates the actual target record; normalized-original controls score10 and the
FFFF-ordinal cost request16. Write score10 before category/cost resolution,
then cost14, existing compact.byte21/255 into0C and descriptor04 bit8 from
compact20 bit40. Do not rerun normalized candidate24.field0 generation.

The source-owner1C, glyph0C, glyph10, glyph14 and WORD normalized scores remain
different channels. No one of them is an application correctness probability.
A typed source-header sidecar can retain native28/30/38/1C/refresh state without
fabricating native pointers, AVL layout or a glyph40 field1C alias.

## Actual reachable evidence

A separate E-only Managed diagnostic ran the already frozen global12 library
on the three raw shapes used by the pre-NN probe, after real V68. It observed
five positive ordinary WORD owner references and ten matching current compact
rows, no negative reference and no missing category. Every initial selected
glyph descriptor ID differs from every matching current compact descriptor ID.
Consequently emitting only the selected record or replacing the owner with
selected-only records would omit real native refresh alternatives. All already
computed NN counts remain unchanged. This is a read-only reachability check,
not an executed refresh, Native capture or quality evaluation.

The next probe must preserve initial rows and prove all new full40-byte rows,
ordered query/write traces, descriptor IDs, source26N/header1C, exact13A830
selection and marker. Actual higher selected sources are not established by
these five owners. Separate real-overlay prepared controls may exercise
higher target/base separation without being called native winners.

## Live V50 emitter boundary

Use the actual live acoustic AL passed to the native glyph helper. TOP18 is its
separate fresh empty control2A; TOP20/30 contain the separate copied alternate
vectors.13C920 passes a NONNULL alternate object even when empty, so kind4 must
fail when its offset table has fewer than two entries. There is no primary-AL
fallback for that case. The fresh raw/first720/global path excludes external
165D60 string insertion and V30 import. Existing static fresh-source proofs
cover their real constructors, copy and insertion callers.

The new emitter can reuse `LegacyNativeCircledUtf8Writeback.Emit` per glyph,
using the validated normalization/payload owners. It must validate the current
same-pool record and lease before and after emission. It must preserve the
actual empty-control lookup behavior and refuse unsupported malformed UTF8/C2
codec or missing alternate data without repair. No whole-WORD NFC and no
postimport dynamic2A substitution are permitted.

After refresh,13A830 must process the FULL owner including original records and
all appended alternatives, retaining the original signed alternative marker.
Selection allocation failure can retry selection over a successful cached
refresh; a failed refresh itself remains poisoned. Zero-count/macro records
must follow actual wrapper skip predicates, never a blanket protocol filter.
A future whole-WORD publisher must refuse an unsupported negative source before
claiming it completed, rather than silently discarding the reference.

## Proposed implementation files and scope

The smallest useful next runtime batch is:

- NEW `LegacyFreshMainGlobalOrdinaryGlyphRefresh.cs`: actual successful V68
  source/session/overlay-bound refresh owner and alternative selector.
- NEW `LegacyNativeOrdinaryGlyphRefreshKernel.cs`: mechanical internal extraction
  of the existing compact scan/update loop. Private owned adapters supply the
  actual resolver and scalar/cost callbacks; no public prepared success inputs.
- Narrow `LegacyOwnedMainOrdinaryGlyphRefresh.cs` delegation to that kernel,
  preserving old raw/configured factories, claim timing and failed-prefix scope.
- Narrow internal scalar accessor on `LegacyFreshMainGlobalOrdinaryGlyphProducer`
  and same-session producer/scoring accessor on GlobalGlyphExecutionSession,
  sharing current caches. Public old APIs remain strict.
- NEW `LegacyFreshMainGlobalGlyphUtf8Emission.cs`: actual live pool/AL, fresh
  control/alternate owner policy and complete per-glyph default helper.
- NEW dedicated probe/portable byte+trace oracle/static audit/docs. The old
  refresh/scoring/emission/selection captures must remain byte-identical in
  fresh Managed and Native builds; the source-order auditor must follow the
  mechanical kernel extraction without weakening native pins.

Proposed public surfaces retain actual typed owners only:

```
LegacyFreshMainGlobalOrdinaryGlyphRefresh.Prepare(ak, actualCompletedSources, token)
refresh.RefreshOwner(canonicalOwnerKey, allocatedRecord40, token)
refresh.Select(actualReference, allocatedRecord40, selectionAllocation4,
               initialSelectionCapacity, token)
LegacyFreshMainGlobalGlyphUtf8Emission.Prepare(actualCompletedSources,
               normalization, circles, token)
emitter.Emit(actualOwnedGlyphRecord, token)
```

Successful refresh state is cached by the actual completion/session and target
owner, never by equal numeric group IDs alone. A separate failed prefix is
single-use. Validate the same source lease on cache hits. Bound aggregate retained
source references before copying26N allocation bytes (proposed portable65536
references, not a native limit); preserve explicit allocator40 and selection4
images. Source owner-count/pool/compact budgets continue to apply. Prepared
budget/failure controls must be labeled independently of the observed inputs.

Following that bounded owner, a separate new global glyph-choice caller should
refresh every participating WORD reference BEFORE selecting/emitting any of its
alternatives, as the native024F30 first/second passes require. It should borrow
the existing pre-NN3E from the same scope and call the already restored
`LegacyNativeTextGlyphCaller`. It must retain actual source positions and WORD
clip from completed source26N. That next step may add narrow scope/resource/
publication-data accessors; no configured WordChoices/TOP coercion is proposed.

Source-rich024F30 CHAR/WORD interning and selection IDs,021AB0 line/TEXT
publication and application/document acceptance remain subsequent actual-owner
joins. Imported completion, app defaults and thresholds are unchanged.

`audit_global_ordinary_refresh_plan.py REPORT [DIAGNOSTIC]` reads pinned vendor
PEs and composes five existing native audits. It pins13C6E0's callback/4000/
selection order and217670's exact compact gates, separately labeling the
optional E-only observation. It never loads or executes vendor code.
