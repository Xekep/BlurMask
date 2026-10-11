# AI pen

AI pen batches strokes after an 800 ms pause and passes their coordinates to the
portable [InkRecognition library](../src/InkRecognition/README.md). It does not
rasterize handwriting, load MyScript DLLs, run Python, or contact a cloud service.
The previous symbol/PARSeq recognizers, weights, training tools and fixtures have
been removed. There is no fallback to them.

This first integration supports an experimental English acoustic model. It uses
a bounding-box normalization heuristic, a three-layer bidirectional LSTM and
standard CTC collapse. It has no recovered lexical dictionary. English examples
and digits have been verified, but Russian and formula recognition are not yet
implemented in the application. Math2 research is separate from this release.
Confidence is an acoustic score, not a calibrated probability of correct text.

## Model installation

Model weights are separate from the source and are not bundled with installers.
An absent, invalid or uncertain model leaves the original ink untouched. Settings
shows the model status and offers **Install AI pen model** for a `.ikrpack` file.
Installation checks the model structure and checksum before replacing a working
model. The pen menu shows the currently supported language.

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
