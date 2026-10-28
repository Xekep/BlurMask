# Fresh first literal context and glyph materialization

This closes one first-controller context branch: a real retained `F0001` cache
candidate can pass through primary98 and B0/B8. It remains in the selected
source trace, but the outer1F42D0 allocation guard suppresses its glyph when
the owned invocation99 is false. It is not converted into a guessed letter.
See [the publication correction](legacy-first-selected-glyph-publication.md).
The main240 linguistic decoder and
complete native primary proposal producer are separate unfinished work.

`LegacyFreshFirstLiteralContextPolicy.ReadSource` requires the matching owned
fresh first source and the audited inline alphabet103 payload SHA256
`2997884bbaaa094929249d1af7e88f285f16ee0e5345ab9013455b66a18eb6a8`.
This is an explicit checked profile restriction, not a general claim that other
alphabets have no Arabic traits. The ordinary E0=false policy, native A2/A4=1
and alphabet come from the same fresh-child binder.

Engine052260 initializes derived array88 to NULL. Reader053F20 does not
deserialize it. Builder0573B0 first scans symbols using04E280 and returns
before allocation if no Arabic shaping symbol exists. Native Unicode service
05F290 searches716 inclusive ranges at1FB6D0; all189 distinct code points in
the actual alphabet were checked against them. No symbol has script5 or an
Arabic digit in0660..0669/06F0..06F9. The59 stored related-form triplets/map78
are distinct from the derived Arabic88 array. Getter052640 therefore returns
false, so Text1EDDE0 chooses1EDDB0. Assembly1EDDB0 writes five zero bytes and
returns success. No caller-supplied guessed trait values are used.

`RunForFreshLiteralSymbols` preserves old `Run` and selected-child plain APIs.
It refuses blank E2, shadda E6 and the EA/EC/EE ancestor-skipping headers
(actual newline182, space180, NBSP181). For current E4=191, native1EF070 changes
only ordinary correction kind0 to6. The resulting record still has the kind0
path bit. B0 extends the context with E4, but B8 kind0 preserves the previous
identity: its native guard covers both E2 and E4 (B0 covers only E2). The
selected glyph trace still contains E4; it must not be equated to a symbolic
context sequence. The kind2 alternative keeps its boundary/identity behavior.

The additive `CorrectLiteral118/140` callbacks preserve existing plain0/2
semantics. Graph initializer194170 sets190/191 to1. Kind6 routes140 to sampler
195650; the first sampler210=1 bypasses its geometry body and returns true
without touching signed cost. Callback118 reaches195C40, whose ordinary switch
has no case6 and returns true without changing cost. The enclosing context
pass still applies its separate impossible-cost comparison. This is an exact
first-sampler branch, not a zero-cost substitute for the main sampler or an
ordinary gap/pair calculation.

`MaterializeLiteralSymbols` is an additive single-word fresh6C overload.
It uses the restored literal symbol plan, actual candidate confidence vectors,
actual cache+category integer costs and owned graph boundaries. Existing
`Materialize` keeps its strict alphanumeric scope. Negative category-map
entries use native geometry0; selected blank, if presented to the symbol plan,
uses the actual headerE replacement. No TOP mass is aliased to glyph scores.
Aliases, circled UTF8 publication, traits, multiple words and all terminal
alternatives are not supplied by this overload.

The headless probe draws an original rectangle and a separate interior stroke.
Actual source/frame/cuts produce3 first NN/cache groups. Prepared bounded
six-category top20 proposals reach primary98 and B8: cost12111, labels134,191.
The selected trace contains134,191; the native allocation guard produces one
word glyph134 and suppresses selected F0001. Its original source probability
remains independently available. The proposal enumeration is an explicit fixture
policy (maximum5 atoms, old descending spans), not the complete native primary
enumerator (resource cap7 and ascending spans). This result proves the branch
composition, not handwriting accuracy or a whole273 winner.

Validation passes in Managed and Windows Native AOT: the actual capture plus
128 original scalar-callback fixtures are independently re-evaluated by the
Python tuple/context oracle (1549 finite records,2052 callback requests,
520 kind6 requests). Six signed extreme/sentinel cost controls verify the
exact unchanged-cost callbacks. Cancellation after each callback and
unsupported skipped labels are checked. The existing256 context controls,
both actual E0 captures,512 word scalar controls and actual35-group `MИP:41268`
word remain unchanged; strict and literal materializers produce identical
word120/glyph40 bytes for that existing plain fixture. `MИP` is mixed script,
not a completed Russian result.

```powershell
dotnet run --project tools/LegacyFirstPrimitiveWord.Probe -- `
  models/ru-RU/ru_RU-ak-cur.lite.res `
  models/ru-RU/legacy-engine-profile.json `
  models/ru-RU/legacy-separation-profile.json <captures.json>
python tools/LegacyFirstPrimitiveWord.Probe/audit_contexts.py <captures.json>
python tools/handwriting/audit_first_literal_context.py `
  --resource models/ru-RU/ru_RU-ak-cur.lite.res --output <static-audit.json>
```
