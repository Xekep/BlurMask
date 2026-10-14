# Later dictionary-word terminal and history consumer

This batch connects the ordinary Text204290 context16 consumer to owned prior
word history, the checked100090 language-record producer, source50 restart and
1f9080 cache insertion. The current source is the selected C00h dictionary,
with mode22F6=false, frontend280=false, nontracing and knowledge.EC<=0. It uses
the actual selected bundle's dictionary/index/model/resource-selected profiles;
same counts or copied coefficients do not establish that ownership.

It does not implement every auxiliary source query, source refresh, case mode,
heap scheduling, tracing, positive-EC sharing or native final winner. Russian
AI-pero production acceptance still needs that complete owned orchestration and
quality validation.

## Context and node ownership

`LegacyNativeLaterDictionaryWordCompletion` takes the owned203690 result and an
index into its16-byte candidates. It resolves the current node/ordinal through
a required node-row getter, then its anchor38/3C and previous ending node
`anchorEnd - signed(anchor20) - 1`. The result's request00/08/0C/20 must match
that anchor. The selected previous ordinal0C is resolved from that previous
node; its state28 must match the original predecessor and high bit21 must be
clear. A finite context cost is required. The fast branch's current ordinal0E
must beFFFF or the actual current row ordinal.

Current, anchor and previous node rows are copied before source-header/line
callbacks. The current-terminal query comes from the already checked local318
injection, retaining its real source tag and label. Unassigned query residue
24..31 is explicit and the regular no-extra-context branch requires it to be0.
The source70 terminal ordinal is read from the same selected dictionary index.

The required frontendE8 getter resolves group->input descriptor and reads its
**line ID at hex10**. This is not physical boundary18 or a physical stroke
ordinal. Current anchor category2/4 gets the native8192 correction at start0
or a nonzero descriptor-line difference. The existing bounded source-line
projection performs this calculation; the wrapper checks cancellation after
every getter, including a returned unknown line-1.

## Actual history selection and F5

`LegacyNativeWordHistoryPreparation` restores the selection around205A38..C5D:

- Matching previous/current source IDs and signed lineDifference<=1 select
  the previous stored payload40..50. A difference greater than1 selects fresh
  history; a negative difference is not replaced by its absolute value.
- For the selected source22EC, the current anchor's actual suffix context is
  walked to its oldest symbol. FFFB,FFF9,FFF5 or the enabled22F8 rangeFFEB..FFF3
  bypass prior history. Static205A59..A7E dereferences a nonnull context first;
  the port rejects a missing context instead of inventing a fallback symbol.
- Other-source, line-separated or marker-bypassed cases use the literal zeroed
  localF8 request: mode00=0, history count04=0. This is distinct from the
  first-word mode1/default64 branch. No default or prior class is guessed.
- The observed order2 payload count at stored40 permits zero/one class slot.
  Larger same-source counts are refused. Unused foreign/fresh payloads are not
  parsed or silently truncated.

Request24 is significant: with reused history,205B52..BB7 replaces source70
token byte0C with **previous row05.bit2**. Source token byte0D/tag42 remains
the separate category-suppression flag. The complete16-byte token otherwise
retains its source70 fields. Previous class/category history comes from the
actual stored44 uint, not the previous word ID or an arbitrary default class.

Knowledge.F5 only considers prior source kind1400h or prior label24C0. Text
20C1C0 installs24C0 through018F70 from literal1802E9B80, `'-'`, length1. It
then resolves that prior word's own anchor38/3C, derives its source start, and
clears the first history category at start0 or an E8 descriptor-line change.
An unknown prior line-1 does not prove a change. The helper keeps this clear
flag distinct from source42 suppression and the request24 boundary flag.

## Numeric score, budget and cache lineage

The current word category comes from the actual sampler composite mapper.
Path cost is context00 plus query cost10 and the source-line correction. The
reused-history budget comes from `router.GetPrimaryHeap(selectedSourceId)`,
the same owned bucket later used by insertion. Fallback localF8 retains literal
request28=04000000h, assigned once at204546; its language result can still be
accepted while the full heap rejects the resulting total. There is no duplicate
caller heap or unrelated capacity. The checked scalar100090 scorer retains its explicit finite int32
domain; overflow is refused rather than advertised as a safe wrapped query.

Accepted language results provide the updated20-byte history and stateFFFF
to1f9080. Increment14 comes from context04. Current row ordinal, inherited
anchor/source metadata, updated history and computed source50 restart retain
their actual owners. Incoming source IDs are cleared for the child restart
getter and stamped only when its returned header05.bit2 requests it; query
kind is installed in header05.bits4..6 without truncating other tag bits.

The contextual5C pair differs from the first-word literal. Native204290 assigns:

```text
slot5C = uint32(context08) << 32
       | uint16(context0C) << 16
       | uint16(context0E)
```

Thus the upper dword is the signed geometry adjustment's raw bits, the middle
ushort is the selected prior secondary ordinal, and the lower ushort is its
current-ordinal provenance. Slot1E still references the current parent ordinal;
it is not overwritten with that prior word ordinal. This distinction is checked
against complete128-byte record movement.

Calls are serialized. Cancellation/exception invalidates producer reuse.
Context packets and query/lexical residue are copied before callbacks; chosen
node-row buffers are snapshotted. Callback cancellation is checked immediately.
Allocator equality remains separate from language-budget equality: an equal
budget can accept the language result while a full heap rejects its equal root.

## Independent and real-resource checks

The original Python oracle checks1024 history/token/source/F5/line/oldest-marker
cases with1463 exact callback-order checks. It covers request24 override,
source42 preservation, negative/positive line differences, prior kind1400h,
dash starts, unknown lines and the inclusive extended-marker endpoints.

An independent packed-bit/BFS-child/preorder reader enumerates the real lexical
terminals and reads its actual word/class/unigram/bigram codebooks. Binary32
calibration and punctuation tables generate256 formatter controls over caller
owned synthetic rows and real terminal IDs. These compare numeric fields,
all40 language bytes and all128 cache bytes, including allocator tail residue,
updated history, restart/5C lineage and strict/equal/tighter live budgets. These
formatter rows are not presented as raw recognition-quality samples.

Thirty consumer guards and thirteen history guards cover model/profile/source
identity, context/node/ordinal matching, malformed history, missing suffixes,
callback cancellation, invalidated reuse and input snapshots.

The real drawing consumer carries10 whole-M NN rows to8 actual profile2/source50
cache rows, then10 native whole-I alternatives and80 full80 geometry contexts.
Seventy-two terminal/history results use8 distinct real prior classes through
100090 and1f9080. It verifies slot5C, current parent ordinals, increments and
all20 updated payload bytes. Prior history is generated by the real first-word
language path, rather than fabricated lexical cache costs. The acoustic guide
and multi-physical descriptor facts remain explicit diagnostic snapshots; a
selected final word and full native main source lifecycle are not claimed.

```powershell
python tools/LegacyLaterWordCompletion.Probe/reference_controls.py .artifacts/later-word-completion-controls.json
dotnet run --project tools/LegacyLaterWordCompletion.Probe -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/later-word-completion-managed-artifacts -- .artifacts/later-word-completion-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

Managed and Windows NativeAOT use the same controls. Vendor DLLs are inspected
statically and are not executed.
