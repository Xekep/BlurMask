from pathlib import Path
import copy,hashlib,json,subprocess,sys
D=Path(__file__).resolve().parent;R=Path(sys.argv[1]);C=Path(sys.argv[2]);O=Path(sys.argv[3]);assert not O.exists()
PREFIX=R/'tools/LegacyMathAkProcessedPrefix.Probe'
PREFIX_LF_PINS={'audit_output.py': '8d7a770038e1c0ca9d0603149c9d16e852493cc73884b93284c382ee23d02403', 'reference_bootstrap.py': 'b879c0204031276a8a213b141f3db424eba70a8edd170d7daecb27dd7bb9f3ce', 'reference_prefix.py': '61fd185b05adb330bc933cddfe8b092ab30ad73c0b1dd335f9d75181a67447ab'}
for name,expected_sha in PREFIX_LF_PINS.items():
 assert hashlib.sha256((PREFIX/name).read_bytes().replace(b'\r\n',b'\n')).hexdigest()==expected_sha
# Unmodified prior 16-case all-F32/provenance oracle composes with added metadata.
p=subprocess.run([sys.executable,'-I','-B',str(PREFIX/'audit_output.py'),str(R),str(C),str(O.with_name(O.stem+'-prefix.json'))],check=True,capture_output=True,text=True)
c=json.loads(C.read_text());extra=['all-returned-buffers-detached','same-geometry-distinct-document','prefix-retains-frame-document','equal-data-foreign-profile-document','frame-precancel-exact-token','copy-precancel-exact-token','negative-id-refused','past-end-id-refused']
guards=['scalar-readset-1']+extra+[f'scalar-readset-{n}'for n in range(2,65)]
def exact(a,b):
 if type(a)is not type(b):return False
 if isinstance(b,dict):return a.keys()==b.keys()and all(exact(a[k],v)for k,v in b.items())
 if isinstance(b,list):return len(a)==len(b)and all(exact(x,y)for x,y in zip(a,b))
 return a==b

def append(n):
 # Literal append transition: empty counters/zero-created words; only NEW ID set.
 ids=[];reverse=[];kinds=[];active=[];ends=[];oldid=0;oldordinal=0;pointend=0
 for i in range(n):
  pointend+=(i*7+n)%3+1;ends.append(pointend)
  ids.append(oldid);reverse.append(oldordinal);kinds.append(-1)
  while len(active)<(oldid>>5)+1:active.append(0)
  active[oldid>>5]|=1<<(oldid&31);oldid+=1;oldordinal+=1
 return dict(strokeCount=oldordinal,itemCount=oldid,pointCount=pointend,activeWordCount=len(active),processedWordCount=(oldid>>5)+1,ends=ends,ids=ids,reverse=reverse,kinds=kinds,active=active)
expected=[append(n)for n in range(1,65)]
def check(z):
 assert exact(z['frames'],expected)and exact(z['frameGuards'],guards)
 assert type(z['networkForwards'])is int and z['networkForwards']==0
 return True
check(c)
mutations=[('id-gap',lambda z:z['frames'][31]['ids'].__setitem__(31,33)),('reverse-ordinal',lambda z:z['frames'][32]['reverse'].__setitem__(32,31)),('kind-zero',lambda z:z['frames'][0]['kinds'].__setitem__(0,0)),('32-active-extra-word',lambda z:z['frames'][31]['active'].append(0)),('32-processed-word-missing',lambda z:z['frames'][31].__setitem__('processedWordCount',1)),('64-last-active-bit',lambda z:z['frames'][63]['active'].__setitem__(1,0x7fffffff)),('end-boundary',lambda z:z['frames'][30]['ends'].__setitem__(-1,0)),('bool-id-zero',lambda z:z['frames'][0]['ids'].__setitem__(0,False)),('float-count',lambda z:z['frames'][0].__setitem__('itemCount',1.0)),('missing-guard',lambda z:z['frameGuards'].pop())]
neg=[]
for name,mutate in mutations:
 z=copy.deepcopy(c);mutate(z)
 try:check(z)
 except(AssertionError,ValueError,IndexError,KeyError):neg.append(name)
 else:raise AssertionError('Accepted '+name)
report=dict(prefixCases=16,prefixGuards=25,prefixTamperControls=11,metadataCases=64,metadataGuards=len(guards),metadataTamperControls=neg,boundaryCounts=[31,32,33,64],networkForwards=0,captureSHA=hashlib.sha256(C.read_bytes()).hexdigest())
O.write_text(json.dumps(report,indent=2)+'\n');print(p.stdout.strip());print('PASS64 append metadata/72 guards/10 tamper/0NN')
