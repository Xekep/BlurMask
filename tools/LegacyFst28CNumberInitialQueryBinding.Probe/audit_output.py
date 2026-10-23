"""Independent raw-source AVL/reset/first-prefix oracle. No28C execution claim.
MODELS CAPTURE COMPLETED_D0_CAPTURE: full oldD0 independent numerical proof runs first.
"""
import ast,copy,hashlib,json,runpy,struct,sys
from pathlib import Path
if len(sys.argv)!=4:raise SystemExit('MODELS CAPTURE COMPLETED_D0_CAPTURE')
models=Path(sys.argv[1]);capturePath=Path(sys.argv[2]);d0Path=Path(sys.argv[3]);capture=json.loads(capturePath.read_bytes());d0=json.loads(d0Path.read_bytes())
repo=Path(__file__).resolve().parents[2];sha=lambda b:hashlib.sha256(b).hexdigest()
# Existing independent19NN/features/weights/permissions proof, not an opaque winner hash.
oldArgs=sys.argv;sys.argv=[str(repo/'tools/LegacyFst28CNumberInitialD0.Probe/audit_output.py'),str(models),str(d0Path)]
try:runpy.run_path(sys.argv[0],run_name='__main__')
finally:sys.argv=oldArgs
pins=json.loads((repo/'tools/handwriting/linguistic-fst28c-number-live-query-pins.json').read_bytes())
# Frozen STATIC pins retain pre-accessor source identity. Verify exact reviewed
# 13-line addition then recover preimage ONLY in memory; never edit old pins.
for row in pins['runtimeReviewedLF']+pins['independentDefinitionsLF']:
 b=(repo/row['path']).read_bytes().replace(b'\r\n',b'\n')
 if row['path'].endswith('LegacyFst28CFreshPrimaryControllerBootstrap.cs'):
  marker=b'    // Trusted readonly seam for the new owning active-context overlay.'
  start=b.index(marker);end=b.index(b'    internal bool IsSameIssuer(',start);addition=b[start:end]
  assert len(addition.splitlines())==13 and sha(addition)=='9de04f4679c9343b7da2b03fee091af2ee36431d8fadd5b350535098b3ac105d'
  b=b[:start]+b[end:]
 assert sha(b)==row['sha256LF'],row['path']
# Accept only the frozen old source or the exact approved readonly13-line
# successor. Recover old pin in memory; historical captures remain unchanged.
def binding_preimage(data):
 b=data.replace(b'\r\n',b'\n')
 marker=b'    // Genuine private prefix handoff; detached diagnostics are not accepted.'
 if marker in b:
  assert sha(b)=='dcfb63ac94035f1c409c56ffa89d9284f557c2b5646deaa78277936df29006e8'
  start=b.index(marker);end=b.index(b'    // Detached inspection cannot be accepted as a query/current-context receipt.',start);addition=b[start:end]
  assert len(addition.splitlines())==13 and sha(addition)=='a84f4d57852e015e525603e65f753495f12c7121f7a3ad63faa7ac1083159fc5'
  b=b[:start]+b[end:]
 assert sha(b)=='859a58db8706c1c292362c28c80d08646f51dc86574d799359c76d5efd96fb8f'
 return b
binding_preimage((repo/'src/InkRecognition/LegacyFst28CNumberInitialQueryBinding.cs').read_bytes())
def defs(path,names):
 ns={'struct':struct};nodes=[n for n in ast.parse(path.read_text(encoding='utf-8-sig')).body if isinstance(n,(ast.FunctionDef,ast.ClassDef))and n.name in names];assert len(nodes)==len(names);exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),ns);return ns
alphabet=defs(repo/'tools/LegacyFst28CPrimaryMap.Probe/audit_output.py',{'alphabet'})['alphabet']
avl=defs(repo/'tools/LegacySymbolContext.Probe/reference_controls.py',{'Node','height','update','left','right','add','case'})
parse=defs(repo/'tools/LegacyFst13AOutgoing.Probe/audit_output.py',{'parse'})['parse']
lk=(models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();ak=(models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert sha(lk)=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'and sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
sn,se,source,reverse=alphabet(lk,299110);an,ae,acoustic,_=alphabet(ak,99);assert(sn,se,an,ae)==(16,299573,192,5233)
lookup={v:k for k,v in acoustic.items()};preserved=[65535 if reverse[i]==65535 else 65533 if source[i]==b'\0'else 65531 if source[i]==b' 'else lookup.get(source[i],65535)for i in range(sn)]
secondary=[65533 if v==65533 else 65531 if v==65531 else 65535 for v in preserved]
keys=[v for v in preserved if v not in(65533,65535)];prefixes=[65531,65529,65531,65532,65530];allocation=list(dict.fromkeys(keys+prefixes));assert len(keys)==15 and len(allocation)==18
slot={v:i for i,v in enumerate(allocation)};tree=avl['case'](keys+prefixes);nodesByKey={r[0]:r for r in tree['nodes']}
expectedNodes=[]
for i,key in enumerate(allocation):
 _,left,right,balance=nodesByKey[key];expectedNodes.append([i,key,-1 if left<0 else slot[left],-1 if right<0 else slot[right],balance,0,0,0])
sourceNodes=[-1 if v in(65533,65535)else slot[v]for v in preserved];existingSlots=[-1 if v in(65533,65535)else keys.index(v)for v in preserved]
aux=[[i,key,slot[key],0,0]for i,key in enumerate(keys)];prefixSlots=[slot[v]for v in prefixes]
reset=[[0xc8,4,-1]]+[[off+i*0x50,n,-1]for i in range(7)for off,n in[(0x100,8),(0x110,8),(0x118,4)]]+[[0x330,8,-1],[0x340,8,-1],[0x348,4,0]]
assert len(reset)==25 and all(a+n<=b or b+m<=a for i,(a,n,v)in enumerate(reset)for b,m,w in reset[:i])
request=[8<<40,1,1,1,1,1,1,0,0,0,0,0,128,1,1]
B=4080+256+18*(56+8)+3*16*2+16*8+16*4+15*16+5*(2+8)+72+80+25*16+8*(80+12*12)+256+288
C=3*16*2+16*8+16*4+15*16+5*(2+8)+25*16+18*(56+8)+72+80+256+288
W=20*(18*16+64)+C+2*18*18*8+18*56+25*25*8+16*15+64+15*56
assert(B,C,W)==(8954,2826,22202)
# Raw root topology is preserved evidence, not a fabricated live page.
states,targets,descriptors=parse(lk[299576:301580],299576)
assert targets[descriptors[0][0]:descriptors[1][0]]==[97]and targets[descriptors[97][0]:descriptors[98][0]]==[65,69,70]
assert [descriptors[t][1]for t in targets[descriptors[65][0]:descriptors[66][0]]]==list(range(1,11))

def verify(j):
 assert j['schema']==1 and j['scope']=='after-D0-library-active-overlay-first-prefix-only'and j['query']is False and j['paging']is False
 assert(j['bytes'],j['work'])==(B,W)
 assert(j['canonicalD0Owners'],j['canonicalSelectedGroups'],j['canonicalGlyphNetworkEvaluations'],j['canonicalContextNetworkEvaluations'])==(4,4,16,3)
 assert(j['additionalGuardD0Owners'],j['additionalGuardSelectedGroups'],j['additionalGuardGlyphNetworkEvaluations'],j['additionalGuardContextNetworkEvaluations'])==(7,7,28,0)
 assert j['additionalGuardPurposes']==['distinct-d0-same-bootstrap','cancel-after-claim','cancel-after-map-copies','cancel-after-source-nodes','cancel-before-publish','fault-after-source-nodes','concurrent-claim']
 assert len(j['cases'])==len(d0['cases'])==4
 for index,(row,old)in enumerate(zip(j['cases'],d0['cases'])):
  assert row['index']==index and(row['capacity'],row['cursors'],row['next'])==(12,8,0)
  assert row['preserved']==preserved and row['secondary']==secondary and row['current']==old['primary']
  # Independent oldD0 oracle already derived permissions/refresh from raw models.
  assert row['current']==[v if v>=192 or old['permissions'][v]&1 else 65535 for v in preserved]
  assert row['root']==slot[tree['root']]and row['nodes']==expectedNodes
  assert row['sourceNodes']==sourceNodes and row['existingSlots']==existingSlots
  assert row['prefixNodes']==prefixSlots and row['auxiliaries']==aux
  assert row['resetWrites']==reset and row['request']==request
verify(capture)
guards=['null-d0','cancel-before-null']
guards+=['byte-domain-'+str(v)for v in[-1,8955,2147483647]]+['work-domain-'+str(v)for v in[-1,22203,2147483647]]
guards+=['one-byte-short','one-work-short','foreign-bootstrap-d0','four-independent-private-header-graph-owners','cancel-before-claim','before-claim-reached']
for i in range(4):guards +=[s+'-'+str(i)for s in['exact-owner-cohort','private-issued-prefix-observable','bounded-inspection-only','constructor-views-remain-historical','same-d0-replay']]
guards+=['real-distinct-d0-same-bootstrap','different-d0-same-bootstrap-replay']
for stage in['after-claim','after-map-copies','after-source-nodes','before-publish']:
 if stage=='after-claim':guards +=['equal-data-foreign-bootstrap-private-header','equal-data-foreign-bootstrap-d0']
 guards +=[s+stage for s in['cancel-','reached-','poisoned-','no-input-mutation-']]
guards+=['fault-after-source-nodes','fault-poisoned','one-concurrent-bootstrap-claim-winner','readonly-node-list','immutable-node-value','immutable-auxiliary-value','immutable-reset-value']
for name,values in [('aux-index-',[-1,15]),('reset-index-',[-1,25]),('prefix-index-',[-1,5])]:guards +=[name+str(v)for v in values]
for bad in[16,192,65535]:guards +=[name+str(bad)for name in['preserved-label-','current-label-','secondary-label-','source-node-label-','slot-label-']]
guards+=['header-inspection-cancel','request-inspection-cancel','parallel-immutable-inspection-1024']
assert capture['guards']==guards and len(guards)==85
# Correlated corruption controls: each must fail exact independent schema/AVL/map.
negatives=[]
def reject(name,mutate):
 bad=copy.deepcopy(capture);mutate(bad)
 try:verify(bad)
 except(AssertionError,KeyError,IndexError):negatives.append(name);return
 raise AssertionError('corruption accepted: '+name)
reject('zero-query-tag',lambda j:j['cases'][0]['request'].__setitem__(0,0))
reject('foreign-header',lambda j:j['cases'][0]['request'].__setitem__(2,0))
reject('wrong-private-cohort',lambda j:j['cases'][0]['request'].__setitem__(4,0))
reject('nonzero-issued-span',lambda j:j['cases'][0]['request'].__setitem__(9,1))
reject('permission-map-corruption',lambda j:j['cases'][0]['current'].__setitem__(1,65535))
reject('preserved-space-bypass',lambda j:j['cases'][0]['preserved'].__setitem__(11,180))
reject('dropped-source-slot',lambda j:j['cases'][0]['existingSlots'].__setitem__(1,-1))
reject('space-independent-node',lambda j:j['cases'][0]['prefixNodes'].__setitem__(0,15))
reject('nonempty-source-payload',lambda j:j['cases'][0]['auxiliaries'][0].__setitem__(3,1))
reject('wrong-avl-balance',lambda j:j['cases'][0]['nodes'][0].__setitem__(4,7))
reject('wrong-avl-child',lambda j:j['cases'][0]['nodes'][0].__setitem__(2,17))
reject('constructor-state0-afterreset',lambda j:j['cases'][0]['resetWrites'][0].__setitem__(2,0))
reject('overlap-q340-ring348',lambda j:j['cases'][0]['resetWrites'][-2].__setitem__(1,12))
reject('invent-untouched-count',lambda j:j['cases'][0]['resetWrites'].append([0xcc,4,0]))
reject('promote-query-authority',lambda j:j.__setitem__('query',True))
print(f'PASS independent4raw-source AVL/mapping/request/reset owners/18nodes/15slots/25stores/85guards/{len(negatives)}negativecaptures; B{B}/W{W};19canonicalNN upstream verified,28extra guardglyphNN separately scoped; noQuery/paging')
