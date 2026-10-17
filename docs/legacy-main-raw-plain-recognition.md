# Fresh single-line raw ink to plain words

`LegacyFreshMainSingleLinePlainRecognition` composes existing owned producers:

1. Original physical strokes pass through main grouping, descriptor reordering
   and same-factory first-stage TEXT inference. Multiple first words survive
   the source720 transfer; they are not replaced by a supplied transcription.
2. Actual source720 rows feed24B guides and24D/29B/265 preprocessing.
3. The one-line sampler and context network build the actual main graph. Its
   CASE dictionary, ascending scheduler and first terminal selection share
   that same source, model and context owner.
4. The actual selected compact result supplies backtrack rows, native flushes,
   per-flush residual association and the owning21FE30 word callback.

The result retains the preprocessing, sampled graph, selected terminal, exact
publication session and typed callback outcomes. `PublicationCompleted` reports
completion of that callback sequence. It is neither an OCR acceptance decision
nor a calibrated confidence value. TOP alternative assembly, subsequent result
choices, quote/string normalization and final language acceptance are separate.
The app recognizer is not switched to this path.

Initial WORD120, allocated record40, label stack256 and request stack64 storage
are explicit inputs. All component ownership, extent and cancellation checks
remain active. Cancellation during a publication preserves the component's
committed prefix and stops that session. An unsuccessful callback is retained
as a typed result and terminates the sequence; it cannot be acknowledged as a
successful publication.

The existing sampler requires one genuinely assigned ordinary descriptor over
the full physical interval. Two lines are refused rather than concatenated.
Empty first-stage guide fallback, associated sources, unsupported glyph branches
and native long-source publication gates retain their explicit component
refusals. No placeholder word, synthetic guide or successful long-source
winner is introduced by the compositor.

`LegacyMainRawPlainRecognition.Probe` exercises five original coordinate inputs
through the complete bounded chain, including inputs with multiple first-stage
words. A genuine two-line input is rejected at the single-line boundary. Thirty
scratch/cancellation guards are checked. The output oracle independently
checks WORD linked references, reading order, source graph spans, packet
boundary overrides, flags and vector count/capacity. It does not infer expected
text from these geometric fixtures or measure handwriting accuracy.

Managed and Native AOT captures must match byte for byte. The component proofs
remain in `legacy-main-text-input.md`, `legacy-main-owned-glyph-flush.md` and
`legacy-main-owned-word-publication.md`; this integration check does not replace
their independent numerical and byte-level oracles.

The probe takes AK, LK, engine profile, separation profile, case data,
publication data, line30, line50 and capture path. Its `audit_output.py` takes
the resulting capture. It is headless and does not load vendor binaries.
