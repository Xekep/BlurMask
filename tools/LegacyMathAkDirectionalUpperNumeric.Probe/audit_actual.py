"""Explicit DATA-only independent replay of actual selected5F; no DLL loading.
Author prepares/compiles this source only. Root owns the actual replay execution.
"""
from pathlib import Path
import hashlib,importlib.util,json,math,struct,sys,copy
assert len(sys.argv)==5 and sys.argv[1]=='--actual-replay'
R,C,O=map(Path,sys.argv[2:]);assert not O.exists()
H=lambda x:hashlib.sha256(x).hexdigest()
ak=(R/'models/math/math-ak.res').read_bytes();assert H(ak)=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
r=json.loads(C.read_bytes())
assert r['scope']=='actual same-source selected5F numerical channels only'
assert r['xy']==[[0,0],[2,1],[4,0]] and r['ModelStart']==1629111
assert type(r['modelExecutions'])is int and r['modelExecutions']==1 and r['vendorExecution']is False
assert r['HasUnifiedAlphabetCosts']is False and r['HasCandidateAuthority']is False
#No captured feature channel participates in deriving the expected vectors.
sys.path.insert(0,str(R/'tools/LegacyMathAkDirectionalUpper.Probe'))
from reference_upper import calculate
feature_reference=calculate(r['xy'],ak);features=feature_reference['values']
assert len(features)==223

def pe(name,expected):
 b=(R/'research/myscript/sdk/build/x64'/name).read_bytes();assert H(b)==expected
 p=struct.unpack_from('<I',b,60)[0];n=struct.unpack_from('<H',b,p+6)[0];o=struct.unpack_from('<H',b,p+20)[0];headers=[struct.unpack_from('<4I',b,p+24+o+40*i+8)for i in range(n)]
 def read(a,n):
  va=a-0x180000000
  for vs,start,size,offset in headers:
   if start<=va and va+n<=start+size:return b[offset+va-start:offset+va-start+n]
  raise AssertionError('Unmapped static span')
 return read
text=pe('MyScriptText.dll','18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f')
mathread=pe('MyScript2D.dll','c633012619b4db77a73de117913d4d1eac6f5c340156bfe054f246cfe3701afd')
table_bytes=text(0x180260fd0,192);assert H(table_bytes)=='db88702e24d4ac761fb778dca068606627c3a8b25e1ff614410c963fa627e26f'
hidden_table=struct.unpack('<48i',table_bytes)
exp_bytes=text(0x180260a50,1408);assert H(exp_bytes)=='f05ff672e4399147c00eb9eb48a17ea4dc15e3f71a541d0508d84bbc4d618470'
exp=struct.unpack('<352i',exp_bytes);assert exp==tuple(int(25*math.exp(-4+22*i/351))for i in range(352))
log_bytes=mathread(0x180074290,8196);assert H(log_bytes)=='01c4ba5cde6506d2a4e1ab37e0bb91624c5282392cd1b289bea8b02219365c1f'
log=struct.unpack('<2049i',log_bytes);assert log==(-(1<<26),)+tuple(int(8192*math.log(i/2048))for i in range(1,2049))
assert struct.unpack('<Q',text(0x1802ba2c0+0x50,8))[0]==0x1800eaf90
assert struct.unpack('<Q',text(0x1802ba2c0+0x78,8))[0]==0x18004a2d0
assert struct.unpack('<Q',text(0x1802ba2c0+0x80,8))[0]==0x18004a2d0
assert text(0x18004a2d0,3)==bytes.fromhex('c20000')
assert struct.unpack('<Q',text(0x1802ba2c0+0xc8,8))[0]==0x1800ebd10
assert text(0x1800ebd10,14)==bytes.fromhex('488b82a00000004963c88b0488c3')
start,end=1629111,1772466
assert ak[1628211]==1 and struct.unpack_from('<H',ak,1628212)[0]==223
calibration_end=1628211+3+223*4;assert calibration_end==start-5
assert ak[calibration_end:start]==bytes([0,1,2,0,1])
assert struct.unpack_from('<HHI',ak,start)==(0xfea0,0x107,end-start-8)
assert ak[start+8]==2 and struct.unpack_from('<H',ak,start+9)[0]==0x5f
assert struct.unpack_from('<5I',ak,start+11)==(223,201,0,0,258) and struct.unpack_from('<H',ak,start+31)[0]==10
first=(start+33+15)&~15;first_stride=224;second=(first+200*first_stride*2+15)&~15;second_stride=208;scales=second+257*second_stride
assert scales+258==end and ak[scales]==1 and all(ak[scales:scales+258])
w1=struct.unpack_from('<'+str(200*first_stride)+'h',ak,first);w2=struct.unpack_from('<'+str(257*second_stride)+'b',ak,second)
assert all(w1[i*first_stride+j]==0 for i in range(200)for j in range(223,first_stride))
assert all(w2[i*second_stride+j]==0 for i in range(257)for j in range(201,second_stride))
wrap=lambda value:(value+2**31)%2**32-2**31

def pairdot(x,w,at):
 value=0
 for i in range(0,len(x),2):value=wrap(value+wrap(sum(x[j]*w[at+j]for j in range(i,min(i+2,len(x))))))
 return value
#The only actual-weight numerical execution in this script: root execution only.
dots=[pairdot(features,w1,row*first_stride)for row in range(200)];assert -2**31 not in dots
hidden=[1024]
for value in dots:
 index=abs(value)>>16;magnitude=hidden_table[index]if index<48 else 1018
 hidden.append(-magnitude if value<=0 else magnitude)
raw=[wrap(pairdot(hidden,w2,row*second_stride)*ak[scales+1+row])for row in range(257)]
threshold=(351<<16)-(4<<20);offset=max(threshold,*raw)-threshold
assert 0<=offset<=2**31-1 and all(v-offset>=-2**31 for v in raw)
values=[0 if v-offset+(4<<20)<0 else exp[min(351,(v-offset+(4<<20))>>16)]for v in raw]
while sum(values)>2**31-1:values=[v>>1 for v in values]
maximum=max(values);assert maximum>0
sum_shift=max(0,sum(values).bit_length()-1-6);divisor=sum(values)>>sum_shift;value_shift=30-(maximum.bit_length()-1)
probability=[min(32767,((v<<value_shift)&0xffffffff)//divisor>>((value_shift+sum_shift-15)&31))for v in values]

def cost(p):
 p=max(1,p);q,rem=p>>4,p&15
 return -(log[q+(rem>8)]if q else log[rem]-0x58b9)
costs=[cost(p)for p in probability]
expected=dict(features=features,hiddenDots=dots,hidden=hidden,raw=raw,probability=probability,costs=costs)
def check(candidate):
 for key,value in expected.items():
  assert type(candidate[key])is list and all(type(v)is int for v in candidate[key]) and candidate[key]==value,key
 assert candidate['ModelStart']==1629111 and candidate['HasUnifiedAlphabetCosts']is False and candidate['HasCandidateAuthority']is False
check(r);controls=[]
for key in expected:
 bad=copy.deepcopy(r);bad[key][0]^=1
 try:check(bad)
 except AssertionError:controls.append(key)
 else:raise AssertionError('Corruption accepted: '+key)
O.write_text(json.dumps(dict(captureSHA256=H(C.read_bytes()),independentFeatures=223,preparedPointCount=feature_reference['preparedCount'],hiddenDots=200,hidden=201,raw=257,probabilities=257,costs=257,allIntegerChannelsExact=True,corruptions=controls,referenceModelExecutions=1,vendorExecution=False,scope='One fixed source-owned5F numerical forward from original synthetic geometry and supplied5F calibration; not unifiedAL costs,grammar or OCR accuracy'),indent=2)+'\n',encoding='utf8')
print('PASS original geometry/upper223/200dots/201hidden/257raw/Q15/costs exact;6tampercontrols')
