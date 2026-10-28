# Ordinary acoustic symbol-mask producer

`LegacyOrdinarySymbolMaskProducer` restores the bounded mask construction in
Text1801b8850 and a separately explicit inactive-preselection working-copy path.
It accepts a prepared controller outcome/seed; it does **not** guess which
linguistic controller should supply that seed for Russian or any other language.
No vendor code is executed by the library or the controls.

The evidence is static decompilation and PE pointer/byte inspection of
MyScriptText.dll and MyScriptEngine.dll. Addresses below refer to that inspected
x64 snapshot, not an API guaranteed across vendor versions. The private analysis
exports live under `research/myscript/output/RussianMaskProducerNext`.

## Proven producer and operation order

Text1801b0760 forwards configuration `param3[6]` to runtime190 and the byte at
configuration18 to runtime1a7. Text1801c01b0 invokes1801b8850 with the packed
restriction at runtime2b8 and base masks at2c0/2c8. The following order matters:

1. If runtime1a7 is false, the runtime190 object exists, and its vtableA8 method
   exists, zero the primary mask and ask that controller to fill it. Failure
   enables all primary symbols. Success visits knowledge groups in stored order:
   a strictly positive coefficient and any enabled member enable the whole group.
   An earlier group is not revisited when a later group enables another member.
   The native branch then enables the exact `-` and soft-hyphen labels if present.
   A bypassed/unbound controller instead enables every primary symbol directly.
2. Enable every secondary symbol independently.
3. Apply the packed restriction, one little-endian bit per symbol, to both masks.
4. If alphabet.virtual68 reports generated contextual-form tables, visit labels
   in ascending order. An enabled secondary label enables its initial, medial and
   final forms in the secondary mask. An enabled primary label also enables those
   forms in the primary mask. This is one pass, not a fixed-point closure.
5. Visit the generated Unicode-wide pairs in stored order; an enabled endpoint
   enables both endpoints **only in the primary mask**.
6. Clear both masks for alphabet.virtual38 virtual symbols.
7. Reconstruct the dash primitive header from `-`, `_`, U+F0002, U+F0003, `ー`,
   `一`; reconstruct the closed primitive header from U+F0001. Restore the five
   unconditional special header entries in native order3,0,4,5,6.

Later expansion can therefore re-enable a symbol excluded by the packed
restriction. Applying that restriction again at the end would change the native
behavior. Likewise, fixed-point transitive group/pair expansion changes it.

The public implementation validates binary callback seeds, finite coefficients,
indices, counts and cancellation. These checks are original defensive behavior;
they are not claims about native handling of malformed callbacks/resources.

## Contextual forms are not the serialized case triplets

The default engine character-property service at globalcontext78 has a vtable
at180202a70. Its slot30 is Engine18005f290: a script lookup over716 ordered
12-byte ranges at1801fb6d0. Inspected Latin, Cyrillic and Arabic script ordinals
are2,3,5 respectively.

Engine18004e280 tests whether an alphabet string is eligible for Arabic joining,
rejecting Arabic digit ranges and allowing common-script components after an
Arabic component. Engine1800573b0 constructs alphabet88/90/98/A0 tables from the
Unicode initial/medial/final decomposition kinds8/16/32. The type43 alphabet
vtable at1801de1b0 points slot70 to180052cd0, which copies those four ushort table
entries (or four65535 values when absent). The first entry is ignored by1b8850.

These generated tables are distinct from `LegacyInkAlphabet.TryGetRelatedForms`
and its serialized three-mode case/related-form maps. Feeding the latter into
the contextual-form argument without a binding proof would be incorrect.
All192 observed Russian acoustic labels were inspected; none are Arabic-joining
eligible. This contextual-form expansion is inactive for that observed alphabet.
The Unicode-wide pairs are also empty there; they are not Latin/Cyrillic aliases.

## Checked v110 resource groups and downstream bridge

`ReadV110KnowledgeGroups` reads resource c8/d0/d8/e0 after the already checked
`LegacyDirectionalAcousticWeightConfiguration` prefix. Text1801c2940 stores a
signed group count, `count+1` ushort range offsets, concatenated ushort symbols,
then one common float coefficient for versions before115. Text1801b0560 copies
that resource body to runtime. The new reader accepts only the proven v110
single-profile/no-size-penalty/no-obsolete-conversion branch; it does not pretend
to support newer per-group coefficients or unrelated versions.

The observed `models/ru-RU/ru_RU-ak-cur.lite.res` ordinary node at byte189734 is
type21d/version110 and contains zero knowledge groups. Artificial inserted groups
test the resource reader independently without being shipped as models.

`CopyWithoutClassPreselectionForSources` ports the inactive branch at the end of
Text1801b8eb0: copy both base masks to distinct working masks. The earlier active
neural class preselection branch is outside this API. The helper then computes
the distinct physical source-ID count plus template prefixes with
`LegacyPreparedSourceStrokeCount`, including native byte wraparound, and feeds
that byte to `LegacyAcousticSymbolRangeFilter`/Text1801b95a0. That byte is a source
count, not a script or case category. Observed v110 has null148/150 bounds, so the
actual resource disables this later source-count filter.

Controls pass prepared primary masks to `LegacyOrdinaryAcousticClassCache` and
`LegacyPreparedAcousticCandidateScorer`: an excluded Cyrillic label with the best
neural cost remains impossible and cannot reach candidate search. This proves
composition of the prepared bounded stages, not which concrete default Russian
controller emits the initial seed or the final application's full word accuracy.

## Verification and remaining boundary

The original Python oracle uses sets, one-pass ordered updates, and a distinct-ID
set count. It generates384 deterministic independent controls spanning all three
controller outcomes, positive/zero/negative groups, packed restrictions,
contextual forms, wide pairs, virtual/special labels and source-count wraparound.
The C# probe adds malformed inputs, asymmetric expansion, non-fixed-point group
order, real RU resource-group decoding, inserted own groups, and final-cache/
candidate composition. The same controls pass managed Release and NativeAOT.

```powershell
python tools/LegacySymbolMask.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/symbol-mask-controls.json
dotnet run --project tools/LegacySymbolMask.Probe -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/symbol-mask-managed -- D:/BlurMask-build-artifacts-20261011/symbol-mask-controls.json models/ru-RU/ru_RU-ak-cur.lite.res
dotnet publish tools/LegacySymbolMask.Probe -c Release -r win-x64 -p:PublishAot=true --artifacts-path D:/BlurMask-build-artifacts-20261011/symbol-mask-aot -o D:/BlurMask-build-artifacts-20261011/symbol-mask-native
D:/BlurMask-build-artifacts-20261011/symbol-mask-native/LegacySymbolMask.Probe.exe D:/BlurMask-build-artifacts-20261011/symbol-mask-controls.json models/ru-RU/ru_RU-ak-cur.lite.res
```

Still unresolved: the concrete producer of configuration `param3[6]`, the actual
runtime190.virtualA8 default-language mask controller, active1b8eb0 neural class
preselection, and full word terminal/confidence orchestration. The dictionary
resource type2b7 is not established as that controller: its inspected runtime
vtable does not expose a matching A8 method. No manual Latin/Cyrillic replacement
rules or corpus-fitted controller seeds are added to compensate for these gaps.
