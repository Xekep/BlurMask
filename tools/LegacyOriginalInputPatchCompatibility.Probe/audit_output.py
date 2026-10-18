"""Independent unsigned range preflight, actual original domain and failure outputs."""
import argparse,base64,json,struct
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('capture',type=Path);a=p.parse_args();d=json.loads(a.capture.read_bytes())
b64=base64.b64decode
def rows(s):
    b=b64(s);assert len(b)%16==0
    return list(struct.iter_unpack('<QQ',b))
def normalized(v):return v&~0xff00 if(v>>8)&255==255 else v
def contains(domain,request):
    #Linear membership is independent of native binary-search implementation.
    return all(first<=last and any(normalized(a)<=normalized(first)<=normalized(b) and last<=b for a,b in domain)for first,last in request)
checked=positive=refused=0
for item in d['actual']:
    counts=item['pointCounts'];assert all(x>0 for x in counts)
    first=3|(0xff00 if counts[0]==1 else 0)
    last=((len(counts)-1)<<32)|((counts[-1]-1)<<16)|0xff05
    assert b64(item['domain'])==struct.pack('<QQ',first,last)
    domain=rows(item['domain']);commands=item['commands']
    for i,c in enumerate(commands):
        assert c['identity']==i+1 and c['operation'] in (4,7)
        if c['operation']==4:assert contains(domain,rows(c['ranges']))
    state=item['state'];worked=False;compatible=0;written=False;inspected=0;faulted=False;presence=b''
    if not item['preentryClosed']:
        faulted=True
        if item['failure']>=0:compatible=1;written=True
        else:
            presence=bytes([1])*len(counts);worked=True;compatible=1;written=True;faulted=False
            for i,c in enumerate(commands):
                if i==item['cancel']:
                    worked=False;compatible=0;written=False;faulted=True;break
                inspected=i+1
                if i==item['incompatible']:
                    compatible=0;faulted=True;break
    assert state['wasRun']==(not item['preentryClosed'])
    assert state['worked']==worked and state['compatible']==compatible and state['written']==written
    assert state['faulted']==faulted and state['inspected']==inspected
    assert state['ready']==(worked and compatible!=0 and written and not faulted)
    assert b64(state['presence'])==presence and b64(state['outputs'])==bytes([worked,compatible])
    checked+=inspected;positive+=state['ready'];refused+=faulted
assert len(d['actual'])==11 and positive==4 and refused==5
for c in d['prepared']:assert c['compatible']==contains(rows(c['domain']),rows(c['request']))
assert len(d['prepared'])==2048 and d['guards']>=100
print(f'PASS4 actual original C20/C21 domains/{checked} inspected commands +5 explicit prepared refusal prefixes/2 closed-owner cases/2048 independent unsigned range controls/{d["guards"]} guards.')
