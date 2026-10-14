"""Independent primary prefix + B0/B8 symbolic tuple oracle. No vendor assets."""
from pathlib import Path
import json,random,sys,struct
r=random.Random(0x1ef071);INF=67108864
def cost(g):return ((g['node']+1)*3+(g['arc']+1)*5+g['symbol']*7+g['category']*11)%91-20
def c118(g,p,kind,value):
 if (g['symbol']+p['symbol']+kind+g['node'])%11==0:return None
 return value+(g['symbol']*13+p['symbol']*7+kind*5+g['node'])%17-8
def c140(g,p,kind,value):return value+(g['node']+p['arc']*3+g['arc']*5+kind*2)%11-5
def insert(rows,entry,cap):
 for i in range(len(rows)-1,-1,-1):
  if rows[i]['sequence']==entry['sequence']:
   if rows[i]['total']>entry['total']:rows[i]=entry
   return
 index=len(rows)
 while index>0 and rows[index-1]['total']>entry['total']:index-=1
 if index<cap:rows.insert(index,entry);del rows[cap:]

def audit(path):
 c=json.loads(Path(path).read_text(encoding='utf-8'));width=c['beamWidth'];cap=c['columnCount'];contexts={n['identity']:n for n in c['contexts']}
 def sequence(identity):
  result=[]
  while identity:
   n=contexts[identity];result.append(n['symbol']);identity=n['parent']
  return result[::-1]
 def decode(hexbytes):
  raw=bytes.fromhex(hexbytes);rows=[]
  for offset in range(0,len(raw),40):
   total,acoustic=struct.unpack_from('<ii',raw,offset);symbol,packed,arc=struct.unpack_from('<HHH',raw,offset+12)
   identity=struct.unpack_from('<Q',raw,offset+24)[0]
   rows.append(dict(total=total,acoustic=acoustic,symbol=symbol,span=(packed>>1)&7,category=(packed>>4)&63,arc=arc,
    previous=raw[offset+9],column=raw[offset+10],pathbit=bool(packed&1),likelihood=raw[offset+8],sequence=sequence(identity) if identity else []))
  return rows
 primary=[]
 for node,h in enumerate(c['primary']):
  row=[]
  for entry in decode(h):
   if entry['total']>=INF:break
   entry['node']=node;row.append(entry)
  primary.append(row)
 costs={tuple(v[:4]):v[4] for v in c['costs']};successors={tuple(v[:3]):v[3:] for v in c['successors']};calls={}
 for v in c['calls']:
  key=tuple(v[:11]);result=(v[11],bool(v[12]))
  if key in calls and calls[key]!=result:raise AssertionError('same prepared native callback input changed outcome')
  calls[key]=result
 def correction(stage,g,p,kind,value):
  return calls[(stage,g['node'],g['arc'],g['symbol'],g['category'],p['node'],p['arc'],p['symbol'],p['category'],kind,value)]
 b0=[];b8=[];tested=0
 for node,current in enumerate(primary):
  for final,tables,captured in [(False,b0,c['b0']),(True,b8,c['nodes'])]:
   rows=[[] for _ in range(width)]
   for ordinal,g in enumerate(current):
    pn=node-g['span']-1
    if pn==-1:rows[ordinal]=[dict(g,sequence=[g['symbol']])];continue
    for prior,pr in enumerate(tables[pn]):
     for alternative,p in enumerate(pr):
      if g['arc'] not in successors[(pn,p['arc'],g['span'])]:continue
      kinds=([0,2] if not c['modeE0'] else [2 if g['pathbit'] else 0]) if final else [2 if g['pathbit'] else 0]
      for kind in kinds:
       gc=costs[(node,g['arc'],g['symbol'],g['category'])]
       v,accepted=correction(140,g,p,kind,p['total']+gc)
       if not accepted:continue
       v,accepted=correction(118,g,p,kind,v)
       if not accepted:continue
       seq=([g['symbol']] if kind==2 else p['sequence']+[g['symbol']]) if not final else p['sequence']+([65531] if kind==2 and not c['modeE0'] else [])+[g['symbol']]
       insert(rows[ordinal],dict(g,total=v,acoustic=p['acoustic']+gc,previous=prior,column=alternative,pathbit=kind==2,sequence=seq,likelihood=0),cap)
   tables.append(rows);actual=decode(captured[node])
   for row,expected in enumerate(rows):
    active=[]
    for a in actual[row*cap:(row+1)*cap]:
     if a['total']>=INF:break
     active.append(a)
    normalized=[{k:v for k,v in e.items() if k!='node'} for e in expected]
    if normalized!=active:raise AssertionError(('actual context graph',final,node,row,normalized,active))
    tested+=len(active)
 print(f'PASS independent actual primary/callback inputs -> B0/B8 symbolic and scalar audit: explicitE0={c["modeE0"]}, {tested} finite records/{len(calls)} distinct callback requests.')

if len(sys.argv)==3 and sys.argv[1]=='--audit':audit(sys.argv[2]);sys.exit()
cases=[]
for _ in range(256):
 n=r.randint(1,9);atoms=r.randint(1,min(5,n));width=r.randint(1,5);cap0=r.randint(1,4);cap8=r.randint(1,4);e0=bool(r.randrange(2));requests=[];primary=[]
 for node in range(n):
  req=[]
  for span in range(min(atoms-1,node),-1,-1):
   for symbol in r.sample([0,1],r.randint(1,2)):
    for category in r.sample([0,1],r.randint(1,2)):
     req.append(dict(node=node,arc=span,span=span,symbol=symbol,category=category))
  requests.append(req);entries=[]
  for g in req:
   pn=node-g['span']-1;gc=cost(g)
   if pn==-1:entries.append(dict(**g,total=gc,acoustic=gc,previous=255,column=0,pathbit=True,likelihood=50+g['symbol']*70));continue
   best=None
   for i,p in enumerate(primary[pn]):
    a=c118(g,p,0,0);a=None if a is None else c140(g,p,0,a)
    b=c118(g,p,2,0);b=None if b is None else c140(g,p,2,b)
    a=INF if a is None else a;b=INF if b is None else b
    d=min(a,b)
    if d<INF:
     total=p['total']+d+gc
     if best is None or total<best['total']:best=dict(**g,total=total,acoustic=p['acoustic']+gc,previous=i,column=0,pathbit=b<a,likelihood=50+g['symbol']*70)
   if best:entries.append(best)
  primary.append(sorted(entries,key=lambda p:p['total'])[:width])
 b0=[];b8=[]
 for node,current in enumerate(primary):
  for final,tables,cap in [(False,b0,cap0),(True,b8,cap8)]:
   rows=[[] for _ in range(width)]
   for ordinal,g in enumerate(current):
    pn=node-g['span']-1
    if pn==-1:rows[ordinal]=[dict(g,sequence=[g['symbol']])];continue
    for prior,pr in enumerate(tables[pn]):
     for alternative,p in enumerate(pr):
      kinds=([0,2] if not e0 else [2 if g['pathbit'] else 0]) if final else [2 if g['pathbit'] else 0]
      for kind in kinds:
       v=c140(g,p,kind,p['total']+cost(g));v=c118(g,p,kind,v)
       if v is None:continue
       sequence=([g['symbol']] if kind==2 else p['sequence']+[g['symbol']]) if not final else p['sequence']+([65531] if kind==2 and not e0 else [])+[g['symbol']]
       entry=dict(g,total=v,acoustic=p['acoustic']+cost(g),previous=prior,column=alternative,pathbit=kind==2,sequence=sequence,likelihood=0)
       insert(rows[ordinal],entry,cap)
   tables.append(rows)
 cases.append(dict(count=n,atoms=atoms,width=width,cap0=cap0,cap8=cap8,e0=e0,requests=requests,b0=b0,b8=b8))
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(cases,separators=(',',':')),encoding='utf-8')
print('Generated256 independent first-primary/B0/B8 symbolic-context controls.')
