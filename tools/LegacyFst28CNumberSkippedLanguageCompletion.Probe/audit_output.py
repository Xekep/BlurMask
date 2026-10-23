"""Independent source-bound Number node0 value/mask oracle.
MODELS CAPTURE OLD_CONSUMER_CAPTURE OLD_CONTEXT_CAPTURE OLD_D0_CAPTURE
The STATIC reference is a canonical dependency; no production C# is executed.
"""
# Exact reviewed source reversal; no numerical/capture authority is changed.
import hashlib as _reviewed_hashlib,runpy as _reviewed_runpy
from pathlib import Path as _ReviewedPath
_reviewed_loader=_ReviewedPath(__file__).resolve().parents[1]/'handwriting/reviewed_node0_source_restoration.py'
assert _reviewed_hashlib.sha256(_reviewed_loader.read_bytes().replace(b'\r\n',b'\n')).hexdigest()=='9f310e7f9397b40d35a623a65e0064a0b9cee709cda3a9c46bd159e37186bc1a','Unreviewed source restoration loader'
_restore_reviewed_source=_reviewed_runpy.run_path(str(_reviewed_loader),run_name='reviewed_node0_source_pin_only')['restore_source_for_pin']
import base64,copy,hashlib,json,runpy,struct,subprocess,sys
from pathlib import Path
HERE=Path(__file__).resolve().parent;REPO=HERE.parents[1];sha=lambda b:hashlib.sha256(b).hexdigest()
# Filled from exact source-review bytes; no arbitrary accessor normalization.
PRODUCTION_LF='fcee1656d3dbb1ef9b1430d536c9bf1110987b4267a2d63d26696c2d41197c02'
PROGRAM_LF='2c8e7acd4609fd386effce4bbe1c7e824bbc103ec2ab0d56ff26c91e8a4b263c'
OLD_CONSUMER_LF='25be1dce91a32902c2b29fcc24c14b3f28ebac2a5f18e7fbd4d87b70620a7935'
CURRENT_CONSUMER_LF='b0704fff028e7323050d36b14dd3cf9232a9d2e79e8d1b398292798675801875'
CLAIM_FIELD='    private int skippedLanguageCompletionClaim;\n'
CLAIM_METHODS='    // Same-receiver scalar claim: no shared table, allocation or resize.\n    internal void ClaimSkippedLanguageCompletion(LegacyFst28CNumberSkippedLanguageCompletion receiver)\n    {\n        ArgumentNullException.ThrowIfNull(receiver);\n        if(!ReferenceEquals(receiver.ExpectedConsumer,this))throw new ArgumentException("Foreign receiving Number consumer.");\n        RequireCompleted();\n        if(Interlocked.CompareExchange(ref skippedLanguageCompletionClaim,1,0)!=0)\n            throw new InvalidOperationException("This exact initial Number consumer already claimed node0 completion.");\n    }\n    internal bool ReadSkippedLanguageCompletionClaim()=>Volatile.Read(ref skippedLanguageCompletionClaim)!=0;\n'
ADDITION='// Trusted receiving read only; no snapshot grants completion authority.\n    internal void AttachSkippedLanguageCompletion(LegacyFst28CNumberSkippedLanguageCompletion receiver,CancellationToken token)\n    {\n        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(receiver);\n        if(!ReferenceEquals(receiver.ExpectedConsumer,this))throw new ArgumentException("Foreign receiving Number consumer.");\n        RequireCompleted();ValidateRetainedPool(token);\n        receiver.AcceptTrustedInitial(this,pool!,heap!,grafts,token);\n    }\n    '
def source_preimage(data):
 data=_restore_reviewed_source('LegacyFst28CNumberInitialConsumer.cs',data)
 b=data.replace(b'\r\n',b'\n');assert sha(b) in (OLD_CONSUMER_LF,CURRENT_CONSUMER_LF)
 if sha(b)==CURRENT_CONSUMER_LF:
  for addition in (ADDITION,CLAIM_FIELD,CLAIM_METHODS):
   part=addition.encode();assert b.count(part)==1;b=b.replace(part,b'')
 assert sha(b)==OLD_CONSUMER_LF;return b
STAGES=['after-claim','before-root-alternatives','before-sort','before-merge','before-heap-rebuild','before-materialize',
 'before-phase1-reset','before-common-tail','before-logical-source-reset','before-publish']
def guard_names(admitted):
 g=['null-consumer','cancel-before-null']
 g+=['byte-domain-'+str(n) for n in (-1,2097153,2147483647)]
 g+=['work-domain-'+str(n) for n in (-1,67108865,2147483647)]
 g+=['one-byte-short','one-work-short','foreign-genuine-consumer','cancel-before-claim','preclaim-cancel-no-claim']
 for i,ok in enumerate(admitted):
  if ok:g+=['restricted-owner-'+str(i),'complete-skipped-tail-'+str(i),'same-consumer-replay-'+str(i),
    'actual-returned-buffer-mutations-'+str(i),'canceled-inspection-'+str(i),'parallel-detached-inspection-'+str(i)]
  else:g+=['native-language-required-'+str(i),'native-language-preclaim-no-claim-'+str(i)]
  g+=['old-consumer-and-context-unchanged-'+str(i)]
 g+=['equal-data-foreign-genuine-owner','equal-data-receiver-refusal']
 for stage in STAGES:g+=['cancel-'+stage,'poison-after-cancel-'+stage]
 return g+['exact-postclaim-fault','poison-after-fault','same-consumer-actual-claim-race']
def compare_result(actual,expected,prepared=False):
 for dst,src in [('stored','stored'),('primary','primary'),('secondary','secondary'),
   ('storedDefined','storedDefined'),('primaryDefined','primaryDefined'),('secondaryDefined','secondaryDefined')]:
  assert base64.b64decode(actual[dst])==bytes.fromhex(expected[src]),dst
 for dst,src in [('count','retained'),('unique','unique'),('sourceEnd','sourceEnd'),('retainedEnd','retainedEnd'),
  ('phase1Count','phase1Count'),('phase1Capacity','phase1Capacity'),('allocatedCapacity','allocatedHeapCapacity'),
  ('cleanup','cleanup'),('cumulative2520','cumulative2520'),('backward2528','backward2528')]:assert actual[dst]==expected[src],dst
 reset=actual['logicalReset'];assert reset['logicalOnly'] and not(reset['livePageEngine'] or reset['liveQuery'] or reset['nextNodeAuthority'])
 assert reset['sameOwner'] is (not prepared) and reset['sourceNode']==0
 if prepared:assert reset['stores']==reset['current']==reset['preserved']==reset['secondary']==[]
 else:
  stores=[[0xc8,4,2**32-1]]
  for i in range(8):stores.extend([[0x100+80*i,8,2**64-1],[0x110+80*i,8,2**64-1],[0x118+80*i,4,0 if i==7 else 2**32-1]])
  assert reset['stores']==stores and len(stores)==25 and sum(s[1] for s in stores)==164
def prepared_merge_inputs(m,primary,mode,swap=False):
 H=250;rows=bytearray(H*128);rm=bytearray(H*128);rm[:120]=bytes([255])*120;rm[128:248]=bytes([255])*120
 m['put'](rows,88,0);m['put'](rows,128+88,1);m['put'](rows,16,10);m['put'](rows,144,20)
 p=bytearray(H*80);pm=m['root_pool_mask'](2,primary)
 if swap:
  # Exact C# prepared swap fixture leaves both input heap costs zero.
  m['put'](rows,16,0);m['put'](rows,144,0)
  rm[128:256]=bytes([255])*128;m['put'](p,0,20);m['put'](p,80,10);m['put'](p,8,1,8);m['put'](p,88,2,8);m['put'](p,40,0x4000000);m['put'](p,120,0x4000000)
 else:
  for i in range(2):
   at=i*80;m['put'](p,at,10+i*10);m['put'](p,at+4,2+i);m['put'](p,at+8,100+i,8);m['put'](p,at+16,200+i,8)
   p[at+31]=32 if primary else 0;m['put'](p,at+40,0x4000000);m['put'](p,at+44,0x4000000);p[at+32:at+36]=bytes([17,34,51,68])
  pm[71]=128 if primary else 160;pm[72:76]=bytes([255])*4
  pm[112:116]={'native-unknown':bytes(4),'prepared-partial':bytes([240,15,170,85]),'prepared-known':bytes([255])*4}[mode]
  pm[111]=37 if primary else 17
 a,b=bytearray(rows[:128]),bytearray(rows[128:256]);am,bm=bytearray(rm[:128]),bytearray(rm[128:256])
 m['merge'](p,a,b,primary,pm,am,bm);rows[:128]=a;rows[128:256]=b;rm[:128]=am;rm[128:256]=bm
 return dict(heap=rows,heapDefined=rm,pool=p,poolDefined=pm)
OBSERVATION_SHA='860cb7f4016f9493777605e9442c7c344a101fccb9f61da0f266efb5cdd0a4fd'
def immutable_lookahead():
 raw=(HERE/'number-gate-observation.json').read_bytes();assert sha(raw)==OBSERVATION_SHA
 graphs=json.loads(raw)['graphs'];assert len(graphs)==4
 return [[[dict(node=n,ordinal=a['ordinal'],span=a['span'],flags=a['flags']) for a in graph[n]] for n in range(1,min(3,len(graph)))] for graph in graphs]
CLEANUP_MODES=('known-write','unknown-flag-write','unknown-parent-write','partial-cost-write','unknown-flag-no-write','known-flag-skip','known-parent-skip','known-cost-skip')
def cleanup_input(m,mode):
 rows=[bytearray(128) for _ in range(250)];rm=[bytearray([255]*128) for _ in rows]
 p=bytearray(20000);q=bytearray(20000);pm=bytearray([255]*20000);qm=bytearray(pm)
 put=m['put'];put(rows[0],16,10);put(rows[0],60,65535,2);put(rows[0],62,65535,2)
 put(p,0,10);put(p,24,125,2);put(p,40,0x4000000);put(q,0,0x4000000);put(q,40,0x4000000)
 if mode in ('unknown-flag-write','unknown-flag-no-write'):pm[31]&=223
 if mode=='unknown-parent-write':pm[24]=0
 if mode=='partial-cost-write':pm[0]=240
 if mode in ('unknown-flag-no-write','known-flag-skip'):p[31]=32
 if mode=='known-parent-skip':put(p,24,0,2)
 if mode=='known-cost-skip':put(p,0,0x4000000)
 return m['phase0'](rows,1,p,q,rm,pm,qm)


def strict_integer(value,lo,hi):
 assert type(value) is int and lo<=value<=hi

def context_mapping(case,old_context):
 pairs=case['contextIdentityMap'];nodes=case['contextNodes'];assert len(pairs)==len(nodes)==26
 assert all(set(p)=={'raw','canonical'} for p in pairs)
 for p in pairs:
  strict_integer(p['raw'],1,2**64-1);strict_integer(p['canonical'],1,2**64-1)
 assert [p['canonical'] for p in pairs]==list(range(1,27))
 raw=[p['raw'] for p in pairs];assert all(type(x) is int and 0<x<2**64 for x in raw) and len(set(raw))==26
 assert raw==[n['identity'] for n in nodes]
 mapping=dict(zip(raw,range(1,27)))
 def slot(value):
  if value==0:return -1
  assert value in mapping;return mapping[value]-1
 topology=[]
 for i,n in enumerate(nodes):
  assert set(n)=={'identity','symbol','left','right','balance','parentLink','childTreeRoot','pageIndex','pageSlot'}
  for key in ('identity','left','right','parentLink','childTreeRoot'):strict_integer(n[key],0,2**64-1)
  strict_integer(n['symbol'],0,65535);strict_integer(n['balance'],-128,127)
  strict_integer(n['pageIndex'],-2**31,2**31-1);strict_integer(n['pageSlot'],-2**31,2**31-1)
  assert (n['pageIndex'],n['pageSlot'])==(0,i)
  topology.append([i,n['symbol'],slot(n['left']),slot(n['right']),n['balance'],slot(n['parentLink']),slot(n['childTreeRoot']),n['pageIndex'],n['pageSlot']])
 assert topology==old_context['nodes']
 assert (old_context['activeNodes'],old_context['sourceNamespace'],old_context['sourceHeader'])==(26,1,1)
 return mapping

def canonical_identity(data,offset,mapping):
 raw=struct.unpack_from('<Q',data,offset)[0]
 if raw:assert raw in mapping;struct.pack_into('<Q',data,offset,mapping[raw])

def canonical_initial(records,mapping):
 result=[]
 for r in records:
  b=bytearray.fromhex(r);assert len(b)==120;canonical_identity(b,8,mapping);result.append(b.hex().upper())
 return result

def canonical_snapshot(snapshot,mapping):
 # Raw evidence remains untouched. Only defined semantic context identity
 # fields are canonicalized on this bounded detached diagnostic copy.
 result=copy.deepcopy(snapshot)
 for key,maskkey,stride,offsets in [('stored','storedDefined',88,(8,)),('primary','primaryDefined',80,(8,16,48,56)),('secondary','secondaryDefined',80,(8,16,48,56))]:
  b=bytearray(base64.b64decode(snapshot[key],validate=True));mask=base64.b64decode(snapshot[maskkey],validate=True)
  assert len(b)==len(mask) and len(b)%stride==0 and len(b)<=156*88
  for at in range(0,len(b),stride):
   for offset in offsets:
    if mask[at+offset:at+offset+8]==bytes([255])*8:canonical_identity(b,at+offset,mapping)
  result[key]=base64.b64encode(b).decode()
 return result

def validate(j,old,d0,m,old_context):
 assert j['schema']=='number-skipped-language-node0-v1' and not(j['liveQuery'] or j['node1'] or j['parentHistory'] or j['completeController'])
 assert(j['bytes'],j['work'])==(1211568,61261520)
 assert(j['identityDiagnosticBytes'],j['identityDiagnosticWork'])==(16384,65536)
 assert(j['canonicalGlyphNN'],j['canonicalContextNN'],j['extraOwners'],j['extraGlyphNN'],j['extraContextNN'])==(16,3,12,48,0)
 assert len(j['cases'])==len(old['cases'])==len(d0['cases'])==4;admitted=[];lookahead=immutable_lookahead()
 for c,prior,dc in zip(j['cases'],old['cases'],d0['cases']):
  i=c['index'];assert i==len(admitted)==prior['index']==dc['index'];assert c['lookahead']==lookahead[i]
  mapping=context_mapping(c,old_context['cases'][i]);assert canonical_initial(c['initialRecords'],mapping)==[r.upper() for r in prior['records']]
  assert c['current']==dc['primary'] and base64.b64decode(c['permissions'])==bytes(dc['permissions']) and base64.b64decode(c['eligibility'])==bytes(dc['eligibility'])
  # Captured readonly genuine retained graph rows are numeric inputs to the
  # native152400 predicate; owner authority is separately guarded in C#.
  boundary=False
  for arcs in c['lookahead']:
   assert len(arcs)<=256
   for arc in arcs:
    assert arc['node'] in (1,2) and 0<=arc['span']<=8 and 0<=arc['flags']<256
    if arc['node']-arc['span']-1==0 and arc['flags']&4:boundary=True
  ok=not boundary;admitted.append(ok);assert c['refused'] is boundary
  if not ok:assert c['result'] is None and c['association']==[];continue
  rows,p,q=m['pools_from_case'](prior);expected=m['phase0'](rows,prior['count'],p,q)
  compare_result(canonical_snapshot(c['result'],mapping),expected);assert c['association']==[1,1,1,1,26,0,0,0]
  reset=c['result']['logicalReset'];assert reset['current']==dc['primary']
  assert reset['preserved']==[65533,0,1,2,3,4,5,6,7,8,9,65531,141,139,138,185]
  assert reset['secondary']==[65533]+[65535]*10+[65531]+[65535]*4
 assert admitted==[True,False,True,False]
 names=guard_names(admitted);assert len(names)==58 and len(set(names))==58 and j['guards']==names
 cancel=['cancel-before-null','cancel-before-claim']+['canceled-inspection-'+str(i) for i,x in enumerate(admitted) if x]+['cancel-'+s for s in STAGES]
 assert [e['name'] for e in j['exceptions']]==cancel+['exact-postclaim-fault']
 for e in j['exceptions']:
  assert e['reached']
  if e['name']=='exact-postclaim-fault':assert e['type']=='System.IO.IOException' and e['message']=='bounded diagnostic fault' and not(e['tokenMatched'] or e['tokenCanceled'])
  else:assert e['type']=='System.OperationCanceledException' and e['message']=='' and e['tokenMatched'] and e['tokenCanceled']
 expected_prepared=[]
 for n in (0,1,125,126,250):
  for duplicate in (False,True):
   rows,p,q=m['prepared_inputs'](n,duplicate);expected=m['phase0'](rows,n,p,q,[bytes([255])*128 for _ in rows],bytes([255])*len(p),bytes([255])*len(q))
   expected_prepared.append(('prepared-%d-%s'%(n,'duplicate' if duplicate else 'unique'),expected))
 assert len(j['prepared'])==10
 for c,(name,result) in zip(j['prepared'],expected_prepared):assert c['name']==name;compare_result(c['result'],result,True)
 expected_merges=[]
 for primary in (False,True):
  for mode in ('native-unknown','prepared-partial','prepared-known'):
   expected_merges.append((('primary' if primary else 'secondary')+'-FIRST-to-SECOND-'+mode,prepared_merge_inputs(m,primary,mode)))
 expected_merges.append(('secondary-full-slot-swap-before-narrow-writes',prepared_merge_inputs(m,False,'',True)))
 assert len(j['merges'])==7
 for c,(name,result) in zip(j['merges'],expected_merges):
  assert c['name']==name and not c['result']['hasOwnerAuthority']
  for k,value in result.items():assert base64.b64decode(c['result'][k])==value,(name,k)
 assert len(j['cleanups'])==len(CLEANUP_MODES)
 for c,mode in zip(j['cleanups'],CLEANUP_MODES):
  assert c['name']=='cleanup-'+mode;compare_result(c['result'],cleanup_input(m,mode),True)
 attempts=j['race']['attempts'];assert len(attempts)==2
 assert [a['won'] for a in attempts]==[True,False]
 for a in attempts:
  assert a['beforeClaim'] is True and a['afterClaim'] is a['won']
  assert (a['type'],a['message'])==(('','') if a['won'] else ('System.InvalidOperationException','This exact initial Number consumer already claimed node0 completion.'))
 return names
def main(args):
 if len(args)!=5:raise SystemExit(__doc__)
 models,capture,oldp,contextp,d0p=map(Path,args)
 for p,digest in [(REPO/'src/InkRecognition/LegacyFst28CNumberSkippedLanguageCompletion.cs',PRODUCTION_LF),(HERE/'Program.cs',PROGRAM_LF)]:assert sha(p.read_bytes().replace(b'\r\n',b'\n'))==digest
 current=(REPO/'src/InkRecognition/LegacyFst28CNumberInitialConsumer.cs').read_bytes();source_preimage(current)
 # Preserves unchanged old capture and numerical oracle; no wrapper/pin bypass.
 cmd=[sys.executable,'-I','-B',str(HERE.parent/'LegacyFst28CNumberInitialConsumer.Probe/audit_output.py'),str(models),str(oldp),str(contextp),str(d0p)]
 r=subprocess.run(cmd,stdout=subprocess.PIPE,stderr=subprocess.STDOUT);assert r.returncode==0,r.stdout.decode();print(r.stdout.decode(),end='')
 m=runpy.run_path(str(HERE.parent/'handwriting/reference_number_node0_completion.py'))
 j=json.loads(capture.read_bytes());old=json.loads(oldp.read_bytes());d0=json.loads(d0p.read_bytes());old_context=json.loads(contextp.read_bytes());names=validate(j,old,d0,m,old_context)
 negatives=[]
 for label,change in [('stored-byte',lambda c:c['cases'][0]['result'].__setitem__('stored',base64.b64encode(bytes(13728)).decode())),
  ('mask-byte',lambda c:c['cases'][0]['result'].__setitem__('primaryDefined',base64.b64encode(bytes(10000)).decode())),
  ('source-end',lambda c:c['cases'][0]['result'].__setitem__('sourceEnd',8)),('phase1-count',lambda c:c['cases'][0]['result'].__setitem__('phase1Count',8)),
  ('reset-live',lambda c:c['cases'][0]['result']['logicalReset'].__setitem__('livePageEngine',True)),
  ('reset-store',lambda c:c['cases'][0]['result']['logicalReset']['stores'][0].__setitem__(2,0)),
  ('cohort',lambda c:c['cases'][0]['association'].__setitem__(0,0)),('gate',lambda c:c['cases'][1].__setitem__('refused',False)),
  ('graph-delete',lambda c:c['cases'][0].__setitem__('lookahead',[])),
  ('graph-ordinal',lambda c:c['cases'][0]['lookahead'][0][0].__setitem__('ordinal',99)),
  ('graph-row-delete',lambda c:c['cases'][0]['lookahead'].pop()),
  ('graph-arc-delete',lambda c:c['cases'][0]['lookahead'][0].pop()),
  ('graph-arc-order',lambda c:c['cases'][0]['lookahead'][0].reverse()),
  ('graph-flags-same-gate',lambda c:c['cases'][0]['lookahead'][0][0].__setitem__('flags',1)),
  ('cleanup-mask',lambda c:c['cleanups'][1]['result'].__setitem__('primaryDefined',base64.b64encode(bytes(10000)).decode())),
  ('cleanup-bytes',lambda c:c['cleanups'][0]['result'].__setitem__('primary',base64.b64encode(bytes(10000)).decode())),
  ('cleanup-counter',lambda c:c['cleanups'][0]['result'].__setitem__('cleanup',999)),
  ('race-type',lambda c:next(a for a in c['race']['attempts'] if not a['won']).__setitem__('type','System.Exception')),
  ('race-message',lambda c:next(a for a in c['race']['attempts'] if not a['won']).__setitem__('message','other')),
  ('race-atclaim',lambda c:next(a for a in c['race']['attempts'] if not a['won']).__setitem__('afterClaim',True)),
  ('race-beforeclaim',lambda c:next(a for a in c['race']['attempts'] if not a['won']).__setitem__('beforeClaim',False)),
  ('context-canonical-bool',lambda c:c['cases'][0]['contextIdentityMap'][0].__setitem__('canonical',True)),
  ('context-balance-float',lambda c:c['cases'][0]['contextNodes'][0].__setitem__('balance',float(c['cases'][0]['contextNodes'][0]['balance']))),
  ('context-map-gap',lambda c:c['cases'][0]['contextIdentityMap'].pop()),
  ('context-map-duplicate',lambda c:c['cases'][0]['contextIdentityMap'][1].__setitem__('raw',c['cases'][0]['contextIdentityMap'][0]['raw'])),
  ('context-map-order',lambda c:c['cases'][0]['contextIdentityMap'].reverse()),
  ('context-node-missing',lambda c:c['cases'][0]['contextNodes'].pop()),
  ('context-page-slot',lambda c:c['cases'][0]['contextNodes'][0].__setitem__('pageSlot',99)),
  ('context-foreign-link',lambda c:c['cases'][0]['contextNodes'][0].__setitem__('left',2**63)),
  ('context-symbol',lambda c:c['cases'][0]['contextNodes'][0].__setitem__('symbol',65535)),
  ('context-row-foreign',lambda c:c['cases'][0]['initialRecords'].__setitem__(0,c['cases'][0]['initialRecords'][0][:16]+'FFFFFFFFFFFFFFFF'+c['cases'][0]['initialRecords'][0][32:])),
  ('context-outside-field',lambda c:c['cases'][0]['initialRecords'].__setitem__(0,'FF'+c['cases'][0]['initialRecords'][0][2:])),
  ('guard-delete',lambda c:c['guards'].pop()),('token-witness',lambda c:c['exceptions'][0].__setitem__('tokenMatched',False)),
  ('prepared-mask',lambda c:c['merges'][0]['result'].__setitem__('poolDefined',base64.b64encode(bytes(20000)).decode()))]:
  bad=copy.deepcopy(j);change(bad)
  try:validate(bad,old,d0,m,old_context)
  except AssertionError:negatives.append(label)
  else:raise AssertionError(('accepted corruption',label))
 for part in [b' ',b'\n',b'//wrong\n']:
  try:source_preimage(current+part)
  except AssertionError:negatives.append('source-'+part.hex())
  else:raise AssertionError('unapproved source accepted')
 assert negatives==['stored-byte', 'mask-byte', 'source-end', 'phase1-count', 'reset-live', 'reset-store', 'cohort', 'gate', 'graph-delete', 'graph-ordinal', 'graph-row-delete', 'graph-arc-delete', 'graph-arc-order', 'graph-flags-same-gate', 'cleanup-mask', 'cleanup-bytes', 'cleanup-counter', 'race-type', 'race-message', 'race-atclaim', 'race-beforeclaim', 'context-canonical-bool', 'context-balance-float', 'context-map-gap', 'context-map-duplicate', 'context-map-order', 'context-node-missing', 'context-page-slot', 'context-foreign-link', 'context-symbol', 'context-row-foreign', 'context-outside-field', 'guard-delete', 'token-witness', 'prepared-mask', 'source-20', 'source-0a', 'source-2f2f77726f6e670a']
 print(json.dumps(dict(status='PASS scoped Number node0',guards=len(names),exceptions=len(j['exceptions']),
  actualCompletions=2,preclaimRefusals=2,prepared=10,partialMaskMerges=7,cleanupControls=8,exactRaceWitness=True,negativeControls=negatives,
  canonicalNN=19,extraGlyphNN=48,captureSHA256=sha(capture.read_bytes()),liveQuery=False,completeController=False)))
if __name__=='__main__':main(sys.argv[1:])
