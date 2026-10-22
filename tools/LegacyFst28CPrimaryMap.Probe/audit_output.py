"""Independent actual-byte primary28C conversion oracle; not a28C query.
Derives16 entries and299 projections from stored alphabet/graph bytes, never
uses captured entries to derive expected labels or source costs.
"""
import hashlib,json,runpy,struct,sys
from pathlib import Path
if len(sys.argv)!=3:raise SystemExit('MODELS CAPTURE')
models=Path(sys.argv[1]);capture=json.loads(Path(sys.argv[2]).read_bytes());sha=lambda b:hashlib.sha256(b).hexdigest()
lk=(models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();ak=(models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert sha(lk)=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'and sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<IIIHH',lk,299000)==(0x101fe90,2572,0x13a,0,0x202)
assert struct.unpack_from('<I',ak,5233)[0]==0x135
assert struct.unpack_from('<IH',lk,299104)==(0x2b,0x103)and struct.unpack_from('<I',lk,299096)[0]==257

def alphabet(raw,start):
 pos=start
 def read(fmt,n=1):
  nonlocal pos
  values=struct.unpack_from('<'+fmt*n,raw,pos);pos+=struct.calcsize('<'+fmt)*n
  return values[0]if len(values)==1 else list(values)
 header=read('H',8);count=header[-1];n=read('H')
 for extra in[32,64,1]:
  table=read('H',n);table=[table]if n==1 else table;n=next(v+extra for v in reversed(table)if v!=65535)
 spans=read('H',n+1);entries=[read('II')for _ in range(spans[-1]+1)];strings=raw[pos:pos+entries[-1][0]];pos+=entries[-1][0]
 tokens={label&65535:strings[off:entries[i+1][0]]for i,(off,label)in enumerate(entries[:-1])};reverse=read('H',count)
 flag=read('B');pos+=(count+7)//8 if flag else 0;triplets=read('H');pos+=triplets*6
 if triplets:
  flags=read('I')
  for bit in[1,2,4]:pos+=count*(1 if triplets<128 else 2)if flags&bit else 0
 return count,pos,tokens,reverse
sn,se,source,reverse=alphabet(lk,299110);an,ae,acoustic,_=alphabet(ak,99)
assert(sn,se,an,ae)==(16,299573,192,5233)
lookup={text:label for label,text in acoustic.items()};expected=[];direct_only_differences=0
for label in range(sn):
 text=source[label]
 if reverse[label]==65535:code=65535
 elif text==b'\0':code=65533
 elif text==b' ':code=65531
 else:code=lookup.get(text,65535)
 kind='AcousticLabel'if code<an else'Missing'if code==65535 else'Special'
 expected.append([label,code,kind]);direct_only_differences+=code!=lookup.get(text,65535)
assert capture['schema']==1 and capture['entries']==expected
assert capture['acousticSha']==sha(ak)and capture['rootSha']==sha(lk)
assert(capture['preparationWork'],capture['sourceFlags68'],capture['sourceAlphabetOffset'],capture['sourceAlphabetEnd'],capture['sourceLabels'],capture['acousticLabels'])==(575551,257,299110,299573,16,192)
assert direct_only_differences==2 and expected[0][1]==65533 and expected[1][1]==0 and expected[11][1]==65531 and lookup[b' ']==180
costs=runpy.run_path(str(Path(__file__).resolve().parents[1]/'handwriting/reference_fst23c_outgoing.py'))['actual_main135_costs']()
assert sha(struct.pack('<256i',*costs))=='6db9bc77ac744b8bb25b71d42d15f2ef088d9273b660b2950cb9bca788406ba3'
assert len(capture['states'])==capture['stateCount']==98;total=0
for state,row in enumerate(capture['states']):
 first=struct.unpack_from('<I',lk,299588+state*8)[0];last=struct.unpack_from('<I',lk,299588+(state+1)*8)[0]
 assert row['state']==state and row['outputFlag']==0 and len(row['rows'])==last-first
 for ordinal,index in enumerate(range(first,last)):
  target=struct.unpack_from('<I',lk,300384+index*4)[0];label,tag,byte7=struct.unpack_from('<BBB',lk,299588+target*8+4);signed=tag if tag<128 else tag-256
  assert row['rows'][ordinal]==[ordinal,target,label,expected[label][1],expected[label][2],costs[signed+128],signed,byte7]
  total+=1
assert total==capture['rowCount']==299
guards=['exact-source-root-child-identity','actual-source-alphabet-provenance','equal-content-independent-owner-identity','foreign-root-outgoing-refused','diagnostic-source-refused','diagnostic-null-cost-source-refused','generic-wire-source-refused','wrong-root-actual-adapter-refused','wrong-ak-length','wrong-ak-source-kind-content','wrong-ak-alphabet-content','null-source','negative-preparation-budget','excess-preparation-budget','insufficient-preparation-budget','cancel-create','cancel-null-first','cancel-invalid-snapshot-first','cancel-copy','label-16','label-192','label-255','label-65535','defensive-entry-copy','caller-snapshot-mutation-gc','parallel-immutable-1024','nul-versus-digit-zero','space-special-precedence']
assert capture['guards']==guards and len(guards)==28
assert capture['secondaryImplemented']is False and capture['query28CImplemented']is False and capture['historyAssumedEmpty']is False
print('PASS independent16 actual primary entries/98states/299 projections/28guards; every target/label/code/cost/tag/flag exact; direct-only lookup fails2special entries; no secondary/cursor/history/28Cquery')
