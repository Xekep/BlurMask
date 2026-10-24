"""Pure synthetic dot/LUT controls, actual static bytes only; no model evaluation."""
from pathlib import Path
import struct,math,json,hashlib,sys,copy,re
repo,capture,output=map(Path,sys.argv[1:]);b=(repo/'research/myscript/sdk/build/x64/MyScriptText.dll').read_bytes()
H=lambda v:hashlib.sha256(v).hexdigest()
assert H(b)=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
p=struct.unpack_from('<I',b,60)[0];n=struct.unpack_from('<H',b,p+6)[0];o=struct.unpack_from('<H',b,p+20)[0];sections=[struct.unpack_from('<4I',b,p+24+o+40*i+8)for i in range(n)]
def get(a,n):
 r=a-0x180000000
 for vs,va,rs,ro in sections:
  if va<=r and r+n<=va+rs:return b[ro+r-va:ro+r-va+n]
 raise AssertionError('Unmapped native span')
table=struct.unpack('<48i',get(0x180260fd0,192))
source=(Path(__file__).resolve().parents[2]/'src/InkRecognition/LegacyMathAkTemporalNetwork.cs').read_text(encoding='utf-8-sig')
match=re.search(r'private static readonly int\[\] HiddenTable\s*=\s*\{([^}]+)\}',source);assert match
literal=tuple(int(x)for x in re.findall(r'\d+',match.group(1)));assert literal==table and len(literal)==48
assert 'Math.Tanh'not in source
assert struct.unpack('<Q',get(0x1802ba2c0+0x78,8))[0]==0x18004a2d0
assert get(0x18004a2d0,3)==bytes.fromhex('c20000')
assert H(get(0x1800ef640,102))=='6b9ef700ee454b5bbb0b069abb5029984ae91ea7adbc8ef2da277a478af27bc6'
assert H(get(0x1800ef6b0,138))=='0a99e907ce557cabc456d35b3c33f7e3118bfc85289cd3d6b5aff4a90155b3e3'
assert struct.unpack('<Q',get(0x1802ba600+0x38,8))[0]==0x1800ef640
assert struct.unpack('<Q',get(0x1802ba5a0+0x38,8))[0]==0x1800ef6b0
assert struct.unpack('<Q',get(0x1802ba2c0+0x50,8))[0]==0x1800eaf90
exp=struct.unpack('<352i',get(0x180260a50,352*4));assert exp==tuple(int(25*math.exp(-4+22*i/351))for i in range(352))
mathbytes=(repo/'research/myscript/sdk/build/x64/MyScript2D.dll').read_bytes();assert H(mathbytes)=='c633012619b4db77a73de117913d4d1eac6f5c340156bfe054f246cfe3701afd'
mp=struct.unpack_from('<I',mathbytes,60)[0];mn=struct.unpack_from('<H',mathbytes,mp+6)[0];mo=struct.unpack_from('<H',mathbytes,mp+20)[0];ms=[struct.unpack_from('<4I',mathbytes,mp+24+mo+40*i+8)for i in range(mn)]
def mget(a,n):
 r=a-0x180000000
 for vs,va,rs,ro in ms:
  if va<=r and r+n<=va+rs:return mathbytes[ro+r-va:ro+r-va+n]
 raise AssertionError('math native span')
log=struct.unpack('<2049i',mget(0x180074290,2049*4));assert log[0]==-67108864
assert log[1:]==tuple(int(8192*math.log(i/2048))for i in range(1,2049))
assert struct.unpack('<Q',get(0x1802ba2c0+0xc8,8))[0]==0x1800ebd10
assert get(0x1800ebd10,14)==bytes.fromhex('488b82a00000004963c88b0488c3')
def convert(raw):
 threshold=(351<<16)-4194304;offset=max(threshold,max(raw))-threshold
 assert offset<=2147483647 and min(raw)-offset>=-2147483648
 vals=[0 if v-offset+4194304<0 else exp[min(351,(v-offset+4194304)>>16)]for v in raw]
 while sum(vals)>2147483647:vals=[v>>1 for v in vals]
 total=sum(vals);maximum=max(vals)
 if maximum==0:probs=[0]*len(vals)
 else:
  shift=max(0,total.bit_length()-1-6);divisor=total>>shift;left=30-(maximum.bit_length()-1)
  probs=[min(32767,((v<<left)&4294967295)//divisor >> ((left+shift-15)&31))for v in vals]
 costs=[]
 for v in probs:
  v=max(v,1);q,rem=divmod(v,16);costs.append(-(log[q+(rem>8)]if q else log[rem]-0x58b9))
 return probs,costs
expectedNames=['signed','pair-overflow','sum-wrap','offset','signed-scale','scale-wrap','offset','scale-one']
def wrap(v):return ((v+2**31)%2**32)-2**31
def check(r):
 assert r['modelExecutions']==0 and r['vendorExecution']is False and r['intMinRefused']is True
 assert [x['name']for x in r['rows']]==expectedNames
 assert len(r['hidden'])==17 and len(r['scoreRows'])==3
 for q in r['scoreRows']:
  assert len(q['raw'])==257
  probability,costs=convert(q['raw']);assert q['probability']==probability and q['costs']==costs
 for row in r['rows']:
  x,w,at=row['input'],row['weights'],row['at'];assert len(x)>0 and at+len(x)<=len(w)
  s=0
  #Independent pairwise PMADDWD reduction followed by modular scaling.
  for i in range(0,len(x),2):s=wrap(s+wrap(sum(x[j]*w[at+j]for j in range(i,min(i+2,len(x))))))
  assert row['result']==wrap(s*row['scale'])
 for q in r['hidden']:
  v=q['value'];assert v!=-2**31
  at=abs(v)>>16;m=table[at]if at<48 else 1018
  assert q['result']==(-m if v<=0 else m)
r=json.loads(capture.read_text());check(r);neg=[]
for name,mut in [('dot',lambda z:z['rows'][0].__setitem__('result',0)),('scale',lambda z:z['rows'][4].__setitem__('scale',1)),('hidden',lambda z:z['hidden'][3].__setitem__('result',0)),('int-min',lambda z:z.__setitem__('intMinRefused',False)),('neural-claim',lambda z:z.__setitem__('modelExecutions',1)),('probability',lambda z:z['scoreRows'][0]['probability'].__setitem__(0,0)),('cost',lambda z:z['scoreRows'][0]['costs'].__setitem__(0,0))]:
 q=copy.deepcopy(r);mut(q)
 try:check(q)
 except AssertionError:neg.append(name)
 else:raise AssertionError(name)
report=dict(captureSHA256=H(capture.read_bytes()),syntheticDots=8,hiddenControls=17,nativeTableEntries=48,expEntries=352,logEntries=2049,syntheticScoreRows=3,syntheticCosts=771,modularDotScopes=2,corruptions=neg,modelExecutions=0,vendorExecution=False,scope='Numerical kernels only; no real weights evaluated, no unifiedAL costs or candidates')
output.write_text(json.dumps(report,indent=2)+'\n');print('PASS 8 synthetic dots /17 hidden /48 native entries /7 corruptions /3x257 costs /0NN')
