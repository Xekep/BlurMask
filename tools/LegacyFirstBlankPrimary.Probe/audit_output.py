"""Independent first proposal/fallback, binary32 geometry and row recurrence."""
import json,math,struct,sys
INF=67108864
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def logcost(x,table,nearest=True):
    x=f(x);v=0 if x>4 else x if x<-4 else f(table[int(float(f(x+4))*255+.5)>>3]);v=f(-v*8192)
    if nearest:return math.floor(v+.5)
    return math.trunc(f(v+(-.5 if v<=0 else .5)))
def decode(b,i):
    p=i*40;packed=struct.unpack_from('<H',b,p+14)[0]
    return [struct.unpack_from('<i',b,p)[0],struct.unpack_from('<i',b,p+4)[0],b[p+9],struct.unpack_from('<H',b,p+16)[0],(packed>>1)&7,struct.unpack_from('<H',b,p+12)[0],bool(packed&1),(packed>>4)&63,b[p+8]]
callbacks=states=proposals=records=fallbacks=0
for d in json.load(open(sys.argv[1],encoding='utf-8')):
    arcs={};pred={};roots=dict((r[0],r[1:]) for r in d['root']);first=dict(d['firstIds']);geom=dict(d['geometryIds']);start=dict(d['start']);end=dict(d['end'])
    for n,a,span,g in d['arcs']:arcs.setdefault(n,[]).append((span,g))
    for n,a,*previous in d['pred']:pred[n,a]=previous
    starts=[0]
    for c in d['counts'][0]:starts.append(starts[-1]+c)
    boxes=[];whole=[]
    for group in d['groups']:
        pts=[p for s,a,b in group['ranges'] for p in d['points'][starts[s]+a:starts[s]+b]]
        boxes.append([min(f(p[0]) for p in pts),min(f(p[1]) for p in pts),max(f(p[0]) for p in pts),max(f(p[1]) for p in pts)])
        whole.append(all(a==0 for s,a,b in group['ranges']))
    future={};prefix={};join={};blank={};matrix={}
    for n in reversed(arcs):
        for a,(span,g) in enumerate(arcs[n]):
            low=boxes[g][0]
            if n!=span and n+1<len(arcs):
                for b,(s,_) in enumerate(arcs[n+1]):
                    if s==0 and a in pred[n+1,b]:low=min(low,future[n+1,b])
            future[n,a]=low
    for n,node in arcs.items():
        for a,(span,g) in enumerate(node):
            if n==span:prefix[n,a]=boxes[g][2];continue
            parent=n-span-1;prior=max((prefix[parent,p] for p in pred[n,a]),default=boxes[g][2]);prefix[n,a]=max(boxes[g][2],prior)
            gap=f(f(future[n,a]-prior)/f(d['height']));join[n,a]=min(logcost(f(f(gap-f(d['gap']))*-f(d['slope'])),d['table']),d['joinCap'])
            x=boxes[g][2] if d['useMaxX'] else boxes[g][0];raw=f(f(f(f(x-prior)/f(d['height']))-f(d['blankThreshold']))*-f(d['slope']))
            blank[n,a]=min(logcost(raw,d['table']),INF)
            for i,t in enumerate(d['matrix']):
                value=logcost(f(f(gap-f(t))*f(d['slope'])),d['table'],False)
                matrix[n,a,i]=value-1 if i//6==5 else value if i%6==5 else min(value,d['matrixCap'])
            states+=1
    lookup={}
    for ci,call in enumerate(d['calls']+d['geometryControls']):
        stage,n,a,s,c,span,kind,on,oa,oarc,os,gn,ga,garc,gs,gc,skipped,incoming,cost,ok=call
        assert on==n-span-1 and oarc in pred[n,a] and gn<=on
        if kind==6:expected=incoming;accepted=True
        elif stage==140:
            if s==189 or gs==189:delta=0
            else:
                cp=d['prototypes'][geom[c]];pp=d['prototypes'][geom[gc]];current=boxes[arcs[n][a][1]];previous=boxes[arcs[gn][garc][1]];inv=f(1/f(d['height']))
                #Native ordinary140 keeps source category, unions distinct original ending(kind0).
                if (on,oarc)!=(gn,garc):
                    ending=boxes[arcs[on][oarc][1]];previous=[min(previous[0],ending[0]),min(previous[1],ending[1]),max(previous[2],ending[2]),max(previous[3],ending[3])]
                bottom=f(f(cp[0])-f(pp[0]));top=f(bottom-f(f(cp[1])-f(pp[1])))
                error=f(abs(f(f(f(current[1])-f(previous[1]))*inv)-top)+abs(f(f(f(current[3])-f(previous[3]))*inv)-bottom))
                value=f(min(f(f(error*f(d['pairScale']))-f(d['pairOffset'])),f(d['pairMaximum']))*8192)
                delta=math.trunc(f(value+(-.5 if value<=0 else .5)))
            expected=incoming+delta;accepted=expected<INF
        elif kind==5:
            possible=not d['useMaxX'] or whole[arcs[n][a][1]] and (n+1==len(arcs) or whole[arcs[n+1][0][1]])
            expected=incoming+blank[n,a] if possible else INF;accepted=possible and expected<INF
        elif kind==0 and (s==189)!=(gs==189):expected=INF;accepted=False
        elif kind==2 and not whole[arcs[n][a][1]]:expected=INF;accepted=False
        elif kind==2 and (s==189)!=(gs==189):expected=incoming;accepted=True
        else:
            delta=join[n,a] if kind==0 else matrix[n,a,first[c]+first[gc]*6]
            current=boxes[arcs[n][a][1]];previous=boxes[arcs[gn][garc][1]]
            displacement=f(max(f(current[0]-previous[0]),f(current[2]-previous[2]))/f(d['height']))
            value=f(f(displacement-f(d['translation']))*f(d['slope'])) if (displacement<=d['translation'] if d['slope']>=0 else displacement>=d['translation']) else 0
            expected=incoming+delta-math.trunc(value)*8192;accepted=expected<INF
        assert (cost,bool(ok))==(expected,accepted),(call,expected,accepted)
        if ci<len(d['calls']):
            key=tuple(call[:17]);value=(incoming,cost,bool(ok));assert key not in lookup or lookup[key]==value;lookup[key]=value
        callbacks+=1
    #Native ascending-span/arc enumeration, top20+start/end, then blank fallback.
    expected=[]
    for n,node in arcs.items():
        for a,(span,g) in sorted(enumerate(node),key=lambda x:(x[1][0],x[0])):
            group=d['groups'][g];containsSpace=any(e[0]==180 for e in group['top'])
            for symbol,cost,likelihood in group['top']:
                if symbol in (180,182) or symbol==181 and containsSpace or d['protocolExcluded'] and symbol in (164,187,188) or n==span and symbol==191:continue
                for category in (start.get(symbol,-1),end.get(symbol,-1)):
                    if category<0:continue
                    extra=0 if symbol in (191,164,187,188,180,181,182) else group['geometry']
                    expected.append([n,a,span,symbol,category,cost+extra,likelihood])
            if span<n:
                cost=group['fallback'];cost=cost if cost>=INF else cost+group['geometry'];expected.append([n,a,span,192,start[192],cost,1]);fallbacks+=1
    assert expected==d['requests'],'Proposal/fallback/order differs'
    rows=[[] for _ in arcs];byNode=[[] for _ in arcs]
    for r in expected:byNode[r[0]].append(r[1:]);proposals+=1
    for n,rs in enumerate(byNode):
        for arc,span,symbol,category,groupCost,likelihood in rs:
            if groupCost>=INF:continue
            parent=n-span-1
            if parent==-1:
                if arc not in roots[span] or symbol==191:continue
                result=[groupCost,groupCost,255,arc,span,symbol,True,category,likelihood]
            else:
                best=INF;chosen=None
                for a,p in enumerate(rows[parent]):
                    if d['excludeConsecutive'] and symbol==192 and p[5]==192 or p[3] not in pred[n,arc]:continue
                    gn=parent;ga=a;g=p;count=0
                    while g[5] in (192,180,181,182):
                        pn=gn-g[4]-1
                        if pn==-1:break
                        ga=g[2];gn=pn;g=rows[gn][ga];count+=1
                    def correction(kind):
                        head=[n,arc,symbol,category,span,kind,parent,a,p[3],p[5],gn,ga,g[3],g[5],g[7],count]
                        v=lookup[tuple([118]+head)];assert v[0]==0
                        if not v[2]:return INF
                        v2=lookup[tuple([140]+head)];assert v2[0]==v[1];return v2[1] if v2[2] else INF
                    firstCost=INF;secondCost=INF
                    if symbol not in (180,181,182) and p[5] not in (180,181,182) and (symbol==189)==(p[5]==189):firstCost=correction(5 if symbol==192 else 6 if symbol==191 else 0)
                    if symbol!=192:secondCost=correction(2)
                    delta=min(firstCost,secondCost)
                    if delta>=INF or p[0]+delta>=INF:continue
                    value=p[0]+delta+groupCost
                    if value<best:best=value;chosen=[value,p[1]+groupCost,a,arc,span,symbol,secondCost<firstCost,category,likelihood]
                if chosen is None:continue
                result=chosen
            row=rows[n];i=len(row)
            while i and row[i-1][0]>result[0]:i-=1
            if i<d['beam']:row.insert(i,result);del row[d['beam']:]
        stored=bytes.fromhex(d['primary'][n]);actual=[decode(stored,i) for i in range(d['beam']) if struct.unpack_from('<i',stored,i*40)[0]<INF]
        assert rows[n]==actual,(n,rows[n],actual);records+=len(actual)
assert callbacks and proposals and fallbacks
print(f'PASS independent actual first blank-primary: {proposals} proposals/{fallbacks} realfallbacks,{states} geometrycache states,{callbacks} sampler values,{records} primaryrecords; original/ancestor ownership and allfields audited.')
