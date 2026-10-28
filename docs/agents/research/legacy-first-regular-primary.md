# First native regular primary proposals

`LegacyFreshFirstRegularPrimary` binds actual first source/frame/cuts, every
supplied NN/cache group and a graph with the resource's own span cap. It
restores the regular-label proposal order of1EF070: node order, ascending
span, ascending arc ordinal, then the native top20 shortlist. Start category
map entries precede end entries, and identical category proposals remain
duplicated in order. It does not filter by Latin/Cyrillic/digits.

Graph pruning is an upstream caller responsibility. This batch's request-order
fixtures deliberately supply an unpruned graph; their42 MIR groups do not
establish that the complete native source preparation retains42 groups.

Frontend98=151FE0 delegates to21b28=1A7580, which returns maximum arc span+1.
FrontendB0=152030 delegates to21b70=1A7850. Assembly1EF5CB passes the current
arc ordinal inR9, so this tests that arc's span, not merely arc0. Ordinary
source280 delegates to21b1C8=05A400, which returnsfalse. Actual first graph
resource12727 has cap7; first247 BA separately supplies F8=5.

Native header exclusions are shadda E6, newline EA, space EC, and NBSP EE
only when the same top20 contains EC. The four protocol labels F0002/3/4/8
are excluded only when native local194 istrue. This API keeps194 explicit:
the constructor20e count0 is proven, but the complete fresh allocation and
owner60 identity chain is a separate binding claim. Tests exercise both
values and demonstrate a real request difference on an original closed
contour. Caller-supplied score masks remain owned input, not a guessed default.

`RunRegularLabels` restores the native1EE3B0 special guards absent from the
old caller-prepared generic recurrence: no root E4 insertion, nonroot ordinary
kind0 current E4 uses first sampler kind6, and exactly one current/prior E8
(replacement symbol) disables ordinary kind0. Kind2 stays separate. Other
same-source geometry remains the restored first sampler. The regular runner
refuses NBSP ancestor traversal; it does not silently remove retained NBSP.

The blank E2/kind5 dispatcher is unfinished. Eligible blank node/arc/span
calls are returned as `PendingBlankProposals` rather than fabricated. A run
of regular requests therefore is not the complete native primary or273
winner. The original generic recurrence and old probes are unchanged.

Actual headless controls cover an original printed MIR input, rectangle plus
interior stroke, and standalone closed contour. They produce51 NN groups and
1059 regular requests across two explicit194 controls. MIR cap7 uses42 groups
(the old capped fixture had35); its best retained cost remains41268 and the
first-stage output stays mixed script. For rectangle plus interior stroke,
the special E4 primary cost becomes12111 through native kind6 (the older
prepared ordinary0/2 fixture had12872). Standalone contour does not get a
root E4 text record. These are branch controls, not a handwriting corpus or
Russian accuracy measurement.

The independent Python oracle rebuilds every request from actual ordered
cache entries and resource category maps, then computes140 primary records
using captured geometry callbacks and its own stable tuple recurrence.
Checks include root E4, current/prior E8, category duplicate/tie order and
reported pending blanks. Managed and Native AOT use the same headless API;
no vendor DLL is loaded or executed.

```powershell
dotnet run --project tools/LegacyFirstPrimaryProducer.Probe -- `
  models/ru-RU/ru_RU-ak-cur.lite.res `
  models/ru-RU/legacy-engine-profile.json `
  models/ru-RU/legacy-separation-profile.json <captures.json>
python tools/LegacyFirstPrimaryProducer.Probe/audit_output.py <captures.json>
python tools/handwriting/audit_first_regular_primary.py --output <static.json>
```
