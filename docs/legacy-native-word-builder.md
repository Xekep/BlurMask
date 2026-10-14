# WORD276 builder and plain publisher

`LegacyNativeWordBuilder` restores the120-byte WORD276 reference builder;
`LegacyNativePlainWordPublisher` restores the scoped279→275 publication with
explicitly disabled geometry finalization/order. They use the actual TOP273
glyph pool and graph references, not a recognized string or guessed rectangle.

## Native methods

WORD276 metadata2dc8a8 points to vtable2dc900:

| Slot | Target | Meaning |
|---|---|---|
|00|13f110|Reset head−1, rawscalars10/14=1,18/1c=0,28count0,30fffe, auxiliary counts0|
|10|13f180|Follow record20 links to the last reference|
|18|13f1f0|Reference count, including zero-symbol-span rows|
|20|13f260|Sum symbol spans; this is **not** glyph allocation|
|28/30|13f410/13f370|Insert descriptor into TOP273 pool, establish head only if empty; caller links records separately|
|38|13f4a0|Set symbol on the last linked record; implemented ordinary one-component branch|
|40|13f520|First/last source bounds through actual frontend168/170, or native negative-arc arithmetic|
|48|13f7b0|Return reference atCount−1−ordinal links, reversing the backwards trace into reading order|

Service50/139d90 gives kind1/3 span`(label08>>2)&1fff`; other kinds have
span1. Zero spans remain references and are not dropped. Source head reference
0 is valid; onlyffffffffffffffff denotes empty. Supplied record producers may
write encoded zero-span placeholders through the owned pool. The ordinary
symbol setter does not invent their component/macro policy.

Constructor input is an explicitly empty allocated120-byte buffer; opaque
bytes survive the exact reset writes. Auxiliary vector/pointer ownership is
unsupported. Scalar offsets10/14/18/1c are copied as rawfloat bits; their
interpretation and derivation belong to the actual1f42d0/candidate producers.

## Publisher and owned representation

Class279 vtable2dcbd0+10→1416b0 resolves the source span via276.40, then finds
a278 span owner and interns the word through275/13fe20. The implemented
plain branch uses explicit geometry flagsfalse and no auxiliary output.
Wordsets are scoped by exact source span and deduplicate the exact64-bit head
reference. Each publication creates a new linked node, even when that word
already exists. New nodes prepend to the output list.

In275's geometry-finalizationfalse branch every linked reference is prepended
to its flat vector; this preserves leading/trailing zero-span references.
The copied120-byte header retains original scalar bits. Pointer20 is represented
by the separate owned `FlatReferences` vector and serialized as zero; count28
and1.5-growth capacity2c retain native values. Word flags32 OR all glyph
publication words06. Bit10 is finally set only if actualOwnerC4 survives every
glyph bit11-without-bit10 veto. Geometry sorting/finalizationtrue is rejected;
no callback/flag default is inferred.

Portable dictionaries replace native pointer trees; lifetime/allocation failure,
geometry-sort callbacks, punctuation attachment and auxiliary vectors remain
outside this scope. A4096-reference/word/node budget and cycle/ownership guards
bound work. Callback cancellation is checked both before and after actual
source-boundary callbacks. The published glyph pool remains the same owner.

## Verification

```powershell
python tools/LegacyWordBuilder.Probe/reference_controls.py D:/word-builder-controls.json
dotnet run --project tools/LegacyWordBuilder.Probe -c Release -- D:/word-builder-controls.json
```

An independent tuple/header oracle covers512 chains with zero-span endpoints,
negative-arc source references, native reference reversal, source-callback
arithmetic, opaque header bytes, arbitraryfloat bitpatterns and bothC4 states.
Publisher snapshots, exact-head dedup/scalar retention, linked prepend,
foreign-pool rejection, source callback cancellation and cycles are checked.
The callbacks/reference records are prepared fixtures, not actual raw273 glyph
materialization or Russian recognition quality. Actual candidate vectors and
fresh flag ownership must be supplied by their separately recovered producers.
