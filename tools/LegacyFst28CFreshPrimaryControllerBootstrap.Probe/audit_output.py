"""Independent raw number/scalar oracle. Graph cardinalities are upstream inputs,
not independently reconstructed recognition-graph outputs. No vendor execution.
"""
import ast,copy,hashlib,json,runpy,struct,sys
from pathlib import Path
if len(sys.argv)!=3:raise SystemExit('MODELS CAPTURE')
models=Path(sys.argv[1]);capture_path=Path(sys.argv[2]);capture=json.loads(capture_path.read_bytes());tools=Path(__file__).resolve().parents[1]
sha=lambda raw:hashlib.sha256(raw).hexdigest()
lk=(models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();ak=(models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert sha(lk)=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
assert sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
def original_function(path,name):
 tree=ast.parse(path.read_text());fn=next(n for n in tree.body if isinstance(n,ast.FunctionDef)and n.name==name)
 namespace={'struct':struct};exec(compile(ast.Module(body=[fn],type_ignores=[]),str(path),'exec'),namespace);return namespace[name]
states,targets,descriptors=original_function(tools/'LegacyFst13AOutgoing.Probe/audit_output.py','parse')(lk[299576:301580],299576)
alphabet=original_function(tools/'LegacyFst28CPrimaryMap.Probe/audit_output.py','alphabet')
labels,end,tokens,reverse=alphabet(lk,299110)
assert (states,len(targets),labels,end)==(98,299,16,299573)
assert set(targets)==set(range(1,98))
assert all(cp in [0,32,43,44,45,46,*range(48,58)]for token in tokens.values()for cp in token)
costs=runpy.run_path(str(tools/'handwriting/reference_fst23c_outgoing.py'))['actual_main135_costs']()
assert sha(struct.pack('<256i',*costs))=='6db9bc77ac744b8bb25b71d42d15f2ef088d9273b660b2950cb9bca788406ba3'
negative=any(costs[descriptors[state][2]+128]<0 for state in range(1,98));assert not negative
header=1|(int(negative)<<1) # actual trueV58 + descriptor-stateV60; .number has category0.
weight24=struct.unpack_from('<I',lk,299092)[0]/65536
weight28=65536/65536 # literal139/v301 default from pinned1D6700.
assert struct.unpack_from('<I',lk,299096)[0]==0x101
f32bits=lambda value:struct.unpack('<I',struct.pack('<f',value))[0]
expected_source=[header,f32bits(weight24),f32bits(weight28),0,1,0xffffffff,1]
first,last=descriptors[0][0],descriptors[1][0];assert last-first==1;target=targets[first];_,label,tag,flag=descriptors[target]
root_expected=[target,label,65533,65533,costs[tag+128],tag,flag,None]
expected_constructor=[True,True,True,True,0,0,8,128,0,True,False]
guards=['same-exact-initial-graph-source-policy-cohort','pure-max-count-calculator-no-graph-claim','exact-whole-byte-and-work-budget','null-initial','null-graph','short-ak','wrong-ak-kind','wrong-ak-alphabet','byte-domain--1','byte-domain-580474','byte-domain-2147483647','work-domain--1','work-domain-969116','work-domain-2147483647','one-byte-short','one-work-short','count-cap-0-1-1-8','count-cap-65536-1-1-8','count-cap-1-0-1-8','count-cap-1-65536-1-8','count-cap-1-1-0-8','count-cap-1-1-262145-8','count-cap-1-1-1-0','count-cap-1-1-1-9','count-cap-2147483647-2147483647-2147483647-8','cancel-before-entry','cancel-null-invalid-first','cancel-source-snapshot','cancel-constructor-snapshot','cancel-stage-before-snapshot','cancel-stage-after-snapshot','cancel-stage-after-hash','cancel-stage-after-graph','cancel-stage-after-scan','cancel-stage-before-publish','after-snapshot-caller-mutation-safe','checkpoint-throw-no-receipt','same-ak-and-equal-root-independent-issuer','defensive-value-snapshots','fifteen-null-payload-slots-not-empty-aux','caller-mutation-gc','parallel-immutable-1024']
retained=6*8+24+24+80+72
whole=len(ak)+2580+len(targets)*16+states+retained
fixed=len(ak)+2580+states+len(targets)+states+labels+15+32

def audit(c):
 assert c['schema']==1 and c['logicalBytes']==whole==580473 and c['fixedWork']==fixed==575901
 assert c['maxWork']==fixed+65535+65535+262144==969115
 assert c['initialRootRow']==root_expected
 assert c['guards']==guards and len(guards)==42
 assert len(c['cases'])==4
 for i,row in enumerate(c['cases']):
  assert row['index']==i and row['kind']=='procedural-owning-graph-not-word-label'
  n,g,r,cap=[row[k]for k in ['nodes','groups','arcs','cap']]
  assert 1<=n<=65535 and 1<=g<=65535 and 1<=r<=262144 and 1<=cap<=8
  assert row['work']==fixed+n+g+r and row['work']<=c['maxWork']
  assert row['source']==expected_source and row['constructor']==expected_constructor
  assert row['sameInitial']is True and row['sameGraph']is True and row['sameSource']is True and row['hasIssuedQuery']is False
 assert all(c[k]is False for k in ['queryImplemented','historyImplemented','ready','fullrootDefault','numberOCR'])
audit(capture)
mutations=[lambda c:c['cases'][0]['source'].__setitem__(0,2),lambda c:c['cases'][0]['source'].__setitem__(6,5),lambda c:c['cases'][0]['constructor'].__setitem__(10,True),lambda c:c['cases'][0]['constructor'].__setitem__(1,False),lambda c:c['cases'][0].__setitem__('sameSource',False),lambda c:c['cases'][0].__setitem__('work',c['cases'][0]['work']-1),lambda c:c['initialRootRow'].__setitem__(4,3453),lambda c:c['guards'].pop(),lambda c:c.__setitem__('ready',True)]
for mutation in mutations:
 c=copy.deepcopy(capture);mutation(c)
 try:audit(c)
 except AssertionError:pass
 else:raise AssertionError('Corrupted capture accepted')
print(json.dumps({'status':'PASS','actualStates':states,'actualRows':len(targets),'allDescriptorCostStates':len(set(targets)),'genuineGraphCases':len(capture['cases']),'graphCountsProvenance':'captured genuine typed-owner cardinalities, not independently reconstructed graph algorithm','guards':len(guards),'negativeCaptures':len(mutations),'captureSha256':sha(capture_path.read_bytes()),'logicalBytes':whole,'queryOrOCRClaimed':False},indent=2))
