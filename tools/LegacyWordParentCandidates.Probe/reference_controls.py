"""Independent201270 parent88->request80 byte oracle; source/ending ownership differs."""
import argparse
import json
import random
import struct
from pathlib import Path

root = Path(__file__).resolve().parents[2]
data = (root / 'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
r = random.Random(0x201270)
def put(b, at, fmt, value): struct.pack_into('<' + fmt, b, at, value)
def wrap(value): return (value + 2**31) % 2**32 - 2**31
at = 188226
count = struct.unpack_from('<H', data, 188224)[0] + 3
def ids():
    global at
    n = data[at]
    at = (at + 4) & ~3
    values = struct.unpack_from('<' + 'i' * n, data, at)
    at += 4 * n
    return values
first_ids = ids()
fm = struct.unpack_from('<' + 'b' * count, data, at)
at += count
ids()
start = struct.unpack_from('<' + 'b' * count, data, at)
at += count
end = struct.unpack_from('<' + 'b' * count, data, at)
pairs = []
for first, a, b in zip(fm, start, end):
    for x, y in ((first, a), (first, b), (first_ids.index(0), a), (first_ids.index(0), b)):
        if x >= 0 and y >= 0 and (first_ids[x], y) not in pairs: pairs.append((first_ids[x], y))
tests = []
for index in range(1024):
    parent = bytearray(r.randbytes(88))
    source_node = index % 113
    previous = (index * 3 + 9) % 127
    arc = (index * 7) % 65536
    packed_arc = (index * 11) % 1024
    parent_category = index % len(pairs)
    category = (index * 13 + 3) % len(pairs)
    put(parent, 24, 'I', source_node | parent_category << 16 | packed_arc << 22)
    put(parent, 28, 'H', arc)
    old_cost = (0, 100, -100, -2**31, 2**31 - 1)[index % 5]
    increment = r.randrange(-2**31, 2**31)
    put(parent, 16, 'i', old_cost)
    put(parent, 20, 'i', increment)
    old_label = index % 195
    put(parent, 36, 'H', old_label)
    current = index % 97 + 200
    span = index % 15
    source_kind = (0, 1, 2, 3, 6, 7, 8, 10)[index % 8]
    prior_kind = (0, 1, 2, 3, 5, 6, 7, 8, 10)[index % 9]
    header = r.getrandbits(32) & ~0x1E00 | source_kind << 9
    prior_header = r.getrandbits(32) & ~0x1E00 | prior_kind << 9
    flags, length, permission = r.randrange(256), r.randrange(256), r.randrange(256)
    symbol = (index * 17) % 195
    returned = (index * 19) % 65536
    tag = r.getrandbits(64)
    query_cost = (0, 100, -100, -2**31, 2**31 - 1)[(index // 5) % 5]
    residue = r.randbytes(80)
    packet = bytearray(residue)
    for at, value in ((0, current), (4, source_node), (8, previous), (20, 0), (24, 0),
                      (32, span), (48, increment & 0xFFFFFFFF), (52, 0x4000000), (64, source_kind), (68, prior_kind)):
        put(packet, at, 'I', value)
    for at, value in ((12, returned), (14, packed_arc), (16, arc), (36, symbol), (38, old_label)):
        put(packet, at, 'H', value)
    packet[18] = pairs[category][1]
    packet[19] = pairs[parent_category][1]
    packet[28] = 7 if parent[6] & 1 else flags & 7
    put(packet, 40, 'Q', 0x1122334455667788)
    packet[56], packet[57], packet[58], packet[60] = length, 0, permission, 0
    packet[72], packet[73] = (tag >> 56) & 1, parent[7] & 1
    tests.append(dict(parent=parent.hex(), residue=residue.hex(), expected=packet.hex(), expectedCost=wrap(old_cost + query_cost),
                      current=current, previous=previous, span=span, arc=returned, symbol=symbol, category=category,
                      tag=tag, length=length, flags=flags, header=header, priorHeader=prior_header,
                      permission=permission, queryCost=query_cost))
p = argparse.ArgumentParser()
p.add_argument('output', type=Path)
a = p.parse_args()
a.output.parent.mkdir(parents=True, exist_ok=True)
a.output.write_text(json.dumps(tests, separators=(',', ':')), encoding='utf-8')
print(f'Generated {len(tests)} independent full80 parent requests with distinct source/ending and native wrap')
