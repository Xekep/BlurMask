# English request identity caches — research candidate

Not production or a completed English recognizer. The source belongs to the
isolated, still-unpublished 950-file periodic English stage, not current main's
935-file library. Do not copy this partial class directly into production.

The candidate replaces repeated linear wrapper and membership searches with
bounded identity indices (64 wrappers/128 slots; 80 requests/256 slots). It keeps
the existing wrapper objects, owner checks, reset order and request lifetime.
Hash collisions use reference equality through the original arrays. Each probe
is charged before reading; backing allocation and cleanup are prepaid. No model,
input, acceptance threshold or work ceiling was changed.

`source/LegacyEnglishCaseGraphPeriodicQueryBudget.cs` SHA256:
`2f00b0b27eab29f8a075066ed1dce3f19c11a5a8f35eb996ce729860fdb50030`.
The 256-slot helper is extracted exactly into `probe/Helper.cs`; the query and
scope fixtures are explicitly synthetic and do not confer real source ownership.

## Completed verification

- New static exporter authenticated native Text functions `20C9D0`, `231ED0`,
  `232090`, `233090`, with every exported instruction matched to the original PE.
  `232037` is a cursor-clear instruction inside the exported `231ED0`, not a
  separate missing function entry. Full upstream delimiter-reset ordering still
  needs its own connected proof.
- Wrapper index: 4095 oracle-checked output rows, collision/reset/guard controls
  and nine capture mutations rejected.
- Membership index: 494 expected rows fixed before compilation, two 80-entry
  collision cycles, 162 interrupted pre-read probes and extracted Enroll/End
  guard checks. These are synthetic ownership objects.
- Two full isolated 950-source libraries and their real-model entry points built
  and passed portable-PDB/source identity checks. The unchanged original input
  `d1c9284dbe4f79301f2a0d860e14b3c00e5a0ba02430b98a6bac5f0c43fa6b99`,
  original index4/three points/resolution96, progressed beyond the earlier first
  glyph-refresh refusal. Both candidates still fail the unchanged visit limit
  in `CompleteGraphCharacterPass`. The combined candidate reaches a later pass
  with an already-refreshed owner, but produces no completed pending TEXT.

Historical evidence is retained under
`E:/BlurMask-build-artifacts-20261014/engine-case-cache-next-20261028` and
`engine-membership-next-20261028`; these paths are evidence references, not
inputs for the portable numeric helper probe. The full 950-source owning route
is not reproduced by this small packet. No recognition quality/speed claim follows.

## Portable numeric reproduction

```powershell
dotnet run --project tools/handwriting/ports/english-query-caches-20261028/probe -c Release | Set-Content -Encoding utf8 .artifacts/membership.jsonl
python -B tools/handwriting/ports/english-query-caches-20261028/check_probe.py .artifacts/membership.jsonl
```

The checked-in expected data predates the probe build. SyntheticScope.cs uses
the exact Enroll/End bodies with a fake owner/ledger; it is not a substitute for
the real periodic owner, and is never compiled by the application.
