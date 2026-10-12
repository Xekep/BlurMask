"""Independent native-field oracle for226's skipped-statistics special path."""
import json,random,struct,sys,pathlib
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def clip(x):return max(-1,min(1,x))
r=random.Random(0x1c8400);cases=[]
for i in range(512):
    n=1+i%5; maximum=bool(i&1);mean=bool(i&2)
    if i%2:
        points=[[[f(r.randrange(-3,4)*2**-28),f(r.randrange(-3,4)*2**-28)] for _ in range(1+i%4)] for _ in range(n)]
    else:
        x=f(r.randrange(-300,301)*.25);y=f(r.randrange(-300,301)*.25);points=[[[x,y] for _ in range(1+i%4)] for _ in range(n)]
    allp=[p for s in points for p in s]
    width=f(max(p[0] for p in allp)-min(p[0] for p in allp));height=f(max(p[1] for p in allp)-min(p[1] for p in allp))
    assert f(width+1)==1 and f(height+1)==1
    h=f(r.choice([2**-28,.25,1,40]));xr=f(r.randrange(-10,11)*h);yr=f(r.randrange(-10,11)*h)
    four=f(h*4);twice=f(h+h)
    #Named native object fields at special-stage exit, rather than raster stats.
    fields={'miny':min(p[1] for p in allp),'ysum':points[0][0][1],'widthPlusOne':1,'heightPlusOne':1,'arc':0,'columns':1,'rows':1,'firstX':points[0][0][0],'lastX':points[0][0][0],'firstY':points[0][0][1],'lastY':points[0][0][1]}
    edge=max((max if maximum else min)(p[0] for p in s) for s in points)
    output=[1,clip(f(f(fields['miny']-yr)/four)),clip(f(f(fields['ysum']-yr)/four)),clip(f(f(1/twice)-1)),clip(f(f(1/twice)-1)),-1,-.5,-.5,0,0,0,0,clip(f(f(edge-xr)/four)),0,0]+[1 if n==j else -1 for j in [1,2,3]]
    for j in range(3):output.extend([clip(f(f(0-yr)/four)) if j==0 else 0,-1,-1,-1])
    cases.append(dict(strokes=points,height=h,xReference=xr,yReference=yr,maximumX=maximum,pointMeanY=mean,expected=output))
path=pathlib.Path(sys.argv[1]);path.parent.mkdir(parents=True,exist_ok=True);path.write_text(json.dumps(cases,separators=(',',':')),encoding='utf8');print('Wrote512 independent226 zero/tiny native-field references.')
