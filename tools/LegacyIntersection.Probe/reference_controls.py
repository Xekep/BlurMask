"""Own synthetic geometry and separate integer segment-intersection reference.

No vendor models, ink samples or binary execution. The reference uses numeric
line records (upper/lower coordinates, range, slope, intercept) rather than the
managed implementation's point helpers. Arithmetic follows the statically
checked 0b5fa0 fixed-point rule, including its overlap representative.
"""
import json
from math import isqrt
from pathlib import Path
import random
import sys


def i32(v):
    return ((v + 2**31) % 2**32) - 2**31


def divide(n, d):
    return (-1 if (n < 0) != (d < 0) else 1) * (abs(n) // abs(d))


def record(first, last):
    if first[1] <= last[1]:
        bottom, top = first, last
    else:
        bottom, top = last, first
    dx, dy = top[0]-bottom[0], top[1]-bottom[1]
    slope = divide(1024*dy, dx) if dx else 2**31-1
    offset = i32(1024*bottom[1] - slope*bottom[0]) if dx else 2**31-1
    return dict(bottom=bottom, top=top, dx=dx, dy=dy, slope=slope, offset=offset,
                minX=min(first[0], last[0]), maxX=max(first[0], last[0]))


def reference(points):
    a, b = record(*points[:2]), record(*points[2:])
    if b['bottom'][1] < a['bottom'][1]:
        a, b = b, a
    if not a['dx'] and not b['dx']:
        if a['bottom'][0] != b['bottom'][0] or b['bottom'][1] > a['top'][1]:
            return None
        return a['bottom'][0], divide(a['top'][1] + b['bottom'][1], 2)
    if a['slope'] == b['slope']:
        if a['offset'] != b['offset']:
            return None
        if a['slope'] < 1025:
            overlap = b['minX'] <= a['maxX'] if a['minX'] < b['minX'] else a['minX'] <= b['maxX']
        else:
            overlap = b['bottom'][1] <= a['top'][1]
        return (divide(a['top'][0] + b['bottom'][0], 2), divide(a['top'][1] + b['bottom'][1], 2)) if overlap else None
    if not a['dx']:
        x = a['bottom'][0]
        y = i32(b['slope']*x + b['offset'] + 512) >> 10
    else:
        x = b['bottom'][0] if not b['dx'] else divide(i32(b['offset']-a['offset']), a['slope']-b['slope'])
        y = i32(a['slope']*x + a['offset'] + 512) >> 10
    for line in (a, b):
        if line['maxX']-line['minX'] < line['dy']:
            if not line['bottom'][1] <= y <= line['top'][1]:
                return None
        elif not line['minX'] <= x <= line['maxX']:
            return None
    return x, y


def rounded_distance(a, b):
    squared = sum((x-y)**2 for x, y in zip(a, b))
    exponent = 0
    if squared >= 16384:
        exponent = 2
        while squared >> (2 * exponent) >= 16384:
            exponent += 1
    reduced = squared >> (2 * exponent)
    root = isqrt(reduced)
    if reduced >= root*root + root + 1:
        root += 1
    return root << exponent


def classification(case):
    step, scale = case['step'], case['gridScale']
    flags = [0] * sum(map(len, case['strokes']))
    if not step:
        return flags, 0
    cell = max(1, (step * scale + 512) // 1024)
    columns, rows = min(100, case['width'] // cell + 4), min(100, case['height'] // cell + 4)
    # Independent reference scans prior records in reverse acquisition order
    # per selected bin instead of using the managed linked-head spatial grid.
    prior_segments, global_start, loops = [], 0, 0
    def squared(point, vertex):
        return i32((point[0]-vertex[0])**2 + (point[1]-vertex[1])**2)
    for stroke_id, stroke in enumerate(case['strokes']):
        for local, (start, end) in enumerate(zip(stroke, stroke[1:])):
            qx = min(columns - 2, max(0, start[0] // cell + 1))
            qy = min(rows - 2, max(0, start[1] // cell + 1))
            for selected_bin in [(qx, qy), (qx+1, qy), (qx, qy+1), (qx+1, qy+1)]:
                for old in reversed(prior_segments):
                    if old['bin'] != selected_bin:
                        continue
                    if old['stroke'] == stroke_id and old['local'] == local - 1:
                        continue
                    if rounded_distance(start, old['a']) * 1024 >= 2 * step:
                        continue
                    crossing = reference([start, end, old['a'], old['b']])
                    if crossing is None:
                        continue
                    ax, ay = end[0]-start[0], end[1]-start[1]
                    bx, by = old['b'][0]-old['a'][0], old['b'][1]-old['a'][1]
                    cross = abs(by*ax - bx*ay)
                    product = rounded_distance(start, end) * rounded_distance(old['a'], old['b'])
                    if i32(product * 1024) >= i32(cross * 3413):
                        continue
                    current_vertex = global_start + local + (squared(crossing, end) < squared(crossing, start))
                    prior_vertex = old['index'] + (squared(crossing, old['b']) < squared(crossing, old['a']))
                    kind = 2 if stroke_id == old['stroke'] else 3
                    for index in (current_vertex, prior_vertex):
                        if flags[index] == 0:
                            flags[index] = kind
                    if kind != 2 or 1 in flags[prior_vertex+1:current_vertex]:
                        continue
                    for index in range(prior_vertex+1, current_vertex):
                        if flags[index] == 0:
                            flags[index] = 1
                    loops += 1
            prior_segments.append(dict(stroke=stroke_id, local=local, index=global_start+local,
                a=start, b=end, bin=(min(columns-1, max(0, start[0] // cell + 1)), min(rows-1, max(0, start[1] // cell + 1)))))
        global_start += len(stroke)
    return flags, loops


rng = random.Random(0x5FA0)
cases = []
for index in range(4096):
    radius = 32767 if index % 3 == 0 else 127
    points = [[rng.randint(-radius, radius), rng.randint(-radius, radius)] for _ in range(4)]
    if index % 11 == 0:
        points[1][0] = points[0][0]
    if index % 13 == 0:
        points[3][1] = points[2][1]
    if index % 17 == 0:
        points[1] = points[0].copy()
    if index % 19 == 0:
        points[2:] = [p.copy() for p in points[:2]]
    result = reference(points)
    cases.append(dict(points=points, hit=result is not None, x=result[0] if result else 0, y=result[1] if result else 0))
for index in range(640):
    radius = 32767 if index % 11 == 0 else 127
    strokes = [[[rng.randint(0, radius), rng.randint(0, radius)] for _ in range(1 + rng.randrange(14))]
               for _ in range(1 + index % 15)]
    case = dict(strokes=strokes, width=radius+1, height=radius+1,
                step=rng.choice([0, 1, 64, 1024, 4096, 16384, 65536]), gridScale=rng.choice([1, 2, 10, 63]))
    case['flags'], case['loops'] = classification(case)
    cases.append(case)
target = Path(sys.argv[1]); target.parent.mkdir(parents=True, exist_ok=True)
with target.open('w', encoding='utf-8') as output:
    json.dump(cases, output, separators=(',', ':'))
print(f'Generated {len(cases)} own fixed-point intersection and spatial-classification controls')
