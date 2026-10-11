# Native191430 final marker limits

`LegacyCursiveMarkerLimit` restores the complete `180191430` stage. It uses original raw physical strokes for X widths and first/last distances, and the corresponding prepared ink for marker tags and segment weights. Synthetic inserted points retain their original ownership. Limits supplied by an upstream category provider remain explicit; no category-dependent limit is guessed.

Only physical strokes beginning with tag1 and having more visible markers than the configured gate are processed. Russian version106 supplies gate6 and width multiplier4. The target truncates `(rawWidth*4)/lineHeight`. An explicitly supplied closed-stroke limit caps this target only when the raw endpoint distance is strictly below `lineHeight*0.8`. Endpoint distance squares and adds in float before a double square root; it differs from the recovered prepared-ink `_hypotf` kernel.

The first marker starts with one segment length. Each subsequent marker is initialized with its segment length and then receives that same length again in the ordinary accumulation loop. The final point of a physical stroke can therefore contribute its forward gap to the next physical stroke. Merge candidates exclude the first marker and tags7/4. Strict comparisons preserve first-encounter ties, and exhausted candidates do not remove protected markers. Removing a marker replaces the previous accumulated weight with the weighted merge cost, not the total arc length.

Assembly `1801917ea` through `18019180f` confirms that both weights are promoted to double, the larger is multiplied by the double constant0.1 at `1802ed1d8`, and their sum is rounded once to float. `CombineWeights` exposes that bounded kernel. Using float0.1 and intermediate float rounding produces different values and can change later selection.

The independent Python control generator supplies1024 own-ink marker cases and1024 merge-cost float-bit controls. Cases exercise explicit closed limits, known/raw geometry, multiple physical strokes, protected tags, gate equality, missing start tag and strict tie order. Managed and Windows NativeAOT headless runs pass all controls;695 ink cases change. Additional controls verify immutable point geometry/provenance, malformed metrics and cancellation. The actual two Russian resource nodes are checked for gate6/multiplier4.

Full191950 runtime branch selection, override1908c0, remaining controller filters and upstream category-limit production are separate work. This final stage does not establish full Russian recognition or end-to-end native parity.
