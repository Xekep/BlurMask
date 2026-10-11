# Ordinary source ingress connected to Russian cuts and final cache

`LegacyOrdinaryCursiveCutSource` connects the checked ordinary source ingress to
the existing Russian106 cut producer. `LegacyRussianCursiveWordRecognizer` has a
separate `RecognizeOrdinarySourceWithFinalCache` diagnostic endpoint using this
bridge. This is source-order bounded orchestration, not a complete source219
lifecycle or SDK-equivalent Russian cursive recognizer. Source frame, cache flags,
language scale, category selector, symbol mask and search policy remain explicit.

## Source identities and per-stroke service

The float input first passes through `LegacyOrdinarySourceInk.Prepare`. Native
`180167210` compares adjacent **raw** points and preserves each retained point's
raw ordinal. `1801680d0` computes the enabled enclosure service on that filtered
physical geometry. Source218 virtual60 at `180168c00` reads physical record+C,
with stride0x1c. The service returns a vector addressed by physical stroke, not
one scalar for a whole word. It is the cross-stroke contour/enclosure score
described in [the source enclosure report](legacy-russian-source-stroke-enclosure.md),
not a learned baseline confidence.

Russian cut controller `180191950` queries source virtual60 while visiting each
physical stroke. The call's stroke argument is visible in assembly at
`180191f22`; a value below0.5 participates in its baseline-cut branch. The bridge
passes the entire computed vector to the existing vector overload of
`LegacyCursiveCutProducer`, preserving stroke ordinals. It does not average or
replicate a scalar. Explicit disabled resource configuration produces zeros via
the source producer rather than a guessed caller default.

Native cut insertion `18018dde0` repeats the preceding retained source-point
map. `Source.Chronology.WithInsertedPreparedInk(Cuts.PreparedInk)` composes this
map with retained raw-point keys. Creating ordinary chronology from the filtered
point counts would silently replace raw ordinals with new contiguous numbers.
The new recognizer path uses the composed chronology for `1801b49a0` range
ordering before normalization and neural classification.

## Same service record consumed by final acoustic cache

In `1801bb020`, the closed-primitive branch requires one normalized stroke and
fetches the group's ranges. It requires the first range to start at0 and the
last range to end at the source physical count, then calls source virtual60 with
the first range's physical stroke index (`*local_f8`). This is separate from
ordinary dot knowledge rules and uses `-ln(value)*weight*8192` if positive.

`BindCacheProbability` supports the unambiguous single complete physical-range
case. It checks that the glyph addresses this exact prepared snapshot, retains
all supplied scope flags, and selects that physical record from the same vector
already consumed by cuts. A partial scope has no source probability; a caller
claiming whole scope for a partial range or multiple ranges is refused. The
multi-range native predicate is deliberately not expanded into an inferred
whole-stroke policy. The caller still decides all other flags, including whether
whole scope is appropriate and whether knowledge rules are active.

On `RecognizeOrdinarySourceWithFinalCache`, the older scalar
`options.SourceStrokeProbability` is unused and may be NaN: the source vector is
computed instead. Existing scalar diagnostic endpoints retain their original
explicit behavior. The new endpoint cleans source geometry once and uses it for
cuts, context windows and line normalization; it does not apply ingress twice.

## Verification

The vendor-free generator creates256 varied closed contours with enclosed/outside
strokes, source-order reversals and repeated raw points. It independently filters
raw-adjacent points and computes enclosure via the established shoelace/ray-crossing
Python oracle. Controls exercise256 nonzero physical source records and106
inserted cut points, checking raw identities at every prepared point, exact
source-record selection, unchanged scope flags, explicit disabled configuration,
foreign/partial/multi-range rejection, malformed indices and cancellation.

With real Russian acoustic/lexical resources, a separate headless diagnostic
passes28 variable groups through four neural families, final195-entry cache and
dictionary/lexical scoring. Tripling every raw point gives identical group counts,
word costs and ranks. The own printed MIR fixture returns `то`, `мне`, `мы`,
`мир`, `МЮ` as its first five candidates, with `мир` rank4. This demonstrates
transport and resource execution by the port; **it does not demonstrate accurate
Russian recognition**, an acceptance policy or UI readiness. The new whole-range
scope explicitly enables a different primitive branch from earlier all-partial
diagnostics, so their top candidates need not match.

```powershell
python tools/LegacyOrdinaryCutSource.Probe/reference_controls.py .artifacts/myscript-analysis/output/RussianAssetsSearch/legacy-ordinary-cut-source-controls.json
dotnet run --project tools/LegacyOrdinaryCutSource.Probe -c Release -- .artifacts/myscript-analysis/output/RussianAssetsSearch/legacy-ordinary-cut-source-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

Upstream group/baseline processing, ordinary source resampling, opaque/custom
source metadata, nonlinear/deferred seed arcs and native recognizer/controller
confidence remain separate stages. This endpoint supplies no invented probabilities
or automatic recognition threshold. No vendor executable or DLL is run.
