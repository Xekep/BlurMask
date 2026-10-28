# Independent numerical reference (test tooling only)

`dtoa.c` is an unmodified copy fetched on 2026-10-28 from
<https://netlib.org/fp/dtoa.c>, SHA256
`ef79323711f162150ec377774320049f10733bd9bbdddcd4f1583e8c6ec25249`.
David M. Gay's copyright, permission and disclaimer are retained at the top of
the file. Preserve the complete notice when copying this reference.

This code is compiled only into a standalone test driver. It is not linked into
BlurMask or its independent managed recognition libraries and is not a MyScript
binary. `NO_BF96` selects the older floating quick path/bigint architecture for
comparison; modes2 and6 are compared for every tuple. The original vendor's exact
source revision is not identified by this copy. Agreement is an independent
numerical check, not an assertion that the vendor runtime was executed.

Reproduce from the repository root (the output directory must be new):

```powershell
python -B tools/Math2DomValue.Probe/finite_controls.py .artifacts/finite.json
python -B tools/Math2DomValue.Probe/reference/run_reference.py .artifacts/finite.json .artifacts/dtoa-reference
```

The managed generator is an independent exact-integer implementation. Its
oracle uses Python Decimal rounding, and this C reference provides a second
implementation with floating quick paths and digit-at-a-time bigint conversion.
