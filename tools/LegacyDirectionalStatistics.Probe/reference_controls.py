"""Own integer reference for the statically inspected global 17-value pass.

This intentionally supplies prepared geometry and intersection classifications;
it does not infer either of them or run any vendor binary. No resource weights
or handwritten samples are included. Independent big-int arithmetic reproduces
the checked signed int32 numerator and integer truncation explicitly.
"""
import json
from pathlib import Path
import random
import sys


def trunc_div(numerator, denominator):
    return (1 if numerator >= 0 else -1) * (abs(numerator) // denominator)


def signed32(value):
    return ((value + 2**31) % 2**32) - 2**31


def reference(case):
    points = [p for stroke in case['strokes'] for p in stroke]
    n = len(points)
    width, height = case['width'], case['height']
    center = [sum(p[axis] for p in points) // n for axis in (0, 1)]
    result = [0] * 17
    for flag in case['flags']:
        x, y = flag & 7, (flag >> 3) & 7
        if x in (1, 2):
            result[11 - x] += 1
        if y in (1, 2):
            result[13 - y] += 1
    result[13] = case['intersections'].count(2)
    result[14] = case['intersections'].count(3)
    result[15] = case['intersectionCount']
    result[16] = 1024 * case['intersections'].count(1) // n
    result[0] = 1024 * width // (width + height)
    result[1] = 1024 * height // (width + height)
    result[2] = len(case['strokes'])
    result[3] = trunc_div(signed32(case['arc'] * 1024), width + height)
    result[4] = 1024 * center[0] // width
    result[5] = 1024 * center[1] // height
    covariance = [0, 0, 0]
    exponent = 0
    for p in points:
        x, y = p[0] - center[0], p[1] - center[1]
        additions = (x*x, y*y, x*y)
        covariance = [old + (new >> exponent) for old, new in zip(covariance, additions)]
        while max(covariance[:2]) >= 2**21:
            covariance = [value >> 1 for value in covariance]
            exponent += 1
    total = sum(covariance[:2])
    if total:
        result[6:9] = [trunc_div(1024 * value, total) for value in covariance]
    return result


rng = random.Random(0x17_313)
cases = []
for index in range(512):
    width = rng.choice([1, 16, 127, 16384, 32768])
    height = rng.choice([1, 16, 127, 16384, 32768])
    strokes = []
    for _ in range(1 + index % 15):
        strokes.append([[rng.randrange(min(width, 32767) + 1), rng.randrange(min(height, 32767) + 1)]
                        for _ in range(1 + rng.randrange(40))])
    point_count = sum(map(len, strokes))
    case = dict(strokes=strokes, width=width, height=height,
                arc=rng.choice([0, 1, 127, 2**21-1, 2**21, 2**31-1]),
                flags=[rng.choice([0, 1, 2, 8, 16, 64, 128, 192, 25, 26]) for _ in range(point_count)],
                intersections=[rng.randrange(4) for _ in range(point_count)],
                intersectionCount=rng.randrange(point_count + 1))
    case['expected'] = reference(case)
    cases.append(case)
target = Path(sys.argv[1])
target.parent.mkdir(parents=True, exist_ok=True)
with target.open('w', encoding='utf-8') as output:
    json.dump(cases, output, separators=(',', ':'))
print(f'Generated {len(cases)} own global-statistic reference controls')
