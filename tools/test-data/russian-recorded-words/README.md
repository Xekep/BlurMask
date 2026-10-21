# Recorded Russian word fixtures

Nine existing word tracks from **Russian Handwritings Tracked**, by Dmitry
Iatsenko and Konstantin Smirnov, version2 (2023),
[DOI10.17632/3h6h5d7xg2.2](https://data.mendeley.com/datasets/3h6h5d7xg2/2).
The dataset is licensed under [CC BY4.0](https://creativecommons.org/licenses/by/4.0/).
This notice applies to these data files; the application code has its own license.

Source: [Skvayzer/russian_handwritings_tracked](https://github.com/Skvayzer/russian_handwritings_tracked),
commit `c6629026b1fdef51fd08e3d2fddffd82f97e60a5`. Each manifest entry records
the original coordinate/timing member hashes and the converted track hash.
`tools/handwriting/fetch_russian_track_corpus.py` converts original numeric
CSV coordinates and intervals to JSON. This directory copies its nine word
outputs unchanged; its character outputs are omitted. The runtime loader
changes the Y-axis direction, and does not supply the dataset's guide lines.
This direction follows the recorder's upward Y axis. Original physical pen-up
boundaries are unknown because its release handler records none; see
[recorder provenance](../../../docs/russian-recording-coordinate-provenance.md).

The three writers and words were selected before model evaluation. Overlap
with the vendor model's training writers is unknown. These examples are
regression inputs, not a representative accuracy benchmark or training data.
Pen-up events are absent: the loader preserves each track as one continuous
coordinate sequence and invents no stroke boundaries. Expected labels stay in
the manifest for separate post-inference evaluation; the recognition loader
reads only coordinates, hashes and sample identifiers.

Run the global choices probe with this `manifest.json` as its optional third
argument, then run its independent capture auditor. All nine examples are
expected to complete the bounded pipeline; correct-word accuracy is reported
separately. The current result remains4/9 exact words and16/33 character edits.
