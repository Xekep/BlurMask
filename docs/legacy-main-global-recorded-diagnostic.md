# Recorded Russian words through the actual global main path

The2026-10-17 diagnostic completes nine pinned coordinate recordings through
actual global preprocessing, cuts, graph/context/acoustic caches, CASE292 main
controller, terminal selection, compact publication, glyph reconstruction and
plain WORD publication. It then emits raw UTF8 from the actual stored labels.
It substitutes no old single-descriptor owner. Expected text is read only after
all inference and an independent raw-AK byte check.

Results are **4/9 exact words**,16 edits over33 reference characters
(48.48% character error rate). Every raw string matches the previous CASE main
WORD275 diagnostic. That comparison is with the older CASE main path, not an
independently established acoustic-first-only baseline. No improvement is shown.

All nine inputs produce one descriptor,7..22 graph nodes, one published word and
two flush callbacks. There are868 acoustic group evaluations,22,440 compact
records and31 published ordinary-kind0 glyphs. Publication does not reevaluate
the networks. Recorded controller traces include362 genuine language queries
and16,557 valid first anchors, with no later anchors. The dictionary is used.

Managed and Native AOT agree on all saved semantic fields, full pool records,
reference order and UTF8; measured elapsed time is excluded from comparison.
Semantic capture SHA256:
`1c4e8cf9247a42514d9c23f317934b96e87815b4542695335a498983b6606fb1`.
The reusable global UTF8 API separately matches the low-level emission on all
nine Native callbacks. Its instrumented capture SHA256 is
`693f691345a3b7a92f5f80f739427fa6b8633f20264a79776d87707612719478`.

The recordings contain no pen-up events. Each is deliberately supplied as one
continuous stroke, without inferring lifts from coordinate jumps. This loses
input information, but it is not a proven cause of the five errors. These nine
words from three writers are not a representative cursive benchmark.

The wrong labels already exist before raw UTF8 emission, which independently
reproduces them. Neither nonempty control/alternate glyph owners nor long-source
LIVE publication are exercised here. Completing those branches cannot be claimed
to fix these particular errors. No causal feature/model defect has been identified
from this diagnostic alone. Final accepted document text and equivalence to the
entire original engine remain unverified.

The optional recorded-input mode of `LegacyMainGlobalWordUtf8.Probe` reproduces
the raw projection with the pinned manifest and track index. The retained local
analysis artifacts are in `global-recorded-execution` and
`global-recorded-projection` under the external build-artifact directory. This
diagnostic does not enable the new path in the application.

To reproduce all nine raw-resource checks before evaluating expected text, use
the standalone evaluator with a built Managed DLL or Native AOT probe:

```powershell
python tools/handwriting/evaluate_global_recorded_words.py --probe <LegacyMainGlobalWordUtf8.Probe.exe-or-dll> --manifest <recorded-corpus/manifest.json> --output <external-output-directory>
```

The evaluator retains each capture and writes `report.json` with track and
capture hashes. It requires identical bytes for all32 caller-residue variants
of each recording and completes every independent AK-resource audit before
computing exact-word accuracy and raw Unicode edit distance.
