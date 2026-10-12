"""Original scalar oracle for the statically verified source218 -> source2DA writer.

No vendor execution, native data fixtures or inferred recognition labels. Float
inputs and the mixed float/double output use explicit IEEE binary32 narrowing.
"""
import argparse
import json
import math
import random
import struct
from pathlib import Path


def f32(value):
    return struct.unpack('<f', struct.pack('<f', value))[0]


def info(guide, rotation):
    slope, baseline, height, adjusted_baseline, adjusted_height = guide
    if adjusted_height > 0:
        slope, baseline, height = 0.0, adjusted_baseline, adjusted_height
    x = -math.sin(rotation) * baseline if rotation != 0 else -0.0 * baseline
    y = math.cos(rotation) * baseline if rotation != 0 else baseline
    angle = math.atan2(slope, 1.0) + rotation
    # Native169456 uses angle itself here, not tangent(angle).
    return [angle, f32(y - x * angle), height, x, y]


def publish(existing, offset, inputs, guides, rotation):
    count = max([i[1] + 1 for i in inputs if i[0] & 15 != 3 and i[1] >= 0] + [0])
    lines = [list(line) for line in existing] + [[False, 0, 0, 0, 0, 0, 0] for _ in range(count)]
    for index in range(count):
        if index >= len(guides) or guides[index] is None:
            continue
        angle, _, height, x, y = info(guides[index], rotation)
        lines[offset + index] = [True, x, y, angle, height, -1, guides[index][1]]
    return [offset + count, lines]


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    rng = random.Random(1692_021)
    samples = []
    for slope in [-4, -0.75, -0.001, 0, 0.001, 0.75, 4]:
        for rotation in [-1.25, -0.125, 0, 0.125, 1.25]:
            for adjusted in [-2, 0, 0.00001, 17]:
                guide = list(map(f32, [slope, -31.75, 13.25, 21.5, adjusted]))
                rot = f32(rotation)
                samples.append({'guide': guide, 'rotation': rot, 'expected': info(guide, rot)})
    for _ in range(1024):
        guide = list(map(f32, [rng.uniform(-5, 5), rng.uniform(-10000, 10000),
                              rng.uniform(-3, 300), rng.uniform(-5000, 5000), rng.uniform(-1, 150)]))
        rotation = f32(rng.uniform(-3, 3))
        samples.append({'guide': guide, 'rotation': rotation, 'expected': info(guide, rotation)})
    publications = []
    for case in range(512):
        existing_count = case % 5
        existing = [[True, 10 + i, 20 + i, 0.1, 40 + i, 800 + i, 50 + i] for i in range(existing_count)]
        offset = case % (existing_count + 1)
        inputs = [[rng.randrange(256), rng.randrange(-2, 9)] for _ in range(case % 16)]
        # Inactive low-nibble3 rows and negative IDs must not enlarge the table.
        inputs += [[0xA3, 1_000_000], [0xF3, 4096], [0, -1]]
        guides = [None if rng.randrange(4) == 0 else samples[rng.randrange(len(samples))]['guide']
                  for _ in range(case % 10)]
        rotation = f32(rng.uniform(-1, 1))
        publications.append({'existing': existing, 'offset': offset, 'inputs': inputs,
                             'guides': guides, 'rotation': rotation,
                             'expected': publish(existing, offset, inputs, guides, rotation)})
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_bytes((json.dumps({'schema': 1, 'scalar': samples, 'publication': publications},
                                        ensure_ascii=True, separators=(',', ':'), allow_nan=False) + '\n').encode('ascii'))
    print(f'Wrote {len(samples)} scalar and {len(publications)} publication controls: {args.output}')


if __name__ == '__main__':
    main()
