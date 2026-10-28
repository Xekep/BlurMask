# Known-word lexical subtotal

`LegacyWordLanguageScorer` combines the previously restored
[stored language tables](legacy-russian-word-language.md),
[explicit cost profiles](legacy-russian-word-cost-profile.md), and
[boundary/category profiles](legacy-russian-word-auxiliary.md) for the checked
order2/flags3 language object. The caller supplies a matching model/profile,
a finite positive runtime scale, a dictionary word ID, zero or one preceding
history slot, category and three flags. Nothing chooses a default profile or
infers category from Unicode.

For the known-word branch without an alternate-vocabulary map, static native
`180100090` selects the word-level code, adds the category cost from
`1800ffcf0`, optionally adds boundary cache `94c` when its input flag `24` is
set, and then adds the calibrated class cost. An empty history uses the
unigram. One preceding entry uses `previousClass * ClassCount + currentClass`
in the bigram table; the previous class is the entry's lower24 bits. Clearing
the category tag does not discard that previous class. The scorer follows
this integer addition order and returns all four separate fields alongside
`LexicalSubtotal`; arithmetic overflow fails closed.

The native later score-compression branch depends on cache field zero. That
field is negated from native `_268c`, which is initialized to `-0x4000000` by
`1800f0ad0`. Flags3 does not serialize an override (the reader requires bit4
for one), so compression is disabled in this checked profile. Alternate
vocabulary, category inference, history update, result pruning/acceptance,
other storage flags and complete acoustic-cost addition remain unsupported.
The subtotal is not a calibrated recognition confidence or a demonstrated
native total score.

Vendor-free tests construct a five-class/41-word model and verify distinct
word, unigram/bigram, category and boundary contributions, unchanged class
history when clearing tags, category suppression, matching profiles,
cancellation, invalid IDs/history/scales and checked total overflow. An
independent Python reference generates 640 original complete scalar profiles,
rounding each floating-point step to binary32 and summing integer fields
separately. Managed .NET and Windows Native AOT pass those controls.

The real RU diagnostic compares `мир` and `миф` across all four profiles,
with/without preceding class643 (the inspected `мама` class) and with/without
the boundary flag: 32 rows. Managed and Windows Native AOT outputs match.
Scale one and category zero are explicit diagnostic choices; no Russian
letter-category inference or acoustic reranking is applied. In profile zero,
empty-history/no-boundary `мир` has lexical subtotal `-4039`; this alone does
not establish the correct recognized word.

```powershell
python tools/LegacyWordScorer.Probe/reference_controls.py .artifacts/own-word-scorer-controls.json
dotnet run --project tools/LegacyWordScorer.Probe -c Release -- --controls .artifacts/own-word-scorer-controls.json
dotnet run --project tools/LegacyWordScorer.Probe -c Release -- models/ru-RU/ru_RU-lk-text.lite.res
```

This component remains headless. Native external profile selection and the
upstream acoustic budget/addition path must be established before claiming
an equivalent shared scorer or deploying it to the application's AI pen.
Representative Russian joined handwriting and cursive quality remain open.
