# Native word global quotas and owned descriptor storage

`LegacyNativeWordQuotaController` ports the finite-factor, nontracing `212ec0`
decisions over explicit prepared heap descriptors. `LegacyNativeWordHeapDescriptor`
provides the mutable backing/capacity behavior of `1f73d0`, including explicit
growth bytes and allocation failure. This is global native quota logic, not a
fixed best-K replacement or native language output/winner selection.

## Descriptor and storage proof

The 56-byte native descriptor contains count at `00`, logical capacity at `04`,
backing allocation count at `08` and storage pointer at `10`. Quota fields are
U32 `18/1c/20` for primary minimum, transition and initial capacity, and U32
`24/28/2c` for the expanded pass. Source index at `30` is signed. The scalar
quota reader checks these exact offsets; it does not dereference the pointer.
The owned descriptor carries its storage identity as explicit opaque U64 bits.

The additive `LegacyNativeWordRecordHeap.GetSlotStorageSnapshot()` returns an
owned copy of all capacity slots, including inactive bytes. Existing allocator
behavior is unchanged. Quota descriptors retain full allocated backing even
when logical capacity shrinks. A later resize within that allocation reuses
those exact bytes, rather than initializing an invented zero-filled allocation.

`1f73d0` writes logical capacity before calling the allocator. If allocation
fails, that new capacity remains written; old allocation bytes, count and
storage identity remain. The owned implementation preserves this failed state
and refuses new insertion into the incomplete descriptor. A growth provider
supplies the full reallocation and explicit new storage identity, or null for
failure. It must preserve prior bytes and supply every new byte. Neither zeroes
nor an invented pointer are presented as native allocator output.

When shrinking below active count, native truncates its max-heap physical prefix
and reinserts the old tail. The descriptor performs this directly in its backing
array with the literal signed-cost `1f75b0` rules, including right-child ties,
whole128 movement, root callback before movement and equal-cost rejection.
It does not rebuild or copy an entire heap per discarded record. Rejected tail
sources invoke the drop callback; accepted replacements invoke it on the old
root. Copying a full record restores all bytes after allocator clearing.

## `212ec0` decision sequence

The ordinary pass budget is prepared ushort `230a`. For any nonzero pass argument,
the budget is truncation of Single arithmetic
`(22e8 bit4 ? 2 : 1) * float230c * ushort230a`, converted through native long
and low32. If the unsigned total active count already fits, no capacity changes.
Knowledge float `ec` is an explicit live property; its constructor default zero
is not inferred from an acoustic resource.

With positive `ec`, the paired descriptor array splits at half its count.
Non-general secondary buckets first resize to their pass transition, promoting
evicted records into the corresponding primary bucket. If minimum is less than
transition, they then resize to minimum using `1f8be0`: try secondary general
bucket; its root eviction promotes into primary general bucket; if the incoming
record is rejected, try that record in primary general bucket instead.

Non-general primary buckets resize to transition without promotion, then to
minimum with drops promoted into primary general bucket. The nonpositive-share
branch applies this primary sequence across all non-general buckets.

Counts of all non-general buckets (excluding the secondary general bucket for
positive share) are subtracted from the budget. While fewer than one general
slot remains, the highest signed root cost among those buckets is removed.
Strict comparison keeps the earliest bucket on root-cost ties. Native starts
this search at `-4000000h`; if no eligible root exceeds that sentinel, native
would use an unavailable descriptor index. The checked API refuses that case
instead of inventing a bucket or reading outside the array.

For positive share, Single `remaining * ec` truncates to the secondary general
capacity and is subtracted from primary remaining capacity. Secondary general
resize promotes drops into primary general; primary general is resized last
without promotion. Allocation failure stops at the same call, preserving the
already-written descriptor capacity and previous mutations. Optional diagnostic
promotion observations carry owned whole-record snapshots; rollback is not claimed.

## Checked boundaries

Prepared quotas and storage identities remain explicit. This batch does not
port their `1fbb90` source-weight production or `212d80` reset pass, nor choose a
native language winner/confidence. Supported finite secondary shares are at
most one; nonpositive values use the native primary branch. NaN/infinity,
outside-long float conversions, capacities above4096, arrays above512,
unpaired positive-share work and trace transport refuse. Values are not silently
clamped into a different native policy.

`GetHeapSnapshot` is an owned consumer view; the authoritative descriptor
mutates its backing directly. The former can be unavailable after a failed
growth, while raw allocated backing and scalar failure state remain inspectable.
Quotas, controller phase/flags and allocator output are not guessed from handwriting.

## Validation

Managed and Windows Native AOT passed 512 independent complete global-quota
streams, 115 explicit realloc requests, 26 native allocation failures and 736
whole-record promotion attempts, plus128 scalar descriptor views. A separate
scalar quota model uses the frozen functional heap/permutation oracle and explicit
random nonzero growth bytes. It checks every allocated byte, capacity versus
allocation, storage identities, positive/nonpositive branches, both pass kinds,
Single rounding, unsigned negative-budget wrap, callback target/order, root
ties, cascaded fallback, failure stopping points and reused inactive storage.
A fixed equal-root case proves earliest-bucket eviction. Snapshot ownership,
cancellation, incomplete-allocation refusal and unavailable sentinel selection
are checked separately.

Actual RU source/cuts/context/neural/cache/alternative records feed the native
default2/2 merge stage, then `212ec0` general-bucket quota with explicit budget
three and zero share. The resulting three stored records consume80-byte primary
and80-byte secondary pool blocks and feed the next continuation's exact root
anchor. The old full allocation remains retained. Prepared budget/general
descriptor, alternative creation and storage identity are diagnostic inputs;
this is not the complete native search scheduler or handwriting acceptance.

```powershell
python tools/LegacyWordQuota.Probe/reference_controls.py .artifacts/legacy-word-quota-controls.json
dotnet run --project tools/LegacyWordQuota.Probe -c Release -- .artifacts/legacy-word-quota-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```
