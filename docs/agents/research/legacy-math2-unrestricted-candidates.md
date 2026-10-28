# Prepared Math2 unrestricted candidate expansion

The internal `LegacyPreparedMathUnrestrictedCandidates` connects explicit
prepared cumulative-cost rows to the restored0351E0 heap and035F70 raw48-byte
sorter. It models the unrestricted C8-null candidate branch in038610, not the
full decoder or an app recognizer.

For every active beam and every vocabulary class, the driver offers that
cost to the same owned heap. On a returned slot it writes symbol ID at08,
per-beam source information at10, conditional metadata at18, all-ones at20
and beam ordinal at28. The other bytes retain the helper's zero initialization.
Source information and conditional metadata remain explicit prepared inputs:
036250 and the optional tensor producer are not fabricated. EEF's cumulative
cost-buffer producer, configured aliases and masks are not joined here.

The heap has2*requestedBeamWidth slots. The supported prepared input must
provide enough actual offers to initialize every slot; the implementation
refuses an undersized set. The native sorter receives the full capacity,
not the heap's active count minus one. Consequently no untouched allocation
bytes reach the supported output even though the library initializes its
allocation safely. Widths1..16 and complete same-sized per-beam sidecars are
validated before iteration. Cancellation cannot expose a successful partial
result.

035F70 uses midpoint quicksort for all nonempty sizes, including two entries.
It is distinct from the completed168-byte sorter036810, whose small-array
branch is stable. Its strict ordered comparisons stop on equal costs and on
NaNs, and it exchanges all48 bytes. Equal pairs reverse; four equal records
rotate by two. The implementation preserves left-first recursion and the
right-hand tail iteration within the32-slot portable bound.

The math probe retains previous controls and adds264 raw-sort cases and120
prepared joins using297-class cost rows, the fingerprinted vocabulary size.
An independent Python byte oracle reconstructs inputs, per-beam sidecars,
all heap movement, caller fields and sorting. Managed and Native AOT captures
must agree in full. These tests do not execute network models or measure
recognition accuracy. Native EOS completion, delta pruning, continuation
vectors and decoder-state reindexing remain separate work.

Static reference: [native candidate heap](legacy-math2-candidate-heap.md) and
[pruning/ordering handoff](legacy-math2-pruning-handoff.md). The latter checks
035F70 and both native capacity-based caller sort sites.
