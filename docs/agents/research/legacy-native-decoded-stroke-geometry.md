# Real source points to prepared decoded-line geometry

This batch closes the executable ordinary raw-stroke branch between the native
source-position range port and the prepared character-line catalog/IRLS fit.
Every symbol boundary is computed from its assigned source points, including
fractional endpoints. No whole-word bounding rectangle is supplied to this API.

## Proven native geometry branch

Ink C20 table**0e5050** virtual90 resolves to **06bcb0**, which forwards selected
source fields to **06b7f0**. The fresh ordinary branch returns the original
source object for a complete selection, or creates a partial point view through
default C1E.virtual8 **0914e0**. That function creates:

- **C4A**, initialized by **08e750**, for integer first/last points;
- **C4B**, initialized by **08eb50**, when a fractional endpoint exists;
- C4C for an explicitly supplied transformation, which this raw owner does
  not support or default to an identity transform.

`08eb50` computes endpoint channel values using separate float32 operations
`(next-current)*fraction + current`. Its C4B XY reader **08f400** emits a
synthetic first point when required, underlying integer points, and a synthetic
last point when required. A nonzero first fraction replaces the first integer
point; a nonzero last fraction adds a terminal point. This also covers a
fractional interval entirely between two adjacent points.

The default context initializer **001910** binds context+0 from classC1E
descriptor**0e6c60**, table**0e6980**. Its virtual48 is **092e90**, which sets
max/min float32 sentinels and invokes **0629e0**. That routine obtains actual
C34 XY points, directly or in batches of20, and scans their minima/maxima.
Global C21.virtual178 **083630** obtains selected descriptors through081ed0,
resolves each selected source object through the owner's virtual90, and unions
its bounds. The point scanner uses strict comparisons; the aggregate scanner
uses its explicit non-strict minimum comparisons. Signed zero can therefore
depend on order, and the port preserves that behavior.

The C20 existing partial-view cache and known-text branches are real additional
native paths, not covered by this fresh raw owner. The port cannot substitute
an ordinary polyline for a C25 known-text object or silently apply cached
transformations.

## Reusable runtime API

`LegacyNativeRawStrokeGeometry` captures and owns indexed `InkStroke` point
arrays. It validates finite XY coordinates, nonempty sources, at most65536
points per object and at most1048576 total captured points. Point counts come
from the captured data; no caller glyph rectangle or expected character text
is involved.

`CreatePositionSnapshot` composes the actual081ed0 selector and00f130 C79
position producer from that same source owner. A weak owner association keeps
each live position snapshot bound to its captured raw source. Passing positions
from another source owner is rejected, even when array sizes happen to match.

`ReadSelectedPoints` returns the integer/fractional native point view as owned
values. `ReadRangeBounds` follows selected source ranges and unions these actual
point bounds. `ReadDecodedItemBounds` consumes prepared decoded item-index and
position-end records through the existing046ae0 interval port. Unassigned/empty
items retain the native max/min empty-bound sentinels, rather than receiving
invented dimensions. Interpolation overflow is rejected explicitly.

`LegacyNativeDecodedStrokeLine.Prepare` connects this point-derived geometry to
the restored0531b0 category item template and C1A primary fit for one explicitly
prepared source line. Resolved alphabet labels and endpoints must come from
the actual decoder. Empty-source items are excluded by this prepared adapter;
it does not recover native source-line allocation, paragraph grouping, cached
alternatives or acceptance/fallback policy. The template starts fresh for this
single-line adapter; multi-line template carry remains a separate lifecycle.

`CreateAcceptedSourceLineMetrics` then transports an explicitly accepted,
positive finite primary fit through the existing source2DA double metric,
result2DB owner and SDK float metric port. Actual derived character source-line
IDs remain required, all0 for this one-line owner. No fit acceptance confidence
is invented. The caller must resolve native fallback behavior if the primary
fit is absent.

## Verification

```powershell
python tools/LegacyRawStrokeGeometry.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/raw-stroke-geometry-controls.json
dotnet run --project tools/LegacyRawStrokeGeometry.Probe/LegacyRawStrokeGeometry.Probe.csproj -c Release -- D:/BlurMask-build-artifacts-20261011/raw-stroke-geometry-controls.json models/line-geometry/character-line-categories.json
dotnet publish tools/LegacyRawStrokeGeometry.Probe/LegacyRawStrokeGeometry.Probe.csproj -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/raw-stroke-geometry-aot
D:/BlurMask-build-artifacts-20261011/raw-stroke-geometry-aot/LegacyRawStrokeGeometry.Probe.exe D:/BlurMask-build-artifacts-20261011/raw-stroke-geometry-controls.json models/line-geometry/character-line-categories.json
```

The original Python oracle generates full expected XY point views, source
range selections and bounds from source points, independently of the C# port.
It reuses the previously independent position oracle for byte/range semantics.
Managed and NativeAOT checks cover256 float32 point views,96 captured source
owners,8922 cropped-bound intervals, signed-zero order, fractional endpoint
rounding, ownership, foreign-owner rejection, malformed inputs and cancellation.

Another256 cases compose **one connected physical polyline** with six prepared
symbol endpoints through actual point bounds, the committed checksum-verified
Latin/Russian category catalog, IRLS, source/result owner binding, SDK narrowing
and font policy. Capital and descender regions have twice the body height,
while the fitted line x-height remains the body height. These are independent
prepared geometry controls, **not handwritten OCR accuracy results**: their
alphabet labels/endpoints are explicit test inputs, not generated by a model.

No vendor DLL, SDK or GUI is executed. Read-only private exports are under
`InkC20SelectedObjectResolutionNext`, `InkSourceRangeBoundsNext`, and the earlier
`InkC79*` directories;08eb50/08f400/0629e0 also exist in the saved full Ink export.

## Remaining production boundary

The current CTC application provider still does not emit this native per-symbol
position/label metadata. Modern EED's real winning endpoint producer and an
adapter from the committed old Russian247/135 resource family remain
unproved. The legacy273 result/source bridge is another path under investigation.
This reusable consumer is ready for a real winner provider; its tests do not
mean the application's connected-word font fallback has already been replaced.
