"""Own independent arithmetic controls for native Y-extremum chord samples.

The reference retains the decompiled control flow; the library uses a simpler
extremum predicate. No DLL execution or resource data.
"""
import json
import random
import struct
import sys
from pathlib import Path


def f(value):
    return struct.unpack('<f', struct.pack('<f', value))[0]


def prepare(points):
    if not points:
        return []
    result = [points[0]]
    for p in points[1:]:
        changed = False
        for a, b in zip(p, result[-1]):
            tolerance = max(f(max(abs(a), abs(b)) * f(2.384185791015625e-7)), f(1.1920928955078125e-7))
            if tolerance < abs(f(a-b)):
                changed = True
        if changed:
            result.append(p)
    return result


def extract(points, half):
    if len(points) < 2:
        return []
    anchor_x, anchor_y = points[0]
    older_y = anchor_y
    out = []

    def append(x, y):
        dx, dy = abs(f(x-anchor_x)), abs(f(y-anchor_y))
        horizontal = f(dx * f(.5)) if half else dx
        out.append(max(dy, horizontal))

    for i in range(1, len(points)):
        current_y = points[i][1]
        previous_x, previous_y = points[i-1]
        if current_y == previous_y:
            continue
        first = previous_y <= older_y or previous_y <= current_y
        if first:
            second = older_y <= previous_y
            older_y = previous_y
            if second or current_y <= previous_y:
                continue
        append(previous_x, previous_y)
        older_y = previous_y
        anchor_x, anchor_y = previous_x, previous_y
    if points[-1][1] != anchor_y:
        append(*points[-1])
    return out


def main():
    rng = random.Random(0x68fa0)
    controls = []
    for size in [0, 1, 2, 3, 4, 5, 16, 64, 100]:
        for _ in range(50):
            points = [[f(rng.uniform(-1000, 1000)), f(rng.choice([-1., 0., 1., rng.uniform(-50, 50)]))] for _ in range(size)]
            half = rng.choice([False, True])
            controls.append(dict(points=points, half=half, raw=extract(points, half), prepared=extract(prepare(points), half)))
    for scale in [1., 100., 10000., 1000000.]:
        points = [[f(scale), 0.], [f(scale+scale*1e-8), f(1e-8)], [f(scale), 2.], [f(scale), 2.], [f(scale+10), -2.]]
        for half in [False, True]:
            controls.append(dict(points=points, half=half, raw=extract(points, half), prepared=extract(prepare(points), half)))
    Path(sys.argv[1]).write_text(json.dumps(controls, separators=(',', ':')), encoding='utf8')
    print(f'Wrote {len(controls)} own raw/prepared extrema controls.')


if __name__ == '__main__':
    main()
