# Primary recombination in the owning main pass

The shared `LegacyOwnedMainDirectWordPass.OrderPrimary` now calls
`LegacyNativePrimaryWordRecombination.Apply` once over the complete active heap
and full alternative pools. This replaces the separate root/ordinary-parent
loop at the native `2144F0 ->20E6A0` callsite. DIRECT, CASE, configured secondary2
and the actual global source continue to share the same scheduler implementation.

The order is unchanged: allocate actual initial ordinals58, recombine primary
rows, sort/deduplicate alternatives, apply quota, then materialize stored88 source
rows. The dispatcher receives the recombined full128 heap as the sort input. Its
lookup uses each computed owning node's `CopyRows()` default source extent23F0,
including source rows outside the separately retained23F8 prefix. No current D0
span, truncated retained array or artificial parent identity replaces that owner.

Prepared dependencies come from this same source/controller:

- Actual acoustic alphabet, sampler categories, context factory and source headers.
- Runtime AL10/12/14 from serialized header words4/5/6 for24E4/24E6/24E8.
  U+0003 and U+FFFD are looked up in that actual alphabet for24EA/24F4.
- Subword source22EC from the existing prepared source table, rather than a new
  guessed source index; maximum parent distance54 from the owning scalar policy.
- Fresh identity2360 and zero count-cap2601/2602/permission2309, retained by the
  existing sealed no-associated policy. Other knowledge/plugin/reuse mutations
  are not accepted by this owning constructor. The named secondary-count2 property
  does not modify these fields.
- The constructor's actual same-source geometry instance, or the exact existing
  `GlobalTransitions` instance with its mutable bounds. Operations are created
  lazily once for a genuine general-higher row; a second transition owner is not
  synthesized.

Only the general higher branch consumes a local80 request residue. A new appended
storage-kind enum value requests those bytes from the existing allocator if that
branch is present. The default root/primary paths make no additional allocator
calls and retain their original allocator ordering and byte results. Unused
portable80 shape storage is not asserted to be the native uninitialized stack.
The dispatcher's existing prepared controls cover scratch carry between multiple
general rows. The scheduler does not generate fake higher predecessors to force
that coverage.

## Verification

The unchanged DIRECT procedural oracle compares4 and56 actual nodes, all
1,423,680 bytes of row/alternative storage and39,360 symbolic context fields.
The56-node fixture uses420 real neural groups and preserves source/retained counts,
the node49 cost offset346098 and cumulative2520 values.

The unchanged CASE4/60-node capture remains byte-identical to the earlier result:
SHA-256 `baed0e90592e79dfae314e314bdc0f6309abd62d492ffb3951efca8b5ec8cfa0`.
Its independent raw AK/LK audit checks457 language rows from160 terminal nodes,
full case tags/restart values and separate source/retained extents.

The unchanged global procedural oracle covers8 actual source owners,110 nodes,
631 neural groups,2,610,080 storage bytes and72,160 symbolic fields. Capture
SHA-256 remains
`d0a1ddd599e5c63e779e122a852750222b648c3bcc21890e064da0e90fc823da`.
The independent raw-language audit checks961 language rows from288 terminal nodes
and1180 descriptor-line decisions. These are the current root-anchor inference
paths; they do not demonstrate an inferred secondary predecessor or later-history
language route.

The existing configured secondary-storage probe checks actual source2410 records
and finite alternative identity transport:431 finite rows and49,920 complete2410
bytes, plus64 explicit periodic controls. Its capture remains
`55f61137842beba4e15f24d3136b7368dd815143bd61c6ba47eedae7612abc42`.
Its default count1 refusals remain unchanged. All four regression projects pass
Managed and Windows NativeAOT; CASE/global/configured captures match their old
goldens and each other. The separate dispatcher probe still supplies explicit prepared higher
rows, scripted full80 geometry and an independent whole-byte oracle. Those
controls are not described as an observed higher-arc handwriting result.

Run `tools/handwriting/audit_main_primary_recombination_integration.py report.json`
for the static native order, fresh source-policy and alphabet-header bindings.
It checks24 Text and8 Engine pins plus12 managed ownership pins, and invokes the
existing full20E6A0, fresh no-associated source and no-property C4 audits.
The native call is `215279`; initial ordinal58 assignment is `215264`, before the
call and before the following count>1 sorting guard. No vendor code is executed.

The runtime regression projects are `LegacyMainDirectWordPass.Probe`,
`LegacyCaseMainPass.Probe`, `LegacyMainGlobalWordPass.Probe` and
`LegacyMainSecondaryStorage.Probe`; their existing argument orders and independent
audit scripts are unchanged. Use the complete line resources
`models/line-geometry/line-2e3-30.res` and `line-2e3-50.res` for56/60-node controls.
All build, output and TEMP/TMP paths should be outside the source tree.

This integration closes the missing secondary-parent dispatch opportunity at
the actual primary phase location. It does not close the upstream producer of a
genuine later-word anchor, schedule203690/204290 history loops, broaden the narrow
same-source geometry contract, or claim a full decoder or acceptance policy.
