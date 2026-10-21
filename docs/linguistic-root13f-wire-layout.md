# Linguistic resource13F wire layout

`LegacyLinguisticRoot13FWireLayout` owns one complete unencoded13F/102 resource
file. It reads the133/100 prefix, then walks declared child envelopes in order.
It does not scan for signatures, decode child processors, select languages,
implement activation or construct a recognizer.

The133 prefix contains a four-byte field,56 encoded name bytes, a sentinel, an extra raw4C
only when that sentinel is-1, and unconditional raw50. The child count follows.
The prefix's licensing-related fields remain opaque copied data. No validity,
activation or licensing decision is inferred from them. Other root versions,
encoded envelopes and nested stream origins are explicitly unsupported.

Alignment applies between children, with no alignment after the final child.
Padding bytes are preserved: the actual Russian resource contains three `ffff`
pads, so requiring zero would reject valid data. Every child retains its exact
owner, ordinal and absolute byte range. Equal bytes from another layout do not
grant child identity. Returned envelopes, prefix and padding are defensive copies.
Unknown child kinds remain opaque wire data, never executable model objects.

Admission bounds the complete snapshot to64MiB, the child count to4096 and every
declared extent/alignment to the root. Count validation precedes child allocation;
copies check cancellation in64KiB chunks. The bound describes retained wire data,
not process RSS or execution limits for a future recognizer.

The actual Russian file contains22 children:19 type13A, two type13B and one13D.
The previously selected Russian dictionary is606964..4565174; an English13B is
4575280..6051538 and associated13D metadata is4575188..4575278. Its identifier
does not establish that this90-byte block contains a learned grammar. This inventory
does not prove which sources a native configuration selects. Automatic bilingual
selection, grammar admission and their runtime observers are separate work.

The probe checks both bundled linguistic resources plus seven generated layouts,
both133 sentinel branches, nonzero padding, unaligned final children, unknown
opaque kinds, the4096-child positive endpoint, defensive copies and39
malformed/foreign-owner/cancellation guards.
The independent Python reference reconstructs every prefix/child/padding byte
range and fingerprint. Managed and NativeAOT captures match SHA256
`8a741235435968f4c46b9e1f226b708196f75b974b431e3015ece493f35ec585`.
The separate [native wire audit](linguistic-root13f-native-wire-20261021.md)
checks three complete reader/decoder functions and35 instruction pins against
the original PE bytes. Native source admission remains separate; matching these
readers is not evidence of complete native recognition or resource activation.
