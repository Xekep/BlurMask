# Fresh append-only math original-input frame

`LegacyMathAkOriginalInputFrame` is an internal library-owned fresh append-only subset of native1F40. `LegacyMathAkProcessedStrokePrefix` creates and retains it BEFORE006A40 bootstrap; the existing139D/139F numerical results are unchanged. Original Float32 point storage stays in the same immutable `LegacyMathAkSourceGeometry`, while ends, original stroke IDs, inverse ordinals, kind-1 and active bits are private sidecars. A distinct private document identity is created even for two preparations of the same geometry. Diagnostics return copies only.

The IDs/masks follow001DB0 ->002900 ->034810/001A20: append uses the old item count as the new ID, old stroke count as the inverse ordinal, zeroes new bitmap words, and sets only the new ID bit. Erasure002F40 is a separate unsupported transition. Raw active-word count is ceil(S/32); later006A40 processed-work count is floor(S/32)+1. They differ at32/64 and must not be conflated. No IDs, selection masks or existing-document claims are supplied by callers.

Scope is NOT complete1388 input handling,1390 returned-range ownership, full13B6, processed-source180/188, matcher request or grammar candidate authority. The source geometry owns original data, not a native incremental document or rectangle set. Later operations must preserve original point/stroke provenance separately.

At64 strokes, retained sidecar array elements are1032 bytes (four Int32 vectors plus two UInt32 bitmap words), plus the new prefix reference8 and Frame field payload56 bytes; CLR object/array headers, alignment and private identity object overhead are excluded. Source point storage is retained through the owner and is not cloned by this frame. This is a logical payload account, not RSS or an enforced allocator/time quota.

Prepared scalar verification:16 unchanged prefix fixtures/25 original guards/11 existing negative captures, plus64 independently reconstructed append cases/72 metadata guards/10 negatives. Boundary31/32/33/64, returned-buffer mutation, same-geometry document distinction, prefix-retained frame, equal-data foreign profile and exact cancellation token checks pass. No learned network or vendor DLL executes. Canonical Program2f8598 reads actual defensive arrays AFTER mutation controls. Earlier07898 diagnostic restoration was superseded and is preserved only in external evidence.

Commands (repo root):

```text
dotnet run --project tools/LegacyMathAkOriginalInput.Probe -c Release -- models tools/LegacyMathAkProcessedPrefix.Probe/fixtures.json FRESH_CAPTURE.json
python -I -B tools/LegacyMathAkOriginalInput.Probe/audit_output.py . FRESH_CAPTURE.json FRESH_REPORT.json
```

The new auditor composes the existing prefix auditor/fixture/reference without copying them. Author build0warnings/0errors and scalar capture615e84a8974ef2d10611213cd5fa5751292903c42b64ebc76958dfbb4675d1f2 were independently reviewed; root16 scalar payloadcc14892e remains exact. Source/native append proof9520 and prepared successor90d28 remain immutable external dependencies; no default or user routing changes.

Root rebuilt the probe against the current main library with zero warnings or
errors. Its complete capture remained byte-identical to `615e84a8…`; all 16
prefix and 64 metadata cases, 97 guards and 21 tamper controls passed the
canonical repository auditor. The run checked 1,118 source/model/runtime pins
before/after and performed no neural inference. Separate root verification of
the earlier compiled successor checked 4,779 dependencies; this is independent
evidence, rather than a count of files in the application or its installer.
