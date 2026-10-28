# Ordinary sampler group pruning

This stage restores the ordinary eligibility pass between graph merging and
acoustic recognition. It does not complete the word scheduler or establish
Russian recognition accuracy.

The native sampler297 method68 is `194140`. A zero span returns false. Other
ordinary groups call `193860` and return true when that **raw separation cost**
is strictly greater than sampler field11C. The separate field118 caps the later
`195520` category cost and must not cap this rejection decision. Both actual
Russian samplers store11C=20480 and118=67108864.

`1A61C0` calls `1A5B00` for nodes0 throughN−2. It deliberately leaves the final
node untouched. Each visited node removes arcs in descending ordinal order.
Removal unlinks predecessor and successor lists, decrements higher referenced
ordinals and shifts complete arc records. It preserves group identities and
does not remove groups merely because their last arc disappeared.

`LegacyOrdinarySamplerGroupPruning` restores this ordinary same-source branch.
It retains all metadata, encounter ordering and surviving adjacency. Its
result keeps the input graph identity. `LegacyOrdinaryGraphSourceRanges` can
accept only that exact owner's result, preserving actual seed and raw point
chronology after arc renumbering. It rejects a different equal-looking graph.

`LegacyFreshFirstCandidateGraphPreparer` binds the actual selected resource,
first factory and cuts, reads the21b node adjacent to its sampler, uses the
serialized cap and runs the eligibility pass. The actual first source has
cap7, while the main source has cap8. Neither is replaced with the old
diagnostic cap5. Seeds are explicit outputs of the existing ordering producer;
this endpoint does not invent native direction flags or token metadata.

The independent oracle reconstructs raw separation costs from prepared point
coordinates, uses binary32 arithmetic and then performs literal mutable graph
deletions.72 actual first-source cases check resource caps3–7, translations,
scales, duplicate input points, thresholds0/20480/67108864 and independently
changed later caps. They remove529 arcs, retain80 final-node arcs that would
otherwise be rejected and exercise233 decisions that would differ if the
later clipping cap were used. Every complete surviving arc, original ordinal
and adjacency list is compared.1895 source callbacks check that selected raw
ranges survive renumbering. These counts describe this corpus, not recognition
quality or a finished main controller.

```powershell
dotnet run --project tools/LegacySamplerGroupPruning.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-separation-profile.json .artifacts/sampler-pruning-audit.json
python tools/LegacySamplerGroupPruning.Probe/audit_output.py .artifacts/sampler-pruning-audit.json
```

`tools/handwriting/audit_ordinary_sampler_group_pruning.py` checks pinned native
method bindings, reader fields and call order without executing vendor code.
Special/cross-source groups, token pruning, seed policy production and the
whole first/main search controller still require further work.
