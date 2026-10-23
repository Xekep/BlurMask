"""Independent first-page oracle. MODELS CAPTURE COMPLETED_D0_CAPTURE.
Full upstream19NN proof then original raw13A decoder/cost table, rawAL/AVL,
physical/accepted cursor ledger and separate raw epsilon flattening. No labels.
"""
# Exact reviewed source reversal; no numerical/capture authority is changed.
import hashlib as _reviewed_hashlib,runpy as _reviewed_runpy
from pathlib import Path as _ReviewedPath
_reviewed_loader=_ReviewedPath(__file__).resolve().parents[1]/'handwriting/reviewed_node0_source_restoration.py'
assert _reviewed_hashlib.sha256(_reviewed_loader.read_bytes().replace(b'\r\n',b'\n')).hexdigest()=='9f310e7f9397b40d35a623a65e0064a0b9cee709cda3a9c46bd159e37186bc1a','Unreviewed source restoration loader'
_restore_reviewed_source=_reviewed_runpy.run_path(str(_reviewed_loader),run_name='reviewed_node0_source_pin_only')['restore_source_for_pin']
import ast,copy,hashlib,json,math,runpy,struct,sys
from pathlib import Path
if len(sys.argv)!=4:raise SystemExit('MODELS CAPTURE COMPLETED_D0_CAPTURE')
models=Path(sys.argv[1]);capturePath=Path(sys.argv[2]);d0Path=Path(sys.argv[3]);j=json.loads(capturePath.read_bytes());d0=json.loads(d0Path.read_bytes())
repo=Path(__file__).resolve().parents[2];sha=lambda b:hashlib.sha256(b).hexdigest()
old=sys.argv;sys.argv=[str(repo/'tools/LegacyFst28CNumberInitialD0.Probe/audit_output.py'),str(models),str(d0Path)]
try:runpy.run_path(sys.argv[0],run_name='__main__')
finally:sys.argv=old
pins=json.loads((repo/'tools/handwriting/linguistic-fst28c-number-first-page-pins.json').read_bytes())
# STATIC remains frozen. Authenticate ONLY the approved13-line OLD accessor
# addition, then recover exact original preimage in memory for historical pins.
for row in pins['runtimeReviewedLF']+pins['existingIndependentDefinitionsLF']:
 b=(repo/row['path']).read_bytes().replace(b'\r\n',b'\n')
 b=runpy.run_path(str(repo/'tools/LegacyFst28CNumberInitialConsumer.Probe/audit_output.py'))['source_preimage'](row['path'],b)
 if row['path'].endswith('LegacyFst28CNumberInitialQueryBinding.cs'):
  # Exact new private-pool diff is authenticated before old13-line recovery.
  auth=runpy.run_path(str(repo/'tools/LegacyFst28CNumberInitialConsumerContext.Probe/audit_output.py'))['binding_preimages']
  b=auth(b)[0]
  marker=b'    // Genuine private prefix handoff; detached diagnostics are not accepted.'
  start=b.index(marker);end=b.index(b'    // Detached inspection cannot be accepted as a query/current-context receipt.',start);addition=b[start:end]
  assert len(addition.splitlines())==13 and sha(addition)=='a84f4d57852e015e525603e65f753495f12c7121f7a3ad63faa7ac1083159fc5'
  b=b[:start]+b[end:]
 assert sha(b)==row['sha256LF'],row['path']
assert sha(_restore_reviewed_source('LegacyFst28CNumberInitialPage.cs',(repo/'src/InkRecognition/LegacyFst28CNumberInitialPage.cs').read_bytes()).replace(b'\r\n',b'\n'))=='410187cde2b70c426511164b55420725c23552c5a54ec583e9b454b76bd8c5c5'

def defs(path,names):
 ns={'struct':struct};nodes=[n for n in ast.parse(path.read_text(encoding='utf-8-sig')).body if isinstance(n,(ast.FunctionDef,ast.ClassDef))and n.name in names];assert len(nodes)==len(names);exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),ns);return ns
alphabet=defs(repo/'tools/LegacyFst28CPrimaryMap.Probe/audit_output.py',{'alphabet'})['alphabet'];parse=defs(repo/'tools/LegacyFst13AOutgoing.Probe/audit_output.py',{'parse'})['parse']
avl=defs(repo/'tools/LegacySymbolContext.Probe/reference_controls.py',{'Node','height','update','left','right','add','case'})
lk=(models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();ak=(models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert sha(lk)=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'and sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
sn,se,source,reverse=alphabet(lk,299110);an,ae,acoustic,_=alphabet(ak,99);assert(sn,se,an,ae)==(16,299573,192,5233)
lookup={v:k for k,v in acoustic.items()};preserved=[65535 if reverse[i]==65535 else 65533 if source[i]==b'\0'else 65531 if source[i]==b' 'else lookup.get(source[i],65535)for i in range(sn)]
keys=[v for v in preserved if v not in(65533,65535)];aux=[-1 if v in(65533,65535)else keys.index(v)for v in preserved];assert len(keys)==15 and aux[0]==-1 and aux[1]==0
prefix=[65531,65529,65531,65532,65530];allocation=list(dict.fromkeys(keys+prefix));slot={v:i for i,v in enumerate(allocation)};tree=avl['case'](keys+prefix);bykey={r[0]:r for r in tree['nodes']}
expectedNodes=[]
for i,key in enumerate(allocation):
 _,left,right,balance=bykey[key];expectedNodes.append([i,key,-1 if left<0 else slot[left],-1 if right<0 else slot[right],balance,0,0,0])
assert len(expectedNodes)==18
states,words,desc=parse(lk[299576:301580],299576)
costs=runpy.run_path(str(repo/'tools/handwriting/reference_fst23c_outgoing.py'))['actual_main135_costs']()
assert sha(struct.pack('<256i',*costs))=='6db9bc77ac744b8bb25b71d42d15f2ef088d9273b660b2950cb9bca788406ba3'
def raw_rows(state):return [[t,desc[t][1],costs[desc[t][2]+128],desc[t][2],desc[t][3]]for t in words[desc[state][0]:desc[state+1][0]]]
assert raw_rows(0)==[[97,0,3454,51,0]]and [r[:2]for r in raw_rows(97)]==[[65,0],[69,14],[70,15]]
literal=runpy.run_path(str(repo/'tools/handwriting/reference_fst28c_number_first_page.py'))['first_page']
leaf_targets=words[desc[65][0]:desc[66][0]]+[69,70]
def flat(current):
 out=[]
 for target in leaf_targets:
  _,label,signed,byte7=desc[target];code=current[label]
  if code!=65535:out.append([target|(1<<32)|((byte7&1)<<41),aux[label],3454,code,0,0,0])
 return out
B=8*(80+16+128*32)+8*(80+12*(16+2))+12*16+128*32+6*64+3*64+16*(2+8)+8*32+80+1024
W=B+16*32+8*80+3*(12*16+64)+6*(128*64+128)+8*(128*64+64)+128*32+6*64+512
assert(B,W)==(42288,165168)

def verify(capture):
 assert capture['schema']==1 and capture['scope']=='complete-actual-first-normal-number-page-only'and capture['paging']is False and capture['generalQuery']is False
 assert(capture['bytes'],capture['work'])==(B,W)
 assert(capture['canonicalD0Owners'],capture['canonicalGlyphNetworkEvaluations'],capture['canonicalContextNetworkEvaluations'])==(4,16,3)
 assert(capture['additionalGuardD0Owners'],capture['additionalGuardGlyphNetworkEvaluations'],capture['additionalGuardContextNetworkEvaluations'])==(9,36,0)
 assert capture['additionalGuardPurposes']==['cancel-'+s for s in['after-claim','after-owned-copies','before-source-fill','after-source-fill','after-backend-call','after-complete-page','before-publish']]+['fault-after-backend-call','concurrent-claim']
 assert len(capture['cases'])==len(d0['cases'])==4
 for i,(row,upstream)in enumerate(zip(capture['cases'],d0['cases'])):
  current=[v if v>=192 or upstream['permissions'][v]&1 else 65535 for v in preserved]
  assert current==upstream['primary']==row['current']
  expected=literal(raw_rows,current,aux);assert expected['records']==flat(current)
  assert row['index']==i and row['records']==expected['records']and row['nodes']==expectedNodes and row['sourceNodes']==aux
  for key in['visible','backendCalls','sourceV80Fills','sourceRowsCopied','sourceNextCursor','outerDepthUsed','contextNodesAdded']:assert row[key]==expected[key],key
  assert row['calls']==[[c['state'],c['tag'],c['skip'],c['depth'],next(v['slot']for v in expected['cursorFinal']if v['state']==c['state']),*c['before'],*c['after'],c['returned']]for c in expected['calls']]
  assert row['cursorFinal']==[[c['state'],c['slot'],c['physical'],c['accepted'],len(raw_rows(c['state']))]for c in expected['cursorFinal']]
  # Independent actual32-byte packing; only context symbolic IDs normalized.
  raw=b''.join(struct.pack('<QqiHBBQ',tag,context,cost,symbol,category,flags,extra)for tag,context,cost,symbol,category,flags,extra in expected['records'])
  assert row['normalizedRecordHex']==raw.hex()
verify(j)
guards=['null-binding','cancel-before-null']+['byte-domain-'+str(n)for n in[-1,42289,2147483647]]+['work-domain-'+str(n)for n in[-1,165169,2147483647]]+['one-byte-short','one-work-short','cancel-before-claim','before-claim-reached','four-pairwise-private-header-identities','foreign-bootstrap-header']
for c in range(4):
 guards+=[s+'-'+str(c)for s in['same-actual-owner-header','first-page-only-budget','complete-call-fill-domains','historical-and-live-inputs-unchanged','replay','defensive-record-bytes','detached-record-value','zero-slot-not-null-context']]
 for name,values in [('record-index-',[-1,len(j['cases'][c]['records'])]),('call-index-',[-1,6]),('cursor-index-',[-1,3])]:guards+=[name+str(c)+'-'+str(v)for v in values]
 guards+=['inspection-canceled-'+str(c)]
for stage in['after-claim','after-owned-copies','before-source-fill','after-source-fill','after-backend-call','after-complete-page','before-publish']:
 if stage=='after-claim':guards+=['equal-data-independent-header-owner','equal-data-foreign-header-refused']
 guards +=[s+stage for s in['cancel-','reached-','poisoned-','no-input-mutation-']]
guards+=['fault-after-backend-call','fault-poisoned','one-concurrent-page-claim-winner','parallel-immutable-inspection-1024']
assert j['guards']==guards and len(set(guards))==len(guards)
# Exhaustive prepared-only numeric arithmetic controls never invoke factory or
# acquire a typed real-owner receipt. Independent direct flatten vs literalwalk.
distribution=[0]*13
for bits in range(4096):
 current=preserved.copy()
 for n,label in enumerate(list(range(1,11))+[14,15]):
  if not bits>>n&1:current[label]=65535
 t=literal(raw_rows,current,aux);assert t['records']==flat(current)and t['visible']==bits.bit_count();distribution[t['visible']]+=1
assert distribution==[math.comb(12,i)for i in range(13)]
# Explicit prepared zero-digit branch controls: actual native do-while refills
# even when the initial state65 block is empty. No D0/input/authority fabrication.
def premature_empty_calls(state,current):
 rows=[r for r in raw_rows(state)if current[r[1]]!=65535]
 calls=1
 for target,label,cost,signed,flag in rows:
  if current[label]==65533:calls+=premature_empty_calls(target,current)
 if rows:calls+=1 # Deliberately WRONG break-before-refill diagnostic.
 return calls
zero_digit_controls=0
for signs in range(4):
 current=preserved.copy()
 for label in range(1,11):current[label]=65535
 for bit,label in enumerate([14,15]):
  if not signs>>bit&1:current[label]=65535
 t=literal(raw_rows,current,aux)
 assert(t['visible'],t['backendCalls'],t['sourceV80Fills'],t['sourceRowsCopied'])==(signs.bit_count(),6,3,14)
 assert t['calls'][2]['state']==t['calls'][3]['state']==65 and t['calls'][2]['returned']==t['calls'][3]['returned']==0
 assert t['calls'][3]['skip']==0 and t['calls'][3]['before']==t['calls'][3]['after']==[10,0]
 assert t['records']==flat(current)and premature_empty_calls(0,current)==5
 zero_digit_controls+=1
assert zero_digit_controls==4
negative=[]
def reject(name,change):
 bad=copy.deepcopy(j);change(bad)
 try:verify(bad)
 except(AssertionError,KeyError,IndexError,ValueError):negative.append(name);return
 raise AssertionError('corruption accepted: '+name)
reject('drop-first-row',lambda v:v['cases'][0]['records'].pop(0))
reject('swap-output-order',lambda v:v['cases'][0]['records'].reverse())
reject('wrong-namespace',lambda v:v['cases'][0]['records'][0].__setitem__(0,v['cases'][0]['records'][0][0]^(4<<32)))
reject('propagated-root-flag',lambda v:v['cases'][0]['records'][0].__setitem__(0,v['cases'][0]['records'][0][0]|(1<<40)))
reject('targetflag-as-caseflags',lambda v:v['cases'][0]['records'][0].__setitem__(5,2))
reject('signedtag-as-cost',lambda v:v['cases'][0]['records'][0].__setitem__(2,51))
reject('zero-slot-treated-null',lambda v:v['cases'][0]['records'][0].__setitem__(1,-1))
reject('foreign-context',lambda v:v['cases'][0]['sourceNodes'].__setitem__(1,17))
reject('current-map-bypass',lambda v:v['cases'][0]['current'].__setitem__(4,preserved[4]))
reject('physical-used-as-skip',lambda v:v['cases'][0]['calls'][3].__setitem__(2,10))
reject('ring-owner-slot-swap',lambda v:v['cases'][0]['calls'][3].__setitem__(4,1))
reject('missing-final-refill',lambda v:v['cases'][0]['calls'].pop())
reject('constructor-cache-reuse',lambda v:v['cases'][0].__setitem__('sourceV80Fills',2))
reject('mutated-live-node',lambda v:v['cases'][0]['nodes'][0].__setitem__(1,999))
reject('normalized-byte-corruption',lambda v:v['cases'][0].__setitem__('normalizedRecordHex','00'+v['cases'][0]['normalizedRecordHex'][2:]))
print('PASS independent4 actual first pages/',sum(len(r['records'])for r in j['cases']),'records/',len(guards),'guards/',len(negative),'negative captures; all4096 prepared-only masks/4zero-digit old-break discriminators, full upstream19NN; no paging/accepted-text claim')
