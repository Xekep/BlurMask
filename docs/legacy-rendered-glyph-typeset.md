# Rendered-font glyph geometry metadata

`LegacyGlyphTypesetGeometryBuilder` ports another bounded original stage, now
from **iink.dll**, rather than the Text recognizer's baseline estimator. Native
`1806e8660` and `1806ec4a0` consume actual external rendered-font glyph metrics,
query reference character **`x`** height, and construct geometry metadata for
converted glyphs. No handwriting bounding-box estimate is substituted for font
metrics in this API.

The statically inspected iink binary is SHA-256
`d637e88db41f00b463d0f2dc1ff9df5346ed2c86f107d57d75283671d62df2d3`.
Focused private Ghidra analysis is stored under
`D:/BlurMask-build-artifacts-20261011/IinkFontProject`, with function exports in
`IinkFontExport`, `IinkFontMatch` and `IinkFontScalar`. This analysis imports and
decompiles files; it never executes original vendor DLLs.

The external provider dependency is also part of the public API: its
[font metrics interface](https://developer.myscript.com/refguides/interactive-ink/android/3.1/com/myscript/iink/text/IFontMetricsProvider.html)
supplies rendered glyph bounds and font pixel size, while
[GlyphMetrics](https://developer.myscript.com/refguides/interactive-ink/android/2.0/com/myscript/iink/text/GlyphMetrics.html)
exposes left and right side bearings.

## Recovered operations

Native `180450590` builds a one-character text/span, requests its rendered bounds,
and returns the first positive height. With no returned box or nonpositive height
it uses internal Style **+0x20**, the font line-height field. Native `18044fa00`
maps +0x20 to external `graphics.Style` +0x68; exported `getFontLineHeight`
at `180327090` reads +0x68. This corrects the earlier font-size interpretation:
the actual font-size field is internal +0x1c, external +0x6c. The recovered callers at `1806e8df4` and
`1806ecdc4` pass literal `0x78` (`x`) to this character-height helper. The port's
`ReferenceCharacterHeight` implements the proven scalar result selection, with
actual measured height and style font line height supplied explicitly. The port
retains the original scalar selection without adding a font-size multiplication.

Native `180450d20` copies six provider glyph values: box X/Y/width/height and two
bearings. Negative provider width or height is clamped to zero; bearings are
preserved. `SanitizeProviderMetrics` implements this operation, rejecting
nonfinite values as a public input boundary.

After native transformation and placement of the glyph box, both concrete
typeset functions construct these two normalized values:

```text
baselineFraction = (baselineY - preparedGlyphBoxY) / preparedGlyphBoxHeight
xHeightFraction = (renderedReferenceXHeight / preparedGlyphBoxHeight) * horizontalScale
```

The first function also constructs side-bearing endpoints using the **original
provider glyph width**, rather than the transformed prepared box width:

```text
leftBearingFraction = providerLeftSideBearing / providerGlyphWidth
rightBearingEndpointFraction = providerRightSideBearing / providerGlyphWidth + 1
```

If both bearings are zero, the native endpoints are exactly `[0,1]` without
division. `NormalizePrepared` reproduces those scalar branches. It accepts the
already transformed, axis-aligned glyph box explicitly; renderer/projection
preparation is not silently guessed. Zero-height normalization and nonzero
bearings on zero source width are rejected. The native public geometry setter
likewise checks finite geometry and nonnegative x-height fraction.

## Setter chain and remaining boundary

The second concrete caller passes the box and the two fractions to iink wrapper
`1804d20e0`, whose recovered operation name is `setGlyphGeometry_`. It calls
Document public interface `0xcab`, table `1801b6d58`, slot `+0x10` (`1800202d0`).
That wrapper requires concrete object type `0xc25`, dispatching virtual `+0x30`.
The implementation resides in MyScriptInk: class descriptor `1800e4360`, object
table `1800e43b0`, method `1800529e0`. That method delegates again to the payload
object at glyph `+0x18`, virtual slot `+0x30`; the payload's final implementation
is still unresolved.

This port establishes rendered glyph ratios and their concrete typeset call
chain. It does **not** establish the final policy that converts recognized ink
height into font point size, nor the full payload geometry implementation. The
inspected scalar helpers depend on external font metrics and contain no neural
inference, but this does not prove absence of a neural component elsewhere in
the complete engine.

## Controls

Managed and Windows x64 NativeAOT pass 240 independently generated rendered-glyph
cases, including provider sanitization, reference-height fallback, nonzero and
zero bearings, explicit prepared-box preservation, and degenerate/nonfinite
rejection. The Python oracle independently inverts a two-coordinate affine basis
to verify baseline ratios and reconstructs reference height from its normalized
ratio. No vendor model or executable supplies the control outputs.

```powershell
python tools/LegacyGlyphTypesetGeometry.Probe/reference_controls.py .artifacts/glyph-typeset-controls.json
dotnet build tools/LegacyGlyphTypesetGeometry.Probe/LegacyGlyphTypesetGeometry.Probe.csproj -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/glyph-typeset-build
dotnet D:/BlurMask-build-artifacts-20261011/glyph-typeset-build/bin/LegacyGlyphTypesetGeometry.Probe/release/LegacyGlyphTypesetGeometry.Probe.dll .artifacts/glyph-typeset-controls.json
```
