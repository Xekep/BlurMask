# Positive24C: weighted slant and source ownership

This is a static plan for the actual first English247/110 source's24C/100
processor. No runtime enable or classifier is introduced here. The preceding
[owned preprocessing](legacy-english-lite-first-preprocessing.md) stops at
signed direction count>=20; this plan closes the next branch's arithmetic and
mutation contract. Native DLL bytes are read, never loaded or executed.

## Angle selection

After the counter gate,176B15 loads descriptor count from source+20, and176B95
loads source+18's88-byte descriptors. Both angle collection and point mutation
visit only descriptor low kinds0 or6. Descriptor+24/+28 are inclusive physical
start/end ordinals, not point ordinals or graph-group IDs. Within each selected
descriptor, only physical1C+4==-1 strokes contribute to the angle collection.
The NEW/no-event direct packet owns one kind0 descriptor spanning all physical
strokes; `LegacyFreshOrdinaryRawPacket.PrepareDirect` already proves this source
lifecycle. Physical directions are not inferred from a guide's line index.

For each adjacent point pair,176C50..176C63 forms **float** `dy` and `dx`.
If dy<0,176C6D/176C71 flips the sign of both.176C7B/176C7F widens dx and dy
to doubles and176C83 calls the CPU-dispatched atan2 entry24DD70. The angle is
therefore measured from the positive Y axis: `atan2(dx,dy)`, after the flip.
The inclusive minimum/maximum angle tests are176C88/176C9C. The bounds are
the actual serialized24C radians, not a degree setter or a hardcoded constant.
Horizontal edges normally fall outside the English approximately+/-pi/6 range.

Accepted edge weight is `sqrt(double(float(dy*dy+dx*dx)))`. Both products and
their addition are float SSE operations before conversion to double
(176CA2..176CBE). At176CCF XMM10 adds weight,176CD4 multiplies weight by the
saved double angle XMM8, and176CD9 adds that product to XMM11. Ghidra's scalar
decompile omits this XMM11 dependence; using its displayed length sum as the
angle would be wrong.176DAC divides XMM11 by XMM10. If no accepted segment has
positive length,176D3C explicitly selects angle0. An empty descriptor vector
returns without point writes; it is not synthesized by the fresh row adapter.

## Descriptor-local transform

The same weighted angle is applied to each eligible descriptor, but its bounding
box center is calculated from that descriptor's current contiguous physical
point range.176DF9/176E01/176E05/176E15 obtain extrema. Center sums and halves
are float. The angle is passed as double to24E3F0 and24FE50, the native cos/sin
CPU-dispatched implementations; the results are converted to float before use.

For `cx=(minX+maxX)/2`, `cy=(minY+maxY)/2`, `c=float(cos(angle))` and
`s=float(sin(angle))`, preserve the displayed operation grouping:

```
tx = (cy*s - cx*c) + cx
ty = cy - cy*c
x' = (y*(-s) + x*c) + tx
y' = (y*c + x*0) + ty
```

This is **not an ordinary rotation**. XMM12 is zeroed at176B3A and is used for
the literal x*0 term at176ECB; substituting x*sin(angle) changes the native
algorithm. Nonzero centers and dy-flipped inputs must distinguish the two.
The only non-stack memory stores in176B15..176F15 are176EE0/176EE4, writing
the current point's X/Y. Neither line guides nor physical assignment records
are updated.265 therefore uses the same pre24C guide height and the original
source metrics, while consuming the transformed points and unchanged key order.

## Next bounded port

Add a separate prepared arithmetic kernel for the proven kind0 single descriptor
covering all supplied physical strokes, then a NEW same-source119/247 owned
full-preprocessing adapter. It must derive raw packet membership and actual
24A/24C/265 profiles internally. Keep the existing short-branch API strict and
its capture unchanged. Do not borrow Russian profiles or erase point chronology.

Checks must include counter19/20 on both sides, strict equal/flat and dy-flipped
edges, horizontal edges, angle-limit inclusivity, zero accepted weight, multiple
physical strokes, nonzero bbox centers, guide immutability, cancellation and
ownership/refusal boundaries. Separate prepared angle-limit tests from actual
resource controls. Use independent scalar/dense geometry oracles plus full
Managed/Native capture equality. `System.Math.Atan2/Sin/Cos` would be a portable
transcendental adaptation, not a universal bit-exact claim against every native
CPU dispatch path. The old source/Russian/short-branch regression data must stay
unchanged. This port still does not produce features, activations or recognition.
