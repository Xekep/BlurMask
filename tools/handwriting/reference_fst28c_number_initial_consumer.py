"""Target-blind PREPARED numerical projection from frozen genuine D0/page snapshots.
Not an issued consumer, root-list receipt, native execution or word recognition.
"""
from pathlib import Path
import json,struct,hashlib,sys
models,contextPath,d0Path,output=map(Path,sys.argv[1:])
ak=(models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes();sha=lambda b:hashlib.sha256(b).hexdigest()
assert sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<HH',ak,188222)==(0x201,192)
p=188226

def ids():
 global p
 n=ak[p];p=(p+4)&~3;v=struct.unpack_from('<'+'i'*n,ak,p);p+=4*n;return list(v)
first=ids();fm=list(struct.unpack_from('<195b',ak,p));p+=195
geometry=ids();sm=list(struct.unpack_from('<195b',ak,p));p+=195;em=list(struct.unpack_from('<195b',ak,p));p+=195
pairs=[];maps=[[],[],[],[]]
for a,b,c in zip(fm,sm,em):
 for k,(x,y)in enumerate([(a,b),(a,c),(first.index(0),b),(first.index(0),c)]):
  v=-1
  if x>=0 and y>=0:
   pair=[first[x],y]
   if pair not in pairs:pairs.append(pair)
   v=pairs.index(pair)
  maps[k].append(v)
assert len(pairs)==35 and len(geometry)==19
assert struct.unpack_from('<i',ak,189363)[0]==0x4000000
# No arbitrary source category, table or policy is supplied.
assert struct.unpack_from('<I',ak,5233)[0]==0x135
assert struct.unpack_from('<3i',ak,566390)==(20480,24576,32768)
assert struct.unpack_from('<H',ak,572667)[0]==125 and ak[5279:5281]==bytes(2)
context=json.loads(contextPath.read_bytes());d0=json.loads(d0Path.read_bytes());predictions=[]
wrap=lambda x:(x+2**31)%2**32-2**31
for i,(c,d)in enumerate(zip(context['cases'],d0['cases'])):
 assert c['sourceNamespace']==1 and c['sourceHeader']==1 and c['sourceKind40']==0
 cache={r['group']:r for r in d['rows']};attempts=[];absent=0;qualified=0
 for rowIndex,(tag,sourceId,base,symbol,length,flags,extra)in enumerate(c['sourceRecords']):
  assert length==flags==extra==0 and symbol<192
  # Exact152520 ordinary length0 branch does not dereference98 or parents.
  if not d['eligibility'][symbol]:continue
  qualified+=1
  for variant in range(2):
   category=maps[variant][symbol]
   if category<0:
    assert variant==1;absent+=1;break
   prototype=pairs[category][1];assert 0<=prototype<19
   for ordinal,arc in enumerate(d['arcs']):
    if arc['span']!=0:continue
    r=cache[arc['group']];assert len(r['ranges'])==1
    # Complete193860 returns0 before geometry/table accesses when rangecount1.
    geometryCost=0;cost=r['cacheCosts'][symbol]
    total=wrap(base+cost+geometryCost)if cost!=0x4000000 else cost
    accepted=cost!=0x4000000 and total<0x4000000
    attempts.append(dict(row=rowIndex,tag=tag,symbol=symbol,variant=variant,category=category,prototype=prototype,
      preparedNode0Ordinal=ordinal,group=arc['group'],sourceCost=base,classCost=cost,geometry=geometryCost,
      total=total,increment=wrap(total-base)if accepted else 0x4000000,accepted=accepted))
 assert qualified==8 and absent==8 and len(attempts)==8 and all(x['accepted']for x in attempts)
 # These arc inputs are captured node0/span0 rows, not a captured F0/F8 vector.
 # The future genuine owner MUST authenticate root.CopySelector(1) membership/order.
 heap=[]
 for a in attempts:
  item=[a['total'],a['row'],a['symbol']];heap.append(item);at=len(heap)-1
  while at:
   parent=(at-1)//2
   if heap[parent][0]>=item[0]:break
   heap[at]=heap[parent];at=parent
  heap[at]=item
 predictions.append(dict(index=i,qualified=qualified,missingAlternate=absent,preparedAttempts=attempts,
   initialHeapCostProjection=heap,heapCapacity=250,unprovenRuntimeRootList=True,query=False,wordRecognition=False))
K=572763;T=64;A=256;R=12;V=2;H=250;Q=R*V*A
B=K+8192+2*16384+T*(195*4+19*4+64)+4096+195+256+2*H*128+8192+R*64+4096
W=6*K+8192+16384+195+2048+A*T+T*(2*195*4+3*19*4+256)+Q*(1024+8*128)+2*H*128+30*64+R*64+4096
assert (B,W,Q)==(754206,16264293,6144)
controls={'wrongDoubleVariantCount':sum(2*x['qualified']for x in predictions)==64,
 'actualMissingAlternateCount':sum(x['missingAlternate']for x in predictions)==32,
 'wrongKindOneThresholdZeroRejectsAll':all(a['classCost']>0 for x in predictions for a in x['preparedAttempts']),
 'range1GeometryZeroBeforeTable':True,'notRootOrdinal5':all(x['sourceNamespace']==1 and x['sourceHeader']==1 for x in context['cases']),
 'strictImpossible':not(0x4000000<0x4000000),'signed32Wrap':wrap(2147483647+1)==-2147483648}
assert all(controls.values())
report=dict(scope=__doc__,contextSHA256=sha(contextPath.read_bytes()),d0SHA256=sha(d0Path.read_bytes()),
 categoryProfile=dict(firstIds=first,geometryIds=geometry,pairs=pairs,start=maps[0],end=maps[1],zeroStart=maps[2],zeroEnd=maps[3]),
 predictions=predictions,preparedControls=controls,bytes=B,work=W,maximumAttempts=Q,vendorExecution=False)
output.parent.mkdir(parents=True,exist_ok=True);output.write_text(json.dumps(report,indent=2)+'\n')
print('PASS PREPARED32 source-row numerical projections/32 missing alternates/4cost heaps; not root-list/word authority; B/W',B,W)
