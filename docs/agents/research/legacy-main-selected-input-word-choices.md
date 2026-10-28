# Selected INPUT context to shared WORD choices

`LegacyFreshMainWordSourceChoices.PrepareSelectedInput(resource, completion,
selectedInput, table, publication, normalization, circles)` retains the exact
selected `LegacyFreshTextFieldInputContext` and its INPUT/C22 owner. Its
document must be the identical original raw document of the completed
configured shared275 source. A fresh root without a selected-copy parent,
a foreign document and pre-entry cancellation are refused.

The existing `Prepare(resource, completion, ...)` and standalone wrapper APIs
keep their original-ID reservation policy and old captures. The new route
does not overwrite a previously published context or replace a completed
candidate-local result. It constructs the shared CHAR/WORD context before
any source registration, through the same existing shared-source pipeline.
All ordinary base-source, actual WORD-array and lifetime guards remain.

The pending cursor starts at the retained selected C51.Next, which is1 for
the supported fresh INPUT context. Raw-object IDs remain0..N-1. CHAR, WORD,
and then TEXT registration consume this pending cursor in native order.
`SelectedInputContext` exposes the retained immutable input owner; its
historical registry Next stays1 because applying pending commands to a live
document is a later boundary. String-pool lifetime, selected279 ownership,
WORD48 rows, CHAR24 links and CHAR40 storage retain their previous owners.

## Exact native producer

Text05F050 builds the tuple used by the document caller. It stores actualC20
in tuple0, reads C20+40's C51.Next through the Ink registry service V20
(08E870: `mov rax,[rdx+18]; ret`), and stores that value in tuple+10. It then
allocates a32-byte wrapper, stores the same C20 at wrapper+8, and creates a
NEW pending C4D command owner through static V48 with NULL auxiliary owner.
This is separate from C20's existing default-observer history.

Ink07D9B0 creates that pending owner: kind7, command head/tail/count fields
at10/18/20 zero. Text05F200 calls05F050, reads DAF input rows and constructs
new2DA, then dispatches its V18=0283B0 with the same tuple. Native0210C0
creates its temporary C22 and increments tuple[2], including its failure
case, before appending the category100 CHAR/WORD/TEXT command. It does not
write the retained C51 counter during this prepared publication stage.

The new named `LegacyOwnedFreshTextSourceContext.PrepareSelectedInput`
transports this exact ownership. The internal shared-state constructor and
the named shared-WORD factory are the only consumer changes. No first
candidate, existing public wrapper signature, source ranges or Unicode/NN
logic is changed. The static audit checks4 PE regions,3 bindings and27 pins.

## Verification

The new probe runs3 actual configured shared spans and6 retained alternatives:
6 complete WORD48 rows,18 CHAR24 links,49 full CHAR40 rows,7 genuine CHAR-key
reuses and29 guards. TEXT registration follows the same real source-line
and NULL-source projection stages. An independent byte oracle replays C21
processing, primary AL/scalar emission, shared string identities, key reuse,
all scalar storage and selection IDs, starting from the derived INPUT Next.
It uses no expected text or ignored research imports.

Managed and fresh win-x64 NativeAOT captures match exactly:
`dac037d4a850b7e302e58d2885c49c39ab82c2035505292c23352d2f674ca8d6`.
The original shared WORD probe still matches its23582-byte capture SHA256
`bfcf07009804c831253ae2549cadf4c5ed419a70ee50c122f432e334a047dc2a`.
The old7-candidate wrapper capture remains
`3a3e0aacfe1565454b95edf33a400459f4963291bd68c8ae9eb23feb81af87f7`.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet build tools/LegacyMainSelectedInputWordChoices.Probe/LegacyMainSelectedInputWordChoices.Probe.csproj -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/main-selected-input-build -p:UseSharedCompilation=false
python tools/LegacyMainSelectedInputWordChoices.Probe/run.py E:/BlurMask-build-artifacts-20261014/main-selected-input-build/bin/LegacyMainSelectedInputWordChoices.Probe/release/LegacyMainSelectedInputWordChoices.Probe.dll E:/BlurMask-build-artifacts-20261014/main-selected-input-managed.json
dotnet publish tools/LegacyMainSelectedInputWordChoices.Probe/LegacyMainSelectedInputWordChoices.Probe.csproj -c Release -r win-x64 -p:PublishAot=true -p:UseSharedCompilation=false --artifacts-path E:/BlurMask-build-artifacts-20261014/main-selected-input-native-build -o E:/BlurMask-build-artifacts-20261014/main-selected-input-aot
python tools/LegacyMainSelectedInputWordChoices.Probe/run.py E:/BlurMask-build-artifacts-20261014/main-selected-input-aot/LegacyMainSelectedInputWordChoices.Probe.exe E:/BlurMask-build-artifacts-20261014/main-selected-input-native.json
python tools/handwriting/audit_selected_input_word_choices.py E:/BlurMask-build-artifacts-20261014/main-selected-input-static.json
```

This batch does not claim an actual2DA result owner or its108 transform-vector
reset/swap,037E80 result writes, imported SDK-page state, prior/cache reuse,
pending command application, or full0283B0/2E7 document acceptance. Those
joins must consume the retained context rather than equating raw-document
identity with C20/C51 ownership.
