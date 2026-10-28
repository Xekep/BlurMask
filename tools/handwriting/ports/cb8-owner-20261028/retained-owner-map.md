# Next owning join: freshly verified CB8 lifecycle

Static dependency map only. It does not add a nonfresh runtime owner, production
edit application or recognition acceptance. The subsequent
[typed C# matcher](../../../../docs/agents/research/math-dom-cb8-edit-kernel-20261028.md)
is tested separately and must not receive snapshot-derived ownership authority.

## Reproducible batch

[retained-owner-request.json](retained-owner-request.json) exports seven complete
function entries in one read-only Ghidra session:32650,32C00,33040,33560,337E0,
33920,339D0. The new batch passed in15.23seconds with650 instructions independently
matched to the original PE, unchanged project/tool inputs and no vendor execution.
Local output: `.artifacts/dom-cb8-retained-owner-20261028-export`.
Document SHA256: `17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f`.

## Concrete retained objects

032C00 requires an exact40-byte constructor packet with nonnull temporary BCC
and retained Ink. Category comes from packet+18; caller context from packet+20.
The retained Ink and BCC must come back as the same identities from retain calls.
An additional parameterized byte0 BCC shares the temporary BCC's name owner;
an optional root name creates/attaches a candidate root in that new arena.

| CB8 offset | Required actual object/value |
| --- | --- |
| +10 | Third, retained C20/Ink clone |
| +18 | Supplied old temporary BCC |
| +20 | Separate CB8-owned candidate BCC |
| +28 | Candidate root in that separate BCC |
| +30 | Registration category,102 in the reviewed Math caller |
| +38 | Supplied caller context |
| +40 | Constructor flag initialized1; semantic use not closed by this batch |
| +48 | Owned72-byte state and its allocated storage |

| State offset | Native role |
| --- | --- |
| +00 | Next temporary ID, initialized2 |
| +08 | Head of pending linked list |
| +10/+18/+1C | Pending-entry chunk storage, count and capacity |
| +28/+30/+34 | Old tag vector, count and capacity |
| +38/+40/+44 | New tag vector, count and capacity |

## Pending entry order and partial construction

33560 obtains a40-byte entry through32650. Allocation failure returns-1 before
advancing the temporary counter. Success assigns/increments that counter and
sets resolvedID=-1 and the supplied canonical name. Nonempty input constructs
C22 using the same retained Ink and actual range pointer/count with flag0.
Count-zero input has a null C22.

**Every newly allocated entry is prepended to the list**, including an entry
whose nonempty C22 construction failed. Its next pointer is the old head. A failed
C22 returns-1 after linking and consuming the ID; do not roll that prefix back.
The matcher must see this actual head-to-tail order. The current fresh publisher
uses distinct IDs in an append-only private list, so order does not affect its
bounded lookups; that representation must not silently become a nonfresh native
linked-list adapter, particularly for first duplicate-ID behavior.

## Matching, resolution, collection and deletion

33040 requires actual candidate BCC/root, invokes the supplied old BCC.V220 to
produce edits from those roots, and filters that produced edit batch through
032800 with the same CB8 and canonical tag name. It does not construct edits from
caller-provided old/new values. A callback result alone is not an applied patch.

337E0 traverses `.//*/@tag` on the supplied subtree of retained temporary BCC
through0336C0. The existing numeric-reader port preserves its INT64_MAX sentinel,
but real late resolution must still use this same retained Ink and pending C22.
33920 collects tags using033870: true chooses state+28, false chooses state+38.
Vectors are ordered and preserve duplicate IDs.

339D0 walks old IDs in order. An ID present anywhere in the new vector is retained;
otherwise the same Ink.VE8 is called with that ID. First failure stops the walk;
successful deletions before it remain. The decompiler omits the ID argument, but
the actual call retains it in R8 as pinned by the earlier owner audit. Do not
deduplicate old IDs or claim transactional rollback for this operation.

## Required source-side change before admission

`Math2Mode0OriginalMathFieldSource` owns concrete private InkOwner/RegistryEntry/
Selection objects and canonical name tables. Its current publication accessor
requires a fresh selected field. `Math2Mode0FreshMathPublisher` stores a third-Ink
identity plus registry records sufficient for its strict fresh three-op2 scope.
Neither its snapshots nor outgoing writer arrays provide a nonfresh C22 owner.

A future factory must take an actual completed live source/application owner,
retain its current old-result DOM and same registry/C22 identities under existing
gates, and clone the selected Ink with Next preserved. It needs a separate
nonfresh accessor; removing the existing fresh-field guard is not a valid join.
An outgoing transfer that has not been applied to the original live document is
not a substitute for that application owner. Original/selected/third registries
and temporary/candidate/outgoing DOM arenas remain distinct.

Preflight all retained storage before copies; retain the established receipt-
before-field lock order; refuse closed/foreign/incomplete owners. Actual old/new
edit production and writer-owned payloads must precede callback use. Disposal,
failed C22/link prefixes, late registration/write failure prefixes and ordered
stale deletion need owning integration tests before any recognition admission.
The constructor's base service4 setup and failure cleanup are visible in this
export, but no complete managed owner/destructor translation is claimed here.
