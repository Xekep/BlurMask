# Concrete retained Ink for the fresh Math CB8 path

The fresh publisher now owns a concrete third InkOwner with private registry and
C22 objects, instead of a standalone identity object plus registry records.
This advances the [remaining owning join](../../../tools/handwriting/ports/cb8-owner-20261028/retained-owner-map.md),
but it is **not a nonfresh result owner or complete recognition integration**.
The fresh source's no-prior-mathResult guard remains. Russian/English cursive and
full-engine readiness are not established by this increment.

## Actual source and retained objects

[Math2Mode0RetainedInk](../../../src/InkRecognition.Math/Math2Mode0RetainedInk.cs)
defines the internal owner operations. Its concrete implementation is private
inside [Math2Mode0OriginalMathFieldSource](../../../src/InkRecognition.Math/Math2Mode0OriginalMathFieldSource.cs),
where it can clone the actual selected InkOwner/RegistryEntry/Selection objects
under the field gate. It preserves IDs, names, category, Next and range bytes,
gives cloned C22s their new owner and retains the same immutable raw document.
The selected and original registries remain unchanged. No factory accepts a
caller-created registry snapshot as its source.

The trusted publication input now supplies a factory closure. Its source/receipt
checks run again on invocation. The publisher invokes it only after complete
payload-budget preflight, the one-use claim and owned outer-vector construction.
The clone is checked for distinct Ink identity, the shared actual DOM name owner
and preserved Next. Existing conservative payload reservations and source/NN caps
are unchanged; this is not a GC/RSS bound or a throughput claim.

Pending entries retain actual C22 handles created against this clone and actual
shared-domain name tokens. Late registration uses that same clone's registry
and Next. The previous independent list of final registry records and publisher
counter are gone. Registry projections used for inspection/transfer derive from
the concrete entries; projections themselves confer no C22 authority.

The supported fresh scope still requires nonempty produced ranges inside the
actual INPUT and registration names MATH_SYMBOL/MATH_NODE, category102. Empty
pending-selection production and imported/nonfresh registry factories are not
admitted. Canonical range equality uses the existing exact range service. The
shared-domain name comparison is used for equality of this caller's canonical
names; no arbitrary cross-owner naming protocol is claimed.

## Live gates and inspection

Operational lookup, name/range comparison, registration and removal require the
same actual parent field gate and a live retained clone. C22 operands must be
private Selection objects belonging to that exact clone; another clone of the
same document/name domain is rejected. Name operands must belong to the retained
shared DOM name domain. The wrapper's logical disposal never disposes the parent
field, original/selected Ink, shared names or borrowed first-copy pool.

SnapshotNext/CopyRegistry/CopySelectionRanges provide immutable inspection data,
including after logical disposal, matching the existing diagnostic snapshots.
They cannot reopen the owner or permit mutations. Like the prior snapshot data,
the retained managed data lives until the publication/inspection references are
collected; no native SDK allocation is involved. Diagnostic operational borrowing
uses receipt validation before the field/publisher gates. No reverse owner->field
lock acquisition is introduced.

## Connection to the already ported CB8 algorithms

The concrete clone implements the [typed matching and removal services](math-dom-cb8-edit-kernel-20261028.md).
Produced fresh opcode2 insertion actions now pass through the actual matcher
under that owner. Their unused old/new variant fields are never read: opcode2
returns false before conversion, as native032800 does. This is the genuine
early-return path, not a manufactured opcode7 action or prior result.

The callback receives a lazy view of actual private pending entries in native
head-to-tail/prepend order. It retains their real C22/name handles without making
an operand-array copy. Historical diagnostic snapshots keep their insertion-order
format so prior failure captures remain directly comparable.

The same concrete Ink is passed to stale-tag cleanup. The existing empty-old-tag
guard is retained, so actual fresh publication performs no deletion. Nonempty
cleanup controls and opcode7 branch controls remain separate kernel tests. A
real prior-result DOM, produced diff, transaction and updated source factory are
still needed before admitting repeat editing of an existing mathematical result.

## Verification scope

The seven-function native dependency packet was reverified against the original
PE:650 instructions, no vendor execution. Static source-join checks cover the
concrete clone factory, owner/name checks, late registration, callback projection
and transfer path; the native435 instruction pins and existing ordered joins stay.

The actual model/official-ORT fresh publisher probe adds216 ownership controls:
distinct original/selected/third identities, shared canonical names, actual C22
and name lookups, range equality/count, detached range-copy mutation, unknown ID,
fake/foreign C22 refusal, preserved Next on refused registration and out-of-gate
operation refusal. The foreign C22 is created by a separate genuine clone of the
same source document and name domain. These are actual owner checks, not the
earlier synthetic service objects.

Removal of an absent ID succeeds without changing Next. Static review of the
original Ink069630 implementation verifies the absent-ID return at069668; the
probe deletes the same actual selection twice and then an unknown ID. This
corrects the initial use of Dictionary.Remove's false return for absence.
The singleton fresh clone guard also remains local to the factory: a future
multi-entry clone must preserve shared C22 aliases explicitly.

The existing independent fresh, outgoing and original-application oracles still
cover six actual NN cases each (cold1/prepared3/16, normal/reversed),70 fresh
failure prefixes/180 guards,64 outgoing prefixes/168 guards,138+8 original
prefixes/379 guards, concurrent claims, inverse/source/DOM failure handling and
borrowed-source GC. No capture schema, budget, input or acceptance threshold was
changed to get a passing result. The current numeric/DOM/mode2/CB8 kernel controls
remain544621 checks against the actual compiled library.

Local staged evidence is under
`E:/BlurMask-build-artifacts-20261014/math-retained-ink-20261028-v4/`.
All71 authored Math files and both numeric-probe files are bound to portable PDBs;
the three owning probes also authenticate authored/generated PDB documents and
copy the exact reviewed Math DLL. The stable core DLL is explicitly pinned.
The reviewed Math DLL SHA256 is
`7cf3293226d19992dc5645622c29bc02fea215bcf15908ceba8a4b229e0ebb2b`.
All three local managed and Windows Native AOT pipelines passed their independent
oracles, with byte-identical managed/native captures. The final static audit
also checks84 consuming-source pins and authenticates the absent-ID removal
path against its pinned Ink PE; it does not execute vendor code.

The dedicated `Math retained Ink ownership` workflow now runs these three actual
owning pipelines in managed and Native AOT builds on Windows, Linux and macOS.
It fetches only the pinned official CPU ONNX Runtime and exports the already
tracked repository resource locally. Uploaded evidence excludes the resource,
model bundle and native binaries. This focused workflow does not establish all
application/research gates or release eligibility.
