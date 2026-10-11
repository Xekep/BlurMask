"""Generate own sample points and a separate axis-wise landmark reference.

No models, vendor sample ink, or engine execution are used. The axis-wise walk
models the statically checked direction pass (0b6d20); Y's first comparison
at point 2 deliberately retains point 0. This is not a character segmenter.
"""
import json
from pathlib import Path
import random
import sys

if len(sys.argv) != 2:
    raise SystemExit("Usage: reference_controls.py output.json")


def axis_flags(values, minimum, maximum, first_previous):
    result = [0] * len(values)
    direction = 0
    for left, right in zip(values, values[1:]):
        if right != left:
            direction = 1 if right > left else -1
            result[0] = minimum if direction == 1 else maximum
            break
    previous = values[first_previous]
    for index in range(2, len(values)):
        next_direction = 1 if values[index] > previous else -1 if values[index] < previous else 0
        if next_direction:
            if direction == -next_direction:
                result[index - 1] = minimum if next_direction == 1 else maximum
            direction = next_direction
        previous = values[index]
    if direction:
        result[-1] |= maximum if direction == 1 else minimum
    return result


def reference(strokes):
    flags, ends = [], []
    for points in strokes:
        if len(points) == 1:
            flags.append(192)
        else:
            x = axis_flags([p[0] for p in points], 1, 2, 1)
            y = axis_flags([p[1] for p in points], 16, 8, 0)
            stroke_flags = [a | b for a, b in zip(x, y)]
            stroke_flags[0] |= 64
            stroke_flags[-1] |= 128
            flags.extend(stroke_flags)
        ends.append(len(flags))
    return dict(strokes=strokes, flags=flags, ends=ends)


rng = random.Random(0x313)
controls = []
for iteration in range(640):
    strokes = []
    for _ in range(1 + iteration % 15):
        count = 1 + rng.randrange(40)
        # Frequent plateaus, early Y reversals and both signed-short limits.
        xs = [rng.choice([-32768, -100, 0, 0, 0, 1, 100, 32767]) for _ in range(count)]
        ys = [rng.choice([-32768, -100, 0, 0, 0, 1, 100, 32767]) for _ in range(count)]
        strokes.append(list(map(list, zip(xs, ys))))
    controls.append(reference(strokes))
controls.append(reference([[[index - 2048, 0] for index in range(4096)]]))
destination = Path(sys.argv[1])
destination.parent.mkdir(parents=True, exist_ok=True)
with destination.open("w", encoding="utf-8") as output:
    json.dump(controls, output, separators=(",", ":"))
print(f"Generated {len(controls)} own reference cases")
