# Published first alternative control import

`LegacyFreshMainGlobalControlImport.PreparePublished` accepts the actual
`LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot` after its owning
callback pipeline. A private immutable source union retains either this type
or the original first snapshot. It does not construct an old snapshot,
terminal, materialization, source graph or model.

The original `Prepare(oldSnapshot)` signature remains unchanged. `Source`
returns the exact old snapshot and explicitly refuses the published variant;
`PublishedSource` returns the exact published snapshot and refuses the old
variant. The internal `CommonSourcePolicy` carries the actual policy for future
same-owner metadata consumers without exposing a fake old source.

Both paths validate the real selection identity, source controller, execution
session, final result-query lease and current native WORD head/count. Prepare
copies every original glyph-pool node, including records not referenced by the
published WORD, plus complete120-byte WORD storage, source spans, pool indices
and unchanged flat reference order. Original WORD auxiliary vectors must be
empty. Later Run/projection calls compare the source pool and WORD bytes with
that snapshot; an external mutation or stale lease is refused.

`Run` uses the existing13BE80/13B350/139DC0 converter unchanged. Its destination
is a newly owned copied pool with a separate fresh dynamic2A control namespace.
Original primary-AL labels are interpreted only while importing. Converted IDs
must subsequently use `ImportedPool` control entries; they must not be read
through the acoustic AL. The original pool, WORDs and source references remain
unchanged. Alternate byte/offset counts are zero.13B5A0 reserves one spare byte,
but this API does not invent its uninitialized content or a storage NUL.

Import creation is single-use once entered, including a failed attempt.
Pre-entry cancellation leaves it reusable. Failed source validation occurs
before alternate storage or conversion; the consumed attempt cannot restart
even after a test restores the source bytes. Existing conversion failures and
cancellation preserve their copied-record/control-entry prefix. The new tests
exercise prewrite failures on actual owners; mid-conversion allocator and
cancellation prefixes remain the original prepared-kernel controls, rather
than invented inference events.

The new probe executes the natural short4-node final-publication route and a
separate friend-only4-node prepared node2 carried-prefix route. The latter is
explicitly not a natural native120-node gate. After the real quote adapter and
freshF4 no-op, four WORD allocator variants per source yield8 captured imports:
16 complete converted40-byte rows,8 private2A entries,8 copied WORDs and8 raw
imported WORD bytes. The independent raw-AK/interner oracle verifies every row,
control ID, capacity/special-slot/cache state, copied120 bytes, source span,
word index and reference. A separate pre-canonical byte oracle reads those
converted IDs from the same private2A. No expected transcript is read.

72 import guards cover null/canceled preparation, the strict old getter,
incomplete projection, repeated Run, cancellation-entry reuse, explicit
prepared source-record mutation/restoration and external head changes.
The8 canceled-entry instances also run successfully after the cancellation
guard; they are reuse controls, not additional captured inputs. Cached main NN
counts remain10 per positive source. The existing512 alternative snapshots,
quote/F4 checks and callback byte oracle remain in the capture. The actual
124-node no-winner source is refused before glyph/WORD allocation.

```text
LegacyPublishedControlImport.Probe AK engine separation case publication line30 line50 capture LK
python tools/LegacyPublishedControlImport.Probe/audit_output.py AK capture.json
python tools/handwriting/audit_published_control_import.py static-report.json
```

Managed and fresh Windows Native AOT captures match. Original
ControlAlphabetImport, ImportedWordBytes and ImportedGlyphCompletion probes
also retain their previous Managed/Native hashes and independent oracles.
Their711 prepared conversion cases,320 raw-byte/completion cases and actual
eight-owner paths remain separate coverage. The conversion, raw-byte and
per-glyph completion runtime files are unchanged; they consume the same
validated import object and private imported pool with either source variant.

The static audit composes actual published ownership/quote/F4 prerequisites
and pinned dynamic2A/import code from Text/Engine PE bytes. Vendor code is not
executed. Nonempty alternate/auxiliary owners, generic associated or reused
settings, full result transport, accepted text and handwriting accuracy remain
outside this bridge. This is not a complete engine or an app acceptance path.
