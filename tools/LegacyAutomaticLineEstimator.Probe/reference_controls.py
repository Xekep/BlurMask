"""Original scalar-node read controls + own complete class24a prepared-line controls."""
import importlib.util
import json
import math
import struct
import sys
from pathlib import Path
import numpy as np

root = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('gaussian_oracle', root / 'tools/LegacyGaussianLineFit.Probe/reference_controls.py')
gaussian = importlib.util.module_from_spec(spec); spec.loader.exec_module(gaussian)


def f(value):
    return float(np.float32(value))


def statistics(strokes):
    ps = [p for s in strokes for p in s]; origin = ps[0][1]
    differences = [f(p[1] - origin) for p in ps]; n = len(ps)
    variance = (sum(f(d*d) for d in differences)*n - sum(differences)**2)/(n*n)
    return f(sum(differences)/n + origin), f(math.sqrt(variance))


def estimate(strokes, profile):
    a, b = gaussian.extrema(strokes, True)
    directions = [[np.sign(t[1] - s[1]) for s, t in zip(stroke, stroke[1:]) if t[1] != s[1]] for stroke in strokes]
    runs = int(sum(1 + sum(i == 0 or values[i] != values[i - 1] for i in range(len(values))) for values in directions if values))
    first, second, endpoints, lo, hi, sparse, dense, reserved = profile
    if runs < sparse:
        mean, std = statistics(strokes); ps = [p for s in strokes for p in s]
        width = f(max(p[0] for p in ps) - min(p[0] for p in ps)); height = f(max(p[1] for p in ps) - min(p[1] for p in ps))
        factor = 5 if height == 0 else max(1, min(5, f(f(width/height) - 2)))
        spread = 1 if width == 0 and height == 0 else max(std, f(width*.05))
        half = f(factor*spread)
        if abs(half) < .0001:
            half = max(f(abs(width)*.01), f(.01))
        return dict(baseline=f(mean+half), upper=f(mean-half), slope=0., runs=runs, dense=False, blended=False)
    lower, upper = gaussian.extrema(strokes, bool(endpoints)); fit = gaussian.fit(lower, upper)
    blend = max(0, min(1, f(f(runs-sparse) / f(dense-sparse)))) if sparse < dense else 1
    slope = min(hi*blend, max(lo*blend, fit[4])); baseline, top = fit[1:3]
    if runs < 20:
        mean, std = statistics(strokes); slope *= .5
        baseline = (mean + baseline + std)*.5; top = (mean + top - std)*.5
    return dict(baseline=baseline, upper=top, slope=slope, runs=runs, dense=True, blended=runs < 20)


def main():
    profiles = []
    header = struct.pack('<IIIHH', 0x0101fe90, 28, 0x24a, 0, 0x102)
    for filename in ['models/ru-RU/ru_RU-ak-cur.lite.res', 'models/en-US/en_US-ak-cur.lite.res']:
        data = (root / filename).read_bytes(); start = 0
        while (offset := data.find(header, start)) >= 0:
            node = data[offset:offset + 36]; values = struct.unpack('<ifBffBBB', node[16:]); profiles.append((node.hex(), values)); start = offset + 36
    if len(profiles) != 6:
        raise ValueError('Expected three original v102 nodes per supported RU/EN acoustic resource')
    # Own profile with endpoint suppression exercises that resource switch.
    own = (0, 100., 0, -.2, .4, 10, 50, 0)
    node = header + struct.pack('<ifBffBBB', *own); profiles.append((node.hex(), struct.unpack('<ifBffBBB', node[16:])))
    cases = []
    rng = np.random.default_rng(0x24a)
    for hexnode, profile in profiles:
        for runs in [9, 10, 11, 19, 20, 21, 49, 50, 51]:
            for tilt in [-1.2, 0., 1.2]:
                x = np.arange(runs, dtype=np.float64)
                y = (np.arange(runs) % 2) * 8 + tilt*x
                strokes = [np.column_stack((x, y)).astype(np.float32).astype(np.float64).tolist()]
                cases.append(dict(node=hexnode, profile=list(profile), strokes=strokes, **estimate(strokes, profile)))
        for k in range(12):
            x = np.linspace(0, 25, 40 + k)
            y = np.sin(np.linspace(0, 10 + 2*k, len(x))) * 4 + rng.uniform(-1, 1)*x
            strokes = [np.column_stack((x, y)).astype(np.float32).astype(np.float64).tolist()]
            cases.append(dict(node=hexnode, profile=list(profile), strokes=strokes, **estimate(strokes, profile)))
    destination = Path(sys.argv[1]); destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_text(json.dumps(cases, allow_nan=False), encoding='utf-8')
    print(f'Generated {len(cases)} original-profile/own-stroke class24a controls')


if __name__ == '__main__':
    main()
