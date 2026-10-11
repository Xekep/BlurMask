"""Original scalar references, no vendor assets; usage: script.py output.json."""
import json
import math
import random
import struct
import sys
from pathlib import Path


def f32(v):
    return struct.unpack("<f", struct.pack("<f", v))[0]


def cost(probability):
    p = max(1, probability)
    if p < 16:
        return -int(8192 * math.log(p / 2048)) + 0x58B9
    return -int(8192 * math.log(((p + 7) // 16) / 2048))


def reference(slots, multiplier, mask):
    width = len(next(s for s in slots if s is not None)["probabilities"])
    result = [0] * width
    total = f32(0)
    for slot in slots:
        if slot is None:
            continue
        weight = slot["weight"]
        total = f32(total + weight)
        factor = f32(weight * multiplier)
        for c, p in enumerate(slot["probabilities"]):
            if mask and not mask[c]:
                continue
            raw = cost(p)
            result[c] += raw if factor == 1 else int(f32(f32(raw) * factor))
    inverse = f32(1 / total) if total > 0 else 0
    for c, raw in enumerate(result):
        if total <= 0 or (mask and not mask[c]):
            result[c] = 0x4000000
        else:
            normalized = raw if total == 1 else raw // 2 if total == 2 else int(f32(f32(raw) * inverse))
            result[c] = min(0x4000000, normalized)
    return result


rng = random.Random(0x1B9E20)
cases = []
for n in range(640):
    width = rng.randrange(1, 33)
    slots = [None] * 7
    for k in rng.sample(range(7), rng.randrange(1, 8)):
        slots[k] = dict(probabilities=[rng.randrange(32769) for _ in range(width)],
                        weight=f32(rng.choice([0, .25, .5, 1, 2, rng.uniform(.001, 3)])))
    multiplier = f32(rng.choice([1, .5, 2, rng.uniform(.001, 5)]))
    mask = [] if n % 3 else [rng.randrange(2) for _ in range(width)]
    cases.append(dict(slots=slots, multiplier=multiplier, mask=mask, expected=reference(slots, multiplier, mask)))
destination = Path(sys.argv[1]); destination.parent.mkdir(parents=True, exist_ok=True)
destination.write_text(json.dumps(dict(source="Own randomized probabilities, explicit scalar weights and step-wise IEEE binary32 arithmetic.", cases=cases), separators=(",", ":")) + "\n", encoding="utf-8")
