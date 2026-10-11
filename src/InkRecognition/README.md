# InkRecognition

Portable CPU recognition of pen coordinates. The project has no UI dependencies,
NuGet packages, Python runtime or native ML libraries. It is compatible with
.NET 10 or later, including Native AOT, and can be referenced from another project or copied as a
separate directory. Source code follows the repository MIT license; model weights
are separate and retain their own terms.

```xml
<ProjectReference Include="../InkRecognition/InkRecognition.csproj" />
```

```csharp
using InkRecognition;

using var stream = File.OpenRead("en-US.ikrpack");
IStrokeTextRecognizer recognizer = StrokeTextRecognizer.Load(stream);
var strokes = new InkStroke[]
{
    new(new InkPoint[] { new(0, 0), new(8, 20), new(16, 0) })
};
var result = recognizer.Recognize(strokes, cancellationToken);
Console.WriteLine(result.Text);
```

Reuse one loaded recognizer. Weights are immutable; each recognition call owns its
scratch buffers. Calls accept cancellation. UI scheduling, row grouping, colors,
fonts and document editing belong to the caller. Model loading retains ownership
of the supplied stream with the caller.

The initial implementation supports 13-channel, unlinked-stroke text models:
float32 geometry/resampling, channel normalization, float/int16/int8 affine
kernels, peephole LSTM/BLSTM and CTC collapse. Input limits are 256 strokes and
20000 finite points within ±100000 coordinate units. Sampling is bounded to 6000
frames. The pack reader validates SHA256, lengths, dimensions, UTF-8, finite
parameters, tensor padding and full consumption of input. SHA256 detects damaged
data; it is not a publisher signature.

`IKRPACK1` models use little-endian primitives and a 48-byte header: eight magic
bytes, version, body length and SHA256 of the body. The body contains language,
labels, preprocessing parameters, architecture, tensor inventory and packed
weights. [The exporter](../../tools/export-ink-model.py) documents the exact layout.
No model weights are embedded in this library.

Current limits: whole-word geometry is heuristic; CTC has no lexical language
model; Russian and Math2 are not implemented here. Confidence is an acoustic
score. This is an experimental engine, not a reproduction of the complete
MyScript SDK.
