"""Original prepared C1A controls; NumPy solves the independent design system each pass.
No vendor executable, resources, handwriting labels or glyph category tables are used.
"""
import argparse, json, math
from pathlib import Path
import numpy as np

def f(value):
    return float(np.float32(value))

def affinity(t, b, pt, pb):
    return 1 / (abs(t - pt) + .7) + 1 / (abs(b - pb) + .7)

def solve(rows, bounds, force):
    # Parameters: [intercept, slope, height]. Each observation supplies midpoint and span rows.
    design, target = [], []
    for x, top, bottom, pt, pb, weight in rows:
        sw = math.sqrt(weight)
        design += [[sw, x * sw, ((pt + pb) * .5) * sw], [0., 0., (pb - pt) * sw]]
        target += [((top + bottom) * .5) * sw, (bottom - top) * sw]
    a, y = np.array(design), np.array(target)
    normal, rhs = a.T @ a, a.T @ np.array(target)
    normal[1, 1] += 1e-12
    # Native's zero-slope branches are solved independently as a two-variable system.
    reduced = normal[np.ix_([0, 2], [0, 2])]
    if force or bounds == [0., 0.]:
        intercept, height = np.linalg.solve(reduced, rhs[[0, 2]])
        slope = 0.
    else:
        intercept, slope, height = np.linalg.solve(normal, rhs)
        clipped = min(max(slope, bounds[0]), bounds[1])
        if clipped != slope:
            slope = clipped
            if slope == 0:
                intercept, height = np.linalg.solve(reduced, rhs[[0, 2]])
            else:
                # Native constrained solve uses the eliminated X/Q equation, not constrained LS.
                q, x = normal[0, 2], normal[0, 1]
                height = (q * rhs[1] - x * rhs[2] - slope * (q * normal[1, 1] - x * normal[1, 2])) / (q * normal[1, 2] - x * normal[2, 2])
                intercept = (rhs[0] - q * height - x * slope) / normal[0, 0]
    return [float(height), float(slope), float(intercept)]

def run(case):
    items, p = case['items'], case['profile']
    known = sum(i['known'] for i in items)
    known_scale = f(f(min(p[6], f(known))) / f(known)) if p[6] > 0 and known else 1.
    blend = max(0., min(1., f(f(f(len(items)) - f(p[0])) / f(f(p[1]) - f(p[0])))))
    bounds = [p[2] * blend, p[3] * blend]
    current = [1., 0., 0.]
    for iteration in range(251):
        rows = []
        for item in items:
            left, right, top, bottom = item['bounds']
            center = f(f(left + right) * .5)
            if item['clip']:
                width = min(p[5], f(right - left))
                middle = f(f(top + bottom) * .5)
                top, bottom = f(middle - f(width * .5)), f(middle + f(width * .5))
            weights = [c[0] ** 2 * (.009999999776482582 if item['marker'] else 1.) * (known_scale if item['known'] else 1.) for c in item['candidates']]
            nt = ((top - center * current[1]) - current[2]) / current[0]
            nb = ((bottom - center * current[1]) - current[2]) / current[0]
            candidates = item['candidates'][:1] if iteration == 250 else item['candidates']
            for index, c in enumerate(candidates):
                base, coefficient, kind, pt, at, pb, ab = c
                weight = weights[index]
                if iteration in (0, 250):
                    if kind == 2:
                        weight *= .5
                        rows.append([center, top, bottom, at, ab, weight])
                else:
                    score = affinity(nt, nb, pt, pb)
                    if kind == 2:
                        alternate = affinity(nt, nb, at, ab)
                        if score < alternate:
                            score, pt, pb = alternate, at, ab
                    weight = weight * .5 * score
                rows.append([center, top, bottom, pt, pb, weight])
        result = solve(rows, bounds, case['force'])
        delta = sum((result[i] - current[i]) ** 2 for i in (1, 0, 2))
        current = result
        if iteration == 250 or iteration >= 9 and delta < 1e-10:
            nt = ((top - center * current[1]) - current[2]) / current[0]
            nb = ((bottom - center * current[1]) - current[2]) / current[0]
            # Exact observed finisher: LAST geometry AND LAST scratch weights reused for all profiles.
            scratch = weights + [0.] * (20 - len(weights))
            total = 0.
            for item in items:
                for index, c in enumerate(item['candidates']):
                    score = affinity(nt, nb, c[3], c[5])
                    if c[2] == 2: score = max(score, affinity(nt, nb, c[4], c[6]))
                    total += scratch[index] * .5 * score
            return dict(solution=current, iterations=iteration + 1,
                score=min(case['incoming'], total / len(items)),
                output=[f(current[2] - current[0]), f(current[0] * -2), f(current[1])])
    raise AssertionError('unbounded fit')

def create():
    rng = np.random.default_rng(21110)
    cases = []
    for k in range(384):
        n = int(rng.integers(3, 26))
        profile = [2, 20, f(-.4), f(.4), f(1e-6), f(200), f(3)]
        height, intercept, slope = rng.uniform(8, 100), rng.uniform(-80, 250), rng.uniform(-.8, .8)
        items = []
        for j in range(n):
            x = f(j * rng.uniform(12, 40) + 10)
            pt, pb = f(rng.uniform(.5, 1.4)), f(rng.uniform(-1.5, -.3))
            width = f(rng.uniform(8, 70))
            top, bottom = f(intercept + slope*x - height*pt + rng.normal(0, .5)), f(intercept + slope*x - height*pb + rng.normal(0, .5))
            candidates = []
            for c in range(int(rng.integers(1, 5))):
                candidates.append([f(rng.uniform(.1, 1.5)), f(rng.uniform(-4, 4)), 2 if (j+c+k)%3==0 else 1,
                    pt if c == 0 else f(pt + rng.normal(0, .4)), f(pt + rng.uniform(-1.5, 1.5)),
                    pb if c == 0 else f(pb + rng.normal(0, .4)), f(pb + rng.uniform(-1.5, 1.5))])
            items.append(dict(known=(j+k)%2==0, clip=k%13==0, marker=(j+k)%7==0,
                bounds=[f(x-width*.5), f(x+width*.5), top, bottom], candidates=candidates))
        case = dict(profile=profile, force=k%3==0, incoming=1e100 if k%5 else .01, items=items)
        try: case['expected'] = run(case)
        except np.linalg.LinAlgError: continue
        if not all(math.isfinite(v) for v in case['expected']['solution']): continue
        cases.append(case)
    return dict(format='own-prepared-c1a-numpy-v1', cases=cases)

if __name__ == '__main__':
    parser = argparse.ArgumentParser(); parser.add_argument('output', type=Path); args = parser.parse_args()
    result = create(); args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, separators=(',', ':')), encoding='utf8')
    print(f"{len(result['cases'])} original IRLS cases; max iterations={max(c['expected']['iterations'] for c in result['cases'])}")
