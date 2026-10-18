# Original selected field worker owner

`LegacyLibraryOriginalSelectedFieldContext` restores a bounded worker-selected
copy from the same library original bootstrap, before any NN/publication. It
retains that exact original, CurrentView, raw Document and chosen INPUT entry.
It is not an SDK CBB/page context or a completed recognition request.

```csharp
var original = LegacyLibraryOriginalTextFieldBootstrap.Prepare(currentView,
    new[] { "left", "right" });
var originalState = original.Run();
var selected = LegacyLibraryOriginalSelectedFieldContext.Prepare(original,
    originalState.Inputs[1]);
var worker = selected.Run();
```

Here the chosen field/input tag is1. Both genuine original INPUT registry IDs0
and1 are copied into the child C20, each with category100 and nameINPUT. Their
nonempty full-domain C22s are rebound to the selected owner and interned by
content into one retained target C22. The child's Next is copied from original
Next2 after all entries succeed. Raw original object IDs remain their own
independent0..N-1 domain. Selecting field0 keeps both entries as well; it does
not change Next to1 or rename INPUT IDs to raw object ordinals.

Only the chosen field DOM subtree is cloned under a fresh selected
kind4/PAGE/namespace/content/layout root. Its literal id and its actual numeric
input.tag are copied, and every cloned node carries its actual original node
identity. New PAGE scaffold nodes have no original node identity. Namespace
kind2 stays distinct from ordinary attributes. Native0EBE40 copies a field's
attributes before children. String values retain their actual tag/bytes;
length15 is inline tag2, length16 and17 are heap tag1. The selected BEE name
owner is new, while canonical names remain shared inside that owner.

`HasNativeReaderContext` is false by the concrete bare BCC V200/V1F8 bindings.
Document00C590 calls the original-context setter but ignores its false return.
The separate Original property is a retained library bootstrap identity, not a
fabricated BEF/CBB interface. NativeHistoryEnabled is false from actual
00D540 argument7=1 ->06E120 parameter history0, not from an arbitrary prepared
flag. The scoped original has no guides, activeArea, prior text, input span
children or auxiliary payload; its private producer proves these restrictions.

`ExportInputGeometry` accepts an exact child registry entry and resolves its
actual ranges through the same CurrentView. It never accepts original or
foreign child C22s. `CopyRawPointStorage` and State snapshots return defensive
copies. C22 equality does not erase separate entry IDs or their original-entry
provenance. The original registry and DOM remain immutable during all copies.

The public API exposes no caller-supplied success/history/context flags. Run
is single-use once entered. Preentry cancellation does not consume the owner.
Entered refusal/cancellation retains a poisoned diagnostic prefix and cannot
export geometry or raw data as a completed input. The probe's internal
RunPrepared failures identify explicit service boundaries; they are not claims
that cancellation or arbitrary native failures always occur at those points.

Native06D2C0 performs its ID query through C51.V68=0A0160. DescriptorC57's
serviceF5120 is bound at Ink001910 global40. Its firstB7690/nextB76E0 iterator
walks left/node/right, with signed key subtraction atB7157. The bounded actual
IDs0..15 therefore have increasing iteration order. The implementation uses
that equivalent ordered sequence, not a new public general-purpose C57 port
or invented allocator/pointer padding. Native09ED40 enforces original Next
strictly above child maximum;17 assigned counter bytes are exposed separately.

Portable failure ownership is explicit: native failed06E120/00C590 creation
releases intermediate owners. This managed stage preserves immutable
structural failure diagnostics and poisons the live operation; GC later
reclaims its private partial raw/DOM graph. Those diagnostic nodes are not
published native live handles, accepted trees, or rollback results. No original
history/observer writes are introduced by the selected copy.

## Verification

The isolated checkpoint uses only raw strokes and caller-supplied actual field
IDs. There are no models, expected labels or recognized strings in this probe.
Managed and NativeAOT both cover32 actual raw/current-view/original owners,
272 chosen-field copies,622 streams,350 explicit failure/cancellation prefixes
and2191 guards. Thirty controls select tag0 while retaining other matching
INPUT IDs. Complete DOM shape/order/raw value bytes, original-node provenance,
all registry ranges and assigned17-byte counters are checked independently.

Both captures are byte-identical:
`5ce6c4cbe8c64c2b18aa5b7934e40079ab0836ce466812a7a4a038fb2b59a657`.
The oracle derives original raw float32 bits from the fixture point generator,
source endpoints from actual point counts, all registry IDs from bootstrap
field count, and cloned attribute/input origins from native insertion order.
It found and corrected a premature tag1 write before failed heap allocation;
Engine0EB740 failure branches around tag/value publication at0EB935/0EB939.

The new static auditor checks6 complete decoded regions/41 pins/5 bindings and
the committed selected-context/bootstrap/registered-request proof dependencies.
It pins attribute-before-child clone order, value tags15/16, the heap-refusal
write gate, C57 signed ordering and global service binding. Vendor execution
is false throughout.

Freshly rebuilt old Managed/Native regressions remain exact:

- Original bootstrap:64 roots/560 streams/1278 guards,
  SHA67849cee000f52a61fc9107169105161cd5e27b3f84586a81f0bac6104134f40.
- Old INPUT context:260 roots/396 selected copies/32910 raw objects/3936 guards,
  SHA56a5f90650802f598579b06148072ee8b7b370283a6cd9457f180a66b095aeb4.
- Old adopted document/DOM:4 trees/5 failure prefixes/62 guards, environmental
  UUID-normalized stableSHA0f4a0c56d13e1a16b55c90fff5287353026d79848e76aea60d8615027fa7f13c.

Run `python tools/LegacyOriginalSelectedField.Probe/run.py PROGRAM CAPTURE`.
Static reproduction is
`python tools/handwriting/audit_original_selected_field.py OUTPUT`.
All new builds/captures are on E; no vendor binary is loaded or executed.

This checkpoint intentionally stops before the additive processing-context
import, pre-NN shared Seed, and publication-scope join. The exact seam inventory
is in legacy-selected-original-context-bootstrap.md. Old one-INPUT APIs and
all their numerical behavior are unchanged. Downstream replay/compatibility
must not coerce this new owner into the old concrete input type.
