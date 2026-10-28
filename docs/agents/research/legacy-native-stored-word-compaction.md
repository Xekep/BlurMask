# Stored secondary-row compaction

`LegacyNativeStoredWordComparer` and `LegacyNativeStoredWordCompaction` port the
nontracing Text `1fce60 -> 212900/1f8310 -> 212b10` producer/consumer chain over
owned stored88-byte word rows and their complete secondary alternative blocks.
This stage sorts and compacts a row after materialization. It does not select
the final word, reconstruct a winning path, run the full `2144f0` scheduler, or
publish acceptance confidence.

The exact comparison order is record28 unsigned64, record21.low6,
record27.bit0, symbol24 unsigned16, packed record18 unsigned32, graph arc1c
unsigned16, and record21.bit7. If those fields agree and source IDs differ, a
left source of kind1400h sorts before the right; otherwise a right1400h source
sorts after the left; two other sources compare equal. Two different1400h
sources therefore compare negative in both directions. This native property
is preserved rather than replaced with an antisymmetric .NET sort rule.

The native `212900` assembly copies the complete middle88-byte row as pivot,
partitions from both ends, recurses on the lower partition, then handles the
upper partition as a tail call. `1f8310` swaps all88 bytes and every40-byte unit
of the row's count2404 alternative block. The implementation uses an explicit
partition stack with the same order. It refuses a scan outside the supplied
partition or the native ushort right-index wrap toFFFF instead of inventing
an ordering for an unsafe input. A zero primary limit can reach that native
wrap; ordinary supported fixtures use a supplied positive prepared limit.

`212b10` reads active count23f0 and primary limit230a. The primary prefix is
untouched. After sorting the secondary suffix, adjacent equal rows retain
the strictly cheaper signed candidate cost10; ties keep the existing leader.
New leaders and cheaper duplicates move by full row/payload swaps, not partial
copies. If none of the retained secondary rows has source kind0200h..0A00h,
the function scans the discarded suffix and recovers its first matching row.
This can deliberately preserve a more expensive dictionary row that compared
equal to a cheaper C00h row. All inactive storage bytes and unused payload units
are retained; nothing is initialized to guessed native zero values.

The API checks complete row capacities, active count, source-header ownership,
and enabled alternative storage before copying. The caller supplies the live
prepared header table and actual count2404, including count2. Returned arrays
are owned snapshots. The function returns the retained count while preserving
the full capacity and discarded rows, as required by later controller stages.
It supports the nontracing path; trace640-byte clone relocation is separate.

The Python oracle independently generates4096 exact comparison cases,
including33 non-antisymmetric source pairs, and1089 complete row/payload sort
and compaction cases. These exercise lowest signed cost, duplicate retention,
primary/inactive bytes, count2..4 full payload transport,1400h priority, and the
discarded dictionary recovery. Seven guards cover malformed extents, missing
payload/header ownership, cancellation and the explicit ushort wrap boundary.

The actual-resource fixture carries11 NN-derived initial word rows through
native ordinal assignment, the root20e6a0 producer with full2/2 alternative
pools, native heap ordering/merge,213410 materialization, and the new212b10
consumer. Each retained row keeps the corresponding80-byte secondary payload
and its real candidate cost. Its prepared primary limit1 and identity symbol
mapping are explicitly supplied fixtures. Record28 is still the initialFFFF
placeholder in this scope; the fixture does not claim that linguistic204290
or native fresh source/row orchestration populated it.

```powershell
python tools/LegacyStoredWordCompaction.Probe/reference_controls.py .artifacts/stored-word-compaction-controls.json
dotnet run --project tools/LegacyStoredWordCompaction.Probe -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/stored-word-compaction-managed-artifacts -- .artifacts/stored-word-compaction-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

The same probe supports Windows NativeAOT and statically inspected vendor
code is not executed.
