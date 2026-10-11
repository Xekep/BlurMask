"""Independent IEEE binary32 first-codebook cache-bound composition."""
import json
import random
import struct
import sys
from pathlib import Path


def f(v):
    return struct.unpack('<f', struct.pack('<f', v))[0]


def transform(raw, slope, offset, scale):
    level = f(f(raw) * f(1 / 8192))
    return int(f(f(f(level * f(scale * slope)) - f(scale * offset)) * f(8192)))


rng = random.Random(0xfe150)
cases = []
for i in range(640):
    ws, wo, uo, bs, bo = [f(rng.uniform(-5, 5)) for _ in range(5)]
    scale = f(rng.uniform(.01, 8))
    word, uni, bi = transform(100, ws, wo, scale), transform(11, ws, uo, scale), transform(102, bs, bo, scale)
    cases.append(dict(ws=ws, wo=wo, uo=uo, bs=bs, bo=bo, scale=scale, word=word, uni=uni, bi=bi, bound=min(0x4000000, word + uni, word + bi)))
destination = Path(sys.argv[1]); destination.parent.mkdir(parents=True, exist_ok=True)
destination.write_text(json.dumps(dict(source='Own stored levels and explicit coefficients; binary32 arithmetic, no vendor assets.', cases=cases), separators=(',', ':')) + '\n', encoding='utf8')
print(f'Wrote {len(cases)} independent initial-bound controls.')
