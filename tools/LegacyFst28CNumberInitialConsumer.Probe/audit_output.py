"""Independent raw category/root/request/heap oracle; import-safe exact source preimages.
MODELS CAPTURE OLD_CONTEXT_CAPTURE OLD_D0_CAPTURE [--observe-root].
"""
# Exact reviewed source reversal; no numerical/capture authority is changed.
import hashlib as _reviewed_hashlib,runpy as _reviewed_runpy
from pathlib import Path as _ReviewedPath
_reviewed_loader=_ReviewedPath(__file__).resolve().parents[1]/'handwriting/reviewed_node0_source_restoration.py'
assert _reviewed_hashlib.sha256(_reviewed_loader.read_bytes().replace(b'\r\n',b'\n')).hexdigest()=='9f310e7f9397b40d35a623a65e0064a0b9cee709cda3a9c46bd159e37186bc1a','Unreviewed source restoration loader'
_restore_reviewed_source=_reviewed_runpy.run_path(str(_reviewed_loader),run_name='reviewed_node0_source_pin_only')['restore_source_for_pin']
import ast,base64,copy,hashlib,json,runpy,struct,sys
from pathlib import Path
sha=lambda b:hashlib.sha256(b).hexdigest()
SOURCE_TRANSFORMS={'LegacyFst28CNumberInitialD0.cs': {'before': 'public sealed class LegacyFst28CNumberInitialD0', 'after': 'public sealed partial class LegacyFst28CNumberInitialD0', 'old': '407e7e8c249ff23864e546fea865f2632e422cfdaa875b3c7e20d3d9e19f8cb0', 'new': '5e2b4353b4031932a74d68d90e741a390050f6d40e410570d50ccfcf5609e0af', 'addition': '546335337a956f15ca315e3145c247633de9f276e0d02c866cb2e927e82e858e'}, 'LegacyFst28CNumberInitialConsumerContext.cs': {'before': '', 'after': '    // Trusted receiving handoff to the next owned initial consumer. Historical\n    // diagnostics/snapshots are not accepted as pool, page or source authority.\n    internal void AttachInitialConsumer(LegacyFst28CNumberInitialConsumer receiver,CancellationToken token)\n    {\n        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(receiver);\n        if(!ReferenceEquals(receiver.ExpectedContext,this))throw new ArgumentException("Foreign receiving context.");\n        var d=pool.ReadDiagnostic(nodes.Length,token);\n        if(!d.Completed||!d.SameD0||!d.SameBootstrap||!d.SameGraph||!d.SameSource||!d.SameHeader||\n            !d.SameBindingAndPage||!d.HistoricalInitialViewRetained||d.ActiveNodes!=18+Count||d.Pages!=1)\n            throw new InvalidOperationException("Exact completed private number pool required.");\n        receiver.AcceptTrustedContext(this,pool,grafts.AsSpan(0,Count),token);\n    }\n', 'old': '98f19fb593fce5701672e213c2049f60c10d4e4a8f1c6532b90ffac08e9bce61', 'new': 'c0cd7b7f58736e7af86d69fcdb05497e92c92a7a2fa5e36ee8fa9e5ec0fbb6fc', 'addition': 'c33928708aa5f34bc823f8cd2b46b9f45e950021634fdd8d2b85f49d4eff1ede'}}

def source_preimage(path,data):
 data=_restore_reviewed_source(str(path),data)
 b=data.replace(b'\r\n',b'\n');name=Path(path).name
 if name not in SOURCE_TRANSFORMS:return b
 t=SOURCE_TRANSFORMS[name];digest=sha(b)
 assert digest in(t['old'],t['new']),('unapproved source',name,digest)
 if digest==t['new']:
  after=t['after'].encode();assert b.count(after)==1 and sha(after)==t['addition'];b=b.replace(after,t['before'].encode())
 assert sha(b)==t['old'];return b

def profile(ak):
 assert sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
 assert struct.unpack_from('<HH',ak,188222)==(0x201,192);p=188226
 def ids():
  nonlocal p
  n=ak[p];p=(p+4)&~3;v=list(struct.unpack_from('<'+'i'*n,ak,p));p+=4*n;return v
 first=ids();fm=struct.unpack_from('<195b',ak,p);p+=195;geometry=ids();sm=struct.unpack_from('<195b',ak,p);p+=195;em=struct.unpack_from('<195b',ak,p)
 pairs=[];maps=[[],[],[],[]]
 for a,b,c in zip(fm,sm,em):
  for k,(x,y)in enumerate([(a,b),(a,c),(first.index(0),b),(first.index(0),c)]):
   v=-1
   if x>=0 and y>=0:
    pair=(first[x],y)
    if pair not in pairs:pairs.append(pair)
    v=pairs.index(pair)
   maps[k].append(v)
 assert len(pairs)==35 and len(geometry)==19 and struct.unpack_from('<3i',ak,566390)==(20480,24576,32768)
 return pairs,maps

def verify_root(x,c,d):
 assert x['index']==c['index']==d['index']
 assert x['sourceNamespace']==c['sourceNamespace']==1 and x['sourceHeader']==c['sourceHeader']==1 and x['nodeCount']==26
 ends=x['rootEnds'];indices=x['rootIndices'];assert len(ends)==9 and ends[0]==0 and ends[-1]==len(indices) and all(a<=b for a,b in zip(ends,ends[1:]))
 roots=indices[ends[0]:ends[1]];assert roots==x['actualRoot']==x['sourceRootSuccessors0']==[0]
 assert x['node0']==[dict(group=a['group'],span=a['span'],auxiliary34=0,predecessors=[65535])for a in d['arcs']]
 assert x['selected']==[r['group']for r in d['rows']]
 assert base64.b64decode(x['eligibility'])==bytes(d['eligibility']) and x['sourceRecords']==c['sourceRecords']
 assert len(x['rows'])==len(d['rows'])
 for row,old in zip(x['rows'],d['rows']):assert row['group']==old['group'] and row['costs']==old['cacheCosts'] and len(row['ranges'])==1
 assert x['grafts']==[[i,record[1],18+i]for i,record in enumerate(c['sourceRecords'])] and len(x['grafts'])==8
 return roots

def heap_insert(heap,item,capacity):
 if not capacity or item[0]>0x3ffffff:return False
 if len(heap)==capacity:
  if heap[0][0]<=item[0]:return False
  target=0
  while target*2<len(heap)-1:
   child=target*2+1
   if child<len(heap)-1 and heap[child][0]<=heap[child+1][0]:child+=1
   if heap[child][0]<=item[0]:break
   heap[target]=heap[child];target=child
 else:
  target=len(heap);heap.append(None)
  while target:
   parent=(target-1)//2
   if item[0]<=heap[parent][0]:break
   heap[target]=heap[parent];target=parent
 heap[target]=item;return True

wrap=lambda n:(n+2**31)%2**32-2**31

def request(symbol,prototype,tag,category,flags,arc):
 b=bytearray(80)
 for off,val in [(0,0),(4,0xffffffff),(8,0xffffffff),(20,0xffffffff),(24,0xffffffff),(32,0),(48,0),(52,0x4000000),(64,0),(68,0)]:struct.pack_into('<I',b,off,val)
 for off,val in [(12,arc),(14,65535),(16,65535),(36,symbol),(38,65535)]:struct.pack_into('<H',b,off,val)
 b[18]=prototype;b[19]=255;b[28]=flags&7;struct.pack_into('<Q',b,40,1);b[56]=category;b[72]=(tag>>56)&1
 return b

def record(tag,context,total,increment,symbol,category,arc,sourceCategory,caseFlags,auxiliary):
 b=bytearray(120);struct.pack_into('<Q',b,0,tag);b[5]=((tag>>40)&0x7d)|(2 if tag&0x20000000000 else 0);b[6]=(tag>>48)&0xfc
 struct.pack_into('<QiiIHH',b,8,context,total,increment,(category&63)<<16|(arc&1023)<<22,arc,65535)
 b[33]=caseFlags&7;struct.pack_into('<HH',b,34,65535,symbol);b[39]=(((caseFlags>>5&3)+sourceCategory)&7)<<1
 struct.pack_into('<QiiiHH',b,40,2**64-1,auxiliary if caseFlags&8 else -1,auxiliary,0,65535,65535)
 struct.pack_into('<Q',b,88,2**64-1);struct.pack_into('<i',b,104,-1)
 return bytes(b)

def verify_case(case,c,d,pairs,maps):
 x=case['input'];roots=verify_root(x,c,d);elig=base64.b64decode(x['eligibility']);cache={r['group']:r['costs']for r in x['rows']};heap=[];trace=[];missing=qualified=accepted=0
 for n,(tag,source,cost,symbol,length,flags,extra)in enumerate(x['sourceRecords']):
  assert length==flags==extra==0 and symbol<192
  if not elig[symbol]:continue
  qualified+=1
  for variant in range(2):
   category=maps[variant][symbol]
   if category<0:assert variant==1;missing+=1;break
   prototype=pairs[category][1]
   for arc in roots:
    a=x['node0'][arc];group=a['group'];cl=cache[group][symbol];total=wrap(cost+cl)if cl!=0x4000000 else cl;ok=cl!=0x4000000 and total<0x4000000
    before=request(symbol,prototype,tag,length,flags,arc);after=bytearray(before);increment=wrap(total-cost)if ok else 0x4000000;struct.pack_into('<i',after,48,increment)
    rec=record(tag,x['grafts'][n][2]+1,total,increment,symbol,category,arc,length,flags,a['auxiliary34'])if ok else None
    inserted=heap_insert(heap,(total,rec),250)if ok else False;accepted+=ok
    trace.append((n,variant,arc,group,category,prototype,cl,bytes(before),bytes(after),total,ok,inserted,rec))
 assert (case['qualified'],case['attempts'],case['acoustic'],case['missing'],case['heapAccepted'],case['heapRejected'],case['count'])==(qualified,len(trace),accepted,missing,accepted,0,len(heap))
 assert len(case['trace'])==len(trace)
 for t,e in zip(case['trace'],trace):
  n,v,arc,g,cat,proto,cl,before,after,total,ok,inserted,rec=e
  assert [t[k]for k in('row','variant','arc','group','category','prototype','classCost','geometryCost','costAfter','acousticAccepted','heapAccepted')]==[n,v,arc,g,cat,proto,cl,0,total,ok,inserted]
  assert bytes.fromhex(t['before'])==before and bytes.fromhex(t['after'])==after
  if ok:
   tag,src,base,symbol,length,flags,extra=x['sourceRecords'][n]
   expectedFields=dict(sourceTag=tag,auxiliary08=x['grafts'][n][2]+1,candidateCost10=total,incrementalCost14=wrap(total-base),label24=symbol,sourceNode18=0,packedSourceArcIndex=arc,sourceArcIndex1c=arc,compositeCategory=cat,previousArcIndex1e=65535,sourceSpan=0,count26=0,headerFlag5=0,headerFlag6=0,requestedSecondaryFlag=0,packedKind27=((flags>>5&3)+length)&255,smallState21=flags&7,stateBit6=0,stateBit7=0,auxiliary30=-1,auxiliary34=0,anchorEndingNode38=0,anchorArcIndex3c=65535)
   assert t['fields']==expectedFields
  else:assert t['fields']is None
 assert [bytes.fromhex(v)for v in case['records']]==[r for _,r in heap] and len(case['candidates'])==len(heap)
 for actual,(_,rec)in zip(case['candidates'],heap):
  expected=dict(sourceTag=struct.unpack_from('<Q',rec)[0],contextIdentity=struct.unpack_from('<Q',rec,8)[0],cost=struct.unpack_from('<i',rec,16)[0],increment=struct.unpack_from('<i',rec,20)[0],symbol=struct.unpack_from('<H',rec,36)[0],compositeCategory=struct.unpack_from('<I',rec,24)[0]>>16&63,sourceArc=struct.unpack_from('<H',rec,28)[0],auxiliary34=0,previousNode=-1,previousContext=0)
  assert actual==expected
 assert case['diagnostic']==[1,1,1,1,26,26,1,len(roots),250,len(heap),1,0,0,0]
 return len(trace),missing

def verify_protocol(j):
 guards=j['guards'];expected=[]
 for i in range(4):
  for k in range(i+1,4):expected.append(f'four-independent-fixture-owners-{i}-{k}')
 expected+=['null-context','cancel-before-null']
 expected+=['byte-domain-'+str(b)for b in[-1,754207,2147483647]]
 expected+=['work-domain-'+str(b)for b in[-1,16264294,2147483647]]
 expected+=['one-byte-short','one-work-short']+['foreign-resource-length-'+str(n)for n in[0,572762,572764]]
 expected+=['mutated-same-size-resource','same-typed-pool-no-claim','foreign-genuine-context-pool','preclaim-cancel','preclaim-cancel-reached']
 per=['same-context-d0','initial-heap-only','fixed-bounds','same-pool-unchanged-after-scoring','genuine-root-order','complete-attempt-trace','same-private-owner-diagnostic','detached-diagnostic','defensive-root-copy','canceled-root-inspection']
 for i,case in enumerate(j['cases']):
  expected+=[name+'-'+str(i)for name in per]
  expected+=[f'candidate-index-{i}--1',f'candidate-index-{i}-{case["count"]}']
  expected+=[name+'-'+str(i)for name in['defensive-heap-copy','same-context-replay','projected-cache-copy-not-authority','foreign-group-projection','parallel-readonly-inspection']]
 stops=['after-claim','before-score-0','after-score-0','before-allocate-0','after-allocate-0','before-heap-snapshot','before-publish']
 for n,stop in enumerate(stops):
  if n==0:expected.append('equal-data-foreign-pool')
  expected+=[prefix+stop for prefix in['postclaim-cancel-','postclaim-checkpoint-','poison-replay-','historical-context-retained-']]
 expected+=['injected-allocation-prefix-fault','fault-consumed','one-shared-context-claim-winner']
 assert len(expected)==124 and len(expected)==len(set(expected)) and guards==expected,('exact guard sequence mismatch',len(expected),len(guards))
 assert j['purposes']==['cancel-'+stop for stop in stops]+['fault-after-allocate','same-context-barrier-race']
 patterns=[[[[0,90],[0,10],[25,60],[50,10],[50,90]]],[[[0,10],[30,90]],[[40,90],[70,10]]],[[[0,10],[40,90],[80,10]],[[0,90],[40,10],[80,90]]],[[[0,90],[0,10]],[[30,90],[30,10]],[[0,230],[0,150]]]]
 signatures=[]
 for strokes in patterns:
  raw=struct.pack('<i',len(strokes))
  for stroke in strokes:
   raw+=struct.pack('<i',len(stroke))+b''.join(struct.pack('<ff',*p)for p in stroke)
  signatures.append(sha(raw))
 assert j['fixtureSignatures']==signatures
 expectedExceptions=['cancel-before-null','preclaim-cancel']+[f'canceled-root-inspection-{i}'for i in range(4)]+['postclaim-cancel-'+stop for stop in stops]+['injected-allocation-prefix-fault']
 assert [e['name']for e in j['exceptions']]==expectedExceptions
 for e in j['exceptions']:
  if e['name']=='injected-allocation-prefix-fault':assert e==dict(name=e['name'],type='System.IO.IOException',message='bounded diagnostic fault',reached=True,matchingToken=False,requested=False)
  else:assert e['type']=='System.OperationCanceledException' and e['reached']is True and e['matchingToken']is True and e['requested']is True and e['message']==''

def main():
 args=sys.argv[1:];observe='--observe-root'in args;args=[a for a in args if a!='--observe-root']
 if len(args)!=4:raise SystemExit('MODELS CAPTURE OLD_CONTEXT_CAPTURE OLD_D0_CAPTURE [--observe-root]')
 models,capture,cp,dp=map(Path,args);repo=Path(__file__).resolve().parents[2];j=json.loads(capture.read_bytes());context=json.loads(cp.read_bytes());d0=json.loads(dp.read_bytes());pairs,maps=profile((models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes())
 old=sys.argv;sys.argv=[str(repo/'tools/LegacyFst28CNumberInitialConsumerContext.Probe/audit_output.py'),str(models),str(cp),str(dp)]
 try:runpy.run_path(sys.argv[0],run_name='__main__')
 finally:sys.argv=old
 preimages=[]
 for name,t in SOURCE_TRANSFORMS.items():
  b=(repo/'src/InkRecognition'/name).read_bytes().replace(b'\r\n',b'\n');old=source_preimage(name,b);assert sha(old)==t['old'] and source_preimage(name,old)==old
  for kind,raw in [('old',old),('new',b),('partial',b.replace(t['after'].encode(),t['before'].encode())+b' ')]:
   bad=bytearray(raw);bad[0]^=1
   try:source_preimage(name,bad)
   except AssertionError:preimages.append(name+':'+kind)
   else:raise AssertionError('source tamper accepted')
 if observe:
  assert j['schema']=='number-genuine-root-observation-v1' and j['newConsumerExecuted']is False and [j[k]for k in('glyphNN','contextNN','totalNN')]==[16,3,19]
  assert len(j['actual'])==4
  for x,c,d in zip(j['actual'],context['cases'],d0['cases']):verify_root(x,c,d)
  assert preimages==[name+':'+kind for name in SOURCE_TRANSFORMS for kind in ['old','new','partial']]
  print('PASS genuine root4/19 existingNN/F0=[0] each; NEWconsumer NOTexecuted/6source negatives');return
 assert j['schema']=='number-owning-initial-consumer-v1' and not(j['query']or j['history']or j['acceptedText'])
 assert [j[k]for k in('bytes','work','canonicalGlyphNN','canonicalContextNN','extraOwners','extraGlyphNN','extraContextNN')]==[754206,16264293,16,3,9,36,0]
 assert len(j['cases'])==4;assert [verify_case(c,o,d,pairs,maps)for c,o,d in zip(j['cases'],context['cases'],d0['cases'])]==[(8,8)]*4
 for control in j['prepared']:
  heap=[];accept=[]
  for cost in control['costs']:
   b=bytearray(120);struct.pack_into('<i',b,16,cost);accept.append(heap_insert(heap,(cost,bytes(b)),control['capacity']))
  assert control['accepted']==accept and [bytes.fromhex(v)for v in control['final']]==[r for _,r in heap]and control['authority']is False
 assert [v['capacity']for v in j['prepared']]==[0,1,2,250]
 verify_protocol(j)
 negatives=[]
 for name,mutate in [('root',lambda c:c['input']['actualRoot'].append(0)),('root-source',lambda c:c['input']['sourceRootSuccessors0'].append(0)),('cost',lambda c:c['input']['rows'][0]['costs'].__setitem__(0,0)),('eligibility',lambda c:c['input'].__setitem__('eligibility','')),('namespace',lambda c:c['input'].__setitem__('sourceNamespace',5)),('category',lambda c:c['trace'][0].__setitem__('category',1)),('prototype',lambda c:c['trace'][0].__setitem__('prototype',1)),('request',lambda c:c['trace'][0].__setitem__('before','00'*80)),('accumulator',lambda c:c['trace'][0].__setitem__('after','00'*80)),('record',lambda c:c['records'].__setitem__(0,'00'*120)),('candidate',lambda c:c['candidates'][0].__setitem__('cost',0)),('missing',lambda c:c.__setitem__('missing',0)),('delete',lambda c:c['trace'].pop()),('swap',lambda c:c['trace'].reverse()),('diagnostic',lambda c:c['diagnostic'].__setitem__(8,249))]:
  bad=copy.deepcopy(j['cases'][0]);mutate(bad)
  try:verify_case(bad,context['cases'][0],d0['cases'][0],pairs,maps)
  except (AssertionError,IndexError,ValueError):negatives.append(name)
  else:raise AssertionError('corrupt capture accepted:'+name)
 assert preimages==[name+':'+kind for name in SOURCE_TRANSFORMS for kind in ['old','new','partial']]
 protocolNegatives=[]
 for name,mutate in [('guard-delete',lambda c:c['guards'].pop()),('guard-order',lambda c:c['guards'].reverse()),('token',lambda c:c['exceptions'][0].__setitem__('matchingToken',False)),('unreached',lambda c:c['exceptions'][1].__setitem__('reached',False)),('fault-type',lambda c:c['exceptions'][-1].__setitem__('type','System.Exception')),('fault-message',lambda c:c['exceptions'][-1].__setitem__('message','different')),('fixture-signature',lambda c:c['fixtureSignatures'].__setitem__(0,'0'*64)),('extra-owner-purpose',lambda c:c['purposes'].reverse())]:
  bad=copy.deepcopy(j);mutate(bad)
  try:verify_protocol(bad)
  except AssertionError:protocolNegatives.append(name)
  else:raise AssertionError('protocol corruption accepted:'+name)
 print('PASS4genuine consumers/32first scores/32missing variants/full80+120bytes/124 ordered guards/14 exception witnesses/15data+8protocol corruptions/6source negatives')
if __name__=='__main__':main()
