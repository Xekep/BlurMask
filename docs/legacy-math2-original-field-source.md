# Math field and selected source before inference

This checkpoint reads the three pinned vendor PE files as data. It supplies
the math-specific original/selected ownership contract needed after the
source-bound first result and one-F0 outer copy. It changes no runtime API,
NN input, existing text context or DOM target. In particular, math category102
does not authorize passing an old category100 text owner to the math path.

## Registered Math field service

MyScriptMath metadata097D40 has type13B9 and baseBE1. Its descriptor097F60
links through+8 to the namespace extension097FD0. The extension contains the
actual page namespace09CBA0, flag1, initializer002920 and getter007570.
Registration0015AE loads this exact metadata before the0015B8 registration
call. The namespace is `http://www.myscript.com/ns/myscript/page`.

The initializer00292E passes marker0981B0. That marker's first pointer names
`mathField` at09C988. Getter007570 first compares the actual marker identity;
only the matching marker and interfaceC98 return table097C80. Its V0=0055B0
writes kind2 and returns true. An arbitrary node named similarly, an arbitrary
C98 supplied by the caller, or a textField with a changed category does not
establish this binding.

Document11D310 checks kind0..5 and jumps through11D3A8. Case2's entry11D3B0
targets11D348, which returns the actual `mathField` string20CF20. Generic
11D3C0 uses that result as the element name, assigns the actual requested id,
attaches it to content (or the real activeArea path) and creates the BCD field
wrapper over the retained DOM/node pair. Its history begin/end/refusal order
is shared machinery, not evidence for a text-specific field kind.

In005380, a missing input tag causes a query of C98 on that actual field.
005637/00563A invoke its V0,00565D adds decimal100,005663 stores the resulting
category argument, and005691 invokes the SAME Ink's VE0. The registered name
is `INPUT`. On this Math binding the category is102. Only after successful
registration does0056C3 write the actual returned id to the input's numeric
tag attribute. Registry IDs, DOM node IDs and original raw object slots are
different domains. A nonmissing tag follows004FA0; it is not silently replaced
by another registration.

C98.V50=0054A0 forwards the same DOM and actual field node to003070.
That function initializes all three returned option pointers to zero before
looking for optional configuration. A new field with only id/input and no
configuration therefore supports the absent-options branch. This is a
structural factory restriction; it is not a claim that imported or configured
math fields always have default options.

## Original and worker-selected owners

Document00D540 obtains the source Ink/DOM from the original field's BCC,
checks the page/content scaffold and C98 option service, resolves the actual
input tag through00BA60, then constructs two worker contexts in order.
00D6C8 calls SAME original C20.V170=06E120;00D6DD invokes00C590 to clone the
actual selected field DOM. The first pair is stored at60/68, the second at78/80.
These are separate owners even when their range contents match.

The final clone argument is1 at00D68F. In06E120 that disables the new C20's
history flag before its constructor.06E237 calls06D2C0 to copy raw objects
and matching registry entries. The established generic selected-context proof
preserves original entry IDs, names, categories, target-owned C22s and the
actual Next counter. It does not renumber INPUTs as stroke slots or keep only
the chosen tag. The fresh math restriction proposed below has one nonempty
INPUT, so no general grouping/remapping inference is required.

00C590 constructs a separate CB5/BCC scaffold, copies actual guides/activeArea
when present, deep-clones the selected field and invokes the new BCC's V1F8
original-context setter at00C80B. The setter's return is ignored. A retained
library original identity must not be advertised as a native reader context,
CBB/store/revision receiver or native original-page acceptance.

The old implementation `LegacyLibraryOriginalSelectedFieldContext` explicitly
requires INPUT category100 and its old text bootstrap. Those concrete APIs
remain valid for their own path and must remain strict. The generic native
clone algorithm is reusable only through a new math-specific owner seam.

## C23 is initially empty

C23 metadata0E61F0 derives from C21 and points to descriptor0E6238. The
parameterized constructor interface0E6048+10 is086370. A16-byte constructor
packet takes its actual C20 from packet[0]. The constructor zero-initializes
its range vector18 and auxiliary vectors through the default range service
V60=07F7F0, retains that C20 at+10, and calls C20.V18 with priority0 and
retain-listener=false.

That V18 is064DA0, an observer attachment operation. It dispatches to0648A0,
which registers the C43 callback and invokes its V0. C23's query08D020 returns
C43 table0E60D0; its V0=020D60 is a literal no-op return. Thus this attachment
does **not** populate the selection with the full raw domain. Calling C20.V18
a domain exporter would give the wrong constructor state.

The C23 destructor086520 detaches from the same C20 via V20 before releasing
its retained C20, listeners and vectors. A future managed source must retain
the corresponding original/selected lifetime; a copied range array alone
does not stand in for that ownership.

## Actual INPUT lookup and mode0 intersection

Document02AF50 takes a tuple containing the actual field, current selected
C20 and recognition context. It compares the actual field node name against
`mathField` before constructing the work.02B072..02B085 call DAF.V8 with the
same field DOM/node and the current C20. Registered DAF table1ED790.V8=02FE40
finds the actual `input`, reads its numeric `tag`, then tail-calls that C20's
V128 with the complete returned ID. A missing input/tag returns null. A
missing registry key reports an error and returns null through06C880; no
fallback source is manufactured.

C20.V128=06C880 reads that C20's C51 registry at+40. C51 service table0E78A0
V30=09EDE0 hashes the low32 bits but compares the complete64-bit ID before
returning the matching payload;06C8D9 returns its retained C22 at payload+8.
The caller's tag is preserved in R8 despite its omission in the decompiler.

02B0BD constructs the empty C23 over the actual current C20. Only if DAF
returned a non-null C22 does02B0EF invoke C23.V50=086FD0->086C20 with that
C22's actual ptr/count and operation0. There are two important details:

- 086C65..086C6F first return success unchanged if the current ptr/count are
  identical to the supplied ptr/count. Fresh null/0 plus null/0 therefore
  remains empty; an allocated zero-count input follows a different path.
- Operation0 clears/reserves and copies the SAME C20+E8 domain at086E26..
  086E47, then **intersects it with the supplied INPUT ranges** through the
  default VC0=080B40 at086E67/086E6E. It finally calls0860A0 to normalize/check
  C25 boundaries. Copying the domain alone is not the result of operation0.

080B40 makes an empty operand empty, rejects the special mode4 combination
by producing empty, clips a single interval through0804E0, or builds a union
of individual intersections through080830. Its endpoint normalization is the
existing source-range algebra, not integer stroke-index slicing. The new
auditor's small branch examples use canonical ordinary endpoints only; they
are not a second general packed-position implementation.

02B134 calls C31.V40=08A750, which tests actual C23.count20==0. Empty selection
jumps to028070 at02B139 without calling029290. A nonempty selection can enter
029290, which creates actual F0/work items and invokes EED.service20 through
027590. With a fresh field and no mathResult/sequence subtree, the existing
result/freeze reconstruction branch is absent. Imported sequence, C25,
partial raw selection, auxiliary payload and multiple F0 grouping require
their own proven owners.

## Smallest owning implementation proposal

The next bounded runtime should create its math context **before** NN:

1. A new library `OriginalMathFieldBootstrap` over an exact immutable raw
   CurrentView/Document creates one fresh PAGE/content/layout and mathField
   with an actual caller id. Its private Math policy derives kind2/category102
   from the registered binding; neither kind/category nor a success flag is
   public input. It owns one real full-domain, nonempty INPUT/C22 and its tag.
2. A new selected Math context clones the exact field and source registry with
   its IDs/category102/Next and target-owned C22. It retains the original and
   the exact selected C20-equivalent identity. No old text owner is fabricated.
3. A new private C23 request resolves that selected INPUT through the actual
   field/tag/registry, starts empty, and performs domain intersection/validation
   before producing geometry. Missing/empty INPUT remains an explicit no-NN
   state; it never becomes full raw input. The initial bounded positive factory
   supports complete ordinary raw domain only and refuses C25, properties,
   guides/activeArea, prior mathResult, configured field/imports and multi-F0.
4. A named NN entry binds the existing source-aware Math preprocessor/C79 and
   RawOnlySource to this exact context and original Document before tensor
   allocation. The present stroke-list factory creates its own raw Document;
   re-running it and matching shapes/ranges afterwards cannot supply this seam.
   Existing raw-only entrypoints and old text getters remain unchanged.
5. Existing full EE4, first-copy and one-source outer vector then keep that
   SAME Document. A later math DOM owner can consume the retained selected
   field/context and root's CB8 tag lifecycle. This pre-NN stage does not
   implement that pending receiver or original-page history application.

Use one shared source claim/gate, preflight combined raw/DOM/registry/mapping
payload before entered mutation, and preserve failure prefixes. Reentry after
entered refusal/cancellation must fail; preentry cancellation remains reusable.
Closing the source prevents new operations while detached historical snapshots
remain inspectable. No replaced source after inference, fake native pointers,
new query callback flags, accepted-formula boolean or text-kind target belongs
to this proposal.

The minimum real test is an original raw source with deliberately reversed
stroke order, one mathField/INPUT, actual selected clone and C23 resolution,
then the SAME NN/EE4/first-copy/outer pipeline. Check category102, distinct
original/selected C22 ownership, retained tag/Next and original Document,
independent features/positions/range unions and unchanged old output captures.
Prepared controls must distinguish absent input, missing tag, missing registry
key, null-empty pointer early return, partial intersection and foreign
same-shaped C20/C22/Document. None may be promoted to a successful full-domain
NN request. No expected transcription is an inference input.

## Reproduction

Run `python tools/handwriting/audit_math2_original_field_source.py REPORT`.
It checks31 complete focused PE regions,194 exact instruction pins,29 table/
metadata bindings, literal strings, the kind2 jump-table entry and21 small
branch controls. Complete PE SHA checks include Math
`d81b9cb071efac9d1300da1c6e8be24db2fbb12793ed1adefba38ffd0f80a8fb`.
The default-context pins explicitly exclude an external service replacement.

Full native bodies are available in the local read-only Math/Ink exports and
`line-document-caller-all`; the new frozen static report includes complete
region hashes and instruction text. Vendor execution is false. There is no
runtime model result, numerical accuracy improvement, CB8/DOM publication or
complete Math request claim in this checkpoint.
