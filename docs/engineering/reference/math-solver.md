# Local symbolic mathematics

The reusable `src/InkMath.Solver` module talks to the native C++ Giac/Xcas CLI in
an isolated process. It has no dependency on Avalonia, recognition models,
Python, a web account, or a cloud API. It is compatible with Native AOT.
All calculations, including basic numeric arithmetic and definite polynomial
integrals, run through Giac. There is no built-in fallback calculator.
Windows x64 (installer and portable ZIP), Linux x64 (portable archive, Debian,
Arch and Void/glibc packages) and native macOS x64/arm64 builds include a
separately executed Giac CLI. GMP/MPFR are linked statically; Windows needs no
additional solver DLLs, and Linux/macOS use only operating-system libraries.
There is no Xcas graphical interface. Licenses and the corresponding-source
archive hash are installed beside the solver; exact sources and build recipes
are published with the release. [Build details and provenance](../../agents/tooling/giac-runtime-build.md).

## Enable it

Enable **Settings > Use Giac**; no separate installation or path selection is
required for the bundled platforms. The switch is on for new settings. Existing
explicit saved preferences, including off, are preserved. With Giac disabled or
unavailable, calculations retain the original formula and report failure.
Settings has no path selectors or download buttons. On other platforms, standard
installed CLI paths are discovered automatically. A custom developer installation
can select `icas.exe` / `icas` / `giac`, rather than the graphical `xcas` executable:

```text
NovaPen --set-math-solver /absolute/path/to/icas
```

The command verifies `1+1`; bundled native solvers take precedence. No Python
runtime, web account or external service is needed for calculations.

## Requests and supported notation

AI pen preserves notation until an explicit calculation request: append `=`.
Basic numeric requests use the same Giac adapter as calculus and algebra.
For a complete equation, append another `=` or select it and use **=** in the
cursor's selection submenu. A written right-hand side is never auto-overwritten.
Automatic routing calculates only after accepting the OCR candidate or a complete
raw-content region. Uncertain or rejected Math candidates do not start Giac.
Accepted numeric requests classified as text also use Giac.
The result participates in document undo/redo and ordinary canvas synchronization.
An unsuccessful requested calculation now reports a notice instead of silently
retaining an unexplained `=`. The original formula remains editable. `\limits`
and `\nolimits`, upright `\mathrm{dx}`/`\text{dx}` and a unit integrand `\int dx`
are supported. A powered differential such as `dx^2` is ambiguous and rejected;
write `x^2 dx` when integrating the square of x.

| Input before the final `=` | Operation |
| --- | --- |
| `\int x^2 dx` | Antiderivative, including `+C` |
| `\int_0^1 x^2 dx` | Definite integral |
| `\frac{d}{dx}\sin(x)` | Derivative; matched higher orders up to 16 |
| `\frac{\partial^2}{\partial x \partial y}x^2y^3` | Mixed partial derivative; total order up to 16 |
| `\log_{2}{8}`, `log_{2}(8)` | Logarithm with explicit base |
| `\lim_{x\to 0}\frac{\sin x}{x}` | Limit |
| `\sum_{k=1}^{10}k`, `\prod_{k=1}^{4}k` | Finite sums/products |
| `x^2-1=0` | Roots, displayed as `x ∈ {−1,1}` |
| A `cases` matrix of equations | Simultaneous equations |
| Matrix, determinant, inverse, trace, rank, eigenvalues | Linear algebra |
| Trigonometry, fractions, roots, powers, `factor`/`expand` | Exact simplification |

This is a general CAS integration, not a Photomath-style explanation service.
It does not generate worked steps, solve word problems, or guarantee an answer
for every mathematical expression. The input adapter deliberately accepts a
bounded math grammar, rather than arbitrary Giac programs. Inequalities,
differential equations, some special functions and ambiguous notation are not
yet adapted. Unknown notation, an unresolved integral, division by zero, an
unsupported typesetting result, a missing engine, or a resource limit preserves
the original formula. Recognition accuracy is a separate limitation.

## Resource and execution boundaries

Each calculation uses a fresh child process, with `UseShellExecute=false`,
redirected streams, no console window, and a private temporary working directory.
No user-supplied string becomes a shell command or a native CAS function name.
AST generation allows only known math operations. The engine has no idle process.
Giac and numerical-library thread counts are set to one. Each request has a
five-second deadline, a sampled 256 MiB private-memory limit, a 64 KiB limit per
output stream, and cancellation which kills the process tree. The memory check
is monitoring, not an OS sandbox or a strict instantaneous allocation cap.

Input limits: 4 KiB, 512 expression nodes, depth 32, eight free variables and
12×12 matrices. Supplied local engines must be trusted software. The installation
command verifies a calculation, not authenticity; use upstream or OS packages.

## Verification

`tools/InkMath.Solver.Probe` checks parsing, forbidden commands, bounded
complexity, actual calculus/algebra results and cancellation of a live native
process. Pass the absolute CLI path to exercise Giac. Without it, only grammar
tests run. `BLURMASK_TEST_GIAC` enables real CAS → application equals policy and
typesetting tests under `NovaPen --ai-pen-test`, without starting any GUI.
Native application CI builds the bundled CLI from pinned sources on Windows,
Linux and macOS, then checks automatic discovery, the default-on toggle and
calculus/algebra results. Package tests run `NovaPen --math-solver-test` after
installation and reinstallation of Windows, Debian, Arch and Void packages.
The legacy upstream CLI remains a separate regression control. Source-build
transport tests and complete license/source manifests accompany every payload.
All application solver checks are headless and do not open application windows.

The Giac-only probe checks47 grammar translations,44 actual native
arithmetic/calculus/algebra fixtures and25 exact integral fixtures, including
`\int_{1}^{10}x^2dx` =333, reversed bounds and rational constant powers.
It also checks undefined division, injection rejection, cancellation, child
cleanup and an executable disappearing before launch. Decimal integral fixtures
use equivalent exact rational inputs for stable exact Giac output.

This is solver verification, not proof that all those expressions are recognized
accurately from handwriting. A separate [actual handwriting corpus audit](../../agents/research/math-ocr-corpus-audit.md)
checks Math2/ORT 1.23.2 on fourteen independently labeled trajectories covering
integrals, derivatives, matrices, limits and a quadratic-root expression. Ten
match their labels after limited notation normalization; four contain genuine
high-confidence symbol errors. Managed and Windows NativeAOT results agree.
The application accepts fourteen direct mathematical candidates and automatically
routes thirteen; one correctly decoded integral stays handwritten because the
raw-content classifier falls below the confidence gate. A larger held-out corpus
and recovered native search/context policies are still needed. Synthetic
structural-token tests verify formatting only.


### Factorial precedence regression check, 23 October 2026

Factorial now applies to a power's base before exponentiation: `3!^2` parses
as `(3!)^2` and computes36. Unbraced `2^3!` remains `2^(3!)`, computing64;
`2^3!^2` is right-associative and computes68719476736. Explicit superscript
boundaries retain their prior meaning: `2^{3}!` and `(2^3)!` both compute40320.
Double/triple-factorial notation is unsupported and left unsolved; it is no
longer silently interpreted as repeated ordinary factorial. `(3!)!` explicitly
requests repeated factorial and still computes720.

The original `3!^2` parser failure was reproduced before the fix. Final checks
passed39 grammar translations,20 exact polynomial integrals and37 native Giac
fixtures, plus invalid notation, injection, cancellation and child cleanup.
This verifies calculation/parsing, not handwriting recognition or math-model
admission. An independent source review also checked the braced-power cases.

Evidence: `root-math-factorial-final-verification-frozen-manifest-20261023.json`, SHA256 `80156ab773285a64d177ffa5801c54268aee044ee8f9682bb0fa7ee88eacdd8f`, under `E:/NovaPen-build-artifacts-20261014/`.

### Recognized limit notation, 23 October 2026

The parser accepts the formatter's plain `lim_{x\to 0}...` in addition to
`\lim_{x\to 0}...`. The plain operator requires a braced lower bound, with
optional whitespace around the underscore. Longer letter products such as
`limx` and `limit`, and the indexed product `lim_2`, retain their prior meaning.
Formatter output and recognition goldens are unchanged. One-sided limits are
outside this change.

The exact existing formatter fixture `lim_{n\to \infty }x` failed parsing
before the fix. The regression probe now checks it, finite and infinite bare
limits, token boundaries and malformed/injected inputs alongside the existing
factorial checks. These tests verify parsing/calculation, not handwriting
recognition accuracy.

### Giac-only calculation, 28 October 2026

The subsequent ordinary-definite-integral fix evaluates these requests in
native calculation mode1 and restores mode0 before solve/LaTeX. Explicit
opposite infinite bounds become two independently evaluated tails. An
unverified-singularity warning requires a sufficient numeric native diagnostic;
unknown status refuses. Generated commands above16KiB refuse before process
launch. This is conservative and can reject valid integrals; it is not a
general convergence proof. See the
[calculation verification](../../agents/research/giac-ordinary-integrals-20261028.md)
for the pinned cause, test scope and remaining native limitations.

Removed `PolynomialIntegralSolver` and `NumericExpressionEvaluator`. Recognition,
LaTeX formatting and the bounded AST adapter remain independent from calculation.
The experimental library calculation helper delegates explicitly to its supplied
Giac adapter and never evaluates arithmetic itself. Retained raw Math2 corpus
probes calculate only with explicit `BLURMASK_TEST_GIAC`; their prepared routing
tests do not constitute native calculation or recognition accuracy evidence.

Bundled CLI discovery, isolated process transport, limits and GPL corresponding
sources are unchanged. There is no cloud dependency or second fallback engine.
