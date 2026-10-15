"""Original byte-level2144F0 periodic normalization controls, no engine execution."""
import json
import random
import struct
import sys
from pathlib import Path

rng = random.Random(0x2144F02520)
tests = []


def wrap(value):
    return ((value + 2**31) % 2**32) - 2**31


for case in range(512):
    count = rng.randrange(1, 32)
    end = [0, count, max(0, count - 2)][case % 3]
    delta = [1, 67108863, 8192, rng.randrange(1, 67108864)][case % 4]
    rows = bytearray(rng.randbytes(count * 88))
    for row in range(count):
        cost = rng.choice([-2147483648, -2147483647, -7, 0, 1, 67108863, 67108864, 2147483647])
        struct.pack_into('<i', rows, row * 88 + 16, cost)
        struct.pack_into('<i', rows, row * 88 + 20, wrap(cost + 123456789))
    expected = bytearray(rows)
    for row in range(end):
        old = struct.unpack_from('<i', rows, row * 88 + 16)[0]
        if old < 67108864:
            struct.pack_into('<i', expected, row * 88 + 16, wrap(old - delta))
    vector = [rng.randrange(-2147483648, 2147483648) for _ in range(count)]
    current = rng.randrange(count)
    backward = [0, 1, 8, count * 2][case % 4]
    updated = vector[:]
    for distance in range(backward + 1):
        if current - distance < 0:
            break
        updated[current - distance] = wrap(updated[current - distance] + delta)
    tests.append(dict(rows=rows.hex(), end=end, delta=delta, expected=expected.hex(),
                      vector=vector, current=current, backward=backward, updated=updated))

Path(sys.argv[1]).write_text(json.dumps(tests, separators=(',', ':')), encoding='utf-8')
print(f'Generated {len(tests)} independent full-row/cumulative periodic controls')
