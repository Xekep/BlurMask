# Local AI pen

Choose AI pen from the main pen menu. Write a row, then pause for 0.8 seconds or choose
Recognize now in the pen menu. Pen lifts inside a character do not trigger conversion.
If writing resumes while recognition runs, its stale result is discarded and the original
strokes join the new batch. Desktop ink and the separate canvas have independent batches.

Rows become single text objects, with one font size/baseline and the original bounding box,
color and fading setting. Undo restores handwriting; saved/shared text uses the existing
Text objects with two bounding points. Recognition stays on the computer, including in a shared canvas.
Inter is the default font. Settings offer Inter, Manrope and Source Serif 4; embedded
STIX Two Math supplies mathematical glyphs. The font choice is saved with each text object.

## Models and scope

- Small symbol CNN: 376,452 bytes; 85 classes of Latin/Cyrillic uppercase letters, digits
  and basic mathematical signs. Confident digits/operators are recognized with this small network.
- Connected cursive: [Hukyl/PARSeq-S Cyrillic handwriting](https://huggingface.co/Hukyl/parseq-s-cyrillic-handwritten),
  pinned revision `7ee349c4aa082a3ea6a694263b329ae613ce4dcd`. Approximately 24M parameters,
  48,434,253 bytes in FP16 storage; computation uses FP32. Its training includes Russian words
  and school notebooks, Ukrainian handwritten lines, and synthetic Cyrillic text. It handles
  Cyrillic lowercase/uppercase and natural slant.
- English: [baudm/PARSeq small](https://huggingface.co/baudm/parseq-small), pinned revision
  `a1526c3d63740e460153987f9aaf6b86aa199dc1`, FP16 storage, 47,672,337 bytes. Reads Latin
  upper/lowercase letters, digits and punctuation at 32×128, up to 25 characters per word.
  Its pretraining is scene text, not a dedicated English handwriting benchmark. Synthetic
  slanted/connected Caveat font examples pass; arbitrary real cursive can still fail.
  Pen menu offers Auto, English and Russian independently of the interface language.
- Flat expressions are supported; structured fractions, matrices and full LaTeX parsing are
  not implemented. Ambiguous stroke boundaries and unfamiliar handwriting can produce errors.
- Both networks run in C# compiled with Native AOT. No Python, ONNX runtime, GPU requirement,
  telemetry, downloads or remote recognition are part of the shipped application. Weights are
  embedded once in the executable and loaded on demand. Both line models add about 92 MiB
  to the uncompressed executable and about 192 MiB of decoded weights if both are used.

Conversions require confidence of at least 0.75. This score is not a correctness guarantee.
The entire row is retained when the selected recognizer rejects it; conversion can be undone.
Words use a line model; confident digits and mathematical signs use the lightweight CNN.
Auto first tries the faster English model and skips the Cyrillic model for high-confidence
English words; ambiguous results use both models, with the UI language only breaking close
ties. An explicit language avoids script ambiguity and unnecessary second-model inference.

## Verification and measurements

`--ai-pen-test` runs without creating any desktop windows. It checks:

- C# symbol logits against the training implementation, including real handwriting fixtures;
- C# line encoder/decoder against FP16-rounded PyTorch reference outputs;
- five real cursive word crops against the reference decoder, including its two errors;
  [true annotations are recorded separately](ai-cursive-smoke.json), so this is a portability
  test, not a claim of five correctly read words;
- English encoder/decoder numerics and seven synthetic cursive/numeric/expression examples;
- multi-stroke +, = and ×, complete rows, separate rows, bounds/color, stale results, undo/redo
  and saved text. CI additionally draws a two-stroke + through the Windows pointer path.

On the development computer, warmed-up symbol inference takes about 0.54 ms per character;
the optimized managed probe reads five Cyrillic examples in about 5.3 seconds and seven
English examples in about 1 second. These are local CPU measurements,
not a guaranteed latency on every computer. Inference runs in the background and is serialized
across canvases. SIMD, at most three CPU workers, pooled tensors and cached cross-attention
keys/values bound resource use. No matrix projection of the encoded image is repeated per
decoded character. Four-channel SIMD projection reuses input loads; decoder refinement
computes only the needed query positions. Native AOT optimizes for speed with no extra DLLs.

The small model's [training report](ai-pen-training.json) uses a stratified sample split,
not an unseen-writer split. The best epoch was selected on that same validation split.
Its approximately 95% validation accuracy is not a measured accuracy on arbitrary users'
handwriting. Synthetic math validation shares the template family with training.

## Rebuilding

Training/export tools use Python only on the developer machine. Install their dependencies in
a virtual environment: PyTorch 2.6, torchvision 0.21, numpy, Pillow; PARSeq export additionally
uses timm 1.0.22, Lightning, hydra-core. Run from the repository root:

```powershell
python tools/train-ai-pen.py --device cpu --epochs 14
python tools/export_ai_handwriting.py
python tools/prepare-ai-english-fixtures.py
python tools/fetch-ai-fonts.py
dotnet build src/BlurMask -c Release -p:PublishAot=false -p:SelfContained=false
dotnet src/BlurMask/bin/Release/net11.0/BlurMask.dll --ai-pen-test
```

Source revisions and hashes are in [symbol report](ai-pen-training.json) and
[Cyrillic model manifest](ai-handwriting-model.json) and [English manifest](ai-english-model.json).
Symbol/Cyrillic weights use CC BY-SA 4.0; English weights use Apache-2.0;
the adapted PARSeq runtime uses Apache-2.0. See [attributions](THIRD-PARTY-NOTICES.txt)
and [full licenses](licenses/). These notices are included in published packages.
