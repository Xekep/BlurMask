# Same-edit library DOM preflight

`LegacyLibraryTextDomPatchCompatibility.Prepare(actualDocumentReceipt)` claims
one optional scratch-tree preflight over that receipt's exact transferred edit,
new library target, captured epoch, Seed/document, and pending C4D owner. The
document retains the service. Once claimed, selected-registry replay and
`ApplyToLibraryTarget` require its exact ready state. The optional original-INPUT
preflight is independent: if claimed it must also be ready before replay.
Neither result is native CBB/page acceptance.

Claim before any replay has entered and before target application. Preparation
and each live run revalidate actual receipt/source ownership; the target gate
protects its prior child throughout copying. Lock order is document, service,
target, then the shared BEE name pool. Public APIs do not promise concurrent
publication-scope disposal. Pre-entry cancellation leaves the service reusable.
Entered runs, including defined failures, cancellation and refused unchecked
clone-null, retain their diagnostic prefix and cannot be retried. Pending or
failed claimed services block both consumers; the old unclaimed path is unchanged.
Consumption also rechecks the captured target epoch. A ready historical result
cannot authorize registry mutation after a different explicit target operation.
The same target operation gate remains held throughout the receipt's replay
callback or apply, so another target operation cannot race that epoch check.

The algorithm is the ordinary operation2/4, kind4, parameterized no-history BCC
subset of Engine0E2870. It creates a separate scratch arena sharing the exact
target BEE names, clones existing children, allocates the parent map, and resolves
every path before simulating any row. It then clones insertion payloads, inserts
before an existing child or appends at the native index, or detaches a required
child for operation4. VB8 is insert-before, not replace; the actual replacement
export consists of removal4 followed by insertion2.
The existing tree and pending registry are never mutated by this service.

The native execution return and compatibility output are separate:

| Branch | Worked | Compatibility written | Compatibility |
| --- | --- | --- | --- |
| Successful scratch simulation | true | yes | 1 |
| Missing path/child or invalid append index | true | yes | 0 |
| Scratch constructor, initial-tree clone or map allocation refusal | false | yes | 1 |
| Unchecked payload clone-null boundary | false (library refusal) | no | caller residue0 |
| Entered cancellation adaptation | false | no | caller residue0 |

The final two rows do not invent defined native failure returns. In particular,
native0E2C60 passes a null payload clone into its next mutator without checking
it. The portable service refuses and poisons that branch. It does not poison or
claim the actual edit, so the existing target failure state is unchanged; the
owning document gate prevents bypass through its normal replay/apply entrypoints.
The target's separate public `Apply` remains an explicit library API, not an
acceptance controller.

Budgets are checked over the complete initial tree plus all row payload copies
before scratch allocations: at most65,536 edit rows,65,536 total nodes including
attributes,4MiB raw value bytes, depth128 including the insertion path and root,
and65,536 total path entries. Only real owned operation2/4 payloads with the same
document and BEE are supported; no public replacement rows or compatibility flags
are accepted. Root kind0 terminators and external/protocol nodes remain unsupported.
Repeated payloads count repeatedly because they are separate native clones.

Immutable state retains the service's provenance and a separate owner-free
`DiagnosticScratchTree` content value. That value is never published as a target
or accepted document. Scratch nodes are logically released at completion;
managed allocation/fault controls and storage collection are documented portable
adaptations, not emulated native addresses or page/history handles. A successful
preflight does not guarantee later live apply allocation success: the real apply
performs another payload clone and retains its existing partial-failure behavior.

## Verification

`LegacyDomPatchCompatibility.Probe` runs the four actual inferred inputs M,
two physical words, reversed physical order and a rotated word through adopted
configured publication, real CHAR/WORD/TEXT sources, detached DOM, edit transfer,
both preflights, selected-registry replay and explicit library-target apply.
No expected transcription is used. It compares complete pre/post tree values and
identities, and checks that preflight changes neither live target nor registry.
It also creates a genuine repeated library formatting invocation from the same
immutable handoff to test an actual op4/op2 replacement; that is explicitly a
library replay, not a second inference or a fabricated native prior page.

The independent Python oracle consumes actual copied tree/edit payloads and
models initial-tree copying, all-path mapping, native operation ordering and
full output trees with allocation/name identities.2,048 prepared tree controls
exercise missing paths, negative/invalid indices, replacement, detached mapped
parents, all-path-before-edit discriminators, setup faults, unchecked payload
clone refusal and row-boundary cancellation. Genuine fresh receipt structural
incompatibility is unreachable and is not relabeled as an actual recognition case.
Named node/depth/foreign-owner guards and pending/closed/one-use consumption gates
are tested separately. The pinned source proof is
[the DOM preflight audit](legacy-library-dom-patch-preflight.md).

Managed and NativeAOT pass4 actual source documents, the real library replacement,
2,048 prepared full-tree cases and122 guards. Their UUID-normalized complete
capture SHA256 is76c039d2fd9ba46f3ec475266915f73d2405ac02fe88f58be2f16a7880731d75.
The direct mutator audit adds3 complete PE regions,4 table bindings and25 pins
to the earlier83-pin preflight proof. It also corrects the earlier roadmap's
VB8 replacement wording using the actual insert-before pointer writes.

Run `python tools/LegacyDomPatchCompatibility.Probe/run.py PROGRAM CAPTURE`.
The output auditor derives expected scratch results from raw input trees/rows;
it normalizes only the environmental `gen` UUID and its exact diagnostic value
trace for Managed/Native parity.
Old document, registry replay, original-INPUT preflight and target tests remain
separate regression checks. No SDK binary is loaded or executed.
