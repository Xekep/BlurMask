# AI pen

AI pen starts asynchronous recognition immediately when the pen is released,
passing the current ink fragment's coordinates to the
portable [InkRecognition library](../src/InkRecognition/README.md). It does not
rasterize handwriting, load MyScript DLLs, run Python, or contact a cloud service.
The previous symbol/PARSeq recognizers, weights, training tools and fixtures have
been removed. There is no fallback to them.

The optional **Experimental text engine** setting uses the bundled restored
global dictionary adapter. The setting remains disabled by default;
Russian cursive is not ready. See the [restoration status and measured limits](restored-text-engine-status.md).

The default English path loads the bundled full acoustic resource directly in C#. It uses
a bounding-box normalization heuristic, a three-layer bidirectional LSTM and
standard CTC collapse. It has no recovered lexical dictionary. English examples
and digits have been verified. The separate experimental Russian path is not ready
for general use, and automatic Russian/English language arbitration is unfinished.
The separate **Formulas** mode uses the reusable native
[InkRecognition.Math module](ink-math-engine.md) and a user-installed Math2 pack.
The default **Automatic** mode selects text or mathematical notation after the
pen release. With an installed [Raw Content2 pack](raw-content-engine.md), a separate
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
powers. With **Settings > Use Giac** enabled, a trailing `=` also evaluates
integrals, derivatives, limits, sums, products and matrix operations through
the [bundled local solver](math-solver.md).
Append another `=` to an equation to request its roots (`x^2-1=0=`); a complete
written equation without that request stays unchanged. Alternatively, select a
recognized formula and press **=** in the cursor submenu. Unsupported requests
stay unchanged. Recognition models and the solver are independent components.
Division by zero, unknown notation and inconsistent written equalities are kept
without an answer. A result extends to the right while retaining the original
height and can be undone together with recognition.
Confidence is an acoustic score, not a calibrated probability of correct text.

There is no artificial idle delay before inference. Automatic typesetting waits
for a 500 ms quiet period measured from pen release; inference runs during that
period, so a 300 ms computation leaves only 200 ms of waiting and a computation
longer than 500 ms adds no wait. Recognition is automatic; there is no separate
manual recognition button. Leaving AI-pen mode flushes the remaining fragment.
This follows the separation used by the official MyScript
[Write-to-Type example](https://github.com/MyScript/interactive-ink-additional-examples-android/blob/master/samples/write-to-type/src/main/java/com/myscript/iink/samples/writetotype/WriteToTypeManager.java):
its adjustable 500 ms commit timer is separate from recognition. This is a
sample's UX policy, not a universal engine timeout; our current inference still
re-evaluates a bounded fragment rather than restoring the SDK's incremental graph.
Model loading and computation still take time. A new pen press cancels obsolete
work and pending conversion; the next release uses the
retained original ink, including ink behind an already converted nearby word.
This lets later crossbars, dots and equals signs refine the complete fragment.
Only visible, unchanged text is eligible: manual edits, movement or deletion do
not get overwritten. Original context is bounded to 256 strokes and 20,000 points;
distant writing starts a new fragment. Undo/redo retain previous live associations.

## Model installation

Source resources for English, Russian, mathematics and Raw Content are stored in
[`models`](../models/README.md). The full English resource and experimental Russian resources are bundled.
Optional application model packs are installed separately
from the executable.
An absent optional English pack uses the bundled English resource. Invalid explicit
packs or uncertain recognition leave the original ink untouched. The
application discovers installed local packs automatically; Settings has no model
selectors or file pickers. Developer installation commands below check model
structure and checksum before replacing a working model. The pen menu shows the
currently supported language.

For formulas, a local `.ikrmath` pack is loaded automatically; choose
**Automatic** or **Formulas** in the AI pen menu. The official CPU runtime is packaged for Windows
x64, Linux x64 and macOS; model weights are supplied separately.

```sh
python tools/export-ink-math-model.py models/math2/math-sr.res --output .artifacts/math2.ikrmath
BlurMask --install-ink-math-model .artifacts/math2.ikrmath
```

The current application stores its model at:

```
<LocalApplicationData>/BlurMask/models/en-US.ikrpack
```

Convert the supported English resource from the repository with:

```sh
python tools/export-ink-model.py --resource models/en-US/en_US-ak-cur.res --output .artifacts/en-US.ikrpack
BlurMask --install-ink-model .artifacts/en-US.ikrpack
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
