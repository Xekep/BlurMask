# Owned Math symbol-sequence negative-log cost

This extends the pinned Math AK sequence primitive; it does not add handwriting recognition, grammar/candidate ownership or an application route. Source ordinals remain explicit prepared input.

## Native meaning and order

Math00F3C0 reads stored output width241. Its distinct IDs are boundary=`241-1=240` and ignore=`241-2=239` (ASM00F43D/00F440). The boundary appears at both ends of the prepared ID vector, with the last ID used only as a target. It is **not** the ignore class.

Math00FD40 requires the mapping minimum0 and maximum239. Each actual290-entry mapping contains239 exactly51 times. The reader initializes a float counter to0, increments with `addss`1.0 for each ignore mapping, then performs `divss`1.0/counter. The decompiler's integer casts for this field are misleading. Each owned model now derives its factor with those F32 operations: factor bits3CA0A0A1, approximately0.019607844. No supplied probability or factor enters the owning score API.

For every real next-target probability,00F3C0 multiplies by the factor only for target239 (`mulss`00F610), then compares with the smallest normal float (`00800000`,1.17549435e-38). Values below it use the stored log floor (`C2AEAC50`,-87.3365478515625). Equality uses log. The chosen log is subtracted from one F32 accumulator (`subss`00F62B). That accumulator is carried across all frames of record1, then all frames of record2, and divided once by the configured record count2 (`divss`00F675). It is not a token mean or a separately rounded sum of record costs.

Native08D200 is a CRT logf dispatcher with AVX/SSE alternatives. The portable helper uses MathF.Log and does not claim bit equivalence to those approximations, MXCSR state, SIMD, NaN payloads or FP flags. Probabilities owned by the existing finite softmax are nonnegative and at most1; the new pure helper explicitly refuses malformed scalars outside that bounded domain. Native exceptional/failure-prefix behavior is not replicated.

## Implemented ownership

`model.ScoreRecord(prediction)` requires that exact model's private prepared owner and returns an unaveraged record cost. `LegacyMathAkSymbolSequence.ScorePair(forward,reverse)` requires the actual forward/reverse models, and exact original source ordinals. Prepared symbols now retain their already copied ordinal snapshot so equal mapped IDs cannot substitute for distinct source ordinals (e.g. ordinals40 and118 both map to239). No arrays/Memory escape; typed predictions still come only from their actual model's `Evaluate`. Tests use the separate pure internal arithmetic primitive for synthetic scalars, never manufacture a Prediction or candidate authority.

The two fixed models are authenticated by the complete same resource SHA. Pair scoring is a numerical composition for explicit matching input ordinals, not an issuer for the native13F8/13A5 candidate cohort. It omits00F6C0 exclusions, candidate cost multiplier0.14, length policy1.5, and alternative reordering. `ScoreRecord` and `ScorePair` do not evaluate a network, change old numerical policies, or grant confidence.

## Bounds and evidence

At most4096 tokens per record/8192 per pair are scored; every token has a cancellation checkpoint. Scoring allocates no arrays and publishes only a float. The retained source-ordinal copy adds one private reference (8 bytes on64-bit) and at most16380 logical element bytes per prepared sequence (it previously existed only during preparation); pair input comparisons inspect at most4095 ordinals. The factor fields add8 logical bytes per model. These are payload/cardinality statements, not CLR object sizes/RSS or CPU instruction bounds. The original feature/network bounds remain unchanged.

The compiled `--score-controls` mode uses authentic resource mappings but zero network forwards:12 numeric cases,10 guards, and a4096+4096 token F32-order witness. It distinguishes ignore/boundary, normal/subnormal threshold and multiplication-before-floor, negative zero, and continuous accumulation. The independent Python reference derives factor51 from raw mappings, uses its own FP32 log arithmetic, and rejects7 corruptions. An initial reference parse correctly stopped on JSON `-0`: Python's default integer parser discarded the sign. Its preserved successor parses that numeric token as negative floating zero; C# output and production were unchanged.

The existing29 prepared/synthetic primitive guards and7 corruptions still pass, with byte-identical capture SHA209f0a9be4d14df573526bbdb3c4ae13f027616fe359b0dd763351c2206f5c9c. Existing NativeNetwork and its numerical policy source are unchanged by this stage.

After source review, the updated `--actual` mode was run on both real networks,
with source ordinals `[0,1,2]`. All1928 output probabilities were identical to
the previously checked primitive capture. An independent FP32 calculation from
those outputs gave record costs15.9745941 and16.3972855 (one and zero ULP from
the managed results), and the ordered pair cost16.1859398 (zero ULP). These are
costs of genuine owned predictions; no supplied score or expected label entered
the model. This short input has no ignore239 target: that branch is covered by
the separate scalar/raw-mapping controls, not by this actual-network capture.
The check establishes neither OCR quality nor native CRT bit equivalence.

```sh
dotnet run --project tools/LegacyMathSymbolSequence.Probe -c Release -- models/math/math-ak.res score.json --score-controls
python -I -B tools/LegacyMathSymbolSequence.Probe/audit_rescore.py models/math/math-ak.res score.json score-audit.json
```
