"""Original scalar oracle. One OUTPUT argument; no resource files are written."""
import json
import random
import sys
from pathlib import Path
if len(sys.argv)!=2 or Path(sys.argv[1]).suffix.lower()!='.json':raise ValueError('Supply one output.json')

rng = random.Random(0x151BF0)
controls = []
for i in range(2048):
    mode = [0, 1, 255][i % 3]
    packed = rng.getrandbits(32)
    associated = bool(i & 1)
    v60, a1, v68, a0 = [rng.randrange(256) for _ in range(4)]
    requested = [-1, 0, 1, -2, 120, 2147483647, -2147483648][i % 7]
    marker = bool(i & 2)
    flags = packed if associated else packed & 0xFFFFFFF7
    limit = (120 if requested == -1 else requested) if mode == 0 and flags & 7 > 1 else 0
    current = rng.getrandbits(32)
    anchor = rng.getrandbits(32)
    state = rng.randrange(256)
    difference = ((current - anchor + 0x80000000) & 0xFFFFFFFF) - 0x80000000
    controls.append(dict(input=[mode, packed, associated, v60, a1, v68, requested, a0, marker],
                         expected=[flags, v60, a1, v68, limit, 1 if marker else a0],
                         parent=[current, anchor, state], allowed=state < 128 and difference <= 120))
with open(sys.argv[1], "w", encoding="utf-8") as output:
    json.dump(controls, output)
