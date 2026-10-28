# Ordinary main language first/later dispatch

The shared DIRECT/CASE/global main pass now selects the first-word or later-word
consumer from the actual current row's `38/3C` anchor. It reads that anchor from
the owning node's complete `23F0` row extent, subtracts signed byte `20` from its
ending node, and uses `first - 1 == -1` for the existing first-word path. A later
anchor invokes the existing full `203690` context producer and the existing
`204290` later-history consumer. An empty context heap produces no language row;
it does not fall back to the first-word consumer.

This is the fresh ordinary C00, nontracing, no-associated-source, `ec == 0`,
`F5 == false` scope already enforced by the shared pass. It does not restore the
positive-ec secondary/fallback call, protocol/replacement labels, opaque query
extra contexts, or associated-source permission branches. Existing source-domain
refusals remain explicit. There is no new acceptance or confidence policy.

## Native order and ownership

`MyScriptText!204290` has the following relevant sequence:

- `204D7B..204DA5` reads source `70` with the complete source tag, actual query
  symbol and boundary zero, retaining the returned 16-byte token.
- `2050CE/2050D3/2050FF` resolves current `38/3C` and signed anchor `20`.
  `2051E1..2051E5` dispatches `previous == -1` to the first-word branch.
- `20562E..20565F` resets the context count and computes its capacity from the
  actual target bucket's logical capacity. The native uint32 value is
  zero-extended to int64, converted to single, multiplied by single `0.1`,
  truncated to int64, then compared unsigned against ten. It is not ten percent
  of the current row count or storage extent.
- `205844` calls `203690`. Its ordinary `R9D` flag is zero. Previously restored
  `203690` applies the actual `[230A, 23F0)` secondary range and full 80-byte
  geometry/context operations.
- `205880..2058A8` consumes the resulting heap array in reverse ordinal order.
  The heap is not sorted before consumption.

The new internal registered terminal snapshot preserves source `70` before
geometry, full query32/tag, context factory and the exact current D0 span/epoch.
The producer registers it and consumes it once for the entire context heap. The
later consumer reuses that token rather than reading source `70` a second time.
Query and context bytes are detached before external owner callbacks. Foreign,
consumed or stale snapshots fail; failed completion producers cannot be reused.
The old public prepared Later APIs retain their numeric behavior.

The shared pass retains the same transition operations and mutable global
geometry owner that it uses for primary recombination. Prior row snapshots use
`CopyRows()` through `SourceEnd23f0`, including secondary records, rather than
the smaller retained/history extent. The original and candidate cost cells have
distinct portable identities. Request80 residue is explicitly supplied through
`LanguageContextRequestResidue` when a later branch is reached.

E8 is a descriptor **line ID**. Frontend `152280` obtains the graph group, then
group field zero through `1A7920`, resolves its source descriptor through
`168AD0`, and reads descriptor int32 at byte `0x10`. The global path uses the
same owned primary descriptor's `LineId`; neither the descriptor ordinal, a
secondary descriptor nor physical boundary metadata substitutes for it.

## Verification and limits

`LegacyMainLanguageDispatch.Probe` checks 2,048 independently generated full
uint32 capacity controls, an actual CASE query32/source70 token, 17 invalid
source/query/epoch/single-consume/cancellation cases, and detached snapshots.
It separately constructs a clearly **prepared** two-element later-history heap
over actual first-language payloads. The new registered-token consumer must
match the existing public Later consumer for complete active/inactive heap
storage, both lexical40 records, costs, restart tags and contextual lineage in
reverse order. Its callbacks deliberately overwrite the supplied context and
query buffers to exercise snapshot ownership. This is not inferred later-word
recognition.

The existing CASE Later probe supplies 339 prepared-history controls with a
separate raw-resource language/record oracle. Existing DIRECT 4/56-node,
CASE 4/60-node, global eight-owner/110-node, and configured-secondary passes
provide regression coverage for the shared scheduler. Their current real
queries exercise the first-word branch; no actual nonroot later anchor is
invented to improve that claim. Observed later-word scheduling and full
multiword accuracy remain unproved for these real inputs.

The static audit pins the complete native `204290` function hash, 47 PE byte
locations, three vtable targets, the single-precision capacity constant and
twelve managed owner/dispatch seams. Vendor DLLs are read as bytes only.

```powershell
python tools/LegacyMainLanguageDispatch.Probe/reference_controls.py E:/BlurMask-build-artifacts-20261014/main-language-dispatch-controls.json
python tools/handwriting/audit_main_language_dispatch.py E:/BlurMask-build-artifacts-20261014/main-language-dispatch-static.json
dotnet run --project tools/LegacyMainLanguageDispatch.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/language-dispatch-managed -- E:/BlurMask-build-artifacts-20261014/main-language-dispatch-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json models/text-unicode/legacy-case-data.json models/text-unicode/legacy-text-publication-data.json E:/BlurMask-build-artifacts-20261014/language-dispatch-managed.json
```
