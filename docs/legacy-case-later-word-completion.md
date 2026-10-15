# Case-aware later-word completion

`LegacyNativeLaterDictionaryWordCompletion.ForFreshCaseSpan` binds one fresh
case292 source to the existing selected model, language profiles, ordinary
graph, shared context container and live span/map lease. `ProcessActualQuery`
consumes the supplied full query32 rather than rebuilding it from the current
stored88 row. The original constructor and `ProcessCandidate` retain their
DIRECT injection behavior and numerical controls.

This is a prepared consumer. The factory proves source/model/span/context
ownership; it does not prove that the supplied query32, context16 candidates or
history rows were emitted by the complete native scheduler. The typed map span
is the last D0 preparation, not necessarily the span of the current word's
anchor. No per-span query reset is added.

Native `204D7B..204D9E` calls source70 with the complete query tag and query
symbol. `205BCD` copies that token into a history request starting at `205C00`;
request category20 remains token+8. `205BF6` changes only boundary24 from the
previous stored row's flag5.bit2. Fallback `205CB9` and `2061D4` likewise retain
the source category. Both unigram and bigram paths in `100090` consume that
category; sampler geometry categories remain separate path/transition inputs.
Source50 restart also receives the complete case tag. The record producer
retains its context lineage, query cost/context, current-row ordinal and actual
language payload.

The supported branch is fresh one-source C00/order2, ordinary frontend,
mode22F6 false, nonpositive ec, no tracing, no associated source, ordinary
anchor/query kinds0/2 and no opaque query24 pointer. Symbols may be ordinary
nonvirtual AL punctuation as well as letters/digits. Replacement, virtual
protocol and force branches still refuse. The old strict injection API keeps
its previous letter/digit restriction.

Anchor56/60 and previous-node/ordinal references must resolve through the
supplied owning row collections. Context16 must match the previous row's
secondary state and current-row lineage. Its previous ordinal+C must be at
least the actual primary limit230A. The previous-node collection must contain
exactly SourceEnd23F0 entries, so its count supplies the upper bound; capacity
or retained end23F8 must not be substituted. The anchor30 original-parent
reference may be primary and does not use this secondary-only restriction. The frontend E8 callback remains an
explicit prepared owner input, not a guessed zero/minus-one boundary. Native
`152280` resolves graph group68, input ordinal78 and source descriptor+10.
Actual sampled main sources expose their assigned descriptor line; a fresh
unassigned descriptor snapshot must not replace that line. Case binding checks
the full source extent, context identity, nonnull query08 ownership, source headers, flags and live lease
before work and after every external owner callback, restart and insertion.

The new probe produces55 real owned case startup/first-language terminals on
the original M fixture. It reuses one actual terminal query and first-language
class payload, then explicitly prepares later anchor placements, context16 and
history collections. Its339 cases cover12 case language categories, retained
and suppressed category tags, unigram/bigram paths, previous boundary flags,
full-tag restart, allocation residue and full16/40/128 outputs. This does not
claim an actual later NN/203690 scheduler path or a decoded winner. A separate
nonterminal-current + supplied-terminal-query control is explicitly prepared.

For the checked RU dictionary, `Symbols.CopyAuxiliary()` has two finite entries:
lexical0 maps to ZWJ184, lexical68 maps to FFFB. Across895571 packed nodes there
are17 nonroot mapped edges and zero mapped terminal edges. Consequently the
genuine RU V48 emits no new auxiliary terminal; an injected current terminal
must not be relabeled as a positive V48 result.

The independent Python audit decodes the raw LK packed dictionary, terminal
preorder, class tables, profile2 costs and case category table. It compares the
complete metadata16, language40 and allocated128 records, including inherited
history, residue and restart bits. Managed and Native AOT must match exactly.
Old later-word history/full-record controls remain separate regressions.

```powershell
dotnet run --project tools/LegacyCaseLaterWordCompletion.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json models/text-unicode/legacy-case-data.json models/text-unicode/legacy-text-publication-data.json case-later.json
python tools/LegacyCaseLaterWordCompletion.Probe/audit_output.py models/ru-RU/ru_RU-lk-text.lite.res case-preparation-controls.json case-later.json
python tools/handwriting/audit_case_later_word_completion.py case-later-static.json
```

Full case scheduling still needs native203690 policy/capacity production,
current/previous row lifecycle and genuine per-node completion routing. This
component does not widen `LegacyOwnedMainDirectWordPass` into a case pass or
enable Russian recognition in AI-pero.
