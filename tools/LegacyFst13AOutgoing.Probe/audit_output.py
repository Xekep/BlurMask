"""Independent raw-byte13A.V80 oracle; no28C label mapping or recognition.

Actual graph and sourceKind/weight come from exact resource bytes; literal
float32 reference is separately native-pinned. Synthetic graph is constructed
from a fixed specification rather than from captured rows or costs.
"""
import hashlib,json,runpy,struct,sys
from pathlib import Path
if len(sys.argv)!=3:raise SystemExit('MODELS CAPTURE')
models=Path(sys.argv[1]);capture=json.loads(Path(sys.argv[2]).read_bytes())
sha=lambda b:hashlib.sha256(b).hexdigest()
root=(models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();ak=(models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert len(root)==6200992 and sha(root)=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
assert len(ak)==572763 and sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<I',ak,5233)[0]==0x135
assert struct.unpack_from('<IIIHH',root,299000)==(0x101fe90,2572,0x13a,0,0x202)
assert struct.unpack_from('<I',root,299100)[0]==0x3f800000
reference=runpy.run_path(str(Path(__file__).resolve().parents[1]/'handwriting/reference_fst23c_outgoing.py'))
costs=reference['actual_main135_costs']();assert sha(struct.pack('<256i',*costs))=='6db9bc77ac744b8bb25b71d42d15f2ef088d9273b660b2950cb9bca788406ba3'

def synthetic(narrow):
 b=bytearray(93);struct.pack_into('<HHIiB',b,0,0x109,254 if narrow else 65535,5,7,0xe7)
 for i,(offset,label,tag)in enumerate(zip([0,5,5,6,7,7],[0,65535,255,1,65534,42],[128,129,0,1,127,255])):
  at=13+i*8;struct.pack_into('<I',b,at,offset)
  if narrow:struct.pack_into('<BBBB',b,at+4,0 if i==0 else 255 if i==1 else i+10,tag,0xa0+i,0xfa)
  else:struct.pack_into('<HBB',b,at+4,label,tag,0xa0+i)
 struct.pack_into('<I7I',b,61,0xdeadbeef,0,1,1,2,3,4,2)
 return b

def parse(raw,origin):
 version,labels,states,words=struct.unpack_from('<HHIi',raw);assert version==0x109
 start=12+(-origin-12)%4;descriptors=[]
 for i in range(states+1):
  at=start+i*8;offset=struct.unpack_from('<I',raw,at)[0]
  if labels<255:
   label,tag,flag=struct.unpack_from('<BBB',raw,at+4);label=65535 if label==255 else label
  else:label,tag,flag=struct.unpack_from('<HBB',raw,at+4)
  descriptors.append((offset,label,tag if tag<128 else tag-256,flag))
 at=start+(states+1)*8+4;assert len(raw)==at+words*4
 targets=list(struct.unpack_from('<'+'I'*words,raw,at));assert descriptors[-1][0]==words
 assert all(descriptors[i][0]<=descriptors[i+1][0]<=words for i in range(states))and all(t<states for t in targets)
 return states,targets,descriptors

custom=[(i-128)*12345+77 for i in range(256)]
actual=root[299576:301580]
spec=[('actual-number',actual,299576,costs,True),('diagnostic-actual-table',actual,299576,costs,False)]
for narrow in [False,True]:
 name='synthetic-narrow'if narrow else'synthetic-wide';raw=synthetic(narrow)
 spec.extend([(name,raw,3,custom,False),(name+'-null',raw,3,None,False)])
assert capture['schema']==1 and capture['sourceCount']==len(capture['sources'])==len(spec)==6
guards=['actual-cost-output-mutation'];totalstates=totalrows=0;negativeunsigned=negativecenter=negativesource=0
for row,(name,raw,origin,table,authority)in zip(capture['sources'],spec):
 states,targets,descriptors=parse(raw,origin)
 assert row['name']==name and row['wireSha']==sha(raw)
 assert row['actualAuthority']==row['rootReceipt']==row['childReceipt']==authority
 if authority:assert row['acousticSha']==sha(ak)
 else:assert'acousticSha'not in row
 assert row['costs']==([]if table is None else table)and row['hasCostTable']==(table is not None)
 assert(row['stateCount'],row['wordCount'])==(states,len(targets))and len(row['states'])==states
 for state,out in enumerate(row['states']):
  assert out['state']==state and out['outputFlag']==0
  expected=[]
  for target in targets[descriptors[state][0]:descriptors[state+1][0]]:
   _,label,tag,flag=descriptors[target];cost=0 if table is None else table[tag+128]
   expected.append([target,label,cost,tag,flag])
   if table is not None:
    negativeunsigned+=cost!=table[tag%256]
    negativecenter+=cost!=table[(tag+127)%256]
    negativesource+=cost!=table[descriptors[state][2]+128]
  assert out['rows']==expected,(name,state,out['rows'],expected)
  totalrows+=len(expected);totalstates+=1
  guards.extend([f'{name}-row-negative-{state}',f'{name}-row-end-{state}',f'{name}-copy-canceled-{state}'])
 if name=='diagnostic-actual-table':guards.append('diagnostic-no-actual-authority')
 if name in ['synthetic-wide','synthetic-narrow']:guards.append('wide-caller-mutation'if name=='synthetic-wide'else'narrow-caller-mutation')
guards+=['foreign-root-identity','generic-wire-no-receipt','wrong-acoustic-length','wrong-acoustic-content','null-root','null-wire','actual-whole-budget','negative-budget','excess-budget','short-table','long-table','query-sentinel','query-uintmax','negative-row-budget','excess-row-budget','negative-row-byte-budget','excess-row-byte-budget','row-budget','row-byte-budget','exact-row-budgets','cancel-actual','cancel-diagnostic','cancel-null-first','cancel-query','cancel-cost-copy','offset-decrease','offset-out-of-range','bad-target','max-target','sentinel-disagreement','retained-lifetime-after-caller-mutation-gc']
guards[-1:-1]=['empty-graph-owned','empty-graph-no-state','cancel-null-cost-copy','nonzero-first-offset-allowed','unused-prefix-target-whole-validation','zero-outgoing-zero-budgets']
assert capture['guards']==guards and len(guards)==689
assert capture['stateCount']==totalstates==216 and capture['rowCount']==totalrows==626
assert capture['query28CImplemented']is False and capture['field20IsInitial']is False
assert negativeunsigned>0 and negativecenter>0 and negativesource>0
print(f'PASS independent6 sources/216 states/626 rows/689 guards; all actual98/299/256costs; incorrect unsigned/centering/source-tag controls differ in {negativeunsigned}/{negativecenter}/{negativesource} rows; no28C/recognition')
