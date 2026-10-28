# Recorded coordinate ingress diagnostic

`tools/LegacyCaseCorpusIngress.Probe` is an opt-in headless diagnostic for the
pinned local coordinate corpus described in `russian-track-corpus-audit.md`.
It reads only direct sibling tracks with matching SHA256 and bounded points.
Expected labels never enter recognition. No corpus is downloaded by this probe
or required by CI.

The route uses physical grouping, descriptor splitting/reordering, actual
first-pass acoustic masks and first results, shear/gap preparation, the
single-assigned-line sampler and acoustic context, then the actual case-aware
main word pass. It reports stage, graph/group/allocation counts and failures.
It does not emit a chosen text string or calculate recognition accuracy.

All nine recorded word tracks reached logical main rows in the initial check:
7–22 graph nodes,28–148 groups and27–55 language allocations. The corpus omits
pen-up information. Each complete track is therefore supplied as one explicit
unsegmented input path; this is a diagnostic assumption, not reconstructed
physical stroke segmentation or a cursive accuracy result.

Arguments: `AK LK engine separation case-data publication-data line30 line50
manifest output word-index`, with word-index0 through8. External data remains
read-only; reports belong in a separate build-artifact directory.
