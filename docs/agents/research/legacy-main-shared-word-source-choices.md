# Shared WORD source choices before document acceptance

`LegacyFreshMainWordSourceChoices.Prepare(AK, completedConfiguredSources,
table, publication, normalization, circles).Publish(recordResidue40,
selectionAllocation4)` implements the fresh new-WORD branch of `0283B0`
over the actual complete `275` WORD payload array. The supported owner is
the completed configured two-alternative result with one shared SPAN and
ordinary positive base source references. Unsupported higher/protocol keys,
unowned WORD auxiliary vectors, other cardinalities and pending source
completion are refused; their rows are not omitted.

The input is not two previously rendered candidate-local wrapper results.
`CaptureSpan` supplies the real120-byte records in native ordinal order.
`LegacyOwnedMainSpanWordView` retains each record and its flat references,
without constructing a fictitious `279` node. Every record runs the existing
per-WORD source refresh, selection, source clipping and `024F30` CHAR path.
An internal seam in `LegacyFreshMainTextGlyphChoices` exposes that first pass
without invoking `213A0`. The existing `024F30` renderer then chooses new
`213A0` publication or `021760` reuse with one shared CHAR tree, UTF8 pool and
selection context. Existing public choice/wrapper APIs remain unchanged.

`028FE1..029039` iterates all WORDs with source stride120 and output stride48.
The fresh output48-byte rows are zero initialized, retaining flags at08 and
the CHAR-vector count/capacity at18/1C. Each24-byte CHAR reference has its
selected marker at10 and the actual CHAR source payload as a typed pointer
sidecar at08. Native addresses are never invented. WORD120 scores are not
copied into those zero-initialized48-byte rows.

The WORD source key and its accumulated ordinary CHAR ranges are distinct.
`0290BF` takes `sourcePayload+20` (`sourceNode+38`) as the kind1 `0210C0`
selection ranges. `024F30` accumulates ordinary ranges into
`sourcePayload+30` (`sourceNode+48`). The producer preserves both and
registers the pending WORD command using the original source key. CHAR
commands consume IDs first, followed by WORD; those IDs belong to this
library's fresh raw document context, not an SDK page. No external document
is modified.

The second genuine TOP alternative selecting that same SPAN reuses the WORD
source and its already rendered choice vector. Its actual `279.WordIndex`
selects the48-byte WORD choice. The returned32-byte TOP-choice scalar row
retains that selected row's low10 flag bits and one24-byte WORD reference.
The WORD reference retains the real selected index and the shared WORD source
as an owned sidecar. The different-SPAN/same-key remapping branch is excluded.

The stage owns its shared UTF8 pool. Renderers borrow it; disposing the stage
disposes that pool once, so consumers must retain the stage while reading its
text references. Publication is single-use after entry. Shape/cancellation
checks before entry allow a valid later call; an entered allocation refusal
retains a failed prefix and prevents retry. There is no rollback claim.

The actual probe runs three independent raw inputs through configured
recognition and completed source processing. Their two retained alternatives
produce six real WORD48 rows,18 CHAR24 links and seven genuine CHAR-key
reuses across the WORD array. Its oracle independently replays the raw AL
labels, UTF8 classification, C21 operations, ordered new/reuse string and
source identities,40-byte CHAR records,48/24/32-byte scalar records, selection
IDs and the pending CHAR/WORD commands. Expected text is never an input.
The explicit zero-length selection allocation also verifies entered-failure
poisoning before the first CHAR registration. Static PE verification pins
the native loop, fresh initialization, original-key WORD registration,
same-SPAN reuse and selected-index transport.

This ends at pending CHAR/WORD registration. It does not implement subsequent
`21AB0` source-line publication, TEXT selection/commits, document postprocessors, prior
or cached document reuse, accepted text, layout or automatic font metrics.
It must not be described as complete `0283B0` document acceptance.

Reproduction (all paths may be changed to another writable artifact root):

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet build tools/LegacyMainWordSourceChoices.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/shared-word-choices-build
python tools/LegacyMainWordSourceChoices.Probe/run.py E:/BlurMask-build-artifacts-20261014/shared-word-choices-build/bin/LegacyMainWordSourceChoices.Probe/release/LegacyMainWordSourceChoices.Probe.dll E:/BlurMask-build-artifacts-20261014/shared-word-choices-managed.json
dotnet publish tools/LegacyMainWordSourceChoices.Probe -c Release -r win-x64 -p:PublishAot=true --artifacts-path E:/BlurMask-build-artifacts-20261014/shared-word-choices-native-build -o E:/BlurMask-build-artifacts-20261014/shared-word-choices-aot
python tools/LegacyMainWordSourceChoices.Probe/run.py E:/BlurMask-build-artifacts-20261014/shared-word-choices-aot/LegacyMainWordSourceChoices.Probe.exe E:/BlurMask-build-artifacts-20261014/shared-word-choices-native.json
python tools/handwriting/audit_main_word_source_choices.py E:/BlurMask-build-artifacts-20261014/shared-word-choices-static.json
```

The last auditor requires the private pinned PE for read-only disassembly;
the runtime, public probe and byte oracle do not load or execute vendor code.
