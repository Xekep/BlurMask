# Actual handwriting math audit (2026-10-11)

The current independent Math2 preprocessing/greedy decoder, with actual ONNX
Runtime 1.23.2 CPU inference, matches **10 of 14** independently labeled online-ink
expressions after the limited formatting normalization described below. This is
a small diagnostic corpus, not an accuracy estimate for all handwriting. Four
incorrect formulas pass the current confidence gate; confidence alone does not
establish correctness. No model was trained or adjusted against these samples.

## Independent inputs and reproducibility

Thirteen human InkML samples come from the official
[Google MathWriting 2024 excerpt](https://github.com/google-research/google-research/blob/master/mathwriting/README.md).
Eleven are from `valid`, one from `test`, and one limit is from `train`; these are
the dataset's split names, not claims about the undocumented Math2 training set.
The [archive description](https://github.com/google-research/google-research/blob/master/mathwriting/archive_readme.md)
defines `normalizedLabel` and original coordinate/time channels. The fixed
selection covers integrals, a second derivative, a partial derivative, a
differential equation, matrices and limits. IDs/categories were chosen before
running recognition. The fourteenth is the official MyScript Android batch-mode
quadratic-root example, pinned to commit
`4374b0c6d948da4fcf8027d7c4027abaa836d46d` in the
[additional examples repository](https://github.com/MyScript/interactive-ink-additional-examples-android/tree/4374b0c6d948da4fcf8027d7c4027abaa836d46d/samples/batch-mode/src/main/assets/conf/pointerEvents/math).

`tools/handwriting/fetch_math_ocr_corpus.py` downloads only the 1.6 MB excerpt,
verifies SHA256 and reads bounded selected members without extracting the
archive. The archive SHA256 is
`cba038def001480a89962b25cb20a60df4c4145e94c86ef9d2af65f192cb82bc`.
It preserves XY coordinates, trace order and point order. Time and pressure are
ignored; no rasterization, resampling or expected-label feedback is added.
Generated JSON and ink remain in the requested private output directory.
MathWriting data is CC BY-NC-SA 4.0 and is used here for a research audit; it is
not included in application packages or committed as application assets.

```powershell
python tools/handwriting/fetch_math_ocr_corpus.py D:/math-ocr-corpus
dotnet run --project tools/InkRecognition.Math.Probe -- --corpus `
  /path/to/math2.ikrmath /path/to/onnxruntime.dll `
  D:/math-ocr-corpus/manifest.json D:/math-ocr-corpus/report.json
```

The CLI loads one model/runtime, recognizes each complete expression twice,
checks deterministic outputs and writes actual LaTeX, raw tokens, independent
expected label, confidence, frame/stroke/point counts and timings. Labels are
compared only after inference. Its exit status checks execution and guards;
the reported mismatches remain visible rather than becoming accepted goldens.

Formatting comparison ignores whitespace, `dfrac/tfrac/frac`, `left/right`
fence sizing, a final matrix row separator, `longrightarrow/rightarrow/to`,
literal `Λ` versus `\Lambda`, and `||` versus `\Vert`. It does not substitute
variables, case, Greek-letter alternatives or algebraically simplify formulas.
These aliases are explicitly recorded in the report and self-checked.

## Findings and corrected formatter defect

The actual model emits `<under>...<under2>...</under>` and
`<underover>...<underover2>...<underover3>...</underover>` for integral/limit
bounds. The previous formatter rejected both structures, so four samples were
returned as unsupported despite complete decoding. The formatter now preserves
the base, lower bound and optional upper bound as LaTeX scripts. Incomplete or
misordered markers still decline. This fixes formatting; it does not claim the
vendor's exact typesetting placement or correct mistaken symbol recognition.

| Sample ID | Expected meaning | Actual result | Outcome |
| --- | --- | --- | --- |
| `0019bb8e97507f8e` | `∫ x·dx` | Same | Match |
| `00245713d08b4f1a` | `R=∫₀^τ X(s)ds` | `τ` becomes `T` | Mismatch, confidence 0.953 |
| `00235ec3dee919dd` | Improper integral `∫₀^∞ sin(x)/x dx` | Same after formatter fix | Match |
| `012f1f23d40a2995` | Integral with lower bound `γ` | `γ` becomes `y` | Mismatch, confidence 0.929 |
| `00146a32fdab9e8e` | Second derivative `d²x/dt²` | Same | Match |
| `003921302c543b2c` | `d²u/dx²-u=0` | Both `u` become `v` | Mismatch, confidence 0.887 |
| `002b6a98000765fb` | `∂φ/∂t(x,t)=F(φ).` | RHS becomes `f(x)`; variant φ differs | Mismatch, confidence 0.914 |
| `01927fe0f1d3c23d` | 2×2 matrix | Same | Match |
| `00a32b900a4aa32f` | Column vector | Same | Match |
| `00e00364101abad0` | Bracketed 2×2 matrix | Same | Match |
| `01752f0f5fba0225` | Matrix containing `-1/f` | Same | Match |
| `033d57e0703ab948` | Limit as `n→∞` | Same after formatter fix | Match |
| `01468b08b993df97` | Limit involving `λ∈Λ` and a norm | Same with notation aliases | Match |
| `android-quadratic` | Quadratic-root fraction | Same | Match |

Managed and Windows NativeAOT outputs/confidences agree. The audited pack SHA256
is `08226987e872d1d81b4c691659286ca4fe4c3e8ce55dda699fddd597340d3ed7`.
One Windows NativeAOT run loaded the model in 396 ms; repeated warm inference
was 9–35 ms, median 22 ms. This measures CPU recognition only, excluding UI,
pen-pause scheduling, raw-content routing and CAS. Timings vary with machine load.
The 84 numeric checks and 17 formatter/model guards also pass managed/AOT.

## Application route and remaining work

With the real installed Math2 and Raw Content2 packs, the optional headless
application corpus audit accepts all 14 as direct mathematical candidates and
automatically converts 13. The simple `∫ x·dx` sample's seven raw stroke predictions
are all Math with confidence 0.557–0.690, below the 0.85 routing gate. It yields no
eligible regions and therefore stays handwritten. This is a routing limitation, not a
failure of the mathematical decoder. The partial-derivative sample's final dot
is classified into a separate text region and stays handwritten. The audit
checks supported formula syntax, original-ink undo for direct candidates and
that written/no equals never starts CAS.

```powershell
$env:BLURMASK_MATH_CORPUS='D:/math-ocr-corpus/manifest.json'
BlurMask --ai-pen-test
```

No desktop lifetime or GUI is started. This application audit needs installed
packs and native runtime; it deliberately reports routing separately from label
accuracy. Undefined/low-confidence/unsupported candidates retain handwritten ink
under the normal guards. The four observed high-confidence symbol errors are
**not** rejected by those guards. Raising a single global threshold would also
reject correct examples and is not an established fix.

Further work requires testing the native search/segmentation/context policy
against this corpus, including alternatives for ambiguous symbols and calibration
on a larger held-out set. Empty raw-region handling needs a drawing-preserving
fallback evaluation before changing production routing. The separate 25 actual
Giac cases prove solver behavior, not 25 handwritten expressions recognized.
