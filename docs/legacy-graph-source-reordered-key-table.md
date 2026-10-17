# Preserve original source-key table after physical reorder

`LegacyOrdinaryGraphSourceRanges.Prepare` previously enumerated current
physical strokes to build its private key table. Later normal-source merging
addresses that table using `LastStart.KeyRecordIndex + 1`, the retained
**original F8 metadata index**. A real15F050 physical reorder changes current
stroke order while keeping that original index. The old table therefore used
two different coordinate systems and could fail to merge adjacent original
source ranges.

The narrow fix places each genuine chronology key at its actual
`KeyRecordIndex`. It checks the raw metadata owner's bounded one-to-one
coverage; it neither infers the index from current order nor changes raw
point keys, endpoint residuals, graph ranges or sorting. Original-order
sources produce the same table and numeric output as before.

`LegacyGraphSourceReordered.Probe` reproduces the failure through actual
pinned source247 and the two embedded2E3 models: two raw M-shaped strokes
are submitted right then left, `LegacyFreshMainPhysicalGrouping` and
`LegacyFreshMainPhysicalReordering` genuinely select order `[1,0]`, and
`PrepareOwnedCurrentSource` creates the real current-source metadata owner.
An explicitly prepared two-whole-physical ordinary graph then queries the
source-reference producer. This graph construction tests source metadata;
it is not a claimed full main sampler/winner or expected text fixture.

Before the fix, the committed code returns two intervals:
`(0,0,0)..(0,0,4)` and `(0,1,0)..(0,1,4)`. The independent original raw/F8
table proves they are adjacent, so native normal merging returns one
`(0,0,0)..(0,1,4)` interval. The new regression was run and failed against
the old committed implementation before the runtime changed. Its diagnostic
capture retains both current enumeration and the actual index-addressed
original table. The corrected Managed and NativeAOT probes pass, and a
separate Python check derives that table directly from the explicit original
raw inputs. No handwritten transcription or label correction is involved.

The earlier576 source-map controls plus18 actual RU frame/cut/graph cases
and their independent residual-byte oracle are rerun. Their original source
owner was not physically reordered, so those controls did not prove this
particular actual current-view case. Their captures remain byte-identical.
The original key-only source oracle also remains unchanged.

```
dotnet run --project tools/LegacyGraphSourceReordered.Probe -- AK.res line-2e3-30.res line-2e3-50.res OUTPUT.json
python tools/LegacyGraphSourceReordered.Probe/audit_output.py OUTPUT.json
python tools/handwriting/audit_reordered_source_key_table.py STATIC.json
```

The static audit pins20 exact instructions of source218.V98=168DC0: next
record addressing subtracts the originalF8 base, divides by12, advances that
index and addresses F8 again. It does not consult current physical order.

This corrects the shared source metadata table. Full V140 higher/protocol
dispatch, lazy217670 and final text wrapper still require their owning
stages. No vendor binary or desktop GUI is executed.
