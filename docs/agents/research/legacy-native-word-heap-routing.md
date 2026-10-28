# Native word heap routing and promotion

`LegacyNativeWordHeapRouter` connects the checked `1f8c90` bucket decision and
`1f8b70` root promotion to `LegacyNativeWordRecordHeap` (`1f75b0`), the initial
record writer (`1f8d60`) and its nontracing anchor/history tail. It does not select
the native word winner, produce a confidence, or choose language descriptors.

## Producer and consumer proof

`MyScriptText.dll` at `1801f8c90` obtains the base bucket from the byte at `+0xc`
of the descriptor addressed by `runtime+22e0[sourceStateIndex]`. The explicit
`sourceStateBuckets` input is this byte vector, not an acoustic category mapping.
The supplied heap array length is the native descriptor count at `+2450`.

For secondary flag zero, the base bucket is used. If the override flag is true,
`+2458 != 0xffffffff`, and the state matches `+22f0`, the low byte of `+2458`
replaces that bucket. For a nonzero secondary byte, the bucket is
`(descriptorCount >> 1) + baseBucket`; this addition is not truncated to a byte.
Overrides apply only to the zero-secondary branch.

A full, nonzero-capacity secondary heap compares its signed root cost against
the incoming cost. Equal or worse incoming cost is allocated in the primary
heap without promotion. Better incoming cost is allocated in the secondary
heap with its old root promoted into the primary heap before any secondary
slot movement. A secondary heap of capacity zero remains the selected heap and
rejects; there is no fallback merely because its capacity is zero.

The C decompilation omits the fifth argument of the `1f75b0` tail call. The x64
instructions at `1801f8d36..1801f8d52` prove the callback and target: `cmovl r9`
selects `1f8b70`, `cmovl rcx, rax` selects the primary descriptor, and `+30` on
the stack carries that primary descriptor to the allocator. `1f8b70` then calls
`1f75b0` with the evicted record's cost and no eviction callback and copies all
sixteen qwords into an accepted slot. Promotion may itself be rejected by the
primary heap. It does not recursively promote its own evicted root.

`TryInsertInitialRecord` connects the initial writer to the router and then the
checked history tail. The native `1f8d60` scale-zero test forces the requested
secondary byte to zero before both routing and record serialization. When a
nonzero secondary request falls back to the primary heap, the stored secondary
bit still records that request. The selected heap does not overwrite it.

## Ownership and explicit inputs

Heaps own separate slot buffers; aliasing descriptors is rejected. The router
owns a copy of the descriptor bucket vector. Invalid used bucket/override
indices fail before allocation. Eviction uses an owned full-record snapshot;
promotion retains all 128 bytes, including the trace pointer bytes `78..7f`
without dereferencing them. No native pointer is manufactured or executed.
Existing allocator callback/initializer failures can leave partial state;
transactional rollback is not claimed.

Descriptor construction, live state selection, bucket capacities, override
activation, nonzero knowledge scale and any prior history lookups remain
explicit. Fresh knowledge construction initializes the live property `+ec` to
zero (`00c860`); `00e680` can update it. It is not inferred from an AK resource.
The actual-resource test deliberately supplies scale one and capacities 4/2 to
exercise promotion, and separately verifies the zero-scale behavior. These are
diagnostic settings, not a recovered native controller configuration.

## Independent checks

`tools/LegacyWordRouter.Probe/reference_controls.py` uses a functional tree-path
permutation oracle and complete record snapshots. Managed and Windows Native
AOT passed 256 streams / 16,384 routes, including 818 promotion attempts and
378 accepted promotions. Controls cover equal and negative costs, right-child
heap ties, odd descriptor counts, high override bits, zero capacities, full
record movement, rejection, cancellation and invalid metadata.

Actual RU resources run raw MIR strokes through ingress, cuts, seed/candidate
graph, prepared geometry/context, neural families, final acoustic cache and
native acoustic candidate scoring. Ten accepted candidates produce ten routed
allocations, two successful promotions and six primary routes. The six retained
records have the best costs across the explicit two capacities and preserve
real category/source references and nontracing history. This proves the
resource-to-record lifecycle bridge; it is not native word decoding or an
accuracy/calibration measurement.

```powershell
python tools/LegacyWordRouter.Probe/reference_controls.py .artifacts/legacy-word-router-controls.json
dotnet run --project tools/LegacyWordRouter.Probe -c Release -- .artifacts/legacy-word-router-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

Remaining lifecycle work includes continuation records (`1f9080`), row storage
and finalization, native search/winner orchestration and acceptance confidence.
