# Legacy Russian directional word diagnostic

`LegacyDirectionalWordRecognizer` composes physical ink, caller baseline,
prior-context windows, restored neural outputs, calibrated family confidences,
alphabet mappings and a packed dictionary in the reusable library.
`tools/LegacyDirectionalWord.Probe` exercises that module;
the application UI has not enabled this experimental path.

The default mode needs no vendor assets. Eight original separated printed word
fixtures exercise grouping and all four input widths (256, 238, 313, 313) with
scaled and translated source baselines. A separate original resource-free
roundtrip uses deliberately reversed acoustic/lexical IDs and nonuniform scores
that calibrate to equal confidence: the only terminal dictionary path and its
expected cost fail if either calibration or label mapping is bypassed. Partial
words and cancellation are also checked. This mode asserts pipeline mechanics,
not handwriting accuracy.

```powershell
dotnet run --project tools/LegacyDirectionalWord.Probe -c Release
```

The optional diagnostic accepts six local caller-owned files:

```text
LegacyDirectionalWord.Probe acoustic.res lexical.res activations.json
  temporal-calibration.json spatial-calibration.json mode1-score-calibration.json
```

Offsets identify only the inspected legacy Russian lite resources. The mode1
file contains four `centers` and four `slopes` rows, each with 180 floats. Other
assets need their own proven configuration and label correspondence. These
Russian `.res` resources are stored under [`models/ru-RU`](../../../models/ru-RU).
Matching activation and calibration tables remain separate caller configuration.

The source baseline bottom and line height are transformed with each fixture:
`90 * scale + y` and `80 * scale`. No inferred baseline or tuned word-specific
height is used. Whole physical strokes are grouped conservatively; context is
scored on the full line and excludes every target physical stroke. Complete
glyph boundaries are +1024. The first pair of families reuses the restored
baseline trajectory/raster extraction, explicitly appending the same 28 context
channels. The second pair uses the independently restored calibrated temporal
and spatial 313-channel profiles.

The diagnostic prints both the 3f+4f subset and all four active families. The
per-family mode1 calibration and native combination order are documented in
[the ensemble notes](legacy-russian-acoustic-ensemble.md). Afterwards, the
confidence-to-integer truncation, related-form strongest-source mapping and
dictionary beam are original adapters. Their scores are not proven native
language-model costs; written case is not preserved by this alias policy.

On the eight own printed fixtures, managed and Windows Native AOT returned
identical JSON ranks and top-five costs for all 16 profile/sample combinations:

| Input | 3f+4f expected rank | All four expected rank |
| --- | --- | --- |
| Four scaled/translated MAMA fixtures | 1, 1, 1, 1 | 1, 1, 1, 1 |
| Four scaled/translated MIR fixtures | 32, 32, 30, 29 | 2, 2, 2, 2 |

The expected dictionary spellings are Russian `мама` and `мир`. The four-family
MIR fixtures still select `миф` first. Actual mode reports these failures rather
than asserting fabricated accuracy. `expectedRankInReturnedBeam == 0` means the
expected spelling is absent from the returned bounded beam. Results on two
constructed printed words cannot establish Russian handwriting quality.

Joined-stroke candidate cuts, native segmentation and lexical/case costs,
automatic line calibration, contextual label masks and representative real
cursive evaluation remain unresolved. This diagnostic is deliberately kept out
of the application UI until those dependencies are verified.
