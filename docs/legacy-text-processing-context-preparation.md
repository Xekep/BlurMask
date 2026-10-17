# 2C1 construction and fresh selected-INPUT request preparation

This read-only checkpoint follows the registered F03/130 caller described in
[registered dispatch](legacy-registered-text-request-dispatch.md). It identifies
the next construction/preparation boundary; it does not add a runtime request
adapter or equate a library result seed with a native processing context.

## Default construction and release

`130.C9E.V8=0479A0` creates a new default 2C1. Its constructor `053C30`
first calls descriptor4's base constructor. Base failure returns false before
the derived success fields exist. It clears `+10`, `+58`, and the four-byte
state at `+18`, then creates **a new 2DA** and stores it at `+10`.

If that creation fails, `053CC4..053D31` performs the explicit generic object
cleanup for `+10` and `+58`, calls descriptor4's destructor, and returns false.
Successful construction initializes the command/metadata arrays at
`20/28`, `30/38`, `40/48`, the comparison byte `50=1`, two range-vector owners
at `90/A0`, further vectors at `60/70/80`, resource rows `100/108`, and scalar
`F4=0`. These are assigned fields, not a claim that every padding byte of the
native allocation is initialized.

The new 2DA constructor is `01C010`: source/tag `30=-1`, UUID `38..47=0`,
TEXT count `D8=0`, transform vector `108/110=0`, and range vectors including
`208/210=0`. It creates its **own new strings3E** at `+118`. Failure of that
creation returns false; later success-only fields must not be read as a
completed owner after failure. Existing library UUID seeding is a separately
documented portability adaptation, not this zero-initialized constructor.

2C1 destructor `053DE0` destroys its owned 2DA through the generic object
destructor, frees/releases the command and metadata structures, clears the
range-vector resources, destroys `+58`, releases per-resource objects in the
`100/108` vector, and invokes the base destructor. The registered worker
retains this 2C1 at state `C0` across processing requests, and resets it when
its mode/configuration conditions require. A portable lifetime model must
preserve that distinction from the invocation-local retain in `049370`.

## Actual input to 2C1.V10

`049370` resolves the DOM `input` tag through the request's selected C20
`V128`, obtaining its retained **C22**. It passes this to
`2C1.V10=054340`, together with the actual selected BCD, DAF-related output
storage, derived guide/change byte, and an output byte.

The relevant 054340 signature is:

| Native parameter | Role |
| --- | --- |
| 1 | engine context |
| 2 | exact retained 2C1 |
| 3 | retained INPUT C22, or native null |
| 4 | caller's DAF-related vector |
| 5 | actual selected BCD (`+10` DOM document, `+18` field node) |
| 6 | actual derived change/guide input byte |
| 7 | caller-owned output byte, written only on its native branch |

**C22 `+20` is its range count, not the number of INPUT span children or DAF
commands.** The existing full selected INPUT has nonempty ranges even though
its addItems-produced DAF vector has zero span rows. Consequently it cannot
take 054340's empty-C22 shortcut.

`054340` has three distinct paths:

- Null C22 uses the 2DA's currently bound C20, if any, otherwise resets 2DA.
- Nonnull C22 with count zero uses its C20 owner at C22 `+10`. These two paths
  clear old context command counts/metadata and write `0x100` at context
  `+18`, even if the binding call returned false.
- Nonempty C22 creates a temporary 2DB snapshot. If context command count
  `+28` is nonzero, it first preserves/remaps old source-related commands.
  A **fresh** 2C1 count `+28=0` skips that old-command loop, but still requires
  the 2DB allocation and subsequent preparation described below.

## Nonempty-C22 fresh branch and failure prefixes

The fresh branch first tests temporary 2DB creation. Failure does not perform
the 2DA import. On success, `0545C5..0545F2` calls the owned 2DA's
`V28=02D8C0` with:

```
actual selected BCD.document,
actual selected BCD.field,
actual INPUT C22.OwnerC20
```

02D8C0 resets prior 2DA state, interns its actual DOM names, binds those three
owners, and imports input/textResult/CHAR/WORD/TEXT/line/transform metadata.
It is a real import, not a no-op attachment. A late import failure may retain
its partial prefix; no rollback is established here.

For the fresh input-only domain, its tail reads imported `input.tag`, resolves
that ID through the same selected C20 `V128`, and copies that C22's actual
ranges into 2DA `208/210` (`0304A3..0304F0`). The nonempty selected ranges
therefore remain nonempty; raw-object count and zero DAF count do not replace
this producer.

After successful import, 054340 calls temporary `2DB.V18=050660` to take a
snapshot of the imported 2DA. A snapshot/import failure leads to the false
status path. At `05472D`, the four context state bytes `18..1B` are cleared
after these attempts; they are not preserved merely because preparation
failed. Only a successful status enters the subsequent flag derivation.
The nonzero imported range count `2DA+210` forces byte `context+18=1`
(`05474E/054755/054936`). Other branches inspect `220`, `1F1`, `200`, and
snapshot records, or call `2DA.V240=020B40`; they cannot be replaced with
constructor constants when importing an existing result.

The temporary 2DB is destroyed on the normal common tail (`054976..05498F`).
The caller's parameter7 is written only in the branch where context byte18
is zero; a fresh nonempty selection does not justify inventing a new value
for that untouched byte.

This explains why the real fresh request enters 049370's changed/full
processing path. A registered no-change export helper is not an automatic
substitute for recognition.

## Bounded next runtime contract

A coherent portable implementation would construct a new library processing
context **before** preparing/publishing its result, own its new 2DA/strings3E,
and bind a typed selected request containing the exact selected C20/C22 and
an actually constructed selected BCD/input DOM. That request must outlive
the imported borrowed DOM/input bindings. Tests should inspect construction
failure boundaries, temporary snapshot release, import failure prefixes,
untouched output-byte residue, and exact range transport into 2DA210.

The existing `LegacyFreshMainTextDocumentSeed` is created before shared WORD
publication and already models several corresponding fields. It is not the
new 2DA allocated by the registered 2C1 constructor, and must not be inserted
post hoc into context `+10`. A future common owner can construct the seed
as its child at the proper time; a cast or a second independent strings3E
would lose the actual ownership relationship.

The first missing input is now concrete: an owning selected BCD/textField
DOM whose name owner, INPUT tag and selected C20 agree, followed by the
input-only 02D8C0 importer. CB5's selected-DOM creation is static evidence for
that producer; the existing empty library BCC target is not that selected
field. Full processing/resource scheduling in 046B60/0475A0 and registered
cache/history application remain later stages, not callbacks that may be
filled with successful booleans.

## Verification

```powershell
python tools/handwriting/audit_text_processing_context_preparation.py E:/BlurMask-build-artifacts-20261014/text-processing-context-preparation-static.json
```

The pinned Text PE audit checks 5 regions, 7 virtual bindings and 64 exact
instruction pins. No vendor module is loaded or executed. The larger import
and request-control observations were checked against the existing full
decompilations; no claim is made that this auditor executes their behavior.
