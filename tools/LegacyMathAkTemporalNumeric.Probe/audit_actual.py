"""Explicit independent replay of one actual model; never loads a vendor DLL."""
from pathlib import Path
import hashlib,importlib.util,json,math,struct,sys
assert len(sys.argv)==5 and sys.argv[1]=='--actual-replay'
R,C,O=map(Path,sys.argv[2:]);assert not O.exists()
ak=(R/'models/math/math-ak.res').read_bytes()
assert hashlib.sha256(ak).hexdigest()=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
r=json.loads(C.read_bytes());assert r['guards']==['pre-cancel-exact-token','equal-data-foreign-source-refused','caller-resource-detached-before-forward','raw-output-detached','probability-output-detached'];assert r['xy']==[[0,0],[2,1],[4,0]] and r['ModelStart']==1007739
assert r['modelExecutions']==1 and r['vendorExecution']is False and r['HasUnifiedAlphabetCosts']is False and r['HasCandidateAuthority']is False
spec=importlib.util.spec_from_file_location('temporal_reference',R/'tools/LegacyMathAkTemporal.Probe/reference_temporal.py')
ref=importlib.util.module_from_spec(spec);spec.loader.exec_module(ref)
mi=struct.unpack_from('<412h',ak,1006085);ma=struct.unpack_from('<412h',ak,1006909)
samples,features=ref.calculate(ref.normalize(r['xy']),mi,ma);assert r['features']==features
text=(R/'research/myscript/sdk/build/x64/MyScriptText.dll').read_bytes()
assert hashlib.sha256(text).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pe=struct.unpack_from('<I',text,60)[0];sections=struct.unpack_from('<H',text,pe+6)[0];optional=struct.unpack_from('<H',text,pe+20)[0]
headers=[struct.unpack_from('<4I',text,pe+24+optional+40*i+8)for i in range(sections)]
def get(address,length):
 va=address-0x180000000
 for vs,start,size,offset in headers:
  if start<=va and va+length<=start+size:return text[offset+va-start:offset+va-start+length]
 raise AssertionError('Unmapped static span')
table_bytes=get(0x180260fd0,192)
assert hashlib.sha256(table_bytes).hexdigest()=='db88702e24d4ac761fb778dca068606627c3a8b25e1ff614410c963fa627e26f'
hidden_table=struct.unpack('<48i',table_bytes)
start,end=1007739,1151090
assert struct.unpack_from('<HHI',ak,start)==(0xfea0,0x107,end-start-8)
assert ak[start+8]==2 and struct.unpack_from('<H',ak,start+9)[0]==0x1f
assert struct.unpack_from('<5I',ak,start+11)==(222,201,0,0,258) and struct.unpack_from('<H',ak,start+31)[0]==10
first=(start+33+15)&~15;first_stride=224;second=first+200*first_stride*2;second_stride=208;scales=second+257*second_stride
assert scales+258==end and ak[scales]==1
w1=struct.unpack_from('<'+str(200*first_stride)+'h',ak,first)
w2=struct.unpack_from('<'+str(257*second_stride)+'b',ak,second)
assert all(w1[i*first_stride+j]==0 for i in range(200)for j in range(222,first_stride))
assert all(w2[i*second_stride+j]==0 for i in range(257)for j in range(201,second_stride))
wrap=lambda value:(value+2**31)%2**32-2**31
def pairdot(x,w,at):
 value=0
 for i in range(0,len(x),2):value=wrap(value+wrap(sum(x[j]*w[at+j]for j in range(i,min(i+2,len(x))))))
 return value
dots=[pairdot(features,w1,row*first_stride)for row in range(200)]
assert -2**31 not in dots and r['hiddenDots']==dots
hidden=[1024]
for value in dots:
 index=abs(value)>>16;magnitude=hidden_table[index]if index<48 else 1018
 hidden.append(-magnitude if value<=0 else magnitude)
assert r['hidden']==hidden
raw=[wrap(pairdot(hidden,w2,row*second_stride)*ak[scales+1+row])for row in range(257)]
assert r['raw']==raw
#Original unmasked EXP/Q15 arithmetic; its complete finite tables are checked
#separately against PE bytes by the reviewed numerical source packet.
exp=[int(25*math.exp(-4+22*i/351))for i in range(352)]
threshold=(351<<16)-(4<<20);offset=max(threshold,*raw)-threshold
assert 0<=offset<=2**31-1 and all(v-offset>=-2**31 for v in raw)
values=[0 if v-offset+(4<<20)<0 else exp[min(351,(v-offset+(4<<20))>>16)]for v in raw]
while sum(values)>2**31-1:values=[v>>1 for v in values]
maximum=max(values);assert maximum>0
sum_shift=max(0,sum(values).bit_length()-1-6);divisor=sum(values)>>sum_shift;value_shift=30-(maximum.bit_length()-1)
prob=[min(32767,((v<<value_shift)&0xffffffff)//divisor>>((value_shift+sum_shift-15)&31))for v in values]
assert r['probability']==prob
log=[-(1<<26)]+[int(8192*math.log(i/2048))for i in range(1,2049)]
def cost(p):
 p=max(1,p);q,rem=p>>4,p&15
 return -(log[q+(rem>8)]if q else log[rem]-0x58b9)
costs=[cost(p)for p in prob];assert r['costs']==costs
report=dict(captureSHA256=hashlib.sha256(C.read_bytes()).hexdigest(),independentFeatures=222,hiddenDots=200,hidden=201,
 raw=257,probabilities=257,costs=257,allIntegerChannelsExact=True,referenceModelExecutions=1,vendorExecution=False,
 scope='One fixed original1F numerical forward from original synthetic geometry; not unified AL costs, grammar output or OCR accuracy')
O.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print('PASS original geometry/features/200dots/201hidden/257raw/Q15/costs exact')
