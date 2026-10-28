# Fresh selected-child text-context policy

`LegacyFreshFirstTextContextPolicy.ReadForFreshSelectedChild` closes the default
owner flag for the **new selected child249** configuration. It does not infer
settings of a reused/external configuration, the outer text driver or main240
linguistic controller. The first model and inline acoustic alphabet must belong
to the same typed247 source. The ordinary B0/B8 branch remains separately
bounded to plain labels; this policy does not make omitted special branches
or Russian cursive production-ready.

## Complete ownership chain

1. Child initializer156200 allocates a **new2b3** container at child+210 using
   the registered constructor00c860. It does not copy outer driver's container.
   Constructor writes C4/C5=0, bytes98..9f=0 and A4..ab=0, so99/A8 are zero.
2. Binder156630 passes that same child+210 to20e initializer1eb340. Container
   attachment00cab0 stores the controller at container+60 and writes its
   controller+18 backpointer to the same container. With fresh20e count0,
   1eb340 chooses23f and calls its initializer1edde0.
3. Binder156630 writes only fields8,11,14,16,18 and9b in this container.
   On the selected frontend150D00 route, the registered pair binder158DF0
   first installs a nonnull original247 at child1D8. The conditional
   container14 cap is then1; container16/18 are unconditionally1, yielding
   A0/A2/A4=1. A standalone unbound249 would instead use A0=20. The
   [paired-source proof](legacy-english119-context-plan.md) pins both routes.
   Aliasfold source11 becomes0.
   These writes do not change C4,99 or A8.
4. Final container.virtual28=00cb50 reaches the owned source247's
   virtual48=1d60d0. It writes40/2c/1c/28 and updates acoustic frontend flags30
   through getter151ba0/setter151b90. It does not change C4,99 or A8.
5. The outer driver's1e87c0 can write its **outer+f0** configuration C4. This
   is a distinct container from the newly allocated child+210. That outer
   operation does not occur in the child's156200/156630 binding path.

Consequently C4=0 is a preserved value after binding, not a guess based solely
on constructor state. Initializer1edde0 enables E0 when ownerC4 is nonzero,
source.virtual58 equals137 or the exact acoustic alphabet contains U+F0009.
It does not use U+0651 for that flag. Source E6 is separately the lookup of
U+0651, or FFFF when missing.

For the committed first RU source6120, source kind135 and alphabet192 lacks
F0009: E0=false. It also lacks U+0651, so E6=FFFF. At the **binding** stage both
score flags99/A8 are zero; this is not the final invocation value of A8.
Immediately after156630,158970 overwrites childConfig210+A8 from the actual
source-frame+150 flag before calling the first recognizer. Consequently the
API exposes `ConfigurationA8AtBinding` and requires that actual flag as the
argument to `ForInvocation`. No default frame150 value is supplied or inferred
from a fresh constructor. Configuration99 is not changed by this copy. E0 is
unaffected, since its initializer reads C4 instead of A8. The result273
reference field and trait penalty remain separate; fresh no-Arabic-trait
diagnostics have inactive F4 anyway.

## Reproducible static checks

`tools/handwriting/audit_fresh_first_text_policy.py` reads the pinned inspected
PE only; it never loads it as executable code. It verifies DLL SHA256, native
table targets and constructor instructions, then inventories direct nonstack
byte/word C4 writes. The two relevant writes are00c8f3 in constructor00c860
and1e88f9 in outer1e87c0. This inventory supplements the caller/owner dataflow;
it is not a claim that every possible generic indirect memory writer has been
recovered. The full156200/156630/1eb340/00cab0/1d60d0 exports provide the
necessary scoped producer-to-consumer ownership proof.

The API requires a fresh selected child explicitly. It should not be used to
reset custom options or to describe a parent/main controller configuration.
No vendor DLL execution or activation/authentication bypass occurs.

`LegacyFreshFirstTextPolicy.Probe` passes Managed and Windows Native AOT with
the committed first RU resource. Controlled mutations separately exercise
native source-kind137 and exactF0009 routes while retaining the original model
and typed source ownership. It also checks that the original alphabet remains
unchanged, rejects malformed/truncated source ownership and honors cancellation.
The mutations are parser/policy controls, not alternate trained-model accuracy
claims.

```powershell
dotnet run --project tools/LegacyFreshFirstTextPolicy.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res
python tools/handwriting/audit_fresh_first_text_policy.py --output D:/fresh-policy-audit.json
```
