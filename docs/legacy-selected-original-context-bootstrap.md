# Original field to selected worker input: pre-NN owner contract

This static checkpoint follows the committed library original PAGE/field/INPUT
bootstrap. It does not execute a vendor binary or add a processing-context
adapter. The existing one-INPUT context and publication APIs are unchanged.

## Original and selected readers are different owners

Document00D540 receives the actual request BCD at state38. At00D5E4..00D5EE
it calls that BCD reader's V200 and keeps the returned ORIGINAL context in R12.
It then calls that original context's V78 to get the original Ink C20 and DOM.
The selected Ink returned later in R14 is not this original context.

00BA60 resolves input through the original reader V1A0, reads its actual tag
through V168 with default -1, and verifies the tag in original C20+40 through
C51.V30. Thus the chosen tag is a registered selection ID, not an index into
raw curves, fields, or the selected registry snapshot.

00D540 loops twice. Its C20.V170 arguments are exactly R8=0, R9=0,
selector={byte0=1,I64+8=chosenTag}, stack arg6=1, arg7=1. It stores the two
returned selected Ink/BCD pairs separately at state60/68 and78/80. The current
request later uses78/80; two copies are not a shared mutable C20.

For each pair, 00D6D3 loads the actual original field BCD, 00D6D7 places the
ORIGINAL R12 in R8, and00D6DD calls00C590. This separate argument is real, but
its presence alone does not prove that the returned selected reader retains it:

- CB5.V0=Document1243C0 constructs a parameterized Engine BCC with flag0.
- 00C590 clones optional guides, optional activeArea, then the actual selected
  field subtree. It appends that field to new page/content (or cloned activeArea)
  and creates BCD with the new reader and actual cloned field.
- 00C801..00C80B invokes that reader's V1F8 with the original context.
  00C80E sets local success to1 without testing returned AL.
- Bare BCC table227080.V1F8=027640 is `xor al,al; ret`; V200=031900 returns0.
  Consequently its original context was NOT stored by this call.
- Document00D760 later accepts BCC/BEF and creates a new BEF/BCD. For a bare BCC,
  00D7B8 explicitly initializes BEF's second constructor qword to0. For a BEF,
  it preserves that BEF's actual20/28. It does not invent a missing original.
- Engine BEF0F0520 retains its input BCC20 and supplied optional context28;
  its V200=00C0A0 returns28. V1F8=0F0700 can replace that retained context.
  These methods belong to BEF, not bare BCC.

The native original CBB/page context therefore must remain a separate owner.
The library bootstrap likewise must retain its exact original typed owner
separately from its selected worker DOM. It is not an SDK CBB implementation.
No selected BCC or library Seed is relabeled as that context.

## Selected C20 carries all matching registry entries

Ink C20.V170=06E120 constructs its child C20 before the copy. Native arg7=1
selects zero history byte in the new C20 parameter record, even when original
C20+58 has an active journal. Its selector byte1 branch resolves chosenTag in
original C51 and passes that entry's actual C22 to06D2C0.

The bounded candidate is a full-domain ordinary raw selection. It is not a
clipped INPUT. Under this restriction06D2C0:

1. Sizes its new original-object slots from original C20 count18. It clones the
   selected ordinary curves while retaining the original raw source ordinal;
   it does not allocate source IDs from the C51 selection counter.
2. Builds current ranges/object mapping from the actual selected C22. Equal
   complete domains preserve the actual current-view point/interval identity.
   Clipped, holes, transforms, foreign raw owners and auxiliary payloads must be
   refused by the proposed owner before claiming this restricted route.
3. Initializes C51 query ID=-1, name0, category=-1, selection=chosen C22, mode2
   (06DC89/06DC8D); calls source C51.V68=0A0160.
4. That query does not filter by the chosen INPUT ID. Its selection path calls
   09FE20 -> spatial traversal09D900 -> callback09FAC0. The callback uses09FA10;
   mode2 returns true at09FA67 for each spatially enumerated candidate and unions
   that candidate's ID set. All original bootstrap INPUTs have equal nonempty
   full-domain C22 ranges, so each is a candidate. Equal C22s may be interned
   together without merging their distinct registry IDs.
5. With caller arg6=1 it calls source C51.V78=0A0850. This constructs/interns an
   EMPTY C22 then removes its associated IDs from the query set. This is not a
   predicate that removes our nonempty INPUTs, nor a reason to keep only one ID.
6. Iterates the actual query ID set. For each ID it looks up the ORIGINAL
   source payload (06DD61/06DD75), copies/intersects its ranges with chosen C22,
   and constructs a TARGET-owned C22 with target C20 (06DE38..06DE69).
7. Registers with the exact requested original ID, original name and category
   through target C51.V38=09EE20 (06DED2..06DF1D), verifies returnedID==requestedID
   at06DF20, then continues. An auxiliary payload invokes additional services;
   the proposed branch refuses it rather than manufacturing a null payload.
8. Only after successful iteration does it read ORIGINAL C51.Next viaV20 and
   set TARGET C51.Next viaV28=09ED40 (06DFBE..06DFDF).09EDCE..09EDDC requires
   Next>maximum live child ID and writes actual Next18. It is not simply N
   raw strokes or the chosenTag+1.

For example, an original bootstrap with field INPUT tags0 and1, both selecting
all raw curves, has Next2. Selecting field0 still produces both child registry
IDs0 and1 and Next2. It keeps chosenTag0 for the field's input.tag and2DA.source30.
Selecting field1 keeps chosenTag1 with the same complete registry domain. This
is a counterexample to casting either result into the old one-INPUT0 owner.
The first future runtime controls must include both chosen IDs and a field
whose INPUT ID differs from its raw curve ordinal.

## Proposed bounded owner and exact joins

New `LegacyLibraryOriginalSelectedFieldContext` is proposed, with private
creation from exact successful Bootstrap and exact State.Inputs entry. It
retains Original, CurrentView, Document, chosen field/tag, all copied registry
entries and counters, target-owned/interned C22s, and selected PAGE/BCD tree.
It exposes defensive structural snapshots. Foreign/stale/failed original
entries, clipping, activeArea/guides, input span children, prior text and aux
payloads stay unsupported. Bare selected reader context is explicitly null.
Its separate `Original` property is a LIBRARY bootstrap provenance reference,
not a simulated native CBB/page/store interface. No caller success flags.

Suggested first additive runtime file: the new selected owner plus new probe,
independent full DOM/registry/range/counter oracle and static audit. Old API
and numeric captures remain unchanged. This owner can be useful before NN
without claiming full00D540 worker/cache/history orchestration.

A pre-NN publication join then needs an internal common selected-input binding,
not a cast to LegacyFreshTextFieldInputContext. Current concrete sites are:

| Existing file | Narrow required change, subject to explicit seam agreement |
| --- | --- |
| LegacyLibraryTextProcessingContext.cs | named CreateForOriginalSelected; retain selected binding; clone actual selected PAGE/field/input, then unchanged input-only02D8C0/054340 import; old SelectedInput and BoundInput28 getters strict on new route |
| LegacyLibraryTextDomArena.cs | internal document-based name/arena owner constructor to retain the SAME Document; no fake old INPUT object |
| LegacyOwnedFreshTextSourceContext.cs | internal selected binding constructor derives actual child C51.Next and retains original registry lineage; old Prepare/PrepareSelectedInput unchanged |
| LegacyFreshMainTextSourceWrapper.cs | internal LegacyMainSharedTextSourceState overload using new binding, same unbound TOP/one-use BindActualTop and borrowed strings3E lifetime |
| LegacyFreshMainTextDocumentSeed.cs | binding-backed source30/C22/DAF/Document; old typed getters remain strict; new original-selected getter; NEW result created before NN, no Seed replacement |
| LegacyFreshMainWordSourceChoices.cs | internal adopted route compares exact binding instead of the old concrete input type; old public signatures unchanged |
| LegacyLibraryTextPublicationScope.cs | named PrepareConfiguredOriginalSelected; retain exact original/selected context before NN and keep existing scope/child leases |

This inventory is a proposal, not completed runtime integration. Every result
must stay bound to the same original raw Document/CurrentView and actual C22,
child C20 and selected field, with context-created2DA/shared3E BEFORE callbacks.
Original-context disposal must not release resources while a retained child
scope remains live. Downstream original-registry replay/compatibility currently
has concrete old-input types too; these need a later same-owner adapter or
explicit refusal before new-route use. No CBB history acceptance is supplied.

## Reproduce

`python tools/handwriting/audit_selected_original_context_bootstrap.py OUTPUT`

The audit checks full pinned Document/Ink/Engine PE hashes,19 decoded regions,
162 instruction pins,14 virtual bindings and three committed proof dependencies.
It includes the correction for BCC's no-op context setter and the broad C51
copy loop. It neither runs models nor executes vendor binaries. The numeric
scope remains pending runtime controls; this document does not assert a new
recognized string, SDK page identity, transport revision or accepted document.
