from pathlib import Path
import json,struct,sys
INF=0x4000000

def decode(raw):
 t,a=struct.unpack_from('<ii',raw);s,k,arc=struct.unpack_from('<HHH',raw,12);return dict(total=t,acoustic=a,previous=raw[9],column=raw[10],arc=arc,span=(k>>1)&7,symbol=s,path=bool(k&1),category=(k>>4)&63,likelihood=raw[8],traits=raw[32]&31)
records=0
for d in json.loads(Path(sys.argv[1]).read_text()):
 succ={tuple(x[:3]):x[3:]for x in d['successors']};callbacks={}
 for x in d['primaryCalls']:
  key=tuple(x[:17]);v=tuple(x[17:]);assert key not in callbacks or callbacks[key]==v;callbacks[key]=v
 rows=[]
 for n,requests in enumerate(d['requests']):
  target=[]
  for _,arc,span,sym,cat,gcost,like in requests:
   if gcost>=INF:continue
   parent=n-span-1
   if parent==-1:
    if sym==d['closed'] or arc not in succ[-1,65535,span]:continue
    v=dict(total=gcost,acoustic=gcost,previous=255,column=0,arc=arc,span=span,symbol=sym,path=True,category=cat,likelihood=0,traits=0)
   else:
    best=INF;v=None
    for a,p in enumerate(rows[parent]):
     if d['excludeConsecutive'] and sym==d['blank'] and p['symbol']==d['blank']:continue
     if arc not in succ[parent,p['arc'],span]:continue
     gn,ga,g,skipped=parent,a,p,0
     while g['symbol']in(d['blank'],d['shadda'],d['newline'],d['space'],d['nbsp']):
      pn=gn-g['span']-1
      if pn==-1:break
      ga=g['previous'];gn=pn;g=rows[gn][ga];skipped+=1
     def correction(kind):
      h=[n,arc,sym,cat,span,kind,parent,a,p['arc'],p['symbol'],gn,ga,g['arc'],g['symbol'],g['category'],skipped]
      incoming,c,ok=callbacks[tuple([118]+h)];assert incoming==0
      if not ok:return INF
      inc,z,yes=callbacks[tuple([140]+h)];assert inc==c
      return z if yes else INF
     first=second=INF
     heads=(d['newline'],d['space'],d['nbsp'])
     if sym not in heads and p['symbol']not in heads and (sym==d['replacement'])==(p['symbol']==d['replacement']):first=correction(5 if sym==d['blank']else 6 if sym==d['closed']else 0)
     if sym!=d['blank'] and sym!=d['shadda']:second=correction(2)
     delta=min(first,second)
     if delta>=INF or p['total']+delta>=INF:continue
     value=p['total']+delta+gcost
     if value<best:best=value;v=dict(total=value,acoustic=p['acoustic']+gcost,previous=a,column=0,arc=arc,span=span,symbol=sym,path=second<first,category=cat,likelihood=0,traits=0)
    if v is None:continue
   i=len(target)
   while i and target[i-1]['total']>v['total']:i-=1
   if i<d['beam']:target.insert(i,v);del target[d['beam']:]
  for v in target:v['likelihood']=next(r[6]for r in requests if r[1]==v['arc']and r[3]==v['symbol'])
  actual=[decode(bytes.fromhex(d['primary'][n])[i*40:(i+1)*40])for i in range(d['beam'])];actual=[r for r in actual if r['total']<INF];assert target==actual,(d['fixture'],n,target,actual);records+=len(target);rows.append(target)
print('PASS independentOWNED primary exact finite scalar/ancestry/stable-tie recurrence',records,'records; actualsource sampler outputs are captured inputs')
