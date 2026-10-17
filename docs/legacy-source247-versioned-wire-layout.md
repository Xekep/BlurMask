# Version-aware247 wire ownership

`LegacySource247WireLayout` reads the complete **parent wire layout** of one
unencoded247/10D or247/110 resource. It owns a private byte snapshot, its hash,
the actual inline AL, ordered scalar fields and explicitly delegated child
envelopes. It does not deserialize or execute those child processors,21D
acoustic weights, optional NN adapters or a controller. No existing RU reader
or recognizer is widened.

`LegacySampler297WireProfile` separately reads a complete known297/106 or108
profile from that layout's actual source130 child. Its supported branch owns
the actual category201 tables and all remaining known scalar bytes, with no
positive134/1D0 payloads. There is no caller-supplied sampler offset or source
alphabet. It refuses another layout's child even when the bytes/hashes match.

## Native field order

The static source is pinned Text PE SHA256
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
The new auditor checks complete ranges of133 reader1D2680,134 reader1D3210,
coordinate reader1D3F00, source-parameter reader1D41E0, sampler delegation
1D47C0,247 reader1D4950 and297 reader196500. It also pins31 relevant
instructions and seven groups of field/order statements.

| Reader | Retained data and ownership |
| --- | --- |
|133/100,1D2680|DWORD10,56 metadata bytes, signed identity marker with the extra4C DWORD only for−1, then DWORD50. They remain data; native platform/activation services are not called or replaced.|
|134/201 or300,1D3210|Actual alphabet-presence flag, inline2B kind/version/payload;300 additionally reads a byte count and that many delegated resources into68/count70. Each resource remains a child envelope with its own kind/version/bounds.|
|247,1D4950|Source kind78 and coordinate mode80; mode2 uses1D3F00, preserving its count, count×4 bytes, two32-bit fields and seven following bytes.|
|247/10D versus110|The legacy skip byte is consumed only below10F. B8/B9, BA/BC and the parameterC0 stream follow at their actual positions. No version bytes are rewritten.|
|ParametersC0,1D41E0|Four32-bit fields10/14/18/1C; flags20/21, separate third-resource flag and22; field24; optional28,30 and38 resources in native order.30 exists for21 OR22. Flags40/41, field44, flag48, field4C, flag50, DWORD60 count and58 children follow.|
|247 tail,1D4950|Versions>=10E read optional150 presence/child, then the sampler130/graph138/acoustic140 trio flag and children.110 also reads optional148 presence/child. DWORD160/count and158 children precede the fixed186-byte metadata arrays.|
|297/106 or108,196500|The common prefix preserves native scalar08..30 fields, bytes18/19/1A,12C and130, then category presence and category201 payload. Its absolute stream offset controls alignment.|
|297 category branch|The following214 bytes are48/4C, byte50,32-bit54/58/5C/60/68/6C/70/74, six six-element matrix rows78..104,108, byte10C and110/114/118/11C/120/124. Raw32-bit values are retained without inventing semantics or normalizing their bits.|
|297 additions|For>=107 read134. A true value requires128 plus the214 payload and its following scalar fields, which are not yet ported. For>=108 read the additional presence byte; true requires delegated resource1D0 and scalar1A0. Both positive branches are explicitly refused by the profile reader.|

The fixed247 metadata tail is retained as16 DWORDs16C,16 WORDs1AC,16 WORDs1CC,
byte1EC, four DWORDs1F0, four WORDs200, four WORDs208, byte210, six WORDs214 and
six WORDs220. This accounts for all186 bytes. It is not interpreted as permission
to run a vendor module.

Every field has its actual wire offset, element width/count, native destination
name and copied raw bytes. Every delegated resource has its destination,
kind/version and exact half-open interval. Independent tests partition every
parent byte into exactly one retained field or delegated envelope: no gap,
overlap or scan-to-magic replacement is used to locate the next field.

`CopyChildEnvelopeBytes` deliberately names an envelope. A successful source
layout must not be used as evidence that a249/24B/21D/21E child payload was
understood. The source130/138/140 slots are checked as297/21B/21D; other
generic native resource slots retain their own header kind instead of being
cast to an unrelated processor. All child headers must be unencoded and stay
within this source's end. Existing source/controller owners continue to use
their previous concrete reader contracts.

## Actual resources and refusal boundaries

| Repository resource | Source/base versions | Actual sampler | Known sampler result | Other dependency |
| --- | --- | --- | --- | --- |
|en_US-ak-cur.lite.res|110/300|297/108 at182412|Complete:134=0,1D0=0|Main21D/119 weights not parsed here; source150/148 absent.|
|en_US-ak-cur.res|110/300|297/108 at374182|Refuses actual positive1D0/21E payload|Source148 also owns an actual21E/103 resource.|
|en_US-ak-superimposed.lite.res|110/300|297/108 at207622|Refuses actual positive134/214 payload|Source kind134 is collection metadata, not silently treated as ordinary135.|
|en_US-ak-superimposed.res|110/300|297/108 at207622|Refuses actual positive134/214 payload|Source148 also owns an actual21E/103 resource.|
|ru_RU-ak-cur.lite.res|10D/201|297/106 at188157|Complete214-byte tail; no added flags|Old source prefix/category offsets agree; old APIs remain unchanged.|

All five actual source layouts succeed. All four English ALs are217; the RU
AL is192. The additional base134/300 resource count is zero in these four
English files; the reader retains the count and delegates nonempty envelope
lists in order but does not claim an observed positive actual payload. Actual
source150 flags are zero. Positive source148 resources are retained as
delegated envelopes, not discarded or replaced with NULL.

The three actual sampler refusals are positive-payload boundaries, not failed
resource identification. The original bytes and source layout remain intact.
No partial sampler object is returned on a refused or malformed payload.
Category-absent samplers, other source/sampler/base versions, encoded nodes,
unknown source coordinate modes and noncanonical Boolean bytes are explicitly
outside this bounded reader. The native generic Boolean readers can accept
nonzero bytes; the portable0/1 check is an explicit supported-input boundary.

## Ownership, bounds and checks

The selected resource envelope is checked before copying. The64MiB maximum,
256 entries per DWORD-counted resource list and the existing bounded AL/category
limits are portable limits. Byte-counted base lists naturally have at most255
entries. They are not asserted to be native allocation limits. Counts and byte
products are validated before traversing child envelopes. Malformed lengths,
bad flags and trailing parent/profile bytes fail without producing a layout or
profile. Cancellation is checked before reading/copying and at field/child
boundaries.

Public field/envelope bytes are copies. The category API reparses the private
same-source snapshot: recovering an array from a previous public category
object cannot change future reads. Source and child reference identities are
checked; byte-identical foreign owners are refused. Reading a selected source
at a nonzero offset preserves original absolute offsets, category alignment and
child IDs, rather than relabelling them as raw document positions.

Managed and Windows Native AOT probes agree on all140529 capture bytes,
SHA256 `3af2186befacf576841b321d54a0853dabbaafbf46c7e5ee3c80c5dc6fe9b96a`.
The independent Python reader imports no research or C# implementation and
checks38284 retained scalar/AL bytes, exact delegated intervals and complete
known sampler fields/categories. It independently finds the three positive
payload refusals.669 guards cover cancellation, byte-identical foreign child
owners, copied-buffer isolation, counts, Boolean flags, child headers/lengths,
root truncation and eighteen sampler-tail truncations that preserve a valid
parent and its following children. The RU old source/category reader remains
part of the probe: its scalar offsets and every category map/prototype match;
the old APIs continue refusing English110 resources. No old expected capture
is replaced with a new result.

```text
LegacySource247Wire.Probe models capture.json
python tools/LegacySource247Wire.Probe/audit_output.py models capture.json
python tools/handwriting/audit_source247_wire_layout.py static-report.json
```

This closes a resource-layout dependency, not English or multilingual
recognition. Full positive sampler payload readers,21D/119 acoustic/cache
families, actual source/frame/controller binding, configured F4 invocation and
their recognition/publication paths remain separate. The supported lite
sampler does not justify enabling a main recognizer, substituting RU models or
claiming a change in handwriting accuracy.
