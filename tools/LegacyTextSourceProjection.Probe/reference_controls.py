"""Original float32 scalar/control oracle for legacy source-map projection.

All rows are own prepared indexed source inputs; no vendor code or OCR labels.
"""
import argparse
import json
import math
import random
import struct
from pathlib import Path


def f32(x):
    return struct.unpack('<f', struct.pack('<f', x))[0]


def pack(source, coordinate, mode, count):
    integer = math.floor(coordinate)
    remainder = f32(coordinate - integer)
    fraction = 255 if remainder == 0 and integer == count - 1 else math.trunc(f32(f32(remainder * 200) + 0.5))
    if fraction == 200:
        integer += 1
        fraction = 255 if integer == count - 1 else 0
    return (source << 32) | (integer << 16) | (fraction << 8) | mode


def project(row, start=None, end=None):
    source, count, first, last, fa, fb, ca, cb = row
    last = count - 1 if last == -1 else last
    length = last - first + (1 if fb > 0 else 0)
    start = 0.0 if start is None or start == -1 else start
    end = float(length) if end is None or end == -1 else end

    def coord(relative):
        if first == last:
            return f32(f32(f32(f32(fb - fa) * relative) + first) + fa)
        if relative < 1:
            return f32(f32(f32(f32(1 - fa) * relative) + first) + fa)
        absolute = f32(first + relative)
        if absolute > last:
            absolute = f32(f32(f32(absolute - last) * fb) + last)
        return absolute

    a = pack(source, coord(start), 3, count)
    b = pack(source, coord(end), 5, count)
    bf = (b >> 8) & 255
    if bf == 255:
        bf = 0
    b = (b & ~0xFF00) | bf << 8
    if not ca and start == 0 and end != 0:
        a = (a & ~255) | 7
    quantized_last = math.trunc(f32(f32(fb * 200) + 0.5)) & 255
    if not cb and (b >> 16) & 65535 == last and bf == quantized_last and (
            (a >> 16) & 65535 != last or (a >> 8) & 255 != quantized_last):
        b = (b & ~255) | 1
    elif b & 255 == 5 and (b >> 16) & 65535 == count - 1:
        b = (b & ~0xFF00) | 0xFF00
    return None if b < a else [str(a), str(b)]


def resolve(rows, prefixes, offset, group, ordinal):
    adjusted = ((group + offset + 2**31) % 2**32) - 2**31
    index = ordinal
    if adjusted > 0:
        if adjusted >= len(prefixes) or ordinal < 0:
            return None
        begin = prefixes[adjusted - 1]
        if ordinal >= prefixes[adjusted] - begin:
            return None
        index += begin
    return index if 0 <= index < len(rows) else None


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    rng = random.Random(53320_5)
    projections = []
    for i in range(4096):
        count = rng.randrange(1, 65)
        first = rng.randrange(count)
        last = rng.randrange(first, count)
        fa = f32(rng.randrange(200) * f32(.005)) if first < count - 1 else 0.0
        fb = f32(rng.randrange(200) * f32(.005)) if last < count - 1 else 0.0
        if first == last and fa > fb:
            fa, fb = fb, fa
        row = [rng.randrange(65536), count, first, -1 if last == count - 1 and i % 3 == 0 else last,
               fa, fb, bool(i & 1), bool(i & 2)]
        length = last - first + (fb > 0)
        start = f32(rng.randrange(9) / 8 * length)
        end = f32(start + rng.randrange(9) / 8 * (length - start))
        if i % 3 == 0:
            start = None
        if i % 7 == 0:
            end = -1.0
        projections.append({'row': row, 'start': start, 'end': end, 'expected': project(row, start, end)})
    maps = []
    traversals = []
    for i in range(256):
        sizes = [rng.randrange(1, 5) for _ in range(3)]
        prefixes = [sum(sizes[:k+1]) for k in range(3)]
        rows = [[1000 + k, 6, 0, -1, 0, 0, True, True] for k in range(prefixes[-1])]
        offset = i % 3 - 1
        lookups = [[g, n, resolve(rows, prefixes, offset, g, n)] for g in range(-2, 6) for n in range(-1, 14)]
        maps.append({'rows': rows, 'prefixes': prefixes, 'offset': offset, 'lookups': lookups})
        # Actual normal reference endpoints cross three mapped source-input groups.
        traversal_offset = -(i % 2)
        first_group = -traversal_offset
        last_group = 2 - traversal_offset
        first_ordinal = rng.randrange(sizes[0])
        last_ordinal = rng.randrange(sizes[2])
        first_point = 2 + i % 2
        first_residual = i % 256
        last_point = 4
        last_residual = i * 7 % 256
        ref = [[first_group, first_ordinal, first_point], [last_group, last_ordinal, last_point], first_residual, last_residual]
        expected = []
        for group in range(first_group, last_group + 1):
            adjusted = group + traversal_offset
            start_ordinal = first_ordinal if group == first_group else 0
            end_ordinal = last_ordinal if group == last_group else sizes[adjusted] - 1
            for ordinal in range(start_ordinal, end_ordinal + 1):
                row = rows[resolve(rows, prefixes, traversal_offset, group, ordinal)]
                a = f32(first_point - f32(first_residual / 200)) if group == first_group and ordinal == first_ordinal else None
                b = f32(last_point - f32(last_residual / 200)) if group == last_group and ordinal == last_ordinal else None
                expected.append(project(row, a, b))
        traversals.append({'rows': rows, 'prefixes': prefixes, 'offset': traversal_offset, 'references': [ref], 'expected': expected})
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_bytes((json.dumps({'schema': 1, 'projection': projections, 'maps': maps, 'traversal': traversals},
                                       separators=(',', ':'), allow_nan=False)+'\n').encode('ascii'))
    print(f'Wrote {len(projections)} projections, {len(maps)*len(maps[0]["lookups"])} lookup controls, {len(traversals)} traversals.')


if __name__ == '__main__':
    main()
