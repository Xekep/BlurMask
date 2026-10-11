"""Own prepared-ink baseline9 controls; no models or vendor sample data.

The separate integer reference reconstructs explicit prepared baselines, inclusive
zone counts, signed truncation and native wrapped numerators. It does not infer
baselines from handwriting or execute the vendor engine.
"""
import json
from pathlib import Path
import random
import sys


def signed32(v):
    return ((v + 2**31) % 2**32) - 2**31


def trunc(n, d):
    return (1 if n >= 0 else -1) * (abs(n) // d)


def reference(case):
    points = [p for stroke in case['strokes'] for p in stroke]
    y, height = case['baselineY'], case['baselineHeight']
    top = signed32(y-height)
    mean = sum(p[1] for p in points) // len(points)
    above = sum(p[1] < top for p in points)
    below = sum(p[1] >= top and p[1] > y for p in points)
    within = len(points)-above-below
    values = [case['width'], case['height'], case['arc'], signed32(mean-y), top, signed32(case['height']-y)]
    result = [trunc(signed32(value*1024), height) for value in values]
    result.extend(value*1024 // len(points) for value in [above, within, below])
    return result


rng = random.Random(0x9380)
cases = []
for index in range(640):
    radius = 32767 if index % 7 == 0 else 127
    strokes = [[[rng.randint(0, radius), rng.randint(0, radius)] for _ in range(1 + rng.randrange(16))]
               for _ in range(1 + index % 15)]
    case = dict(strokes=strokes, width=radius+1, height=radius+1,
                baselineY=rng.choice([-2**31, -32768, -100, 0, 1, 64, 128, 32767, 2**31-1]),
                baselineHeight=rng.choice([1, 2, 64, 128, 32767, 2**21, 2**31-1]),
                arc=rng.choice([0, 1, 128, 2**21-1, 2**21, 2**31-1]))
    case['expected'] = reference(case); cases.append(case)
target = Path(sys.argv[1]); target.parent.mkdir(parents=True, exist_ok=True)
with target.open('w', encoding='utf-8') as output:
    json.dump(cases, output, separators=(',', ':'))
print(f'Generated {len(cases)} own directional raw-baseline controls')
