# Fresh source219 orientation preparation

`LegacySourceOrientationPreparation` closes the ordinary source-state metric
producer→child265→2d0 projection bridge. It keeps the owned ingress snapshot and
raw point chronology intact. It does not choose a final angle through child249
or infer the source's orientation-enable property.

## Producer and consumer proof

The pinned Text binary/hash is recorded in
[orientation evidence](legacy-line-orientation.md). In1680d0, after15a1c0's
source processor selection and before the optional2d0 virtual10 call,
15abe0 writes state at source219+120, fieldsa0 and a4. It visits physical
records whose associated-record field+4 is−1. In fresh ordinary ingress all
records satisfy this condition. The native loop uses **exact float equality**
to count horizontal/vertical edges and their direction transitions. The
denominators are total segments and total segments minus physical stroke count.
Zero totals use zero. It does not calculate per-stroke probabilities or average
independent scalar metadata.

This differs from265's172680 negative-metric fallback, which uses nearly-equal
coordinates. Source15abe0 runs before081e90 reads statea0/a4 and passes them to
265. A near-horizontal edge may count as nonorthogonal in source219 and as
orthogonal in the fallback. A blanket call of the fallback would therefore
feed the wrong smoothing weight even though it returns a plausible ratio.
Prior-state130 can preserve the previousa0 within a ten-percent change; that
incremental branch is intentionally excluded from the fresh snapshot API.

`Prepare` performs source exact metrics,2d0 axis amplitude scale with its
resource quantile, optional265 preparation using those metrics, then the
native candidate angles and projection histogram. It does not invent a
positive child cost to select an orientation. Child profile presence must
match the supplied optional smoothing profile.

## Orientation-enable property is separate

Source219 initialization163970 sets byteb4 to zero. Leaf169070's getter returns
whetherb4 is nonzero and processor40 exists. Leaf169090 copies a supplied
property byte intob4. Their resource metadata at2df278 has packed property
header`0x00000001012d000c` followed by those getter/setter addresses.1680d0
executes2d0 only whenb4 is enabled, processor40 exists and suppression84 is zero.
Consequently a serialized processor7/type2d0 does **not** imply orientation is
enabled by default. An upstream property-configuration binding is needed
before declaring a universal automatic orientation mode. The new API exposes
the preparation stage without silently setting that property.

## Checks

```powershell
python tools/LegacySourceOrientation.Probe/reference_controls.py controls.json
dotnet run --project tools/LegacySourceOrientation.Probe -c Release -- controls.json models/ru-RU/ru_RU-ak-cur.lite.res
```

256 independent controls reproduce source167210's adjacent-raw dedup and exact
metrics.128 intentionally differ from the tolerant265 fallback. Integer-source
controls also run the composed producer into the projection stage; very small
axis-amplitude cases remain metric controls because their rotated histogram
would exceed the checked one-million-bin limit.

Actual Russian profiles drive three own printed MIR inputs at0,+90,−90.
Source-state exact metrics, rather than hardcoded(0,0), reach265 and produce54
prepared points. Their first projection angles are respectively0,−90,+90;
the next candidates are retained as diagnostics. Exact equality changes the
orthogonal fraction from`.61538464` to`.53846157` under the double-trig/float
90-degree rotation; the port does not conceal that numerical behavior with a
rotation-invariance claim. Smoothing weight is`.1125` in these cases.

These checks establish the producer/consumer arithmetic and raw ownership,
not final winner confidence or representative Russian cursive accuracy.
Associated/custom records, prior metric carry, orientation property binding,
child249 final scoring and ordinary source frame materialization remain their
separate native branches.
