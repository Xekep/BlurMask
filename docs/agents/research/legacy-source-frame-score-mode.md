# Source frame150 and first invocation scoring mode

The first binding policy has zero A8 **at binding**. It does not by itself
establish the invocation value. Native158970 copies the parent source
secondary+150 flag to selected childConfig210+A8 immediately before the first
recognizer call. This is separate from E0, which reads ownerC4.

## Exact source-property producer

* Source secondary constructor16a130 initializes byte150=0 at16a222.
* Source218 property metadata at2df018 registers property12d007e, bool type101,
  getter169140 and setter169160. They access secondary at source object+120.
  Getter returns byte150; setter copies the supplied property byte there.
* Container2b3 setter00e560 writes container+A8. When its value changes, it
  notifies frontend.virtual218 with12d007e. Frontend150c00 calls150800, which
  obtains that exact property from the container and forwards it to registered
  source targets through the property interface. Source169160 is its consumer.
* Initial frontend150d00's19-property forwarding list does **not** contain
  12d007e. Thus initialization is not evidence that an externally configured
  or cloned source has inherited a particular A8 mode. An explicit later
  property event can change150 independently of ink geometry.

The static evidence binary remains MyScriptText SHA256
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
No vendor DLL is executed. The field name here describes its role in the
score-mode copy; no undocumented user-facing property name is invented.

## Scoped direct-source invocation

`LegacyFreshDirectFirstInvocationPolicy.Create` accepts only the sealed
`LegacyFreshOrdinaryPacketFrame` produced by its new direct-source factory.
That factory retains original ingress ownership, creates one ordinary
descriptor/line0, has no existing-source clone or property-event API, and runs
direct167210/24a/265 preparation. These stages do not change150. Its owned
fresh150 therefore remains zero through this exact supported path.

The adapter validates the supplied typed first source against the frame's row
profile, source-reference ownership and descriptor scope before publishing
the fresh invocation settings. It is not used to guess150 for a reused parent
source, a source with property events, a native document or the main240
controller. Those callers still must pass their actual flag to
`LegacyFreshFirstTextContextPolicy.ForInvocation`.

This resolves invocation scoring mode for the existing direct-source
diagnostic pipeline. It does not complete language-model refinement, all
special branches, native terminal geometry or Russian cursive support.

`LegacyFreshDirectInvocation.Probe` passes Managed and Windows Native AOT on
three actual raw MIR packet/frame fixtures at different scales/translations.
It checks matching ingress/frame ownership, rejects a controlled foreign
typed source, preserves the separate explicit property override path and
checks cancellation. The read-only static audit pins the producer/getter/setter
instructions and property metadata, then checks the initial19-property list.

```powershell
dotnet run --project tools/LegacyFreshDirectInvocation.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res
python tools/handwriting/audit_source_frame_score_mode.py --output D:/source-score-mode-audit.json
```
