# Fresh library-owned textField INPUT context

`LegacyFreshTextFieldInputContext.Prepare(actualCurrentView)` creates a new,
separate input/selection owner above the immutable ordinary XY root. It does
not interpret that root as an existing SDK page. It does not change the older
document selection policy or its original-ID registration rules.

The supported lineage is fresh default C20 -> ordinary raw appends -> a new
textField/addItems INPUT -> retained C22 -> full selected C20 copy. The input
registry ID is independent of raw-object ordinals. Fresh C51 starts with Next0
and maximum LONG_MIN. Raw append does not register a selection in this scope.
The first successful INPUT registration therefore returns ID0 and advances
Next to1, including when the raw root has256 objects. Category is100 decimal
(0x64), not0x100. No caller can supply the category, registry counter, tag,
absence of span children, or a replacement C22 owner.

`CreateSelectedCopy()` retains the original raw point bits and original IDs,
creates a distinct context and C22 identity, preserves the actual INPUT tag,
and restores the parent registry Next only after registration. Export rejects
the parent's C22 on the child and the child's C22 on the parent. Immutable
geometry is resolved through the checked original full-current-view owner;
the context separately owns its copied raw point storage. Cropped/rebased
copies and auxiliary metadata are outside this entrypoint.

`Input.CopyRangeStorage()` returns the actual full selected16-byte range.
`ExportInputIdentityTransforms(Input)` joins the exact retained C22 to the
existing checked06B7F0/06F080 geometry exporter. Its result keeps both context
and INPUT identity. This does not write a result2DA transform vector.

The private INPUT creation path has no span children: addItems creates the
input/tag and rejects prior setInputSpans. Document030FB0 visits children named
span and returns before allocation when none exists. Consequently the typed
new input exports zero DAF32 rows. This is not a rule that a nonempty tag, an
empty range, or arbitrary imported DOM implies no spans.

## Static ownership evidence

The four full PE SHA256 values are checked by
`tools/handwriting/audit_textfield_input_context.py`; it only reads bytes.

* Text2BC descriptor275F50 derives BE1. Its metadata275CB0 points to extension
  table276000 with the page namespace, initializer03FF40 and getter045740.
  The initializer registers actual textField marker275E98. The getter tests
  that exact marker and interfaceC98, returning275BD0. Its method042BA0
  writes kind0 and succeeds. This is separate from Document's drawingField
  getter, which writes kind5.
* Text0416C0 reads actual C98 kind at04194D..04197F, adds0x64 at04199D,
  calls C20.VE0 with name INPUT, the selected C21 and requestID-1, then writes
  the actual returned ID into input.tag via DOM.V128. Its earlier span-child
  guard excludes setInputSpans. It does not construct a span child.
* Ink063EC0 uses0638A0 with formatNULL/history enabled. This creates fresh
  C51 plus C4D and C5E observers, priorities0x6E and0x78. There is no SDK-page
  CB7 observer or external event source in this new private owner.
* C5E's raw append and tag before/after slots are020D60 no-ops. C4D slots
  078950/078AC0 and0797F0/079A10 retain history rows and queue commands.
  Engine003200 installs the history service; V8=0299D0 allocates a command
  and V90=02B0A0 queues it. Base70 ctor0292E0 zeros parent/listener60.
  C4D's actual primary slots28/30 are NULL. Queue pruning frees command
  payloads through077530 ->0772D0, not through C51 Add. This proof includes
  roots beyond the native75-command pruning threshold; raw count is not
  substituted for registry Next.
* C51 ctor09E4C0 initializes Next18=0, maximum20=LONG_MIN and dirty28=0.
  Add09EE20 updates the counters only after09BB20 succeeds, stores the
  retained selection and actual category, and keeps the auxiliary lane NULL
  for the supported ordinary addItems call. The API publishes only complete
  successfully constructed owners; it does not simulate native allocation
  failure or claim rollback of native partial intern allocations.
* C20.V128=06C880 retrieves the actual registry payload's retained C22, not a
  raw-object index. C20.V170=06E120 ->06D2C0 constructs a new target C20.
  At06DE38..06DF23 it creates C22 with target owner, passes preserved name,
  exact registry ID, new C22 and category to09EE20, and checks returned ID.
  The successful tail06DFAB..06DFDF copies source Next through09ED40, which
  requires that it exceeds the target maximum live ID.
* Document030FB0 finds input/span at031023..031063. Missing span jumps to
  03131F before the32-byte append at03106E..0310DB.

The audit covers26 regions,27 table bindings and77 instruction pins.
This is a supported new textField context, not proof of an existing native
SDK page/CBB context, imported layout tags, or prior history. The native
Document task's DOM/page ownership and result transport remain separate.

## Verification

Managed and fresh win-x64 NativeAOT execute4 recognition-owned raw roots and
256 additional fresh library roots. They cover396 selected copies,32910 raw
objects and3936 guards. The independent Python oracle consumes only raw point
bits and captured structural output; it imports no research files or expected
text. It verifies complete C22 range bytes, all17 assigned counter bytes,
copied point bytes, selected original-ID matching, matrices and zero DAF rows.
Counts75/76 and256 distinguish the registry from raw count and include sizes
on both sides of the native history-pruning threshold. History callbacks and
their lack of registration are checked statically, not executed as vendor
code by the probe. Native/Managed captures are byte-identical SHA256:
`56a5f90650802f598579b06148072ee8b7b370283a6cd9457f180a66b095aeb4`.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet build tools/LegacyTextFieldInputContext.Probe/LegacyTextFieldInputContext.Probe.csproj -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/line-textfield-input-build -p:UseSharedCompilation=false
python tools/LegacyTextFieldInputContext.Probe/run.py E:/BlurMask-build-artifacts-20261014/line-textfield-input-build/bin/LegacyTextFieldInputContext.Probe/release/LegacyTextFieldInputContext.Probe.dll E:/BlurMask-build-artifacts-20261014/textfield-input-managed.json
dotnet publish tools/LegacyTextFieldInputContext.Probe/LegacyTextFieldInputContext.Probe.csproj -c Release -r win-x64 -p:PublishAot=true -p:UseSharedCompilation=false --artifacts-path E:/BlurMask-build-artifacts-20261014/line-textfield-input-native-build -o E:/BlurMask-build-artifacts-20261014/textfield-input-aot
python tools/LegacyTextFieldInputContext.Probe/run.py E:/BlurMask-build-artifacts-20261014/textfield-input-aot/LegacyTextFieldInputContext.Probe.exe E:/BlurMask-build-artifacts-20261014/textfield-input-native.json
python tools/handwriting/audit_textfield_input_context.py E:/BlurMask-build-artifacts-20261014/textfield-input-static.json
```

Existing SDK page/layout/reused registrations, setInputSpans, cropped or
auxiliary copies, actual2DA/037E80 result binding and final0283B0/2E7 handoff
are not implemented by this context. No accepted document or font metric is
inferred from its source state.
