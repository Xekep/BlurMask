# Original-selected document continuation

The pre-NN original-selected publication route has the same retained selected
INPUT/C22 owner throughout Context, imported2DA, shared3E, Seed and WORD choices.
Its downstream transform and document guards currently stop a type mismatch,
not an unsupported neural operation. The following bounded continuation can
remove those guards without substituting the older one-INPUT context or
claiming original page acceptance. This checkpoint adds static proof only;
the frozen pre-NN runtime and its captures are unchanged.

## Exact selected source at the transform boundary

Text0283B0 first completes023260. At02980E it loads result.SourceTag30 intoR8.
The `-1` branch leaves the local source vector empty. The supported adopted
route has the actual chosen INPUT registry tag instead:02982F loads the
current Ink C20.V128 callback and029836 invokes it. The callback is Ink06C880,
bound by C20 table0E5050+128. Its C20+40 is that selected C20's C51 registry.
NativeR8 is unchanged before the registry service.V30 call at06C89D; the
decompiler omits this argument. C51.V30=09EDE0 hashes the low32 bits, then
compares the complete signed64-bit requested ID against each key. It returns
the matching entry payload, or null.06C880 returns payload+8, the retained
child-owned C22. A missing key reports1000000 and returns null; a convenient
empty C22 or original parent's entry is not the result of this branch.

The caller reads C22+18 and+20 into its local source range vector at029839 and
02983D. It calls037E80 with the same current C20 and writeDom=false at02985C.
The existing037E80 proof establishes that the result-owned108 vector is
replaced even on export failure;029861 tests that status before026EF0.
Actual selected INPUT span children, rather than the transform count, supply
the DAF input. The supported selected PAGE clone retains an `input` with no
span children. No DAF row is fabricated from an empty C22.

The actual OriginalSelected owner already offers
`ExportInputGeometry(exactChildRegistryEntry)`: it checks completed state,
exact selected owner and registry membership before exporting through the
same CurrentView. Its preparation also verifies the original full-domain
geometry before cloning. The new transform result must retain that exact
entry alongside the returned identity geometry. Geometry bytes alone cannot
prove which selected C20 exported them. The old `SourceExport` getter can
remain strict on this new route; no old export owner should be manufactured.

## Registry identity and native command order

Document00D540 calls C20.V170 with the last argument1. Ink06E120 therefore
constructs the new selected C20 with history=false.06D2C0 copies every matching
nonempty registry entry with its original ID/name/category into target-owned
C22s; its final counter copy retains the original Next. The chosen field and
chosen tag do not restrict this registry to one entry. In the supported
two-field case both INPUT IDs0 and1 survive even when tag0 is chosen, and
Next is2. Original point IDs remain a different domain.

The same pending C4D created before WORD publication starts allocating CHAR,
WORD and TEXT selection IDs from that actual Next. Native0283B0 compares the
initial and final tuple counters after026EF0 and only appends operation7 when
they differ.07AAF0 stores oldCounter at command20 and newCounter at18. A
new continuation must read the common retained binding's Next, not chosen
tag+1, source-line count, raw-object count, or a fresh counter0.

Ink C20.V188=06ED30 reads selected C20+58. In this exact worker clone it is
null, so the native branch invokes C4D.service.V40=078090 with the same target
C20, incoming transaction+8 and cloneSelections=true. This is not the
original C20's history branch. Operation4 clones the actual command C22 onto
that selected target, preserves name/category/requested ID, and requires the
returned ID to equal the requested ID when there is no remap table. The
existing raw range and command ordering algorithms apply unchanged.

All initial selected IDs must participate in collision checks and budgets,
not only the chosen tag.09EE20 updates Max and Next only after the relevant
registration succeeds: an explicit ID advances Next if necessary. Operation7
checks both counters are nonnegative, reads the current C51.Next and skips
VF8 when Next has already reached newCounter. Otherwise C20.VF8=06A100 clamps
the request above Max, optionally writes the clamped output, enters the C20
mutation nesting, calls C51.V28 and drains pending observer changes through
064610. C51.V28 only stores a value greater than Max. The supported finite
active state always has Next>Max; overflow and inconsistent states must be
refused before mutation.

History=false does not mean every observer disappears.0638A0 still constructs
C5E;064610 processes pending observer removals and additions. This continuation
models an immutable library registry overlay over the selected owner, not
native C5E versions/events. It must not claim those side effects were ported
or silently introduce global history-manager calls.

## Proposed bounded runtime changes

The one common `LegacyLibrarySelectedInputBinding` remains internal and is
the ownership discriminator. No first-candidate or NN owner changes are
needed. The proposed files and responsibilities are:

| File | Narrow change |
| --- | --- |
| `LegacyLibrarySelectedInputBinding.cs` | Resolve and retain the exact chosen selected entry for identity export; expose immutable complete initial registry IDs, Next and Max internally. |
| `LegacyFreshMainDocumentInputTransforms.cs` | Consume that export, preserve the old typed export getter, expose the new actual selected-entry/geometry provenance, retain the existing unconditional same-seed108 swap and failure prefix. |
| `LegacyFreshMainDocumentCounterHandoff.cs` | Read initialNext from the same binding; preserve all native operation7 bytes and command order. |
| `LegacyLibraryTextDomTransportCache.cs` | Compare exact binding references for cache/transfer/transaction; preserve old getters and add a named OriginalSelected getter rather than coercing types. |
| `LegacyLibrarySelectedInputRegistryReplay.cs` | Keep complete initial selected registry identity and counters, clone actual command ranges into an active selected-owner overlay, retain existing single-use prefix and budget checks. Old InitialContext/Input getters remain strict. |
| `LegacyLibraryOriginalInputPatchCompatibility.cs` | A named new route checks the exact retained original bootstrap/chosen original entry/current view and actual selected child C22 against the independently rebuilt raw domain. It provides the existing library compatibility result, not a native original-history target. |
| `LegacyLibraryTextPublicationDocument.cs` | Remove the new-route preflight guard only after the above checks are available; bind the receipt, transferred C4D, replay and optional compatibility results to the same input binding. Preserve document-first lock order and failure gates. |

New probe, independent oracle, static consumption checks and documentation
would form a separate batch after approval. No public caller IDs, ranges,
success booleans, history flags or replacement Seed are inputs. Old public
APIs and their old-route outputs stay unchanged. Existing DOM target,
range-UTF8, BE2 scratch simulation and actual CHAR/WORD/TEXT algorithms need
no ownership substitution.

The new actual controls should choose both fields of the same two-INPUT
original bootstrap before NN, including reversed physical input. They must
show initial IDs0/1 andNext2, unchanged chosen source tags, exact CHAR/WORD/TEXT
operation4 IDs/category100/ranges and trailing operation7 old2/finalNext;
explicit original-selected export membership; and successful same-binding
transport, optional preflights, selected replay and explicit library DOM
apply. Independent checks must read actual source/C22/raw40/48/24/TEXT/line
records after inference. Prepared clone refusal and cancellation controls
retain prefixes and poison entered owners; preentry cancellation is reusable.
Foreign original/selected entries, foreign transaction, repeated claim and
scope close must refuse before the next write. Old scope/document/transport/
replay/original compatibility captures must remain exact.

## Acceptance boundary

The source original is the actual retained bootstrap and not a generic
caller-provided C20. Its library journal records original mutations, but does
not instantiate CBB/store/revision/CB7 receiver machinery. Native original
C20.V180=06EC60 obtains its target from its original58 C4D via07E2E0/F8.
Native CBB115800 applies original Ink.V188 and original DOM.V218 under page
locks, revisions, rollback and history services. Neither a selected clone's
null58 nor a library kind4 DOM target can replace that owner. The proposed
result remains explicitly library registry replay plus library DOM transport
and application. Full C20/page acceptance and all0283B0 remain unfinished.

Run `python tools/handwriting/audit_original_selected_continuation.py OUTPUT`.
It verifies two full PE hashes,6 focused native regions,8 bindings and52 exact
instruction pins plus three existing constructor/clone/transform/replay proof
trees. It reads vendor bytes only and never loads or executes a vendor DLL.
