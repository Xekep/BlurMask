# Fresh first-result ownership factory

`LegacyFreshFirstResultOwnershipFactory` creates the checked raw packet, actual fitted first frame, first-source model and direct invocation policy together. It then creates the actual274 pool and279/275 plain publisher with source-derived policies. Callers no longer need to supply diagnostic `false` flags for this factory's fresh branch. This closes flag ownership, not final winner emission or the later main247 frame.

## Capture producer → consumers

Fresh driver initializer155EF0 and driver clone initializer155DA0 set byte220 tozero. The new driver does not copy an external reused capture override.158970 passes driver220 into156630, which writes configuration9B. Configuration00ECA0 dispatches property12D0081 only when9B is nonzero; controller1F2480 changes runtime23F101 only for that event. Fresh23F constructor1EDB60 writes word100=0;1EDDE0 writes only byte100 from configurationAA, not101. Therefore this **no-property-event fresh invocation** keeps101 zero through preparation, rather than assuming a generic controller default.

Separately157C60 forwards driver220 as the one-byte TOP273 constructor argument.13B870 writes it at27341 and passes that byte into274 constructor13AB80, which stores capture at27410. This proves the actual glyph pool capture branch is disabled in the same fresh lifecycle. A reused driver, supplied event or custom capture configuration is outside the factory; it is not silently reset by a general API.

156200 sets new configuration6C=1. OwnerC4 is supplied by the already verified fresh first binding and remains zero. The result factory preserves those proven settings alongside the actual invocation owner.

## Word finalization/order producer

ControllerF0's constructor value is insufficient:1EF070 overwrites it from the selected resource's virtual68. For the actual source247 interface table2E5A20 this is1D62A0, an exact byteB9 getter. Reader1D4950 fills B9 from the serialized source prefix, immediately afterB8 and beforeBA/BC. The checked profile's `ParametersOffset-5` therefore identifies the serializedB9 byte without guessing a model-dependent fixed offset. The pinned first source6120 uses coordinate mode2/count0 and hasB9 at11376, value0.

1F42D0 passes F0 to279 publication1416B0, which forwards the same flag to275 finalization13FE20 and uses it for geometry sorting. The factory derives `FinalizeWordGeometry` and `SortWordsByGeometry` from actualB9. A resource withB9=1 is retained as requesting geometric publication and explicitly refused by the plain result factory; it is not coerced tozero or replaced with bounding-box geometry.

## Ownership and fingerprint

The factory snapshots the input resource before parsing/preparation and retains its `LegacyFirstAcousticModel`, `LegacyFreshOrdinaryPacketFrame` and `LegacyFreshDirectFirstInvocationPolicy` together. `ValidateInvocation` requires those exact objects; equal-resource foreign frames remain different invocations. `SourceFingerprint` is the actual first resource-node SHA256 already carried by first neural score channels. `ValidateScores` checks that fingerprint and both model output widths. This is a source-model check, not a replacement for per-group/candidate ownership or final-confidence selection.

Every `CreatePlainResult` returns a fresh shared274 pool/publisher owner. WORD276 reference chains and published275 records retain that pool. Cross-result words are rejected. No native pointers, labels, geometry280 buffers or source boundaries are invented from the factory flags.

Independent tests derive source-slice hashes in Python, use three actual source variants (original, altered inactive metadata andB9=1), prepare raw packet/frame/first ownership, compose128 factory-produced274/276/279/275 plain fixtures, and check real fixed-input neural score fingerprint guards. Same-resource foreign invocations, malformedB9, cancellation and the actual trueB9 refusal are covered. Word fixture source callbacks remain explicit prepared controls; they do not claim handwritten word winners.

```powershell
python tools/LegacyFreshFirstResultFactory.Probe/reference_controls.py models/ru-RU/ru_RU-ak-cur.lite.res D:/factory-controls.json
dotnet run --project tools/LegacyFreshFirstResultFactory.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json D:/factory-controls.json
python tools/handwriting/audit_fresh_first_result_flags.py --output D:/factory-flags-audit.json
```

The static auditor checks14 concrete native bindings from the pinned Text PE18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f. Probes are headless managed/NativeAOT code and never load a vendor DLL. Actual720-byte first-result source rows → main247 frame, winner glyph/word materialization and general geometric publication remain separate stages.
