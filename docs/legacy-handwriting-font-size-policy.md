# Prepared handwriting → typeset font size

`LegacyHandwritingFontSizePolicy` ports the numeric value policy in iink **180860ac0**, given explicit recognized ranges and their prepared ink geometry. It uses neither a bounding-box font-size guess nor a neural font-placement model. The evidence below establishes this particular scalar policy; it does not establish absence of other models elsewhere.

Source binary: `iink.dll`, SHA-256 `d637e88db41f00b463d0f2dc1ff9df5346ed2c86f107d57d75283671d62df2d3`. All inspection was static. The SDK DLL was not invoked by the probes.

## Proven dispatch and configuration

- **1807a0f30**, instruction **1807a3304**, queries a boolean with literal `convert.match-text-size` at **180f33f40**, then writes the result to configuration **+283**. This is exactly the gate read by **180860ac0**.
- The original style scalar is internal Style **+1c**. **18044fa00** maps it into external `graphics.Style` **+6c**; exported **1803270c0** `getFontSize` reads +6c and **1803270a0** `setFontSize` writes it. Its meaning is font size, rather than an arbitrary line-height field.
- The value helper is at vtable **180f38de0+f8**. **180783760** and **180763520** call it through slot f8, read its output float **+4**, and pass that float to **180869370**, which updates cached typesetting size **+1e8** and marks geometry dirty. This connects the policy to runtime conversion.
- The caller also receives a boolean based on the pre-operation segment condition. The library takes `canUsePreparedSource` explicitly for the value helper's gate (post-operation segment condition false and a non-null prepared source). It does not reproduce segment mutation or document ownership.

`font-size` uses **millimeters/model units** in iink, as specified by the official [styling reference](https://developer.myscript.com/docs/interactive-ink/3.1/reference/styling/). All geometry and rendered-reference metrics must use those same model units. The returned value is not a typographic point size. A display adapter must perform its own explicit model-unit conversion.

## Prepared range sampling

Each range supplies its first decoded code point, character count, first prepared character's x-height, word left/right coordinates and native range flags. The original range enumerator is **1807035a0/1807035c0**; character decoding is **1806b4930 → 180645d30**. The port accepts those prepared inputs instead of claiming to reproduce their upstream recognizer.

Ranges with flag bit 10 or 21 are skipped. The first code point is filtered by the original masks and exact intervals for punctuation, spaces and currency symbols; the port preserves these intervals rather than using generalized Unicode categories. Cyrillic letters pass this filter.

Let `n` be range character count, `h` its first character's prepared x-height, and `w = right − left`:

- Single-character ranges and the native fullwidth/CJK/Hangul/high-surrogate intervals contribute `h`.
- Other two-character ranges contribute `(h + w/n)/n`.
- Other longer ranges contribute `w/n`.

**1806b4cd0** obtains the source x-height from prepared character record **+1c**, via **1804c2660**. Independently, JIIX exporter **1806db5a0** reads that same field and emits the literal key `x-height`, proving the field's semantics.

The policy averages contributions **per eligible range**, not per character. If the rendered font's literal `x` height is positive, it computes:

```text
matched size = (original font size / rendered 'x' height) × mean prepared contribution
```

Rendered `x` is obtained through FontMetricsWrapper slot **+40**, helper **180450590**. The port requires the actual reference value returned by the adapter; it does not synthesize font metrics. If there are no eligible ranges, the original size is retained. A nonpositive rendered reference leaves the original text size unchanged.

## Embedded math, quantization and dirty tolerance

After at least one eligible text range, native **180833b30** supplies optional embedded math pairs `(character count, font size)`. Nonzero sizes contribute an unweighted mean across math items; that mean is then weighted by the **total math character count** against the matched text size weighted by **total eligible text character count**. The library accepts the prepared pairs. The math producer's **180a51960** calculation is still outside this bounded stage.

Caller **180783760** can additionally apply **18085b150** quantization using caller-provided reference size and relative step. Below the reference, sizes round to quarter-reference steps with a minimum of one quarter. At/above the reference, the index is `round_away((size/reference − 1)/relativeStep)` and the result is `(index × relativeStep + 1) × reference`. PE import resolution verifies `roundf` and `lroundf`; tie behavior is away from zero. Reference size and relative step are explicit inputs; their configuration producer has not been ported.

Cached setter **180869370** marks geometry dirty only when the size difference is at least `max(0.002, max(abs(old), abs(new)) × 1e−7)`. `RequiresSizeUpdate` preserves that threshold, including equality.

The numerical helper strengthens input validation for finite values, invalid prepared ranges and arithmetic overflow. These rejection paths are deliberate library hardening, not claimed vendor behavior. The main policy does not estimate the baseline or word origin. Those are distinct prepared-line and glyph-placement stages.

## Independent controls

`tools/handwriting/create_font_size_controls.py` produces deterministic own-input controls with NumPy float32 arithmetic and explicit Unicode interval sets. The managed and Windows NativeAOT probes both passed **684** complete prepared-size cases, **1388** Unicode branch controls, **864** quantization cases, and **120** cached-setter tolerance cases. Numeric outputs are checked bitwise. Controls cover Latin/Cyrillic/Asian ranges, punctuation exclusion, native flags, both gates, zero reference height, zero/absent math items, branch boundaries, quarter-step ties and finite/overflow rejection.

```powershell
python tools/handwriting/create_font_size_controls.py .artifacts/font-size-controls.json
dotnet run --project tools/LegacyHandwritingFontSize.Probe -- .artifacts/font-size-controls.json
```

This is a proven executable prepared-input font-size policy. Full automatic grouping, preparation of its source geometry, embedded-math sizing and final baseline placement must remain separately identified until their complete callers/producers are recovered and tested.

## Desktop display adapter

`TextRecognition.FontSizeSamples` can carry actual prepared recognized-range geometry, expressed in the same world-coordinate units as the input ink. `AiInkLayout` copies it into an owned row snapshot; `AiTypesetLayout` calls the recovered policy with the real embedded font's rendered `x` height in those same units. It does not use minimum or maximum character height. Input box geometry is never substituted for a prepared character's x-height. The currently used CTC recognizer does not supply this metadata.

Without prepared metadata, the desktop adapter uses an explicitly original fallback for rows with multiple separate handwriting components. It removes flat crossbars and very small detached marks, estimates ordinary body height from a lower height quantile, and aligns to the median component bottom. It measures `x` for text containing lowercase letters and `H` otherwise. This prevents one tall component from enlarging the whole row, while a dot does not shrink it. A single connected word still uses the existing whole-word metric fallback until its decoder supplies prepared range geometry. This is not a claim that native handwriting segmentation or baseline placement has been ported.

Structural math retains its formula metrics, including fraction and integral heights. Nearby converted text can still share its size and baseline; persisted text keeps its natural glyph aspect ratio. Offscreen production-font checks cover an isolated tall component, punctuation/crossbars, world-coordinate scaling, the prepared-range average, formula isolation and peer serialization:

```powershell
dotnet run --project tools/BlurMask.RenderProbe -c Release -- --typeset-only true
```
