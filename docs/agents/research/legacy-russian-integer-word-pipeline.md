# Integer word dictionary pipeline

`LegacyIntegerWordDecoder` composes caller-prepared
`LegacyIntegerLexicalCandidate` choices, `LegacyIntegerLexicalDecoder`,
`LegacyLexiconWordIndex`, and `LegacyWordCandidateScorer`. Complete dictionary
words receive their restored ordinary lexical subtotal **before** final
beam pruning. Output preserves labels, terminal state, word ID, acoustic
cost, additional path contribution, lexical fields, budget and total.

The constructor checks graph identity against its word index and requires
the dictionary and language model to have equal word counts. These checks
do not establish the provenance of arbitrary equal-sized resources: use
the checked model bundle for the matched Russian resource pair.

Every call explicitly supplies history, category, clear/suppress/boundary
flags, additional path cost, total cost limit, heap fullness and bounded
beam width. The language scorer already contains explicit profile and
runtime scale. The supplied additional cost accounts for any previous
path, segmentation or correction costs required by the caller. Integer
addition rejects unsupported range overflow; negative lexical costs can
rescue an acoustic path above the supplied limit.

The search is an original bounded beam over a fixed sequence of supplied
glyph choices. Prefix pruning uses acoustic cost, so a narrow prefix beam
can still discard a word that later lexical scoring would prefer. The
adapter does not implement the SDK's dynamic word search, lexical lower
bounds, word category derivation, native default heap capacity, automatic
case policy or continuous-stroke cuts. Its caller choices and parameters
must remain stable during decoding. It does not enable Russian recognition
in the application by itself.

## Checks

The vendor-free probe serializes its own three-word dictionary and tiny
word language resource. A beam-width-one case deliberately gives the
lexically preferred word a worse acoustic cost, proving that terminal
scoring precedes final pruning. It also checks retained score components,
additional cost, budget equality/full-heap ties, negative lexical offsets,
overflow, graph/index identity, model count, invalid bounds and cancellation.
Python independently generates 640 controls including duplicate choices,
out-of-dictionary labels, history, category flags, limits and signed offsets.

```powershell
python tools/LegacyIntegerWord.Probe/reference_controls.py .artifacts/integer-word-controls.json
dotnet run --project tools/LegacyIntegerWord.Probe -- --controls .artifacts/integer-word-controls.json
dotnet run --project tools/LegacyIntegerWord.Probe -- models/ru-RU/ru_RU-lk-text.lite.res
```

The actual-resource mode supplies deliberately synthetic integer choices
for the dictionary alternatives «мир» and «миф». With profile 2 and explicit
runtime scale 1, acoustic costs 150/50, neutral category, no history and
additional cost 7, the lexical costs are 4537/9461 and totals 4694/9518.
Thus «мир» wins after lexical scoring despite its worse supplied acoustic
cost. Four controls cover preceding class 643 and the explicit boundary
flag; beam-width-one selection is checked in each. These controls test
dictionary integration and numerical composition, **not** handwriting
accuracy, a real stroke sample, or joined cursive recognition.

Managed and Windows Native AOT executions passed the own checks, 640
independent controls and four actual-resource controls. The native probe
does not load a vendor DLL. See
[ordinary candidate scoring](legacy-russian-word-candidate.md) and
[stored source profile forwarding](legacy-russian-word-source-profile.md)
for the restored native arithmetic and the scope of profile 2.
