# Prepared float ink to normalized signed shorts

`LegacyPreparedGlyphNormalization.NormalizeOrderedRanges` restores one checked
branch of native`1b4e40`: source flags8/9 areFF/FF and the stored target height
is positive. In the committed Russian type21d prefix, bytes189750/189751 are
FF/FF and the ushort at189752 is100. These flags force the two general
translation/rescaling conditions off and select the special branch.

Native instructions at`1b552a..1b5578` establish the arithmetic directly:

```text
x = truncate(((preparedX - candidateMinimumX) * targetHeight) / sourceLineHeight)
y = truncate(((preparedY - sourceBaseline)   * targetHeight) / sourceLineHeight)
normalized baseline = 0; normalized line height = targetHeight
```

Subtract, multiply and divide are separately rounded float instructions.
`cvttss2si` truncates toward zero; there is no rounding of raw fractional
coordinates before scaling. Native clamps values above32767 and passes the
low16 bits of the converted integer to the short-point container. This bounded
implementation rejects nonfinite results or values below−32768 instead of
reproducing out-of-domain negative wrapping. For example source(10.25,49.75)
with minimumX10, baseline50,height40,target100 becomes(0,0), while rounding
source geometry or rounding normalized values would produce another point.

The explicit input frame is intentional. Native source line resolution reads
line geometry through its source getter+40 and can merge a preceding line's
baseline/height. That upstream policy is not inferred from a bounding box.
Candidate minimumX, source baseline/height and target height remain supplied.

The caller also supplies the confirmed maximum range count before native
post-normalization pen-up merge. The actual single Russian profile's first
short among its five geometry shorts is30 at native profile+1c;`1b4bd0` uses
this divided by3, giving10 when the profile is enabled. The API accepts a
caller-confirmed bound1..15 and rejects counts beyond it. It does not implement
the native post-pass that removes selected pen-up boundaries above that bound.

Range chronology also stays explicit. Native`1b4e40` obtains source metadata
keys and sorts through`1b49a0`; the adapter preserves the supplied range order,
with one output stroke per range. It does not assume all possible source
metadata are interchangeable with physical stroke indices. Prepared inserted
points are read in place and remain unchanged; output retains the exact
prepared source object and selected range provenance.

```powershell
python tools/LegacyPreparedNormalization.Probe/reference_controls.py .artifacts/myscript-analysis/output/RussianAssetsSearch/legacy-prepared-normalization-controls.json
dotnet run --project tools/LegacyPreparedNormalization.Probe -c Release -- --controls .artifacts/myscript-analysis/output/RussianAssetsSearch/legacy-prepared-normalization-controls.json
dotnet run --project tools/LegacyPreparedNormalization.Probe -c Release -- --resource models/ru-RU/ru_RU-ak-cur.lite.res
```

768 independent Python float32 controls cover selected subranges, supplied
order, frame changes and upper clamp. Own fixtures verify fractional insertion,
negative truncation, consistent scale/translation, normalized baseline,
ownership, lower-range rejection, point/range bounds and cancellation. Actual
resource validation checks the FF/FF,target100 branch. These controls validate
normalization arithmetic and transport, not automatic line resolution, native
full search, Russian word accuracy or cursive GUI readiness.
