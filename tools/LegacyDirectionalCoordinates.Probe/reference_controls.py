"""Own arbitrary ink and independent integer coordinate-preparation reference.

No vendor weights, ink samples or binary execution are used. The low-segment
scale loop follows the inspected endpoint-doubling path, unlike the managed
implementation's equivalent squared-length recurrence. Square roots use isqrt
plus an integer rounding threshold, not the managed floating-point formula.
"""
import json
from math import isqrt
from pathlib import Path
import random
import sys


def norm(source):
    all_points = [p for stroke in source for p in stroke]
    min_x, min_y = [min(p[axis] for p in all_points) for axis in (0, 1)]
    extent = max(max(p[0] for p in all_points) - min_x, max(p[1] for p in all_points) - min_y)
    largest = 0
    endpoints = None
    for stroke in source:
        for a, b in zip(stroke, stroke[1:]):
            square = min(2**31-1, sum((x-y)**2 for x, y in zip(a, b)))
            if square > largest:
                largest, endpoints = square, (a, b)
    shift = 0
    if largest:
        a, b = endpoints
        start = [a[0] - min_x, a[1] - min_y]
        end = [b[0] - min_x, b[1] - min_y]
        if largest < 16384:
            # Native initially doubles end, tests against doubled start, then
            # performs additional endpoint doublings with shift starting zero.
            end = [value * 2 for value in end]
            while sum((x - y * 2)**2 for x, y in zip(end, start)) < 16384:
                end = [value * 2 for value in end]
                start = [value * 2 for value in start]
                shift += 1
        else:
            while True:
                start = [value >> 1 for value in start]
                end = [value >> 1 for value in end]
                shift -= 1
                if sum((x-y)**2 for x, y in zip(start, end)) < 16384:
                    break
        cap = 14 - (extent >> 1).bit_length()
        shift = min(shift, cap)
    elif extent > 32767:
        return None  # Native singleton-wrap domain is explicitly rejected.
    def scale(value):
        return value << shift if shift >= 0 else value >> -shift
    strokes = [[[scale(p[0] - min_x), scale(p[1] - min_y)] for p in stroke] for stroke in source]
    total, cumulative = 0, []
    for stroke in strokes:
        for index, point in enumerate(stroke):
            if index:
                square = sum((x-y)**2 for x, y in zip(point, stroke[index-1]))
                distance_shift = 0
                if square >= 16384:
                    distance_shift = 2
                    while square >> (2 * distance_shift) >= 16384:
                        distance_shift += 1
                reduced = square >> (2 * distance_shift)
                root = isqrt(reduced)
                if reduced >= root*root + root + 1:
                    root += 1
                total += root << distance_shift
            cumulative.append(total)
    return dict(source=source, strokes=strokes, shift=shift, minY=min_y, cumulative=cumulative)


def distance(a, b):
    square = sum((x-y)**2 for x, y in zip(a, b))
    shift = 0
    if square >= 16384:
        shift = 2
        while square >> (shift * 2) >= 16384:
            shift += 1
    reduced = square >> (2 * shift)
    root = isqrt(reduced)
    if reduced >= root*root + root + 1:
        root += 1
    return root << shift


def resample(case, target):
    step = case['cumulative'][-1] * 1024 // target
    prepared = []
    for stroke in case['strokes']:
        output = [stroke[0]]
        if step:
            for source_point in stroke[1:]:
                origin = output[-1]
                length = distance(origin, source_point)
                offset = step
                # Increment fixed-point offset rather than using a repetition
                # count as the managed implementation does.
                while offset <= length * 1024:
                    fraction = offset // length
                    output.append([(fraction * end + (1024-fraction) * start + 512) >> 10
                                   for start, end in zip(origin, source_point)])
                    offset += step
            if output[-1] != stroke[-1]:
                output.append(stroke[-1])
        prepared.append(output)
    cumulative, total = [], 0
    for stroke in prepared:
        for i, point in enumerate(stroke):
            if i:
                total += distance(stroke[i-1], point)
            cumulative.append(total)
    if len(cumulative) > 255:
        raise ValueError('Own target100 fixture exceeded byte-index bounds')
    unit = 1 if case['shift'] < 0 else 1 << case['shift']
    maximums = [max(p[axis] for stroke in prepared for p in stroke) for axis in (0, 1)]
    return dict(target=target, step=step, strokes=prepared, cumulative=cumulative,
                width=maximums[0]+unit, height=maximums[1]+unit)


rng = random.Random(0x64_313)
cases = []
for index in range(640):
    source = []
    for _ in range(1 + index % 15):
        length = 1 + rng.randrange(14)
        if index % 3:
            x, y = rng.randint(-30000, 30000), rng.randint(-30000, 30000)
            stroke = [[x, y]]
            for _ in range(length - 1):
                x = max(-32768, min(32767, x + rng.randint(-160, 160)))
                y = max(-32768, min(32767, y + rng.randint(-160, 160)))
                stroke.append([x, y])
        else:
            stroke = [[rng.randint(-32768, 32767), rng.randint(-32768, 32767)] for _ in range(length)]
        source.append(stroke)
    result = norm(source)
    if result is not None:
        cases.append(result)
for length in [1, 2, 31, 32, 63, 64, 65, 127, 128, 129, 255, 256, 32767]:
    cases.append(norm([[[0, 0], [length, 0]]]))
cases.append(norm([[[-32768, -32768], [32767, 32767]], [[32767, -32768], [-32768, 32767]]]))
cases.append(norm([[[index - 2048, (index % 3) - 1] for index in range(4096)]]))
for case in cases:
    case['sampling'] = resample(case, 100)
target = Path(sys.argv[1])
target.parent.mkdir(parents=True, exist_ok=True)
with target.open('w', encoding='utf-8') as output:
    json.dump(cases, output, separators=(',', ':'))
print(f'Generated {len(cases)} own normalized-coordinate reference controls')
