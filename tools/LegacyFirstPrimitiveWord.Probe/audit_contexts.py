"""Independent symbolic tuple/context oracle over captured inputs; no library imports.
Correction kind6 changes callbacks; B8 kind0 E4 keeps the prior context.
"""
from pathlib import Path
import sys,json,struct
INF=67108864
def insert(rows,entry,cap):
 for i in range(len(rows)-1,-1,-1):
  if rows[i]['sequence']==entry['sequence']:
   if rows[i]['total']>entry['total']:rows[i]=entry
   return
 index=len(rows)
 while index>0 and rows[index-1]['total']>entry['total']:index-=1
 if index<cap:rows.insert(index,entry);del rows[cap:]

def audit(c):
 width=c['beamWidth'];cap=c['columnCount'];contexts={n['identity']:n for n in c['contexts']}
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
       request_kind=6 if kind==0 and g['symbol']==c['closedSymbol'] else kind
       v,accepted=correction(140,g,p,request_kind,p['total']+gc)
       if not accepted:continue
       v,accepted=correction(118,g,p,request_kind,v)
       if not accepted:continue
       if final and kind==0 and g['symbol']==c['closedSymbol']:seq=p['sequence']
       else:seq=([g['symbol']] if kind==2 else p['sequence']+[g['symbol']]) if not final else p['sequence']+([65531] if kind==2 and not c['modeE0'] else [])+[g['symbol']]
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
 return tested,len(calls),sum(1 for k in calls if k[9]==6)


cases=json.loads(Path(sys.argv[1]).read_text(encoding='utf-8'))
results=[audit(c) for c in cases]
assert results and sum(v[2] for v in results)>0
print(f'PASS independent literal B0/B8 context oracle: {len(results)} captures/{sum(v[0] for v in results)} records/{sum(v[1] for v in results)} callbacks/{sum(v[2] for v in results)} kind6 requests; paths, contexts, signed costs and every finite scalar field.')
