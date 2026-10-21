# Private ordinary English query/request ownership: STATIC proposal

This proposal follows current main 5eff0c6, merged genuine D0 (17e52cc), and
immutable NEW11 transition runtime manifest cfdd6508. It adds no runtime and
makes no whole-word, native-acoustic, default-configured or quality claim. The
existing lifecycle PE closure is reused; no new SDK invocation is proposed.

Integration note: Russian V3 was subsequently merged in f237b9e, separately from
this proposal. References below to the generic2B0 Russian baseline describe the
frozen review base, not the current opt-in recognizer. English transition runtime
admission additionally requires the separate fresh sampler210 provenance proof.

## Exact dependency and seam inventory

The proposed next executable owner is a private decoder shell, not a public
prepared-row adapter. Four NEW files are sufficient for the lease/aggregate-row/
written-request boundary, but do not by themselves finish a word decoder:
`LegacyEnglishOwnedWordDecoder.cs`, `LegacyEnglishOwnedWordQueryBinding.cs`
(NEW partial on the existing binding), `LegacyEnglishOwnedWordQueryRow.cs`, and
`LegacyEnglishOwnedWordScoringRequest.cs`.

ONE narrow old seam is necessary: in binding `CopyPreparationSnapshot`, inside
its existing gate and before `contexts.Capture`, reject a NEW companion's
`queryBusy`. Existing source permits this snapshot even when permanently
`active`, while `LegacySymbolContextFactory` explicitly requires serialized
access. Keeping only `d0Busy` in a companion would not stop snapshot enumeration
from racing off-lock context mutations. `queryBusy` is set/cleared by the same
atomic query lease, independent of d0Busy, so normal D0 and inactive snapshots
retain their present behavior. No full query, model validation, callback, point
copy or context capture may be performed under the binding gate. This is a
proposed guard, not an already applied fix.

All existing generic numeric kernels, query source/aggregate, startup factories,
D0 owners and public prepared helpers remain unchanged. NEW production cannot
accept raw parent88, raw request80, arbitrary flags or supplied callbacks as
execution authority. Existing numerical factories remain data components.

The decoder privately creates the SpanSession from a pristine binding; it never
returns that session or receipts. Actual `ClaimD0Owner` is the only claim and
atomically verifies attempts=outputs=operations=0, checkpoints=epoch=1 and the
actual initial context count. A caller racing preparation against this claim
causes refusal, not an implicit reset. Another companion field cannot block the
already public SpanSession.PrepareSpan; privacy is therefore mandatory.

## Atomic lease, lock order and work prefixes

After each actual scheduled D0 pass, acquire the SAME binding gate once. Require
exact private span owner, d0Version, expected installed map and receipt lineage;
not `IsCurrent` followed by a separate query. Reserve d0Busy and queryBusy, charge
one operation at BEGIN, assign a monotonically increasing private query sequence,
and leave the gate. This prevents BeginD0 and guarded snapshots while mutable
source ring/context/grafts are in progress. Do not acquire the SpanSession gate
under binding gate: its owner is private, immutable receipt lineage was obtained
from the just-completed operation, and the binding is the serialization boundary.

The lease is opaque and non-reusable. All query pages, categories, scoring and
materialization belonging to this native invocation remain inside it. The next
scheduled D0 starts only after release. Release retains d0Version and working-map
identity, does not reset source68, query ring, context pool or aggregate bases.
A row remains a readable historical snapshot after release but cannot execute.
Every consumed operation/output/insertion is monotonic; no rollback of counters
or context IDs on cancellation. Post-entry cancellation/fault poisons both the
binding and private decoder and retains the actual prefix. Pre-entry foreign,
stale, missing-budget, cancellation and concurrent-attempt refusals consume no
operation. No arbitrary public callback is exposed.

For capacity128, raw initial/ordinary continuation query preparation has parent
context0 and reads existing source68; it performs zero ResolveChild calls. The
startup context graft can attempt up to TWO insertions per accepted row
(FFFA initial root and its symbol child), including duplicate lookups. Reserve
256 attempts and128 output rows before reading a full page; charge actual calls
and actual nonnegative ReturnedCount. Other future graft paths must derive their
own bound; this two-call bound does not cover continuation or general1F6FB0.
Negative native page counts are control results, never negative output charges.
Do not silently clamp a negative result into a successful lexical row.

Remaining attempts/outputs/operations must be checked before every mutating
page/graft group. Whole decoder limits separately cover retained rows, parent
records, request cells, heap/candidate attempts and schedule work; refuse the
whole word if a declared independent limit is reached. This is not a replacement
for the native125 source candidate quota or a claim that the model has104 symbols.
No per-page copy of643868 dictionary rows or per-group whole-ink copy is needed.
The existing16MiB binding envelope includes128x32 scratch and <=64B/output;
NEW decoder retained snapshots/parent rows/heaps require an additional explicit
complete envelope, not double-counted as already covered by the binding.

## Actual call order and gaps that remain

2144F0 visits nodes ascending, spans ascending, including intermediate empty
spans up to actual maximum span; D0 precedes every query attempt. Initial branch
requires previous=node-span-1 == -1. Other branches iterate actual finite stored
parent88 records in native order. A missing parent producer is NOT permission
to seed numeric parent rows, replay all earlier D0 receipts or skip continuation.
The existing lifecycle audit independently projects134 graphs and this order;
it is graph evidence only, not evidence of native NN scores or accepted words.

A private aggregate row is a real dictionary transition. It does not become an
NN-selected token merely because it is finite or a terminal. N's frontend128/
category/backend58 numerical helper still needs its reviewed runtime join to
actual pooled D0 rows. Native scoring acceptance, heap quota/pruning/allocation,
compact/materialized parent88, language terminal scheduling and final winner
are later required producers. The NEW boundary must not publish a whole-word
success until all are genuinely joined. They are explicit blockers, not fallback
branches.

The minimal useful next approved batch may implement internal ownership plus
actual aggregate-page/request production, with tests examining those components;
there is no public word result yet. It must not expand the prepared API. A later
scoped batch can join N's helper and actual parent/heap producer without changing
this private identity contract.

## Required independent checks

Use current corrected N134/7 and blind100; BC280 is fixed in5eff0c6. Preserve
all current main RU production/captures (historical2B0 baseline137/261); pending
RU2B1 changes, if separately merged, need their own intentional baseline. These
new English components must not require restoring an old wrong NN golden.

Independently derive raw dictionary labels, node IDs and terminal/root bits;
compare all32 bytes, sourceID0 before aggregate versus1 after aggregate, native
skip/bases, context AVL/page metadata and actual duplicate attempt order. Compare
request80 write masks over multiple NONZERO residue patterns, source/ENDING and
category identity, stable cost-cell identity and actual mutation prefixes. Do
not equate every zero byte to a native-written field.

Exhaustively check the proposed lease state model, but label that as a proposal
model, not running production. Runtime tests must additionally race snapshot /
query / D0 boundaries deterministically; ensure snapshot refusal while queryBusy,
no binding->span lock inversion, unchanged snapshots outside query, failure after
query ring mutation and after each committed context write, stale/replayed row
refusals, foreign equal-byte owners, cancellation before/after entry, and no
partial word publication. Managed/AOT parity needs independent scalar/row/AVL
oracles rather than only matching one another. No expected text labels enter
inference and no beams/weights/thresholds are tuned.

---

## Native field inventory retained from preceding draft

## Actual aggregate and source identity

Private querySource.Read emits direct child32 with byte04 initially0. It is NOT
an owning source-row receipt suitable for startup. Use the existing unchanged
236910 aggregate kernel over the one actual same-source dictionary child. C0E's
093AB0 eligibility is true because kind is not800; aggregate stamps byte04=1
from selected ordinal0 and retains byte05 terminal/root bits. Native implicit
source0 header/weight and selected source1 C0E header are distinct. Do not derive
sourceID from an arc, inject a caller ID, stamp before native aggregate, or assume
raw child0 is an absent source. Bind private child reads/context resolve/link to
our exact contexts/querySource with budget accounting, not public callbacks.

## Query and request inventory

Ordinary initial query derives tag8<<40, Node0, ParentContext0, Category18=0,
Parameter19=0, Parameter1c=current ending node, Parameter20=actual D0 span,
Flags24=0, Flags26=actual native inputFlags0, Skip0, Capacity128 and the actual
shared ContainerIdentity. Continuation uses actual stored parent88 tag00,
parent26 count and parent38 anchor; state21 negative/distance120 gates precede
mutation. It does NOT use parent tag28 or invented prior context. Pure
LegacyOrdinaryDictionaryQueryPreparation can be reused with same-source scalar
policy; no Prepared cursor promotion. Page skip follows actual ReturnedCount
and aggregate per-source bases, not dictionary numeric node increments.

A NEW sealed typed row has an internal constructor only on that actual leased
aggregate page path: binding/private decoder/D0 receipt/query sequence,
immutable complete32 bytes, exact sourceID and dictionary node, context08,
symbol14 and category16/flags17; its current symbol must match actual shared
context. The public snapshot grants no owner authority. Category alternatives
come from the SAME actual sampler217 and qualifier220; arc comes from actual D0
selected row and graph connection enumeration, not arbitrary supplied ordinal.

The consumed scoring request retains typed native-written fields/mask and a
stable private cost-cell identity; do not accept or claim zero-filled native80
stack ABI. Pure initializer work-storage may preserve explicit private residue
with independent mask checks. Startup fields00=current node,04/08=-1,
0C=actual returned arc,0E/10=FFFF,12=current category geometry,13=FF,
14/18=-1,1C=query flags&7,20=actual span,24=symbol,26=FFFF,
28=same private cost-cell token,30=0,34=IMP,38=query length,39=0,
3A=actual fresh permission2309=0,40=(actual C0E>>9)&15=6,44=0,
48=actual tagbit56(0 on ordinary rows),49=0. Unwritten bytes1D..1F,
3B..3F,4A..4F are residue; no fake zero-write claim.

Parent request differs:04=parent18.low16 SOURCE,08=actual previous ENDING,
0E=(parent18.uint>>22).low16,10=parent1C.low16;13=parent composite category's
geometry,14/18=0,1C=7 if parent06.bit0 else rowflags&7,
26=parent24.low16,30=parent14,3C=0,44=parent source header kind,
49=parent07.bit0; pointed cost=query base+parent10 with signed32 wrap. Bytes3B,
3D..3F and other unassigned padding remain residue. No initializing all80 as
native data, no numeric parent/node ownership coercion.

## Reached dependencies and scope

N owns frontend128/categoryGeometry/backend58 numerical source helper, consuming
actual pooled SelectedRows. Policy/118/140 scope is separate. Full regular source
request execution still needs actual context graft, category qualification,
source-header precheck, connection order, record/heap quota/materialization,
terminal/language scheduling and final winner rules. The existing delimiter290
and special/punctuation/alternative paths need their actual reached predicate
or an explicit whole-operation refusal, never silently discarded candidates.
Native full-root/configured/bilingual policy is not admitted by this fresh API.
No word-ready result follows from lexical V58 or one typed source page alone.

Before runtime request approval: pin any additional reached branches not already
covered by lifecycle16/native query9 closure; independent raw record/tag/map/
context and mask-over-nonzero-residue oracle; all query operation budget/fault
prefixes; foreign equal-byte owner, stale receipt, replay, external public calls,
query-vs-D0 concurrency and deterministic cancellation. Compare current corrected
N134/7+PluColl100 and SAME corrected RU baseline, not superseded NN goldens.
