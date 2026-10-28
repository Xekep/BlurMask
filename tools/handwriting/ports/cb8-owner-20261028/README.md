# CB8 owning adapter review

The [new seven-function retained-owner map](retained-owner-map.md) records the
freshly verified constructor/state fields, prepend order and owning join still
required after the typed C# callback port. It is static evidence only.

Static review only; no vendor execution, native acceptance, or production changes.

## Current owner boundary

`Math2Mode0OriginalMathFieldSource.WithFreshPublicationInput` requires the actual
selected owner, its sole INPUT registry entry, and no `mathResult`. The fresh
publisher creates an empty old result and emits exactly three opcode2 insertions.
Consequently its produced actions cannot enter Document032800's opcode7 branch.
Calling the matcher with manufactured opcode7/old values would not implement an
owning integration.

The existing selected/original registries and pending entries cannot be mixed.
Document032800 looks up old range/name in the **same retained CB8 Ink** at owner
offset10, while its matching pending entry is from that CB8's state+48 list. The
original application exposes another registry owner after outgoing transfer; it
is not a substitute for the retained CB8 Ink. The current publisher models its
third Ink as an identity plus private registry records, not the source's concrete
`Selection` owner. Passing array snapshots as purported old C22 identities would
lose this requirement.

## Conversion is not yet a late-registration join

The existing focused export of Document0336C0 shows late registration first
calling **Document031E20(attribute, INT64_MAX)**. Document033870 calls the same
helper with sentinel -1 when collecting tags. Neither body directly calls
Document0078D0. The opcode7 edit callback032800 explicitly converts both values
through0078D0 and then calls Engine serviceglobal90.V88. Export/prove031E20 before
assuming those are interchangeable native boundaries.

The fresh publisher creates typed INT64 tag4 placeholders; late resolution writes
tag4. OutgoingKernel.ValidateTags and OriginalKernel.ValidateTags intentionally
require tag4. No string tag is currently produced at these tag sites. An added
tag4-to-text-to-ID roundtrip would be redundant library validation, not evidence
of a native call-site port. Accepting string tags in those receivers would expand
the current policy without an actual supported producer.

## Typed value representation limits

The0078D0 export represents strings as byte slices: tag1 reads pointer plus a
32-bit length at payload+8; tag2 returns the inline16-byte payload and reads its
length with **MOVSX** from byte15, using15 when that byte is zero; tag3 returns
the payload with length zero. A normal inline writer therefore needs at most15
UTF-8 bytes, not15 UTF-16 characters. Do not turn arbitrary high-bit length bytes
into positive unsigned lengths or infer C-string termination at this conversion.
The publisher's `<16` UTF-8 byte selection is consistent with this normal bound.

Tag4 reads signed64; tag7 sign-extends signed32. Integer formatting uses the
retained Engine global90.VB8 with capacity0x32. Other supported numeric variants
dispatch to distinct services;0078D0 alone does not establish their formatting
algorithms. Unsupported tags return a null pointer and zero length, distinct
from a tag3 empty slice.

## Smallest next production prerequisite

Own an actual nonempty prior selected `mathResult`, matching retained third-Ink
old registry/C22 identities, canonical name tokens, and produced old/new opcode7
attribute actions. Provide a borrowed adapter only under those owners' gates;
derive pending operands from the same CB8 list, old lookups from the same Ink,
and range equality from its retained range service. Preserve first-ID-match and
null-C22/count-zero behavior. `LegacyLibraryOriginalDomName` tokens are canonical
only within their owner; native name comparison is a service call, so arbitrary
cross-owner token reference equality is not its replacement. Snapshot names and
equal-looking range arrays do not confer owner identity.

## Fresh031E20 export and proposed genuine numeric-read hook

The follow-up batch now exports031E20,0336C0,033870 in
`research/myscript/output/cb8-numeric-owner-20261028/`. All three passed the new
batch tool's PE-instruction validation. The project and tool inputs remained
unchanged; the read-only project is released.

031E20 proves a smaller **numeric attribute read** hook in the current fresh
registration loop. Importantly, tag4 directly returns its signed64 payload and
does not consume0078D0. The same holds for tag9's64 raw bits; tag7 sign-extends32,
tag8 zero-extends32, and tag6 returns byte!=0. Only tags1/2 call0078D0, then V88
with an explicit end pointer; parsed data is accepted only when the returned
pointer equals that end. A partial or failed parse returns the caller's fallback.
Tag3 and every other unsupported tag call the error service with0x02000000 and
return fallback; malformed strings do not call this error service.

Tag5 rounds away from zero by adding/subtracting0.5 in double, then executes
CVTTSD2SI into signed64. Tag10 performs single-precision +/-0.5 then CVTTSS2SI
into signed32 and sign-extends. NaN, infinity and overflow therefore yield the
appropriate x64 integer-indefinite minimum, rather than the fallback sentinel.

`Document031E20.cs` proposes the semantic kernel, with an explicit retained string
service interface and no owner construction. `BranchFixture.cs` checks19 branch
controls, including typed/default dispatch, partial parser consumption, signed
casts and integer-indefinite results. Run:

`dotnet run --project tools/handwriting/ports/cb8-owner-20261028/Cb8OwnerProposal.csproj -c Release`

The actual production hook is the existing fresh publisher's tag loop, immediately
after its strict `attribute.Tag==4 && Number is long` admission: supply the real
borrowed attribute to the numeric reader with fallback INT64_MAX, and preserve
the native sentinel comparison before checking negativity/resolving pending IDs.
For its tag4-only scope, a semantic overload taking the actual tag plus signed64
payload is sufficient; do not serialize to text. This ports a genuine native
caller but does not exercise the string converter, widen tag acceptance, or
activate opcode7 reuse. V48's actual reads use fallback -1 and fail on -1, while
preserving duplicate IDs. A later adapter can add the string branches when an
actual admitted producer and owner supplies those variants.

Evidence: `research/myscript/output/math-transport-cb8-20261028/180032800.c`,
`1800078d0.c` and its instruction TSV; existing
`math2-cb8-tag-owner-20261018/focused.c` (0336C0/033870); production source inspected
in FreshMathPublisher, OriginalMathFieldSource, OutgoingTransfer, OriginalKernel,
and OriginalApplication. No build was needed for this read-only review.
