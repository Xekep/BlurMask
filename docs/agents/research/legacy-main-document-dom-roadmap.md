# Detached fresh textResult subtree

This is a static contract and port plan, not a completed DOM or document
acceptance implementation. Range formatting is committed separately.

## Actual caller and owner shape

Text2DA primary table275550 has V18=0283B0 and V30=031880. The successful
0283B0 tail passes its unchanged param7 to2E7.V0=0658E0. The fresh prior-null
or priorD8-zero branch calls061510. In that call frame:

| Offset | Actual role |
| --- | --- |
| 00 | DOM reader, used by the nonfresh path for V1B0 lookup |
| 08 | Existing parent node;061510 scans its children at20 |
| 10 | Target DOM writer passed directly to2DA.V30 |
| 18 | Retained BE2 batch-list owner |
| 20 | Result of BE2.V50, assigned only after subtree/queue success |

061510 invokes `source.V30(source, context.writer10)`, then queues the new
subtree under the actual parent through BE2.V60. This sequence does not
justify inventing a textField parent or treating the library raw document
as an existing SDK DOM page. BE2.V50 builds edit-result transport; no applied
C20/accepted document result follows merely from having a subtree.

Engine BCC descriptor227030 points through metadata2272D8 to primary table
227080. BEF descriptor227350 declares parentBCC and points through
metadata2275F8 to primary table2273A0. Both tables have exactly the same
V68=0ED3A0, V80=0ED510, VA8=0EAAF0 and attribute operations. Document00D760
explicitly accepts a BCC or BEF object and wraps its retained BCC into BEF.
The original SDK caller identity is not present in our independent raw API;
the proposed port must therefore be explicitly library-owned and detached.

## Literal text and arena construction

V68 interns an element name then calls0EA360 with kind3. V80 calls0EA360
with kind1, then0EB740 with the original UTF8 pointer/end. When end is zero,
0EB740 obtains strlen, allocates/copies exactly those bytes and a terminal
NUL, with small-string/pooled/large-string storage branches. It performs no
XML parse, entity decoding or escaping. On value-copy failure V80 releases
the newly allocated node and returns null. Text0305D0's ordinary branch
therefore creates `charCandidate` with a literal text-node child.
The proposed port must preserve bytes up to the first NUL when the native
end argument is zero, including for attribute strings. Decoding/re-encoding
UTF8 with replacement characters would change this raw-byte contract.

BCC constructors differ: default0EC250 passes byte1 to0F1E40 and attaches a
BE2 observer. Parameterized0EC260 requires16 bytes and forwards byte0 plus
the name-owner pointer at8. An explicit library parameterized byte0/NULL
name-owner scope creates a fresh BEE name owner, a kind4 root arena and BEC
namespace state without the default BE2 observer. This is a deliberate
library-owned no-history arena, not a claim about native SDK defaults.
BEF0F0520 requires a retained BCC and shares its name owner/node arena.

The proposed new arena must retain the exact seed/document, its own name
owner, and actual copied UTF8. Node identities are arena-local sidecars,
not fabricated native pointers. A detached `textResult` needs no parent or
BE2 context. Attaching it and executing edits remain separate boundaries.

## Source-derived subtree and native order

031880 first creates `textResult` with `gen` from result38. Our source is the
retained `Seed.LibraryIdentifier38`: restored native UUID byte formatting
with the already documented library cryptographic seed adaptation.
D8 is the actual document alternative count, not the source-line count.
For the supported nonemptyD8 path it then appends:

1. `charSegmentList`: every actual CHAR source in insertion-list order.
2. `wordSegmentList`: every actual WORD source in insertion-list order.
3. `text`: tagB0, full source rangeC0/C8, selectedBC, optional freezeB8,
   optional textE8/F0, then document alternatives as `textCandidate`.
4. `lines`: actual post025E40 line rows; transforms only if110 is nonzero.

CHAR0309C0 and WORD030EF0 use their real key20/28, optional tag10, optional
forceSelected40, optional prior range30/38 and freeze from field1C low7.
CHAR also has optional line60. Range strings and separate bit400000 brace
rewrite already have an owned producer. Field1C is not source flags18.
The selected CHAR/WORD marker at44 is not forceSelected40.

Fresh CHAR024D40 explicitly clears1C and sets the full qword40 to minusone.
Fresh WORD0283B0 clears qword18 (flags18 plus field1C) and sets qword40 to
minusone before publication. The supported selection stages write flags18
and selected index44, preserving forceSelected40 and field1C. Prior branches
remain unsupported unless the corresponding real prior-owner data exists.

0305D0 ordinary CHAR choices use the actual interned text bytes. Its
external400/math800 branches are separate and must initially refuse before
any invented external attributes or math payload. 030CC0's `wordCandidate`
contains actual24-byte CHAR references: `char ref=<source range> selected=
<marker16>` and freeze byte14 only if nonzero. 031360's `textCandidate`
contains actual24-byte WORD references with the same ref/selected/freeze
contract. Use the existing typed links/alternatives and scalar rows; do not
reconstruct them by comparing strings or by concatenating expected labels.

Fresh0283B0 writes qwordB8=0, producing both freezeB8=0 and selectedBC=0.
TEXTtagB0 comes from actual pending kind2 registration; it is not seed30's
INPUT source tag. Character and word tags likewise retain their selection
IDs. Full counter/registry execution remains outside the subtree.

031590 line attributes are literally `x`, `y`, `a`, `h`, not invented
`baseline-y` or `x-height` names. It emits x only when both abs(row8) and
abs(row18) are at least double FLT_EPSILON, then always y(row10), a(row18),
h(row20), and tagId(row28) when nonnegative. Use the owned current trimmed
line view, preserving its raw doubles and row order. The scoped transform
owner's actual count110=0 skips the transform subtree; do not synthesize
an empty DAF input or an enabled transform branch.

## Proposed executable checkpoint

`LegacyFreshMainDetachedTextDom.Prepare(actualRangeFormatting)` would create
the named library-owned parameterized BCC arena, validate all actual source
links/flags/current seed, and build only031880's supported subtree. Its
result would retain the exact range formatting/counter handoff/seed/document,
arena/name owner and immutable root snapshot, with raw UTF8 text/attributes,
typed numeric attribute lanes and ordered child/source-reference sidecars.
It would expose neither a caller-selected success flag nor an accepted C20
document. Existing frozen producers need no semantic change.

The native caller uses one range scratch across source, origin and reference
calls. A complete construction/failure trace must retain that single scratch
lifetime rather than claim our existing fresh-capacity prepared controls
cover every DOM call. Allocation/cancellation tests must preserve actual
prefix and cleanup ordering, including V80's release on failed copy and the
final unsuccessful-root release. Independent tests should rebuild the full
tree from actual raw CHAR40/WORD48/ref24/TEXT32/line56 inputs; expected text
must never enter inference. First supported fixtures are the same four
retained documents already passing range formatting.

Before runtime implementation: finish numeric attribute storage/tag flags
and exact031880 failure ownership, then freeze this static proof. No generic
multiline, cached/prior, external/math, XML serialization or full BE2 claim
is included in this checkpoint.
