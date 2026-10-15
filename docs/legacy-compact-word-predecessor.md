# Compact result predecessor resolution

`LegacyPreparedCompactWordPredecessor` restores the initialization and loop-tail
cursor of21FFD0. Inputs are an already materialized compact40 pool2460, node
start table2478, selected source node/relative record and selected alternative.
It does not create these inputs or claim they came from a complete207CB0 pass.

`Begin` adds the node start to the relative record, reads the packed arc from
coordinate8 and substitutes terminal1E only when the supplied alternative is
FFFF. The initial node is the selected node argument; it is not replaced by the
packed coordinate's node.

`Advance` first reads the signed global predecessor at10. A predecessor of-1
produces the end sentinel and preserves the carried alternative. Otherwise:

- If predecessor1E differs fromFFFF, alternative resolution is required.
- If current symbol1C equals24EA and the predecessor's signed24 node is less
  than the current node, alternative resolution is also required. The literal
  symbol comparison still applies when24EA equalsFFFF.
- During resolution, alternatives0..FFFD replace the predecessor with
  `nodeStarts[predecessorNode] + alternative`. FFFE/FFFF retain the direct
  predecessor. Both branches then load the carried alternative from the chosen
  record's1E. An ordinary direct step preserves the old carried value.
- The chosen coordinate8 supplies the next signed24 source node and ushort arc.

Coordinate fields are references to source nodes/arcs, not XY glyph bounds.
Native32-bit index addition and signed comparisons are retained. The prepared
API rejects incomplete pools, out-of-range reads, missing node starts, canceled
calls and attempts to advance the end cursor. It does not mutate either input.
An end cursor can retain a valid compact record when its packed source node is-1;
the remaining native flush stage must handle it separately.

Independent Python controls cover4352 predecessor steps and256 initializations,
including signed24 extrema, FFFD/FFFE/FFFF, marker redirection, direct links and
root sentinels. A three-record sequence is also produced by the existing206D30
writer and consumed through this cursor. Twelve refusal controls exercise invalid
indices, extents, missing tables and cancellation. Managed and Native AOT probes
consume the same controls. The static PE audit pins complete loop-tail bytes and
bootstrap/threshold instructions without executing a vendor DLL.

This is not the full21FFD0 function. Glyph descriptors, protocol handling,
blank merging217DB0, callback score channels, result publication218560 and final
text remain separate work. In particular, walking this cursor alone cannot be
used as a recognized string or an accuracy test for Russian cursive.

Generate: `reference_controls.py controls.json`.
Probe: `LegacyCompactWordPredecessor.Probe controls.json`.
Static audit: `tools/handwriting/audit_compact_word_predecessor.py [report]`.
