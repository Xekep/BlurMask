"""Original raw-run ownership interval oracle, independent of native key writer."""
import json,random,sys
rng=random.Random(0x168e10);cases=[]
for _ in range(256):
    strokes=[];ends=[]
    for stroke in range(rng.randint(1,5)):
        raw=[];ownership=[]
        for point in range(rng.randint(1,12)):
            run=rng.randint(1,4);raw.extend([[stroke*120+point*5,10 if point%2==0 else 90]]*run)
            ownership.append(len(raw)-1)
        strokes.append(raw);ends.extend(ownership)
    cases.append(dict(strokes=strokes,endOrdinals=ends))
cases.append(dict(strokes=[[[0,0]]],endOrdinals=[0]))
cases.append(dict(strokes=[[[0,0],[0,0]]],endOrdinals=[1]))
with open(sys.argv[1],'w',encoding='utf8') as stream:json.dump(cases,stream,separators=(',',':'))
print('Wrote',len(cases),'independent raw ownership interval controls')
