# Ordinary definite integrals through the bundled Giac

29 October CLI compatibility follow-up: some official builds emit the imaginary
constant as `\mathrm{i}`. The adapter canonicalizes only that supported token
before its existing unsupported-markup filter. The original permanent probe
passes managed/Native AOT against both the bundled and exact official pinned
CLI. Evidence: `giac-cli-timing-compat-review-20261028` and
`giac-cli-compatible-root-20261028` under the build-artifact root. Two historical
outer45s timeouts remain recorded; complete traces found distributed per-call
cost rather than a reproduced stuck calculation. No calculation limit or golden
was relaxed. A fresh51-run procedural NN corpus also passes its equality gates
with the actual bundled CLI; the CI job now binds that downloaded solver.

The old default mode0 adapter could publish0 for ordinary divergent integrals:
1/x on[-1,1],1/(x-2) on[1,3], and tan(x) on[0,pi]. This was an actual bundled
CLI/adapter result, not a handwriting-recognition failure. The fix is in
[SymbolicMathSolver](../../../src/InkMath.Solver/SymbolicMathSolver.cs).

## Pinned native cause and behavior

The packaged runtime builds GeoGebra Giac commit
`bdbb1c41fe52d8b4fb7c1d15c2deaea88fc3803c`. Its
[intg.cc](https://github.com/geogebra/giac/blob/bdbb1c41fe52d8b4fb7c1d15c2deaea88fc3803c/src/giac/cpp/intg.cc#L4489)
runs additional interior-singularity checks in calculation mode1/38. Mode0
skips that branch. The adapter evaluates expressions containing definite
integrals in mode1, stores the result in a grammar-inexpressible generated
temporary, and restores mode0 before solve/simplify/LaTeX. Mode1 cannot simply
remain enabled: its root-set and imaginary-unit presentation differ.

Mode1 alone still returned the principal value0 for x/(1+x^2) over the whole
line. Opposite explicitly represented infinite endpoints are therefore split
at0 into separately evaluated tails. Each integrand is stored once, preventing
exponential duplication of nested whole-line requests. Their divergent tails
produce no accepted finite answer. All mathematics still executes in Giac;
there is no local integration engine or numerical fallback.

The pinned warning about unverified antiderivative singularities is checked
for every definite integral. The generated diagnostic asks Giac whether the
body is polynomial or all reported singular points are proven nonreal or
strictly outside both bounds. Symbolic/unknown comparisons cannot count as
true: they are reduced to numeric0/1 only after checking native numeric types.
A native diagnostic exception prints status0; it does not prevent a valid
calculation without that particular warning. Missing, unknown or substring-only
status markers cannot authorize an answer in the presence of the warning.

This is a sufficient response to a specific native diagnostic, not a universal
convergence certificate. Unknown parameter-dependent poles refuse. Endpoint,
branch-cut and removable-singularity handling remains native and can refuse
valid integrals, notably the negative half of whole-line sin(x)/x. An unsupported
answer remains null; the UI does not label every refusal as divergence.

## Limits and locale

The generated script refuses above16384 characters before creating a child.
Existing input4096/node512/depth32 limits and process5s/256MiB/output64KiB limits
remain. Generated value/integrand/lambda/error names cannot be produced by the
admitted grammar. Native child LC_ALL/LANG/LANGUAGE are fixed to C; the UI locale
is unaffected. The current custom bundled wrapper uses identity gettext, so a
translated-warning bypass was not demonstrated in that binary. The explicit
child locale also stabilizes supported gettext-enabled CLI builds.

## Verification scope

[Permanent solver controls](../../../tools/InkMath.Solver.Probe/Program.cs)
retain47 grammar translations,44 native algebra/calculus cases,25 exact integral
cases and process cancellation/unavailable/undefined/injection guards. They add
12 ordinary/mode/nested results,10 divergence/domain refusals,12 generated-name
binding guards,6 status-marker guards and a valid-AST command cap that launches
zero children. Tests preserve333 for integral[1,10] x^2 dx and refuse the
ambiguous dx^2 syntax instead of inventing an integrand.

## Endpoint and infinity follow-up

Additional root negatives found silent mode1 zeros at the endpoints too:
tan(x) on[-pi/2,pi/2], x/(1-x^2) and x/(1-x^2)^2 on[-1,1]. They also found
whole-line PV bypasses when infinity was written as -1*infinity,
-infinity+1 or2*infinity. The earlier mode1/split checkpoint did not close these.

The successor always requires an exact native endpoint-status1 line, even
without a warning. Giac computes the primitive in mode0 and checks one-sided
limits; unknown, nonfinite, missing or failed status refuses. Both directions
at both endpoints are checked conservatively. Native finite-bound polynomial
and nonsingular-primitive cases retain symbolic parameters. This remains a
native finite diagnostic, not a universal proof or a host-side antiderivative.

Infinity-containing bounds outside the admitted explicit signed forms refuse
before creating a process. This also refuses convergent arithmetic equivalents;
no host constant evaluator was added. Canonical opposite infinities retain the
two-tail evaluation. The existing16384-character generated-command ceiling
still applies.

The permanent suite additionally checks6 divergent endpoints,2 integrable
singular endpoints,6 endpoint-status guards,8 unsupported infinity AST forms
with zero children and18 total generated-name binding cases. In particular,
1/sqrt(x) and1/sqrt(abs(x)) on[0,1] remain2. Two independent oscillatory-endpoint
controls also refuse.

Root successor evidence is under
`E:/BlurMask-build-artifacts-20261014/giac-endpoints-root-20261028/`:managed and
Windows Native AOT permanent suites, all98 original adapter controls and13
root endpoint/infinity controls pass against the actual current Solver DLL.
Its SHA256 is
`d238adc631bb95f7fb50122e8c35e2b539663782de9de649779e8db9bc5a94a8`.
Earlier baseline/packet evidence above remains historical, with failed controls
preserved under `giac-ordinary-root-20261028`.

Initial checkpoint: `E:/BlurMask-build-artifacts-20261014/giac-ordinary-root-20261028/`.
The initial compiled Solver passed the permanent suite in managed and
Windows Native AOT. The98 adapter controls invoke the existing real
AiMathLayout.WithSolvedResult with this exact compiled Solver and the bundled
CLI. A separate run passes with inherited French locale. Authored Solver/probe
sources are portable-PDB authenticated. The reviewed Solver DLL SHA256 is
`54cc23a63c58a41316c8ff06d3924b9b26f0555b5e54911f09949e36df4263ed`.

The agent's baseline negatives, pinned primary sources, independent five-case
semantic review and generation-growth checks remain under
`E:/BlurMask-build-artifacts-20261014/giac-integral-review-next-20261028/`.
These tests establish the calculation boundary. They do not prove that pen
strokes were recognized correctly, that all improper integrals are classified,
or that the full MyScript restoration is complete.
