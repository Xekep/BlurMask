# Live contextual-quote configuration owner

The native enabled21F320 body reads a live **2B3 configuration byte F4**. It is
not a scalar loaded from an acoustic resource. The property path and the same
configuration object's attachment to a controller are now statically pinned.
This does not enable an existing fresh recognizer or turn a prepared WORD list
into an actual enabled controller result.

The separate [prepared driver](legacy-prepared-published-glyph-rewrite.md)
already tests the ordered enabled-body mutations on actual AL/category slices,
linked WORD reading vectors and their pool. The existing fresh global no-op and
`GlobalCompletion` remain unchanged.

## Native property and command

All addresses below are VAs in the pinned PE, with the `180` prefix omitted.
Text SHA256 is
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`;
Engine SHA256 is
`d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c`.

| Owner or operation | Exact evidence | Meaning |
| --- | --- | --- |
| NEW2B3 default | Text00C8A8 clears EAX;00C923 stores WORD at F4 | F4 and the adjacent F5 start at zero. |
| Registered property | Text2751E0 row, index50 of52 at274BA0 | ID `0x012D0017`, payload size1, getter00EAF0, setter00EB00. |
| Property lookup | 2B3 registration274B88 points to00EB50 | The lookup searches52 rows and returns the matching descriptor with the same owner pointer. |
| Read | 00EAF0 | Copies the live byte F4 to the output. |
| Write | 00EB00..00EB1B | NULL payload clears F4. A non-NULL payload copies its first byte verbatim; it does not normalize arbitrary byte values to0/1. Both paths return success. |
| Script command table | Engine252C30/38 | The literal `EnableCurlyQuotesMapping` at228B58 is paired with0F6D00. |
| Command registration | Engine0FA275 and0FA284 | The registration loop installs the39 name/callback pairs, including this pair. |
| Command callback | Engine0F6D00..0F6E28 | With no argument the local byte starts at1. One argument uses the actual optional-Boolean parser. More than one argument fails. A supported non-NULL target of type12F orF03 receives property012D0017 with a one-byte payload; a failed property call returns failure before recording the command value. |
| Optional command recording | Engine0F6DDC/0F6DE9 | When the command record exists, writes presence byte46=1 and value byte47. This is distinct from writing containerF4. |

The script callback also permits no target or another target type without
calling that property setter, then may record the command value. The deferred
record's later replay is **not** proved by these two stores. In particular,
presence46/value47 must not be treated as an already-mutated2B3 or as an
invocation event accepted by the current fresh API. The F03 implementation is
not closed here; the exact owner path below is the native Text12F path.

## Same-owner12F path

1. Native Text12F constructor1E7690 creates its208 and20E objects, then requests
   a NEW2B3 at1E7903 and stores the returned object at12F+F0 at1E7928. A failed
   allocation does not produce a successful configured root.
2. The12F property resolver1E9CF0 first queries12F+E0, its20E. The actual NEW20E
   resolver1EB690 searches exactly one row,2E7488: property012D000B. It does
   not intercept012D0017.1E9D4A then loads the same12F+F0 and forwards the
   original property key/output arguments to the property service again.
   On success,012D0017 is outside the five-key override interval handled later
   in1E9CF0 and the resolved2B3 row/owner is returned unchanged.
3. The12F preparation path1E87C0 selects20E's interface and, when it needs to
   create/bind the controller, passes **12F+F0** in R9 at1E91E7,12F+E8's
   controller slot in R8, and12F+E0 in RDX.1E920C calls20E.V08. Its actual
   interface binding2E7420 is1EB340.
4.1EB340 invokes the incoming configuration's V18 at1EB464..475.2B3.V18 is
   00CAB0: it stores container60=controller and controller18=the identical
   container. No F4 profile copy or model-derived Boolean is substituted.
5.21F337 reads that controller18 pointer and21F344 tests its live F4. At the
   caller221840,21FBF0 precedes the knowledge99 gate and21F320 call. An F4
   property mutation does not remove that caller gate or the later
   replacement-availability gate.

These are conditional native owner paths, not a claim that every call to
1E87C0 takes its controller-allocation branch. The existing native controller
may be reused; such reuse, arbitrary properties, associated/custom sources,
external wrappers and F03 are outside the current private fresh controller
factory. They must not inherit the fresh no-op proof.

## Registered-property copy is ordered and can retain a prefix

2B3.V70 is00EBA0. Its RDX object is the destination and R8 object is the source.
It iterates52 property descriptors in table order. Where a row's byte19 is
nonzero, it first tests the source getter with a NULL output. It then reads a
successful source getter into local scratch and calls the destination setter.
Getter failure skips that property. If R9b is nonzero, a failed setter returns
false immediately; prior destination writes remain. There is no rollback.
With R9b zero, setter failure is ignored and the end clears the engine error
before returning true.

F4 is row50. Its getter and setter are unconditional-success byte operations,
so a valid source can copy a nonzero F4 before the final F5 row is processed.
This is another actual mutation route. It does not prove that an arbitrary
configuration copied successfully as a whole or that the copied controller is
the fresh no-property owner.

## Resource version boundary

The five complete repository resources are hash-checked against
`models/manifest.json`. Their raw headers and explicitly identified sampler
envelopes differ:

| Resource family | Root247 version | Inline base134 version | Sampler297 version |
| --- | --- | --- | --- |
| Four English cursive/superimposed resources |110|300|108|
| Russian cursive lite |10D|201|106|

This is not a version-only compatibility problem. The static auditor pins the
complete native readers1D3210,1D4950 and196500. Before a full English ingress
factory can claim the same owner route, it must restore and independently
check at least these wire differences and all affected parent/child ends:

- Base134 reader1D3210 takes an additional version300 branch after its earlier
  payload: a byte count grows the resource-pointer vector68/count70/capacity74
  and the corresponding nested resources are read in order. It cannot be
  silently skipped by treating300 as201.
- Source247 reader1D4950 omits the older one-byte skip for versions>=10F;
  versions>=10E read the optional150 resource presence/object; versions>=110
  read a further presence/object into148. Later source arrays and boundaries
  remain owned by that same reader. An offset65 category slice does not prove
  those whole-resource branches.
- Sampler297 reader196500 has the>=107 optional134/128/138 payload and the
  >=108 additional optional object/scalar tail. The actual initial AL/category
  prefix is supported separately by the prepared driver, but the full sampler
  and downstream graph/acoustic ownership cannot be obtained by rewriting108
  to106.

`LegacyFirstAcousticModel` still validates247/10D and297/106 at their actual
typed positions; `LegacySourceSamplerCategoryOwner.ReadV10dSource` still uses
the proven source traversal and requires297/106. Neither is widened here.
No AL IDs, category owner, source-kind callback, WORD pool or TOP+18 live2A is
borrowed from another resource to manufacture an enabled result.

The pinned Russian AL has none of the five curly replacement strings. An
actual nonzero F4 on that resource still reaches21F320's second early return.
Positive enabled-body tests on the English AL in the prepared driver therefore
remain prepared tests. They are not evidence of a complete configured English
recognizer.

## Next bounded runtime seam

The smallest justified next configuration seam is an explicit **new configured
owner**, separate from the existing fresh no-property owner. It would own one
NEW2B3 byte-property state, apply validated012D0017 events in order with the
native NULL-versus-byte semantics, bind that exact state to its controller,
and snapshot the invocation's caller99 policy. It must not offer a Boolean
override on an already-produced fresh result. A copied/reused owner would need
its own ordered-copy lifecycle and failure-prefix tests.

Joining that owner to a positive real English main pass still depends on the
full v110/base300/v108 ingress and actual source/WORD/TOP2A lineage described
above. Neither step is implemented by this read-only proof. A further numeric
helper or a changed fresh flag would not close that boundary.

## Reproduce

```text
python tools/handwriting/audit_contextual_quote_configuration_owner.py E:/BlurMask-build-artifacts-20261014/contextual-quote-configuration-owner-static.json
```

The auditor checks11 PE ranges,49 exact instructions,11 dispatch bindings,
the two registration tables and five original resource hashes/versions. It
reads the vendor PE as bytes and never loads or executes it. It does not run
the native script parser or prove deferred command replay. No existing runtime,
fresh flag, golden capture, UI, model, CI or accepted document is changed.
