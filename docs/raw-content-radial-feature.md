# Raw-content radial feature

The unusual angle normalization in `RawContentFeatures` is intentional relative
to the inspected native implementation. In the original `MyScriptInk` B610
extractor, instruction BBC5 multiplies the approximate centroid-ray angle by
the binary32 constant at FA19C (`0x3ea2f983`, approximately `1/pi`). The same
normalized argument is passed to the cosine and sine routines at BBD0/BBDD.
The resultant length is computed from their averaged outputs and subtracted
from one before the node feature's signed square root.

Removing the normalization would change model inputs. For a two-point line
with opposite centroid rays, the normalized-angle semantic value after the
square root is `sqrt(1-cos(1/2))`, about `0.349882`. Ordinary circular variance
of opposite rays would instead yield one. No production formula was changed.

The raw-content probe now checks channel 3 on three line orientations, four
cardinal rays, eight octants and a single point. These symmetric controls have
exact zero centroids. The four-ray value is `sqrt(1-cos(1/2)*cos(1/4))`;
the eight-ray value is `sqrt(1-cos(1/2)*cos(1/4)*cos(1/8))`.
Expected values follow the pinned normalized-angle semantics and these
trigonometric identities. The `1e-6` tolerance serves semantic regression;
it is not a proved error bound for native CRT or centroid paths.
These controls are not captured vendor outputs or
proof of complete floating-point/centroid/graph parity.

Static PE bytes, Capstone instruction ranges and constants are retained under
`E:/BlurMask-build-artifacts-20261014/root-raw-radial-native-inspection-20261024/`.
The first B610 unwind record covers only its initial 226-byte fragment; the
separate B610-through-BF50 raw region contains the relevant angle/trig loop.
The scope addendum records this distinction. No SDK or vendor function was run.
