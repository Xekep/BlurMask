# Prepared acoustic permissions and dictionary source descriptor

This restores a bounded producer-to-consumer path: prepared ordinary final cache rows → permission bytes → source `virtual68` working-map refresh → packed dictionary/native lexical scoring. It also reads the actual RU source name, fixed-point weights and language order before deriving its fresh descriptor. It does not implement the native group-selection/cache lifecycle or final recognition acceptance.

## Static evidence

The inspected `MyScriptText.dll` SHA-256 is `18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`. Vendor binaries are read statically; the probes execute only the independent library.

* Ordinary type21d table `1802e4180`, slot48, points to `1801c01b0`. Controller `1802144f0` forwards source+30 through frontend222 table `1802de610`, slotD0=`1801520e0`. That wrapper supplies callback `1801520b0` and dispatches to the ordinary backend's slot48.
* `1c01b0` resets the permission buffer at runtime+2e8 and seeds six slots with3: alphabet object+10/+12/+14 (serialized header fields4/5/6), followed by exact U+F0000/ZWNJ/ZWJ lookups established by `1b0760`. Missing labels remain FFFF.
* For each selected successful cache row, costs below `0x4000000` contribute3, or1 in the special state. Independently, costs strictly below trunc(binary32(scale×40000)) contribute4. All contributions are ORed across rows.
* The special state startsfalse once per invocation. Only a **recomputed kind1** row changes it, using the graph record's presence and low3 flag bits. Ordinary kind0, kind2 and reused rows retain its previous value. Replacing this with a per-row default changes native behavior.
* Failed preparation does not reach the source callback. The independent API returns failure without an accepted partial buffer or working-map snapshot.
* The successful tail calls source `virtual68(permission)`. Aggregate28d method `180236ca0` dispatches to children in order; dictionary2b7 method `180093f80` rebuilds its working50 map from preserved58 before applying those permission bits. The immutable helper composes with the previously restored working map rather than mutating shared state.
* `1801bc900` writes scale1 in the disabled-power branch. The actual RU v110 profile has runtime100=0, checked by the existing strict profile reader. Other profiles/scales are not inferred.

## Identifier service and descriptor

Engine `180031910` installs global+90 from registration pointer `180252408` → descriptor `18020f3c0`, service37. Its method table `18020f290`, slot10=`180089680`, implements **byte-ordinal substring search**, NUL-terminated when no explicit end is supplied. This is a string service, not a dictionary metadata key lookup. Host registration overrides are outside this bounded builtin implementation.

Source139 identifier+14 is the resource name. Text133 reader `1801d2680` obtains56 name bytes and calls `1801d22e0`. In the checked markerFFFFFFFF branch the decoder XORs all56 bytes with repeating `14 12 19 77`. The new prefix reader checks that exact layout: type13b/version300, inherited139301/138100, the marker, scalar weight format and alphabet2b103 header. It reads data without executing authentication or loading later nested objects.

Source139 reader `1801d6700` supports an initial scalar word whose high byte is0 (weight60=word, weight64=65536) and a tagged explicit pair of signed weights. Its getters `1801d6de0`/`1801d6df0` return fields60/64. `092990` converts both to binary32 weights with multiplier2^-16. The v301 scalar field68 and runtime language scale at6c are retained separately.

Language2d1 reader `1800fcc10` writes the first post-base uint, constrained to1..3, into language+60. **This is n-gram order, not case mode.** The existing full RU language loader independently checks order2 and its tables. The scalar reader alone does not validate all language tables.

Fresh2b7 constructor `180092790` starts a zero descriptor and sets query node80=1. `092990` derives:

* bit1 from the supplied prepared binding's presence;
* bits2..4 from language order×4;
* bit5 from whether packed node1 has children;
* category800 only when that interval is nonempty and the name contains lowercase `subword`; otherwise categoryC00.

`180091ce0` directly indexes its supplied packed node; no1→0 translation is performed. The API consequently probes `GetChildCount(1)`, not the public graph root0.

For the pinned Russian source at LK bytes606964..4565174 the decoded name is `ru_RU-lk-text.lite.lex`, weights0/1, runtime scale1 and language order2. **Packed node1 has no children**, so the fresh prepared-language descriptor is **0xC0A**, not0xC2A. Its categoryC00 is resource-derived, not a guessed script/category policy. The actual reserved acoustic slots are182,180,181,190,FFFF,184.

## Executable checks

`tools/LegacyAcousticDictionaryPermissions.Probe/reference_controls.py` generates512 independent deterministic set-union controls. It rounds multiplication to binary32 before deriving the threshold and covers threshold±1, impossible±1, signed extremes, kind1 state changes followed by ordinary/reused rows, reserved symbols, and both source category800/C00 filtering. The managed/native probe additionally checks84 source descriptors, empty and case-sensitive names, both scalar weight formats, NUL truncation, language orders1..3, node1 leaf/nonleaf, malformed headers, cancellation and failure/no-forward semantics.

Opt-in actual model checks hash-validate the RU bundle, derive the descriptor from its source prefix and language object, and obtain the145-symbol mask from a prepared dictionary controller/binding. Ordinary source ingress supplies actual per-physical enclosure/cut probabilities: a scalar NaN intentionally confirms that this endpoint does not substitute a guessed scalar probability. Real NN/context/cuts and final195 cache rows feed the new permission producer and then the frozen working map and actual lexical terminal scorer.

These are two original diagnostic drawings, with explicit guide baseline90/height80 and explicit prepared kind0/partial-cache scope. Both evaluate28 real cache rows; permission bit1 retains64/43 symbols. The original bounded beam (public dictionary root0) returns128 terminals in each case. **This is not the native query lifecycle beginning at node1, native acceptance confidence, or a word-accuracy test.** Expected handwritten words are not used for inference, no Latin/Cyrillic aliases or manual corrections are applied, and no probability is invented. Native callback selection, graph flags/recomputation, active source-chain factory, automatic frame integration and winner/confidence remain upstream responsibilities.

Managed and Windows NativeAOT checks passed with identical descriptor, permission counts and terminal totals. Observed NativeAOT times were17.50ms/18.67ms on this machine; they are diagnostic timings, not a portable benchmark.

```powershell
python tools/LegacyAcousticDictionaryPermissions.Probe/reference_controls.py D:/controls.json
dotnet run --project tools/LegacyAcousticDictionaryPermissions.Probe -c Release -- D:/controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

Omit the four model arguments for independent synthetic controls only. No model samples, vendor exports or new binary assets are added by this batch.
