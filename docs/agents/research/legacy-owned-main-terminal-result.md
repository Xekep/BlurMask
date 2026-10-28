# Owned DIRECT pass to terminal selection

`LegacyOwnedMainDirectWordPass.RunOwned()` connects the existing owning
`2144F0` DIRECT loop to the prepared `2271F0` invocation and `2257C0` scan.
`Run()` and `SelectPreparedRows()` retain their existing signatures and behavior.
Both run methods use the same single-use guard, including failed or canceled
entered runs; cancellation before entry does not consume the pass.

`LegacyOwnedMainDirectWordPassResult` has a private constructor and is created
only after the pass completed every owned source node. It deep-copies the final
88-byte row storage, primary alternatives, source/retained extents and cumulative
`2520` values after periodic normalization. It retains the actual controller
identity. Every public storage getter returns a new copy. In particular,
`ReadOnlyMemory<byte>` alone is not used as an immutability boundary: its array
can be recovered through `MemoryMarshal.TryGetArray`.

`LegacyOwnedFreshMainTerminalInvocation.SelectOwnedPass(result)` rejects a
result from another controller, even when both controllers share a span owner
or the same loaded model. It additionally checks the direct dictionary owner.
It consumes the last actual node's complete `SourceEnd23F0` extent, rather than
its retained count, and its owned cumulative cost. The returned
`LegacyOwnedMainTerminalResult` contains the numeric winner and a copied full
88-byte chosen record (empty if no winner). It retains the producing pass
result. There is no caller override for row bytes, extent, cumulative cost or
terminal invocation options in this path.

The invocation remains the proven fresh DIRECT/no-property route: mode0,
secondary alternative count1, EC0, inactive rescue, Filter4=false, and the
resource-bound sentinel policy. This does not replace the native case292
factory or implement the optional source-rescue/multiple-alternative scans.
The caller-provided allocation residue contract of the owning pass is unchanged;
this bridge does not claim native `1FA300` physical row-pointer allocation.

## Resource evidence

`tools/LegacyMainOwnedTerminal.Probe` obtains its rows from actual RU acoustic
networks, prepared typed main source ownership, graph, dictionary queries and
the complete owning pass. No expected text or chosen row is supplied. A separate
scalar terminal scan checks the actual fresh invocation (uninspected secondary
rows with strict lowest-cost/first-tie selection); it does not call the selector.
The chosen complete88 bytes and numeric fields are checked as well.

* Single source: 4 nodes; SourceEnd143 and RetainedEnd141 remain distinct;
  chosen ordinal138, relative cost20327, cumulative0, total20327.
* Actual14-physical source: 420 NN groups and56 nodes; chosen ordinal155,
  relative77859 plus owned periodic cumulative346098 equals total423957.

The probe also mutates all public snapshot backing arrays, selected-row copies
and cumulative copies; the private stored data and repeated terminal result stay
unchanged. Foreign controller/source, null, cancellation, invalid node indices,
Run/RunOwned cross-entry and pre-entry cancellation lifecycle guards are tested.
Existing procedural scheduler/storage and prepared terminal tests remain the
separate regression coverage for their numerical policies.

These are numeric beam results, not reconstructed recognized text or calibrated
acceptance confidence. Native `207CB0` symbolic sequence/coordinates and compact
source lineage through `206D30`/`21FFD0` remain the next bridge. The complete
case-aware application recognition path is unfinished.

Native `2271F0` calls `207CB0` before selecting its terminal row. The current
bridge deliberately consumes the owning pass's **pre-207CB0** storage. In
particular, native materialization rewrites beam field22 with a compact-relative
index; `Winner.RowField22` and `CopySelectedRecord()` currently retain the raw
pre-publication value instead. The full88-byte checks compare those pass bytes,
not a completed native compact result. The numerical scan is independently
checked for its supplied rows and owned options; equivalence to the complete
result route still requires the missing materialization stage and its effects
on every field used by selection to be checked.

Individual prepared pieces now include the [ordinary frame producer](legacy-prepared-compact-word-frame.md),
[compact record writer](legacy-compact-word-records.md) and
[predecessor/alternative cursor](legacy-compact-word-predecessor.md).
They consume explicit prepared buffers and do not construct the complete native
materialization schedule, rewrite this pass's beam22 indices or publish its final
glyph/text result. `SelectOwnedPass` still has the pre-materialization contract
above; the availability of these helpers does not change that contract.

## Reproduce

Run the probe with `AK LK engine separation`, optionally followed by
`line30 line50 case-data publication-data` to include the actual56-node source.
Use the repository RU resources, line-geometry networks and text-unicode data.
Managed Release and Windows NativeAOT use the same arguments and test logic.
All build artifacts and TEMP/TMP belong outside the source tree.
