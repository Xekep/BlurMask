# Recognition checkpoint after RC14 — 2026-10-24

RC14 (`94cde63`) remains a prerelease. The later English lazy refresh and
connected Math13B6 changes (`28f0fb7`, `dcfd221`) are on main and are not part
of that release. These changes do not establish Russian cursive accuracy or
completion of the independent engine.

## Original-source diagnostic

A single purposeful diagnostic enrichment of the previously reviewed shared
TEXT path completed in 4.28 seconds, CPU2/affinity3 with a45-second watchdog.
The library and runtime configuration were byte-identical to the previous
run. All42 frozen dependencies were verified before and after execution.
Only the harness added detached preparation-input DATA; no exported values
were fed back into recognition. No target labels or vendor runtime were used.

Capture SHA256:
`c13c1dec24eb3880e1dde064beeb8e77b8f7d0efe05916e858b9115f98498669`.
Every earlier capture field matched exactly. The additional DATA contains
physical ordering, source-key chronology, smoothing/cut maps, descriptors
and guides for independently checking source positions. It is input evidence,
not an oracle for its own correctness.

Root also reviewed and replayed the separate scoped DATA auditor: two WORD
choices,17 CHAR links and12 shared CHAR payloads matched the raw AK alphabet,
Unicode tables and final pool selection. All30 corruption controls were
rejected without inference. This capture contains only one actual WORD node;
it does not demonstrate multiple-word recognition or a quality improvement.
The successful scoped audit still leaves merged-source provenance,
Source26/line56 recomputation, refresh/ranking numerical inputs and the
multiple-WORD runtime unverified. The enriched inputs enable further checks;
their presence alone does not close those gaps.

Local immutable evidence, outside release assets:
`root-original-source-data-actual-20261024` and
`root-higher-data-replay-20261024` under the build-artifact root.

## CI frontier

On `eb2460e`, the repaired configured-secondary compact oracle passed its
complete163840-byte compact and144832-byte source/pool comparison. The next
Build failure occurred in `LegacyMainDocumentInputTransforms.Probe`, line34:
its configured sample has active bit8 candidates but invokes the explicitly
no-candidate completion path. That path correctly refuses to omit219580
augmentation. The positive INPUT-transform test remains required; disabling
the guard, pretending candidates are absent, or replacing every positive
sample with a refusal would not validate the missing configured owner path.
Current-main CI must finish before claiming it is green.
