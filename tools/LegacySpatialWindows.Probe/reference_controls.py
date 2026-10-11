"""Own spatial points, independent bin lookup; no SDK execution or assets."""
import json,random,sys
from pathlib import Path
randomizer=random.Random(1840)
cases=[]
for i in range(640):
    n=randomizer.randrange(1,256)
    width,height=randomizer.randrange(1,2049),randomizer.randrange(1,2049)
    rows,columns=randomizer.randrange(1,17),randomizer.randrange(1,17)
    quantiles=bool(i&1)
    points=[(randomizer.randrange(-5,width+6),randomizer.randrange(-5,height+6))for _ in range(n)]
    if i%5==0: points=[(x,0)for x,y in points]
    if i%7==0: points=[(0,y)for x,y in points]
    def locate(value,axis,dimension,bins):
        if quantiles:
            ordered=sorted(p[axis]for p in points)
            intervals=[(ordered[k*n//bins],ordered[(k+1)*n//bins]if k+1<bins else ordered[-1])for k in range(bins)]
            matching=[k for k,(low,high)in enumerate(intervals)if low<=value<high]
            return matching[0]if matching else bins-1
        scaled=value*1024
        step=dimension*1024//bins
        # Independent quotient form for equal intervals, with native fallback.
        candidate=scaled//step
        return candidate if 0<=candidate<bins else bins-1
    groups=[[]for _ in range(rows*columns)]
    for index,(x,y)in enumerate(points):groups[locate(y,1,height,rows)*columns+locate(x,0,width,columns)].append(index)
    cases.append(dict(points=points,width=width,height=height,rows=rows,columns=columns,quantiles=quantiles,groups=groups))
path=Path(sys.argv[1]);path.parent.mkdir(parents=True,exist_ok=True)
path.write_text(json.dumps(dict(cases=cases),separators=(',',':')),encoding='utf8')
print(f'Wrote{len(cases)} own controls to{path}')
