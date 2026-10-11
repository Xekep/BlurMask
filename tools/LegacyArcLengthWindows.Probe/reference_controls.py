"""Own fresh-state reference for the statically inspected temporal window pass.

No resource models, vendor samples or engine execution. Point memberships use
caller-supplied monotonically increasing physical distances (not pen-up gaps).
"""
import json
from pathlib import Path
import random
import sys


def reference(cumulative, count):
    groups = [[] for _ in range(count)]
    if len(cumulative) <= count:
        for index in range(len(cumulative)):
            groups[index] = [index]
    else:
        stride = cumulative[-1] * 1024 // count
        cut_number = 1
        memberships = []
        for index, distance in enumerate(cumulative):
            if index and distance > (cut_number * stride + 512) // 1024:
                groups[cut_number - 1] = memberships
                memberships = []
                cut_number += 1
            memberships.append(index)
        groups[cut_number - 1] = memberships
    return groups


rng = random.Random(0x16_313)
cases = []
for index in range(640):
    point_count = 1 + index % 255
    count = rng.choice([1, 2, 4, 16, 32, 64])
    cumulative = [0]
    for _ in range(point_count - 1):
        cumulative.append(cumulative[-1] + rng.choice([0, 0, 1, 4, 63, 127, 1024]))
    cases.append(dict(cumulative=cumulative, count=count, windows=reference(cumulative, count)))
target = Path(sys.argv[1])
target.parent.mkdir(parents=True, exist_ok=True)
with target.open('w', encoding='utf-8') as output:
    json.dump(cases, output, separators=(',', ':'))
print(f'Generated {len(cases)} own temporal window reference controls')
