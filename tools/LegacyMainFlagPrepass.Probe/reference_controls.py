"""Independent original2136 full80 field projection; output argument only."""
import json
import random
import struct
import sys
from pathlib import Path

resource = (Path(__file__).resolve().parents[2] / 'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
p = 188222
version, base = struct.unpack_from('<HH', resource, p)
assert version == 0x201 and base == 192
p += 4
count = resource[p]
p = (p + 4) & ~3
first_ids = struct.unpack_from('<' + 'i' * count, resource, p)
p += 4 * count
first = struct.unpack_from('<' + 'b' * (base + 3), resource, p)
p += base + 3
count = resource[p]
p = (p + 4) & ~3
p += 4 * count
start = struct.unpack_from('<' + 'b' * (base + 3), resource, p)
p += base + 3
end = struct.unpack_from('<' + 'b' * (base + 3), resource, p)
zero = first_ids.index(0)
composites = []
for a, b, c in zip(first, start, end):
    for key in [(a, b), (a, c), (zero, b), (zero, c)]:
        if min(key) >= 0 and key not in composites:
            composites.append(key)
assert len(composites) == 35 and start[base] == 9

rng = random.Random(0x2136d0)
cases = []
for index in range(1024):
    current = 16 + index % 128
    span = index % 15
    previous = current - span - 1
    parent = bytearray(rng.randbytes(88))
    residue = bytearray(rng.randbytes(80))
    category = rng.randrange(len(composites))
    first_node = rng.randrange(previous + 1)
    first_arc = rng.randrange(1024)
    struct.pack_into('<I', parent, 24, first_node | category << 16 | first_arc << 22)
    header = rng.getrandbits(32)
    arc = rng.randrange(65536)
    expected = bytearray(residue)
    def u16(at): return struct.unpack_from('<H', parent, at)[0]
    def u32(at): return struct.unpack_from('<I', parent, at)[0]
    def w16(at, value): struct.pack_into('<H', expected, at, value)
    def w32(at, value): struct.pack_into('<I', expected, at, value & 0xffffffff)
    w32(0, current)
    w32(4, first_node)
    w32(8, previous)
    w16(12, arc)
    w16(14, first_arc)
    w16(16, u16(28))
    expected[18] = start[base] & 255
    expected[19] = composites[category][1]
    w32(20, 6)
    w32(24, 0)
    expected[28] = 5
    w32(32, span)
    w16(36, base)
    w16(38, u16(36))
    struct.pack_into('<Q', expected, 40, 0x1122334455667788)
    w32(48, u32(20))
    expected[56] = 0
    w32(64, header >> 9 & 15)
    w32(68, header >> 9 & 15)
    expected[72] = 0
    expected[73] = parent[7] & 1
    cases.append(dict(current=current, previous=previous, span=span, header=header,
                      arc=arc, parent=parent.hex(), residue=residue.hex(), expected=expected.hex(),
                      cost=struct.unpack_from('<i', parent, 16)[0]))
if len(sys.argv)!=2 or Path(sys.argv[1]).suffix.lower()!='.json':raise ValueError('Supply one output.json')
Path(sys.argv[1]).write_text(json.dumps(cases), encoding='utf8')
print(f'Generated{len(cases)} independent complete80 flag-prepass packets.')
