# Common owned main span and controller policy

The additive `LegacyOwnedMainWordSpanPreparation` constructor accepts
`LegacyFreshMainSingleLineSampled` and its matching
`LegacyFreshMainSingleLineContext`. The context must reference the exact sampled
source. The dictionary/model/resource checks remain those of the owned
dictionary and actual main acoustic stage. No singleton is manufactured and no
first-source frame or neutral context is supplied.

The common `SourceGraph`, `SourceInk`, `SourceHeight`, `SelectedSourceOwner` and
`AcousticPolicy` properties support code that operates on either restored source
variant. The earlier `Source` and `Acoustic` properties retain their original
single-glyph types and behavior. They explicitly refuse a multi-physical owner;
that variant uses `MainLineSource` and `MainLineAcoustic`.

The existing D0 span selection, epoch, cache reuse, permission-byte production,
working-map refresh, finite-class union, query dispatch and scalar geometry-cost
path now use the common source. `LegacyOwnedFreshMainWordControllerPolicy`
checks the same actual selected metadata for either source variant. The node
buffer's domain guard reads `SourceGraph.NodeCount`. The row budgets, parent
distance, record layout, cost thresholds and dictionary/query algorithms do not
change. This is an owning adaptation of previously restored stages, not a new
claim of automatic controller/decorator selection or full search scheduling.

`tools/LegacyMainLineSpan.Probe` constructs the actual14-physical fixture from
[the multi-main ingress probe](legacy-main-single-line-ingress.md), then checks
all420 D0 spans and420 selected NN/cache rows (81,900 class values). A separate
scorer over the same owned source supplies the actual reference costs. The probe
independently derives the finite unions, reserved/symbol permissions and filtered
working maps; eight initial source queries return79 real dictionary records
whose node, source, label and interned-context identities are checked. The
derived controller's125-row quota and120/20 scalar distances, stale-span guard,
failure invalidation, matching-source context and common node-buffer allocation
are also checked. This tests real acoustic-to-query dataflow without consulting
an expected word label.

Run with the same nine arguments as `LegacyMainSingleLineIngress.Probe`, changing
the project to `tools/LegacyMainLineSpan.Probe` and audit output to a separate
file. Publish that project with `-r win-x64 -p:PublishAot=true` for the native
check. Both `TEMP` and `TMP`, build artifacts and output files should point to the
task's E: artifact directory.

The existing single-glyph span, controller-policy and node-recurrence probes
remain regression checks. Higher scheduler consumers that still dereference the
old single-glyph properties require their own explicit common-owner adaptation;
this constructor does not make those old assumptions silently succeed.
