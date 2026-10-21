# English recognition: recorded multi-stroke baseline

The genuine English Parent/FIRST pipeline was tested on100 recorded Latin-word
inputs. It completes all100 but recognizes only14 complete words exactly, with
203 character edits in721 reference characters (28.16% CER). This establishes a
baseline for the unfinished MAIN dictionary path; it does not establish working
English recognition or justify enabling the engine by default.

Inputs come from [Schomaker's PluColl recording archive](https://zenodo.org/records/7894283).
The author documents real Wacom pen coordinates and explicit pen pressure0/100
for up/down. The selected archive is `Plucoll-hwr-lbl.tgz`, SHA256
`a1c84fb7ba8a87c8c775fa1283817d73760b022217406e6acd348208b855d07d`.
No recordings are redistributed in the repository. The catalogue labels the
dataset English; the vocabulary also includes loanwords and proper names, so
this small sample is not a representative English-language benchmark.

Selection was fixed before inference: the first five lexicographic writers,
the first20 ASCII-labelled word files per writer in archive order. The writers
are angelien, annemiek, anton, beatrijs and corrie. The100 records contain86
multi-stroke words,646 strokes and29,992 retained pen-down points. Stroke splits
use only explicit pen-up samples. X stays unchanged, Y is negated for UI
coordinates; no new release events, scaling or smoothing are invented.

The independent headless harness uses the existing actual English Parent owner,
resolution0 and its owned FIRST candidate/cache/primary/context/terminal/plain
WORD pipeline. It records every emitted40-byte glyph row and its real selected
FIRST alphabet. A separate Python audit reads the raw selected source247/110 at
8843..182187, decodes its inline AL104 and verifies all759 glyph IDs/text
emissions. Only after that audit were the separately stored word targets opened.
Managed and NativeAOT captures are byte-identical. Model-training overlap is
unknown; the sample is no longer unseen and must not be used to tune thresholds,
beam sizes, language weights or models while still calling it a blind test.

| Artifact | SHA256 |
|---|---|
| Blind coordinates | `bcd10f1a92a3abf6d78c5eb18787abf33ad291ab07660daae7468cf465dab285` |
| Separate targets | `1fdf16d271a02012557876275c204ad3455ce5cfe06088702a5399b5de34132d` |
| Complete Managed/Native capture | `41edb9d0ebe1ad44c0fea690653188371eae0228028f28740f71e2762d64bfca` |
| Independent emission audit | `b0076abe82be0864d47517400e30b5e55492594475b859f82aa17c0cc12f9993` |
| Post-inference evaluation | `e8532a735985e3998381f26436cccf1a1739945c13967a3d537f1a526ca31b47` |

Evidence and the headless harness remain under
`E:/BlurMask-build-artifacts-20261014/`, in
`english-handwriting-plucoll-20261021`,
`root-plucoll-first-baseline-20261021`, `root-audit-plucoll-first-20261021.py`,
`root-evaluate-plucoll-first-20261021.py` and the corresponding
`root-plucoll-first-*-20261021.json` files. The same fixed inputs are intended for
comparison after genuine MAIN neural scoring and language decoding are joined;
container, mask and data-loading checks alone are not improved accuracy.
