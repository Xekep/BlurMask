"""Original integer/IEEE controls for2276BD and13CBA0/227CCC publication."""
import json
import math
import random
import struct
import sys
from pathlib import Path

if len(sys.argv) != 2:
    raise SystemExit('controls.json')
rng = random.Random(0x2276bd)
def bits(v): return struct.unpack('<I', struct.pack('<f', v))[0]
def value(v): return struct.unpack('<f', struct.pack('<I', v))[0]
def single(v):
    try: return value(bits(v))
    except OverflowError: return math.copysign(math.inf, v)
def divide(a, b):
    if b == 0:
        if a == 0 or math.isnan(a): return math.nan
        return math.copysign(math.inf, a * math.copysign(1., b))
    return single(a / b)
def wrap(v): return (v + 2**31) % 2**32 - 2**31
def exp(v):
    try: return single(math.exp(v))
    except OverflowError: return math.inf
def minimum(a, b): return b if math.isnan(a) or math.isnan(b) or b <= a else a
def put(row, at, v): struct.pack_into('<I', row, at, bits(v))
def get(row, at): return struct.unpack_from('<f', row, at)[0]
def residue(n): return bytearray(rng.randrange(256) for _ in range(n))

scalar = []
pairs = [(0, 0), (1, 0), (-1, 0), (2**31-1, -2**31), (-2**31, 2**31-1),
         (100000001, 100000000), (16777217, 16777216), (-16777217, -16777216),
         (614400, 0), (-614400, 0), (614401, 0), (-614401, 0),
         (122880, 0), (-122880, 0), (122881, 0), (-122881, 0)]
inputs = [(a, b, n, rng.randrange(-2**31, 2**31), kind)
          for n in [1, 4, 120, 140, 262144, 524288, 524289, 2**31-1]
          for kind in [0x135, 0x137, -1] for a, b in pairs]
inputs += [(rng.randrange(-2**31, 2**31), rng.randrange(-2**31, 2**31),
            rng.choice([1, 7, 120, 262144, 524288, 2**31-1]), rng.randrange(-2**31, 2**31),
            rng.choice([0x135, 0, 0x137])) for _ in range(512)]
for a, b, n, secondary, kind in inputs:
    difference = wrap(a-b)
    divisor = single(wrap(n << 13))
    path = divide(single(difference), divisor)
    cost14 = single(single(secondary) * single(1/8192))
    log = single((-5. if kind == 0x135 else -1.) * path)
    mass = 0. if log < -75 else single(3.73e32) if log > 75 else exp(log)
    old_order = divide(single(single(a)-single(b)), divisor)
    different = math.isfinite(old_order) and math.isfinite(path) and bits(old_order) != bits(path)
    original = residue(40)
    row = original.copy()
    for at, v in [(8, mass), (16, path), (20, cost14)]: put(row, at, v)
    scalar.append(dict(input=[a, b, n, secondary, kind], expected=[difference, bits(path), bits(cost14), bits(log), bits(mass)],
                       differentFloatBeforeSubtract=different, original=original.hex(), row=row.hex()))

normalized = []
special = [0., -0., 1., -1., .5, 2., math.inf, -math.inf, value(0x7fc01234), value(0xffc04321)]
patterns = [([], []), ([0.], [0.]), ([-0.], [-0.]), ([2.], [1.]), ([1., 2., 4.], [9., 3., 6.]),
            ([0., 0.], [0., 0.]), ([1., -1.], [1., -1.]),
            ([math.inf, 1., math.inf], [math.inf, 0., -math.inf]),
            ([1., 2., 3.], [-1e5, math.nan, 1e5]),
            ([math.nan, 1.], [math.nan, 0.]), ([1., math.nan], [0., math.nan])]
cases = [(m, c, old) for m, c in patterns for old in special]
for _ in range(320):
    n = rng.randrange(0, 11)
    cases.append(([rng.choice(special) if rng.randrange(5) == 0 else single(rng.uniform(0, 8)) for _ in range(n)],
                  [rng.choice(special) if rng.randrange(5) == 0 else single(rng.uniform(-80, 80)) for _ in range(n)],
                  rng.choice(special)))
for masses, costs, old in cases:
    n = len(masses)
    original = residue((n + rng.randrange(0, 4)) * 40)
    for i, (m, c) in enumerate(zip(masses, costs)):
        put(original, i*40+8, m); put(original, i*40+20, c)
    row = original.copy()
    top = old
    if n:
        if n == 1: put(row, 8, 1.); put(row, 12, 1.)
        else:
            denominator = 0.
            for i in reversed(range(1, n)):
                denominator = single(masses[i] + masses[0])
                put(row, i*40+8, divide(masses[i], denominator))
            put(row, 8, divide(masses[0], denominator))
            lo = value(0x7f7fffff)
            for c in costs: lo = minimum(lo, c)
            for i, c in enumerate(costs): put(row, i*40+12, exp(-single(c-lo)))
        cap = single(masses[0]*single(.9)) if masses[0] < 1 else single(1.-divide(single(.1), masses[0]))
        top = minimum(old, cap)
    normalized.append(dict(count=n, oldTopBits=bits(old), topBits=bits(top), rawFirstBits=bits(masses[0]) if n else None,
                           original=original.hex(), storage=row.hex()))
report = dict(scalar=scalar, normalization=normalized,
              castOrderDisagreements=sum(x['differentFloatBeforeSubtract'] for x in scalar))
Path(sys.argv[1]).write_text(json.dumps(report, separators=(',', ':'))+'\n', encoding='utf-8')
print(f'GENERATED {len(scalar)} main scalar/{len(normalized)} complete-storage controls; {report["castOrderDisagreements"]} expose cast-order difference.')
