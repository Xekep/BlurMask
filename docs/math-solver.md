# Local symbolic mathematics

The reusable `src/InkMath.Solver` module talks to the native C++ Giac/Xcas CLI in
an isolated process. It has no dependency on Avalonia, recognition models,
Python, a web account, or a cloud API. It is compatible with Native AOT.
Finite definite polynomial integrals also have a built-in exact rational path;
for example, `\int_{1}^{10} x^2 dx=` produces `333` without an installed CAS.
This path supports rational/decimal numeric bounds, polynomial degree at most32,
and sums/products/powers/division by nonzero rational constants. Trigonometric
functions, symbolic bounds and singular rational integrands still require Giac.
Windows x64 (installer and portable ZIP), Linux x64 (portable archive, Debian,
Arch and Void/glibc packages) and native macOS x64/arm64 builds include a
separately executed Giac CLI. GMP/MPFR are linked statically; Windows needs no
additional solver DLLs, and Linux/macOS use only operating-system libraries.
There is no Xcas graphical interface. Licenses and the corresponding-source
archive hash are installed beside the solver; exact sources and build recipes
are published with the release. [Build details and provenance](giac-runtime-build.md).

## Enable it

Enable **Settings > Use Giac**; no separate installation or path selection is
required for the bundled platforms. The switch is off by default. Built-in
numeric calculations and exact polynomial definite integrals work with it off.
Settings has no path selectors or download buttons. On other platforms, standard
installed CLI paths are discovered automatically. A custom developer installation
can select `icas.exe` / `icas` / `giac`, rather than the graphical `xcas` executable:

```text
BlurMask --set-math-solver /absolute/path/to/icas
```

The command verifies `1+1`; bundled native solvers take precedence. No Python
runtime, web account or external service is needed for calculations.

## Requests and supported notation

AI pen preserves notation until an explicit calculation request: append `=`.
A simple numeric expression uses the existing fast numeric evaluator first.
For a complete equation, append another `=` or select it and use **=** in the
cursor's selection submenu. A written right-hand side is never auto-overwritten.
Automatic routing calculates only after accepting the OCR candidate or a complete
raw-content region. Uncertain or rejected Math candidates do not start Giac.
Accepted numeric requests classified as text still use the fast numeric evaluator.
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
typesetting tests under `BlurMask --ai-pen-test`, without starting any GUI.
Native application CI builds the bundled CLI from pinned sources on Windows,
Linux and macOS, then checks automatic discovery, the default-off toggle and
calculus/algebra results. Package tests run `BlurMask --math-solver-test` after
installation and reinstallation of Windows, Debian, Arch and Void packages.
The legacy upstream CLI remains a separate regression control. Source-build
transport tests and complete license/source manifests accompany every payload.
All application solver checks are headless and do not open application windows.

The 2026-10-12 integral update passed 31 grammar translations and 20 built-in
exact integral cases, including rational/decimal bounds, reversed bounds,
zero-length intervals and varied differential notation. All 20 exact results are
also compared against independent Giac calculations (decimal literals converted
to equivalent fractions because Giac uses floating point for those literals).
The managed and Windows NativeAOT probes exercise 29 actual Giac cases, including improper
integrals, mixed partial derivatives, explicit log bases and plain recognized
`sin^{2}(x)+cos^{2}(x)`. The headless application probe additionally exercised
result typesetting, trailing-equals policy, rejection before CAS, complete-region
coverage and written-RHS preservation.

This is solver verification, not proof that all those expressions are recognized
accurately from handwriting. A separate [actual handwriting corpus audit](math-ocr-corpus-audit.md)
checks Math2/ORT 1.23.2 on fourteen independently labeled trajectories covering
integrals, derivatives, matrices, limits and a quadratic-root expression. Ten
match their labels after limited notation normalization; four contain genuine
high-confidence symbol errors. Managed and Windows NativeAOT results agree.
The application accepts fourteen direct mathematical candidates and automatically
routes thirteen; one correctly decoded integral stays handwritten because the
raw-content classifier falls below the confidence gate. A larger held-out corpus
and recovered native search/context policies are still needed. Synthetic
structural-token tests verify formatting only.
