# Experimental global text host adapter

The opt-in AI-pen setting uses the actual Russian global dictionary path through
`ExperimentalGlobalTextRecognizer`. The previous first-stage adapter remains a
diagnostic component. The setting is still disabled by default. English and
automatic text/formula selection are not implemented by this adapter.

One loaded context owns eight model/profile resources. Each call snapshots bounded coordinates before recognition and creates its own
query and publication state. Input is bounded to32 strokes/1024 finite points
within100000 coordinate units. Caller cancellation propagates; the host's
cooperative two-second deadline retains ink. This is not a hard process timeout.

Admission requires a completed genuine winner containing exactly one raw WORD,
1..128 ordinary glyphs with actual original graph source keys, valid visible
non-whitespace Unicode and at most1024 UTF-16 code units. The unchanged native
glyph-score gate is.5..1 for every glyph. The returned minimum is explicitly an
experimental candidate score, not a calibrated probability. Unsupported or
uncertain output returns empty text, preserving the original strokes in the app.
No detached document insertion/undo or source-range receipt is fabricated.

`ExperimentalGlobalText.Probe` checks eleven input/cancellation/public-entry guards, including mutable caller snapshot isolation and shrinking collections,
the actual five-stroke synthetic MIR word and the original nine recorded words.
The optional frozen261-input mode reads only coordinates. Its internal30-second
diagnostic deadline separates algorithm/admission from host wall-clock effects;
the ordinary public MIR call still checks the production deadline. CI checks the
small corpus on Managed and NativeAOT and compares exact captures. Local checks
also publish the entire application with NativeAOT and run `--ai-pen-test`.

The independent `audit_output.py` validates capture bounds/guards/parity. With
`--raw-scores` and `--raw-words` it reconstructs every admitted text and score
from separate full-global snapshots, without reading expected labels. On the
fixed261 inputs this yields122 admissions,93 exact/29 wrong after targets are
opened, and139 retained inputs. The previous application produced29 exact words
with228 incorrect and four empty. These are limited diagnostics, not a release
quality claim; model-training overlap is unknown. No expected answer, score
threshold, language weight or beam was tuned for this corpus.
