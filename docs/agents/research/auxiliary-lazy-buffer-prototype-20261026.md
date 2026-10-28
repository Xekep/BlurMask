# Auxiliary output allocation prototype

ROOT compiled a private renamed copy of the original auxiliary reader with two allocation-only edits: initialize its output to an empty array, then allocate before the first actual output write. The production library and repository sources are unchanged by this prototype. The old reader comes from the already authenticated f7 library; both synthetic paths share the same public dependency types.

The prototype passes 296 comparisons across empty and positive pages, multiple capacities, pagination, parameter changes, cached reads, resets, pre-entry cancellation, backend exceptions, malformed pages and failed-reader reuse. Complete returned bytes, accepted counts, backend call counts and request transcripts agree for these fixtures. Compilation/PDB authentication covers both authored files. No model or actual handwriting input is executed.

This is deliberately limited evidence. SOURCE review found a missing case: a backend closure can mutate the outer request capacity. The original eager allocation snapshots the size before that callback, while the naive prototype reads it at the later allocation. The production proposal must preserve the old snapshot point and test this counterexample. The passing stable-request fixtures do not establish full generic equivalence.

The next isolated proposal restores the three earlier scoped auxiliary files through exact inverses, keeps the separate outer-wide experiment unchanged, and implements generic lazy allocation with a preserved size snapshot. Original public signatures, cache/protocol behavior, cancellation and poisoning must remain covered. Any additional closure/stack metadata must be accounted. No model, threshold or work limit changes are authorized by this test.

Evidence: `E:/BlurMask-build-artifacts-20261014/root-lazy-aux-buffer-prototype-20261026/report.json`. SOURCE plan: `english950-generic-lazy-aux-buffer-plan-source-20261026/manifest.json`, SHA `5f8c41ce…`. Neither artifact proves recognition readiness.
