# English-lite full24C preprocessing

`LegacyOwnedEnglishLiteFullPreprocessing.Prepare(model, rawRow)` is a NEW
same-selected119/source247 preprocessing factory. It restores the positive24C
branch after24A and before265. The preceding strict short-branch API is unchanged;
no old reader guards, Russian profile or captured golden is widened.

The factory resolves ordered24A/24C/265 from the retained actual source owner,
requires the proved ordinary135/lite/no-early-processor branch, constructs a fresh
`LegacyFreshOrdinaryRawPacket`, and checks its one kind0 descriptor covers all
physical strokes. It does not accept supplied assignment flags or a surrogate
Russian frame. Original metrics/duplicate suppression/point identities come from
that packet. `RawDescriptor` is explicitly the ingress snapshot **before**24A
assigns descriptor+10; it is not a postprocessor descriptor snapshot.

After actual English24A estimation/deskew, the source-bound24C profile is passed
to `LegacyPreparedEnglishFirstSlant`. That class is separately named prepared:
its standalone input points do not acquire source-preprocessing provenance merely
because the angle profile belongs to a source. The full factory establishes
that provenance with its packet, model and ordered operations.

The scalar kernel uses the exact direction gate, float dx/dy, dy-negative sign
flip, inclusive radian limits, float squared length widened to double sqrt, and
double length-weighted angle accumulation. Zero accepted length selects angle0.
Its single descriptor's bbox spans all physical strokes, including physical
pen lifts. It applies the native affine matrix with literal yx=0 and float
coefficients/centers, without writing a guide.265 therefore consumes the same
24A guide height and original-source metrics while processing the changed XY.
Factor200 keys are projected back through the original packet chronology.
See [the50-pin static proof](legacy-english-positive24c-plan.md).

`System.Math.Atan2`, `Sin` and `Cos` are an explicit portable transcendental
adaptation of native CPU-dispatched math entries. Neither the implementation nor
the tests claim universal bit equality to every vendor CPU path. All supported
input/output snapshots are privately held; public geometry/key/count accessors
return copies. Prepared inputs retain the portable1..256 strokes/4096 points cap;
nonfinite coordinates, squared-length/center overflow and nonfinite output are
explicit portable refusals. Native huge/overflow behavior is not recreated.

The dedicated probe records121 real raw-source invocations, including28 taking
positive24C,4204 final prepared points and93 old-short-path bit comparisons.198
separate prepared scalar controls cover19/20/21, equality/flat runs, dy-negative
flip, horizontal segments, physical-range spanning, nonzero centers, zero
accepted length, neighboring float bounds and exact0-angle inclusive bounds.
It also checks foreign child ownership, wrong processor, immutable public
copies, cancellation and finite/budget refusals. These are geometry and ownership
checks, not recognition-quality results or a fabricated native word winner.

The independent audit parses raw resource envelopes/profiles and recomputes
24A with a Gaussian normal-system solver, float deskew, positive24C scalar trace,
then dense NumPy spline265, every factor200 key and raw source identity. At each
separately checked float boundary it uses the recorded float values to avoid
claiming universal IRLS solver bit parity. Both weighted-angle traces and complete
Managed/Native captures are checked; existing short capture remains unchanged.

No source feature normalization, pen-up merge,217/199 integer feature producer,
219/201 boundary-appended vector, native activation constructor, NN, graph,
controller or application enable is joined by this stage. Full source/event,
kind6 and multiple-descriptor mutation semantics are not advertised by the
single fresh-kind0 kernel. English recognition remains unfinished.

```powershell
python tools/LegacyEnglishFirstSlant.Probe/reference_controls.py E:/BlurMask-build-artifacts-20261014/enlite-preprocessing-controls.json E:/BlurMask-build-artifacts-20261014/english-first-slant-controls.json
dotnet run --project tools/LegacyEnglishFirstSlant.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/english-first-slant-managed-artifacts -- models E:/BlurMask-build-artifacts-20261014/english-first-slant-controls.json E:/BlurMask-build-artifacts-20261014/english-first-slant-managed.json
python tools/LegacyEnglishFirstSlant.Probe/audit_output.py models E:/BlurMask-build-artifacts-20261014/english-first-slant-managed.json
```
