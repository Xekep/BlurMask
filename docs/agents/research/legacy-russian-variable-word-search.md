# Variable-cut Russian word endpoint

`LegacyVariableWordSearch` connects the checked ordinary `LegacyCursiveCandidateLattice`
to the packed dictionary without first choosing a fixed partition. Every returned
letter retains its candidate group and ranges addressing prepared ink, including
inserted cut points. A dead intermediate cut does not stop longer candidates that
span that cut. Dictionary terminals are rescored before the final beam is pruned.

The bounded beam is an original search, not a port of the large native text beam.
Its prefix state is `(covered atom count, dictionary state)`; it retains the lowest
prefix-independent ink cost for that state. It does not model native prefix-dependent
category transitions, alternative graph seed ordering, special-token arcs, skipped
arcs, or cross-source joins. Terminal callbacks must preserve ordering by ink cost
for the same dictionary state. Candidate arcs are scored individually, even when
they share a geometry ID: their arc/category corrections may differ.

Costs in this explicit search domain may be negative, including native synthesized
class costs. They are bounded to `[-0x4000000, 0x4000000]`; the positive endpoint is
impossible. Prefixes outside the signed domain are excluded. Beam width is 1..256,
choices per candidate at most 512. Cancellation and a caller-selected expansion
budget bound work; exceeding that budget throws rather than returning an apparently
complete result.

`LegacyRussianCursiveWordRecognizer.RecognizeSourceOrderDiagnostic` is a complete
resource-backed diagnostic producer: Russian106 geometry cuts, inserted-point
chronology, variable candidate groups (stored RU span cap 8), FF/FF normalized
glyphs, context30/28, four neural families, dynamic seven-slot weights, integer
costs, dictionary search, terminal word identities, and ordinary lexical totals.
It requires a supplied baseline, line height and source-service probability. The
latter is opaque `source218.virtual60` data, also used elsewhere for primitive
confidence; it is not a demonstrated baseline confidence estimate.

`RecognizeSourceOrderDiagnostic` uses raw 180 neural class costs. These are **not** the native
195-entry final class cache (192 alphabet slots plus three synthesized slots).
It omits special-label synthesis, pair filling, pruning, corrections and final
candidate geometry costs. `RecognizeSourceOrderWithFinalCache` runs the complete
ordinary final-cache path using `LegacyCursiveFinalCacheGroupScorer`: raw180 to
final195, primitive synthesis and explicit native pruning/pairs, sampler separation
cost shared by35 categories, native acoustic candidate scoring, variable dictionary
search, and native whole-word lexical totals. Caller cache source scopes/masks,
native pair list, start/end composite-map selector and shortlist selection are
explicit. No additional recognizer or active native confidence-prune hook is
synthesized. Costs used in the beam are prefix-independent; native path-dependent
penalties remain outside it. The ordinary seed order is source stroke
order, and context uses an original common FF/FF line frame; neither policy is
presented as the full native orchestration. Case aliases are optional original
mapping behavior. This endpoint does not yet establish production cursive accuracy.

`LegacyCursiveGlyphBoundaryPolicy` restores the two appended boundary inputs from
native `1801b7f70`. In the ordinary branch the first chronological range beginning
at point0 gets+1024, and an interior start gets-1024; the last range ending at its
physical source length gets+1024, otherwise-1024. Explicit runtime suppression
bytes override each sign. Kind1 uses the start suppression byte for both signs.
Ordinary initialization `1801b0760` zeros both bytes; `1801b75b0` appends the two
float signs to eligible feature families through the float-input path. The API
still requires the correct current physical counts and chronological ranges.
It does not sort ranges or invent source identities. The word producer defaults
to these prepared-range signs, with explicit runtime overrides available; forced
caller signs remain an optional diagnostic mode. A native resampled provider
with different ownership/counts needs its explicit upstream mapping.

`LegacyCursiveSeparationTable.Load` loads the separate engine data snapshot
`models/ru-RU/legacy-separation-profile.json`. SHA-256 over its256 little-endian
float values is `b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf`.
The values came from `MyScriptText.dll` static table`1802e1090`; they have engine
provenance and are separate from the independent implementation. No DLL is loaded
at runtime. An analytic sigmoid table is not byte-exact and is not substituted.

## Verification

`tools/LegacyVariableWord.Probe/reference_controls.py` exhaustively enumerates all
partitions independently of the C# beam for 512 small dictionaries/candidate sets,
including negative and duplicate scores. The C# probe compares all 12,462 accepted
word minima and their ranking, plus512 independent boundary-sign controls. It also checks complete prepared ownership, dead-cut
crossing, terminal-before-beam reranking, invalid requests, budgets and cancellation.

With the actual Russian resource pair and engine profile, the probe exercises two
original hand-drawn diagnostic fixtures: printed MIR and one continuous zigzag.
It runs cuts, all variable group classifiers, physical context windows, dictionary
word IDs and independently recomputed native terminal cost components. The raw-cache
top outputs are incorrect for MIR; no accuracy claim is made. The final-cache
path improves the expected MIR candidate into the leading handful (rank4 with
prepared boundary signs), but still chooses the wrong first word in this explicit
scope. A known-choice MIR/MIF
fixture separately verifies real dictionary variable partitioning and lexical
reranking with beam width 1. This is a score transport control, not recognition
accuracy. The tested source translations/scales preserve graph topology and top3
raw-cache outputs; lower ranks can change through floating geometry and truncation.
The final-cache control exercises nonzero native table grouping costs on15 of28
printed candidates and checks the public complete endpoint against the explicit
composition. This is still a scoped diagnostic, not calibrated replacement confidence.

Run the managed probe with:

```powershell
python tools/LegacyVariableWord.Probe/reference_controls.py .artifacts/legacy-variable-word-controls.json
dotnet run --project tools/LegacyVariableWord.Probe/LegacyVariableWord.Probe.csproj -- .artifacts/legacy-variable-word-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

The probe is compatible with headless Windows NativeAOT. No vendor DLL, SDK
authentication, desktop application, or GUI is loaded.
