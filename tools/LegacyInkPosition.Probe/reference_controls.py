"""Original independently expressed scalar oracle for native bounded position/range ports."""
import argparse
import bisect
import itertools
import json
import math
import random
import struct
from pathlib import Path


def f32(x):
    return struct.unpack('<f', struct.pack('<f', x))[0]


def pack(source, x, mode, count):
    integral = math.floor(x)
    fraction = f32(x - f32(integral))
    if fraction == 0 and integral == count - 1:
        q = 255 if mode in (3, 5) else 0
    else:
        q = int(f32(f32(fraction * 200) + .5))
        if q == 200:
            integral += 1
            q = 255 if mode in (3, 5) and integral == count - 1 else 0
    return source * 2**32 + integral * 2**16 + q * 256 + mode


def normalize(a, b):
    sa, pa, fa, ma = a >> 32, (a >> 16) & 65535, (a >> 8) & 255, a & 255
    sb, pb, fb, mb = b >> 32, (b >> 16) & 65535, (b >> 8) & 255, b & 255
    if sa == sb and pa == pb and (fa == fb or fa == 255 or fb == 255):
        return [((a // 256) * 256 + 3) if fb != 255 else ((a // 65536) * 65536 + 255 * 256 + 3),
                (b // 256) * 256 + 5]
    if ma == 5:
        ma = 3
    elif ma in (1, 7):
        ma = 7
        if fa == 255:
            fa = 0
    terminal = ((b & 65535) + 253) & 65533 == 0
    if mb == 5:
        if not terminal:
            mb = 1
    elif mb == 3:
        mb = 5 if terminal else 1
    elif mb == 7:
        mb = 5
        return [sa * 2**32 + pa * 65536 + fa * 256 + ma,
                sb * 2**32 + pb * 65536 + fb * 256 + mb]
    if mb == 1 and fb == 255:
        fb = 0
    return [sa * 2**32 + pa * 65536 + fa * 256 + ma, sb * 2**32 + pb * 65536 + fb * 256 + mb]


def make_snapshot(selections):
    positions, separators = [], []
    for s in selections:
        source, count, first, last, ff, lf, fc, lc, known = s
        if known:
            positions.extend([source * 2**32 + 3, source * 2**32 + 0x3ff05])
        else:
            if last == -1:
                last = count - 1
            a, b = pack(source, f32(first + ff), 3 if fc else 7, count), pack(source, f32(last + lf), 5 if lc else 1, count)
            positions.append(a)
            for point in range(((a >> 16) & 65535) + 1, ((b >> 16) & 65535) + 1):
                mid = source * 2**32 + point * 65536 + (0xff05 if point == count - 1 else 5)
                if mid < b:
                    positions.append(mid)
            same = a >> 32 == b >> 32 and (a >> 16) & 65535 == (b >> 16) & 65535 and ((a >> 8) & 255 == (b >> 8) & 255 or (a >> 8) & 255 == 255 or (b >> 8) & 255 == 255)
            if not same:
                positions.append(b)
        separators.append(len(positions))
    return positions, separators


def select_ranges(p, separators, originals, begin, end):
    if begin == 0 and end == len(p):
        return originals
    if begin == end:
        return []
    at_end = end == len(p) or end == 0 or end in separators
    if begin > 0 and begin + 1 in separators and begin not in separators:
        begin += 1
        if begin == end and at_end:
            return []
    first_segment = bisect.bisect_right(separators, begin)
    final_segment = bisect.bisect_left(separators, end)
    result = []
    for segment in range(first_segment, final_segment + 1):
        left = begin if segment == first_segment else separators[segment - 1]
        right = separators[segment] - 1 if segment < final_segment else end - int(at_end)
        result.append(normalize(p[left], p[right]))
    return result


def controls():
    rng = random.Random(0xc79)
    codec = []
    for count in [1, 2, 7, 100, 65536]:
        coords = [0., float(count - 1)]
        if count > 1:
            coords += [f32((q + .5) / 200) for q in range(200)]
            for q in range(200):
                bits = struct.unpack('<I', struct.pack('<f', f32((q + .5) / 200)))[0]
                coords += [struct.unpack('<f', struct.pack('<I', bits + step))[0] for step in (-1, 1)]
            coords += [f32(rng.uniform(0, count - 1)) for _ in range(32)]
        for x, mode in itertools.product(coords, [1, 3, 5, 7]):
            source = rng.randrange(2**32)
            codec.append([source, x, mode, count, pack(source, x, mode, count)])
    canonical = []
    for sa, sb, pa, pb, fa, fb, ma, mb in itertools.product([0, 0xffffffff], [0, 0xffffffff], [0, 65535], [0, 65535], [0, 50, 200, 255], [0, 50, 200, 255], [1, 3, 5, 7], [1, 3, 5, 7]):
        a, b = sa * 2**32 + pa * 65536 + fa * 256 + ma, sb * 2**32 + pb * 65536 + fb * 256 + mb
        canonical.append([a, b, *normalize(a, b)])
    snapshots = []
    patterns = [[], [[1, 1, 0, -1, 0., 0., True, True, False]],
                [[1, 2, 0, -1, 0., 0., True, True, False]],
                [[12, 0, 0, 0, 0., 0., False, False, True]]]
    for i in range(64):
        selections = []
        for n in range(1 + i % 4):
            count = rng.randrange(2, 8)
            first, last = 0, count - 1
            selections.append([rng.randrange(100), count, first, last, 0., 0., bool(i & 1), bool(i & 2), False])
        patterns.append(selections)
    for selections in patterns:
        positions, separators = make_snapshot(selections)
        originals = [[0x4400000003, 0x440001ff05]]  # explicit supplied owner snapshot, preserves full-range shortcut
        queries = [[a, b, select_ranges(positions, separators, originals, a, b)]
                   for a in range(len(positions) + 1) for b in range(a, len(positions) + 1)]
        endpoints = [[n % 3 - 1, end] for n, end in enumerate(range(len(positions) + 1))]
        items, previous = [[], []], 0
        for item, end in endpoints:
            ranges = select_ranges(positions, separators, originals, previous, end)
            if item >= 0:
                items[item].extend(ranges)
            previous = end
        snapshots.append(dict(selections=selections, positions=positions, separators=separators,
                              originals=originals, queries=queries, endpoints=endpoints, items=items))
    selected = []
    for case in range(128):
        objects = [[rng.randrange(3, 8), False] for _ in range(1 + case % 4)]
        if case % 11 == 0:
            objects[0] = [0, True]
        original_ranges, descriptors = [], []
        for n in range(1 + case % 3):
            first_source = rng.randrange(len(objects)); last_source = rng.randrange(first_source, len(objects))
            if objects[first_source][1]:
                first_pos = first_source * 2**32 + 3
            else:
                first_pos = pack(first_source, f32(.25 if case & 1 else 0.), 7 if case & 2 else 3, objects[first_source][0])
            if objects[last_source][1]:
                last_pos = last_source * 2**32 + 0x3ff05
            else:
                end = f32(objects[last_source][0] - (1.5 if case & 4 else 1.))
                last_pos = pack(last_source, end, 1 if case & 8 else 5, objects[last_source][0])
            original_ranges.append([first_pos, last_pos])
            for source in range(first_source, last_source + 1):
                partial_first = source == first_source and (first_pos & 0xffffffff) != 3
                partial_last = source == last_source and (last_pos >> 8) & 255 != 255
                first_fraction = 0 if (first_pos >> 8) & 255 == 255 else (first_pos >> 8) & 255
                last_fraction = 0 if (last_pos >> 8) & 255 == 255 else (last_pos >> 8) & 255
                descriptors.append([source, objects[source][0], (first_pos >> 16) & 65535 if partial_first else 0,
                    (last_pos >> 16) & 65535 if partial_last else -1,
                    f32(first_fraction * f32(.005)) if partial_first else 0.,
                    f32(last_fraction * f32(.005)) if partial_last else 0.,
                    not partial_first or first_pos & 255 == 3, not partial_last or last_pos & 255 == 5, objects[source][1]])
        positions, separators = make_snapshot(descriptors)
        queries = [[a, b, select_ranges(positions, separators, original_ranges, a, b)]
                   for a in range(len(positions) + 1) for b in range(a, len(positions) + 1)]
        selected.append(dict(objects=objects, originals=original_ranges, descriptors=descriptors,
                             positions=positions, separators=separators, queries=queries))
    return dict(format='original-native-ink-position-controls-v1', codec=codec, canonical=canonical,
                snapshots=snapshots, selected=selected)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(); parser.add_argument('output', type=Path); args = parser.parse_args()
    result = controls(); args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_bytes((json.dumps(result, separators=(',', ':')) + '\n').encode())
    print(f"Prepared {len(result['codec'])} pack / {len(result['canonical'])} canonical / {len(result['snapshots'])} snapshot controls")
