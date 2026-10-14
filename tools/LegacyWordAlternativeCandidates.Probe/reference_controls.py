"""Independent byte/array oracle for regular1fe740 and source50 restart tags."""
import argparse
import json
import random
import struct
from pathlib import Path

root = Path(__file__).resolve().parents[2]
data = (root / 'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
r = random.Random(0x1FE740)
at = 188226
count = struct.unpack_from('<H', data, 188224)[0] + 3

def ids():
    global at
    n = data[at]
    at = (at + 4) & ~3
    values = struct.unpack_from('<' + 'i' * n, data, at)
    at += n * 4
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
        if x >= 0 and y >= 0 and (first_ids[x], y) not in pairs:
            pairs.append((first_ids[x], y))

def put(b, offset, fmt, value):
    struct.pack_into('<' + fmt, b, offset, value)

def signed(value):
    return (value + 2**31) % 2**32 - 2**31

packets = []
for index in range(1024):
    parent, related, residue = bytearray(r.randbytes(88)), r.randbytes(88), r.randbytes(80)
    old_category, category = index % len(pairs), (index * 13 + 3) % len(pairs)
    source, packed_arc, arc = index % 113, index * 11 % 1024, index * 7 % 65536
    current, previous, span, returned, symbol = index % 97 + 200, index * 3 % 127, index % 15, index * 19 % 65536, index * 17 % 195
    put(parent, 24, 'I', source | old_category << 16 | packed_arc << 22)
    put(parent, 28, 'H', arc)
    old_cost = (0, 100, -100, -2**31, 2**31 - 1)[index % 5]
    increment = r.randrange(-2**31, 2**31)
    put(parent, 16, 'i', old_cost)
    put(parent, 20, 'i', increment)
    prior_symbol = index % 195
    put(parent, 36, 'H', prior_symbol)
    header, prior_header, tag = r.getrandbits(32), r.getrandbits(32), r.getrandbits(64)
    length, flags, permission = r.randrange(256), r.randrange(256), r.randrange(256)
    b = bytearray(residue)
    for offset, value in ((0, current), (4, source), (8, previous), (20, pairs[category][0]),
                          (24, pairs[old_category][0]), (32, span), (48, increment & 0xFFFFFFFF),
                          (52, 0x4000000), (64, header >> 9 & 15), (68, prior_header >> 9 & 15)):
        put(b, offset, 'I', value & 0xFFFFFFFF)
    for offset, value in ((12, returned), (14, packed_arc), (16, arc), (36, symbol), (38, prior_symbol)):
        put(b, offset, 'H', value)
    b[18], b[19], b[28] = pairs[category][1], pairs[old_category][1], flags & 7
    put(b, 40, 'Q', 0x1122334455667788)
    b[56], b[57], b[58], b[60], b[61] = length, 0, permission, 0, 0
    b[72], b[73] = tag >> 56 & 1, related[7] & 1
    packets.append(dict(parent=parent.hex(), related=related.hex(), residue=residue.hex(), expected=b.hex(),
                        expectedCost=old_cost, current=current, previous=previous, span=span, arc=returned,
                        symbol=symbol, category=category, tag=tag, length=length, flags=flags,
                        header=header, priorHeader=prior_header, permission=permission))

restarts = []
for index in range(512):
    children = [(r.getrandbits(64), r.getrandbits(64)) for _ in range(1 + index % 8)]
    source_id = 1 + index % len(children)
    incoming = r.getrandbits(64) & ~(255 << 32) | source_id << 32
    result = children[source_id - 1][1 if incoming >> 40 & 4 else 0]
    if result >> 40 & 4:
        result = result & ~(255 << 32) | source_id << 32
    restarts.append(dict(children=children, incoming=incoming, expected=result))

groups = []
for index in range(512):
    primary, limited = index % 4, index % 2 == 0
    span = (0, 1, 14, 2**31 - 1, 2**31, 2**32 - 1)[index % 6]
    limit = index % 256
    rows, expected = [], []
    for ordinal in range(primary + 20):
        b = bytearray(r.randbytes(88))
        key, state = (8 << 40) + ordinal // 3 % 3, ordinal // 7 % 2
        b[33] = state | (128 if ordinal % 9 == 0 else 0)
        b[38] = ordinal % 4
        put(b, 40, 'Q', key)
        rows.append(b.hex())
        if ordinal < primary or b[33] & 128 or limited and signed(b[38] + span) > limit:
            continue
        if not expected or expected[-1]['key'] != key or expected[-1]['state'] != state:
            expected.append(dict(key=key, state=state, ordinals=[]))
        expected[-1]['ordinals'].append(ordinal)
    groups.append(dict(rows=rows, primary=primary, span=span, limited=limited, limit=limit, expected=expected))

reductions = []
for index in range(1024):
    operations, expected = [], []
    boundary = index % 2 == 0
    for step in range(32):
        arc = r.randrange(8)
        cost = (0x4000000, 0x4000001, -2**31, 2**31 - 1, -4, 100, 100)[r.randrange(7)]
        increment, parent, a, b = r.randrange(-2**31, 2**31), step, r.randrange(3), r.randrange(3)
        operations.append([arc, cost, increment, parent, a, b])
        matches = [i for i, row in enumerate(expected) if row[0] == arc]
        slot = matches[0] if matches else len(expected)
        if not matches:
            expected.append([arc, 0x4000000, 0x4000000, 0, 0])
        if cost < expected[slot][1]:
            expected[slot] = [arc, cost, increment, parent, 0 if boundary else int(a != 0) | int(b != 0) << 1]
    reductions.append(dict(boundary=boundary, operations=operations, expected=expected))

p = argparse.ArgumentParser()
p.add_argument('output', type=Path)
a = p.parse_args()
a.output.parent.mkdir(parents=True, exist_ok=True)
a.output.write_text(json.dumps(dict(packets=packets, restarts=restarts, groups=groups, reductions=reductions), separators=(',', ':')), encoding='utf-8')
print('Generated1024 full80 packets,512 restart tags,512 groupings and1024 strict-order reductions')
