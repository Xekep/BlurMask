# Main sampling across ordinary source descriptors

This is a static ownership roadmap, not an implemented multiline recognizer.
The existing [single-line ingress](legacy-main-single-line-ingress.md) deliberately
requires one assigned ordinary descriptor covering every current physical stroke.
Removing that check or invoking that sampler independently for each descriptor
would change the native point, context and graph domains.

The first missing producer is a **shared descriptor cut source**. After that,
the local graph append/remap and cross-descriptor candidate construction must be
restored before the existing main classifier can consume a genuine global graph.
The same dependencies apply to multiple words and multiple lines; descriptor
count is not a word count.

The bounded [descriptor cut-source implementation](legacy-main-descriptor-cut-source.md)
now supplies the first source replacement stage. The global graph and NN-context
stages below remain separate; the existing single-line recognition guard stays.

## Pinned inputs and source chain

The observations below use static PE inspection only:

| Input | SHA-256 |
| --- | --- |
| `research/myscript/sdk/build/x64/MyScriptText.dll` | `18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f` |
| `models/ru-RU/ru_RU-ak-cur.lite.res` | `7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978` |

The input is the actual fresh main owner after descriptor grouping/reordering,
first TEXT publication, source720 transfer, guide processing, `24D`, active
`29B`, and factor200 `265` preparation. The existing
`LegacyFreshMainActiveGapSmoothed` retains this ownership. This is main source247
at resource offset0, with sampler297 at188157, category profile188222,
graph21B at189379 and ordinary21D at189734. First-child source6120 and its
sampler/graph/frame are separate owners.

`1A61C0` obtains the descriptor count from source218.V8, allocates one local
graph record of136 bytes per descriptor, then calls `19F850` with the descriptor
ordinal. Its ordinary local path includes `19C280`, `1A1EF0`, `1A4F10`,
`1A5700`, `1A5920`, and local `1A5B00` pruning. Only after processing the
descriptors does `19E8B0` append their local graphs to the shared graph. Subsequent
processing compacts live wide96-byte groups to compact56-byte groups.

## Cut points share one changing source

Sampler297.V38 is `193380`. Its descriptor argument is significant even where
the decompiler omits a virtual-call operand:

| PE instruction | Ownership evidence |
| --- | --- |
| `1933A2`, `1933B7`, `1933BB` | Preserve input R8 descriptor ordinal and pass it to source218.V10. |
| `193401`–`193424` | Read descriptor I32+10 as line ID; reject−1; call source218.V40 with that ID. |
| `193449`, `193453` | Read selected guide height+4 and baseline+0. |
| `19345C`, `193480`–`193493` | Pass descriptor physical-first+24 and physical-last+28, inclusive, to `191950`. |
| `1934B7`–`1934C0` | Pass null order to source218.V30 after point preparation. |

`191950` prepares geometry and markers only for that physical interval, but its
replacement point buffer includes the untouched physical prefix and suffix.
Selected records and markers retain global physical indices. The selected point
map starts at the original prefix point count, rather than at zero. Prefix and
suffix entries also participate in the returned complete source map.

Source218.V30 (`168B40` through `16AA00`) composes the replacement map onto the
existing source chronology. Thus a later descriptor reads points already
replaced by earlier descriptors. Insertion can move flattened global point
offsets while preserving physical-stroke ordinals and local point ranges in
earlier disjoint intervals. Original parent point identities and the factor200
preparation map must survive all replacements.

The marker workspace is not a cumulative global marker array. At the end of the
ordinary atom loop, `1A0A2D` loads the sampler from graph+180 and
`1A0A3F`/`1A0A43` calls its V30. The actual target `18DD50` zeroes marker count
1E8 and prepared-record count1F8. It does not discard the shared source point
replacement. Sampler V40/V50 (`1935D0`/`1935E0`) enumerate the current descriptor's
12-byte marker records; V58/V60 (`193690`/`193720`) compose physical/local ranges.

Consequently, concatenating independently prepared 28-byte record buffers is
insufficient. The native operation replaces a complete XY buffer per interval;
the temporary geometry records and final shared XY view need distinct owners.

## Limits and pending descriptor metadata

The actual main297 version106 and absent optional category-limit owner give the
already restored [marker-limit policy](legacy-main-closed-physical-marker-policy.md).
The ordinary marker gate/width still use each descriptor's actual guide height.
Per-physical source probabilities are read with global physical indices. They
must not be averaged, replaced by constants, or silently indexed by a sliced
local ordinal.

A kind3 descriptor with QWORD+20 equal to−1 takes `19F850`'s pending-control path:
`19FA17`/`19FA21` test those values; `19FA2C` sets runtime1F0; `19FA3F` appends
control `FFFFFFF9`. It creates no ordinary NN group for that descriptor.
Subsequent `19EB10` transfers pending flags/control values into a32-byte context
record referenced by the next group. Disabling explicit special-group creation
does not make that metadata disappear.

The explicit-special-group switch is graph1D8, from configuration byte3. Its
binding is separate from optional wrapper flags C4/C5 and the frontend23F test:

1. `150E41`–`150E8E` sets frontend91 when container.C4 is nonzero, selected source
   V58 is137, or the selected alphabet contains U+F0009.
2. `1514DF`/`1514E7` copies frontend91 into byte3 of the configuration whose
   address is passed at `151593`/`1515A7`.
3. `1A6BD2`/`1A6BD6` copies configuration byte3 into graph1D8.

For the separately owned NEW-container, no-external-property source135 scope,
C4 is zero and the actual acoustic alphabet lacks U+F0009, so this switch is
false. This does not establish the value for reused/external native containers.
Even in that fresh scope, pending kind3 context handling remains necessary.

## Local graph IDs cannot become global IDs by concatenation

`19D470` appends a local graph to graph+220. It first filters wide groups by
their live-reference byte+6 and creates a local-to-global group map. It copies
the surviving groups, deep-copies ordinary ranges, offsets non-FFFF context
indices, and appends the32-byte context records.

`19C150` copies each arc using that group map. Its start/end context IDs receive
the previous global node count shifted by7. Its non-FFFF auxiliary context index
receives the previous global context count. Span, placement, flags, scalar fields
and predecessor/successor lists are retained. `19D270` stitches root connections;
root/lookahead cases also belong to the append operation. Dead local groups mean
that adding the old global group count to every local group ID is incorrect.

`19E8B0` then calls `1A3B30` across adjacent nonempty descriptor graphs when
graph1DB is clear and the span cap permits it, followed by boundary-window
pruning. The fresh constructor clears1DB and `1A6980` does not replace that byte.
Cross-input merging is a reachable actual main branch:

| graph resource | span cap, node+16 | flags at node+306..308 | runtime158/159 wire bytes, node+349..350 |
| --- | --- | --- | --- |
| main189379 | 8 | 1,1,0 | 1,0 |
| first12727 | 7 | 1,0,0 | 0,0 |

Reader `1AD9D0` assigns those latter bytes to runtime158/159. In `1A3B30`, a
zero runtime158 excludes differing input descriptor IDs; the actual main value1
instead applies descriptor metadata, line-ID and geometry compatibility checks.
The `crossSource` call also excludes same-input pairs. A two-line test whose
geometry happens to reject every pair cannot justify omitting this branch.

Wide group U16+0 is the input descriptor ordinal supplied to `19F850`, not a
physical-stroke ID. A merged group retains the left input owner. Frontend E8
(`152280`) resolves graph V68 (`1A77D0`) to a group, V78 (`1A7920`) to that
descriptor ordinal, then source218.V10 to descriptor I32+10 line ID. A group can
therefore use its left owner's guide while covering physical ranges from another
descriptor. Neither minimum physical index nor global group ID supplies its line.

## Source references, NN context and normalization

The current [source-range resolver](legacy-ordinary-graph-source-ranges.md) owns
one seed graph. Native forward resolution uses `1A9070` DFS, `1A8E60` dual source
keys, `1A9570` sorting and `1A9AE0` coalescing. A generalized resolver must follow
the actual global graph and preserve the original source chronology. Independent
local resolvers cannot reinterpret their local context IDs as shared graph IDs.

Lazy227 context production `1C9870` also iterates source descriptors. For fresh
ordinary descriptors (kind0 and I32+4C<0), it resets the reference extremum for
each descriptor, obtains that descriptor's guide, and enumerates windows within
its physical interval. The initial window starts after its first physical
stroke; the actual main profile permits up to three physical strokes per window.
Context record+0 stores the descriptor ordinal, not the guide ordinal; +4/+8
store global physical window bounds. Its unassigned-line geometry fallback is
not a substitute for `193380`, which rejects an unassigned ordinary line earlier.

The classifier must select context, normalization baseline/height, dynamic
weights and geometry using each group's retained input owner. Existing
single-line methods legitimately use one `SourceHeight`, `SourceBaseline` and
descriptor0. Generalizing only the sampler would leave those consumers wrong.
The source-probability and primitive-scope metadata must likewise retain the
actual original owner after point replacement and physical reordering.

## Minimum implementation sequence

The proposed first addition is an owning `LegacyFreshMainDescriptorCutSource`
with `Prepare(resource, LegacyFreshMainActiveGapSmoothed, token)` and a cut stage.
It should retain the exact resource, input, category/alphabet, descriptor and
guide owners; derive marker policies; and sequentially replace one shared point
buffer/map in native descriptor order. Its immutable per-descriptor results
should contain descriptor ordinal, actual line ID, inclusive physical interval,
guide, global-physical markers/ranges and the temporary selected geometry. The
output should retain one final shared XY/count view and composed chronology,
plus kind3 pending-control metadata. It should not expose a fabricated graph,
NN cost, recognized text or separator glyph.

Reusing local cut arithmetic is possible only with explicit rebasing and
prefix/suffix-map proof. Existing seed generation expects its own complete
physical domain; a global interval beginning above0 needs an owned adapter, not
unchecked reuse. This source producer is small enough to implement and verify
before the graph work, but alone would not enable multiline recognition.

The next stage must restore the global append/remap and pending-context owner,
cross-descriptor `1A3B30`/`1A37B0`, boundary pruning and global source-range
resolution. It must expose immutable global group-to-input-descriptor ownership
and the actual per-arc contexts. Only then can a generalized227 context/scorer
consume the same graph, source points and selected guide, with exact model and
source-reference identity checks.

Verification should compare an independently composed source-map oracle across
two disjoint intervals, inserted-point chronology, duplicate/reordered raw point
keys, and untouched prefix/suffix identities. Graph controls must cover dead
group filtering, endpoint/context offsets, pending kind3 descriptors, eligible
and rejected cross-input merges and boundary pruning. Actual two-word and
two-line raw inputs must run through the existing owned preparation; their
expected text must not select descriptors, masks, cuts or graph candidates.
Single-line byte parity remains a regression requirement.

The private static report checked11 exact instruction-boundary PE ranges,
seven297 vtable entries and both resource graph flag blocks. No native DLL was
loaded or executed, and no existing runtime API was changed for this roadmap.
No multiline recognition or accuracy result is claimed by these checks.
