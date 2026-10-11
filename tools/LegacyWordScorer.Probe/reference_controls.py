"""Independent full lexical subtotal controls over the probe's own tiny model."""
import json
import random
import struct
import sys
from pathlib import Path


def f32(v):
    return struct.unpack("<f", struct.pack("<f", v))[0]


def calibrated(raw, slope, offset, scale):
    factor, bias = f32(scale * slope), f32(scale * offset)
    level = f32(f32(raw) * f32(1 / 8192))
    return int(f32(f32(f32(level * factor) - bias) * 8192))


rng = random.Random(0x100090)
cases = []
for n in range(640):
    word_id = rng.randrange(41)
    word_class = word_id * 7 % 5
    tag = rng.randrange(13)
    previous = rng.randrange(5)
    history = [] if n % 3 == 0 else [(tag << 24) | previous]
    flags = rng.choice([0, 1, 512])
    category = rng.randrange(13)
    clear, suppress, boundary = (bool(rng.randrange(2)) for _ in range(3))
    scale = f32(rng.uniform(.01, 4))
    ws, wo, uo, bs, bo, boundary_offset, cat_offset, cat_slope = [f32(rng.uniform(-3, 3)) for _ in range(8)]
    transitions = [f32(rng.uniform(-4, 3)) for _ in range(169)]
    word_cost = calibrated([100, 300, 800][word_id % 3], ws, wo, scale)
    cls_raw = [11, 23, 47][word_class % 3] if not history else [102, 501, 903][(previous * 5 + word_class) % 3]
    cls_cost = calibrated(cls_raw, ws if not history else bs, uo if not history else bo, scale)
    effective_tag = 0 if clear or not history else tag
    if suppress:
        cat_cost = 0
    elif flags and category and effective_tag:
        val = f32(f32(f32(cat_slope * transitions[effective_tag * 13 + category]) + cat_offset) * 8192)
        cat_cost = -int(f32(val + (-.5 if val <= 0 else .5)))
    else:
        cat_cost = (-2 if clear else 1) if category == 3 else 2 if category in (5, 6) else 3 if category in (10, 11, 12) else 0
    boundary_cost = int(f32(f32(f32(scale * boundary_offset) - f32(scale * wo)) * -8192)) if boundary else 0
    cases.append(dict(wordId=word_id, flags=flags, wordSlope=ws, wordOffset=wo, unigramOffset=uo,
                      bigramSlope=bs, bigramOffset=bo, boundaryOffset=boundary_offset, categoryOffset=cat_offset,
                      categorySlope=cat_slope, transitions=transitions, scale=scale, category=category,
                      history=history, clear=clear, suppress=suppress, includeBoundary=boundary,
                      wordCost=word_cost, classCost=cls_cost, categoryCost=cat_cost, boundaryCost=boundary_cost,
                      subtotal=word_cost + cat_cost + boundary_cost + cls_cost))
destination = Path(sys.argv[1]); destination.parent.mkdir(parents=True, exist_ok=True)
destination.write_text(json.dumps(dict(source="Own tiny word/class model, explicit histories and profiles, IEEE binary32 step arithmetic; no vendor assets.", cases=cases), separators=(",", ":")) + "\n", encoding="utf-8")
