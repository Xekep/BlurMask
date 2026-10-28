# Ordinary word candidate cost

`LegacyWordCandidateScorer` connects a caller's accumulated ink path cost to the
validated known-word lexical scorer. It restores one language-enabled branch of
the native word beam; it does not restore the complete native search policy or
choose a default recognition profile.

## Native evidence

In `MyScriptText.dll` function `180204290`, the dictionary is explicitly checked
against type `13b`. Its field `88` is the language model: reader `18010c5f0`
loads the first nested object into field `80` and the second into field `88`
when its presence flag is set. The inspected Russian dictionary contains a
`2b4` trie followed by the checked `2d1` model. The latter's raw method table
`1802babb0 + 58` points to `180100090`.

On the ordinary `local_314 == -1`, `local_336 == 0` branch:

1. `uVar15` is previous beam state cost at `10` plus current candidate cost
   `local_304`, read from the candidate record at `58`.
2. The lexical query's field `28` receives `currentLimit - uVar15`.
3. `180100090` admits a lexical candidate if its cost is at most that budget.
4. The returned heap record's first integer is added to `uVar15`, then passed
   to candidate insertion `1801f9080`.

`1800fb510` rejects lexical record costs at or above `0x4000000`.
`1801f75b0`, called by `1801f9080`, similarly rejects total costs at or above
that sentinel. If its final heap is full, a candidate equal to its current
worst cost is rejected; a strictly better candidate can replace it.

The API uses `long` arithmetic for budget and total, then rejects values outside
signed 32-bit range. This is an explicit robustness restriction: it does not
reproduce native integer wrapping. Negative lexical offsets are supported:
a path above the current limit is not prematurely discarded if a negative
lexical cost can bring the final total below that limit.

## Explicit caller responsibility

The caller must select matching language/profile/scale, dictionary word ID,
history, category, boundary flags, accumulated pre-lexical path cost, total
limit and whether the final heap is full. The component does not invent ink
costs from calibrated confidence. The original path can already include cut
costs and corrections; their production has to be established separately.

Native `local_336 != 0` is a different branch: the caller queries lexical data
but uses `uVar15` without adding the returned lexical cost. This scorer must
not be used for that language-bypass branch. It also does not manage the
separate temporary lexical heap, reconstruct beam width, update history,
apply native case policy, or perform candidate segmentation.

## Validation

The headless probe runs end-to-end through an original five-class, 41-word
resource: word and prior-class costs, category transitions and boundary costs
all reach the final candidate evaluation. It verifies exact budget equality,
strict full-heap ties, negative offsets rescuing a path, both impossible-cost
guards, widened overflow guards and cancellation. An independent Python
reference covers 640 cases. These are arithmetic and integration checks,
not evidence of Russian handwriting accuracy or product AI-pen readiness.

```
python tools/LegacyWordCandidate.Probe/reference_controls.py controls.json
dotnet run --project tools/LegacyWordCandidate.Probe -c Release -- --controls controls.json
```
