"""Independent own controls for native sampler297 one-range atoms and stitching."""
import argparse
import json
import random
from pathlib import Path
parser=argparse.ArgumentParser();parser.add_argument("output",type=Path);args=parser.parse_args()
rng=random.Random(0x1935E0);tests=[]
for _ in range(1024):
    counts=[rng.randrange(1,65) for _ in range(rng.randrange(1,12))]
    markers=[]
    for stroke,count in enumerate(counts):
        cuts=sorted(rng.sample(range(count),rng.randrange(1,min(count,8)+1)))
        markers += [[rng.randrange(1,16),stroke,p] for p in cuts]
    # Native1935e0 performs exactly next-point+1 on the same source stroke.
    atoms=[]
    for i,(_,stroke,point) in enumerate(markers):
        end=counts[stroke]
        if i+1<len(markers) and markers[i+1][1]==stroke: end=markers[i+1][2]+1
        atoms.append([stroke,point,end])
    start=rng.randrange(len(atoms));length=rng.randrange(1,len(atoms)-start+1)
    # Independent grouping of a path by the native one-shared-endpoint relation.
    groups=[]
    for atom in atoms[start:start+length]:
        if groups and groups[-1][-1][0]==atom[0] and groups[-1][-1][2]-1==atom[1]:groups[-1].append(atom)
        else:groups.append([atom])
    composed=[[g[0][0],g[0][1],g[-1][2]] for g in groups]
    tests.append(dict(counts=counts,markers=markers,atoms=atoms,start=start,length=length,composed=composed))
args.output.parent.mkdir(parents=True,exist_ok=True)
args.output.write_text(json.dumps(tests,separators=(",",":")),encoding="utf-8")
print(f"Wrote {len(tests)} independent sampler297 atom controls.")
