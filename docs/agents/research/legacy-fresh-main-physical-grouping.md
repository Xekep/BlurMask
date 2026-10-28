# Fresh main source physical memberships

`LegacyFreshMainPhysicalGrouping.Prepare` owns the actual pinned Russian main
`247/v10D` source at offset0, its direct unscoped raw physical input, source scale
and the unknown-line2E3 memberships used by `15B260`. It does not accept a guessed
geometry profile, baseline, physical memberships, receiver half-X flags or a
first-row model substituted for the main source.

The pinned acoustic source and both committed portable embedded model resources
are read as data. A private vendor DLL is unnecessary at runtime. Resource/model
byte counts and SHA-256 identities are checked before preparation. No vendor code
or GUI is executed.

## Native ownership and configuration

The fresh source has no prior object, page guides, property events or input packet
boundary events. Its physical ingress uses `167210` through
`LegacyFreshOrdinaryRawPacket.PrepareDirect`: source descriptor0 initially covers
all retained physical strokes. Its point identities still refer to original raw
physical ordinals and original point ordinals; pen lifts do not become descriptor
ordinals. The scale snapshot reuses that exact retained ingress owner.

`1D41E0` source configuration and `163970` select ordinary kind`135`, coordinate
mode2, scale parameters`(.96,.85,3,10)` and processor flags`(0,1,1,1)`. Fresh
`15A1C0` sees no prior state/line rows and checks processor6 (`2AB` at5309): its
child`2A9` at5325 has byte9 zero, selecting `2E3`, rather than the page/prior`2E2`
branch. `1680D0` invokes `15B260` when source byte29 is set, as it is here.

`15B260` supplies the source's primary/secondary scales through provider slot38,
then receives its physical strokes through slot48 and invokes the unknown-line
slot50 branch. Fresh physical association is?1, so its receiver half-X argument
is false. The receiver writer `077974` separately stores byte4 zero: the signed
association field at source physical+4 is not that half-X flag.

Provider initializer `076BA0` takes source219+8. It forwards the actual coordinate
mode byte at source+14 to runtime1C, so the quarter-vertical-when-half-X flag is
true for this resource. Every fresh receiver half-X flag is false, so that shared
flag does not alter these samples. The initializer's internal quantile is0.8;
constructor `076780` supplies window width5 and overlap0.75. These are the actual
fresh no-property-overrides policy, not defaults for other source/controller
lifecycles. Physical resolution remains a mandatory actual source setter value;
zero is legitimate, and the API does not invent desktop DPI.

The30/50-state classifiers are the exact resources embedded at Text addresses
`18029E9D0` and `180277280`, already stored under `models/line-geometry`. Their
42-feature/affine/BLSTM path, deferred vertical sweep, singleton fallback and
`07A850` mean-Y/postselection order run through the existing recovered producer.

## Additive grouping API and regression preservation

`LegacyOrdinaryNeuralLineInferenceFactory.InferGrouping` exposes the previous
`Infer` grouping calculation before its explicitly supplied24A geometry callback.
`Infer` calls that shared implementation and retains its original sorting,
Gaussian fitting, deskew and numeric outputs. The grouping result retains its
source snapshot, memberships, neural scale, vertical-strip count, tiny physical
indices and actual NN pair evidence. It carries no line frame.

32 independent NumPy controls read the real portable model weights and generate
memberships without invoking C#. They include source duplicates, reversed input
chronology, multiple physical strokes/rows and far-separated vertical strips.
Managed and Windows NativeAOT pass104 actual NN pairs,733 original retained point
keys and10 multiple-strip cases, including all prior `Infer` memberships and pair
values. Zero runtime resolution/singleton, bounded input, malformed source/model
and precancellation guards are also checked.

The older512 sweep/deferred-continuation controls,16 raw/synthetic model controls
and actual printed MIR row/frame/word goldens verify that the refactor preserves
the established old `Infer` numerical behavior. These are scalar/model controls
and original printed fixtures, not held-out Russian handwriting accuracy.

## Remaining main lifecycle boundary

The native caller invokes `15B260` **before** `15F050`. `15B260` may split the
original88-byte descriptor into separate ranges and copies provider group fields
20/24/28 into descriptor metadata. Those fields and the descriptor split are not
implemented by this new memberships endpoint. Applying `15F050` directly to the
initial coalesced descriptor0 would therefore skip a native producer and is not
performed here.

The subsequent physical insertion/order algorithm, `16AA00` physical metadata
reordering, general24A/24B main descriptor/guide lifecycle, nonempty main contexts
and automatic final main winner remain separate work. This helper does not claim
that the existing single-physical main endpoint now supports multiple physical
strokes, and it does not fix live font x-height or complete OCR on its own.

```powershell
python tools/LegacyFreshMainGrouping.Probe/reference_controls.py D:/BlurMask-checks/main-groups.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res
dotnet run --project tools/LegacyFreshMainGrouping.Probe -c Release -- D:/BlurMask-checks/main-groups.json models/ru-RU/ru_RU-ak-cur.lite.res models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res
python tools/handwriting/audit_fresh_main_physical_grouping.py --output D:/BlurMask-checks/main-groups-static.json
```

The last command is optional static evidence checking against the separately kept
pinned PE. It is not needed by the independent reusable library or model loader.
