"""Original ordinary boundary/row oracle. No vendor binary is loaded."""
from pathlib import Path
import json
import random
import struct
import sys

rng = random.Random(203390)
profiles = []

def signed32(value):
    return (value + 2**31) % 2**32 - 2**31

for index in range(512):
    counts = [rng.randint(1, 6) for _ in range(rng.randint(1, 4))]
    physical = [stroke for stroke, count in enumerate(counts) for _ in range(count)]
    boundaries = [node == 0 or physical[node] != physical[node - 1] for node in range(len(physical))]
    tests = []
    for case in range(8):
        node = rng.randrange(len(physical))
        current = -1 if case % 2 == 0 else rng.randrange(len(physical))
        packed = rng.getrandbits(32) & ~8
        if case == 0:
            packed |= 8
        row_count = rng.randrange(7)
        primary = rng.randrange(row_count + 1)
        headers = [rng.choice([0, 0x200, 0x400, 0xa00, 0xc00, 0x1a00]) | rng.randrange(256) for _ in range(4)]
        distance = rng.choice([-2**31, -1, 0, 1, 7, 2**31 - 1])
        rows = []
        for ordinal in range(row_count):
            data = bytearray(rng.getrandbits(8) for _ in range(88))
            source = rng.randrange(4)
            replacement = rng.randrange(5) == 0
            cost = rng.choice([-1, 0, 0x3ffffff, 0x4000000, 0x4000001, 2**31 - 1])
            anchor = rng.choice([-2**31, -1, 0, node, node + 1, 2**31 - 1])
            data[4] = source
            struct.pack_into('<i', data, 16, cost)
            struct.pack_into('<H', data, 36, 65534 if replacement else 0)
            struct.pack_into('<i', data, 56, anchor)
            rows.append({'bytes': data.hex(), 'replacement': replacement})
        def boundary(end, previous):
            width = end - previous
            return 1 <= width <= 8 and boundaries[previous + 1]
        callback = any(boundary(future, node) for future in range(node + 1, min(len(physical), node + 1 + min(node + 2, 8)))) if current == -1 else boundary(current, node)
        expected = bool(packed & 8) or callback
        if not expected:
            for row in rows[:primary] if current == -1 else rows[primary:]:
                data = bytes.fromhex(row['bytes'])
                if struct.unpack_from('<i', data, 16)[0] == 0x4000000:
                    break
                kind = headers[data[4]] & 0x1e00
                delta = signed32(node - struct.unpack_from('<i', data, 56)[0])
                if row['replacement'] or (delta >= distance and 0x200 <= kind <= 0xa00):
                    expected = True
                    break
        tests.append({'node': node, 'current': current, 'packed': packed, 'primary': primary,
            'headers': headers, 'distance': distance, 'rows': rows, 'expected': expected,
            'following': any(boundary(future, node) for future in range(node + 1, min(len(physical), node + 1 + min(node + 2, 8)))),
            'boundary': callback if current != -1 else False})
    profiles.append({'counts': counts, 'tests': tests})

output = Path(sys.argv[1])
output.parent.mkdir(parents=True, exist_ok=True)
output.write_text(json.dumps({'profiles': profiles}, separators=(',', ':')))
print(f'Generated {len(profiles)} independent physical-overlap graphs/{sum(len(p["tests"]) for p in profiles)} language-gate controls.')
