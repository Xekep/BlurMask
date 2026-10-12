"""Own float32 controls: each range supplies its containing source line's x-height.

No vendor binaries/resources or SDK invocation. Requires NumPy; emits supplied output path.
"""
import json, math, random, sys
from pathlib import Path
import numpy as np

f = np.float32
excluded = {10, 13, *range(0x20, 0x30), *range(0x3a, 0x41), *range(0x5b, 0x61),
            *range(0x7b, 0x7f), *range(0xa0, 0xc0), 0xd7, 0xf7, 0x1680, 0x202f, 0x205f, 0x3000,
            *range(0x20a0, 0x20bf), *range(0x2000, 0x200b)}
character_bands = [(0xff00, 0xff60), (0x4e00, 0x9fff), (0x3000, 0x318f), (0x3400, 0x4dbf),
                   (0x20000, 0x2a6df), (0x31a0, 0x31bf), (0x1100, 0x11ff), (0xac00, 0xd7af),
                   (0x31f0, 0x31ff), (0xd800, 0xdbff)]
def wide(cp): return any(lo <= cp <= hi for lo, hi in character_bands)
def reference(c):
    size = f(c['style'])
    if not c['enabled'] or not c['available']: return float(size)
    eligible = [s for s in c['text'] if s[0] not in excluded and not s[5] & 0x200400]
    if not eligible: return float(size)
    values = []
    for cp, n, line_xheight, left, right, flags in eligible:
        if n == 1 or wide(cp): value = f(line_xheight)
        else:
            value = f(f(f(right) - f(left)) / f(n))
            if n == 2: value = f(f(f(line_xheight) + value) / f(2))
        values.append(value)
    total = f(0)
    for value in values: total = f(value + total)
    if c['reference'] > 0: size = f(f(size / f(c['reference'])) * f(total / f(len(values))))
    contributions = [s for s in c['math'] if s[1] != 0]
    if contributions:
        total = f(0)
        for _, value in contributions: total = f(total + f(value))
        count = sum(s[0] for s in contributions)
        chars = sum(s[1] for s in eligible)
        weighted_math = f(f(total / f(len(contributions))) * f(count))
        weighted_text = f(f(chars) * size)
        size = f(f(weighted_math + weighted_text) / f(chars + count))
    return float(size)
def round_away(value): return math.floor(float(value) + .5) if value >= 0 else math.ceil(float(value) - .5)
def quantize(size, ref, step):
    ratio = f(f(size) / f(ref))
    if ref <= size: index = round_away(f(f(ratio - f(1)) / f(step)))
    else: index = max(1, round_away(f(ratio * f(4)))) - 4
    return float(f(f(f(index + 4) * f(ref)) * f(.25))) if index < 0 else float(f(f(f(f(index) * f(step)) + f(1)) * f(ref)))

rng = random.Random(860)
cps = set(range(0x500)) | excluded | {0x1680, 0x202f, 0x205f, 0x3000, 0x20a0, 0x20be, 0x20bf, 0x2000, 0x200a, 0x200b, 0x10ffff}
for lo, hi in character_bands: cps.update([lo - 1, lo, lo + 1, hi - 1, hi, hi + 1])
filters = [[cp, cp in excluded, wide(cp)] for cp in sorted(cps)]
policy = []
for i in range(360):
    choices = [65, 0x410, 0x431, 0x4e00, 0xac00, 0x20000, 0xd800, 0xff21, 0x31f0, 0x31bf, 0x31c0, 10, 32, 43, 0x3000, 0x20be]
    samples = []
    for j in range(i % 9):
        left = f(rng.uniform(-10, 20)); right = f(left + f(rng.uniform(.1, 30)))
        samples.append([rng.choice(choices), rng.choice([1, 2, 3, 7]), float(f(rng.uniform(.1, 12))), float(left), float(right), rng.choice([0, 1 << 10, 1 << 21, 1 << 11])])
    c = dict(style=float(f(rng.uniform(.5, 10))), reference=float(f(rng.uniform(.1, 8))) if i % 11 else 0,
             enabled=i % 13 != 0, available=i % 17 != 0, text=samples,
             math=[[rng.randint(0, 10), float(f(rng.uniform(.1, 10))) if j else 0] for j in range(i % 4)])
    c['expected'] = reference(c); policy.append(c)
# Explicitly exercise all Unicode band edges in the 1/2/3-character branches.
for cp in sorted(cps - set(range(0x500))):
    for n in (1, 2, 3):
        c = dict(style=4., reference=2., enabled=True, available=True, text=[[cp, n, 3., -2., 10., 0]], math=[])
        c['expected'] = reference(c); policy.append(c)
quantized = []
for ref in [1., 4., 7.25]:
    for step in [.1, .25, .5]:
        for k in range(1, 33):
            for offset in [-.00001, 0, .00001]:
                size = float(f(ref * (k / 8 + offset)))
                quantized.append([size, ref, step, quantize(size, ref, step)])
updates = []
for old in [0., 3., -3., 32768., -32768., 100000.]:
    for sign in [-1, 1]:
        for delta in [0, .001, .0019999, .002, .0020001, .003, .009999, .01, .010001, .1]:
            next_size = f(old + sign * delta)
            tolerance = max(f(.002), f(max(abs(f(old)), abs(next_size)) * f(1e-7)))
            updates.append([old, float(next_size), bool(tolerance <= abs(f(f(old) - next_size)))])
out = Path(sys.argv[1]); out.parent.mkdir(parents=True, exist_ok=True)
out.write_text(json.dumps(dict(filters=filters, policy=policy, quantize=quantized, updates=updates)), encoding='utf8')
print(f'Wrote {len(policy)} policy, {len(filters)} Unicode, {len(quantized)} quantization, {len(updates)} update controls: {out}')
