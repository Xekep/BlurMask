# Live global ordinary glyph refresh

`LegacyFreshMainGlobalOrdinaryGlyphRefresh.Prepare(ak, completedSources)` binds
the successful live global TOP.V68 completion to its existing glyph pool,
execution session, source overlay, compact rows and acoustic score caches.
It does not construct an old single-descriptor result, import control symbols,
evaluate another neural network, or change WORD references.

The supported source lifecycle is the existing pre-NN global publication
scope: fresh ordinary source135, no associated source, no tracing, no bit8,
one WORD and no pending metadata. The actual pool, graph, callbacks, geometry,
acoustic resource and completed-source lease must be identical. Nonnegative
canonical source-owner keys are required. Higher references retain the same
overlay's originalLast acoustic identity and use its actual merged ranges.
The original stored glyph descriptors remain; refresh appends or updates every
matching current compact descriptor, rather than replacing the selected row.

## Native order and explicit storage

The [static checkpoint](legacy-global-ordinary-refresh-plan.md) pins
`273.V40=13C6E0`, `240.V90=217670` and `274.V60=13A830`. The shared internal
`LegacyNativeOrdinaryGlyphRefreshKernel` is a mechanical extraction of the old
217670 loop. Its only substitutions are the span view, bound base key, scalar
resolver, and same-owner score/cost delegates. The extraction auditor compares
the entire whitespace-normalized loop with committed `532cdcf` and retains the
original native instruction pins.

For each compact row in the base node's exact 2478 interval, the scan requires
trait bit20, subtype0 and the full64-bit base source key. It then performs:

1. Append/find descriptor `compactIndex - nodeStart`, preserving explicit
   allocator40 residue and all previous records.
2. Resolve with conversion0, then apply the actual 13AF30 symbol update.
3. Read the normalized-original symbol's actual vector10 confidence and commit
   field10. This is distinct from candidate24.field0, byte21, and TOP mass.
4. Build the actual request16 with ordinalFFFF, actual geometry category and
   normalized symbol; commit integer cost14.
5. Commit byte21/255 to float0C and trait40 to flag04.bit8.

Before the compact loop, the source owner counts/fills actual V140 references
and stores26N bytes and the first-candidate vector score in typed sidecars.
These are not fabricated raw64-byte AVL headers. A portable aggregate cap of
65536 retained source references is checked before retention/copy; this is a
resource bound, not a native limit. The caller supplies the complete40-byte
allocator residue. No unfilled source allocation bytes are exposed as zero.

Native13C6E0 marks owner4000 after a refresh attempt, including callback false.
This bounded facade preserves ordered partial pool writes and poisons a failed
owner: retry throws. It does not claim to reproduce native failed4000 replay or
materialize an unknown raw owner header. Successful refresh is cached once per
same completed source and owner. `Select` subsequently runs the real13A830
selection over the entire owner, with explicit selection allocation and the
signed original marker; insufficient selection storage leaves refresh intact.

## Actual live V50 namespace

`LegacyFreshMainGlobalGlyphUtf8Emission.Prepare(completedSources, normalization,
circles)` uses the same current pool and **actual acoustic primary AL**. Fresh
TOP18 is a separate empty lookup-only control2A, while TOP20/30 are a non-null
empty alternate object. Kind4 therefore fails; it cannot fall through to the
primary AL. An absent control lookup returnsFFFF. An ordinary glyph uses the
existing per-glyph default0491A0/circled helper and its validated Unicode data.
No post-import2A or alternate acoustic namespace is substituted.

The emitter accepts a current exact40-byte record from that pool, checks the
completion lease before/after emission and rejects stale snapshots. It returns
per-glyph bytes only. WORD source choices, 024F30 CHAR/WORD/TEXT publication,
document acceptance and application confidence policy remain separate joins.

## Verification

The dedicated `LegacyGlobalOrdinaryRefresh.Probe` takes `MODELS CAPTURE`. Its
eight actual source/residue runs cover12 ordinary owners and24 appended compact
rows. All1440 resulting pool bytes, request16 layouts, scalar labels,
first-candidate channel, alternative order/marker and emitted bytes are checked
by the portable raw-resource auditor. Two actual two-physical sources produce
geometry8232; four actual requests distinguish applying that cost. The retained
four-family probabilities and final class costs come from the already audited
owning acoustic stage. The oracle recomputes calibration and geometry from raw
AK/table bytes; it does not claim a second independent NN implementation.

Four explicitly prepared internal-kernel controls observe complete pool bytes
at resolve, score and cost callbacks, including resolver failure, score failure
and cancellation at cost. Seven separately identified prepared emission cases
exercise empty control runs, zero-span runs, undefined subtype refusal and the
non-null-empty alternate failure. They are not decoded winners. Invalid-source
and disposed-lease controls verify failed-prefix poisoning and stale refusal.
WORD storage and cached NN counts remain unchanged.

Run from any working directory:

```powershell
dotnet run --project tools/LegacyGlobalOrdinaryRefresh.Probe -c Release -- MODELS CAPTURE
python -I tools/LegacyGlobalOrdinaryRefresh.Probe/audit_output.py AK SEPARATION CAPTURE
python tools/handwriting/audit_global_ordinary_refresh.py STATIC_REPORT
```

The static auditor alone needs private PE research files. The capture auditor
works with `python -I` outside the checkout and reads only repository code plus
the supplied raw resource/table/capture. Fresh Managed and Native outputs are
byte-identical. Old ordinary refresh, configured refresh and UTF8 emission
captures remain unchanged; the old public owner APIs stay strict.

The paired old captures are `9e5ff593...3aa4fc` (ordinary),
`9023988f...6ac7e9` (configured) and `b2cc13b6...0667dd` (emission).
A separate clean committed `532cdcf` checkout reproduced the emission capture
in both Managed and Native. The older archival `123f46f6` capture differs in
one existing descriptor00 value,199 versus207, already before this extraction;
all emitted UTF8 bytes match. No old fixture expectations or captures were
rewritten to accommodate this batch.

This stage does not establish full Russian or English recognition, new corpus
quality, calibrated acceptance, or application integration.
