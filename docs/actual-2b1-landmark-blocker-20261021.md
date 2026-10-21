# Actual2B1 landmark producer: open defect

Russian V2 integration is blocked. English MAIN N is already committed but has
the same unresolved helper mismatch. No app default or release is changed.

The actual2B1 BEDF0 producer calls BC280. Before comparing point2, its full body
loads previousY from psVar19[3], the Y coordinate of point1. The generic2B0 B6D20
pass instead retains the initial point0 Y through this first comparison. The
current LegacyDirectionalLandmarks deliberately implements that older quirk.
Reusing it for actual2B1 is incorrect. Independent review confirms BC4DA loads
word[rbx+2] before BC4DE advances the point1 pointer and BC4F7 compares point2 Y.
Vendor binaries were inspected as static bytes and were not executed.

A three-point reversal distinguishes the branches: Y=[0,10,5] requires a
vertical-maximum landmark at point1 in BC280. Comparing point2 with point0
misses it. Physical samples after native resampling must also be tested; this
prepared witness alone does not establish a recorded-word accuracy change.

The complete receiver68 inventory remains valid: one write and three consumers
control whole/local dimension conventions. It does not prove that every helper
called by those methods is numerically equivalent to its older2B0 counterpart.
The previous broad configured-producer/native-feature closure claim is withdrawn.
Independent numerical references reused the same old landmark convention, so
their matching captures cannot close this defect.

Required next work: a separately proved BC280 pass, actual English/Russian
producer joins, an independent reference, full selected-helper comparison and
fresh feature/NN/cache captures. Preserve the generic2B0 public behavior and all
earlier frozen evidence. Russian V2's132/261 exact words/342 character edits,
versus historical137/325, are incomplete-producer results, not a readiness claim.
English N/D0 numerical captures are likewise implementation checkpoints pending
this upstream correction, not complete native feature parity or word accuracy.
