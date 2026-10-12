"""Vendor-free scalar geometry oracle. No native library/model execution."""
import json, math, random, struct, sys
from pathlib import Path

def f(x): return struct.unpack('<f',struct.pack('<f',x))[0]
def pair(a,b,pa,pb,height,scale,offset,cap):
    bottom=f(pa[0]-pb[0]); top=f(bottom-f(pa[1]-pb[1])); inverse=f(1/height)
    residual=f(abs(f(f(f(a[1]-b[1])*inverse)-top))+abs(f(f(f(a[3]-b[3])*inverse)-bottom)))
    v=f(min(f(f(residual*scale)-offset),cap)*8192)
    return math.trunc(f(v+.5 if v>0 else v-.5))

def logcost(x,table):
    if x>4: v=0
    elif x<-4: v=x
    else: v=table[math.trunc(float(f(x+4))*255+.5)//8]
    return math.floor(f(-v*8192)+.5)

def audit(path):
    """Scalar fold of captured actual graph/callback inputs, independent of C#."""
    fixtures=json.loads(Path(path).read_text(encoding='utf-8'));states=0;calls=0
    for c in fixtures:
        graph=c['graph']; height=f(c['height']); profile=list(map(f,c['profile']));table=list(map(f,c['table']));matrix=list(map(f,c['matrix']))
        future=[[None]*len(row) for row in graph];prefix=[[None]*len(row) for row in graph]
        join=[[None]*len(row) for row in graph];matrixcost=[[None]*len(row) for row in graph]
        for node in range(len(graph)-1,-1,-1):
            for ordinal,a in enumerate(graph[node]):
                value=f(a['box'][0])
                if node!=a['span']:
                    for succ in a['succ']:value=min(value,future[node+1][succ])
                future[node][ordinal]=value
        for node,row in enumerate(graph):
            for ordinal,a in enumerate(row):
                box=list(map(f,a['box']))
                if node==a['span']: prefix[node][ordinal]=box[2];join[node][ordinal]=67108864;matrixcost[node][ordinal]=[67108864]*36
                else:
                    previous=node-a['span']-1
                    p=max([prefix[previous][v] for v in a['pred']],default=box[2])
                    prefix[node][ordinal]=max(p,box[2]);gap=f(f(future[node][ordinal]-p)/height)
                    join[node][ordinal]=min(logcost(f(f(gap-profile[3])*-profile[5]),table),67108864)
                    matrixcost[node][ordinal]=[logcost(f(f(gap-v)*profile[5]),table)-(1 if i//6==5 else 0) for i,v in enumerate(matrix)]
                expected=[future[node][ordinal],prefix[node][ordinal],join[node][ordinal]]
                recorded=[f(a['state'][0]),f(a['state'][1]),a['state'][2]]
                if expected!=recorded:raise AssertionError(('actual graph fold',node,ordinal,expected,recorded))
                states+=1
        for stage,n,arc,cat,pn,parc,pcat,kind,incoming,actual,accepted in c['calls']:
            a=list(map(f,graph[n][arc]['box']));b=list(map(f,graph[pn][parc]['box']))
            if stage==118 and kind==2 and not graph[n][arc]['wholeStarts']:
                cost=67108864
            elif stage==118:
                base=join[n][arc] if kind==0 else matrixcost[n][arc][c['categories'][cat][0]+6*c['categories'][pcat][0]]
                delta=f(max(f(a[0]-b[0]),f(a[2]-b[2]))/height)
                term=f(f(delta-profile[4])*profile[5]) if (delta<=profile[4] if profile[5]>=0 else delta>=profile[4]) else 0
                cost=incoming+base-math.trunc(term)*8192
            else:
                pa=list(map(f,c['prototypes'][c['categories'][cat][1]]));pb=list(map(f,c['prototypes'][c['categories'][pcat][1]]))
                cost=incoming+pair(a,b,pa,pb,height,*profile[:3])
            if cost!=actual or int(cost<67108864)!=accepted:raise AssertionError(('actual callback',stage,n,arc,cost,actual))
            calls+=1
    print(f'PASS independent actual graph/state/callback audit: {len(fixtures)} fixtures/{states} arc states/{calls} callback values.')

if len(sys.argv)==3 and sys.argv[1]=='--audit':
    audit(sys.argv[2]);sys.exit()

r=random.Random(195530)
table=[f(-math.log1p(math.exp(-(-4+8*i/255)))) for i in range(256)]
cases=[]
for i in range(1024):
    a=[f(r.uniform(-200,200)) for _ in range(4)]
    b=[f(r.uniform(-200,200)) for _ in range(4)]
    a=[min(a[0],a[2]),min(a[1],a[3]),max(a[0],a[2]),max(a[1],a[3])]
    b=[min(b[0],b[2]),min(b[1],b[3]),max(b[0],b[2]),max(b[1],b[3])]
    pa=[f(r.uniform(-2,2)),f(r.uniform(0,3))];pb=[f(r.uniform(-2,2)),f(r.uniform(0,3))]
    if i%8==0: a=b[:];pa=pb[:]
    h=f(r.uniform(1,300));s=f(r.uniform(0,3));o=f(r.uniform(-1,2));cap=f(r.uniform(.1,4))
    cases.append(dict(a=a,b=b,pa=pa,pb=pb,height=h,scale=s,offset=o,cap=cap,cost=pair(a,b,pa,pb,h,s,o,cap)))
logs=[]
xs=[-5,-4,-3.9999998,0,3.9999998,4,4.0000005,5]+[f(r.uniform(-8,8)) for _ in range(1016)]
for x in xs:
    x=f(x);v=x
    if x>4:v=0
    elif x>=-4:v=table[math.trunc(float(f(x+4))*255+.5)//8]
    cost=f(-v*8192)
    logs.append(dict(x=x,cost=math.floor(cost+.5)))
path=Path(sys.argv[1]);path.parent.mkdir(parents=True,exist_ok=True)
path.write_text(json.dumps(dict(table=table,pairs=cases,logs=logs),separators=(',',':')),encoding='utf-8')
print('Generated1024 pair residual/negative offset cases and1024 table/tail/edge cases.')
