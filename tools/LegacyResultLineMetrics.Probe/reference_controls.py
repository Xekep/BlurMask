"""Original exhaustive prepared source/result line range controls, no vendor code/assets."""
import argparse, itertools, json, struct
from pathlib import Path

def make(ids,line_count):
    # Simultaneous-neighbor formulation is independent of native's forward mutation loop:
    # each slot receives min(original[i],original[i+1]) and the final slot stays unchanged.
    repaired=[min(a,b) for a,b in zip(ids,ids[1:])]+ids[-1:]
    # Result cursor is the longest reachable exact-index prefix, stopping on skipped/backward IDs.
    reachable=[];previous=0
    for index,line in enumerate(repaired):
        if line < previous or line>=line_count:break
        previous=line;reachable.append((index,line))
    bounds=[]
    for line in range(line_count if ids else 0):
        before=sum(value<line for _,value in reachable)
        end=sum(value<=line for _,value in reachable)
        bounds.append([line,before,end])
    heights=[1.125+line*.25 for line in range(line_count)]
    return dict(ids=ids,heights=heights,expectedIds=repaired,ranges=bounds)

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('output',type=Path);a=p.parse_args()
    cases=[]
    for length in range(6):
        for ids in itertools.product(range(4),repeat=length):
            for count in range(5):cases.append(make(list(ids),count))
    a.output.parent.mkdir(parents=True,exist_ok=True)
    a.output.write_text(json.dumps(dict(format='own-result-line-ranges-v1',cases=cases),separators=(',',':')),encoding='utf8')
    print(f'{len(cases)} exhaustive prepared line-range cases')
