"""Own local-window fixtures and separate arbitrary-precision reference.

Caller geometry/membership/flags are explicit; no vendor samples, weights or
engine execution. The reference calculates weighted selected point lists and
uses integer covariance rescaling and signed truncation independently.
"""
import json
from pathlib import Path
import random
import sys


def divide(n, d):
    return (1 if n >= 0 else -1) * (abs(n) // d)


def i32(v):
    return ((v + 2**31) % 2**32) - 2**31


def short(v):
    return ((v + 2**15) % 2**16) - 2**15


def ratio(v, denominator):
    return short(divide(i32(v * 2048), denominator) - 1024)


def assemble(case):
    geometry = [1024]
    for index, value in enumerate(case['global']):
        if index == 2:
            output = short(divide(i32(value*2048 - 2048), 5) - 1024)
        elif index == 8:
            output = short(short(value) * 2)
        else:
            output = ratio(value, 3072 if index == 3 else 3 if 9 <= index <= 15 else 1024)
        geometry.append(output)
    for window in case['windows']:
        for index, value in enumerate(window):
            if index == 8:
                output = short(short(value) * 2)
            elif index in (4, 5, 9, 10, 11, 12, 13, 14):
                output = short(short(value) * 2048 - 1024)
            else:
                output = ratio(value, 128 if index == 15 else 1024)
            geometry.append(output)
    for index in range(1, 274):
        low, high = case['minima'][index], case['maxima'][index]
        if high != low:
            geometry[index] = short(divide((geometry[index] - low) * 2048, high-low) - 1024)
    for index, value in enumerate(case['baseline']):
        if index == 3:
            value = i32(value + 4096)
        geometry.append(ratio(value, 8192 if index in (2, 3) else 1024 if index >= 6 else 4096))
    return geometry + case['context'] + [case['start'], case['end']]


def reference(case):
    result = [0] * 16
    if not case['members']:
        return result
    points = [p for stroke in case['strokes'] for p in stroke]
    chosen = [points[i] for i in case['members']]
    means = [sum(p[axis] for p in chosen) // len(chosen) for axis in (0, 1)]
    unit = 2**case['shift'] if case['unit'] and case['shift'] >= 0 else 0
    dimensions = case['width'], case['height']
    for axis in (0, 1):
        result[axis] = 1024 * means[axis] // dimensions[axis]
        extent = max(p[axis] for p in chosen) - min(p[axis] for p in chosen) + unit
        result[axis + 2] = 1024 * extent // dimensions[axis]
    for member in case['members']:
        flag = case['flags'][member]
        horizontal, vertical, endpoints = flag & 7, (flag >> 3) & 7, flag >> 6
        if horizontal in (1, 2):
            result[11-horizontal] += 1
        if vertical in (1, 2):
            result[13-vertical] += 1
        if endpoints in (1, 3):
            result[4] += 1
        if endpoints in (2, 3):
            result[5] += 1
        classification = case['intersections'][member]
        if classification:
            result[{1: 15, 2: 13, 3: 14}[classification]] += 1
    result[15] = result[15] * 1024 // len(points)
    covariance = [0, 0, 0]
    exponent = 0
    for p in chosen:
        x, y = p[0]-means[0], p[1]-means[1]
        covariance = [value + (addition >> exponent) for value, addition in zip(covariance, [x*x, y*y, x*y])]
        while max(covariance[:2]) >= 2**21:
            covariance = [value >> 1 for value in covariance]
            exponent += 1
    total = sum(covariance[:2])
    if total:
        result[6:9] = [divide(value * 1024, total) for value in covariance]
    return result


rng = random.Random(0x7F60)
cases = []
for index in range(640):
    radius = 32767 if index % 7 == 0 else 127
    strokes = [[[rng.randint(0, radius), rng.randint(0, radius)] for _ in range(1 + rng.randrange(14))]
               for _ in range(1 + index % 15)]
    count = sum(map(len, strokes))
    case = dict(strokes=strokes, width=radius+1, height=radius+1,
                shift=rng.choice([-10, -1, 0, 1, 6, 14]), unit=bool(index % 2),
                flags=[rng.choice([0, 1, 2, 8, 16, 64, 128, 192, 25, 26, 81, 138]) for _ in range(count)],
                intersections=[rng.randrange(4) for _ in range(count)],
                members=[rng.randrange(count) for _ in range(index % 256)])
    case['expected'] = reference(case)
    cases.append(case)
for index in range(512):
    def raw():
        return rng.choice([-2**31, 2**31-1, -65536, -32768, -1024, -1, 0, 1, 128, 1024, 32767, 65535, rng.randint(-2**31, 2**31-1)])
    low, high = [], []
    for column in range(274):
        first, last = sorted([rng.randint(-32768, 32767), rng.randint(-32768, 32767)])
        if column % 17 == 0:
            last = first
        low.append(first); high.append(last)
    case = dict(global_=[raw() for _ in range(17)], windows=[[raw() for _ in range(16)] for _ in range(16)],
                baseline=[raw() for _ in range(9)], context=[rng.randint(-32768, 32767) for _ in range(28)],
                start=rng.randint(-32768, 32767), end=rng.randint(-32768, 32767), minima=low, maxima=high)
    case['global'] = case.pop('global_')
    case['expected'] = assemble(case)
    cases.append(case)
target = Path(sys.argv[1]); target.parent.mkdir(parents=True, exist_ok=True)
with target.open('w', encoding='utf-8') as output:
    json.dump(cases, output, separators=(',', ':'))
print(f'Generated {len(cases)} own local-window and calibrated313-column controls')
