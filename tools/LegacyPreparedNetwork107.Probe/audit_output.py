"""Independent signed scalar107/quantizer10/EXP oracle; no research imports."""
from pathlib import Path
import base64,decimal,hashlib,json,math,struct,sys
if len(sys.argv)!=3:raise SystemExit('models-directory capture.json')
root=Path(sys.argv[1]);capture=json.loads(Path(sys.argv[2]).read_bytes())
manifest={x['path']:x['sha256']for x in json.loads((root/'manifest.json').read_bytes())['resources']}
lut=capture['PreparedHiddenLut'];assert len(lut)==48
assert hashlib.sha256(struct.pack('<48i',*lut)).hexdigest()=='db88702e24d4ac761fb778dca068606627c3a8b25e1ff614410c963fa627e26f'
def signed(v):return(v+2**31)%2**32-2**31
def f32(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def node(d,p,kind):
 magic,size,k,flags,v=struct.unpack_from('<IIIHH',d,p);assert magic==0x101fe90 and kind==k and flags==0
 assert 8<=size<=len(d)-p-8;return p+8+size,v
def first_networks(d):
 #These origins are pinned raw data for the two manifest-bound cursive files.
 end249,v=node(d,8827,0x249);assert v==0x100
 end247,v=node(d,8843,0x247);assert v==0x110 and end247==end249-4
 end,v=node(d,18695,0x21d);assert v==0x119 and end<end247
 assert struct.unpack_from('<H',d,8942+6)[0]==180
 p=18695+16+2+2+4;assert d[p:p+3]==bytes([0,0,1]);p+=3;assert d[p]==0;p+=45
 def array(copies):
  nonlocal p
  n=struct.unpack_from('<h',d,p)[0];assert 0<=n<=1024;p+=2+n*copies*2
 array(1);array(2);array(2);p+=16;assert d[p:p+2]==b'\0\0';p+=2;result=[]
 for slot in range(7):
  assert d[p]==0;p+=1;first,second,mode=struct.unpack_from('<BBB',d,p);p+=3
  if mode==0:p+=4
  for position in range(3):
   present=d[p];p+=1
   if present:
    assert position==1 and(first,second,mode)==(0,1,2)
    size=struct.unpack_from('<I',d,p+4)[0];result.append((slot,p,p+8+size));p+=8+size
 return result
def load(d,p,end):
 magic,v,size,architecture,kind,cols,hidden,zero1,zero2,outputs,q=struct.unpack_from('<HHIBHIIIIIH',d,p)
 assert magic==0xfea0 and v==0x107 and p+8+size==end and architecture==0x12 and q==10 and zero1==zero2==0
 matrices=[];at=p+33
 for columns,rows in((cols,hidden-1),(hidden,outputs-1)):
  at=(at+15)//16*16;stride=(columns+15)//16*16;scale=at+stride*rows
  weights=[struct.unpack_from('<'+'b'*columns,d,at+i*stride)for i in range(rows)]
  assert all(not any(d[at+i*stride+columns:at+(i+1)*stride])for i in range(rows))
  scales=d[scale+1:scale+rows+1];assert d[scale]==1 and all(scales);matrices.append((weights,scales));at=scale+rows+1
 assert at==end;return matrices,cols,hidden-1,outputs-1
exp_table=[int(25*math.exp(-4+22*i/351))for i in range(352)]
def exp_prob(raw):
 threshold=(351<<16)-4*1024*1024;offset=max(threshold,max(raw))-threshold
 values=[0 if n-offset+4*1024*1024<0 else exp_table[min(351,(n-offset+4*1024*1024)>>16)]for n in raw]
 while sum(values)>2**31-1:values=[n>>1 for n in values]
 if not max(values):return[0]*len(raw),offset
 total=sum(values);ss=max(0,total.bit_length()-1-6);div=total>>ss;vs=31-max(values).bit_length()
 result=[min(32767,(((n<<vs)&0xffffffff)//div)>>((vs+ss-15)&31))for n in values];return result,offset
def check(vector,matrices,special):
 input=vector['Input'];dots=[];hidden=[1024]
 for row,scale in zip(*matrices[0]):
  dot=signed(signed(sum(x*w for x,w in zip(input,row)))*scale);assert dot!=-2**31;dots.append(dot)
  index=abs(dot)>>16;value=lut[index]if index<48 else 1018;hidden.append(value if dot>0 else-value)
 raw=[signed(signed(sum(x*w for x,w in zip(hidden,row)))*scale)for row,scale in zip(*matrices[1])]
 assert vector['HiddenDots']==dots and vector['HiddenValues']==hidden and vector['OutputDots']==raw
 probabilities,offset=exp_prob(raw);assert(vector['ExpProbabilities'],vector['ExpOffset'])==(probabilities,offset)
 if special<0:source,so=probabilities,offset
 else:
  ordinary,so=exp_prob([n for i,n in enumerate(raw)if i!=special]);source=ordinary[:special]+[0]+ordinary[special:]
  argument=f32(f32(f32(raw[special])*-2.0)/1048576.0)
  source[special]=0 if argument>75 else int(32768/(math.exp(argument)+1)+.5)
 assert(vector['SourceProbabilities'],vector['SourceOffset'],vector['SpecialClassIndex'])==(source,so,special)
 return raw[special]if special>=0 else None

count=0;signs=set();actual_networks={}
assert len(capture['Models'])==2 and capture['Guards']>=227
for model in capture['Models']:
 d=(root/model['Path']).read_bytes();assert hashlib.sha256(d).hexdigest()==model['Fingerprint'].lower()==manifest[model['Path']]
 expected=first_networks(d);assert len(expected)==len(model['Networks'])
 for observed,(slot,p,end)in zip(model['Networks'],expected):
  matrices,cols,hidden,outputs=load(d,p,end);actual_networks[(model['Path'],slot)]=matrices
  assert(observed['Slot'],observed['Offset'],observed['End'],observed['InputWidth'],observed['HiddenWidth'],observed['OutputWidth'])==(slot,p,end,cols,hidden,outputs)
  assert observed['Fingerprint'].lower()==hashlib.sha256(d[p:end]).hexdigest() and len(observed['Vectors'])==32
  for ordinal,vector in enumerate(observed['Vectors']):
   expected_input=[0 if ordinal==0 else 1024 if ordinal==1 else-32768 if ordinal==2 else 32767 if ordinal==3 else(-32768 if i%2==0 else 32767)if ordinal==4 else((i*7919+ordinal*104729)^(i*ordinal*31337))for i in range(cols)]
   expected_input=[(v+32768)%65536-32768 for v in expected_input];assert vector['Input']==expected_input and vector['Ordinal']==ordinal
   value=check(vector,matrices,180);signs.add(value>0);count+=1
assert signs=={False,True} and count==192
for prepared in capture['PreparedResourceControls']:
 assert prepared['Mutation']=='zeroWeights';base=actual_networks[('en-US/en_US-ak-cur.lite.res',0)]
 matrices=[([tuple(0 for _ in row)for row in rows],scales)for rows,scales in base]
 check(prepared['Vector'],matrices,-1 if prepared['HeaderSpecial']==65535 else prepared['HeaderSpecial'])
assert [p['HeaderSpecial']for p in capture['PreparedResourceControls']]==[0,180,181,65535]
scalars=capture['PreparedSpecialScalarControls'];expected={-2**31,2**31-1,0}
for n in range(32768):
 p=n+.5;center=math.floor(524288*math.log(p/(32768-p)));expected.update(range(center-2,center+3))
for center in(-39321600,39321600):expected.update(center+n for n in(-8,-4,-3,-2,-1,0,1,2,3,4,8))
assert scalars['Raw']==sorted(expected)
assert len(scalars['Raw'])==len(scalars['ArgumentBits'])==len(scalars['Probabilities'])
near=0;minimum_distance=.5;decimal.getcontext().prec=60
for raw,bits,observed in zip(scalars['Raw'],scalars['ArgumentBits'],scalars['Probabilities']):
 arg=f32(f32(f32(raw)*-2)/1048576);assert bits==struct.unpack('<i',struct.pack('<f',arg))[0]
 value=0 if arg>75 else 32768/(math.exp(arg)+1);expected=int(value+.5);assert observed==expected
 distance=abs((value%1)-.5);minimum_distance=min(minimum_distance,distance)
 if distance<.00001:
  exact=decimal.Decimal(32768)/(decimal.Decimal.from_float(arg).exp()+1)
  assert observed==int(exact+decimal.Decimal('.5'));near+=1
assert near>100
print(f'PASS independent scalar107: {count} full prepared vectors/4zero-weight resource controls; special180 both signs/native hiddenLUT hash/wrapping dots/all probabilities exact')
print(f"PASS{len(scalars['Raw'])} prepared mixed-special near-rounding/float-threshold controls; host math.exp adaptation checked, not universal native CRT parity")
print(f'PASS{near} closest-boundary controls via60-digit Decimal.exp; minimum observed probability distance to half={minimum_distance:.12g}')
