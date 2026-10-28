# Shared global main word-controller integration

This bounded port adds an explicit typed global
CASE facade around the same owning main240 node/span scheduler. It never
constructs a single-descriptor graph, converts sampled ordinals to raw document
IDs, creates fake seed IDs, clears native pending flags, or exposes a global
source through the original strict DIRECT/same-source getters. It changes no UI
or default recognizer.

## Owner and API plan

The new internal `LegacyOwnedMainWordSourceServices` is a private sum type of an
existing `LegacyOwnedMainWordSpanPreparation` and the actual
`LegacyFreshMainGlobalCaseSession`. Its produced `SpanLease` retains the original
old/global span and map epoch. It delegates `Prepare`, `AdvanceNodeEpoch`,
`ValidateSpan`, `GetCosts`, `Read`, terminal70, restart50 and qualification to the
same owner. There is exactly one CASE dictionary backend/context factory; no
parallel DIRECT source or duplicate mask refresh is created.

`LegacyOrdinaryRecognitionGraph` retains its original graph identity and now
copies actual flags and start/end context keys alongside group/span metadata.
Successors come from the original connections for old sources or the actual
compact global selector(span+1), including the actual root. Group IDs retain
allocation identity. Global group193860/195520 costs and the private pooled
four-family final-cache rows are reused through the current D0 lease.

New public `LegacyFreshMainGlobalWordControllerPolicy.Prepare` binds the actual
main0 resource, global CASE session, group-cost owner and118/140 owner by reference.
Fresh scalar refresh/limits use the existing checked native kernels and the
selected resource caps. No external properties, associated source, tracing,
configured secondary count or manual mode booleans enter this factory.
`LegacyFreshMainGlobalWordPass` produces a separate immutable logical-row result.
It cannot be passed off as a legacy same-source result or decoded text.

The shared scheduler retains real V80(-1, then each completed node), ascending
D0 spans, persistent outer290 and one inner292 source. Initial/parent/flag and
language-gate consumers have narrow internal binding paths. Their original
public constructors and getters remain strict. First/later completion private
binding interfaces retain actual full source tags, metadata70 and restart50,
and validate the current source lease around external callbacks.

## Required pending-boundary branch

The global graph contains genuine pending kind3 contexts. Status100 marks
request3C rather than silently treating these as an ordinary same-line gap.
`1FCF40` handles marked ordinary requests by querying frontendE8 with the original
source reference04/0E and source node04+1/arc0, not previous ending08/10. It requires
an assigned previous line, a line change of exactly+1 and the previous source
kind4/6/7/8 (or separate exceptional policies). The selected C00 source has kind6.
`LegacyGlobalOrdinaryWordPathGuard` implements this actual kind0/2/C00 branch
through the same global group descriptor and assigned line owners. A native
false decision drops that candidate normally; it does not throw for an expected
negative or clear3C. Kind7/source-kind5 intervals and other exceptional policies
remain explicit unsupported branches.

## Scope still to close

The shared language loop currently invokes only the proven first-word completion
branch. Its helper preserves the native root-anchor guard. The selected fresh
one13B->292/290 source still has V60=false/noAssociated; this batch proves no new
associated or restart source and creates no nonroot anchor. The later-history
204290 schedule and language context recombination are not yet called. A source
that reaches those missing branches must fail, not inject the current terminal
or restart at a fabricated root. The new later completion binding is only its
owner/lease adapter, not a replacement for that missing producer.

Global live207CB0/intermediate publication and final compact/text ownership are
also not supplied here. The global logical-row facade explicitly refuses more
than120 nodes, because at node119 with later nodes native2144F0 must publish
beforeV80. It cannot silently omit that required stage. Final winner, Unicode
publication, raw document positions, acceptance and handwriting quality are not
claimed by logical node rows.

All allocator bytes remain explicit portable allocation residue. Numerical
probe/oracle fixtures use zero-filled supplied buffers, including auxiliary
bytes24..31. This is not a claim that native malloc is zero-filled. A prior
poisoned auxiliary side-pointer fixture correctly refused the unsupported query
extra-cost branch; it was not normalized away in runtime.

## Checks and reproducible commands

Managed and fresh win-x64 Native AOT pass eight genuine raw-source preparations,
including two-descriptor/two-line inputs: 110 global nodes and 631 distinct NN
groups, each evaluated once in the actual source session. These are deliberately
drawn probe geometries, not a writer corpus or Russian accuracy measurement.
The probe supplies no expected text, fixed main frame, all-enabled mask or
fabricated first result. An independently owned-session procedural oracle
compares 2,610,080 complete scalar storage bytes and 72,160 symbolic context fields.
It constructs separate pools/maps/contexts and follows its own scheduler rather
than using the new pass result as expected rows.

The raw-resource Python audit decodes actual 88-byte source descriptors and checks
all 1,180 marked path decisions (40 true), then delegates to the independently
packed raw LK/AK arithmetic oracle: 961 real language rows, 288 terminal nodes,
all 12 case categories, original anchor geometry and fulltag restart. Static-only
audit pins 4 PE blocks, 52 instructions and 2 dispatch targets, including V80 before
spans/after completed nodes and mandatory live207CB0 before the next node.
There are 45 additional ownership, stale epoch, cancellation, single-use and
snapshot-isolation guards. Managed/Native capture SHA256:
`d0a1ddd599e5c63e779e122a852750222b648c3bcc21890e064da0e90fc823da`.

Old DIRECT4/56 full-storage procedural oracles, CASE4/56, initial special
candidates, parent candidates and language gate pass without changing goldens.
Old CASE later completion retains 339 explicitly prepared-history cases and 35
guards; old live compact publication retains 37 guards and byte-identical
Managed/Native capture. Its independent raw LK/compact audit also passes.
Configured secondary storage remains a separate existing public path.

```powershell
dotnet run --project tools/LegacyMainGlobalWordPass.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/main-global-word-pass-managed-artifacts -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json models/text-unicode/legacy-case-data.json models/text-unicode/legacy-text-publication-data.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res E:/BlurMask-build-artifacts-20261014/main-global-word-pass-managed.json models/ru-RU/ru_RU-lk-text.lite.res
python tools/LegacyMainGlobalWordPass.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res E:/BlurMask-build-artifacts-20261014/main-global-session-case-controls.json E:/BlurMask-build-artifacts-20261014/main-global-word-pass-managed.json
python tools/handwriting/audit_main_global_word_pass.py --output E:/BlurMask-build-artifacts-20261014/main-global-word-pass-static.json
```

The case controls argument is the unchanged output of
`LegacyCasePreparation.Probe/reference_controls.py`; it contains independent
Unicode class expectations. CI owns its generated output location. Native uses
the same nine arguments as the managed probe, with a distinct capture filename.
These checks do not establish a later-word scheduler, final publication or OCR
quality. No vendor library is loaded or executed.
