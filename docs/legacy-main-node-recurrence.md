# Owned main node rows and ordinary punctuation recurrence

`LegacyOwnedFreshMainWordNodeBuffer` owns the logical row storage for one
fresh DIRECT main-controller node under the prepared `2/2/1` configuration.
It consumes the existing source-bound controller policy and explicit full
allocation bytes. It does not create native pointers or claim the grouped
`1FA300` allocator/ring layout.

## Native producers and consumers

`2144F0` prepares the primary extent from `230A` and the expanded row extent
from `230C`, doubled when packed source bit `10` is present. The actual fresh
resource has 125 primary slots and 156 expanded slots. Native initialization
writes impossible to the two row costs and the first primary alternative's
two costs for all125 slots. The second alternative and other allocation bytes
are preserved. The buffer requires these allocation bytes explicitly.

Primary and language writes consume the actual `213410` materializer. Before
language, `2144F0` sets primary row `3E` to `FFFF`, including unused impossible
rows. The source extent starts at125, rather than the number of finite primary
rows. Language records begin at ordinal125. `212B10` compacts the language
tail; the buffer preserves both the original source end `23F0` and retained
end `23F8`. They are different inputs: `203690` reads `23F0`, while `1FE740`
and `203C90` use `23F8`. Primary-alternative cleanup writes only cost00 when
the unreset alternative's parent ordinal points into the language tail.

The buffer enforces its wrapper lifecycle: primary writes, one language start,
language writes, one compaction. This is a logical ownership contract, not an
assertion that native heap arrays have the same allocation order.

## Punctuation and quote scope

The previous ordinary `201270`/`1FE740` wrappers imposed a Unicode
Latin/Cyrillic/digit filter that the native ordinary query branch does not
contain. Their guards now accept actual nonvirtual, single-BMP AL entries.
Replacement `FFFD`, join control `200C`, shadda `0651`, virtual/private
primitives and the supported context protocol controls still require their
separate owning paths. The existing native loop/absent-auxiliary skip in
`201270` remains before the unsupported branch.

Both native routines force two category attempts for apostrophe and double
quote even with input flag `10`. The first attempt uses `StartCompositeMap`;
the second uses `EndCompositeMap`, stopping when the second entry is absent.
The fields `24BA`/`24BC` are resolved from the actual alphabet, not inferred
from Unicode categories. These consumers still exclude associated-service
rewrites and extra query contexts. Apostrophe contexts containing joiner
history are explicitly refused because their composed `ZWNJ/FFFB` rewrite
is not produced here; no replacement context is fabricated.

The geometry widening remains bounded to the existing same-descriptor facts.
In `1A8670`, pointer equality of the actual input descriptors with low flags0
returns status0 **before** the later special quote matrix at graph `0C`.
`1A7F20` likewise reaches status0 in this scope. Therefore the existing main
sampler `1941B0` cache and translation consumer remain valid for these ordinary
punctuation labels. Cross-descriptor quote matrices, endpoint metadata,
boundary networks and other group kinds remain unsupported. No integer graph
matrix is replaced with the sampler's distinct float gap matrix.

## Verification

Run the headless probe with actual model files:

```powershell
dotnet run --project tools/LegacyMainNodeRecurrence.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/main-node-recurrence-managed -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

The probe independently checks128 full-allocation byte controls, distinct
source127/retained126 counts, exact alternative cleanup and lifecycle guards.
Its real single-descriptor fixture consumes all39 root queries and33 terminal
queries, producing33 real language records. The next owning restart page has
nine actual queries including apostrophe; every returned query is consumed.
Both input flags0/16 are checked against an original point-range/resource
geometry oracle and hand-written full128 record overlays. This yields19
alternative records, two explicit parent-consumer quote controls and192
geometry controls. The parent controls consume an actual returned quote and
actual LM parent; they do not claim that the outer controller routes secondary
rows through `201270`. NativeAOT runs the same resource-backed controls.

The earlier parent, alternative and geometry probes retain their original
goldens, including signed-wrap packets, first-parent ties, heap limits and
7632 original geometry callbacks checked by the Python oracle.

## Remaining orchestration

This stage connects actual whole-prefix startup, language materialization and
the next restart-page consumer. It is not a complete `2144F0` node/span loop,
case292 dictionary factory, final main winner or acceptance-confidence policy.
The fixture's first-parser proposals retain their earlier diagnostic scope.
It does not enable production Russian AI-pen replacement or substitute Latin
lookalikes with Cyrillic characters.
