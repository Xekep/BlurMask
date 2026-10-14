"""Independent scalar oracle, with explicitly rounded float32 operations."""
import math
import random
import struct
import sys
import json
from pathlib import Path


def f32(x):
    try:
        return struct.unpack('<f', struct.pack('<f', x))[0]
    except OverflowError:
        return math.copysign(math.inf, x)


def divide(a, b):
    if math.isnan(a) or math.isnan(b) or (a == 0 and b == 0) or (math.isinf(a) and math.isinf(b)):
        return math.nan
    if b == 0:
        return math.copysign(math.inf, a * math.copysign(1, b))
    return f32(a / b)


def exp32(x):
    try:
        return f32(math.exp(x))
    except OverflowError:
        return math.inf


def oracle(source, prior):
    result = bytearray(source)
    count = len(source) // 40
    if prior == count:
        return result
    mass = [struct.unpack_from('<f', source, row * 40 + 8)[0] for row in range(count)]
    costs = [struct.unpack_from('<f', source, row * 40 + 20)[0] for row in range(count)]
    # Direct pair formula independent of the native backwards/unrolled walk.
    for row in range(prior, count):
        if count - prior == 1:
            value = 1.0
        else:
            partner = prior + 1 if row == prior else prior
            value = divide(mass[row], f32(mass[row] + mass[partner]))
        struct.pack_into('<f', result, row * 40 + 8, value)
    if count == 1:
        struct.pack_into('<f', result, 12, 1.0)
    else:
        minimum = f32(3.4028234663852886e38)
        for cost in costs[prior:]:
            # MINSS returns its source for unordered/equal inputs.
            minimum = minimum if not math.isnan(minimum) and not math.isnan(cost) and minimum < cost else cost
        for row in range(prior, count):
            struct.pack_into('<f', result, row * 40 + 12, exp32(-f32(costs[row] - minimum)))
    return result


def generate():
    rng = random.Random(0x1F6A70)
    cases = []
    specials = [0.0, -0.0, math.inf, math.nan, 1.0, f32(2**-149)]
    for i in range(768):
        count = i % 17
        prior = rng.randrange(count + 1)
        source = bytearray(rng.randbytes(count * 40))
        for row in range(count):
            mass = specials[(i + row) % len(specials)] if i % 7 == 0 else f32(10 ** rng.uniform(-35, 35))
            cost = specials[(i + row * 3) % len(specials)] if i % 11 == 0 else f32(rng.uniform(-200, 200))
            struct.pack_into('<f', source, row * 40 + 8, mass)
            struct.pack_into('<f', source, row * 40 + 20, cost)
        cases.append(dict(input=source.hex(), prior=prior, expected=oracle(source, prior).hex()))
    return cases


if __name__ == '__main__':
    if len(sys.argv) != 2:
        raise SystemExit('output-controls.json required')
    cases = generate()
    Path(sys.argv[1]).write_text(json.dumps(cases, separators=(',', ':')), encoding='utf-8')
    print(f'Generated {len(cases)} independent float32 publication batches.')
