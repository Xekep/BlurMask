"""Own float ink and independent scalar population statistics; no native engine execution."""
import json,math,random,struct,sys
from pathlib import Path
def f(x): return struct.unpack('<f',struct.pack('<f',x))[0]
def estimate(strokes,threshold):
    runs=0
    for s in strokes:
        signs=[1 if a[1]>b[1] else -1 for a,b in zip(s,s[1:]) if a[1]!=b[1]]
        if signs:runs+=1+sum(i==0 or signs[i]!=signs[i-1] for i in range(len(signs)))
    if runs>=threshold:return dict(runs=runs,estimated=False)
    ps=[p for s in strokes for p in s]; y0=ps[0][1]
    deviations=[f(p[1]-y0) for p in ps];n=len(ps)
    mean=f(sum(deviations)/n+y0)
    # The native helper squares in float, then accumulates in double.
    variance=(sum(f(d*d) for d in deviations)*n-sum(deviations)**2)/(n*n)
    std=f(math.sqrt(variance));xs=[p[0] for p in ps];ys=[p[1] for p in ps]
    width=f(max(xs)-min(xs));height=f(max(ys)-min(ys))
    factor=5 if height==0 else max(1,min(5,f(f(width/height)-2)))
    spread=1 if height==0 and width==0 else max(std,f(width*.05))
    half=f(factor*spread)
    if abs(half)<.0001:half=max(f(abs(width)*.01),f(.01))
    baseline=f(half+mean);top=f(mean-half)
    return dict(estimated=True,runs=runs,baseline=baseline,height=f(baseline-top),mean=mean,std=std,half=half)
rng=random.Random(0x24a);output=[]
own=[[[[0,20]]],[[[0,3],[0,3]]],[[[0,10],[100,10]]],[[[5,0],[5,20]]],[[[0,0],[.001,.001]]],[[[0,0],[0,.0002]]],[[[0,0],[10,0],[10,20],[0,20]]],[[[0,0],[1,1],[2,0],[3,1],[4,0],[5,1],[6,0],[7,1],[8,0],[9,1]]]]
for strokes in own:
    for threshold in [1,5,10,255]:
        strokes=[[[f(x),f(y)] for x,y in s] for s in strokes]
        output.append(dict(strokes=strokes,threshold=threshold,**estimate(strokes,threshold)))
for i in range(200):
    strokes=[]
    for k in range(1+i%4):
        x=rng.uniform(-200,200);y=rng.uniform(-100,100);s=[]
        for j in range(1+i%13):
            if i%3==0:y+=rng.uniform(0,10)
            elif i%3==1:y+=rng.uniform(-10,10)
            s.append([f(x+j*rng.uniform(.001,40)),f(y)])
        strokes.append(s)
    threshold=[5,10,255][i%3]
    output.append(dict(strokes=strokes,threshold=threshold,**estimate(strokes,threshold)))
Path(sys.argv[1]).parent.mkdir(parents=True,exist_ok=True)
Path(sys.argv[1]).write_text(json.dumps(output,separators=(',',':')),encoding='utf8')
print('Wrote',len(output),'native sparse-branch controls')
