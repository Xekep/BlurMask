# Native source count and result reference bridge

`LegacyPreparedResultReferencePolicy` restores three bounded metadata operations
needed by `LegacyPreparedResultScore`: available source-node count, result
reference-cost update and explicit source-kind multiplier dispatch. It does not
construct source graphs or select the terminal winner.

Native `2144f0` uses a supplied requested source count, or asks the source service
virtual88 when the requested value is-1. The inspected ordinary frontend
type222 implements that dispatch at `151fc0`, forwarding through its raw70
type21b object virtual20 to `1a73c0`. The last getter returns a positive cached
joined graph count at raw220. Otherwise it sums field0 of every source graph
record at raw210, using record count raw218 and stride88. The vectorized native
sum is an integer count; checked nonnegative, non-overflowing counts give the
same result regardless of grouping. A positive cache bypasses the source
records, including the zero/negative fallback cases of the cache itself.

Native `1a61c0` initializes the ordinary graph field0 from seed-group count70
produced by `19f850`, then builds its graph through `1a1ef0`, `1a4f10`,
`1a5700` and `1a5920`. Later joining can populate the positive cached count.
Consequently source-node span counts actual constructed graph positions,
including any inserted special nodes. It must not be inferred from the number
of recognized glyphs, raw pen strokes or prepared markers. The reusable count
method takes explicit graph-node counts; original source-order diagnostic
lattices are not automatically promoted to native source orchestration.

The ordinary result context begins with0 at reference field78. `2271f0` asks
`2257c0` for a terminal path winner. That selector adds the last cumulative
path cost (runtime2520[count-1]) to its selected relative beam cost. The result
controller skips a winner equal to Impossible (`4000000`) or to
`lastCumulativeCost + Impossible`. For the first valid output alternative only,
when configuration99 and configurationA8 are both0, it sign-extends the supplied
winner into field78. Later alternatives retain the reference. This reference
often makes the best alternative's prepared normalized difference0 and mass1;
it is not accumulated prior-text cost and does not create acceptance confidence.

The API implements the sentinel/flag update exactly but leaves the winner
explicit. `2257c0`'s whole selection policy contains knowledge flags, source
rules, optional penalty queries, multi-source beams and alternate filtering;
none are guessed here. Signed32 sentinel addition and summed count overflow
are rejected outside the checked ordinary scope.

At `2271f0` and backtrack `21ffd0`, explicit source kind0x135 (309 decimal) selects scalar5;
other source kinds select1. This is a dispatch on source metadata, not detection
of the user's language. `GetSourceScoreMultiplier` exposes that exact branch
without choosing a source kind.

Independent original controls cover1024 source-count/cache/reference cases,
first/later alternatives, both configuration flags, negative terminal costs,
both absent-path sentinels and explicit kind dispatch. Additional cases check
positive-cache record bypass, overflow/invalid ordinary counts, cancellation
and composition with the existing result scalar stage. Managed and Windows
Native AOT execute the same controls without loading vendor engine code.

```powershell
python tools/LegacyResultReference.Probe/reference_controls.py .artifacts/legacy-result-reference-controls.json
dotnet run --project tools/LegacyResultReference.Probe -c Release -- .artifacts/legacy-result-reference-controls.json
```

Still required for an automatic application adapter: complete native source
graph construction and terminal selection, configuration dispatch, result
posterior/acceptance policy and real Russian cursive accuracy evaluation.
