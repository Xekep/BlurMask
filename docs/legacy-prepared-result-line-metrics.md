# Prepared source-line owner to SDK font metric

`LegacyPreparedResultLineMetrics` ports the proven source-line binding phase of
Text **050660**, then Document **177590**'s SDK x-height transport. It requires
actual source-line x-heights and already derived character source-line IDs.
It owns snapshots and uses source indices rather than inventing native pointer
addresses or successful decoder output.

## Concrete owner chain

Text class **2DB** has runtime table **276740**, whose **+18** entry is
**050660**. Its constructor/source reader **053060** initializes its character,
word and line arrays. `050660` loads actual driver **2DA** rows and recognized
characters; its line binding phase is gated on result character count **+40**
being nonzero.

The function allocates result **+58**, count **+60**, with **0x30 = 48-byte**
line records. For source line index `i`, it writes record **+0** to
`source2DA.F8 + i*0x38`, the real **56-byte** source-line record. It writes
record **+8/+C** as the exclusive character start/end range. Thus the line
object subsequently read by Document is the source2DA record itself, rather
than a classifier frame or a newly guessed glyph-box height.

Document **177590** obtains the cached field's result via **15c5f0** at field
state **+38**, indexes result **+58** by stride **30**, follows its source
record pointer **+0**, and narrows **source-line double+20** into SDK
`lineInfo.float+18`. Existing interface **CA8+78 = 0b6810** and iink **4c2660**
then carry this height into the font matching payload. This closes the actual
source2DA-F8 -> result2DB -> Document SDK owner boundary.

Source **+20** is the native source-line x-height already produced by C1A or
explicit existing line geometry; it is not raw24A/source219 frame height.
Document **0531b0/052e90** writes C1A's float x-height promoted to double at
this position. Source metric updates such as **060820** can mutate the native
referenced record later; this library instead takes a clearly owned snapshot
at the selected preparation time, not a live native pointer alias.

Private static exports:
`D:/BlurMask-build-artifacts-20261011/TextResultLineOwnerNext/180050660.c`
and `DocumentSourceItemsBindingNext/MyScriptDocument.dll/focused.c`.
Inspected Text SHA-256:
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`;
Document SHA-256:
`17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f`.
Only static inspection and original portable code run during this work.

## Exact range behavior

Before line binding, native performs a **single forward pass** over character
source-line IDs. If current ID is smaller than the immediately preceding ID,
only that preceding slot is lowered. For example `5,4,3` becomes `4,3,3`.
Global sorting or a fully monotonic repair would change native behavior and is
not implemented.

The result cursor starts at zero. For each source line index, its start is the
current cursor; the cursor advances only while the current character's ID
equals that source index. Empty lines retain an empty range, and backward or
out-of-source IDs can leave later characters unconsumed. The owned port keeps
these observed semantics, without inventing assignments for orphan characters.
Negative IDs must already have undergone native automatic-ID derivation; this
API rejects them rather than guessing that derivation.

The explicit budgets are4096 source lines and65536 characters. It validates
finite source double heights, supports cancellation and checks float narrowing
separately at SDK read time. `CreateFontSizeSample` combines that actual source
metric with explicit recognized-range code point/count/width/flags. It does not
fabricate any of those inputs. Negative finite heights are retained through the
value transport; the prepared font policy applies its own validation.

## Recognition source-item upstream boundary

The actual item/string producer is now traced further:
Document **0540d0** calls **0526c0**, which schedules **027590**. This callback
invokes recognition interface **EED.virtual20** for each prepared source block.
After recognition, **04fab0** clones each recognized block's **+98/count+A0**
40-byte symbol items via **043740**, preserving source identifier vectors,
UTF8 candidates and flags. This produces the source vector passed as argument6
to **053c60 -> 0531b0**. **04ffa0** can contextually rewrite its strings when
driver mode **+24** equals1 and the alphabet has the corresponding feature.

The concrete EED owner/virtual20 implementation, recognized block producer and
native final word winner -> source-group boundaries remain unfinished. This
trace does not authorize replacing unknown connected handwriting with expected
text labels or a whole-word bbox. Application live `FontSizeSamples` must wait
for those actual recognized-source inputs.

## Verification

The independent Python oracle creates **6825** exhaustive short source-ID and
line-count cases. It uses simultaneous neighbor minima and reachable prefix
counting rather than native's mutation/cursor loops. Managed and NativeAOT
verify all repaired IDs, source indices, character ranges and SDK float heights.

Another **256** cases load the committed portable category model and compose
mixed Latin/Russian body/uppercase/descender prepared geometry through category
production, C1A IRLS, actual float->source double storage, result owner binding,
SDK float narrowing and font policy. Other tests cover owned snapshots, empty
character gating, nonmonotonic ranges, missing lines, malformed IDs, explicit
budgets, double-to-float overflow and cancellation. These are prepared pipeline
tests, not raw handwriting or native winner accuracy claims.

```powershell
python tools/LegacyResultLineMetrics.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/result-line-metric-controls.json
dotnet run --project tools/LegacyResultLineMetrics.Probe -c Release -- D:/BlurMask-build-artifacts-20261011/result-line-metric-controls.json models/line-geometry/character-line-categories.json
dotnet publish tools/LegacyResultLineMetrics.Probe -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/result-line-metric-aot
D:/BlurMask-build-artifacts-20261011/result-line-metric-aot/LegacyResultLineMetrics.Probe.exe D:/BlurMask-build-artifacts-20261011/result-line-metric-controls.json models/line-geometry/character-line-categories.json
```
