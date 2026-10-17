# Owned main descriptor cut source

`LegacyFreshMainDescriptorCutSource.Prepare(resource, actualActiveGapSmoothed,
token)` accepts the real fresh main0 input after first TEXT/source720, assigned
24B guides,24D, active29B and265. It performs the selected-interval193380/191950
cuts in descriptor order over one changing source XY/map domain. It does not
construct the global21B graph or run multiline word recognition. The remaining
graph/context dependencies are listed in the
[multiline roadmap](legacy-main-multiline-sampler-roadmap.md).

The public entry takes no caller guide, probability, cut, marker, category-limit
or configuration boolean. It verifies the actual572763-byte AK fingerprint,
selected main0/source135 lineage, sampler297/v106 at188157, main graph189379,
category owner188222 and ordinary21D189734. The actual graph cap is8.
`Preparation` retains the exact upstream owner.

The public output exposes copied descriptor/guide/point snapshots and immutable
per-descriptor steps. `Chronology` returns a defensive snapshot so recovered
`ReadOnlyMemory` arrays cannot modify the retained source map. Its source factor
and original metadata identities are preserved. Source720 publication ranges
remain in their original parent domain; they are not rewritten to cut points.

## Selected interval and shared replacement

Each ordinary descriptor must have an actual assigned guide I32+10 and inclusive
physical interval I32+24/+28. The bounded fresh input must cover its physical
strokes in descriptor order, without overlap or omissions. Only ordinary kind0
and the proven pending kind3 branch are supported. Unassigned ordinary input,
other source kinds or a custom category limiter are refused.

For an ordinary step, the existing complete Russian106 cut arithmetic consumes
the current selected physical slice, that guide's baseline+0/height+4 and the
corresponding global source218.V60 probabilities. Those probabilities are not
averaged or replaced by a scalar. The optional category-limit owner is absent in
the selected247/v10D resource; normal count/width marker limiting still runs.

The selected preparation has a zero final delta, even when physical strokes
follow outside that descriptor. This matches191950's temporary selected buffer.
Its selected record identities are then rebased explicitly:

- physical index = selected-local physical index + descriptor first physical;
- original point index = selected-local original point index + current global
  prefix point count;
- markers and atoms retain their local point positions and gain the global
  physical index.

The full incoming map contains the unchanged prefix identity, the selected
prepared indices and the unchanged suffix identity. Only selected XY/counts are
replaced. `RemapPreparedPoints` composes that factor1 map through the preceding
source map using the existing16AA00 implementation. The next descriptor therefore
addresses the new global point domain, including earlier insertions.

Steps retain selected geometry records separately from final shared XY. They
do not expose a fabricated whole-source `LegacyCursivePreparedInk` made by
concatenating incompatible per-descriptor deltas. There is no NN group ID or
word prediction in this output.

The portable limits remain256 physical strokes,4096 total points and512 source
descriptors/guides. A shared insertion exceeding4096 is refused before allocating
the full replacement map. All retained input arrays are snapshotted and long
point/cut loops honor cancellation. These bounds do not claim native allocator
equivalence outside that input size.

## Pending kind3 metadata and configuration byte3

For kind3, only descriptor QWORD+20 ==−1 is restored. Native19FA17/19FA21 takes
that path;19FA2C sets pending flag1;19FA3F appends controlFFFFFFF9. The cut source
records each pending descriptor ordinal, carries it to the next ordinary step
and preserves any trailing pending ordinals. It does not create an NN glyph or
pretend that pending metadata has already become a native32-byte graph context.
The later19EB10/global graph owner must perform that assignment.

Graph1D8's configuration byte3 is derived for this fresh owner, not supplied:

| Native stage | Evidence |
| --- | --- |
| NEW2B3 |00C8A8 zeroes RAX;00C8F3 clears C4/C5. The typed input has no property events, reused container or external wrapper. |
| Frontend condition |150E41..150E8E tests C4, selected source V58==137, and alphabet U+F0009 lookup. Main source135 and the actual192-label AL exclude the latter two cases. |
| Configuration |1514DF/1514E7 copies frontend91 into config byte3;151593/1515A7 passes that configuration to graph initialization. |
| Graph |1A6BD2/1A6BD6 copies config byte3 into graph1D8. |

Thus `GraphConfigurationByte3` is false in this exact scope. It does not suppress
the pending kind3 metadata described above and says nothing about external or
reused native containers. It is independent of the existing wrapper flags0/7
and the frontend23F/type check.

Sampler V30=18DD50 clears marker count1E8 and selected-record count1F8 at the end
of each ordinary19F850 loop. The owning result stores an immutable step before
that workspace would be reused; shared XY and source chronology remain current.

## Verification and its limits

`LegacyMainDescriptorCuts.Probe` runs192 prepared descriptor/map states, including
77 inserted points, nonzero prefix offsets, mixed guide/physical partitions,
fractional factor200 maps and leading/interior/trailing kind3 controls. These
are numerical controls, not handwriting accuracy samples.

Seven actual raw inputs run through the existing owned TEXT/720/24B/24D/29B/265
preparation. One has multiple descriptors; six admit the old single-descriptor
sampler. Those six compare every prepared record, marker, atom, shared XY and
chronology map against that unchanged sampler byte for byte. There are27 bounds,
ownership, snapshot and cancellation guards, including an actual insertion at
the shared4096-point limit. No first-NN or expected-text golden
is changed by this batch.

The independent Python capture audit checks3952 float records and point
identities,8834 composed map entries,269 pending separator events,289 nonzero
prefix replacements and88 total insertions including actual input outputs.
It independently computes float-rounded deltas/lengths, marker/atom projection,
raw-point retention, prefix/suffix maps, sequential composition and ceil metadata
resolution. It does not implement a second full loop/horizontal/baseline cut
algorithm; that arithmetic remains the previously ported cut producer.

The static audit pins47 instructions and seven297 vtable bindings, including the
descriptor/guide operands, sourceV30 null-order call, marker reset, kind3 control
and the complete config-byte3 forwarding above. It reads PE/resource bytes only.
Managed and Native AOT captures must match exactly before freezing this batch.

Commands use these eight positional probe arguments:

```text
AK engine-profile separation-profile case-data publication-data line30 line50 capture.json
```

```powershell
python tools/LegacyMainDescriptorCuts.Probe/audit_output.py `
  models/ru-RU/ru_RU-ak-cur.lite.res <capture.json>
python tools/handwriting/audit_main_descriptor_cut_source.py --output <static.json>
```

The next runtime dependency remains global graph append/remap19D470/19C150,
pending19EB10 context ownership, cross-descriptor1A3B30 candidates and boundary
pruning. Per-group descriptor/guide identity must then reach normalization and
227 context extraction. This cut owner alone does not remove the existing
single-descriptor recognition guard.
