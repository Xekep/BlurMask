# Legacy order2 boundary and category costs

`LegacyWordAuxiliaryProfile` restores two auxiliary lexical cost branches for
the validated type2d1/base100/order2/flags3 language object. Four profiles are
loaded from the caller's complete resource; profile selection, category labels
and input flags remain explicit. This is not a total recognizer or reranker.

Native `1800fe2a0` computes cache field `94c` from the boundary offset `_120`
and word offset `_110`. For the proven positive-scale branch with stored word
levels, the cost is `truncate((scale * boundaryOffset - scale * wordOffset) *
-8192)`, rounding each arithmetic operation to binary32. `180100090` adds it
only when the caller's boundary flag at input offset `24` is set. The API
provides `GetBoundaryCost(scale)` and leaves that decision to the caller.

The resource's punctuation section contains a uint16 flag at native `_150`,
four float offsets `_154`, four float slopes `_164`, 169 pair transitions at
`_174`, and 2,197 second-history transitions at `_418`. The flag is **512** in
the checked RU object, not the number of categories. Nonzero enables category
history lookup. Category indices are zero through twelve, with zero selecting
the no-tag fallback behavior.

`1800fe2a0` converts each nonzero-category pair as follows: binary32
`(slope * transition + offset) * 8192`, add or subtract `.5` according to the
sign, truncate, then negate the integer. This is signed rounding away from
zero followed by negation. The category coefficients are not multiplied by
the word model's runtime scale. The reusable loader validates finite values
in both stored transition arrays; the order2 path supports at most one prior
history slot and does not select the stored second-history table.

`ScoreCategory(category, history, clearFirstCategoryTag, suppressCategoryCost)`
restores `1800ffcf0` for that order2 scope. History category is bits24..27 of
the preceding class entry. A zero tag stops tagged history lookup; clearing
the first tag therefore forces the no-tag fallback, without mutating caller
memory. The separate suppress flag returns zero while retaining the number
of tagged history entries. Without a tagged history entry, category3 costs
one (or minus two when clearing), categories5/6 cost two, categories10/11/12
cost three, and the other categories cost zero. Unicode-to-category inference
and history update `1800ffec0` remain unsupported. Values above category12,
more than one history slot and nonfinite/overflowing arithmetic fail closed.

At explicit runtime scale one, the checked resource has these diagnostics:

| Profile | Boundary cost | Category1 → category3 cost |
| --- | --- | --- |
| 0 | 0 | 9830 |
| 1 | 11264 | 0 |
| 2 | 11264 | 2048 |
| 3 | 10649 | 9830 |

These values are separate fields. Native `180100090` adds calibrated word and
unigram/bigram costs plus category and optional boundary costs, then applies
additional native result handling. The upstream acoustic budget/addition and
external profile IDs still need proof; these diagnostics must not be used to
claim a correctly ranked word or complete Russian cursive recognition.

Vendor-free tests construct an original complete language object with distinct
profile fields, check transition orientation, signed ties, fallback/suppress/
clear flags, ignored upper/lower history bits, boundary scaling, cancellation,
invalid categories/history, nonfinite storage and overflow. Independent Python
binary32 controls cover 640 original randomized profiles. Managed .NET and
Windows Native AOT pass those checks and produce identical actual diagnostics.

```powershell
python tools/LegacyWordAuxiliary.Probe/reference_controls.py .artifacts/own-auxiliary-controls.json
dotnet run --project tools/LegacyWordAuxiliary.Probe -c Release -- --controls .artifacts/own-auxiliary-controls.json
dotnet run --project tools/LegacyWordAuxiliary.Probe -c Release -- models/ru-RU/ru_RU-lk-text.lite.res
```

This component remains headless and is not connected to the application's
AI pen. The full model snapshots under `models` are separate from the ignored
derived diagnostic controls.
