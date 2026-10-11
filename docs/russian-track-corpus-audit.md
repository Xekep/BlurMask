# Real Russian coordinate-track diagnostic

This opt-in headless audit runs the actual checked Russian acoustic/lexical resources on independently collected handwriting. It does **not** establish Russian cursive accuracy or SDK equivalence: the public source omits pen-up events, its guide-line semantics are unspecified, and overlap with the original model's training writers is unknown.

## Provenance and format

The primary [Russian Handwritings Tracked dataset](https://data.mendeley.com/datasets/3h6h5d7xg2/2) credits Dmitry Iatsenko and Konstantin Smirnov, DOI `10.17632/3h6h5d7xg2.2`, and licenses its data CC BY 4.0. Its [source repository](https://github.com/Skvayzer/russian_handwritings_tracked/tree/c6629026b1fdef51fd08e3d2fddffd82f97e60a5) is pinned to commit `c6629026b1fdef51fd08e3d2fddffd82f97e60a5`. Archive SHA256:

```text
2960052698884e00d8fa428295f538d52c30f47ea3f7695e5c7be610fd943edf
```

The repository's capture code is GPL-3.0; this project does not copy it. Our importer independently reads the documented records with Python's CSV library. The public samples and capture source stay in a private research directory and are not bundled with the application.

Inspecting the pinned [capture scanner](https://github.com/Skvayzer/russian_handwritings_tracked/blob/c6629026b1fdef51fd08e3d2fddffd82f97e60a5/scanner.py) establishes these limits:

- Each raw file contains one quoted CSV row of flattened XY coordinates. Its `_times` sibling contains one elapsed-millisecond interval per point. Pyglet coordinates have Y increasing upward.
- Mouse-drag records are sampled after an elapsed-time threshold; the scanner stores `x+dx, y+dy`. The importer preserves those values as recorded rather than correcting them.
- Mouse release records no event. There are no pen-up sentinels in the 2,812 character and 333 extra-word raw files inspected. Neither time gaps nor coordinate jumps are interpreted as pen-up.
- The source draws guide lines at Y = 150, 200, 250, 300, but does not explicitly assign a native recognition baseline or x-height. The audit's `baselineYUp=200, height=50` is a caller-supplied hypothesis, not recovered ground truth.
- The pinned tree contains 13 distinct writer IDs across 37 sessions, although the dataset summary says 12 writers. Physical author identities are not inferred from that discrepancy.

Rasterized `mnist-like/*.csv` files are not trajectories and are excluded. Original point order, intervals, coordinate/timing SHA256 and a generated-track SHA256 remain in the private manifest. The recognizer does not consume these timing intervals; its prepared endpoint uses the recorded point sequence. One artificial unsegmented path is supplied per sample, with partial-group cache scope. It is not called a genuine physical stroke.

## Reproduction

Selects three sessions (`w_3_1`, `w_7_1`, `w_12_1`), each with the same 11 fixed symbols (`а ё м р т я А Ж Д 1 7`) and three words (`да ещё мягких`). Selection was fixed before inference. No samples are used to train or tune this audit. Original model training overlap is unknown, so this is not a guaranteed writer-held-out benchmark.

```powershell
python tools/handwriting/fetch_russian_track_corpus.py D:/BlurMask-build-artifacts-20261011/russian-real-corpus --self-test

dotnet run --project tools/LegacyRussianCorpus.Probe -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/russian-corpus-managed -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json D:/BlurMask-build-artifacts-20261011/russian-real-corpus/manifest.json D:/BlurMask-build-artifacts-20261011/russian-real-corpus/report-managed-release.json 200 50
```

The probe flips Y into the library's downward coordinate frame, preserving order. Expected labels are read only after inference. Character requests evaluate the complete track with four resource-backed neural families and the final 195-entry acoustic cache. Word requests use restored cuts, group geometry, the final cache, dictionary and lexical scoring, with an original bounded variable beam. Flags, category mapping and line frame are explicit. Per-sample cancellation is 15 seconds; beam width is 64, at most 512 scored groups and 2,000,000 expanded choices. Costs are native-style integer costs, **not calibrated confidence**. Expected-rank ranges preserve tied costs; top-five order uses ordinal text as a deterministic tie-breaker.

## Observed result, 2026-10-11

Managed Release and Windows NativeAOT agree on all 42 top-five candidates/costs, expected-rank ranges, statuses, classified-group counts and expanded-choice counts. All samples completed without errors or budget cancellation.

| Diagnostic comparison | Exact first candidate |
| --- | ---: |
| Whole-track symbols | 7 / 33 |
| Whole-word dictionary candidates | 2 / 9 |

`да` was first for writers 7 and 12, and second for writer 3. `ещё` was absent from the returned beam for two writers and ranked 28 for writer 7. `мягких` was absent from every returned beam. Inference reports preserve these failures; labels do not rewrite candidates.

Concrete failures include Latin `a` above Cyrillic `а`, Latin `p` above Cyrillic `р`, lower-case `ж` above the expected upper-case `Ж`, and `•` above several multi-part letters. Missing pen-up data and uncertain baseline can contribute, but this audit does not identify them as the sole cause. It does not justify a label-based alias correction or a production confidence threshold.

NativeAOT model initialization was about 97 ms; median per-track inference was about 0.65 ms for symbols and 26.7 ms for words (largest word about 67.4 ms). These single-run figures describe this bounded probe, not interactive UI latency or a throughput benchmark. Managed tiered-JIT measurements vary during warmup.

The next meaningful quality test needs true pen-down/pen-up trajectories, documented writing-band metadata, and a fixed author split. Ordinary source-scope/case/category routing and the native grouping controller still need independent verification before this pipeline can claim robust Russian cursive recognition.
