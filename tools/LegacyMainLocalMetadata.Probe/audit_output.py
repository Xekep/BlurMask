"""Independent mutable-prune, allocation-header/history and append oracle.
Input prepared cases are explicit arithmetic controls; actual cases retain raw
preprocessing/cut ownership. Neither is a recognition-quality measurement.
"""
import base64,copy,json,pathlib,struct,sys
j=json.loads(pathlib.Path(sys.argv[1]).read_bytes())
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def bits(x):return struct.unpack('<i',struct.pack('<f',x))[0]
def sort_history(rows):
    #Literal independent index quicksort, including swaps on equal first keys.
    a=copy.deepcopy(rows)
    def q(lo,hi):
        i,j=lo,hi;p=a[(lo+hi)//2][0][:2]
        while i<=j:
            while a[i][0][:2]<p:i+=1
            while a[j][0][:2]>p:j-=1
            if i<=j:a[i],a[j]=a[j],a[i];i+=1;j-=1
        if lo<j:q(lo,j)
        if i<hi:q(i,hi)
    if len(a)>1:q(0,len(a)-1)
    return a
def compose(rows):
    result=[]
    for r in rows:
        if result and result[-1][0]==r[0] and r[1]==result[-1][2]-1:result[-1][2]=r[2]
        else:result.append(r[:])
    return result
def rounded(x):
    v=struct.unpack('<I',struct.pack('<f',x))[0];m=v&0x7fffffff
    if m>=0x4b000000:return x
    if m<0x3f800000:v=(v&0x80000000)|(0x3f800000 if m>=0x3f000000 else 0)
    else:
        shift=150-(m>>23);v=((v>>shift)+((v>>(shift-1))&1))<<shift
    return struct.unpack('<f',struct.pack('<I',v))[0]
def separation(ranges,c):
    boxes=[]
    for stroke,start,end in ranges:
        pts=c['strokes'][stroke-c['offset']][start:end];assert pts
        boxes.append((min(p[0] for p in pts),max(p[0] for p in pts)))
    total=0
    for n in range(1,len(boxes)):
        a,b=boxes[:n],boxes[n:]
        gap=max(f(min(v[0] for v in b)-max(v[1] for v in a)),f(min(v[0] for v in a)-max(v[1] for v in b)))
        x=f(f(f(gap/f(c['height']))-f(c['gap']))*f(-c['slope']))
        log=0 if x>4 else j['table'][int(f(f(f(x+4)*255)+.5))>>3] if x>=-4 else x
        v=int(rounded(f(f(-log)*8192)));total=min(total+v,0x4000000)
    return total
allocations=deleted=groups=deferred=cases=ties=0
def check(c):
    global allocations,deleted,groups,deferred,cases
    seeds=c['seeds'];offset=c['offset'];pending=c['pending'];descriptor=c['descriptor'];n=len(seeds)
    assert sorted(s['sourceAtom'] for s in seeds)==list(range(n))
    expected=[]
    for s in seeds:
        r=s['range'];pts=c['strokes'][r[0]][r[1]:r[2]]
        box=[min(p[0] for p in pts),min(p[1] for p in pts),max(p[0] for p in pts),max(p[1] for p in pts)]
        assert [bits(v) for v in s['bounds']]==[bits(v) for v in box]
        r=[r[0]+offset,*r[1:]];aux=0 if pending and s['sourceAtom']==0 else 65535
        expected.append(dict(fields=[descriptor,65535,aux,0,1,0,0,0],boundsBits=[bits(v) for v in box],ranges=[r],history=[[r[:]]]))
        deferred+=s['deferred']
    for gid,left,right in c['mergedGroups']:
        assert gid==len(expected) and left<gid and right<n
        a,b=expected[left],expected[right];h=sort_history(a['history']+b['history']);ranges=compose([r for v in h for r in v])
        ba=[struct.unpack('<f',struct.pack('<i',v))[0] for v in a['boundsBits']];bb=[struct.unpack('<f',struct.pack('<i',v))[0] for v in b['boundsBits']]
        box=[min(ba[0],bb[0]),min(ba[1],bb[1]),max(ba[2],bb[2]),max(ba[3],bb[3])]
        expected.append(dict(fields=[descriptor,65535,a['fields'][2],0,0,0,0,0],boundsBits=[bits(v) for v in box],ranges=ranges,history=h))
    arc_meta={}
    for kind,node,ordinal,gid,ln,lo in c['allocations']:
        assert c['before'][node][ordinal][0]==gid
        if kind==0:
            assert gid<n and ln==-1 and lo==-1
            placement=node-gid if seeds[gid]['deferred'] else 0
            assert placement in (0,-1,-2)
            arc_meta[node,ordinal]=(placement,expected[gid]['fields'][2])
        else:
            assert gid>=n and (ln,lo) in arc_meta
            arc_meta[node,ordinal]=(0,arc_meta[ln,lo][1])
            assert c['before'][node][ordinal][1]==c['before'][ln][lo][1]+1
        expected[gid]['fields'][3]=min(200,expected[gid]['fields'][3]+1);allocations+=1
    assert len(arc_meta)==sum(map(len,c['before']))
    graph=copy.deepcopy(c['before']);labels=[list(range(len(row))) for row in graph];removals=[]
    costs=[separation(g['ranges'],c) for g in expected]
    for node in range(len(graph)-1):
        for ordinal in range(len(graph[node])-1,-1,-1):
            arc=graph[node][ordinal]
            if arc[1]==0 or costs[arc[0]]<=c['threshold']:continue
            gid=arc[0];removals.append([node,labels[node][ordinal],gid]);counter=expected[gid]['fields'][3]
            if counter<200:assert counter>0;expected[gid]['fields'][3]-=1
            for nn,row in enumerate(graph):
                for a in row:
                    if nn-a[1]-1==node:a[5]=[p-(p>ordinal) for p in a[5] if p!=ordinal]
                    a[6]=[[d,p-(p>ordinal)] if nn+d==node else [d,p] for d,p in a[6] if nn+d!=node or p!=ordinal]
            del graph[node][ordinal];del labels[node][ordinal]
    assert graph==c['after'] and labels==c['originalOrdinals'] and removals==c['removed']
    state=c['state'];assert state['groups']==expected
    ctx=[] if not pending else [dict(prefix=base64.b64encode(bytes([1])+bytes(15)).decode(),controls=[-7]*pending)]
    assert state['contexts']==ctx
    root_next=[[] for _ in range(state['cap'])];out=[]
    for node,row in enumerate(graph):
        out.append([])
        for ordinal,a in enumerate(row):
            placement,aux=arc_meta[node,labels[node][ordinal]];vectors=[[] for _ in root_next]
            for d,p in a[6]:vectors[d-1].append(p)
            out[-1].append(dict(header=[a[0],a[1],placement,a[2],a[3],a[4],aux,0,0],previous=a[5],next=vectors))
            if node==a[1] and -1 in a[5]:root_next[a[1]].append(ordinal)
    assert state['nodes']==out
    assert state['root']==dict(header=[0,0,0,4,0,0,65535,0,0],previous=[],next=root_next)
    deleted+=len(removals);groups+=len(expected);cases+=1

for c in j['cases']:check(c)
for actual in j['actual']:
    #Independent append-stage projection over the verified richer local state.
    cap=8;global_state=dict(cap=cap,groups=[],contexts=[],nodes=[],root=dict(header=[0,0,0,4,0,0,65535,0,0],previous=[],next=[[] for _ in range(cap)]))
    for c in actual['steps']:
        check(c);local=c['state'];old=len(global_state['nodes']);context=len(global_state['contexts']);base=len(global_state['groups']);mapping={}
        for i,g in enumerate(local['groups']):
            if g['fields'][3]==0:continue
            mapping[i]=len(global_state['groups']);out=copy.deepcopy(g)
            if out['fields'][2]!=65535:out['fields'][2]+=context
            retain=any((n<cap or n==len(local['nodes'])-1) and any(a['header'][0]==i for a in row) for n,row in enumerate(local['nodes']))
            if retain:out['fields'][4]=1
            else:out['history']=[]
            global_state['groups'].append(out)
        global_state['contexts']+=copy.deepcopy(local['contexts'])
        for row in local['nodes']:
            row=copy.deepcopy(row)
            for a in row:
                h=a['header'];h[0]=mapping[h[0]];h[4]=(h[4]+((old&0xffffff)<<7))&0xffffffff;h[5]=(h[5]+((old&0xffffff)<<7))&0xffffffff
                if h[6]!=65535:h[6]+=context
            global_state['nodes'].append(row)
        if not old:global_state['root']=copy.deepcopy(local['root'])
        else:
            terminal=len(global_state['nodes'][old-1])
            for selector in range(min(cap,len(local['nodes']))):
                roots=[i for i,a in enumerate(local['nodes'][selector]) if a['previous']==[-1]]
                for i in roots:global_state['nodes'][old+selector][i]['previous']=list(range(terminal))
                for i in range(terminal):global_state['nodes'][old-1][i]['next'][selector]+=roots
    assert global_state==actual['appended']
for c in j['historySort']:
    assert sort_history(c['input'])==c['output']
    ties+=len(c['input'])-len({tuple(row[0][:2]) for row in c['input']})
for c in j['referenceLifecycle']:
    counter=0
    for _ in range(c['allocated']):
        if counter<200:counter+=1
    for _ in range(c['removed']):
        if counter<200:counter-=1
    assert counter==c['counter'] and c['retained']==c['allocated']-c['removed']
assert j['prepared']==96 and j['actualCount']==5 and j['removed']==deleted and j['allocations']==allocations and j['deferred']==deferred
assert deleted>0 and deferred>0 and ties>0
print(f'PASS independent {cases} local states/{groups} group histories/{allocations} allocations/{deleted} mutable removals/{deferred} deferred seeds,12 prepared199/200/201 lifecycles,64 tie-sensitive sorts and5 actual append owners')
