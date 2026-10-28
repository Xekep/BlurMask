# Actual ordinary main glyph refresh

`LegacyOwnedMainOrdinaryGlyphRefresh` joins the nonnegative ordinary CASE part
of `217670` to a real completed raw main result. It owns the exact compact pool,
node starts, result glyph pool, residual-source metadata and shared acoustic
scorer. Repeated `Prepare` calls retain the same per-result refresh lifecycle.
It does not enable the new recognizer in the app or produce accepted text.

For an existing canonical owner, the callback exports its actual source records
and endpoint residual bytes, reads the original acoustic group's first-candidate
score, then scans the base node's complete compact interval in stored order.
Only matching full source keys with compact bit20 and subtype0 participate.
The target higher-source key and its original base key remain distinct.

Descriptor allocation/dedup precedes actual `216860` scalar resolution. The
emitted symbol updates the pool; normalized-original selects scoring. Field10
is written before cost calculation, followed by field14, compact byte21/255 in
field0C and descriptor bit8 from compact bit40. The ordinary backtrack producer's
unit-score shortcut is deliberately absent from this separate native caller.
Allocated row residue remains explicit and unrelated bytes survive.

`Select` joins a completed refresh to the checked `13A830` alternative ordering
and marker update. Selection allocation refusal retains the refreshed owner,
so a later selection can use a sufficient allocation without refreshing again.
Native pointer fields and AVL shape are represented by owned portable state.

Unsupported source branches or cancellation retain prior writes. This portable
adapter refuses to retry a failed refresh epoch. That is an explicit boundary:
native `13C6E0` sets bit4000 even after callback failure and may subsequently
rank a partial owner. That retry behavior, negative/span source callbacks,
final wrapper filtering/point publication and text acceptance are not completed
by this API. Caller ordering relative to quote cleanup is also separate.

## Verification

Five original raw trajectories run through actual first/main inference and
publication. Their 19 existing ordinary glyph owners refresh from 38 real
compact records and gain 38 additional pool entries. No expected text enters
the probe. These cases do not demonstrate selected-path higher-source events.

Five explicitly prepared controls run after inference: real source-overlay
merges provide higher membership, and one matching compact descriptor seeds
each diagnostic pool owner. These cover higher/base identity separation and
updating an existing descriptor. Five further diagnostic span-owner controls
verify refusal and failed-epoch reuse rejection. They are not NN evidence.

Together the probe covers 24 refreshes, 48 compact rows, 43 added entries,
116 guards and 24 insufficient-selection allocations. Managed and Native AOT
captures match byte for byte. The independent composition audit reconstructs
all 2,680 final pool bytes, descriptor dedup, finite alternative order and marker.
It consumes the separately audited scoring-stage outputs; it does not claim
an independent neural-network implementation or handwriting accuracy.

Run `LegacyMainOrdinaryGlyphRefresh.Probe` with AK, LK, engine/separation profiles,
case/publication data, line30, line50 and the capture path. Run its
`audit_output.py CAPTURE.json`. The private static audit
`tools/handwriting/audit_main_ordinary_glyph_refresh.py REPORT.json` checks the
pinned vendor PE and exact callback/store order without executing vendor code.
