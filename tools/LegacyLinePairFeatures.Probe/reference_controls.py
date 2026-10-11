"""Own independent float32 references for the prepared 2e3 stroke/pair profile."""
import json
import importlib.util
import math
import sys
from pathlib import Path
import numpy as np
f = np.float32


def statistics(points):
    prepared = [np.array(points[0], dtype=np.float32)]
    for p in points[1:]:
        p = np.array(p, dtype=np.float32)
        tolerance = np.maximum(np.maximum(abs(p), abs(prepared[-1]))*f(2**-22), f(2**-23))
        if (tolerance < abs(p-prepared[-1])).any(): prepared.append(p)
    minimum, maximum = prepared[0].copy(), prepared[0].copy()
    sums = prepared[0].copy(); arc = f(0); bins = np.zeros(8, np.float32)
    for a, b in zip(prepared, prepared[1:]):
        minimum, maximum = np.minimum(minimum, b), np.maximum(maximum, b)
        sums += b
        dx, dy = b-a
        segment = f(np.sqrt(f(dy*dy)+f(dx*dx))); arc = f(arc+segment)
        angle = f(math.atan2(float(dy), float(dx)))
        index = min(int(math.floor(float(f(angle/f(.7853982)))))+4, 7)
        bins[index] = f(bins[index]+segment)
    mean = sums/f(len(prepared)); width, height = maximum-minimum
    diagonal = f(np.sqrt(f(height*height)+f(width*width)))
    end = f(0)
    if diagonal > 0:
        dx, dy = prepared[-1]-prepared[0]
        end = f(f(np.sqrt(f(dy*dy)+f(dx*dx)))/diagonal)
    ratio = f(1) if arc <= 0 else f(diagonal/arc)
    return np.array([*minimum, *maximum, *mean, arc, *bins, end, ratio], dtype=np.float32)


def pair(a, b, old, new, scale, group_width, skipped):
    ah0, bh0 = f(a[3]-a[1]), f(b[3]-b[1]); floor = max(f(scale*f(.05)), f(3))
    ah, bh = max(ah0, floor), max(bh0, floor)
    union_height = f(max(a[3], b[3])-min(a[1], b[1])); uh = max(union_height, floor)
    gap_y, mean_y = f(b[1]-a[3]), f(b[5]-a[5])
    result = [f(gap_y/x) for x in [ah, bh, uh, scale]]+[f(mean_y/x) for x in [ah, bh, uh, scale]]
    inverse_x = f(a[4]-b[4])
    for inverse_y in [f(a[5]-b[5]), f(a[1]-b[1]), f(a[3]-b[3])]:
        angle = f(np.arctan2(inverse_y, inverse_x))
        result += [f(np.cos(angle)), f(np.sin(angle))]
    areas = [f(f(a[2]-a[0])*ah0), f(f(b[2]-b[0])*bh0),
             f(f(max(a[2], b[2])-min(a[0], b[0]))*union_height)]
    for area in areas:
        root = f(np.sqrt(max(area, f(1)))); result += [f(root/x) for x in [ah, bh, uh]]
    result += [f(b[6]/uh), f(a[6]/uh), b[15], a[15], b[16], a[16], f(np.argmax(b[7:15])), f(np.argmax(a[7:15]))]
    for gap in [f(b[0]-a[2]), f(b[4]-a[4])]: result += [f(gap/x) for x in [ah, bh, uh, group_width, scale]]
    delta = new-old; distance = abs(delta); middle = 0
    if skipped and distance < len(skipped): middle = sum(min(old, new) < x < max(old, new) for x in skipped)
    result += [f(0 if distance < 2 or middle >= distance-1 else (-1 if delta < -1 else 1))]
    if len(result) != 42: raise ValueError('Unexpected channel count')
    return np.array(result, dtype=np.float32)


def main():
    rng = np.random.default_rng(0x759c0); controls = []
    classifiers = []
    if len(sys.argv) > 2:
        spec = importlib.util.spec_from_file_location('line_reference', Path(__file__).parents[1] / 'PreparedLineClassifier.Probe' / 'reference_controls.py')
        line_reference = importlib.util.module_from_spec(spec); spec.loader.exec_module(line_reference)
        classifiers = [line_reference.load(Path(p)) for p in sys.argv[2:]]
    for index in range(200):
        n = int(rng.integers(1, 8)); strokes = []
        for _ in range(n):
            count = int(rng.integers(1, 30)); points = rng.uniform(-100, 100, (count, 2)).astype(np.float32)
            if index % 7 == 0: points[:, 1] = f(50)
            if index % 13 == 0: points[:] = points[0]
            strokes.append(points.tolist())
        stats = [statistics(s) for s in strokes]
        indices = rng.permutation(n).tolist(); skipped = sorted(rng.choice(n, size=int(rng.integers(0, n+1)), replace=False).tolist())
        scale, width = f(rng.uniform(1, 500)), f(rng.uniform(1, 500))
        previous = indices[0]; features = []
        for current in indices:
            features.extend(pair(stats[previous], stats[current], previous, current, scale, width, skipped).tolist()); previous = current
        control = dict(strokes=strokes, statistics=[x.tolist() for x in stats], indices=indices,
                       skipped=skipped, scale=float(scale), width=float(width), expected=features)
        if index < 20 and classifiers:
            predicted = []
            for network, preparation, means, scales in classifiers:
                rows = np.array(features, dtype=np.float32).reshape(n, 42)
                normalized = (rows-means)*scales
                hidden = np.tanh(np.array([line_reference.reference.affine(preparation, x) for x in normalized])).astype(np.float32)
                probabilities, _ = line_reference.reference.infer(network, hidden)
                predicted.append(probabilities.ravel().tolist())
            control['neural'] = predicted
        controls.append(control)
    Path(sys.argv[1]).write_text(json.dumps(controls, separators=(',', ':')), encoding='utf8')
    print(f'Wrote {len(controls)} prepared-stroke and pair-feature controls.')


if __name__ == '__main__': main()
