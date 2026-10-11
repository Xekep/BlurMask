"""Independent scalar references for prepared line records and 219 scale selection.

Only own numerical controls. Does not load SDK libraries, vendor models or fit lines.
"""
import json
import random
import struct
import sys
from pathlib import Path


def f32(value):
    return struct.unpack('<f', struct.pack('<f', value))[0]


def scales(samples, associated, resolution, profile):
    q, sparse_q, numerator, record_weight = map(f32, profile)
    floor = f32(resolution) / 25.4
    if not samples:
        return [floor or 1., floor or 1.]
    ordered = sorted(map(f32, samples))
    position = f32(f32(len(samples) - 1) * q)
    primary_index = min(int(f32(position + (.5 if position > 0 else -.5))), len(samples) - 1)
    weighted = len(samples) + associated * int(record_weight)
    blend = numerator / (f32(f32(weighted) / record_weight) + 1.)
    capped = min(blend, 1.)
    secondary_position = ((1. - capped) * q + sparse_q * capped) * len(samples)
    secondary_index = min(int(secondary_position + (.5 if secondary_position > 0 else -.5)), len(samples) - 1)
    primary, secondary = ordered[primary_index], ordered[secondary_index] * (blend + 1.)
    if floor > 0 and primary < floor:
        primary, secondary = floor, secondary / primary * floor
    return [primary, secondary]


def main():
    rng = random.Random(0x219)
    scale = []
    for size in [0, 1, 2, 3, 4, 17, 64]:
        for _ in range(40):
            samples = [f32(rng.uniform(.01, 120.)) for _ in range(size)]
            associated = rng.randrange(30)
            resolution = rng.choice([0., 96., 144., 1200.])
            profile = [rng.choice([0., .25, .5, .75, 1.]), rng.choice([0., .5, 1.]),
                       rng.choice([0., .5, 1., 10., 100.]), rng.choice([.5, 1., 1.75, 4.])]
            scale.append(dict(samples=samples, associated=associated, resolution=resolution,
                              profile=profile, expected=scales(samples, associated, resolution, profile)))
    lines = []
    for _ in range(200):
        count = rng.randrange(1, 100)
        row = rng.randrange(count)
        baseline, pitch, height = [f32(rng.uniform(1., 1000.)) for _ in range(3)]
        expected = [f32(f32(f32(0 if count == 1 else row) * pitch) + baseline), height]
        lines.append(dict(cluster=[count, baseline, pitch, height], row=row, expected=expected))
        y, height = f32(rng.uniform(-1000., 1000.)), f32(rng.uniform(.01, 1000.))
        lines.append(dict(bounds=[0., y, 100., height], row=0,
                          expected=[f32(f32(height * f32(.75)) + y), f32(height * f32(.25))]))
    result = dict(scales=scale, lines=lines)
    Path(sys.argv[1]).write_text(json.dumps(result, separators=(',', ':')), encoding='utf8')
    print(f'Wrote {len(scale)} scale and {len(lines)} line controls.')


if __name__ == '__main__':
    main()
