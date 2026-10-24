# Fresh WORD source-key and text projection

`LegacyFreshMainTextSourceWrapper` joins the existing actual source refresh,
C21 operations, source-key interner,213A0 publication,024D40 metadata and
021760 reuse. It uses the identical completed candidate, original document,
glyph pool and private UTF8 string pool throughout. A caller cannot supply a
positive-source flag, source ID, selected text or success callback.

This is a bounded **pre-final-choice projection**, not a completed native
string wrapper or accepted OCR. The current factory requires exactly one
fresh first-alternative WORD and a forward ordinary source interval. It
exports the genuine219580 source prefix, but does not yet execute219580's
additional word alternatives,13D360 pruning/ranking or13D000 rewriting.
`WordSourceCollection.IsTopV68Completed` therefore explicitly returns false.
0283B0 WORD/TEXT registration, final alternative assembly and document
acceptance are subsequent stages.

## The two position collections are different owners

Native1416B0 stores a pointer to a64-byte275 **span owner** in each279 list
node. The selected120-byte WORD row is separate. Its span owner has the
source vector at28/count30/residual pointer38.13FCD0 initializes those fields
empty. Exporting the WORD's glyph-reference vector as source geometry would
reinterpret the wrong owner.

The normal result getter00F460 calls273.V68=13E7F0 after successful2271F0.
V68 lazily calls controller.V88=219580 for each unprepared span owner, then
sets its2000 bit even after a failed prefix. This occurs even when2271F0's
external argument was null: skipping its earlier direct219580 call does not
prove that a source vector remains empty. See the independent
[result-source lifecycle audit](../tools/handwriting/audit_main_result_source_lifecycle.py).

`LegacyOwnedMainWordSourceCollection.ExportBeforeWordAugmentation` implements
219580's real count/fill source prefix against the exact published span's
First/Last graph contexts. It preserves the26*N layout:24-byte source pairs,
then two residual bytes per pair. On this typed fresh single-descriptor
no-protocol graph, the successful forward1A9AE0 mode2 path equals its
already verified mode0 path. Mode2's additional final-node protocol append
is absent by the same constructor/source lifecycle proof used by the owned
span-source service. Reverse/protocol spans and source publication across
several WORD owners remain refused here. Native278 lookup uses exact First/Last
equality; the earlier description of containing-span reuse was incorrect.

The source pairs are projected through the actual original raw document and
05AFA0 codec. That WORD collection clips each ordinary glyph's first-pass
source vector. The original whole-document range is separately retained for
the later Contains118 degenerate-endpoint check. They are never substituted
for each other. Source IDs retain the NEW library root's original caller
stroke ordinals; they are not claimed to be SDK page object IDs.

## Ordered source processing and metadata

The first pass refreshes all non-skipped actual WORD references before any
alternative selection. Native kind1 skipping is exactly: kind-low3 equals1,
`run & 0x7FFC` is **nonzero**, and its low two bits equal3. TEST/JBE machine
code pins the nonzero condition; a zero-length NBSP descriptor is not skipped.

After each actual alternative selection succeeds, the second pass processes
its source key. It preserves the native outside-degenerate downgrade to
mode4, U+200D insertion endpoint, first later-source overlap only, ordered
equality and subtraction of the appropriate current/later vector. Only the
private owning path creates `LegacyOwnedMainProcessedGlyphSource`; numeric
controls cannot pass a fabricated processed owner into the public wrapper.

A new key runs213A0 before024D40. Positive processed keys register pending
CHAR selections through the same document's fresh0210C0/C51/C4D context.
Its counter starts at N from successful original-object insertion, not zero.
The metadata retains exact initialized scalar fields and named vector/string
sidecars; it does not serialize managed IDs as pointers or invent unwritten
padding/capacities.021D10's out byte starts1, becomes0 upon visiting any NEW
ordinary C4A input, and remains1 on an empty traversal.01FAE0's record-flag
aggregation, degenerate bit and prior byte are preserved. Source line IDs
come from the actual single descriptor and retain the original signed-arc
condition even when a source is downgraded to an insertion.

An existing key invokes the complete021760 kernel using the **same** string
pool. It scans only the source node's original record count and keeps its
native append/failure-prefix semantics. Selected markers are not replaced
with an arbitrary surviving alternative. A missing marker refuses the
projection. String bytes remain readable only while their wrapper owner is
alive. Publication is single-use, including failed/canceled prefixes; emitted
records, source registrations and interning are not rolled back.

The public `LegacyFreshMainTextGlyphChoices.Prepare/Publish` signatures and
their behavior remain unchanged. Internal seams separate first-pass source
refresh, actual alternative selection and fresh publication so the new-key
and reused-key paths share their real emitters and string owner.

## Verification

The probe's standard ten model arguments are AK, LK, engine profile,
separation profile, case data, publication data, normalization data, circled
payload data, line30 and line50; argument11 is the output capture. Optional
`--controls path` loads original prepared numeric cases. Optional
`--recorded manifest` is an opt-in private diagnostic; its pinned nine tracks
have no pen-up events and are each treated as one continuous stroke.
Expected transcription labels are never read by inference or the probe.
Samples and private captures are not shipped with the repository.

```powershell
python tools/LegacyMainTextSourceWrapper.Probe/reference_controls.py E:/BlurMask-build-artifacts-20261014/text-source-wrapper-controls.json
dotnet build tools/LegacyMainTextSourceWrapper.Probe --artifacts-path E:/BlurMask-build-artifacts-20261014/text-source-wrapper-build
python tools/LegacyMainTextSourceWrapper.Probe/audit_output.py E:/BlurMask-build-artifacts-20261014/text-source-wrapper-managed.json
python tools/handwriting/audit_main_text_source_wrapper.py E:/BlurMask-build-artifacts-20261014/text-source-wrapper-static.json
```

Managed and Windows NativeAOT pass2048 independent prepared source-lifecycle
cases,8449 emitted keys and524288 exhaustive ushort/kind skip predicates.
The independent Python interval implementation checks later-vector mutations,
endpoint downgrades, joiners and overlap ownership. These prepared cases are
not decoder accuracy samples.

Seven real raw inference inputs produce26 processed glyph sources and64 full
40-byte publication rows. The opt-in nine recorded tracks produce31 sources
and68 rows. An independent raw-AK/Unicode/source-position audit verifies UTF8,
all row bytes, marker choice, owner-local string identities, projected WORD
clip, source keys, CHAR metadata/counters and per-WORD aggregation. These
actual inputs contain zero reused keys, zero negative keys and zero overlap
branches; no broader real-inference branch coverage is claimed. The existing
reuse kernel and negative source service retain their separate controls.

The older public TextChoices capture remains byte-identical in Managed and
NativeAOT: SHA256
`eb2c197d59781b75805cdcf029a84baf069f97fed0012978880bc219b730ebfc`.
Its26 real glyphs/64 alternatives/52 selected bytes, seven genuine independent
span-source projections and592 prepared fractional-position checks still
pass. Static auditors read pinned PE bytes only; no vendor DLL is executed.

`PrepareCompletedSources` also accepts the actual successful bounded
`LegacyFreshMainNoCandidateSourceCompletion`. It retains that owner's existing
26N source collection rather than exporting another prefix. An unrun or failed
completion is rejected. The historical `Prepare` path remains available for
prefix diagnostics; neither factory claims0283B0 document acceptance.

The probe option `--complete-sources true` runs this composed path. Seven
control inputs and nine recorded tracks pass both Managed and Windows Native
AOT with byte-identical captures. All published sample payloads match the
historical wrapper; its default capture SHA256 remains
`3a3e0aacfe1565454b95edf33a400459f4963291bd68c8ae9eb23feb81af87f7`.
