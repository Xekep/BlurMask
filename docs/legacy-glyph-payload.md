# Glyph geometry payload setter and line readback

`LegacyGlyphGeometryPayload` ports the mutable value-update portion of Document
`18005b5d0`, including its concrete normalized-line readback. It accepts the
native six-float affine geometry (origin and two edge vectors), stores the four
corners in native order, and sets/removes normalized baseline/x-height metadata.
It does not reproduce Document ownership, event notifications, candidate labels
or its immutable-object copy-on-write machinery.

## Correct concrete setter dispatch

The earlier rendered-glyph analysis traced public Document `1800202d0` as far as
virtual `+0x30` but incorrectly associated that dispatch with the Ink `0xc25`
vtable. The public wrapper first requires a **Document `0xc93` element** and then
asks that wrapper for its underlying Ink type. Its return value `0xc25` does not
mean the wrapper itself has the Ink `0xc25` vtable.

The verified chain is:

```text
iink 1804d20e0 (setGlyphGeometry_)
  → Document interface 0xcab / 1800202d0
  → Document element class 0xc93, descriptor 1801fd8f0
  → vtable 1801fd9b0 +0x30 = 18005b5d0
  → underlying Ink 0xc25 glyph record / geometry payload
```

Thus Ink `1800529e0` is **not** the concrete Document setter. It is an unrelated
Ink geometry delegation. This corrects the unresolved-chain hypothesis in
`legacy-rendered-glyph-typeset.md`.

For a mutable element, `18005b5d0` stores optional normalized values at glyph
`+0x3c/+0x40`, with presence byte `+0x38`. Removing the pair clears that byte and
retains the old values. Ink `180054190` reads the presence byte; `1800541b0`
copies the stored pair and returns the presence byte. The port preserves this
behavior, including the out value when its presence return value is false.

Its geometry values `[x,y,dx1,dy1,dx2,dy2]` become:

```text
(x, y)
(x + dx1, y + dy1)
((x + dx1) + dx2, (y + dy1) + dy2)
(x + dx2, y + dy2)
```

Each addition uses single precision and native left association. The public
wrapper validates six finite geometry values and an optional finite pair with
nonnegative second value. The reusable port additionally rejects overflow in
derived corners before changing state. This explicit boundary strengthening is
not claimed to reproduce a native overflow case.

For an immutable element, the same native setter constructs a new Ink `0xc25`
record with payload length `0x68`. Existing candidates and other metadata are
carried over; the old underlying object is released. That lifetime branch is
outside this value API.

## Font sizing remains a separate boundary

The setter stores glyph geometry and normalized lines. It does not choose a font
point size. In the iink typeset editor, `1806e9890` builds a uniform affine
transform using an explicit caller scale and translation; `1806ec4a0` uses it to
place actual font-provider glyph boxes. The inspected normal upstream callers
`1806feb70`, `18085cb10` and `180b4dfc0` pass scale **1**, so their styles must
already have been prepared upstream. A final handwriting-band-to-style-font-size
policy is not yet established. Neither a bounding-box guess nor an invented
learned font-placement component is presented as a recovered native stage.

Private static evidence: `D:/BlurMask-build-artifacts-20261011/DocumentGlyphPayload`,
`InkGlyphReadbackConcrete`, `IinkFontScaleCallers` and `IinkFontSizeChain`. No vendor
DLL was executed. Binary hashes are recorded in the rendered-glyph analysis.

## Independent controls

Managed and Windows x64 NativeAOT both pass **320** own affine-basis cases with
bitwise float corner comparisons, alternating normalized-line presence, stored
pair readback after removal, and atomic nonfinite/overflow rejection.

```powershell
python tools/LegacyGlyphPayload.Probe/reference_controls.py .artifacts/glyph-payload-controls.json
dotnet build tools/LegacyGlyphPayload.Probe/LegacyGlyphPayload.Probe.csproj -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/glyph-payload-build
dotnet D:/BlurMask-build-artifacts-20261011/glyph-payload-build/bin/LegacyGlyphPayload.Probe/release/LegacyGlyphPayload.Probe.dll .artifacts/glyph-payload-controls.json
```
