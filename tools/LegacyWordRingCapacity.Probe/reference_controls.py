"""Independent literal branch oracle from 1FA300; no managed output input."""
import json, random, sys
from pathlib import Path

def signed(value):
    return (value + 2**31) % 2**32 - 2**31

def native(a, b, c, d):
    # Keep the native branch sequence independent of the C# min/max expression.
    largest_cd = d if d > c else c
    largest = a
    if a < largest_cd:
        largest = d if d > c else c
    total = signed(c + a)
    combined = d if d >= total else total
    left = signed(largest + b)
    return combined if left >= combined else left

rng = random.Random(0x1FA300)
inputs = [(-1, -1, -1, -1), (0, 0, 0, 0), (1, 0, 0, 0),
          (120, 20, 20, 0), (1, 2**31-1, 0, 0), (2**31-1, 1, 1, 0)]
inputs += [tuple(rng.randrange(-200, 1000) for _ in range(4)) for _ in range(512)]
inputs += [tuple(rng.randrange(-2**31, 2**31) for _ in range(4)) for _ in range(512)]
controls = []
for values in inputs:
    distance = native(*values)
    for requested in (1, 39, 40, 41, 79, 80, 81, 1001):
        capacity = next(n for n in range(requested, requested + 40) if n % 40 == 0)
        physical = min(capacity, distance) if distance > 0 else 0
        # Follow native previous-slot alias chains, not modulo arithmetic.
        identities = []
        for node in range(capacity):
            identities.append(node if node < physical else identities[node-distance] if physical else -1)
        controls.append(dict(fields=values, requested=requested, distance=distance,
                             capacity=capacity, physical=physical, identities=identities))
if len(sys.argv) != 2:
    raise ValueError('output.json')
p = Path(sys.argv[1]); p.parent.mkdir(parents=True, exist_ok=True)
p.write_text(json.dumps(controls, separators=(',', ':'))+'\n', encoding='utf-8')
print(f'Generated {len(controls)} independent signed retention/growth/alias controls')
