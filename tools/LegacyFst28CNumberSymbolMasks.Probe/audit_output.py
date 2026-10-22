"""Independent raw-model source/M-mask oracle. No vendor DLL loading or execution."""
from pathlib import Path
import argparse,ast,struct,json,hashlib,copy
p=argparse.ArgumentParser();p.add_argument('models',type=Path);p.add_argument('capture',type=Path);a=p.parse_args();R=Path(__file__).resolve().parents[2]
ak=(a.models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes();lk=(a.models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert hashlib.sha256(lk).hexdigest()=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
# Original independent stored AL parser, shared with earlier primary-map proof.
node=next(n for n in ast.parse((R/'tools/LegacyFst28CPrimaryMap.Probe/audit_output.py').read_text()).body if isinstance(n,ast.FunctionDef)and n.name=='alphabet');ns={'struct':struct};exec(compile(ast.Module(body=[node],type_ignores=[]),'original-al','exec'),ns)
sn,se,source,_=ns['alphabet'](lk,299110);an,ae,acoustic,reverse=ns['alphabet'](ak,99);assert(sn,se,an,ae)==(16,299573,192,5233)
# Independently traverse the complete variable v110 prefix before interpreting C8.
pos=189734+20
assert struct.unpack_from('<I',ak,189742)[0]==0x21d and struct.unpack_from('<H',ak,189748)[0]==0x110
def read(fmt,n=1):
 global pos
 size=struct.calcsize('<'+fmt)*n;assert 0<=n<=1048576 and size<=572573-pos
 result=struct.unpack_from('<'+fmt*n,ak,pos);pos+=size;return result[0]if n==1 else list(result)
def skip(n):read('B',n)
def flag():
 v=read('B');assert v in [0,1];return v
def net():
 assert(read('H'),read('H'))==(0xfea0,0x106);length=read('I');skip(length)
def shorts(copies):
 count=read('H');assert count<=512;skip(count*copies*2)
assert flag()==1;net();skip(3*4+2+2+4+1);assert read('B')==2;skip(2);assert flag()==1;read('H');skip(9*4);assert read('B')==1;assert flag()==0;skip(2+4+5*2+7*4);shorts(1);shorts(2);shorts(2);skip(2+2+4+4+2+2);assert flag()==0
if flag():shorts(2)
for i in range(7):
 if flag():shorts(2)
 flag();flag();mode=read('B');assert mode<=2
 if mode==0:read('f')
 for k in range(3):
  if flag():net()
assert pos==566330;skip(1+4+1+8);lists=read('H');assert lists<=512
for i in range(lists):skip(read('H')*2)
skip(6*4);assert flag()==0;skip(8);assert flag()==0;assert read('h')==0 and pos==566382
lookup={v:k for k,v in acoustic.items()};preserved=[65533 if source[i]==b'\0'else 65531 if source[i]==b' 'else lookup.get(source[i],65535)for i in range(16)]
seed={code for code in preserved if code<192};primary=set(seed);secondary=set(range(192))
# Actual K0; constructor-only restriction absent; contextual/wide empty as static proof.
for text in [b'-',b'\xc2\xad']:
 if text in lookup:primary.add(lookup[text])
virtual={i for i,v in enumerate(reverse)if v==65535};primary-=virtual;secondary-=virtual
header=struct.unpack_from('<8H',ak,99)
for mask in [primary,secondary]:
 if header[1]!=65535:
  if any(lookup[t]in mask for t in [b'-',b'_',b'\xf3\xb0\x80\x82',b'\xf3\xb0\x80\x83',b'\xe3\x83\xbc',b'\xe4\xb8\x80']if t in lookup):mask.add(header[1])
  else:mask.discard(header[1])
 if header[2]!=65535:
  if lookup.get(b'\xf3\xb0\x80\x81')in mask:mask.add(header[2])
  else:mask.discard(header[2])
 for i in [3,0,4,5,6]:
  if header[i]!=65535:mask.add(header[i])
expected={'sourceTokens':list(range(16)),'preserved':preserved,'seed':[int(i in seed)for i in range(192)],'primary':[int(i in primary)for i in range(192)],'secondary':[int(i in secondary)for i in range(192)]}
logical=572763+4*(16*4+16*2)+8*192+4*5134+256;work=2*572763+2*5134+16*192+8*192+16*4+512
required=['four-independent-graph-bootstrap-issuers','exact-bootstrap-initial-source-policy-graph-identity','exact-whole-logical-and-work-budgets','null-bootstrap','short-resource','empty-resource']
required+=['wrong-resource-byte-'+str(i)for i in [0,99,5233,189742,189748,566380,572762]]
required+=['byte-domain-'+str(i)for i in [-1,595476,2147483647]]+['work-domain-'+str(i)for i in [-1,1160979,2147483647]]
required+=['one-byte-short','one-work-short','cancel-before-entry','cancel-before-invalid']
required+=['cancel-stage-'+x for x in ['before-snapshot','after-snapshot','after-hash','after-alphabet','after-seed','after-masks','before-publish']]
required+=['cancel-copy-'+x for x in ['tokens','preserved','seed','primary','secondary']]
required+=['throw-before-publish-no-owner','equal-content-foreign-initial-not-merged','after-snapshot-input-mutation','all-five-output-copies-defensive','parallel-1024-immutable-inspections','source-initial-aux15-query-state-unchanged','caller-mutation-gc-inspection']
def audit(j):
 assert j['schema']==1 and j['logicalBytes']==logical and j['work']==work
 assert len(j['cases'])==4 and set(j['guards'])==set(required)and len(j['guards'])==len(required)
 for i,row in enumerate(j['cases']):
  assert row['index']==i and row['kind']=='procedural-owning-graph-not-OCR'
  assert 1<=row['nodes']<=65535 and 1<=row['groups']<=65535 and 1<=row['arcs']<=262144 # actual upstream typed counters, not reconstructed graph
  for key,values in expected.items():assert row[key]==values,(i,key)
  for key in ['sameBootstrap','sameInitial','sameSource','sameGraph','samePolicy']:assert row[key]is True
  for key in ['queryAuthority','d0PermissionAuthority','bootstrapIssued']:assert row[key]is False
 for key in ['queryImplemented','permissionRefreshImplemented','D0Implemented','numberOCR','fullrootDefault']:assert j[key]is False
j=json.loads(a.capture.read_text());audit(j);negatives=0
for key,index in [('sourceTokens',0),('preserved',0),('seed',0),('primary',0),('secondary',0)]:
 bad=copy.deepcopy(j);bad['cases'][0][key][index]^=1
 try:audit(bad)
 except AssertionError:negatives+=1
 else:raise AssertionError('Accepted wrong '+key)
for key in ['sameBootstrap','sameGraph','d0PermissionAuthority','queryAuthority']:
 bad=copy.deepcopy(j);bad['cases'][0][key]=not bad['cases'][0][key]
 try:audit(bad)
 except AssertionError:negatives+=1
 else:raise AssertionError('Accepted wrong owner/authority '+key)
for key in ['logicalBytes','work']:
 bad=copy.deepcopy(j);bad[key]-=1
 try:audit(bad)
 except AssertionError:negatives+=1
 else:raise AssertionError('Accepted wrong budget '+key)
print(json.dumps(dict(scope='raw source/AL/v110/group/header numerical oracle; not graph algorithm or OCR quality',cases=4,guards=len(required),negativeCaptures=negatives,sourceLabels=16,acousticLabels=192,knowledgeGroups=0,seedEnabled=len(seed),primaryEnabled=len(primary),secondaryEnabled=len(secondary),captureSha256=hashlib.sha256(a.capture.read_bytes()).hexdigest())))
