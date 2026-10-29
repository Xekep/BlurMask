"""Independent B0/B8 packed-row/column walks and symbolic tuple oracle."""
import json,struct,sys
INF=67108864
def insert(row,e,cap):
    for i in range(len(row)-1,-1,-1):
        if row[i]['seq']==e['seq']:
            if row[i]['total']>e['total']:row[i]=e
            return
    i=len(row)
    while i and row[i-1]['total']>e['total']:i-=1
    if i<cap:row.insert(i,e);del row[cap:]
counts=[0,0];records=[0,0];calls=[0,0];skips=[0,0];blankparents=blankshares=0
for c in json.load(open(sys.argv[1],encoding='utf-8')):
    c=dict(c,synthetic=False,calls=c['contextCalls'])
    typ=int(c['synthetic']);counts[typ]+=1;nodes={n['identity']:n for n in c['contexts']}
    def seq(identity):
        r=[]
        while identity:
            n=nodes[identity];r.append(n['symbol']);identity=n['parent']
        return r[::-1]
    def decode(h):
        b=bytes.fromhex(h);out=[]
        for p in range(0,len(b),40):
            total,acoustic=struct.unpack_from('<ii',b,p);s,packed,a=struct.unpack_from('<HHH',b,p+12);identity=struct.unpack_from('<Q',b,p+24)[0]
            out.append(dict(total=total,acoustic=acoustic,symbol=s,span=(packed>>1)&7,category=(packed>>4)&63,arc=a,previous=b[p+9],column=b[p+10],path=bool(packed&1),likelihood=b[p+8],seq=seq(identity) if identity else []))
        return out
    primary=[[r for r in decode(h) if r['total']<INF] for h in c['primary']]
    costs={tuple(r[:4]):r[4] for r in c['costs']};successors={tuple(r[:3]):r[3:] for r in c['successors']};lookup={}
    for r in c['calls']:
        key=tuple(r[:22]);v=(r[22],bool(r[23]));assert key not in lookup or lookup[key]==v;lookup[key]=v
    calls[typ]+=len(c['calls']);b0=[];b8=[]
    for n,current in enumerate(primary):
        for final,tables,cap,captured in [(0,b0,c['cap0'],c['b0']),(1,b8,c['cap8'],c['b8'])]:
            rows=[[] for _ in range(c['beam'])]
            for ri,g in enumerate(current):
                parent=n-g['span']-1
                if parent==-1:rows[ri]=[dict(g,seq=[g['symbol']])];continue
                gc=costs[n,g['arc'],g['symbol'],g['category']]
                if gc>=INF:continue
                for pr,originals in enumerate(tables[parent]):
                    if g['symbol']==c['blank'] and pr!=g['previous']:blankparents+=1;continue
                    for pa,p in enumerate(originals):
                        if c['excludeConsecutive'] and g['symbol']==c['blank'] and p['symbol']==c['blank']:continue
                        an=parent;ar=pr;aa=pa;anc=p;skipped=0
                        while anc['symbol']==c['blank'] or anc['symbol']==65535 or not c['modeE0'] and anc['symbol'] in (180,181,182):
                            pn=an-anc['span']-1
                            if pn==-1:break
                            ar=anc['previous'];aa=anc['column'];an=pn;anc=tables[an][ar][aa];skipped+=1
                        if g['arc'] not in successors[parent,p['arc'],g['span']]:continue
                        kinds=([0,2] if not c['modeE0'] else [2 if g['path'] else 0]) if final else [2 if g['path'] else 0]
                        for kind in kinds:
                            if kind==2 and g['symbol'] in (c['blank'],65535):continue
                            rk=5 if kind==0 and g['symbol']==c['blank'] else 6 if kind==0 and g['symbol']==c['closed'] else kind
                            head=[final,n,ri,g['arc'],g['symbol'],g['category'],g['span'],rk,parent,pr,pa,p['arc'],p['symbol'],an,ar,aa,anc['arc'],anc['symbol'],anc['category'],skipped]
                            v,ok=lookup[tuple([140]+head+[p['total']+gc])]
                            if not ok:continue
                            v,ok=lookup[tuple([118]+head+[v])]
                            if not ok or v>=INF:continue
                            skips[typ]+=skipped
                            if kind==0 and (g['symbol']==c['blank'] or final and g['symbol']==c['closed']):s=p['seq'];blankshares+=g['symbol']==c['blank']
                            elif not final and kind==2:s=[g['symbol']]
                            else:s=p['seq']+([65531] if final and kind==2 and not c['modeE0'] else [])+[g['symbol']]
                            insert(rows[ri],dict(g,total=v,acoustic=p['acoustic']+gc,previous=pr,column=pa,path=kind==2,likelihood=0,seq=s),cap)
            tables.append(rows);actual=decode(captured[n])
            for ri,expected in enumerate(rows):
                active=[r for r in actual[ri*cap:(ri+1)*cap] if r['total']<INF]
                assert expected==active,(typ,final,n,ri,expected,active)
                records[typ]+=len(active)
assert counts[0]==4 and counts[1]==0 and records[0]>0 and calls[0]>0
print(f'PASS actualOWNED4 context fullscalar/symbolic replay: {records[0]}records/{calls[0]}actualgeometry callback inputs; no synthetic context coverage claim')
