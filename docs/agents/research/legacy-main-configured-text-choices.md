# Configured selected WORD to text-glyph choices

`LegacyFreshMainTextGlyphChoices.PrepareConfigured` now accepts an actual
`LegacyOwnedMainCandidateSource` of a retained configured alternative. It
reuses the existing numeric publication kernel while preserving its genuine
2410 terminal/session, shared TOP/glyph pool, candidate-local source overlay
and original raw document. It does not manufacture a count1 first candidate.
The old `Candidate` getter explicitly refuses this new variant.

## Producer/consumer order

The actual selected279 WORD list is captured in native head order. For every
WORD, all references are refreshed before any ordered alternative selection.
Ordinary217670 refresh, negative-span217670 refresh and document positions
use their named configured owners. Selection still uses the actual pool
records and the existing13A830 logic, including a real allocation refusal.

The shared kernel then calls TOP273.V50 through the actual emitter, followed
by0213A0 empty removal/selected-marker adjustment and021000 classification and
string interning. HeaderE filtering stays false from the wrapper's zeroed
local138.byte8 and absent prior-result property12D0065 branch. Knowledge99
is separate; setting the supported main short18 property does not create a
prior result or change the empty control/alternate source vectors. The exact
filter producer is pinned in [the global choice plan](legacy-global-text-glyph-choices-plan.md).

Every choices object owns one fresh local UTF8 string pool. It does not claim
that independently preparing two choice objects recreates a complete native
wrapper's shared string-pool lifecycle. Ordinary configured higher-source
refresh remains explicitly outside the existing supported scope; negative
insertion sources retain their distinct mode4 projection.

Pure storage/cancellation checks precede the publication claim. A real failed
selection retains the started prefix and cannot be retried through the same
object. Disposing the choices object invalidates its interned string handles.

## Evidence

The probe runs three actual configured raw inputs (one duplicated-point M
trajectory and two six-physical-stroke inputs), with both retained alternatives
of each input. It reads no expected text or recorded-corpus transcriptions.

Each Managed/Windows Native AOT run produces18 actual WORD glyphs,49 classified
alternatives and35 selected raw UTF8 bytes.42 guards cover malformed storage,
entry cancellation followed by successful reuse, historical getter refusal,
repeated publication, a genuine selection-allocation failure with retained
failed prefix, and disposed string lifetime. These are six selected candidates
from three inferences, not six distinct recognition inputs.

The independent Python oracle checks finite-score selection/dedup order and
marker identity, raw AK UTF8 labels, Unicode classification flags, per-object
string interning, retained selection rows and source positions against the
original caller point counts. Captured prewrapper WORD strings are diagnostic;
they never drive inference, row selection or an alias correction. Managed and
Native outputs match SHA256
`2bdb523a3fea5a8bb78efc62bdc95fc1ae50b399d9fc21ce9f1ff8650951abf0`.

The unchanged historical text-choice probe also passes Managed/Native and its
independent auditor: seven actual candidates, 26 glyphs, 64 alternatives, 52 selected
bytes, 35 guards, 14 failed-prefix checks, 12 foreign-source checks, seven genuine
negative pool owners and 592 prepared position cases. Its complete output is
unchanged SHA256
`eb2c197d59781b75805cdcf029a84baf069f97fed0012978880bc219b730ebfc`.

The new static script composes the pinned configured-source/emitter and fresh
0213A0 contracts, validates all 716 native script table rows and 120 literal trie
rows directly against the PE, and checks managed owner/stage-order pins. No
vendor binary is executed and no private Unicode download is required.

## Reproduce

```powershell
$env:TEMP = 'E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP = $env:TEMP
dotnet run --project tools/LegacyMainConfiguredTextChoices.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/configured-choices-managed -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json models/text-unicode/legacy-case-data.json models/text-unicode/legacy-text-publication-data.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res models/text-unicode/legacy-normalization-data.json models/text-unicode/legacy-circled-payload-data.json E:/BlurMask-build-artifacts-20261014/configured-choices.json
python tools/LegacyMainConfiguredTextChoices.Probe/audit_output.py E:/BlurMask-build-artifacts-20261014/configured-choices.json
python tools/handwriting/audit_main_configured_text_choices.py E:/BlurMask-build-artifacts-20261014/configured-choices-static.json
```

Native AOT uses the same 11 resource/output arguments. The probe is self-contained
and reads no private recorded corpus.

This is an owning selected-WORD-to-choice bridge. Configured219580 word-source
collection, complete024F30 source-node interning/reuse, final0283B0 orchestration,
acceptance thresholds and application auto-replacement remain separate joins.
No TextSourceWrapper, WordSourceCollection, ProcessedTextSources or UI file is
widened by this batch.
