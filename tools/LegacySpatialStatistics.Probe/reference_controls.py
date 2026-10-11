"""Own spatial local16 controls, arbitrary-precision native-arithmetic reference."""
import json,random,sys
from pathlib import Path
def divide(n,d):return (1 if n>=0 else -1)*(abs(n)//d)
def reference(case):
    values=[0]*16
    if not case['members']:return values
    points=[p for stroke in case['strokes']for p in stroke]
    selected=[points[index]for index in case['members']]
    means=[sum(p[axis]for p in selected)//len(selected)for axis in (0,1)]
    unit=2**case['shift']if case['unit']and case['shift']>=0 else 0
    for axis,extent in enumerate((case['width'],case['height'])):
        values[axis]=(max(p[axis]for p in selected)-min(p[axis]for p in selected)+unit)*1024//extent
    values[2]=len(selected)*1024//len(points)
    mean=sum(case['cumulative'][index]for index in case['members'])//len(selected)
    total=case['cumulative'][-1]
    values[3]=mean*1024//total if total else mean
    for index in case['members']:
        flag=case['flags'][index];h,v,endpoint=flag&7,(flag>>3)&7,flag>>6
        if h in (1,2):values[11-h]+=1
        if v in (1,2):values[13-v]+=1
        if endpoint in (1,3):values[4]+=1
        if endpoint in (2,3):values[5]+=1
        classification=case['intersections'][index]
        if classification:values[{1:15,2:13,3:14}[classification]]+=1
    values[15]=values[15]*1024//len(points)
    accumulators=[0,0,0];shift=0
    for x,y in selected:
        dx,dy=x-means[0],y-means[1]
        accumulators=[prior+(delta>>shift)for prior,delta in zip(accumulators,(dx*dx,dy*dy,dx*dy))]
        while max(accumulators[:2])>=2**21:
            accumulators=[v>>1 for v in accumulators];shift+=1
    total=sum(accumulators[:2])
    if total:values[6:9]=[divide(value*1024,total)for value in accumulators]
    return values
def i32(v):return ((v+2**31)%2**32)-2**31
def short(v):return ((v+2**15)%2**16)-2**15
def ratio(v,d):return short(divide(i32(v*2048),d)-1024)
def assemble(case):
    result=[1024]
    for index,value in enumerate(case['global']):
        if index==2:channel=short(divide(i32(value*2048-2048),5)-1024)
        elif index==8:channel=short(short(value)*2)
        else:channel=ratio(value,3072 if index==3 else 3 if 9<=index<=15 else 1024)
        result.append(channel)
    for cell in case['cells']:
        for index,value in enumerate(cell):
            if index==8:channel=short(short(value)*2)
            elif index in (4,5,9,10,11,12,13,14):channel=short(short(value)*2048-1024)
            else:channel=ratio(value,256 if index in (0,1,2,15)else 1024)
            result.append(channel)
    for index in range(1,274):
        low,high=case['minima'][index],case['maxima'][index]
        if low!=high:result[index]=short(divide((result[index]-low)*2048,high-low)-1024)
    return result
rng=random.Random(0x82c0);cases=[]
for index in range(640):
    radius=32767 if index%7==0 else 127
    strokes=[[[rng.randint(0,radius),rng.randint(0,radius)]for _ in range(1+rng.randrange(14))]for _ in range(1+index%15)]
    count=sum(map(len,strokes))
    case=dict(strokes=strokes,width=radius+1,height=radius+1,shift=rng.choice([-10,-1,0,1,6,14]),unit=bool(index%2),
        flags=[rng.choice([0,1,2,8,16,64,128,192,25,26,81,138])for _ in range(count)],
        intersections=[rng.randrange(4)for _ in range(count)],members=[rng.randrange(count)for _ in range(index%256)])
    case['cumulative']=sorted(rng.randrange(2097152)for _ in range(count))if index%9 else [0]*count
    case['expected']=reference(case);cases.append(case)
for index in range(512):
    def raw():return rng.choice([-2**31,2**31-1,-65536,-32768,-1024,-1,0,1,128,1024,32767,65535,rng.randint(-2**31,2**31-1)])
    low,high=[],[]
    for column in range(274):
        a,b=sorted([rng.randint(-32768,32767),rng.randint(-32768,32767)])
        low.append(a);high.append(a if column%17==0 else b)
    case=dict(global_=[raw()for _ in range(17)],cells=[[raw()for _ in range(16)]for _ in range(16)],minima=low,maxima=high)
    case['global']=case.pop('global_');case['expected']=assemble(case);cases.append(case)
path=Path(sys.argv[1]);path.parent.mkdir(parents=True,exist_ok=True)
path.write_text(json.dumps(cases,separators=(',',':')),encoding='utf8');print(f'Wrote{len(cases)} own spatial local16 and calibrated geometry274 controls')
