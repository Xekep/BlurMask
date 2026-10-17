# Portable Math2 score-channel adaptation

The internal `LegacyPortableMathLogProbabilities` implements the proven
Engine54.V48 float32 operation order on the last time frame of a complete
rank3 time/batch/297-class decoder output. It keeps log probabilities and
cumulative costs in distinct private arrays. All exported arrays are copies.
No model inference or app replacement occurs in this component.

For each beam, the algorithm finds the maximum raw logit and calculates an
inverse temperature using float32 division. It accumulates exp of each scaled
difference in input order, takes log of that float32 sum, adds the separately
scaled maximum, then subtracts that normalizer from each separately scaled
raw logit. The cost channel flips the exact sign bit of the log channel before
adding the previous beam cost with one float32 addition.

`MathF.Exp` and `MathF.Log` are an explicit portable CPU adaptation. The
original engine's CPU-dispatched transcendental implementations remain
unported. This component therefore does not claim bit-identical vendor math,
calibrated handwriting confidence or a complete EEF tensor lifecycle. It
rejects unsupported dimensions, nonfinite inputs/intermediates, nonpositive
temperature and invalid prior costs. It does not invent a native clamping or
error policy for those unsupported inputs.

The existing math probe adds120 scenarios with three time frames, five batch
sizes and three temperatures, including a rounded reciprocal of3. Independent
Python double elementary functions are rounded to float32 at each required
boundary. The declared cross-platform absolute tolerance for log values is
2e-5; cost sign/addition bits must match the actual log bytes exactly. It also
checks normalized probability mass and unchanged input logits. The tested
Windows Managed/Native captures agree in full over220968 log/cost pairs;
the independent log oracle has zero observed error on these controls. This
observation does not prove the unexecuted native vendor functions equivalent.

The adaptation is an internal dependency for the next owned ONNX Runtime
state join. It is not connected to the app's current greedy recognizer.
Native [score-channel evidence](legacy-math2-score-channels.md) and
[tensor lifecycle](legacy-math2-tensor-lifecycle.md) remain separate proofs.
