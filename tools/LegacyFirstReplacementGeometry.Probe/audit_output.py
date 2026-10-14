"""Independent numeric geometry and early-dispatch oracle for captured callbacks."""
import json,math,struct,sys
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
d=json.load(open(sys.argv[1],encoding='utf-8'));seen=set();nonzero=0
for r in d['rows']:
    kind=r['kind'];replacement=r['current']==d['replacement'] or r['prior']==d['replacement']
    if kind==6 or replacement:cost=r['incoming']
    else:
        #Independent bottom/top residuals with each native binary32 stage.
        bottom=f(f(r['cp'][0])-f(r['pp'][0]));top=f(bottom-f(f(r['cp'][1])-f(r['pp'][1])))
        inv=f(1/f(d['height']))
        residual=f(abs(f(f(f(r['a'][1])-f(r['b'][1]))*inv)-top)+abs(f(f(f(r['a'][3])-f(r['b'][3]))*inv)-bottom))
        value=f(min(f(f(residual*f(d['scale']))-f(d['offset'])),f(d['maximum']))*8192)
        delta=math.trunc(f(value+(-.5 if value<=0 else .5)));cost=r['incoming']+delta;nonzero+=delta!=0
    assert r['cost']==cost and r['accepted']==(kind==6 or cost<0x4000000),r
    seen.add((kind,r['current']==d['replacement'],r['prior']==d['replacement']))
assert len(seen)==12 and nonzero
print(f"PASS {len(d['rows'])} independent first140 callback values; all12 kind/endpoint combinations and nonzero ordinary residuals.")
