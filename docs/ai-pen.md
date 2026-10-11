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
pause. Confident natural text avoids loading the formula engine; numeric/operator
input and uncertain text can use the Math2 model. **Text** and **Formulas** remain
explicit overrides. Auto uses a conservative confidence guard, not a universal
classifier, and ambiguous input stays handwritten.

Numeric arithmetic now appends an answer: `2+2` becomes `2+2=4`. Supported input
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

Completed words in a row share one font size and bounding rectangle. Spatial gaps
between words become spaces; original pen chronology is preserved when passing
each word to inference. Printed text uses Inter. Color, position, disappearing-ink
duration and shared-document identity are retained. Undo restores the original
strokes. Starting another stroke invalidates a pending result.

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
