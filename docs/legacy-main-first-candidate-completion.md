# Bounded first-candidate completion

`LegacyFreshMainFirstCandidateCompletion.Complete` consumes an actual
`LegacyFreshMainSingleLinePlainRecognitionResult` after its successful ordinary
callbacks. It joins the currently proven fresh first-alternative stages:

1. Capture the exact terminal/session/WORD/beam-owned first alternative, using
   one explicit400-byte allocation and its in-place40-byte constructor.
2. Run actual published-WORD21FBF0 quote cleanup with two separate explicit
   four-byte scratch slots, retaining their carry and the real shared pool.
3. Apply the typed freshF4=0 proof for21F320 to that same source/snapshot.
4. Expose copied scored/final numerical channels and separate raw glyph UTF8
   projections with source keys/ranges, in native279.Head18/Next20 WORD order.

The return value retains the original raw result, snapshot, typed no-op proof
and actual publication/source owners. It does not insert spaces, concatenate
words into a presumed transcription, replace confusable glyphs, choose an
acceptance threshold or enable a new app recognizer. The per-word bytes precede
the unfinished final string-wrapper/text-source acceptance stages. Projection
retains the existing explicit refusal for missing alternate/control-alphabet
owners, circled-start processing and unsupported source mappings.

## Storage and stage boundaries

The numerical snapshot computes its pure singleton finalization early. Those
bytes are returned only after the actual quote/no-op stages succeed. This is a
post-callback composition, not an emulation of native allocation-before-callback
failure ordering, a complete TOP273 header or its subsequent alternative loop.
Quote cleanup changes owning glyph ushort08 rather than the separate numerical
alternative channels. The raw08 and reference78 are preserved separately from
the normalized score storage/TOP80; the fresh -1 sentinel is not confidence.

All scratch/allocation extents, overlap, cancellation and pure ownership
preconditions run before acquiring the per-result single-use claim. A rejected
precondition leaves the result usable. Once that claim is acquired, any later
failure/cancellation consumes completion for the **same raw result object**,
including cancellation before a first glyph write. A fresh recognition run
creates a different result/owner. The claim uses weak keys and adds no persistent
strong-reference cache of completed source graphs.

There is no rollback. If quote cleanup commits a prefix and a later stage
refuses unsupported projection, the actual pool and scratch prefix remain.
The API does not publish a partial completion as accepted text or silently retry
quote cleanup against changed glyphs. Native allocation/exclusion/rescue,
enabled21F320 and multi-alternative result orchestration remain separate work.

The standalone quote, numerical snapshot and raw projection APIs retain their
existing behavior. This new composition does not edit the raw recognition class
or manufacture a different DIRECT dictionary to stand in for its CASE owner.
Source ranges describe the actual sampled point domain; document endpoint/font
layout ownership is not inferred from them.

## Verification

Five original raw-coordinate inputs traverse first TEXT, source720,
24B/24D/29B/265, actual CASE scheduling/terminal, compact publication, WORD
callbacks and this completion API. Their outputs contain five distinct WORD
records and19 glyph records. Expected transcriptions never enter the route.
The independent Python oracle decodes the pinned raw acoustic alphabet and
every captured glyph descriptor to compare exact per-word UTF8, source keys,
scored/final400-byte allocations, inactive residue and pool-record data.
Managed and fresh Windows Native AOT captures are byte-identical on this scope.

Thirty-eight guards include canceled-entry-then-success on the same actual
result, scratch/extent/overlap checks and exact single-use retry rejection.
One separate **prepared post-inference fault** changes an actual pool's leading
label to synthetic193 and a later descriptor to an unowned alternate alphabet.
Completion writes the leading apostrophe134 through the real quote kernel,
then correctly refuses raw projection. Its quote prefix stays intact and a
retry is refused by the completion claim. This controlled fault is not evidence
that the recognition networks emitted a synthetic quote or that an alternate
alphabet owner was restored.

`audit_main_first_candidate_completion.py` reads two pinned native ranges and
eight instruction pins. Native22198E quote cleanup precedes the99-gated21F320
call at2219AD; later227CC1 invokes273.V60. It separately checks the new managed
composition's explicit call order. The underlying snapshot, quote, freshF4
and ordinary UTF8 components retain their own independent proofs.
No vendor DLL is loaded or executed.

```
LegacyMainFirstCandidateCompletion.Probe AK LK engine separation case publication line30 line50 capture.json
python tools/LegacyMainFirstCandidateCompletion.Probe/audit_output.py AK.res capture.json
python tools/handwriting/audit_main_first_candidate_completion.py static-report.json
```

This bounded completion is not full engine restoration or measured Russian
cursive accuracy. The next owning joins remain final string-wrapper output,
general context/normalization branches, additional alternatives and an honest
acceptance/quality policy over real handwriting data.
