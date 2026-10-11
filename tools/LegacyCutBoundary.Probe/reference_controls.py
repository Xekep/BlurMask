"""Independent native sampler252 boundary branch and float32 arithmetic controls."""
import argparse
import json
import math
import random
import struct
from pathlib import Path
def f32(x): return struct.unpack("<f",struct.pack("<f",x))[0]
def bits(x): return struct.unpack("<i",struct.pack("<f",x))[0]
def cost(p,scale):
    c=-math.log(p)*8192.0*scale
    return int(c+0.5 if c>0 else c-0.5)
parser=argparse.ArgumentParser();parser.add_argument("output",type=Path);args=parser.parse_args()
rng=random.Random(0xC2FB0);cases=[]
edges=[0.0,1.0,0.25,0.5,0.75,f32(1e-40),f32(1e-38),f32(0.99999994)]
for n in range(2048):
    base=f32(rng.choice(edges) if n<256 else rng.random())
    previous=f32(rng.random());current=-1.0 if n%7==0 else f32(rng.random())
    threshold=base if current==-1 else f32(f32(base*f32(previous+current))*0.5)
    probability=threshold if n%11==0 else f32(rng.choice(edges) if n<256 else rng.random())
    hint=bool(rng.randrange(2));previous_hint=bool(rng.randrange(2));scale=f32(rng.uniform(0.05,20))
    promoted=(hint or previous_hint) and probability<threshold
    effective=threshold if promoted else probability
    retained=effective>0 and effective>=threshold
    cut=0x4000000 if effective<=0 else cost(effective,scale)
    join=0 if not retained else 0x4000000 if effective>=1 else cost(f32(1-effective),scale)
    cases.append(dict(base=base,previous=previous,current=current,probability=probability,scale=scale,hint=hint,
        previousHint=previous_hint,thresholdBits=bits(threshold),effectiveBits=bits(effective),promoted=promoted,retained=retained,cut=cut,join=join))
args.output.parent.mkdir(parents=True,exist_ok=True)
args.output.write_text(json.dumps(cases,separators=(",",":")),encoding="utf-8")
print(f"Wrote {len(cases)} independent sampler252 cut policy controls.")
