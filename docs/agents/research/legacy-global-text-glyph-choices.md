# Live global UTF8 glyph choices

`LegacyFreshMainGlobalTextGlyphChoices.Prepare(scope, completion, ak, properties,
normalization, circles)` retains the exact pre-NN global scope, completed V68,
Document, global TOP, primary acoustic AL and shared3E. No old source owner is
coerced into this API and no new string pool or neural invocation is created.
It is a bounded glyph-alternative publication stage, not document acceptance.

`Publish(allocator40, selection4)` is single-use after entry, including failure.
All actual WORD references are copied in their existing order. Negative/span
references are refused before mutation. The first loop refreshes every ordinary
owner and projects its actual V140 source metadata. Duplicate owner keys reuse
the cached same refresh and position object. The second loop performs native
13A830 selection and the existing0213A0/021000 UTF8 publication. Original WORD
references and acoustic evaluation counts must remain unchanged.

One projection context per choices owner builds and validates the exact immutable
Document's selected-input map and original root interval once. Every glyph
retains that same context; only its V140 references and projected ranges vary.
This avoids copying/validating all input rows separately for each glyph.
The source projection uses that shared selected-input map,
actual original source IDs, point ordinals and residual bytes. Existing verified
05AFA0 projection preserves ordered ranges and endpoint modes. It does not use
sampled ordinals or approximate bounding boxes. Fresh group0 ordinary input and
one original closed collection interval are supported; C25/prior objects,
external groups, insertion/protocol spans and multiple original intervals refuse.

The [static plan](legacy-global-text-glyph-choices-plan.md) establishes signed
reference bits40..55, successful selection-marker writeback and empty-alternative
removal. An unavailable resulting marker returns no selected bytes; there is
no extra choice policy. The actual HeaderE filter is wrapper-third-argument
byte8: fresh0283B0 zeroes it, and only a prior-result property12D0065 branch can
override it. Knowledge99 is not its direct producer. The scoped fresh original
bootstrap/selected-input API has no such prior-result input, so filter=false.

Choices share the pre-NN3E. Full scope closure closes the context and releases
its final publication lease, disposing that pool. Both the convenience selected
text accessor and direct `Published.Records[i].Text.ReadBytes()` then throw;
position access also refuses. Reading copied scalar metadata does not require
the pool. A choices owner does not dispose its borrowed scope or pool.
Repeated `Prepare` for one completion returns the same facade and requires the
same scope and Unicode-data identities. Entered exceptions retain pool/string
prefixes and poison this publication; no retry or automatic rollback is invented.
Pre-entry malformed storage/cancellation remains reusable.

## Verification

The probe accepts `MODELS CAPTURE [RECORDED-MANIFEST]`. Four procedural inputs
plus nine stored genuine Russian coordinate tracks produce13 successful
publications,37 glyph references,80 classified UTF8 alternatives,38 exact packed
original ranges and71 selected bytes. No stored word label enters inference.
The input loader checks the pinned corpus commit and track hashes, and does not
invent missing pen-up metadata.

The portable Python auditor independently reads raw AK labels and committed
Unicode tables, reconstructs alternative identity/order and original marker,
checks UTF8 classification and shared intern identities, and recomputes packed
source positions from actual24-byte references and2-byte residuals. It checks
all initial owner records survive the compact refresh.93 ordinary lifecycle
guards,13 direct published-string lifetime checks and two foreign invocation/V140 controls sharing the **same Document**
verify owner identity, storage, cancellation, single-use and lifetime. A genuine
insufficient-selection run retains both appended compact records, no new string,
unchanged WORD/NN, and a poisoned non-retryable owner.

Fresh Managed and Native AOT captures are byte-identical. Static analysis reads
vendor bytes only. This batch adds seven new files and modifies zero old files.
The shared-map optimization preserves every prior captured glyph/range/string
byte; the only capture addition counts the direct string lifetime checks.

Post-inference Russian quality remains **4/9 exact words,16 edits/33 expected
characters**. This ownership/publication join does not improve that baseline.
It does not establish English quality, calibration, CHAR/WORD/TEXT source-node
registration, document transport/acceptance or the application recognizer switch.

```powershell
dotnet run --project tools/LegacyGlobalTextGlyphChoices.Probe -c Release -- MODELS CAPTURE RECORDED-MANIFEST
python -I tools/LegacyGlobalTextGlyphChoices.Probe/audit_output.py MODELS CAPTURE
python tools/handwriting/audit_global_text_glyph_choices_runtime.py REPORT.json
```
