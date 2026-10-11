# Legacy prepared result score

`LegacyPreparedResultScore.Evaluate` restores the ordinary scalar cost stage of
`180225fb0`. It accepts explicit cumulative path cost, current candidate cost,
previously accounted signed64 cost, source node end index, extra-node flag,
source multiplier and language cost. It returns separate normalized path and
language channels, log mass and unnormalised exponential mass.
It does not select a word, change the dictionary beam or decide whether AI-pero
should replace handwriting. Mass can exceed1 and is not acceptance confidence.

The checked branch has no alternate-source record (`param2+68 == null`) and no
detailed-cost bookkeeping (`runtime+2368 ==0`). Native first adds signed32
`runtime2520[sourceNodeEnd] + candidateRecord4`, converts that sum to float,
then subtracts the independently float-converted signed64 context offset at
`outputContext78`. It divides by
`(sourceNodeEnd +1 + (configurationC4 !=0))*8192`.
This denominator describes native source nodes; it is not proven to equal
letters, glyph count or the independently prepared cut-atom count.

The language channel independently converts its supplied signed32 cost to float
and multiplies by1/8192. The result exponent is the negative explicit multiplier
times normalized path cost. Native returns0 below-75, the stored float
`3.73e32` above75, and float-converted double exponential inside the inclusive
interval. Equality at75 differs from the upper clamp constant.

Assembly independently confirms the arithmetic: `22611b` loads the cumulative
cost, `22611f` adds the candidate integer, and `226128` converts to float;
`2265b9` converts the signed64 offset, `2265d0` subtracts, `2265d4..2265d8`
build the source span and shift by13, and `2265e2` divides. `2265f3..226600`
loads and negates the explicit multiplier and multiplies. `226604..22663e`
implements the inclusive exponential/clamp branches. The language channel is
stored independently at `2265b1`.

Backtrack `21ffd0` provides additional evidence that source kind135 uses factor5
in its negative score channel and other kinds use1. Callback `21fe30` copies
the three scalar channels into separate result fields at offsets10,18,1c.
The API deliberately retains an explicit multiplier instead of inferring that
this dispatch also initializes every225fb0 caller. It rejects nonpositive or
nonfinite multipliers, signed32 combined-cost overflow and source spans that
would overflow the positive signed32 denominator. It does not emulate wrapping
invalid configurations.

Independent original controls cover778 float32 cases, signed costs, large
signed64 offsets, cast-before-subtraction behavior, both source-span flags,
clamp equality and values beyond both clamps. Path/language/log channels must
match exactly; transcendental results allow two float ULPs because platform
math libraries can round exponentials differently. Cancellation and invalid
domain cases are checked. Optional actual RU controls run the restored
neural/cut/dictionary endpoint and transport32 candidate cost decompositions
through this scalar stage. Their prepared-atom source-node mapping is explicitly
a caller-owned diagnostic, not restored native source orchestration or cursive
accuracy evidence.

```powershell
python tools/LegacyResultScore.Probe/reference_controls.py .artifacts/legacy-result-score-controls.json
dotnet run --project tools/LegacyResultScore.Probe -c Release -- .artifacts/legacy-result-score-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
```

Remaining controller gaps include native source-node/backtrack construction,
previous-context offset production, alternate-source correction, final
candidate posterior/acceptance policy, and calibrated real Russian cursive
accuracy. The result stage can be reused with independently supplied metadata;
it does not justify automatic Russian replacement yet.

Separate prior-path investigation identified the provider bridge:
`204290` reads prior cost from runtime48 candidate records at offset10;
ordinary wrapper type290 virtual48 forwards through `2392e0`→`239100`→`2388e0`.
The last routine recursively sums supplied item costs while flattening a source
chain, copies each record, and adds the accumulated prior to its offset10.
Underlying source-provider item generation and complete dispatch remain open.
This observation is documented as evidence only; no guessed prior was added to
the variable word beam.
