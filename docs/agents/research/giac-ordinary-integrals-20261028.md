# Ordinary definite integrals through the bundled Giac

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

Root evidence: `E:/BlurMask-build-artifacts-20261014/giac-ordinary-root-20261028/`.
The actual current compiled Solver passes the permanent suite in managed and
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
