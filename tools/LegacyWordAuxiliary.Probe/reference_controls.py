"""Vendor-free auxiliary cost reference; usage: script.py output.json."""
import json
import random
import struct
import sys
from pathlib import Path


def f32(v):
    return struct.unpack("<f", struct.pack("<f", v))[0]


rng = random.Random(0xFFCF0)
cases = []
for n in range(640):
    transitions = [f32(rng.uniform(-8, 3)) for _ in range(169)]
    category = rng.randrange(13)
    tag = rng.randrange(13)
    history = [] if n % 5 == 0 else [(rng.randrange(16) << 28) | (tag << 24) | rng.randrange(1024)]
    clear, suppress = bool(rng.randrange(2)), bool(rng.randrange(2))
    flags = rng.choice([0, 1, 512, 65535])
    offset, slope = f32(rng.uniform(-2, 2)), f32(rng.uniform(-2, 2))
    word, boundary, scale = f32(rng.uniform(-4, 4)), f32(rng.uniform(-4, 4)), f32(rng.uniform(.01, 3))
    boundary_cost = int(f32(f32(f32(scale * boundary) - f32(scale * word)) * f32(-8192)))
    effective_tag = 0 if clear or not history else (history[0] >> 24) & 15
    count = int(bool(flags and category and effective_tag))
    if suppress:
        cost = 0
    elif count:
        level = f32(f32(f32(slope * transitions[effective_tag * 13 + category]) + offset) * 8192)
        cost = -int(f32(level + (-.5 if level <= 0 else .5)))
    else:
        cost = (-2 if clear else 1) if category == 3 else 2 if category in (5, 6) else 3 if category in (10, 11, 12) else 0
    cases.append(dict(flags=flags, wordOffset=word, boundaryOffset=boundary, categoryOffset=offset,
                      categorySlope=slope, transitions=transitions, scale=scale, boundaryCost=boundary_cost,
                      category=category, history=history, clear=clear, suppress=suppress, categoryCost=cost, taggedCount=count))
destination = Path(sys.argv[1]); destination.parent.mkdir(parents=True, exist_ok=True)
destination.write_text(json.dumps(dict(source="Own transitions/categories and step-wise IEEE binary32 arithmetic; no vendor assets.", cases=cases), separators=(",", ":")) + "\n", encoding="utf-8")
