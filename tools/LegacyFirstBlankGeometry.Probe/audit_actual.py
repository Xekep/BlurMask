"""Independent prepared-point bbox/prefix/blank cache and dispatch audit."""
import json,math,struct,sys
def f(x):
    try:return struct.unpack('<f',struct.pack('<f',x))[0]
    except OverflowError:return math.copysign(math.inf,x)
count=0;accepted=0;rejected=0;pairs=0
for d in json.load(open(sys.argv[1],encoding='utf-8')):
    starts=[0]
    for c in d['counts']:starts.append(starts[-1]+c)
    boxes=[];whole=[]
    for ranges in d['groups']:
        points=[p for s,a,b in ranges for p in d['points'][starts[s]+a:starts[s]+b]]
        assert points
        boxes.append([min(f(p[0]) for p in points),min(f(p[1]) for p in points),max(f(p[0]) for p in points),max(f(p[1]) for p in points)])
        whole.append(all(a==0 for s,a,b in ranges))
    prefix=[];cached=[]
    for n,node in enumerate(d['nodes']):
        pref=[];cache=[]
        for arc in node:
            box=boxes[arc['group']];parent=n-arc['span']-1
            if parent<0:prior=box[2];cost=0x4000000
            else:
                prior=max((prefix[parent][p] for p in arc['pred']),default=box[2]);x=box[2] if d['maximum'] else box[0]
                transformed=f(f(f(f(x-prior)/f(d['height']))-f(d['threshold']))*-f(d['slope']))
                log=0 if transformed>4 else transformed if transformed<-4 else f(d['table'][int(float(f(transformed+4))*255+.5)>>3])
                val=f(-log*8192);cost=0x4000000 if val>=0x4000000 else math.floor(val+.5)
            assert cost==arc['blankCost'],(n,arc,cost)
            pref.append(max(box[2],prior));cache.append(cost);count+=1
        prefix.append(pref);cached.append(cache)
    for stage,n,a,p,symbol,cost,ok in d['calls']:
        arc=d['nodes'][n][a]
        if stage==118:
            possible=not d['maximum'] or whole[arc['group']] and (n+1==len(d['nodes']) or whole[d['nodes'][n+1][0]['group']])
            expected=cached[n][a] if possible else 0x4000000
            assert cost==expected and ok==(possible and expected<0x4000000)
            accepted+=ok;rejected+=not ok
        else:
            parent=n-arc['span']-1;current=boxes[arc['group']];previous=boxes[d['nodes'][parent][p]['group']]
            bottom=f(f(d['cp'][0])-f(d['pp'][0]));top=f(bottom-f(f(d['cp'][1])-f(d['pp'][1])));inv=f(1/f(d['height']))
            error=f(abs(f(f(f(current[1])-f(previous[1]))*inv)-top)+abs(f(f(f(current[3])-f(previous[3]))*inv)-bottom))
            v=f(min(f(f(error*f(d['pairScale']))-f(d['pairOffset'])),f(d['pairMaximum']))*8192)
            expected=math.trunc(f(v+(-.5 if v<=0 else .5)))
            assert cost==expected and ok==(expected<0x4000000);pairs+=1
assert accepted and rejected and pairs
print(f'PASS independent actual pruned blank geometry: {count} bbox/prefix/cache states,{accepted}/{rejected} accepted/rejected118,{pairs} pair140 values.')
