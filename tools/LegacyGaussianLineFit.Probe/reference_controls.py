"""Own geometric controls, using an independent 5x5 ridge-system solve.

No vendor executable or model is loaded. Extremum membership is defined by
neighbouring non-flat direction runs; fitting solves the full normal equations,
independent of the port's scalar Schur elimination.
"""
import argparse
import json
import math
from pathlib import Path
import numpy as np


def extrema(strokes, endpoints):
    lower, upper = [], []
    for stroke in strokes:
        # Retain the last sample before a strict change, including plateau ends.
        transitions = [(i, int(np.sign(stroke[i + 1][1] - stroke[i][1])))
                       for i in range(len(stroke) - 1) if stroke[i + 1][1] != stroke[i][1]]
        for j, (i, direction) in enumerate(transitions):
            before = transitions[j - 1][1] if j else 0
            if before == -direction or endpoints and before == 0:
                (lower if direction == -1 else upper).append(stroke[i])
        if endpoints and transitions:
            (lower if transitions[-1][1] == 1 else upper).append(stroke[-1])
    if not lower and not upper:
        lower.append(strokes[0][0]); upper.append(strokes[0][0])
    return lower, upper


def fit(lower, upper):
    points = np.array(lower + upper, dtype=np.float64)
    n = len(points); center = float(np.sum(points[:, 0]) / n)
    mean = float(np.sum(points[:, 1]) / n)
    variance = (1e-6 + float(np.sum((points[:, 1] - mean)**2))) / n
    prior = mean + math.sqrt(variance) * np.array([float(np.float32(2.6)), .5, float(np.float32(-.65)), -3])
    means = prior.copy(); pv = variance * np.array([2., 4., 4., 2.])
    lv = variance * .25; normalizer = 1 / math.sqrt(lv * 6.283185005187988)
    slope_pv = n * float(np.float32(.1)); slope = 0.
    labels = np.array([0] * len(lower) + [2] * len(upper)); xs = points[:, 0] - center
    for iteration in range(20):
        adjusted = points[:, 1] - xs * slope
        arguments = ((adjusted[:, None] - means[None, :])**2 / lv).astype(np.float32) * np.float32(-.5)
        scores = np.exp(arguments.astype(np.float64)) * normalizer
        selected = labels + (scores[np.arange(n), labels] < scores[np.arange(n), labels + 1])
        accepted = scores[np.arange(n), selected] > normalizer * .75 * (.25 if iteration == 0 else 1)
        # Unknowns are [shared slope, component intercept 0..3].
        normal = np.diag(np.r_[n / slope_pv, n / pv])
        target = np.r_[0., n * prior / pv]
        for p, x, component, keep in zip(points, xs, selected, accepted):
            if not keep:
                continue
            row = np.zeros(5); row[0] = x; row[component + 1] = 1.
            normal += np.outer(row, row) / lv
            target += row * p[1] / lv
        parameters = np.linalg.solve(normal, target)
        next_means = parameters[1:]; slope = float(parameters[0])
        distance = float(np.linalg.norm(next_means - means) * .5)
        means = next_means; pv[[0, 3]] *= 2
        if not distance > 0:
            break
    return [float(v) for v in means - center * slope] + [slope]


def main():
    parser = argparse.ArgumentParser(); parser.add_argument('output'); args = parser.parse_args()
    rng = np.random.default_rng(249); cases = []
    shapes = [[[0., 0.]], [[0., 0.], [1., 0.]], [[0., 1.], [2., 4.]],
              [[0., 0.], [1., 4.], [2., 4.], [3., 1.], [4., 1.], [5., 5.]],
              [[-2., -1.], [0., 2.], [2., -1.], [4., 3.]]]
    for shape in shapes:
        for endpoints in [False, True]:
            strokes = [shape]; lower, upper = extrema(strokes, endpoints)
            cases.append(dict(strokes=strokes, endpoints=endpoints, lower=lower, upper=upper, fit=fit(lower, upper)))
    for k in range(120):
        stroke_count = int(rng.integers(1, 8)); scale = float(10**rng.uniform(-1, 2)); slope = float(rng.uniform(-.7, .7))
        strokes = []
        for j in range(stroke_count):
            count = int(rng.integers(2, 30)); x = np.linspace(j * 8, j * 8 + 6, count)
            y = np.sin(np.linspace(0, rng.uniform(2, 15), count)) * rng.uniform(.5, 3) + x * slope
            xy = np.column_stack((x, y)) * scale + np.array([float(rng.uniform(-50, 50)), float(rng.uniform(-50, 50))])
            strokes.append(xy.astype(np.float32).astype(np.float64).tolist())
        endpoints = bool(k % 2); lower, upper = extrema(strokes, endpoints)
        cases.append(dict(strokes=strokes, endpoints=endpoints, lower=lower, upper=upper, fit=fit(lower, upper)))
    # Prepared extrema exercises separate populations, missing populations and many observations.
    for k in range(60):
        lower = rng.normal([0., 3.], [20., 2.], (int(rng.integers(0, 80)), 2)).astype(np.float32).astype(np.float64).tolist()
        upper = rng.normal([0., -3.], [20., 2.], (int(rng.integers(1, 80)), 2)).astype(np.float32).astype(np.float64).tolist()
        cases.append(dict(lower=lower, upper=upper, fit=fit(lower, upper)))
    Path(args.output).parent.mkdir(parents=True, exist_ok=True)
    Path(args.output).write_text(json.dumps(cases, allow_nan=False), encoding='utf-8')
    print(f'Generated {len(cases)} independent Gaussian/ridge and extremum controls')


if __name__ == '__main__':
    main()
