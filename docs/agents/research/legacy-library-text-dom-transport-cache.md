# Explicit library DOM transport cache

This stage transfers the already exported library BE2 edit together with its
actual pending C4D transaction. It does not call C20.V188, execute observer or
history callbacks, apply the DOM edit, or expose an accepted document. Its
CreateFresh(target) is an explicitly library-owned cache constructor, not a
replacement for the unproved native05F720 receiver A.

The pending C4D is an existing privately created owner, not a caller command
array. The same selected INPUT/C22 context creates the shared3E selection
owner before WORD publication. CHAR, WORD and TEXT append their actual
operation4 commands to it; the final07AAF0 operation7 counter command closes
that append phase. The retained source formatter's CounterHandoff exposes
that same owner and an immutable command snapshot. Prepare(edit) validates
the completed transforms, seed, document, selected INPUT context, exact
transaction owner, command object identities and linked order. No public
transaction, receiver, version, success or history flag is supplied.

The cache is bound to the exact library target and seed. Prepare(edit) claims
one receipt per actual edit and per actual pending transaction, including
after disposal. A second source-formatting invocation over the same handoff
cannot manufacture another owning C4D transfer. The supported producer owns
one actual transaction; multiple cached entries, reverse edits, incoming
history and native C8F copies remain unsupported.

TransferTo(cache) preserves11B860's allocation boundary: count0/capacity0
reserve exactly one40-byte row. A failed reserve leaves both incoming slots
and the cache unchanged. Successful append retains the actual export and
actual C4D as typed sidecars, versions00/08=-1, and null reverse edit18.
Only after that append succeeds are the incoming edit/C4D slots cleared.
CopyScalarStorage40 returns all40 scalar bytes: native pointer lanes10/18/20
are zero placeholders backed by the exact typed owners, never fabricated
addresses. All assigned nonpointer bytes agree with the native writer.

The target operation scope surrounds transfer preflight and append. The lock
order is target, cache, receipt. Dispose never acquires a target monitor.
Pre-entry cancellation is reusable; stale, foreign-target/seed/selected-source,
disposed and replayed receipts are refused before ownership moves. Prepared
internal allocation failure represents11B860's fallible reserve service;
it is not a public caller success flag. A retained failed reserve may be
retried by the owning receipt, or the caller may dispose that receipt.

Disposal is an explicit managed adaptation. Disposing a cache clears its
own row leases and typed sidecar slots; disposing an untransferred receipt
clears its incoming slots. Repeated disposal is harmless. Neither operation
invalidates or mutates the borrowed immutable recognition commands, source
owner, caller's DOM value, earlier snapshots or raw registry. Released row
sidecar access is refused, while its immutable40-byte scalar copy remains
available. Existing managed aliases keep their data alive until collection;
this does not simulate native frees or claim native handle validity.

The separate probe runs four actual raw-input pipelines through selected
INPUT, shared WORD/CHAR/TEXT, postprocessing, counter handoff and BE2 export.
It verifies three successful row transfers and one disposed incoming owner,
four reserve-failure prefixes, cancellation, failed/stale export refusal,
same-seed foreign target, different-source cache, disposed cache, duplicate
transaction and replay guards. It checks exact owner references and that
all24 actual pending commands and target trees remain unchanged. An
independent Python oracle checks all row bytes, command categories/ranges,
counter bodies and transfer/disposal state transitions. No expected labels
enter inference.

The native proof is the committed cache-acceptance owner audit (13 regions,
18 bindings,89 pins), together with fresh pending-context and counter-handoff
audits. Actual BEF/CBB/CB7 request/history binding remains a separate static
task. This transport cache does not close that acceptance boundary.
