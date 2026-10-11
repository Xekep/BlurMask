"""Independent point-ownership controls for ordinary native samples, no vendor bytes."""
import argparse
import json
import random
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument("output", type=Path)
args = parser.parse_args()
rng = random.Random(0x1B5BA0)
controls = []
for _ in range(768):
    source = [[[rng.randrange(-32768,32768),rng.randrange(-32768,32768)]
               for _ in range(rng.randrange(1,33))] for _ in range(rng.randrange(1,9))]
    ranges = []
    for _ in range(rng.randrange(1,16)):
        stroke = rng.randrange(len(source))
        first = rng.randrange(len(source[stroke]))
        ranges.append([stroke,first,rng.randrange(first+1,len(source[stroke])+1)])
    suppress_start = rng.choice([False,True])
    suppress_end = rng.choice([False,True])
    # Point enumeration uses independently tested set membership, not C# slice.
    selected = [[point for n,point in enumerate(source[s]) if first <= n < last]
                for s,first,last in ranges]
    start = 1024 if not suppress_start and ranges[0][1] == 0 else -1024
    end = 1024 if not suppress_end and ranges[-1][2] == len(source[ranges[-1][0]]) else -1024
    controls.append(dict(source=source,ranges=ranges,selected=selected,
        suppressStart=suppress_start,suppressEnd=suppress_end,startBoundary=start,endBoundary=end))
args.output.parent.mkdir(parents=True,exist_ok=True)
args.output.write_text(json.dumps(controls,separators=(",",":")),encoding="utf-8")
print(f"Wrote {len(controls)} independent native point-range controls.")
