# Prepared deletion-mask segmentation score

`LegacyPreparedMaskedMathSegmentScore` restores the finite numeric mask branch
of041CF0 alongside the existing fresh/no-deletion helper. It accepts an explicit
original-position byte mask, assignments in original coordinates, and an already
compressed activated probability matrix. Every nonzero mask byte means deleted.
This is a prepared numeric kernel, not a completed EF4 request pipeline.

The original-to-compressed row map stores the number of retained positions
strictly before each original index. Each nonnegative assignment increments the
denominator before checking its deletion byte. Deleted assignments omit their
logarithm but remain in that denominator. Only retained positions access the
compressed matrix. The score uses the F32 `2EDBE6FF` floor, portable `MathF.Log`,
separate F32 subtraction, division, multiplication and addition. Detached
diagnostic values preserve the before-score and unweighted penalty that the
native optional90 pointer receives before updating the score. No native pointers,
unwritten padding or accepted-record mutations are fabricated.

The adaptation rejects nonfinite values, probabilities outside0..1, invalid
assignments, incomplete shapes, cancellation, overflow, more than1000 original
positions, more than6000 segments or more than1,000,000 matrix elements. An
all-deleted mask correctly accepts an empty compressed matrix. Native allocator
failure returns false before numerical stores; managed preparation reports its
failure by exception instead. Native CRT logarithm equivalence is not claimed.

Verification on2026-10-18:

- 4096 prepared controls, including635 all-deleted and2720 mixed masks.
- 741 controls match the existing no-deletion helper bit for bit.
- 2142 controls distinguish counting deleted assignments in the denominator;
  2275 distinguish compressed rows from original positions.
- Independent standard-library Python oracle uses Decimal75 logarithms and F32
  rounding, checks detached diagnostics and separate final arithmetic; maximum
  observed numerical difference was zero on this Windows machine.
- 23 malformed-input/cancellation guards and mutated-input/result-copy checks.
- Fresh Managed and Windows NativeAOT captures are byte-identical, SHA-256
  `451d96def8cf37ffcda5c2ed5b79691be27565c806ce80ed1263f66d949158b5`.
- Static audit pins the entire041CF0 code-region hash and32 instructions covering
  remapping, mask tests, denominator order and diagnostic stores. Vendor binaries
  were read as data only.

Run `tools/LegacyMathMaskedSegmentScore.Probe` with one output JSON argument,
then `python -I tools/LegacyMathMaskedSegmentScore.Probe/audit_output.py capture.json`.
The static audit requires the separately preserved local research binaries.

Still required: actual request-policy ownership; correction helpers040CD0,
041240,041EC0,041660 and041BA0; provenance-bound matrix compression and output
mapping; transactional completed-record updates, final ordering and publication.
This kernel establishes neither full formula recognition nor Russian readiness.
