# Main result to final string wrapper: remaining joins

This is a read-only map of the legacy `273 -> 0283B0 -> 024F30 ->
0213A0 -> 021000` path. The owning main publication and raw UTF8 projection
already produce real selected glyph references. They do not yet constitute
the final text-source wrapper. No runtime, recognition policy or measured
transcription was changed for this investigation.

## Current integrated boundary

The following investigation is historical. The owning
[actual text-choice adapter](legacy-main-text-choices.md) now joins ordinary
and fresh negative source refresh,13A830,full273.V50 and213A0/021000, with
05AFA0 positions in an immutable original raw document. FullV50 uses distinct
control2A/alternate owners; it does not substitute the acoustic alphabet for
control lookup. Ink C21.V80=07FA50 is ordered append, not interval union.
024F30 source-node interning/reuse/metadata and0283B0 final document assembly
remain separate. None of these component joins enables accepted app OCR.

## Subsequent ordinary refresh adapter

The standalone13A830 kernel and owning nonnegative ordinary CASE217670
refresh have since been joined in `LegacyOwnedMainOrdinaryGlyphRefresh`. Its
source exports and score channels come from the actual completed main result.
This closes the positive ordinary callback dependency described below; the
complete024F30/0213A0/021000 source-document wrapper, span sources and native
failed-refresh retry semantics are still separate. See
[the current adapter and checks](legacy-main-ordinary-glyph-refresh.md).

## The actual input is an owner selection, not one flat word string

`0283B0` takes result wrappers whose `+10` points to TOP273. Each actual
TOP279 alternative has a linked list of published WORD nodes; the consumer
iterates those nodes and passes each WORD275 to `024F30`. The latter starts
from the WORD's reference vector `+20/count+28`, but does not simply emit
that vector as one UTF8 string.

For each reference, `024F30` canonicalizes the low24 node, arc16 and bit56,
looks up its actual274 owner, projects source geometry, and asks
`273.V40 = 13C6E0` for an ordered vector of **4-byte alternative selections**.
The two ushort fields in those rows are not two glyph references. Native
`13A830` writes the actual record ordinal as a 32-bit integer; `0213A0` uses
its low ushort shifted40 and ORed with the canonical owner key. It preserves
the complete four bytes when removing rows.

`13C6E0` first checks owner packed-count bit4000. If clear, it calls the
actual TOP's controller `+10.V90`, when present, and then sets bit4000.
For ordinary240, table `2E7CB0+90` is **217670**. This lazy callback can append
additional glyph records before ranking. It is not equivalent to declaring
the current portable pool snapshot refreshed, nor to using the selected
WORD's one reference as its whole alternatives vector.

`217670` has a real producer dependency: it reads the owner's graph source
ranges and compact2460 records, resolves symbols through216860, and obtains
frontend1B8/1C0 scores. It writes record0C from compact byte21/255,10/14 from
the corresponding score services, source flags and optional geometry. Its
negative-arc branch publishes span-source references. Existing owned main
confidence, symbol resolver and source geometry cover parts of those
services, but an owning implementation of this callback and its lifecycle
has not been joined to `13C6E0`.

The packed owner bit4000 is distinct from descriptor-dedup state and from
publication flags. `LegacyNativeGlyphRecordPool.CopyOwnerNodes` currently
preserves count and descriptor-dedup information; that snapshot is not proof
that native lazy refresh ran. Native failure also sets4000 after the attempt;
a portable implementation must preserve that ordering rather than silently
retry a partially mutated native-equivalent owner.

## Smallest complete next kernel

The independently transferable next block is **13A830** over one genuine
complete40-byte owner array and its incoming selected ordinal:

1. Clear the output selection count and scan owner records in stored order,
   using native signed packed-count extraction.
2. Deduplicate equal kind-low3, complete descriptor08 and publication06
   high-byte identities. Descriptor ID00 is not the deduplication key.
3. Insert a new identity in descending raw float32 record0C order. Equal
   finite scores retain earlier order. A better duplicate replaces its
   representative and moves it toward the front. Preserve native unordered
   comparison behavior if nonfinite input is supported; a generic sort is
   not an exact replacement for this insertion loop.
4. Track replacement of the originally selected record and return its
   resulting selection position, orFFFFFFFF if unavailable. Emit complete
   uint32 record ordinals in the selection vector.

This kernel needs no invented confidence, raw point IDs or expected labels.
An implementation could expose an explicitly prepared owner-selection stage
with exact immutable pool provenance. It would not by itself establish
`217670` refresh, wrapper acceptance or document geometry. No new runtime
adapter was added without approval. The subsequent owning adapter must call
the real lazy producer or require an explicitly named prepared-refresh
boundary; it must not accept a caller boolean as native completion evidence.

## Filtering and byte publication already implemented

`LegacyNativeTextGlyphCaller` contains the bounded fresh **0213A0** loop,
and `LegacyNativeTextGlyphPublication` contains **021000** plus
019030/018B40 classification. These remain reusable prepared-stage APIs.

* Wrapper byte08 enables removal only for kind0 whose symbol equals actual
  primary alphabet runtime word0E. `053F20` copies serialized header+6 to
  runtime+0E. Pinned RU HeaderE is178, while actual space is180.
* `0283B0` obtains this wrapper byte through property **12D0065** on the
  actual result's knowledge owner. Setter00DCC0 writes knowledge99; the
  matching fresh no-external policy already proves99 false. This false
  value must be derived from that same owning source lifecycle, not guessed
  from the language, alphabet or a caller-supplied switch.
* Successful empty glyph emission removes the selected row. Removal decrements
  the marker only when the retained index is strictly before it. Emission
  failure is failure, not empty success. The fresh path zeroes destination
  vector68/70 in024F30 before calling0213A0.
* Exactly E298B9 and E298BA become F09F9981 and F09F9982 respectively.
  Longer text and repeated occurrences are not searched or replaced.
* Exactly four bytes F3B08080/F3B08081 set source flag100000 after021000.
* 021000 zeros record00, copies glyph publication06 low10 bits into flags08,
  writes byte count20 and interns the exact byte interval through the
  **source2DA+118** owner. Embedded NUL counts are retained by the intern
  owner; first-cluster/literal classification has its own native semantics.

`LegacyNativeCircledUtf8Writeback` already supplies the bounded canonical
scratch/circle-writeback helper. Therefore circle processing is not a missing
numeric primitive. The owning wrapper still must connect the actual273
emitter's primary alphabet, dynamic control2A at TOP+18 and alternate byte
storage separately. Existing ordinary projection deliberately refuses their
unowned nonordinary branches. It must not replace the control owner with the
acoustic alphabet. `021760` is a separate existing/reused-record branch,
with intern-identity deduplication and different selection updates; the
fresh caller API does not silently claim that branch.

## Source identity and accepted output remain separate

`024F30` also constructs source intervals through global2BE.V30=05AFA0,
using owner source records and actual24-byte references. It merges/intersects
those intervals, finds or creates source record nodes through01B4E0, then
uses024D40 to attach the new string records and retained selections. The
actual input descriptors/prefixes used by this path are not created by the
current main raw UTF8 projection.

The executable prepared pieces already exist:

| Piece | Existing implementation | Missing owning input/join |
| --- | --- | --- |
| Actual274 signed reference lookup | `LegacyNativeGlyphRecordOwner` / pool snapshots | Lazy217670 owner refresh and bit4000 lifecycle |
| Descriptor/token/raw UTF8 | `LegacyNativeGlyphUtf8Tokens`, ordinary/circle emitters | Dynamic TOP control and alternate owners for their branches |
| Fresh filtering/classification/interning | `LegacyNativeTextGlyphCaller`, `LegacyNativeTextGlyphPublication`, `LegacyNativeUtf8StringPool` | Actual selection vector/marker, source2DA string-owner lifetime |
| Legacy source-key lookup and indexed source position | `LegacyNativeTextSourceInputMap`, `LegacyNativeTextSourceRangeProjection` | Real wrapper-selected40-byte rows, group prefixes, original source object indices |
| Cropped geometry and source-line publication | Existing selected-point and prepared result-line APIs | Accepted record ownership and complete0283B0 postprocessing |

Main graph keys are node/arc identities. Sampled ranges are not document
object indices. Parent-retained/original raw point chronology is not the
wrapper's cumulative group-prefix table. None of those may be substituted
for another to make05AFA0 or downstream line metrics appear integrated.

After024F30,0283B0 still runs alternative aggregation and source postprocessing
such as025E40/027670/0248D0,020920/023260 and037E80/026EF0. This investigation
does not assert that every one of those functions is required for every
fresh single-word case, or that they can be omitted. Their actual branch
guards and owning producers need separate closure before final accepted
text/document positions can be reported.

### Actual WORD caller boundary

The normal container getter00F460 calls controller.V80=2271F0 at00F65B and,
after success, unconditionally calls TOP273.V68=13E7F0 at00F6A5. Thus the null
external argument only skips the earlier2271F0 augmentation path; it does not
prove empty WORD source vectors at the subsequent document wrapper boundary.
13E7F0 visits all alternative WORD span owners, invokes controller.V88=219580
when span-owner packed10 bit2000 is clear, and sets that bit even when the
callback failed. It then requires13D360 and13D000 in that order. A portable
source-export prefix must not be reported as completion of this whole stage.

13D000's three literals are U+2610/U+2611/U+2612 (checkbox states), all absent
from the pinned Russian acoustic alphabet. Its actual alphabet lookup and
single-scalar WORD predicates still need their owning bridge before a scoped
no-op can replace the call.13D360 is a separate candidate aggregation/pruning
stage; neither the absent checkbox labels nor null external arguments justify
skipping it.

Further static inspection distinguishes the64-byte275 span owner from its
120-byte WORD alternatives. Native279 list-node field00 refers to the span
owner; a portable `PublishedWordNode.Word` directly exposes the selected
120-byte record instead. Span-owner source vector28/count30/residual38 still
needs its producing lifecycle. It cannot be replaced by a union of already
projected glyph ranges.

0283B0 first collects source vectors for the actual span owners, applies
WORD-level overlap operations, and interns the resulting key in the separate
WORD tree at result+78 (call028E25). Fresh024F30 gets `wordNode+18`, whose
field20 is the WORD clipping range vector. This is distinct from the original
document range retained in024F30's local78 for degenerate containment.
Using the whole raw document for both inputs does not establish the caller.

For a new WORD node,028F9D loads its flags (`node+30`),028FA0 clears bit23,
028FA7 sets bit21, and028FAD stores them. The fresh024F30 call is02901D;
the other alternative path calls it at0292B1. Selected payload flags are
combined at029080/02908C/02909A. WORD0210C0 registration at0290DA is conditional
on bit21 being clear. TEXT registration is a later kind2 call at02962A.
These are caller-provided seeds and separate selection levels, not flags
invented inside024F30.

### Postprocessing is not just concatenation

The direct call order after TEXT registration includes the following still
separate owners. Static hashes identify their bodies but do not prove an
applicable no-op for a new portable document:

| Callsite | Callee | Observed boundary |
| --- | --- | --- |
|029647|025E40|Returns early only when result field100 is zero; otherwise inspects selected WORD/glyph alternatives and their line identities. |
|029680|026090|Conditional on multiple input results, field22A and a retained nonnull source cache. |
|029693|027670|Also gated by field100; can extend selected glyph lists between WORDs. |
|0296AE|0248D0|Unions WORD source ranges; uncovered TEXT ranges require238B0 processing. |
|0297D7|01FD40|Traverses source nodes and resolves selected alternatives and original document object kinds. |
|0297DF|020000|Marks the selected WORD/character hierarchy with08000000 and retains chosen alternative ordinals. |
|0297EA|020920|Processes ordinary character and WORD source nodes; sets result1F0 only after both traversals succeed. |
|029801|023260|Further owning result/source processing, not an inferred confidence threshold. |
|02985C|037E80|Builds actual source transforms throughDAF; optional document-tree serialization is a distinct flag. |
|029876|026EF0|Matches retained external span input against the selected WORD hierarchy; its empty-input return needs actual input provenance. |

None of these calls licenses changing `PublicationCompleted` into final
accepted-text readiness, guessing calibrated confidence from TOP80, or omitting
source metadata merely because a raw UTF8 string is available.

## Static provenance

Only pinned PE bytes/decompiler output were read. No vendor binary was loaded
or executed, and no GUI or training sample was used to decide a branch.
Text PE SHA256:
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.

| Inclusive start / exclusive end | SHA256 of pinned native byte interval |
| --- | --- |
|0213A0 /021760|1b14a995679d2234a4d35b1042b2e21c59b44212bfd2c6b74b7cb87b8bbe7e2f|
|021000 /0210C0|ff78128dd66d165f6585e20178cf45f9489dd7b1c0d275742be5ff6021741a09|
|024F30 /025940|e838efca10bf49d0762690504804658e6b3cecee451beed108eb88b2e77d400c|
|0283B0 /029990|fa2576435b07665bfe72ead6e2da50229aadea857719d8c27ab507ef9b8ab195|
|13C6E0 /13C840|6c2600caf2d7b6b388aedfe41f0be739896e5adbb72a5413464c07f006f2b176|
|13A830 /13ABA0|9381032ff21ab1220e8bb50266882281a4f17528c00ba036a572e1c13bc940eb|
|217670 /217DB0|8321a0ed9770f4e6f4ff036643ff8d1490ad6989dd616084b739dd5a2c0b176b|
|00DCC0 /00DD60|f8e3066abaedaeb3976fce956f7764f4c3f5960abbb34b766114949db1b81750|

Static table bindings checked directly:273 table2DC740+40=13C6E0,
274 table2DC680+60=13A830,240 table2E7CB0+90=217670. These pins identify
the actual build; hashes alone are not behavioral tests for a future port.
