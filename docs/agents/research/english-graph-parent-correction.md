# Ordinary English graph correction

The internal generic English graph pass had three separate obstacles beyond
the original two-node fixture:

- Preparation always reserved three D0 operations, enough for node0 and the two
  node1 spans. Graphs with more nodes exhausted this bookkeeping limit early.
- Ordinary parent processing received a constant previous node0, even though
  the caller had selected a different actual parent node from the retained ring.
- The outer row gate rejected every nonzero flags byte, including kind2 already
  implemented by the inner ordinary parent producer.

Preparation now derives one node0 call plus every actual span0..maximum at each
later node, counting every selected arc exactly once. Existing logical byte
caps and additive preflights are unchanged. The parent call receives the same
computed predecessor used to select its row. The outer gate admits kind0/2 while
retaining sourceID1, ordinary symbol checks and every inner producer guard.
Graphs beyond49 nodes still require the separate periodic continuation.

## Evidence on 2026-10-25

Static PE-byte checks confirm31 native caller/callee instructions, including
Windows x64 argument6 and the previous-node/request08 relationship. Prepared
controls cover363 node/span schedules. Separate historical geometry controls
cover106 graphs; the original three-node case needs six D0 preparations and six
selected rows, including the initial node0 row. Exhaustive byte controls cover
the outer predicate. The observed kind2 label28 is independently decoded from
the authenticated raw alphabet as ordinary nonvirtual S.

The original raw141 fixture, models and old graph2 reference were not changed.
Each actual used a separately frozen build/package under .NET10.0.12, two logical
processors and affinity3, with a45-second outer watchdog and no automatic retry:

1. Stock code stopped at the original D0 preparation budget.
2. The D0-only correction reached and reproduced the previous-node adjacency
   error on the same original-index6 input.
3. D0 plus predecessor reached the outer kind gate. A separate failure-only
   diagnostic retained the guard and identified node2/span0/previous1/parent72,
   source1, label28, length0, flags2; it did not repeat a query.
4. All three corrections completed the actual three-node pass in about0.995s:
   1604 query rows,342 query pages,1540 scoring calls,1206 accepted allocations
   and five noninitial selected D0 rows.
5. The original-index4 graph2 capture remained byte-exact, including every row,
   primary alternative, heap byte, knownness mask and ordinal. Its unchanged
   auditor passes against the independently reconstructed historical reference.

All929 library source documents and both one-source probes were checked against
their Portable PDB checksums and PE CodeView records. The new portable regression
runs graph2 compatibility and graph3 completion on Windows/Linux/macOS in the
source workflow. Its graph3 validator rejects ten incomplete/corrupt captures.

These checks establish this internal FIRST-language/phase1 graph continuation.
They do not independently reconstruct every new graph3 score byte, demonstrate
handwriting accuracy, publish a final word, establish long/periodic graph support
or prove NativeAOT parity. Russian cursive and full engine integration remain
unfinished.
