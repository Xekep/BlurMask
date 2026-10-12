"""Independent float32 source-point/cropped-view/bounds controls; no recognition labels or native DLL execution."""
import argparse
import importlib.util
import json
import random
import struct
from pathlib import Path

previous = Path(__file__).resolve().parents[1] / 'LegacyInkPosition.Probe/reference_controls.py'
spec = importlib.util.spec_from_file_location('position_oracle', previous)
position = importlib.util.module_from_spec(spec); spec.loader.exec_module(position)
f32 = position.f32
MAX = f32(3.4028234663852886e38)


def select(originals, sources):
    result = []
    for a, b in originals:
        begin, end = a >> 32, b >> 32
        for index in range(begin, end + 1):
            has_first = index == begin and a & 0xffffffff != 3
            has_last = index == end and b >> 8 & 255 != 255
            result.append([index, len(sources[index]), a >> 16 & 65535 if has_first else 0,
                b >> 16 & 65535 if has_last else -1,
                f32((0 if a >> 8 & 255 == 255 else a >> 8 & 255) * f32(.005)) if has_first else 0.,
                f32((b >> 8 & 255) * f32(.005)) if has_last else 0.,
                not has_first or a & 255 == 3, not has_last or b & 255 == 5, False])
    return result


def lerp(a, b, part):
    return [f32(f32(f32(y - x) * part) + x) for x, y in zip(a, b)]


def view(source, selection):
    _, _, first, last, fp, lp, *_ = selection
    if last == -1: last = len(source) - 1
    result = []
    if fp: result.append(lerp(source[first], source[first + 1], fp)); first += 1
    result.extend(source[first:last + 1])
    if lp: result.append(lerp(source[last], source[last + 1], lp))
    return result


def bounds(sources, originals):
    output = [MAX, MAX, -MAX, -MAX]
    for s in select(originals, sources):
        point_box = [MAX, MAX, -MAX, -MAX]
        for x, y in view(sources[s[0]], s):
            if x < point_box[0]: point_box[0] = x
            if y < point_box[1]: point_box[1] = y
            if point_box[2] < x: point_box[2] = x
            if point_box[3] < y: point_box[3] = y
        for k in [0, 1]:
            if point_box[k] <= output[k]: output[k] = point_box[k]
        for k in [2, 3]:
            if not point_box[k] <= output[k]: output[k] = point_box[k]
    return output


def controls():
    rng = random.Random(0xc4b)
    views = []
    for case in range(256):
        source = [[f32(rng.uniform(-100, 100)), f32(rng.uniform(-100, 100))] for _ in range(2 + case % 20)]
        first = rng.randrange(len(source) - 1)
        last = rng.randrange(first, len(source))
        fp = [0., f32(.005), f32(.995), .25, .5][case % 5]
        lp = [0., .25, .5, f32(.995)][case % 4] if last + 1 < len(source) else 0.
        if first == last and fp > lp: fp = lp
        selection = [0, len(source), first, last, fp, lp, bool(case & 1), bool(case & 2), False]
        views.append(dict(source=source, selection=selection, points=view(source, selection)))
    cases = []
    for case in range(96):
        sources = [[[f32(rng.uniform(-1000, 1000)), f32(rng.uniform(-1000, 1000))]
                    for _ in range(2 + (case + index) % 6)] for index in range(1 + case % 4)]
        if case == 0: sources = [[[0., -0.], [-0., 0.]]]
        if case == 1: sources = [[[0., 0.]], [[-0., -0.]]]
        original = []
        for source_index, source in enumerate(sources):
            original.append([position.pack(source_index, 0., 3, len(source)),
                             position.pack(source_index, len(source) - 1., 5, len(source))])
        descriptors = select(original, sources)
        positions, separators = position.make_snapshot(descriptors)
        queries = []
        for begin in range(len(positions) + 1):
            for end in range(begin, len(positions) + 1):
                ranges = position.select_ranges(positions, separators, original, begin, end)
                queries.append([begin, end, bounds(sources, ranges)])
        endpoints = [[index % 3 - 1, end] for index, end in enumerate(range(len(positions) + 1))]
        per_item, previous_endpoint = [[], []], 0
        for item, end in endpoints:
            ranges = position.select_ranges(positions, separators, original, previous_endpoint, end)
            if item >= 0: per_item[item].extend(ranges)
            previous_endpoint = end
        cases.append(dict(sources=sources, originals=original, queries=queries, endpoints=endpoints,
                          itemBounds=[bounds(sources, values) for values in per_item]))
    return dict(format='original-native-raw-stroke-geometry-controls-v1', views=views, cases=cases)


if __name__ == '__main__':
    p = argparse.ArgumentParser(); p.add_argument('output', type=Path); args = p.parse_args()
    data = controls(); args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_bytes((json.dumps(data, separators=(',', ':')) + '\n').encode())
    print(f"Prepared {len(data['views'])} point views / {len(data['cases'])} source owners / {sum(len(c['queries']) for c in data['cases'])} cropped bounds")
