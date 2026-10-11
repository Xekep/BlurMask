# Prepared ordinary linguistic-mask attachment

This stage closes the concrete frontend-to-acoustic-controller attachment that
was still unresolved in [the controller implementation](legacy-linguistic-symbol-mask-controller.md).
It ports the observable **prepared-source** producer/consumer integration. It
does not port the complete source factory, refresh machinery or object lifetime.

## Native binding evidence

All addresses refer to the pinned x64 `MyScriptText.dll` with SHA256
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
Inspection reads PE bytes and static decompilation; no vendor binary is executed.

| Stage | Static native evidence | Meaning |
|---|---|---|
| Linguistic controller construction | `1eb340`, configured source count nonzero and config+30 false | Creates class240; zero-source and alternate-mode branches create different classes and are outside this port. |
| Container attachment | Class2b3 descriptor `274aa0`, vtable `274af0`+18 → `00cab0` | Assigns container+60=controller; nonnull controller+18=container backpointer. |
| Acoustic frontend | Class222 descriptor `2de8a0`, vtable `2de610`+60 → `151ce0` | Creates backend if absent through `150d00`, then calls backend.virtual28 with container+60. |
| Acoustic resource selection | Class247 descriptor `2e5ba0`, vtable `2e5a20`+98 → `1d6330` | Returns source profile+140, the ordinary acoustic resource in the observed RU bundle. |
| Ordinary attachment | Class21d descriptor `2e3c80`, vtable `2e4180`+28 → `1c0ce0` | Assigns ordinary+190=controller. Its bytes are `4c898290010000c3`. |
| Mask callback | Class240 vtable `2e7cb0`+A8 → `206720`; consumer `1b8850` | If runtime1a7 is false and controller/callback are present, preparation and ordered source enumeration feed the ordinary mask producer. |

The previous ordinary initializer `1b0760` writes its config pointer into
**runtime188**, not runtime190. The separate `1c0ce0` setter is the missing
binding. Class240+18 is a **container backpointer**, not the RU source profile.
These distinctions prevent confusing configuration or frontend flags with model
resource fields.

`1503f0` initializes the fresh class222+A0 ushort to zero. `150d00` copies its low
byte into config+24, which `1b0760` stores as ordinary runtime1a7. This proves the
fresh object's flag value only. Later configuration, complete factory success,
runtime refresh and alternate wrapper modes are not inferred from it. The public
adapter therefore requires both `languageControllerBypassed` and
`preparationSucceeded` explicitly.

## Actual Russian resource attachment

The pinned `models/ru-RU/ru_RU-ak-cur.lite.res` has SHA256
`7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978`.
Its outer resource is concrete class247/v10d. Its stored **source kind** at byte
5233 is 0x135; that is not its concrete class descriptor. Reader `1d4950` consumes
the enabled flag at byte188156, then assigns the three following resources in
order:

| Field | Resource offset | Kind/version | Total bytes including 8-byte outer framing |
|---|---:|---|---:|
| profile+130 | 188157 | 297/v106 sampler | 1222 |
| profile+138 | 189379 | 21b/v10a source graph | 355 |
| profile+140 | 189734 | 21d/v110 ordinary acoustic cache | 382839 |

`196490` returns profile+138; `1d6330` returns profile+140. This connects the real
RU resource to the ordinary controller setter rather than assuming its runtime
type. The static auditor checks the nested headers, adjacency, lengths, concrete
class metadata and four accessor byte sequences. It does not construct objects.

## Independent implementation and controls

`LegacyPreparedOrdinaryLinguisticMaskBinding` snapshots the supplied source
tokens/maps and attaches them to the ordinary alphabet/header. Caller mutations
cannot change an attached snapshot. Its `Build` combines the existing restored
controller and ordinary producer:

- Explicit bypass uses `UnboundOrBypassed`, without invoking preparation.
- Failed preparation or enumeration uses `Failed`, discarding a partial seed.
- Successful preparation/enumeration uses `Succeeded` and its binary seed.
- Knowledge groups, user restriction, contextual forms and wide pairs remain
  explicit, ordered upstream inputs; no script aliases are fabricated.

The independent Python oracle specifies an original nine-label alphabet and
set-based source/restriction outcomes. It produces 256 cases across success,
failure, bypass, dictionary mapping, wide callback tokens and packed restrictions.
The C# probe also checks immutable snapshots, malformed input, cancellation and
source→ordinary mask→final cache propagation, including a deliberately best
acoustic score on an excluded Latin label.

The optional actual-resource check uses the real 192-label acoustic alphabet,
139-label lexical alphabet and v110 cache profile. The prepared exact dictionary
source yields 145 enabled primary labels. Its mask rejects a deliberately best
excluded class when 180 **controlled test costs** pass into the 195-entry final
cache. Those costs are not neural inference or a handwriting fixture. Both Latin
`p` and Cyrillic `р` remain distinct enabled labels in the observed dictionary.
No OCR accuracy, confidence or Russian word-accuracy claim follows from this test.

Managed Release and Windows NativeAOT pass these controls. Reproduce without GUI:

```powershell
python tools/LegacyLinguisticBinding.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/linguistic-binding-controls.json
dotnet run --project tools/LegacyLinguisticBinding.Probe -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/linguistic-binding-managed -- D:/BlurMask-build-artifacts-20261011/linguistic-binding-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res
dotnet publish tools/LegacyLinguisticBinding.Probe -c Release -r win-x64 -p:PublishAot=true --artifacts-path D:/BlurMask-build-artifacts-20261011/linguistic-binding-aot -o D:/BlurMask-build-artifacts-20261011/linguistic-binding-native
& D:/BlurMask-build-artifacts-20261011/linguistic-binding-native/LegacyLinguisticBinding.Probe.exe D:/BlurMask-build-artifacts-20261011/linguistic-binding-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res
```

Optional static audit needs a separately available, hash-matching DLL; the tool
does not download, load or redistribute it:

```powershell
python tools/handwriting/audit_legacy_linguistic_mask_binding.py --text-dll .artifacts/myscript-analysis/sdk/build/x64/MyScriptText.dll --acoustic-res models/ru-RU/ru_RU-ak-cur.lite.res --out D:/BlurMask-build-artifacts-20261011/linguistic-mask-binding/static-report.json
```

## Remaining boundary

Full `20d8f0` source refresh, decorator policy and epochs, allocation/failure
handling of `03fa80`/`150d00`/`1eb340`, alternate controllers/wrappers and live
linguistic configuration are not implemented by this adapter. Preparation is an
explicit caller result, not automatically declared successful. The source graph
exists in the observed resource triplet, but the integration control exercises
the final cache directly; it does not claim an automatic graph→word pipeline.
