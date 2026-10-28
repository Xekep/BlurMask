# Actual multiatom descriptor local graphs and append state

`LegacyFreshMainDescriptorGraphAppend.Prepare(AK, actualDescriptorCutSource,
separationTable)` consumes the actual sequential descriptor cuts. It replaces
the single-atom restriction of the separate older append adapter. It retains
that cut-source owner, every ordinary descriptor, its line, global physical
ranges, pending kind3 contexts and its local pruning provenance.

This is an append-stage endpoint. It does **not** expose a finished global
classifier graph. Cross-source `1A3B30`/`19E8B0`, global boundary pruning and
multiline NN/context/word recognition have not run. No independent per-line
sampler is substituted for the shared changing source domain.

The allocation sidecar is attached to the existing ordinary graph producer.
Its public arithmetic projection is unchanged. Initial groups retain
`SourceAtomIndex` through seed reordering. Consequently the pending context
belongs to original atom0 even if its ordered position changes; no auxiliary
zero is guessed. Source flags70/80 and boundaryF0 come from the fresh ordinary
source contract already checked by the owning descriptor producer and earlier
source-metadata ports.

Native lifecycle:

* `19F850`: ordered initial groups contain descriptor ordinal, secondaryFFFF,
  pending `19EB10` context or FFFF, one owned history entry and actual bbox.
* `1A1850`/`1A1EF0`: record seed allocation **order**, placement alternatives,
  context IDs and auxiliary inheritance. Every allocated arc increments the
  group's byte only below200.
* `1A35A0`: concatenate the left/right histories. `1A3450` sorts their first
  range's signed physical/start pair using middle-pivot quicksort, including
  swaps for equal keys. Then `193690/193720` compose the actual ranges.
* `1A3B30`/`1A37B0`: only newly allocated merged groups/arcs emit sidecar
  events. A duplicate arc may clear flag1 without allocating or incrementing.
  New ordinary groups inherit the left descriptor/auxiliary; histories remain
  distinct from flattened ranges. New merged arcs inherit left auxiliary,
  start and right end, with zeroed placement/extra/scalar fields.
* `1A5B00`: mode0 visits nodes ascending and ordinals descending, excluding the
  final node. Replay that exact deletion stream; decrement only below200.
  Counter200 remains sticky even when all its arcs are later removed.
* `19D470`: append the resulting richer state through the existing checked
  remap/root-stitch implementation, preserving allocation IDs before its
  ordered live-group remap.

Selected cut records use global physical IDs and their original map entries.
The local arithmetic view rebases only physical indices and reconstructs
forward geometry from the selected coordinates, checking delta/length bits
against every actual selected record. Its newly numbered point IDs are never
published as chronology. The source's composed chronology and shared XY stay
with `Source`. Global ranges in `LocalState` are rebased back to their actual
physical interval before append.

Portable budgets remain explicit:4096 nodes/points,256 physical strokes,
131072 groups/arcs and1048576 range/history/connection entries. Cancellation
is checked during source traversal, allocation, history sorting, pruning
replay and snapshot construction. No native pointer/padding bytes are invented.

Validation: `LegacyMainLocalMetadata.Probe` checks96 prepared allocation/prune
cases, five actual raw owning inputs (including two physical-line descriptors),
old projection parity and whole-single-atom append parity. Independent Python
recomputes actual bbox/range histories, allocation inheritance, binary32 raw
separation, mutable deletion order, counters and append remapping. Twelve
**prepared**199/200/201 allocation/deletion streams exercise sticky saturation;
they are not actual final-node pruning observations. Sixty-four equal-key sort
controls distinguish native history order from stable sorting. Raw input tests
carry no expected text and establish ownership/arithmetic, not OCR quality.

Run managed or Native AOT probe with eight arguments: AK, engine profile,
separation profile, case data, publication data, line30, line50, capture JSON.
Audit with `python tools/LegacyMainLocalMetadata.Probe/audit_output.py capture`.
Static-only audit:
`python tools/handwriting/audit_main_local_graph_metadata.py --output report`.
Existing candidate graph, seed graph and sampler-pruning gates remain applicable.
