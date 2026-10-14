# First acoustic symbolic context passes

`LegacyFirstOrdinaryContextPasses` implements the fresh23f ordinary **plain
label / zero-Arabic-trait** B0 and B8 branches of Text1ef070. It consumes the
real first primary recurrence and graph50/58 connections. It is not the main
class240 linguistic decoder, a confusable-letter normalization rule or final
Russian handwriting support. Nonordinary/primitive/fallback labels, aliasfold,
punctuation-skipping ancestors, source-scope/cross-descriptor corrections and
the640-byte diagnostic payload remain outside this scoped API.

## Native record and symbolic behavior

The producer walks nodes in order, B0 before B8 at each node, using one fresh
shared0918d0 symbol-context container. Every node retains fixed F8 rows and
explicit A2/B0 or A4/B8 columns. Rows refer to the corresponding primary-record
ordinal, rather than A8's distinct-arc group ordinal. Active counts and
impossible sentinels do not change the grid stride.

Roots copy all40 bytes of their primary record, then replace qword18 with the
initial symbol-context identity. Nonroot candidates re-query their actual
ranked glyph/category cost, scan compatible previous primary rows and stored
context alternatives, and call frontend140 **before** frontend118. This
reverses the primary recurrence's callback order. Callback rejection and the
checked signed cost domain are preserved. Native glyph/raw acoustic sum is
kept separately from the total including geometric corrections.

B0 follows primary packed pathbit0: false uses kind0 and extends the previous
symbol path; true uses kind2 and starts a root symbol path. B8 with native E0=0
tries kind0 and kind2, where kind2 inserts control symbolFFFB between the
previous context and current symbol. With E0=1 it tries the primary-selected
kind and extends the previous context without inserting FFFB. The existing
AVL/page factory retains shared identity for identical symbol paths; native
1ee030 inserts signed costs and replaces a worse identical identity in place.
Portable IDs represent symbolic identity, never native addresses.

Nonroot byte9 is the previous primary-row ordinal; byteA is the previous
context-column ordinal. Packed span/category and the kind choice are written
by1ee030. Unwritten likelihood byte8 remains zero in newly allocated managed
nonroot output slots; the root copy preserves the original byte8. The library
does not claim arbitrary native allocator residue is zero. Returned record
and grid storage are copies. The explicitly supplied symbolic container is
live mutable native-style state; callers must serialize its access and must
not restore/reset it while interpreting these records. Cancellation can leave
earlier completed symbolic allocations, but returns no partial result.

## Policy ownership and remaining gap

Aliasfold88=0 is proven through child249 binder156630 writing owner+11=0 and
first iteration1ef070 reloading that value. Selected child249 capsA0/A2/A4=1
are separately proven by the binder. Native E0 is **explicit**, without an
invented default. Initializer1edde0 enables it when owner.C4 is nonzero, source
virtual58 returns137, or the acoustic alphabet contains U+F0009. The owned
first RU source is135 and its inline alphabet lacks F0009. Constructor00c860's
C4=0 alone does not prove the value after property application; that upstream
configuration path is still under investigation. Both explicit E0 branches
are tested, rather than selecting one because its output looks plausible.

## Independent and actual checks

256 vendor-free fixtures use independent Python symbolic tuples instead of
the managed AVL factory, checking12305 B0/B8 records across both E0 branches,
different caps, signed costs, stable ties/in-place identity replacements,
root copies, predecessor row/column fields and inserted space-control paths.
Meaningful cancellation checks cover glyph-cost, callback140 and callback118,
including suppression of118 after canceled140. Bounds/fresh-container and
record-copy ownership are checked.
Managed and freshly published Windows Native AOT pass the same controls and
actual resource diagnostic; their captured symbolic/scalar buffers also pass
the independent actual replay.

Actual resources are exercised from an original printed MIR drawing through
the raw one-descriptor packet, assigned line0,24a/265/297,35 firstNN/final195
groups, complete nonroot primary recurrence,19 active A8 groups, and fixed
F8=5/A4=1 B8 grids. Each explicit E0 branch yields90 finite B0/B8 records over
9 nodes. Independent Python replay of captured actual primary records,
graph connections, glyph costs and callback inputs reproduces their scalar
fields and full symbolic paths:540 distinct callback requests for E0=false,
270 for E0=true.

The five final B8 diagnostic paths are the same for both explicit modes:
`MИP:41268`, `Mrrp:42211`, `Mrrro:43748`, `Mrrр:44513`, `MlТ:45221`.
These remain mixed-script acoustic alternatives. They are not returned as
the application's Russian result; native terminal273 materialization and
the genuine main240 linguistic stage remain separate. A synthetic printed
fixture is not a representative cursive accuracy assessment.

```powershell
python tools/LegacyFirstContextPass.Probe/reference_controls.py D:/controls.json
dotnet run --project tools/LegacyFirstContextPass.Probe -c Release -- D:/controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
python tools/LegacyFirstContextPass.Probe/reference_controls.py --audit D:/controls.json.actual-e0-False.json
python tools/LegacyFirstContextPass.Probe/reference_controls.py --audit D:/controls.json.actual-e0-True.json
```

The optional actual mode writes private captures next to the controls file:
complete B8 and B0 buffers, primary records, callbacks, graph lists and portable
symbol-context mappings. No vendor DLL is loaded or executed.

`RunForFreshSelectedChild` additionally accepts the owned
`LegacyFreshFirstTextContextPolicy`. It uses that policy's alphabet, column
caps and proved E0 choice without a caller mode switch. The actual prepared
MIR fixture matches the independently audited explicit-mode scalar/metadata
fields and every finite symbol-context path in both B0 and B8. Stable numeric
context IDs remain separate per-container identities. This convenience entry
point retains the same plain-label scope; it is not automatic text/math
classification or the main linguistic recognizer. Invocation score flagA8
still requires the actual source-frame150 flag and is not guessed here.
