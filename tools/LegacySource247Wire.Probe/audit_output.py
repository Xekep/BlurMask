"""Independent raw-wire247/134/297 audit; no research/runtime imports."""
from pathlib import Path
import base64,hashlib,json,struct,sys
if len(sys.argv)!=3:raise SystemExit('models-directory capture.json')
models=Path(sys.argv[1]);capture=json.loads(Path(sys.argv[2]).read_bytes())
manifest={x['path']:x['sha256'] for x in json.loads((models/'manifest.json').read_bytes())['resources']}

def alphabet_end(data,at,version):
 def read(fmt,n=1):
  nonlocal at
  size=struct.calcsize('<'+fmt)*n
  assert 0<=n<=65536 and 0<=at<=len(data)-size
  v=struct.unpack_from('<'+fmt*n,data,at);at+=size;return v
 count=read('H',8)[-1];n=read('H')[0];assert count>0
 for step in(32,64,1):
  assert 0<n<65536;values=read('H',n);n=next(x for x in reversed(values) if x!=65535)+step
 spans=read('H',n+1);assert all(a<=b for a,b in zip(spans,spans[1:]))
 entries=[read('I',2) for _ in range(spans[-1]+1)];length=entries[-1][0];assert entries[0][0]==0
 assert length<=16*1024*1024 and length<=len(data)-at
 for (a,label),(b,_) in zip(entries,entries[1:]):
  assert 0<=a<b<=length and label&65535<count;data[at+a:at+b].decode('utf8')
 at+=length;reverse=read('H',count);assert all(x==65535 or x<spans[-1] for x in reverse)
 bits=read('B')[0];assert bits in(0,1)
 if bits:read('B',(count+7)//8)
 triples=read('H')[0];read('H',triples*3)
 if triples:
  flags=read('I')[0];assert flags&~7==0
  for bit in(1,2,4):
   if flags&bit:read('B' if triples<128 else'H',count)
 assert version in(0x103,0x104)
 if version==0x104:read('B')
 return at,count

class Wire:
 def __init__(self,data,start=0,end=None):self.data=data;self.at=start;self.end=len(data) if end is None else end;self.fields=[];self.children=[]
 def take(self,owner,destination,size=1,count=1):
  n=size*count;assert 0<=n<=self.end-self.at
  raw=self.data[self.at:self.at+n];self.fields.append(dict(owner=owner,destination=destination,offset=self.at,size=size,count=count,raw=base64.b64encode(raw).decode()))
  self.at+=n;return raw
 def u(self,owner,destination,size=1):return int.from_bytes(self.take(owner,destination,size),'little')
 def flag(self,owner,destination):
  n=self.u(owner,destination);assert n in(0,1);return n
 def node(self,destination,kind=None):
  at=self.at;magic,length,k,flags,v=struct.unpack_from('<IIIHH',self.data,at)
  assert magic==0x101fe90 and flags==0 and 8<=length<=self.end-at-8 and(k==kind or kind is None)
  self.at=at+8+length;self.children.append(dict(destination=destination,Offset=at,End=self.at,Kind=k,Version=v))

def source(data):
 r=Wire(data);magic,length,kind,flags,v=struct.unpack_from('<IIIHH',data);assert magic==0x101fe90 and kind==0x247 and flags==0 and length+8==len(data)
 assert v in(0x10d,0x110);r.take('247','envelope',1,16)
 bv=r.u('247','base134Version',2);assert bv in(0x201,0x300) and r.u('134','base133Version',2)==0x100
 r.u('133','10',4);r.take('133','14.metadata',1,56);marker=r.u('133','identityMarker',4)
 if marker==0xffffffff:r.u('133','4C',4)
 r.u('133','50',4);assert r.flag('134','alphabetPresent')==1 and r.u('134','alphabetKind',4)==0x2b
 av=r.u('134','alphabetVersion',2);end,count=alphabet_end(data,r.at,av);r.take('2B','payload',1,end-r.at)
 if bv==0x300:
  n=r.u('134','70.additionalCount')
  for i in range(n):r.node(f'134.68[{i}]')
 sk=r.u('247','78',4);mode=r.u('247','80');assert sk in range(0x134,0x138) and mode in range(3)
 if mode==2:
  n=r.u('coordinate88','00.count');r.take('coordinate88','08.values',4,n)
  for f in('10','14'):r.u('coordinate88',f,4)
  for f in('18','19','1A','24','25','26','27'):r.u('coordinate88',f)
 if v<0x10f:r.u('247','legacySkippedByte')
 for f in('B8','B9'):r.flag('247',f)
 for f in('BA','BC'):r.u('247',f,2)
 parameters=r.at
 for f in('10','14','18','1C'):r.u('parametersC0',f,4)
 p=[r.flag('parametersC0',f) for f in('20','21','legacyThirdPresence','22')];r.u('parametersC0','24',4)
 for field,yes in zip(('28','30','38'),(p[0],p[1]or p[3],p[2])):
  if yes:r.node('parametersC0.'+field)
 enclosure=r.at
 for f,s in(('40',1),('41',1),('44',4),('48',1),('4C',4),('50',1)):
  if s==1:r.flag('parametersC0',f)
  else:r.u('parametersC0',f,4)
 n=r.u('parametersC0','60.count',4);assert n<=256
 for i in range(n):r.node(f'parametersC0.58[{i}]')
 if v>=0x10e and r.flag('247','150.present'):r.node('247.150')
 if r.flag('247','trioPresent'):
  for d,k in(('130',0x297),('138',0x21b),('140',0x21d)):r.node('247.'+d,k)
 if v>=0x110 and r.flag('247','148.present'):r.node('247.148')
 n=r.u('247','160.count',4);assert n<=256
 for i in range(n):r.node(f'247.158[{i}]')
 for field,size,n in(('16C',4,16),('1AC',2,16),('1CC',2,16),('1EC',1,1),('1F0',4,4),('200',2,4),('208',2,4),('210',1,1),('214',2,6),('220',2,6)):r.take('247',field,size,n)
 assert r.at==len(data)
 return r,dict(Version=v,Base134Version=bv,SourceKind=sk,CoordinateMode=mode,ParametersOffset=parameters,EnclosureFlagOffset=enclosure,alphabetCount=count)

def category(data,at):
 p=at;version,count=struct.unpack_from('<HH',data,p);assert version==0x201;p+=4;entries=count+3
 def ids():
  nonlocal p
  n=data[p];p=(p+4)&~3;v=struct.unpack_from('<'+'i'*n,data,p);p+=4*n;return v
 def mapping(n):
  nonlocal p
  v=struct.unpack_from('<'+'b'*entries,data,p);p+=entries;assert all(-1<=x<n for x in v);return v
 first=ids();fm=mapping(len(first));geo=ids();start=mapping(len(geo));end=mapping(len(geo))
 for i in range(len(geo)):
  proto=struct.unpack_from('<fffB',data,p);p+=13;assert all(abs(x)<float('inf') for x in proto[:3])
 zero=first.index(0);composites=[]
 for a,b,c in zip(fm,start,end):
  for x,y in((a,b),(a,c),(zero,b),(zero,c)):
   if x!=-1 and y!=-1 and(first[x],y)not in composites:composites.append((first[x],y))
 return p,count,list(first),len(composites)

def sampler(data,child,alphabetCount):
 r=Wire(data,child['Offset'],child['End']);r.take('297','envelope',1,16)
 for f in('08','0C','10','14'):r.u('297',f,4)
 for f in('18','19','1A'):r.flag('297',f)
 for f in('1C','20','24','28','2C','30'):r.u('297',f,4)
 r.u('297','12C');r.u('297','130',4);assert r.flag('297','40.categoryPresent')==1
 at=r.at;p,n,first,composites=category(data,at);assert n==alphabetCount;r.take('297','40.category201',1,p-at)
 for f in('48','4C'):r.u('297',f,4)
 r.flag('297','50')
 for f in('54','58','5C','60','68','6C','70','74'):r.u('297',f,4)
 for i in range(6):r.take('297',f'{0x78+i*24:X}.matrixRow',4,6)
 r.u('297','108',4);r.flag('297','10C')
 for f in('110','114','118','11C','120','124'):r.u('297',f,4)
 if child['Version']>=0x107:
  if r.flag('297','134.present'):return None,'134'
  if r.flag('297','1D0.present'):return None,'1D0'
 assert r.at==r.end
 return dict(categoryOffset=at,count=n,fields=r.fields,categoryFirstIds=first,categoryCompositeCount=composites),None

total=0;negatives={};rawBytes=0
for row in capture['sources']:
 data=(models/row['path']).read_bytes();assert hashlib.sha256(data).hexdigest()==row['sha256'].lower()==manifest[row['path']]
 r,values=source(data);assert all(row[k]==v for k,v in values.items())
 assert row['Offset']==0 and row['End']==len(data) and row['fields']==r.fields and row['children']==r.children
 intervals=[(f['offset'],f['offset']+f['size']*f['count'])for f in r.fields]+[(c['Offset'],c['End'])for c in r.children]
 last=0
 for a,b in sorted(intervals):assert a==last and b>=a;last=b
 assert last==len(data) #Every parent byte is assigned to a field or a delegated envelope.
 child=next(c for c in r.children if c['destination']=='247.130');expected,error=sampler(data,child,values['alphabetCount'])
 assert row['sampler']==expected
 if error:assert error in row['samplerError'];negatives[row['path']]=error
 else:assert row['samplerError']is None;total+=1
 rawBytes+=sum(len(base64.b64decode(f['raw'],validate=True))for f in r.fields)
assert total==capture['positiveSamplers']==2 and len(negatives)==capture['refusedSamplerPayloads']==3
assert capture['oldRuParity']==1 and capture['guards']==669
assert negatives=={'en-US/en_US-ak-cur.res':'1D0','en-US/en_US-ak-superimposed.lite.res':'134','en-US/en_US-ak-superimposed.res':'134'}
print(f'PASS5 full parent byte partitions/{rawBytes} retained scalar+AL bytes/2 complete known samplers/3 actual positive-payload refusals/669 guards; no child119 weights or English controller claim')
