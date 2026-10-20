# Actual global024D40 payload metadata

This extends the [024F30 proposal](legacy-global-text-source-wrapper-plan.md)
with static producer closure. No runtime, CHAR source registration, recognition
quality improvement or document acceptance is implemented by this checkpoint.

## SourceLineId comes from the actual graph and source descriptor

0283B0 binds its temporary wrapper from one actual frontend: V250=1530F0 returns
the source object at frontend+60; its cast218 interface is retained separately.
V248=0CA630 returns the same frontend's graph21B at+70. The wrapper stores them
at28(source),30(cast218) and38(graph). These are not an old count1 sampler.

024D40 initializes payload60 to-1. After successful preceding work it checks the
**original signed source arc**, independently of the source-processing kernel's
ordinary/insertion result. For a nonnegative arc and nonnull source object:

1. If graph.V20 reports a positive count, graph.V68=1A77D0 resolves the original
   WORD target node/arc to the compact group ID. A higher source resolves through
   its stored originalLast reference before reading the actual base arc.
2. graph.V78=1A7920 reads that group's source **descriptor ordinal**. Its stride
   is56 when the actual graph is compact and96 otherwise. It does not return a
   guide coordinate or SourceLineId directly.
3. Actual source cast218.V10=168AD0 returns `source120.descriptors + ordinal*88`.
   DWORD at descriptor+10 is the assigned source LineId.
4. 024EEB..024EF1 adds wrapper68 and stores payload60. 0288E0/028902 clears that
   offset at **each alternative**, so the proposed fresh single-alternative
   route uses the genuinely initialized offset. It must not invent an offset
   from a count of previously allocated result-line records.

If graph.V20 is nonpositive, the native branch uses descriptor ordinal0; it does
not leave LineId=-1 merely because the graph count is zero. A negative original
arc or a null source object does keep-1. The first global implementation should
refuse unsupported/null/empty graph owners explicitly, rather than fabricating
those source objects or claiming their fallback is exercised by ordinary input.

The independent library already retains the corresponding actual producers:
the same global V140 metadata resolves the base key and exposes its real group
ID; `Graph.State.Groups[group].DescriptorOrdinal` retains the compact conversion
field; the same cut owner's descriptor bytes contain I32+10, also retained as
`LegacyMainDescriptorCutStep.LineId`. A new payload must join these exact owners
and verify descriptor identity, using the current global graph and original
source descriptor. It must not assign literal0, read guide40+10 as an integer,
or substitute `LegacyFreshMainSingleLineSampler`.

This preserves the important special case: an ordinary positive reference can
be downgraded to insertion by source processing, yet still receive its actual
source LineId because native line selection tests the original arc sign.

## Remaining scalar metadata and native order

The processed source key is already the interner node's actual C21 key at
node38/count40, corresponding to payload20/count28. It is not reconstructed from
selected UTF8 or a bounding box. Fresh glyph records are already at payload50,
count58/capacity5C following0213A0;024D40 does not replace those records.

| Payload field | Native producer/meaning in this scope |
| --- | --- |
| 10 | Ordinary source:0210C0 CHAR registration output, written on success. Nonordinary source:literal-1. |
| 08 | I32-1, written after the registration attempt. |
| 40 | I64-1. |
| 00 | Literal zero scalar slot; no native pointer is manufactured. |
| 1C | U32 zero. |
| 64 | Byte zero. |
| 48 | I32-1. |
| 30 | Fresh empty prior-range C21, initialized after the scalar defaults. |
| 60 | I32-1 first; actual source descriptor LineId plus wrapper68 only on the supported successful branch. |
| 18 | Computed source flags from01FAE0, only when preceding work succeeds. |

An ordinary source attempts CHAR registration **before** the scalar defaults.
It can consume the selected registry cursor even when a later allocation fails.
A native false registration result still proceeds through scalar defaults,
prior-C21 initialization and the literal line60=-1 write, then returns failure
without the01FAE0 computed-flags write. The failed output ID and computed flags
must not be exposed as recovered zero bytes: they may be unwritten. New typed
prefix diagnostics must distinguish written fields from absent/unwritten state.
Managed cancellation is a separate checkpoint policy, not a claimed native
callback-false branch; it must remain separately identified in tests.

## Prior state: empty traversal is different from ordinary visitation

024D40 starts its local prior byte at0 and calls021D10 only for an ordinary
source after successful registration.021D10 sets the OUT byte to1 before
traversing actual V140 reference bounds against the same original C4A input map.
Every visited fresh ordinary row with no cached payload and type different from
C25 clears that byte to0. An empty traversal leaves1. No source label, positive
boolean or eventual processed-key length determines this byte.

In the supported group0 domain the traversal uses each actual reference's
first/last input ordinals inclusively. At least one ordered pair visits ordinary
rows and yields0; all valid-boundary pairs with first ordinal greater than last
perform no visits and retain1. This empty case must not be replaced by blanket
fresh=false. It is a separately prepared boundary control unless a genuine
global source actually produces it. Nonordinary sources bypass021D10 and retain
their initial0. Malformed or foreign bounds remain explicit refusals.

For fresh raw input the C4A rows have no cached payload and are not C25, so the
prior-range vector remains empty both with visits and with empty traversal.
The implementation must verify the actual row/source ownership rather than
claiming all native021D10 variants have empty output. Prior C25/cached payload
copying remains excluded.

01FAE0 first ORs every published text-record flag. Its final payload flag is:

```text
(((priorByte & 1) << 24) ^
 ((ordinary ? 0 : 0x200000) |
  (OR(all published flags) & 0xCE1FFFFF) |
  (ordinary && one same-coordinate range ? 0x800000 : 0))) & 0xFDFFFFFF
```

The same-coordinate test compares original source ID and point index, with equal
fractions or either fractionFF; it does not demand equal mode bytes.021760 later
updates the retained payload using its existing actual numeric Process loop,
including fixed old-prefix matching, interning-before-lookup and partial rows.
That loop must remain byte-identical through any emitter-binding refactor.

## Typed source-key reuse is mandatory

The planned wrapper begins from the exact scope's fresh CHAR interner and the
same Seed, selections/InputBinding and shared3E. It verifies fresh empty cache
state before its first publication claim. A node found later by equal C21 bytes
is reusable **only** if it retains the same actual global wrapper receipt and
the matching successfully initialized typed global payload in that wrapper's
dictionary. Merely matching Document, range bytes or node identity is
insufficient. A preexisting prepared/count1/configured/foreign-global node must
cause explicit refusal before reuse emission; it must never acquire an invented
global provenance sidecar retrospectively.

New nodes receive a global receipt produced only by actual same-owner source
processing. Duplicate real source keys inside this wrapper may reuse that
payload. Public glyph-choice publication remains unchanged, and future source
wrapping still interleaves select -> source-process -> interner -> fresh/reuse
for each glyph after the complete first pass. The five proposed class seams
remain those listed in the earlier proposal; no additional existing class is
opened by this metadata closure.

Runtime verification must independently check original group/descriptor/LineId
and actual offset, native scalar write stages, ordinary visits versus separately
prepared empty traversal, exact aggregate flags, actual duplicate-source reuse,
foreign/prepared-node refusal, pending CHAR command IDs and failure prefixes.
WORD/TEXT registration, result-line publication, transforms and acceptance remain
later work. Russian quality remains a separate post-inference diagnostic.

```powershell
python tools/handwriting/audit_global_text_payload_metadata_plan.py REPORT.json
```

The script verifies11 metadata PE regions,6 virtual bindings and37 exact
instructions, composing four existing audits. SDK files are read as bytes;
they are never loaded or executed.
