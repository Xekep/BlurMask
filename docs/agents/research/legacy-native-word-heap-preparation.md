# Native word source, limit and heap preparation

`LegacyNativeWordHeapPreparation` restores fresh source summary **20d8f0**,
quota field production **1fbb90**, and empty-heap allocation through the owned
**1f73d0** descriptor. It supplies quota fields and initial capacities from
their checked producers. Search scheduling, native winner/confidence and
language output remain separate.

Static source: MyScriptText.dll SHA-256
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
No vendor DLL is executed. Private exports are under
`D:/BlurMask-build-artifacts-20261011/root-heap-source-preparation`.

## Sources and controller limits

Dictionary2b7 table **2b4360+28 = 093aa0** directly returns `object+20`.
Compact fields00/04/08 therefore read flags20 and weights24/28.
`FromDictionary` connects the verified descriptor producer to this view,
without inventing preparation success or reordering sources.

Fresh20d8f0 clears packed22e8. Low three bits take maximum language order;
bit3 accumulates header bit5. Bit4 accumulates that bit for categories other
than800; bit5 marks A00; bits6/7 accumulate header bits7/8. The last800/A00
sources supply22ec/22f0. Constructor20baa0 initializes both indices to-1.
Nonfresh mutable source arrays are outside this fresh summary API.

`LegacyNativeWordControllerLimitPreparation` restores the scalar block in
**2144f0**. Every iteration narrows actual frontend.virtualC8 to ushort230a.
Initially knowledge shorts14/16/18 set header2400, primary2402, secondary2404.
Only-1 is the default sentinel: header/primary default2; secondary defaults1
when mode22f6 is zero and packed language order exceeds1, otherwise2. Other
signed short bit patterns retain their unsigned output; downstream pool scope
is separate. Later iterations preserve all three counts.

A primary/secondary count change requires native pool reset before storing
counts. The returned flag exposes that requirement; it does not perform native
pointer freeing. Header-only changes do not request reset. Knowledge2b3
constructor **00c860** writes shorts14/16/18=-1. Child **156630** writes
primary/secondary1 and header1 if its first source exists, otherwise20.
These distinct paths show why controller constructor counts2/2/2 do not prove
prepared counts. Class222 **2de610+C8 = 1520a0** reads container+40; first
source obtains200 there, but this does not establish a main-source budget.

## Quota fields and allocation

Primary budgetA=ushort230a. Expanded budgetB uses Single arithmetic
`(packed bit4 ? 2 : 1) * expansion230c * A`, truncating through signed long
and low32. Exact weights0/1 route to general bucket0. Others append56-byte
descriptors and write their indices to source byte0C.

Minimum is0 for weightL<=0, otherwise max(1,trunc(budget*L)); transition is
max(1,trunc(budget*U)). Primary initial capacity is2*transition+2; expanded
initial capacity is4*transition+1, with native32-bit wrapping. Source30 is
the supplied controller source position.

At shareEc=0, a matching special source22f0 appends an override descriptor,
copies primary quotas, adds its primary minimum and zeros expanded quotas.
The scan includes the still-zero general descriptor's source30=0: an A00
source0 with weights0/1 can match it, whereas a later such source cannot.
General source30 is not prematurely changed to-1 before the actual native store.

PositiveEc splits original non-general quotas by1-Ec andEc. Zero minima stay
zero; all other scaled fields have minimum1. General primary remainder is
signed max(2,A-sumMin); expanded remainder is signed max(1,B-sumMin).
Initial capacities are twice/four times these remainders, split with the same
float rules forEc>0. The unsplit minimum sums remain in use after splitting.

Assembly **1fbcac** passes the entire old descriptor count in r9d to1f77b0,
not1. **1f77b0** zeroes every appended56-byte record. Secondary general copies
only quota bytes18..2F, so source30 stays0; primary general becomes-1.
The full metadata image retains zero reserved bytes. Record backing is not
assumed zero: explicit allocator bytes and portable storage tokens supply it.

Initialization allocates max(primaryInitial,expandedInitial)128-byte slots.
Failure retains the failed logical capacity, preserves earlier allocations
and skips later initialization. Allocator-callback cancellation is checked
before returning either success or failure.

## Controls and boundaries

Independent binary32 Python controls cover4096 controller-limit snapshots,
512 full56-byte preparations,2972 allocation calls and293 partial failures.
Cases cover flags, source order, exact0/1 routing, override scan, split rounding,
wrapping,512 paired descriptors, opaque bytes, zero-capacity allocation and
cancellation. Managed and Windows NativeAOT probes pass these controls.

Real-resource composition: RU descriptorC0A/weights0,1 plus explicit fresh
knowledge -> counts2/2/1 -> general-only capacity6/4 -> ten real NN/cache
candidates -> primary-budget3 global quota with opaque tails retained.
Budget3 is an explicit diagnostic frontend value, not a native default/winner.

Descriptor-array allocation must have succeeded. The checked API supports255
supplied source entries, finite share0..1 and the existing4096-slot descriptor
scope. Negative share has distinct nonpositive/exact-zero native branches and
is refused rather than clamped. Nonfresh mutation, descriptor-array allocation
failure, reset212d80, full scheduling and native acceptance remain unported.

```powershell
python tools/LegacyWordHeapPreparation.Probe/reference_controls.py .artifacts/legacy-word-heap-preparation-controls.json
dotnet run --project tools/LegacyWordHeapPreparation.Probe -c Release -- .artifacts/legacy-word-heap-preparation-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```
