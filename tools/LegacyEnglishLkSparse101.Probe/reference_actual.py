"""Independent raw English 2D2/v101 partition and fresh profile2 binding reference.
Static data only: no vendor loads, expected word input, fitted values or RU owner.
"""
from pathlib import Path
import argparse,hashlib,json,math,struct
p=argparse.ArgumentParser();p.add_argument('models',type=Path);p.add_argument('output',type=Path);a=p.parse_args()
d=(a.models/'en-US/en_US-lk-text.lite.res').read_bytes();sha=lambda b:hashlib.sha256(b).hexdigest()
assert len(d)==3300812 and sha(d)=='9f4ef50bca4a1e3cc42189c279b72bf8fdf03f62e638673079d57075e168f7c9'
base=1252412;end=2832716;assert struct.unpack_from('<IIIHH',d,base)==(0x101fe90,end-base-8,0x2d2,0,0x101)
at=base+16;fields=[];values={};raws={};arrays={}
def take(n,label):
 global at
 assert 0<=n<=end-at,(label,at,n)
 start=at;at+=n;raw=d[start:at];raws[label]=raw
 fields.append(dict(destination=label,offset=start,end=at,bytes=n,sha256=sha(raw)));return raw
def ints(n,label):
 v=list(struct.unpack('<'+'I'*n,take(n*4,label)));values[label]=v;return v
assert struct.unpack('<H',take(2,'2CD.version'))[0]==0x102
reserved=ints(3,'2CD.64,68,6C');take(72,'138.baseMetadata')
order=ints(1,'26AC.maxOrder')[0];flags=ints(1,'wire.flags')[0];words=ints(1,'70')[0];classes=ints(1,'74')[0];sizes=ints(order,'78')
assert(order,flags,words,classes,sizes)==(3,7,260075,1024,[1024,113643,155484]);assert reserved==[260074,0xffffffff,0xffffffff]
def packed(label):
 global at
 start=at;assert struct.unpack('<I',take(4,label+'.version'))[0]==0x1000001
 take((-at)%4,label+'.countAlign');count=ints(1,label+'.count')[0];width=take(1,label+'.width')[0];assert 1<=width<=31
 assert take(1,label+'.metadata')==b'\1';take((-at)%4,label+'.dataAlign');raw=take(((count*width+31)//32)*4,label+'.data');mask=(1<<width)-1
 #Byte windows and32-bit crossing are independent extraction algorithms.
 v=[];w=struct.unpack('<'+'I'*(len(raw)//4),raw);inverse=bytearray(len(raw))
 for i in range(count):
  bit=i*width;q=bit//8;s=bit%8;n=(s+width+7)//8;x=(int.from_bytes(raw[q:q+n],'little')>>s)&mask;v.append(x)
  wi=bit//32;ws=bit%32;y=w[wi]>>ws
  if ws+width>32:y|=w[wi+1]<<(32-ws)
  assert y&mask==x
  for k in range(n):inverse[q+k]|=((x<<s)>>(8*k))&255
 assert bytes(inverse)==raw
 arrays[label]=v;values[label]=dict(start=start,end=at,count=count,width=width,minimum=min(v),maximum=max(v),valuesSha256=sha(struct.pack('<'+'I'*count,*v)))
if flags&0x10:packed('80')
if flags&1:packed('88')
if flags&0x20:packed('90')
if flags&2:take(len(arrays.get('88',range(words))),'98.wordCodes');ints(1,'A0');ints(values['A0'][0],'A8')
for i in range(1,order):packed('B0.'+str(i))
for i in range(order-1):packed('B8.'+str(i))
for i,n in enumerate(sizes):take(n,'C0.'+str(i))
for i,n in enumerate(sizes[:-1]):take(n,'C8.'+str(i))
d0=ints(order,'D0')
for i,n in enumerate(d0):ints(n,'D8.'+str(i))
e0=ints(order,'E0')
for i,n in enumerate(e0[:-1]):ints(n,'E8.'+str(i))
for i in range(4):ints(order,'F0.'+str(i))
ints(4,'110');ints(4,'120')
for i in range(4):ints(order,'130.'+str(i))
ints(4,'266C');ints(4,'267C')
if flags&4:
 values['150']=struct.unpack('<H',take(2,'150.casePathVersion'))[0];ints(4,'154');ints(4,'164');take(0x2a4,'174');take(0x2254,'418')
if flags&8:ints(4,'268C')
if flags&0x40:
 for i in range(order):packed('26A0.'+str(i))
assert at==end and len(fields)==75
#All actual sparse edges and codes are checked; no arbitrary query histories.
assert len(arrays['88'])==words and max(arrays['88'])<classes and arrays['88'][reserved[0]]==0
for i in range(order):assert max(raws['C0.'+str(i)])<d0[i]
assert max(raws['98.wordCodes'])<values['A0'][0]
sparse=[]
for i in range(order-1):
 offsets=arrays['B8.'+str(i)];labels=arrays['B0.'+str(i+1)]
 assert len(offsets)==sizes[i]+1 and len(labels)==sizes[i+1] and offsets[0]==0 and offsets[-1]==len(labels)
 assert all(x<=y for x,y in zip(offsets,offsets[1:])) and max(labels)<classes
 for l,r in zip(offsets,offsets[1:]):assert all(labels[j]<labels[j+1]for j in range(l,r-1))
 assert max(raws['C8.'+str(i)])<e0[i]
 sparse.append(dict(order=i+2,parentCount=len(offsets)-1,edgeCount=len(labels),emptyRows=sum(x==y for x,y in zip(offsets,offsets[1:])),maximumDegree=max(y-x for x,y in zip(offsets,offsets[1:]))))
#105CA0 fresh source135 => profile2, source13B runtime scale1, packet.byte0=0.
f32=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
def floats(label):return list(struct.unpack('<'+'f'*(len(raws[label])//4),raws[label]))
def signed(x):return x if x<0x80000000 else x-0x100000000
def code(raw,mult,bias):
 x=f32(f32(signed(raw))*f32(2**-13));x=f32(x*mult);x=f32(x-bias);return math.trunc(f32(x*8192))
def digest(v):return sha(struct.pack('<'+'i'*len(v),*v))
profile=2;scale=f32(1);bias=[f32(scale*x)for x in floats('F0.2')];mult=[f32(scale*x)for x in floats('130.2')];boundaryBefore=f32(scale*floats('110')[profile]);boundaryAfter=f32(scale*floats('120')[profile])
wordCompiled=[code(x,mult[0],boundaryBefore)for x in values['A8']]
classCompiled=[[code(x,mult[i],bias[i])for x in values['D8.'+str(i)]]for i in range(order)]
backoffCompiled=[[code(x,mult[i],0.0)for x in values['E8.'+str(i)]]for i in range(order-1)]
caseBias=floats('154')[profile];caseMult=floats('164')[profile]
def caseCost(x):
 x=f32(f32(f32(caseMult*x)+caseBias)*8192);return -math.trunc(f32(x+0.5 if x>0 else x-0.5))
case2=floats('174');case3=floats('418');caseRows=[];caseTriples=[]
for i in range(1,13):
 for j in range(1,13):
  caseRows.append([i,j,caseCost(case2[i*13+j])])
  for k in range(1,13):caseTriples.append([i,j,k,caseCost(case3[i*169+j*13+k])])
compiled=dict(sourceKind=0x135,profile=profile,sourceScale=scale,packetByte0=0,controlBytes1And2="not selected by this numeric-only reference; configured owner deferred",classSubtractionBias=bias,classMultiplier=mult,wordSubtractionBias=boundaryBefore,boundaryBefore=boundaryBefore,boundaryAfter=boundaryAfter,boundary94C=math.trunc(f32(f32(boundaryAfter-boundaryBefore)*-8192)),context00=0x4000000,context00Provenance='103020 ctor268C..2698=FC000000; wire flag8 absent; 105CA0 negates',wordCodesSha256=digest(wordCompiled),classCodesSha256=[digest(v)for v in classCompiled],backoffCodesSha256=[digest(v)for v in backoffCompiled],finalUnigram94D=classCompiled[0][-1],caseBias=caseBias,caseMultiplier=caseMult,case2ExplicitZero=[0,0,0],case3ExplicitZero=[0,0,0,0],caseRows=caseRows,caseTriples=caseTriples,caseFirstHistoryZeroPlane='same compiled144 caseRows; all other unspecified border slots excluded')
assert len(wordCompiled)==256 and [len(x)for x in classCompiled]==[256]*3 and [len(x)for x in backoffCompiled]==[256]*2
assert(len(caseRows),len(caseTriples))==(144,1728)
result=dict(scope='STATIC raw sparse language fields and fresh selected-main numeric binding ONLY; no configured root/controller/query/masks/main inference claim',resourceSha256=sha(d),base=base,end=end,fields=fields,values=values,sparseRows=sparse,reservedWord=dict(index=reserved[0],classID=arrays['88'][reserved[0]],wordCode=raws['98.wordCodes'][reserved[0]]),compiled=compiled,nativeExecuted=False)
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(result,indent=2)+'\n',encoding='utf8');print('PASS actual EN2D2/v101:',len(fields),'fields;',sum(len(v)for v in arrays.values()),'packed values dual-unpacked/inverse; sparse rows',sum(x['parentCount']for x in sparse),'; compiled1536 cost entries +144/1728 case entries; fresh default context',hex(compiled['context00']))
