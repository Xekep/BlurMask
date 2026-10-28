# Ordinary later-word context recombination

`LegacyNativeLanguageContextRecombination` restores the ordinary Text203690
producer and connects it to full80-byte frontend118/140/F0F8 operations and
the native203640/203520 bounded16-byte context heap. It consumes real word-row
anchors and secondary cache state. It does not execute all204290 word-history
selection, category/case decisions, language-result insertion or final winner
publication.

## Anchor and previous-row ownership

The current stored88 row supplies anchor38/3C. The producer resolves that exact
ending node and ordinal, reads signed anchor span20, and computes the previous
ending node as `anchorEnd - span - 1`. Anchor1E identifies the original row in
that previous node. The required node-row getter receives the computed previous
ending node; a second caller-supplied collection cannot supply another node's
records. The selected previous-row collection is snapshotted before
external header/geometry callbacks; neither physical stroke indices nor a guessed
previous record substitute for these native ordinals.

The ordinary request is the aligned80-byte representation of native localA8.
Its current node is the anchor end, current graph arc comes from anchor1C,
current span from signed20, and current label/related flag come from24/07 when
anchor18 matches the ending node. Otherwise its actual suffix symbol context
selects the three native special-label replacements, with untouched residue
for the remaining branch. Current kind uses27 bits1..3, current count uses21
low6, and both category fields come from the actual sampler composite profile.
The current and previous source kinds come from their owning descriptor headers.

The previous request fields use that original previous row: descriptor18,
variant bits22..31 of its packed18 dword, graph arc1C, symbol24, mapped category,
and the referenced same-node row1E's07.bit0. Native39/3B and ushort3C are zero,
3A receives2309, and limit34 is04000000h. Unassigned80-byte residue survives.
Two explicit distinct portable cost-cell identities replace local stack-pointer
identities; they are not vendor ABI pointers and are never dereferenced.

## Full geometry and secondary scan

The original predecessor cost and increment enter request28/30. After the
actual118 and140 operations, accepted geometry gives three retained deltas:
current-row cost minus adjusted predecessor cost, current increment minus
adjusted predecessor increment, and adjusted-minus-original predecessor cost.
If original geometry fails, the first two stay04000000h and the third stays0.
Later arithmetic still follows literal signed32 wrapping and native comparisons;
failure does not silently replace the remaining native loop with an early exit.

Secondary rows are visited in ordinal order from230A to23F0. A row needs high
bit21 clear and the original predecessor's exact64-bit state28. Optional2601
count checks and the explicit secondary-flag requirement retain their actual
caller values. The row's graph arc updates request10 before F0/F8; the returned
successor list must contain the anchor's actual graph arc1C. Ordinary connection
operations bind these selectors to the existing source graph, preserving list
order and duplicates.

For the normal branch, the selected row replaces previous request fields and
uses the candidate cost cell, then actual118/140 run. Accepted finite candidates
store corrected total cost, increment and geometry adjustment. Their16-byte
record0C contains the previous secondary ordinal and0E isFFFF. The explicit
kind6 geometry-reuse branch performs the native fast calculation, retaining the
original request/cost-cell binding, and writes the current input ordinal at0E.
This fast branch is separately controlled; it is not a general fallback for
missing geometry.

`LegacyMainWordTransitionGeometry.CreateOperations()` supplies the real full
main callback contract. The narrower first23f transition contract is never
substituted. A complete native main source/frame producer remains a separate
requirement; the diagnostic probe states its guide and source-fact inputs.

## Native16-byte collector

`203520` rejects capacity0 and costs greater than03FFFFFFh. A full heap rejects
costs equal to or above its root. Replacement moves complete16-byte records
downward, selecting the right child on equal child costs. Append moves upward
only on a strictly larger cost. At the chosen hole, native203520 explicitly
zeros both qwords, writes cost00, and203640 assigns increment04, geometry08,
previous ordinal0C and current ordinal0E. The port preserves those exact moves
and does not use a generic collection sort or fabricate inactive vendor storage.

Anchor/node-owner/header/118/140/F0F8 callback cancellation is checked immediately. Row and
request residue snapshots prevent callbacks from changing already selected
input ownership. Limits, source headers and graph state remain explicit inputs;
no dictionary state or acceptance confidence is invented.

## Verification

The independent Python interpreter decodes the actual resource category profile
and runs512 full80-byte controller cases with3328 exact callback packets/cost
cells. Transition behavior is explicitly scripted in those controls, rather
than represented as an allegedly recovered geometry policy. They cover direct
anchor labels, the three suffix replacements and untouched nonreplacement
residue. Separate1024
collector streams verify30720 complete16-byte transitions, including negative
costs, overflow, zero/full capacities, sentinel rejection and equal-cost ties.
Twenty-five guards cover callback cancellation, extents, source/ordinal/category
ownership and cost-cell identities. Callback mutation checks preserve the
snapshotted current row, previous-node rows and request residue.

The actual resource consumer carries10 whole-M NN parent rows through the
following-boundary gate and profile2 language scoring into8 computed source50
cache rows. Those actual primary/secondary rows feed1FE740 over whole-I NN
costs and the real main geometry operations, producing10 native alternatives.
Their materialized anchors feed203690, which returns80 matched full80 contexts
and80 owned16-byte candidates. Previous cache costs, histories and restart tags
come from the actual language producer; expected text is never fed to scoring.
The input guide/frame and descriptor/line facts are explicit diagnostic snapshots,
and the full native graph-pruning/scheduler/winner lifecycle remains unfinished.

```powershell
python tools/LegacyMainLanguageContext.Probe/reference_controls.py .artifacts/main-language-context-controls.json
dotnet run --project tools/LegacyMainLanguageContext.Probe -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/main-language-context-managed-artifacts -- .artifacts/main-language-context-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

The same probe is checked in Windows NativeAOT. Vendor DLLs are inspected
statically and are not executed.
