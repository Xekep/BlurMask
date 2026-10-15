# Actual Russian coordinate diagnostics

`tools/handwriting/import_russian_tracked_coordinates.py` reads the pinned
[authors' public archive](https://github.com/Skvayzer/russian_handwritings_tracked)
at commit `c6629026b1fdef51fd08e3d2fddffd82f97e60a5`. Its archive SHA256 is
`2960052698884e00d8fa428295f538d52c30f47ea3f7695e5c7be610fd943edf`.
The [Mendeley release](https://data.mendeley.com/datasets/3h6h5d7xg2/2)
identifies the dataset and its CC BY 4.0 data license. The authors' repository
code is GPL-3.0. The importer executes none of that downloaded code and does
not add dataset assets to this repository.

## Fidelity boundary

The archived scanner stores flat XY coordinates and time deltas. Its mouse
release handler does nothing; the saved CSV does not contain pen-up state.
Rendering uses points rather than a line between every saved coordinate. Thus
turning a complete CSV into one physical stroke can introduce connecting
segments that the writer never drew. Timing does not prove exact stroke breaks.

The importer explicitly preserves each record as one flat trace, inverts its
Y-up coordinates and translates the bounding box to zero. It does not rescale,
smooth, insert points, infer pen-ups, change case or replace confusable scripts.
Time deltas and CSV hashes remain in the output for audit. The filename
`cъешь` deliberately retains its original Latin `c`. Personal names and
signatures are outside the explicit letter/digit/pangram selection.

These results are diagnostics of real recorded coordinates under a lossy
adaptation, **not an exact Russian handwriting accuracy benchmark**. A corpus
with original pen-down/pen-up events is needed for that claim.

## Current first-stage result

The default selection is three explicit sessions `w_0_1,w_1_1,w_2_1`, eleven
letters/digits per session (`а и м р А И М Р 0 1 2`) and all nine pangram words.
Selection is fixed before recognition; expected text is used only by the
offline report. All sixty selected records were imported and processed by
the owning first23F Native AOT entry, with no unsupported cases or missing
published words.

| Subset | Records | Exact literal | Case-insensitive literal |
| --- | ---: | ---: | ---: |
| Letters/digits | 33 | 16 | 19 |
| Words | 27 | 2 | 2 |
| Total | 60 | 18 | 21 |

The two exact word outputs were `да`. Other genuine first-stage outputs include
Latin `a` for written `а`, `M` for written `м`, alongside correctly selected
Cyrillic `А`, `И` and `М` on other records. This is before main240 linguistic
refinement and final winner selection. No postprocessing changes those output
strings. The result does not establish full Russian or joined-cursive support.

Every unsupported record, import error, null publication and literal mismatch
is retained in the report. Case-insensitive comparison is an explicitly named
offline metric, not a recognition correction. Private captures also contain
the real primary98/B8 records and per-group final costs.

```powershell
python tools/handwriting/import_russian_tracked_coordinates.py self-test
python tools/handwriting/import_russian_tracked_coordinates.py import tracked.zip corpus.json
LegacyFirstOwnedRecognition.Probe AK.res engine-profile.json separation-profile.json captures.json corpus.json
python tools/LegacyFirstOwnedRecognition.Probe/audit_output.py AK.res captures.json
python tools/handwriting/import_russian_tracked_coordinates.py summarize corpus.json captures.json report.json
```

Only the own-data CSV adaptation/metrics tests belong to portable checks. The
downloaded corpus stays outside Git and is optional; a network dependency is
not introduced into the normal build.
