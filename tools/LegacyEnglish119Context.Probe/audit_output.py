"""Independent raw108/class costs + A8/full B0/B8 byte/symbol/order replay.
Only stdlib. Actual rows and explicitly prepared numeric controls stay separate.
"""
from pathlib import Path
import base64,hashlib,json,math,runpy,struct,sys
if len(sys.argv)!=7:raise SystemExit('models candidates.json controller.json controls.json primary.json context.json')
root=Path(__file__).resolve().parents[2];args=sys.argv[:];context=json.loads(Path(args[6]).read_bytes());controls=json.loads(Path(args[4]).read_bytes())
sys.argv=[str(root/'tools/LegacyEnglish119Primary.Probe/audit_output.py'),*args[1:6]]
g=runpy.run_path(sys.argv[0]);sys.argv=args
primary=g['out'];controller=g['costs'];f=g['f'];INF=0x4000000;composites=g['composites']
assert(context['Path'],context['Fingerprint'])==(primary['Path'],primary['Fingerprint'])
def lookup(group,symbol):
 cost=group['Fallback']if symbol==217 else next((e['Cost']for e in group['Entries'][:20]if e['Symbol']==symbol),INF)
 return cost if cost>=INF or symbol in g['bypass']else cost+group['GeometryCost']
def raw(value):return bytearray(base64.b64decode(value))
def decode(record):
 b=raw(record['Bytes']);out=[]
 for i in range(record['Count']):
  p=i*40;total,acoustic=struct.unpack_from('<ii',b,p);symbol,packed,arc=struct.unpack_from('<HHH',b,p+12)
  out.append(dict(total=total,acoustic=acoustic,previous=b[p+9],column=b[p+10],arc=arc,span=(packed>>1)&7,symbol=symbol,path=bool(packed&1),category=(packed>>4)&63,likelihood=b[p+8],source=bytes(b[p:p+40])))
 return out

def replay(source,current,captured,preparedCosts=None):
 correct=g['environment'](source);nodes=source['Nodes'];rows=[decode(r)for r in current];trace=captured['Calls'];ti=0;made=set()
 ids={n['Identity']:n for n in captured['Contexts']};mapping={};sequences={}
 def seq(identity):
  if identity==0:return ()
  if identity not in sequences:
   n=ids[identity];sequences[identity]=seq(n['ParentLink'])+(n['Symbol'],)
  return sequences[identity]
 for identity in ids:
  sequence=seq(identity);assert sequence not in mapping;mapping[sequence]=identity
 def resolve(sequence):
  made.add(sequence);assert sequence in mapping,('symbol allocation',sequence);return mapping[sequence]
 tables=[[],[]];stores=[[],[]]
 groups={v['Id']:v for v in preparedCosts['Groups']}if isinstance(preparedCosts,dict)else None
 def glyph(n,r):
  if groups is None:return preparedCosts[n]
  return lookup(groups[nodes[n][r['arc']]['Group']],r['symbol'])
 def inspect(final,n,ri,r,kind,parent,pri,ai,o,an,ar,aa,anc,skipped,stage,incoming):
  nonlocal ti
  cost,ok=correct(stage,n,r['arc'],r['symbol'],r['category'],kind,an,anc['arc'],anc['symbol'],anc['category'],incoming)
  expected=[stage,final,n,ri,r['arc'],r['symbol'],r['category'],r['span'],kind,parent,pri,ai,o['arc'],o['symbol'],o['category'],o['span'],o['previous'],o['column'],o['total'],o['acoustic'],int(o['path']),an,ar,aa,anc['arc'],anc['symbol'],anc['category'],anc['span'],anc['previous'],anc['column'],anc['total'],anc['acoustic'],int(anc['path']),skipped,incoming,cost,int(ok)]
  assert ti<len(trace)and trace[ti]==expected,('context ordered callback',ti,trace[ti]if ti<len(trace)else None,expected);ti+=1
  return cost,ok
 for n,currentRows in enumerate(rows):
  for final in (0,1):
   target=[[]for _ in range(5)];storage=[bytearray(40)for _ in range(5)]
   for b in storage:struct.pack_into('<i',b,0,INF)
   for ri,r in enumerate(currentRows):
    parent=n-r['span']-1
    if parent==-1:
     identity=resolve((r['symbol'],));b=bytearray(r['source']);struct.pack_into('<Q',b,24,identity);storage[ri]=b;target[ri]=[dict(r,seq=(r['symbol'],))];continue
    cost=glyph(n,r)
    if cost>=INF:continue
    for pri in range(len(rows[parent])):
     if r['symbol']==217 and pri!=r['previous']:continue
     for ai,o in enumerate(tables[final][parent][pri]):
      an,ar,aa,anc,skipped=parent,pri,ai,o,0
      while anc['symbol']in(217,65535,184,182,183):
       previous=an-anc['span']-1
       if previous==-1:break
       aa=anc['column'];ar=anc['previous'];an=previous;anc=tables[final][an][ar][aa];skipped+=1
      if o['arc']not in nodes[n][r['arc']]['Predecessors']:continue
      kinds=(0,2)if final else (2,)if r['path']else(0,)
      for kind in kinds:
       if kind==2 and r['symbol']in(217,65535):continue
       rk=5 if kind==0 and r['symbol']==217 else 6 if kind==0 and r['symbol']==211 else kind
       value,ok=inspect(final,n,ri,r,rk,parent,pri,ai,o,an,ar,aa,anc,skipped,140,o['total']+cost)
       if not ok:continue
       value,ok=inspect(final,n,ri,r,rk,parent,pri,ai,o,an,ar,aa,anc,skipped,118,value)
       if not ok or value>=INF:continue
       if kind==0 and(r['symbol']==217 or final and r['symbol']==211):sequence=o['seq'];identity=mapping[sequence]
       elif not final and kind==2:sequence=(r['symbol'],);identity=resolve(sequence)
       else:
        sequence=o['seq']
        if final and kind==2:sequence=sequence+(65531,);resolve(sequence)
        sequence=sequence+(r['symbol'],);identity=resolve(sequence)
       e=dict(r,total=value,acoustic=o['acoustic']+cost,previous=pri,column=ai,path=kind==2,likelihood=0,seq=sequence)
       #Native cap1 preserves an equal-cost earlier entry and updates a lower
       #same-identity entry in place. This is not a sorted Python dictionary.
       if target[ri]and target[ri][0]['total']<=value:continue
       target[ri]=[e];b=storage[ri];struct.pack_into('<ii',b,0,e['total'],e['acoustic']);b[9]=pri;b[10]=ai
       struct.pack_into('<HHH',b,12,r['symbol'],(struct.unpack_from('<H',b,14)[0]&0xfc00)|(r['span']<<1)|int(kind==2)|(r['category']<<4),r['arc']);struct.pack_into('<Q',b,24,identity);b[32]&=224
   tables[final].append(target);stores[final].append([dict(Count=len(target[ri]),Bytes=base64.b64encode(storage[ri]).decode())for ri in range(5)])
 assert ti==len(trace)and made==set(mapping),'all ordered callbacks and all allocated symbolic paths'
 assert stores[0]==captured['B0']and stores[1]==captured['B8'],'full B0/B8 storage'
 return sum(len(r)for t in tables for n in t for r in n),len(trace),sum(c[33]>0 for c in trace)

def shortlist(source,captured,costRow):
 groups={v['Id']:v for v in costRow['Groups']}
 for n,record in enumerate(captured['Primary']):
  rows=decode(record);seen=set();owners=[];counts=[0]*5;storage=bytearray(200)
  for ri,r in enumerate(rows):
   arc=r['arc']
   if arc in seen:continue
   seen.add(arc);index=len(owners);owners.append(ri);group=groups[source['Nodes'][n][arc]['Group']];entries=group['Entries'];values=[f(math.exp(f(-f(e['Cost'])/8192)))for e in entries[:20]];total=0.
   for value in values:total=f(total+value)
   weights=[1.]if len(entries)==1 else[f(v/total)for v in values]
   for entry,weight in zip(entries,weights):
    if entry['Symbol']in(217,211):continue
    off=index*40;struct.pack_into('<ii',storage,off,r['total'],r['acoustic']);storage[off+8]=max(1,min(255,int(f(weight*255))));struct.pack_into('<H',storage,off+12,entry['Symbol']);struct.pack_into('<H',storage,off+16,arc);counts[index]=1;break
  for ri,count in enumerate(counts):
   if count==0:struct.pack_into('<i',storage,ri*40,INF)
  assert captured['A8'][n]==dict(Groups=len(owners),PrimaryOrdinals=owners,Counts=counts,Bytes=base64.b64encode(storage).decode()),('full A8',n)

actual=[0,0,0];prepared=[0,0,0]
assert len(context['Actual'])==124 and len(context['Prepared'])==len(controls)==512
for case,source,costRow in zip(context['Actual'],primary['Actual'],controller['Rows']):
 assert case['Raw']==source['Raw']and case['Primary']==source['Primary'];shortlist(source,case,costRow)
 expected=[];groups={v['Id']:v for v in costRow['Groups']}
 for n,record in enumerate(case['Primary']):
  q=[]
  for r in decode(record):
   if n-r['span']-1<0:continue
   cost=lookup(groups[source['Nodes'][n][r['arc']]['Group']],r['symbol']);q.append([r['arc'],r['symbol'],r['category'],composites[r['category']][1],cost])
  expected.append(q)
 assert case['Costs']==expected,'context FFFF lookup with resolved prototype (not compositeID)'
 values=replay(source,case['Primary'],case,costRow);actual=[a+b for a,b in zip(actual,values)]
for case,control,source in zip(context['Prepared'],controls,primary['Prepared']):
 assert case['Primary']==source['Primary']and case['Costs']==control['costs'];values=replay(primary['PreparedOwner'],case['Primary'],case,case['Costs']);prepared=[a+b for a,b in zip(prepared,values)]
assert actual==[context[k]for k in('ActualRows','ActualCalls','ActualSkips')]==[7700,56854,24]
assert prepared==[context[k]for k in('PreparedRows','PreparedCalls','PreparedSkips')]==[4096,8448,2048]and context['Guards']==623
print(f'PASS actual124 A8+{actual[0]}full B0/B8 records/{actual[1]}ORDERED callback snapshots/{actual[2]}ancestor; prepared512/{prepared[0]}records/{prepared[2]}ancestor/623guards; raw217/prototypes/FFFF, symbolic allocation/identity and every storage byte audited. No terminal/text.')
