# Owned ordinary source references with endpoint residuals

`LegacyOrdinaryGraphSourceRanges.ResolveCandidateWithResiduals` and
`ResolveForwardWithResiduals` expose the exact ordinary normal forward
1A9AE0 result, including each24-byte reference's separately stored two
endpoint bytes. `LegacyOrdinaryGraphSourceReference` owns value fields and
returns fresh serialized24-byte/two-byte copies. Existing key-only APIs
remain unchanged; both forms project one common verified path/sort/merge
core, without an additional intermediate result array in the old API.

The metadata is produced from the **same owned prepared chronology and
original retained metadata** as the source graph. It is not recovered from
Unicode, geometry boxes or current physical ordinals. Native1A8E60 stores
first key/remainder from source.V90 in temporary row00/19, original ending
metadata from source.VA0 in08/1A, and prepared last-start metadata from V90
in10/1B. Row18 bit0 is the proven ordinary tail condition. After the native
sort/coalescing,1A9AE0 exports the first key and19 byte, then either the08
key/1A byte for tail or10 key/1B byte otherwise. Coalescing retains the first
start and last end identities.

`LegacySourcePointIdentity.RemainderByte` was already retained through all
those operations in the private owned row. The new API exports it instead
of dropping it at the final key-only projection. No zero residual defaults,
new rounding rule, chronological sorting shortcut or source identity alias
was introduced. Physical reorder may change current range stroke indices;
the source keys and original ending metadata continue to address the
original owner. A duplicate raw-point run may therefore change the correct
last key; end is not blindly start-ordinal plus one.

This restores the ordinary reference+residual producer required by the lazy
glyph owner's V140 join. It is **not** a complete1AA470 dispatcher: higher
arc source membership, reverse/protocol source branches, two-pass mutable
owner allocation,217670 refresh, wrapper prefixes and document object IDs
still require their own owning stages. The new result triples retain their
native source-key meaning. See [the lazy-refresh roadmap](legacy-main-lazy-glyph-refresh-roadmap.md).

## Verification

The new headless probe uses576 committed original raw-coordinate controls,
explicit factor200 numeric resampling, inserted prepared points, mixed raw
point counts and288 genuinely permuted prepared physical/key owners. These
are explicitly prepared source-map controls, not claims that a native neural
grouping model selected those permutations. It also runs18 actual pinned RU
first-source frame/cut/graph compositions. All11252 resulting callback
queries compare the old key-only API to the new key projection.

The independent Python oracle derives ceil-selected keys and remainder bytes
from fixed-point maps and source factor, resolves original raw ending keys,
walks the actual seed DAG, independently bubble-sorts/coalesces its temporary
rows, and checks every reference24/residual2 byte. It verifies9186 nonzero
endpoint bytes,7195 reverse merges,288 reordered owners and2368 duplicate-tail
differences. It never reads expected text. Native and Managed captures are
byte-identical. Separate original `LegacyGraphSourceRanges.Probe` regression
checks the old key-only contract against its committed independent oracle.

```
python tools/LegacyGraphSourceResiduals.Probe/reference_controls.py RAW_CONTROLS.json
dotnet run --project tools/LegacyGraphSourceResiduals.Probe -- RAW_CONTROLS.json AK.res OUTPUT.json
python tools/LegacyGraphSourceResiduals.Probe/reference_controls.py OUTPUT.json --audit
python tools/handwriting/audit_graph_source_residuals.py STATIC.json
```

Bounds, unavailable source paths and cancellation retain the same checks as
the original forward API. Result-array/serialized-buffer mutation controls
verify that caller changes cannot replace owned metadata.

Read-only native proof pins40 exact instructions and two complete function
intervals in Text PE SHA256
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`:

| Native interval | SHA256 |
| --- | --- |
|[1801A8E60,1801A9070)|60343601da38a868f3ff3f3790ac48fc2331e6df96be4ce8a29cf88050feeeec|
|[1801A9AE0,1801AA470)|f1c1d83c50d0d8a9dd275272dc40d1b454c5dd5958a624dfca35bf5579c8de55|

No vendor DLL or GUI is executed, and no live application/font-size accuracy
claim is inferred from these metadata controls.
