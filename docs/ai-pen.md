# AI pen

AI pen batches strokes after an 800 ms pause and passes their coordinates to the
portable [InkRecognition library](../src/InkRecognition/README.md). It does not
rasterize handwriting, load MyScript DLLs, run Python, or contact a cloud service.
The previous symbol/PARSeq recognizers, weights, training tools and fixtures have
been removed. There is no fallback to them.

This first integration supports an experimental English acoustic model. It uses
a bounding-box normalization heuristic, a three-layer bidirectional LSTM and
standard CTC collapse. It has no recovered lexical dictionary. English examples
and digits have been verified. Russian raw-stroke recognition is not ready.
The separate **Formulas** mode uses the reusable native
[InkRecognition.Math module](ink-math-engine.md) and a user-installed Math2 pack.
The default **Automatic** mode selects text or mathematical notation after the
pause. With an installed [Raw Content2 pack](raw-content-engine.md), a separate
local neural analyzer first partitions text and formula regions in a mixed batch;
drawings and uncertain regions stay handwritten. Each region uses its own decoder.
Without this optional pack, confident natural text avoids loading the formula
engine; numeric/operator input and uncertain text can use the Math2 model.
**Text** and **Formulas** remain explicit overrides. The analyzer does not add
Russian handwriting recognition, and the recovered pipeline is experimental.

Numeric arithmetic appends an answer only after a trailing equals sign:
`2+2` stays `2+2`, while `2+2=` becomes `2+2=4`. A written right-hand side is
preserved, including an incorrect answer. Supported input
includes explicit arithmetic, decimal comma, parentheses, fractions, roots and
powers. It does not solve symbolic equations, evaluate integrals or execute code.
Division by zero, unknown notation and inconsistent written equalities are kept
without an answer. A result extends to the right while retaining the original
height and can be undone together with recognition.
Confidence is an acoustic score, not a calibrated probability of correct text.

## Model installation

Model weights are separate from the source and are not bundled with installers.
An absent, invalid or uncertain model leaves the original ink untouched. Settings
shows the model status and offers **Install AI pen model** for a `.ikrpack` file.
Installation checks the model structure and checksum before replacing a working
model. The pen menu shows the currently supported language.

For formulas, install a `.ikrmath` pack with **Install formula model**, then choose
**Automatic** or **Formulas** in the AI pen menu. The official CPU runtime is packaged for Windows
x64, Linux x64 and macOS; model weights are supplied separately.

```sh
python tools/export-ink-math-model.py /private/path/math-sr.res --output /private/path/math2.ikrmath
BlurMask --install-ink-math-model /private/path/math2.ikrmath
```

The current application stores its model at:

```
<LocalApplicationData>/BlurMask/models/en-US.ikrpack
```

For local research, convert an existing supported English resource with:

```sh
python tools/export-ink-model.py --resource /path/to/en_US-ak-cur.res --output /private/path/en-US.ikrpack
BlurMask --install-ink-model /private/path/en-US.ikrpack
```

The exporter does not download, activate or distribute vendor assets. Exported
files are ignored by Git. See its help for the input file and output arguments.

## Layout and editing

Completed words in a row share one font size and baseline. Spatial gaps
between words become spaces; original pen chronology is preserved when passing
each word to inference. Printed text uses Inter. Color, position, disappearing-ink
duration and shared-document identity are retained. Undo restores the original
strokes. Starting another stroke invalidates a pending result.

After inference, the UI measures the embedded font's actual glyph geometry and
matches the handwriting height using uniform scaling. Close sizes are grouped in
1.25 steps, and nearby converted words can supply the row size and baseline.
Natural glyph widths replace the old independent horizontal/vertical stretching.
Computed math answers use the original expression for size estimation and extend
to the right at the same scale. This follows the documented MyScript diagram
size-matching principle, not its proprietary internal layout algorithm.
See [MyScript whiteboard sizing](https://developer.myscript.com/doc/interactive-ink/3.0/android/advanced/interactive-white-board/index.html)
and [font/style reference](https://developer.myscript.com/doc/interactive-ink/4.1/reference/styling/).

Formula mode keeps each expression together, including fractions, radicals,
superscripts and matrices. Its bounded native geometry renderer uses STIX Two
Math and STIX Two Text Italic for variables. Regular text uses Inter. The fonts
are embedded and automatic; settings contains no font selector. Unsupported
structures, incomplete decoding and low-confidence results remain handwritten.
The font defaults follow the [MyScript styling reference](https://developer.myscript.com/doc/interactive-ink/3.2/reference/styling/).

The application keeps the entire handwritten row if any word is uncertain.
Unsupported writing may remain ink or be misrecognized; the first-stage engine
is explicitly experimental.

## Verification

```sh
dotnet run --project tools/InkRecognition.Probe -c Release
dotnet src/BlurMask/bin/Release/net11.0/BlurMask.dll --ai-pen-test
dotnet run --project tools/InkRecognition.Probe -c Release -- model.ikrpack strokes.json
```

The portable checks use original synthetic weights, independent numerical
goldens, malformed model data, cancellation and invalid coordinate cases. App
checks verify row grouping, uncertainty, undo/redo and preservation of peer ink.
They run without desktop windows, including in Native AOT CI. Vendor weights and
recorded vendor examples are not required for the public CI checks.
