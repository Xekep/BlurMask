"""Independent regular proposal ordering and primary tuple dynamic program.
Uses actual observed geometry callbacks as inputs, not the C# recurrence.
Full blank/ancestor/protocol-owner producers are deliberately not invented.
"""
import json,struct,sys
from pathlib import Path
INF=0x4000000
cases=json.loads(Path(sys.argv[1]).read_text(encoding='utf-8'));total=0;rows_tested=0
for c in cases:
 arcs={};pred={(r[0],r[1]):r[2:] for r in c['pred']};roots={r[0]:r[1:] for r in c['root']}
 for n,a,span,group in c['arcs']:arcs.setdefault(n,[]).append((a,span,group))
 start=dict(c['start']);end=dict(c['end']);first=dict(c['firstIds']);expected=[];blanks=[]
 #Pinned native table/inlined first alphabet semantics: no shaping traits.
 #Headers EA/EC/EE=182/180/181; protocol lookups187/188/164/FFFF.
 for node,entries in sorted(arcs.items()):
  extent=max([s+1 for a,s,g in entries],default=0)
  for span in range(extent):
   for ordinal,s,group in entries:
    if s!=span:continue
    gs=c['groups'][group];have_space=any(e[0]==180 for e in gs['top'])
    for label,cost,likelihood in gs['top']:
     if label in (65535,182,180) or have_space and label==181 or c['protocolExcluded'] and label in (187,188,164):continue
     if node==span and label==c['closed']:continue
     for mapping in (start,end):
      category=mapping[label]
      if category<0:continue
      assert first[category]<=5
      value=cost+(0 if label in (191,187,188,164,182,180,181) else gs['geometry'])
      expected.append([node,ordinal,span,label,category,value,likelihood])
    if span<node:blanks.append([node,ordinal,span])
 assert expected==c['requests'],('native regular request order/category/scalar',expected,c['requests'])
 assert blanks==c['blanks'],('blank dispatch reporting',blanks,c['blanks'])
 total+=len(expected)
 calls={}
 for call in c['calls']:
  key=tuple(call[:11]);value=(call[11],bool(call[12]))
  if key in calls:assert calls[key]==value
  calls[key]=value
  stage,n,a,s,cat,pn,pa,ps,pcat,kind,incoming,result,ok=call
  if kind==0 and (s==189)!=(ps==189):assert not ok and result==INF
  elif kind==0 and s==c['closed']:assert ok and result==incoming
 bynode={n:[] for n in arcs}
 #Own tuple model, stable signed-cost insertion and root/noE4 special case.
 source={n:[] for n in arcs}
 for r in expected:source[r[0]].append(r)
 def correction(stage,g,p,kind,value):
  k=(stage,g['node'],g['arc'],g['symbol'],g['category'],p['node'],p['arc'],p['symbol'],p['category'],kind,value)
  return calls[k]
 for node,requests in source.items():
  entries=[]
  for n,arc,span,symbol,category,gc,likelihood in requests:
   if gc>=INF:continue
   g=dict(node=n,arc=arc,span=span,symbol=symbol,category=category,likelihood=likelihood)
   pn=n-span-1
   if pn==-1:
    if arc in roots[span]:entries.append(dict(g,total=gc,acoustic=gc,previous=255,column=0,pathbit=True))
    continue
   best=None
   for i,p in enumerate(bynode[pn]):
    if p['arc'] not in pred[(node,arc)]:continue
    deltas=[]
    for kind in (0,2):
     value,ok=correction(118,g,p,kind,0)
     if ok:value,ok=correction(140,g,p,kind,value)
     deltas.append(value if ok else INF)
    delta=min(deltas)
    subtotal=p['total']+delta
    if delta>=INF or subtotal>=INF:continue
    result=dict(g,total=subtotal+gc,acoustic=p['acoustic']+gc,previous=i,column=0,pathbit=deltas[1]<deltas[0])
    if best is None or result['total']<best['total']:best=result
   if best is not None:entries.append(best)
  bynode[node]=sorted(entries,key=lambda g:g['total'])[:c['beam']]
  raw=bytes.fromhex(c['primary'][node]);observed=[]
  for p in range(0,len(raw),40):
   value,acoustic=struct.unpack_from('<ii',raw,p)
   if value>=INF:break
   symbol,packed,arc=struct.unpack_from('<HHH',raw,p+12)
   observed.append(dict(node=node,total=value,acoustic=acoustic,likelihood=raw[p+8],previous=raw[p+9],column=raw[p+10],
    symbol=symbol,span=(packed>>1)&7,category=(packed>>4)&63,arc=arc,pathbit=bool(packed&1)))
  assert bynode[node]==observed,('primary tuple recurrence',node,bynode[node],observed)
  rows_tested+=len(observed)
print(f'PASS independent native regular ordering + primary tuple recurrence: {len(cases)} actual captures/{total} requests/{rows_tested} records; source cap7, rootE4, current/priorE8, duplicate category/tie order, pending blanks.')
