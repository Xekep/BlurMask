# Current actual2B1 line-frame regression

The synthetic printed MIR fixture still selects `мир` after the corrected
actual2B1/1111 producer replaced the historical generic2B0 path. Its former
single cost54456 is stale. The exact expected results depend on the inferred
line profile; no source frame, search policy, model or winner is changed.

| Profile resource offset | Acoustic path cost | Lexical subtotal | Total |
| --- | ---: | ---: | ---: |
|5789|48714|4537|53251|
|11421|48744|4537|53281|
|187976|48744|4537|53281|

`LegacyOrdinaryLineScale`, `LegacyLinePostselection` and `LegacyLineSweep`
assert these exact totals and their acoustic/lexical decomposition. The sweep's
existing three-row,8000-unit translation case remains a separate scope: it
checks the winner and strip/frame structure, rather than an invariant cost.
Float translation changes the middle frame's prepared geometry there; it is
not grounds to widen the one/two-row assertions or tune recognition.

Reverification on5d79bca: all three probes passed Managed and Windows NativeAOT,
with identical per-probe output. This covers27 actual profile/scenario runs,
48 frame recognitions (39 exact-cost cases),384 independent scale controls,
512 postselection controls and512 sweep-stage plus16 full synthetic controls.
The existing separate `LegacyFreshMainAcoustic` independent oracle also passed
40 actual NN evaluations,7200 probabilities,1800 integer channels and1950 cache
entries on the same corrected core. That numerical oracle is separate from
these bounded word-search fixtures; it does not independently reconstruct every
line fixture's search winner. These are regression controls, not a Russian
cursive accuracy or native final-winner claim.
