# Aggregate dictionary query and ordinary owner

`LegacyPreparedAggregateDictionaryQuerySource` restores normal and auxiliary query dispatch `236910` over an already selected, ordered table. `LegacyPreparedDictionarySourceSelectionBuilder` supplies that order and the original source indices. The caller binds the corresponding child readers and shared `LegacySymbolContextFactory`; the aggregate maintains its own selected-source cursor and accepted-count bases. Selection, child creation/preparation, decorators, word startup and final winner selection remain separate stages.

## Native dispatch

Startup is exactly query node 0, source-ID byte 0 and header-byte bit 8. A startup request with global skip 0 resets the selected cursor and every base to `0xffffffff`. The first visit to a child sets its base to the global accepted skip. Its query receives `globalSkip - childBase`; the global value is restored before returning or moving to the next child. Only a zero child count advances to the next selected child. A negative count is nonzero and stops dispatch just like a positive page. One returned page contains records from one child.

Ordinary continuation uses source-ID byte minus one as the selected ordinal, without modifying the request skip or selected cursor. An ineligible child returns -1. Both paths forward normal/auxiliary mode and stamp only byte 4 of each positive 32-byte record with selected ordinal plus one. Node bytes, flags, context identities, symbols, remaining record bytes and supplied extra-cost records are preserved. Extra-cost clearing and native packed child offsets are implemented by the child reader, not by aggregate dispatch.

The implementation rejects out-of-range IDs, inconsistent node tags, backward relative skips and malformed callback extents. It owns copies of returned records/costs. Failure during an active request invalidates the session; request skip is restored in `finally`. Pre-entry cancellation leaves an untouched session valid. Cancellation is checked after both eligibility and reader callbacks, covering callbacks which cancel the token and return normally. Serialized access is required; reentrant callbacks are rejected.

## Proven ordinary owner binding

The pinned MyScriptText.dll SHA-256 is `18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`. The prepared class2b3/container and class222/unwrapped21b path has the following concrete identity chain:

1. Factory208 `1ec0a0`, in its nonzero config+8 branch, creates class222 and calls the supplied container's `virtual10`. Class2b3 table `274af0 + 10` is `00caa0`: container+48 receives that frontend, and frontend+8 receives the container.
2. Ordinary engine20e `1eb340` chooses controller class240 when its configured source count is nonzero and alternate config+30 is false. Container `virtual18 = 00cab0` stores it at container+60 with a controller+18 backpointer. It invokes controller `virtual18 = 20c1c0` with container[9], the frontend pointer stored at container+48; `20c1c0` stores that pointer at controller+20.
3. Refresh `20d8f0` passes controller+20 to the selected source's preparation callback. Dictionary `093970` stores that owner pointer at source+8 and obtains its active resource through owner `virtual18` before `092990`.
4. Dictionary eligibility `093ab0` examines `descriptorFlags & 0x1e00`. Only category `0x800` consults owner `virtual280`; other categories, including the observed Russian `0xc0a`, are always eligible.
5. Frontend222 table `2de610 + 280 = 155710` forwards to frontend+70 candidate graph `virtual1c8`. Ordinary21b table `2e1ff0 + 1c8 = 05a400` returns false (`xor al,al; ret`). Therefore this ordinary owner permits category `0x800` without a guessed controller mode.

`LegacyOrdinaryDictionarySourceOwner.BindDictionary` requires the restored typed ordinary graph and binds that proven false getter. It does not instantiate a native frontend or infer alternate graph/wrapper selection. The generic `BindDictionary` still requires an explicit live getter for category `0x800` when this ordinary scope is unavailable. The adapter does not alter dictionary labels, case aliases or confidence.

The opt-in original PE auditor `tools/handwriting/audit_ordinary_dictionary_owner.py` reads the pinned DLL as bytes and verifies seven concrete table pointers and four exact setter/forwarder/getter implementations. It never imports or executes vendor code. Larger factory/preparation identities above were verified from static decompilation; the audit is not a fresh factory execution test.

## Independent and actual-resource controls

`tools/LegacyAggregateDictionaryQuery.Probe/reference_controls.py` models accepted streams using original synthetic records and an independent Python oracle. **384 traces / 12,288 dispatch pages**, including **3,618 negative nonzero stop pages**, cover reset, global versus relative accepted skips, selected cursor, changing eligibility, normal and auxiliary mode, byte-exact stamping, cost transport and preserved request fields. Synthetic child responses isolate aggregate behavior; they do not claim the real child can safely clear costs for a negative native count.

Managed and Windows NativeAOT tests also cover 255 source IDs, cancellation before and during eligibility/reading, malformed counts/extents, request mutation, reentrancy, session invalidation, array ownership and copied base tables.

The hash-checked Russian path composes the recovered selection builder, two distinct dictionary query readers and one shared context factory. It returns **264 records in four pages**, with accepted bases **0 and 132**. Source stamping distinguishes the two selected readers while 139 initial contexts and continuation contexts are shared by exact acoustic/control keys. Auxiliary queries check the actual terminal gate. A separate category800 control exercises the proven ordinary owner; the real Russian resource remains categoryC00. This is executable prepared dictionary/context composition, not handwriting accuracy or complete native word startup.

```powershell
python tools/LegacyAggregateDictionaryQuery.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/aggregate-dictionary-controls.json
dotnet run --project tools/LegacyAggregateDictionaryQuery.Probe -c Release -- D:/BlurMask-build-artifacts-20261011/aggregate-dictionary-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
dotnet publish tools/LegacyAggregateDictionaryQuery.Probe -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/aggregate-dictionary-aot
& D:/BlurMask-build-artifacts-20261011/aggregate-dictionary-aot/LegacyAggregateDictionaryQuery.Probe.exe D:/BlurMask-build-artifacts-20261011/aggregate-dictionary-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
python tools/handwriting/audit_ordinary_dictionary_owner.py research/myscript/sdk/build/x64/MyScriptText.dll D:/BlurMask-build-artifacts-20261011/ordinary-dictionary-owner-audit.json
```
