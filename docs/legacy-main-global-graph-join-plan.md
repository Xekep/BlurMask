# Global main graph join: ownership and next kernel

This is a read-only implementation plan following the
[owned descriptor cut source](legacy-main-descriptor-cut-source.md). No global
graph API is implemented or enabled by this document. The existing one-descriptor
sampler, classifier and transition guards remain necessary.

The source is the pinned AK with SHA-256
`7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978` and static
Text PE with SHA-256
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
Addresses below use the180000000 image base. No vendor code was loaded or run.

## What an existing graph projection does not retain

`LegacyOrdinaryCursiveCandidateGraph` contains ordinary group ranges, seed
indices, arc span/flags, endpoint context IDs and adjacency. It deliberately
does not own the native96-byte group header or32-byte auxiliary source contexts.
The global join cannot infer the missing fields from physical range order.

The minimum richer state needs these separately owned identities:

| Native item | Required retained data |
| --- | --- |
| Ordinary group | Primary descriptor U16+0, secondary descriptor U16+2, auxiliary context U16+4, reference counter byte+6, kind byte+7, physical ranges and exact bbox. |
| Group flags | Native bytes50/51/52, distinct from the live reference counter and from arc flags. |
| Arc | Group ID, span+4, placement+5, flags+6, ordered predecessor and per-selector successor vectors, endpoint IDs10/14, auxiliary index18, scalar/extra fields1A/1C. |
| Root | The actual root record with its ordered successor vectors; −1 is the predecessor sentinel, not a global group/context ID. |
| Auxiliary context | Allocated32-byte header, opaque control vector, actual ordinal and descriptor/pending-event provenance. |
| Source | Same shared final XY/count domain, composed chronology and original parent source as the cut owner. |

Current one-descriptor generation and pruning can contribute topology, but a
public graph alone does not prove that complete native state. In particular,
placement and auxiliary metadata cannot be recreated as all-zero side arrays.

## Exact local-to-global append

`1A61C0` performs19F850, seed expansion/overlap processing,1A5700,1A5920 and
local1A5B00 pruning before19E8B0 combines descriptor graphs. Empty kind3 descriptor
graphs retain pending events for the next ordinary descriptor; they add no NN
group or extra graph node in the scoped config-byte3-clear path.

`19D470` takes a local graph and appends it to the shared graph. It snapshots old
global group/context counts and the old node-count endpoint base. The native
mapping is:

```text
live local group g -> oldGlobalGroupCount + liveOrdinal(g)
endpoint context id -> id + ((oldGlobalNodeCount & 0xFFFFFF) << 7)
auxiliary context id -> FFFF unchanged, otherwise id + oldGlobalContextCount
source descriptor id -> unchanged
physical stroke/local point range -> unchanged (already in the shared domain)
```

Live groups are selected in local allocation order by byte+6 nonzero. The PE
instructions19D510..19D534 test that byte and assign the remap.19D498/19D4A2
capture the old context count and endpoint base;19D5FC..19D60B preservesFFFF or
adds the context offset to the copied group header. Ordinary ranges and auxiliary
control vectors are deep-copied. Local descriptor ordinals must remain the actual
source ordinals, including gaps occupied by kind3 descriptors.

`19C150` deep-copies ordered connection vectors.19C212/19C219 copies placement
and the supplied mapped group ID;19C21B..19C235 preserves or offsets the auxiliary
index;19C239..19C248 offsets both endpoint IDs. Span, flags and scalar fields are
copied separately. Replacing this with a new set-based adjacency list would lose
native list order and duplicate behavior.

For each new source-root-linked arc within the lookahead window,19D470 removes
the sole−1 predecessor with19D270, appends the old shared last node's arc ordinals
in ascending order, and adds reciprocal successor references to those old arcs.
This stitches the two descriptor paths. It is independent of whether a new
cross-descriptor glyph group is later admitted. Root vectors, per-selector lists
and terminal-node counts are therefore necessary inputs to the append kernel.

### Saturated live-reference counters

Reference byte200 is a sticky native value.1A38F1..1A38F8 increments a group's
counter only below200.1A6109..1A610F decrements it on arc deletion only below200.
A group that reached200 can remain live for append even if later pruning removed
all of its arcs. Recounting references from the retained graph would wrongly
drop such a group and shift subsequent global IDs.

For the existing ordinary pre-pruning generation, an adapter can derive
`min(200, allocatedArcOccurrences)` before pruning, then replay the actual
removals with the sticky200 rule. It must retain the pre-pruning graph/removal
identity rather than counting only the final projected graph. Prepared controls
must cover199,200 and201 allocations and all-removed states.

## Cross-descriptor construction has a precise narrow schedule

At19E94F,19E8B0 checks graph1DB. With the fresh value clear, its cross-input call
uses:

```text
leftNode = old global final node
requiredLeftSpan = previous nonempty local node count - 1
rightNode = new global final node
merge hint pointer = null
crossSource = true
```

19E96B..19E974 admits the call only when
`previousLocalNodes + currentLocalNodes - 1 < spanCap`.19E976..19E99C supplies
the arguments above.1A3B30 additionally selects the matching left span and the
right candidates reachable through that exact selector. This is not a general
request to merge every trailing glyph with every leading glyph.

Actual main189379 has cap8 and runtime158 enabled, while first12727 has cap7 and
runtime158 disabled. Runtime158=0 excludes different descriptor IDs; the actual
main branch instead applies descriptor metadata/line and bbox/guide tests.
Cross-input group creation copies the left descriptor into group+0 and the
right descriptor into group+2. Its context+4 is inherited from the left group;
group51 combines the source flags, and the cross-source endpoint field52 takes
its native branch. The group bbox is the true union. Geometry must not be
replaced by the bbox of one selected physical stroke.

1A35A0 asks the sampler to compose the actual ranges, then1A3B30 searches existing
ordinary groups in native allocation order and checks predecessor/successor
compatibility before reusing an arc. Newly needed arcs are created by1A37B0,
which retains left start/right end contexts, computes the span, updates reference
counters and reciprocal vectors, and in the cross branch copies more than only
the immediate successor selector. A same-source-only Generate projection cannot
stand in for this stage.

After append/cross construction,19E8B0 revisits the actual boundary windows with
1A5B00 mode1 and local final nodes with mode0. Mode1 examines the primary and
secondary descriptor ranges before the sampler rejection callback; it is not
identical to the existing local-pruning traversal. Its full range-membership
shortcuts must be retained before exposing a finished global graph.

## Actual captured inputs that exercise the dependency

The frozen cut-source capture includes a real two-line raw input with ordinary
descriptors0 and2, separator1, line IDs0/1, physical intervals0..2 and3..5, and
10 atoms per ordinary descriptor. The pending separator reaches descriptor2.
The cross-call cap rejects10+10 before geometry. That example proves source
ownership and shared replacement, but cannot cover cross-input construction.

A separate read-only managed capture ran16 real raw preparations through the
same actual first TEXT/720/24B/24D/29B/265 and cut-source owners. The inputs use
two single-physical letter strokes at vertical offsets100,140,180 and250, with
four explicit geometric shapes. All16 preparations completed. Fifteen produced
two ordinary descriptors with at most eight atoms in total:

- M-shaped strokes:4+4 atoms;
- vertical strokes at offsets140/180/250:1+1 atoms;
- P-shaped and diagonal shapes:3+3 atoms.

The remaining vertical pair at offset100 belongs to one ordinary descriptor.
These captures establish real cap-eligible inputs for the next implementation.
They do not establish that the subsequent native geometry tests accept a merge,
nor that a decoded character is correct. No expected text selected a descriptor,
cut, label or topology. The private capture is
`global-join-actual-small-descriptors.jsonl` under the E build-artifact root.

## Smallest coherent next implementation

The first internal numerical kernel should restore19D470/19C150/19EB10 append,
ordered root stitching and sticky live-group remapping over a richer owned local
graph state. It should return an append-stage state and complete provenance
maps, not label itself a completed main graph. No physical pointers or allocator
residue need to be fabricated; pointer-backed lists become owned ordered vectors.

The actual adapter should accept only `LegacyFreshMainDescriptorCutSource` and
the same separation table/resource. For each actual ordinary step it must:

1. retain its descriptor/line/global physical identities;
2. create a selected geometry view only for existing seed/overlap arithmetic,
   keeping any local rebasing explicit and never treating its identity point map
   as original source chronology;
3. restore the ordinary group header and pending19EB10 context producer during
   group allocation, before reordering/merging, rather than infer metadata from
   sorted ranges afterward;
4. keep the pre-pruning allocation identity and sticky reference counters;
5. append with the true native remap and ordered connections.

The second kernel should then add the cross-source1A3B30/1A37B0 branch and exact
19E8B0 boundary-pruning schedule to that same state. Only its completed result
should expose a classifier-facing global projection plus immutable group/arc
source metadata. It must retain the cut source itself for shared XY/chronology
and original metadata ownership.

Independent controls should compare live/dead/saturated remaps, nonzero endpoint
and context bases,FFFF sentinels, ordered root cross-products, duplicate list
entries, pending context ordinals, actual source-pair headers, admitted/rejected
geometry, interning and boundary deletion. Actual one-descriptor output must
remain byte-equivalent to the old path. Actual small two-descriptor captures
above then exercise the owning adapter, without asserting expected words.

The final downstream extension is separate: group-owned descriptor/guide identity
must reach source-range DFS,227 context selection, normalization, dynamic weights
and transition callbacks. Existing consumers using one global `SourceHeight`,
`SourceBaseline` and descriptor0 cannot be widened just because a graph joined.
