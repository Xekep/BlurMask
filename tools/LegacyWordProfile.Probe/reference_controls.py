"""Independent vendor-free float32 scalar cost references; optional output path."""
import json
import random
import struct
import sys
from pathlib import Path


def f32(value):
    return struct.unpack("<f", struct.pack("<f", value))[0]


def case(cost, slope, offset, scale):
    slope, offset, scale = map(f32, (slope, offset, scale))
    # Native scalar branch rounds each operation to binary32 before truncating.
    scaled_slope = f32(scale * slope)
    scaled_offset = f32(scale * offset)
    level = f32(f32(cost) * f32(1 / 8192))
    weighted = f32(level * scaled_slope)
    centered = f32(weighted - scaled_offset)
    result = f32(centered * f32(8192))
    assert -2147483648 <= result < 2147483648
    return dict(cost=cost, slope=slope, offset=offset, runtimeScale=scale, expected=int(result))


cases = [case(cost, slope, offset, scale) for cost, slope, offset, scale in [
    (8192, .5, 1, 2), (8192, 2, .25, .5), (8192, -.5, -.25, .5),
    (-1, .5, 1, 1), (1, .5, 1, 1), (0, 0, 0, 1),
    (-2147483648, 1, 0, 1), (2147483647, .5, 0, 1),
    (16777217, .1, .22, 1.3), (-16777217, -.3, -1.375, .7),
    (9851, .4, 4, 1), (30643, .3, 1.3, 1),
]]
rng = random.Random(0x2D1FE2A0)
for _ in range(768):
    cases.append(case(rng.randint(-8_000_000, 8_000_000), rng.uniform(-3, 3),
                      rng.uniform(-64, 64), rng.uniform(.0001, 4)))
document = {"source": "Own scalar values, IEEE binary32 step rounding and signed truncation; no model assets.", "cases": cases}
serialized = json.dumps(document, indent=2) + "\n"
if len(sys.argv) == 2:
    destination = Path(sys.argv[1])
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_text(serialized, encoding="utf-8")
else:
    print(serialized, end="")
