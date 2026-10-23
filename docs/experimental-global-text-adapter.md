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
non-whitespace Unicode and at most1024 UTF-16 code units. The host policy
requires each native-derived glyph score to be in.5..1. This cutoff was added
by the host adapter; it is not a recovered native recognition threshold.
The returned minimum is explicitly an
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

## Public application-path diagnostic, 2026-10-24

The nine recorded tracks were also tested through the actual application's
`AiRecognitionModels.Current`, with the experimental setting enabled and all
nine embedded resources checked against their complete hashes. This is a
headless managed application diagnostic, not execution of the released Native
AOT installer. It does not read user profiles or start the GUI.

The public two-second calls returned text for indices0,3,6 (`да`) and retained
the ink for indices1,2,4,5,7,8. An instrumented copy of the same published path,
with a30-second deadline, produced identical results. Its trace identifies the
six refusals as glyph scores below.5, with minima.1555675,.10441014,.043263048,
.14691281,.0883231,.12353215. No observed call threw or expired; public calls
took74..1544ms on this CPU2 run, excluding resource loading and including cold
JIT on the first call in each process. These are local timings, not a latency
guarantee. Extending the deadline did not recover these six inputs.

The actual `AiInkLayout` then consumed cached recognizer results without any
additional inference. Coordinates, chronological points and source coverage
were preserved; accepted text survived layout. This closes the distinction
between the raw-WORD diagnostic and the application adapter: raw4/9 exact
output must not be reported as application4/9 replacements. Original pen-up
boundaries are missing from these nine tracks, so neither count establishes
general Russian cursive quality. No admission threshold was changed.

The source/runtime packet SHA256 is
`0eefa317acfab3fd0988b14688e5dfcc3591bff78a107cee4982c95a2665aafe`;
1230 pinned files were checked before and after both bounded processes. The
diagnostic runtime copy enables JSON reflection only for trace serialization;
the production adapter remains unchanged. Local immutable captures are
`root-ru-public-app-index0-20261024`, `root-ru-public-app-indices1to8-20261024`
and `root-ru-public-app-verified-v2-20261024` under the build-artifact root.
