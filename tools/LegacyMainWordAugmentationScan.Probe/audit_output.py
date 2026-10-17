"""Prepared219580 candidate-scan oracle; explicit controls, no inferred text."""
import base64,json,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('capture.json')
cases=json.loads(Path(sys.argv[1]).read_text());calls=chosen=negative=status1=0
def sign(v,bits):return (v+(1<<(bits-1)))%(1<<bits)-(1<<(bits-1))
for case in cases:
    data=base64.b64decode(case['storage']);starts=case['starts'];last=sign(case['last']>>7,24)
    assert len(data)//40>case['active']and starts[-1]==case['active']
    selected=[];attempts=[]
    for index in range(starts[last],starts[last+1]):
        at=index*40;flags=data[at+32]
        if not flags&8 or case['knowledge98']and not flags&16:continue
        key=struct.unpack_from('<Q',data,at+8)[0];node=sign(key,24);arc=key>>24&65535
        if node!=last or (node<<7)+(arc%2)!=case['last']:continue
        status,firstkey=case['dry'][index];include=False
        if status!=1:
            source=sign(firstkey,24);firstarc=sign(firstkey>>24,16)
            if firstarc<0:first=source+firstarc;negative+=1
            else:first=(source-firstarc%3)<<7
            if first==case['first']:selected.append(index);include=True
        else:status1+=1
        minimum=-1 if case['first']<0 else sign(case['first']>>7,24)
        attempts.append([index,minimum,status,firstkey,include])
    assert attempts==case['attempts']and selected==case['selected']
    calls+=len(attempts);chosen+=len(selected)
assert len(cases)==2048 and calls>1000 and chosen>100 and negative>100 and status1>100
print(f'PASS independent2048 prepared scans/{calls} callbacks/{chosen} selected/{negative} negative-arc endpoints/{status1} code1 skips; inactive allocation tail never scanned')
