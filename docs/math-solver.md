# Local symbolic mathematics

The reusable `src/InkMath.Solver` module talks to the native C++ Giac/Xcas CLI in
an isolated process. It has no dependency on Avalonia, recognition models,
Python, a web account, or a cloud API. It is compatible with Native AOT.
Giac is an optional, separately installed engine; no Giac binary is redistributed
inside BlurMask. This keeps ordinary drawing installations small. Giac's GPL
license and dependencies are handled by the upstream installer/package manager.

## Enable it

Install [Giac/Xcas from upstream](https://xcas.univ-grenoble-alpes.fr/backup/install_en).
Windows normally installs `C:\xcaswin\icas.exe`; macOS upstream documents
`/Applications/usr/bin/icas`. Standard installed CLI paths are discovered
automatically. For another location, open **Settings → Connect Giac solver** and
select `icas.exe` / `icas` / `giac`, not the graphical `xcas` executable. The
connection is tested with `1+1` before being saved. A headless equivalent is:

```text
BlurMask --set-math-solver /absolute/path/to/icas
```

Linux packages: `sudo apt install --no-install-recommends xcas` on Debian/Ubuntu,
`sudo pacman -S giac` on Arch, or `sudo xbps-install -S giac` on Void. Arch marks
Giac as an optional dependency and the Debian package suggests Xcas. Void users
install Giac separately. No Python runtime is needed for calculations.

## Requests and supported notation

AI pen preserves notation until an explicit calculation request: append `=`.
A simple numeric expression uses the existing fast numeric evaluator first.
For a complete equation, append another `=` or select it and use **=** in the
cursor's selection submenu. A written right-hand side is never auto-overwritten.
Automatic routing calculates only after accepting the OCR candidate or a complete
raw-content region. Uncertain or rejected Math candidates do not start Giac.
Accepted numeric requests classified as text still use the fast numeric evaluator.
The result participates in document undo/redo and ordinary canvas synchronization.

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
picker verifies a calculation, not authenticity; use upstream or OS packages.

## Verification

`tools/InkMath.Solver.Probe` checks parsing, forbidden commands, bounded
complexity, actual calculus/algebra results and cancellation of a live native
process. Pass the absolute CLI path to exercise Giac. Without it, only grammar
tests run. `BLURMASK_TEST_GIAC` enables real CAS → application equals policy and
typesetting tests under `BlurMask --ai-pen-test`, without starting any GUI.
CI exercises actual Windows and Ubuntu CLIs with a Native AOT probe; macOS builds
the adapter and runs portable tests, but actual macOS Giac execution is not yet
validated. The Windows test helper verifies the pinned upstream installer's hash,
extracts only the CLI/dependencies/license with 7-Zip and never executes the
installer or uploads the third-party runtime as an application artifact.

The 2026-10-11 audit passed the managed and Windows NativeAOT probes with 24
grammar translations and 25 actual Giac calculus/algebra cases, including improper
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
