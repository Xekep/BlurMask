from pathlib import Path
import copy,hashlib,importlib.util,json,struct,sys
D=Path(__file__).resolve().parent;R=Path(sys.argv[1]).resolve();capture=Path(sys.argv[2]);out=Path(sys.argv[3]);B=D.parent
ref=D/'reference_bootstrap.py';spec=importlib.util.spec_from_file_location('prefix_reference',ref);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
expected=m.controls()['cases'];raw=(R/'models/math/math-ak.res').read_bytes();sha=hashlib.sha256(raw).hexdigest();assert sha=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
def bits(x):return struct.unpack('<i',struct.pack('<f',x))[0]
GUARDS=['same-original-owner-1','detached-returned-arrays','input-array-detached','precancel-exact-token','equal-data-distinct-profile-owner','no-graph-or-request-authority']+[f'same-original-owner-{i}'for i in range(2,17)]+['maximum-strokes-expansion','maximum-original-points','over-original-point-bound','over-original-stroke-bound']
def exact(a,b):
 if type(a) is not type(b):return False
 if isinstance(b,dict):return a.keys()==b.keys() and all(exact(a[k],v)for k,v in b.items())
 if isinstance(b,list):return len(a)==len(b) and all(exact(x,y)for x,y in zip(a,b))
 return a==b
def check(c):
 assert c['resourceSHA']==sha and type(c['networkForwards']) is int and c['networkForwards']==0 and c['reflectionDiagnostics'] is True
 assert c['raw139D']==raw[7175:7217].hex() and c['raw139F']==raw[7217:7237].hex()
 for at,t,v,end in [(7175,0x139d,0x104,7217),(7217,0x139f,0x100,7237)]:
  magic,length,typ,flags,version=struct.unpack_from('<4sIIHH',raw,at);assert magic==b'\x90\xfe\x01\x01'and length+at+8==end and typ==t and flags==0 and version==v
 assert exact(c['guards'],GUARDS) and type(c['cases']) is list and len(c['cases'])==16
 for row,want in zip(c['cases'],expected):
  q=want['output']['result'];coords=[[bits(x),bits(y)]for stroke in q['points']for x,y in stroke]
  assert exact(row,dict(name=want['name'],scaleBits=bits(q['scale']),pointsBits=coords,ends=[len(r)for r in q['points']],originalPointIndices=q['originalPointIndices'],originalStrokeIndices=q['originalStrokeIndices'],expanded=q['expanded'],pointCount=sum(len(r)for r in q['points']))),want['name']
 return True
c=json.loads(capture.read_text());assert check(c)
mutations=[('point-f32-bit',lambda z:z['cases'][0]['pointsBits'][0].__setitem__(0,1)),('scale-f32-bit',lambda z:z['cases'][0].__setitem__('scaleBits',0)),('original-point-index',lambda z:z['cases'][0]['originalPointIndices'][0].__setitem__(0,1)),('original-stroke-index',lambda z:z['cases'][0]['originalStrokeIndices'].__setitem__(0,1)),('stored139D-byte',lambda z:z.__setitem__('raw139D','00'+z['raw139D'][2:])),('guard-deleted',lambda z:z['guards'].pop()),('bool-zero-point',lambda z:z['cases'][0]['pointsBits'][0].__setitem__(0,False)),('bool-zero-provenance',lambda z:z['cases'][0]['originalPointIndices'][0].__setitem__(0,False)),('bool-zero-network-forwards',lambda z:z.__setitem__('networkForwards',False)),('near-final-xy-replaced-by-index-target',lambda z:z['cases'][10]['pointsBits'][-1].__setitem__(0,1065353217)),('adjacent-original-chain-map',lambda z:z['cases'][12]['originalPointIndices'][0].__setitem__(-1,1))]
negative=[]
for name,mutate in mutations:
 z=copy.deepcopy(c);mutate(z)
 try:check(z)
 except (AssertionError,ValueError,KeyError,IndexError):negative.append(name)
 else:raise AssertionError('Tamper passed '+name)
report=dict(cases=16,guards=len(GUARDS),negativeControls=negative,comparedFloat32Bits=True,raw139DBytes=42,raw139FBytes=20,networkForwards=0,captureSHA=hashlib.sha256(capture.read_bytes()).hexdigest(),referenceSHA=hashlib.sha256(ref.read_bytes()).hexdigest())
out.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print('PASS 16 cases/25 guards/11 tamper/0 NN; all F32 bits and provenance exact')
