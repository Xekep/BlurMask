# Completed Russian text publication in the opt-in AI-pen adapter

The existing experimental-text setting now loads the resource-complete Russian
adapter. It performs one recognition run on the original selected document,
completes the live word source and publishes CHAR/WORD/TEXT before extracting
text. It no longer concatenates the earlier raw-word snapshot on this path.
The setting's default remains unchanged. The historical eight-resource API is
explicitly documented as raw-only and is retained for existing diagnostics.

Actual active compact flags select completion before any completion claim: no
bit8 uses the old typed source completion; any active bit8 uses owning
augmentation. An unsupported augmentation is not retried through the other
branch. The nine current actual cases exercise augmentation; current live q0
runtime compatibility is still unproven.

The new loader takes normalization and circled-character resources in addition
to the previous eight inputs. It copies the acoustic bytes before loading the
model context and retains that same private snapshot for later typed source
calls. Native table loaders authenticate the Unicode data. The app embeds these
two additional files (60928 bytes total). A PE-only reader verifies all ten
experimental Russian resource payloads in the compiled app without loading or
executing the target assembly.

## Replacement and admission

Whole-input replacement requires actual WORD coverage. The adapter compares the
normalized union of the typed original WORD ranges with the union of the
original document's selected inputs. The always-full null-projected TEXT key
cannot establish that the word consumed all ink. Missing strokes, tails, middle
gaps, empty words and extra object ranges refuse replacement.

Selected character links must retain the same document, wrapper, metadata and
ordered current word references. Text comes from completed selected TEXT;
candidate scores come from byte16 of those references' actual live glyph
records. UTF8 scalar weights and stale raw-word scores are not substituted.
The existing finite .5..1 host gate, text filters, output bound and2-second
deadline are unchanged. These scores remain uncalibrated experimental candidate
scores. Unsupported or uncertain requests preserve ink.

## Verification

Root ran the unchanged nine-recording corpus once through the new public2-second
route, with one source invocation per recording. All nine reached completed text
and covered their complete original input. Acoustic evaluation counts remained
unchanged across completion/publication. Three calls returned `да`; six preserved
ink because the selected native glyph score failed the existing host gate. This
matches the accepted indices/text of the earlier fixed-corpus public report;
it does not demonstrate improved Russian handwriting recognition.

An independent DATA audit derived fourteen captured glyph texts from the original
acoustic alphabet, verified native score bits and result confidence, checked all
nine original ranges against the source point counts, and rejected eight corrupt
captures. It reads no target labels. The recordings lack original pen-up metadata;
each remains one physical stroke, without inferred breaks. They are not a
representative cursive-quality benchmark.

Observed calls took244..1348ms in this single CPU2/affinity3 run. A bounded
FirstChance exception observer adds diagnostic overhead, and the process loads
its model context once. These numbers are not a general latency guarantee or a
controlled performance comparison. There were no deadline refusals in this run.
No30-second diagnostic retry or second recognition pass was performed.

Evidence: immutable source packet `5f1c9483…`, root artifact directory
`root-completed-adapter-nine-20261024`, capture SHA256
`8c2af3d018de7e2c4d43e8f645f3df6585ae4b27f26c5ff0ef6557ef5a104b6a`.
All source/resource/runtime pins were checked before and after the actual run.
Root then built the merged library, both probes and app with0warnings/0errors,
ran35 prepared policy/coverage/delivery controls, and verified all ten compiled
app resource payloads. The nineteen resource/recording pins also match committed
Git bytes, independently of working-tree line endings.

## Reproduction and incomplete work

`ExperimentalCompletedGlobalText.Probe --prepared REPORT` runs host-policy,
dispatch, coverage, cancellation and Unicode-delivery checks without inference.
The separate actual batch requires explicit `--actual PLAN FRESH_DIRECTORY`.
Generate its plan using
`python -I tools/ExperimentalCompletedGlobalText.ActualBatch/write_plan.py REPO PLAN`;
`--validate-plan PLAN REPORT` performs only file checks. Actual execution belongs
under a CPU2/affinity3,45-second watchdog. The portable independent audit is
`python -I tools/ExperimentalCompletedGlobalText.ActualBatch/audit_actual.py REPO PLAN CAPTURE FRESH_REPORT`.
CI runs prepared controls, validates the plan and reads app PE resources;
it does not run this actual batch automatically.

The loader retains an additional572763-byte acoustic snapshot. Diagnostic mode
returns detached bounded records only: at most128 glyph40 records,1024 total
UTF8 bytes, existing bounded range arrays and scalar counters/reasons. Public
calls allocate no diagnostic holder. No returned DATA can issue a source or
completion. Whole-process memory/RSS is not established by these payload counts.

The selected TOP route is still distinct from all275 WORD alternatives. Complete
WORD-choice publication, the remaining recognition branches, English dictionary
integration and representative Russian/English cursive quality remain unfinished.
This change does not claim complete engine reconstruction, new Native AOT/Linux
runtime verification, or release readiness for those unfinished parts.
