"""Independent raw-wire oracle. Uses only Python standard library and models."""
from pathlib import Path
import base64,hashlib,json,struct,sys
if len(sys.argv)!=3:raise SystemExit('models-directory capture.json')
root=Path(sys.argv[1]);capture=json.loads(Path(sys.argv[2]).read_bytes())
manifest={v['path']:v['sha256']for v in json.loads((root/'manifest.json').read_bytes())['resources']}
def node(d,p,limit):
 magic,size,kind,flags,version=struct.unpack_from('<IIIHH',d,p)
 assert magic==0x101fe90 and flags==0 and 8<=size<=limit-p-8
 return p+size+8,kind,version
def source(d,p):
 end,kind,version=node(d,p,len(d));assert kind==0x247
 q=p+16;base,parent=struct.unpack_from('<HH',d,q);assert base in(0x201,0x300)and parent==0x100;q+=64
 marker=struct.unpack_from('<i',d,q)[0];q+=8+(4 if marker==-1 else 0)
 assert d[q]==1 and struct.unpack_from('<I',d,q+1)[0]==0x2b;av=struct.unpack_from('<H',d,q+5)[0];q+=7
 def u(fmt,n=1):
  nonlocal q
  assert 0<=n<=65536 and q+struct.calcsize('<'+fmt)*n<=end
  v=struct.unpack_from('<'+fmt*n,d,q);q+=struct.calcsize('<'+fmt)*n;return v
 count=u('H',8)[-1];n=u('H')[0]
 for step in(32,64,1):n=next(v for v in reversed(u('H',n))if v!=65535)+step
 spans=u('H',n+1);entries=[u('II')for _ in range(spans[-1]+1)];q+=entries[-1][0];u('H',count)
 if u('B')[0]:u('B',(count+7)//8)
 triples=u('H')[0];u('H',triples*3)
 if triples:
  flags=u('I')[0]
  for bit in(1,2,4):
   if flags&bit:u('B'if triples<128 else'H',count)
 if av==0x104:u('B')
 else:assert av==0x103
 children=[]
 def child(role):
  nonlocal q
  z,k,v=node(d,q,end);children.append((role,q,z,k,v));q=z
 if base==0x300:
  for i in range(u('B')[0]):child('134.68')
 sk=u('I')[0];mode=u('B')[0]
 if mode==2:
  n=u('B')[0];q+=n*4+15
 if version<0x10f:q+=1
 q+=22;flags=u('BBBB');q+=4
 for role,yes in zip(('28','30','38'),(flags[0],flags[1]or flags[3],flags[2])):
  if yes:child(role)
 q+=12
 for i in range(u('i')[0]):child('processor')
 if version>=0x10e and u('B')[0]:child('150')
 if u('B')[0]:
  for role in('130','138','140'):child(role)
 if version>=0x110 and u('B')[0]:child('148')
 for i in range(u('i')[0]):child('158')
 assert q+186==end
 return end,sk,count,children

class Acoustic:
 def __init__(self,d,p,end):self.d=d;self.p=p;self.end=end;self.fields=[]
 def take(self,owner,name,size=1,count=1):
  assert 0<=count and self.p+size*count<=self.end
  raw=self.d[self.p:self.p+size*count];self.fields.append(dict(Owner=owner,Destination=name,Offset=self.p,ElementSize=size,Count=count,Raw=base64.b64encode(raw).decode()));self.p+=len(raw);return raw
 def u(self,name,size=1,owner='21D'):return int.from_bytes(self.take(owner,name,size),'little')
 def f(self,name,owner='21D'):
  v=struct.unpack('<f',self.take(owner,name,4))[0];assert abs(v)<float('inf');return v
 def lists(self,count,name,copies,owner='profile0'):
  n=self.u(count,2,owner);assert n<=1024;self.take(owner,name,2,n*copies)

networks=0;wirebytes=0
assert len(capture['Models'])==2 and capture['Guards']>=181 and capture['TailTruncations']==76
for item in capture['Models']:
 d=(root/item['Path']).read_bytes();assert hashlib.sha256(d).hexdigest()==manifest[item['Path']]
 parentEnd,_,_,children=source(d,0);chosen=next(c for c in children if c[3]==0x249);_,at,stop,_,v=chosen
 assert at==item['Processor'] and v==0x100 and item['ParentFingerprint'].lower()==hashlib.sha256(d[:parentEnd]).hexdigest()
 start=at+16;end,kind,alphabet,children=source(d,start);assert kind==0x135 and end==stop-4
 assert(start,end,alphabet)==(item['Source'],item['SourceEnd'],item['AlphabetCount'])
 assert item['SourceScoreExponent']==struct.unpack_from('<f',d,end)[0]
 _,at,end,kind,version=next(c for c in children if c[0]=='140');assert kind==0x21d and version==0x119
 assert item['Acoustic']==dict(Offset=at,End=end) and item['Fingerprint'].lower()==hashlib.sha256(d[at:end]).hexdigest()
 r=Acoustic(d,at,end);r.take('21D','envelope',1,16);r.u('08');r.u('09');target=r.u('0A',2);mult=r.f('0C')
 assert(target,mult)==(item['RawNormalizationTarget'],item['CostMultiplier'])
 assert r.u('18.initialPresent')==r.u('10.dynamicPresent')==0 and r.u('68.profileCount')==1 and r.u('00.gridPresent',owner='profile0')==0
 r.u('14',2,'profile0');r.u('18',4,'profile0')
 for f in('1C','1E','20','22','28'):r.u(f,2,'profile0')
 for f in('2C','30','34','38','3C','40','44'):r.f(f,'profile0')
 for c,v,n in(('48','50',1),('58','60/68',2),('70','78/80',2)):r.lists(c,v,n)
 r.u('88',owner='profile0');r.u('89',owner='profile0');r.u('90',2,'profile0')
 for f in('94','98'):r.f(f,'profile0')
 for f in('9C','9E'):r.u(f,2,'profile0')
 assert r.u('optionalA0',owner='profile0')==r.u('optionalPair',owner='profile0')==0
 for index,slot in enumerate(item['Slots']):
  owner=f'slot{index}';assert r.u('conversionPresent',owner=owner)==0
  first=r.u('first',owner=owner);second=r.u('second',owner=owner);mode=r.u('mode',owner=owner);weight=r.f('weight',owner)if mode==0 else None
  assert(index,bool(first),bool(second),mode,weight)==(slot['Index'],slot['First'],slot['Second'],slot['Mode'],slot['Weight'])
  actual=[]
  for position in range(3):
   present=r.u(f'network{position}.present',owner=owner)
   if not present:continue
   assert position==1;p=r.p;magic,v,size,architecture,kind,inputs,hidden,unused1,unused2,outputs,trailing=struct.unpack_from('<HHIBHIIIIIH',d,p)
   z=p+8+size;assert magic==0xfea0 and v==0x107 and architecture==0x12 and unused1==unused2==0 and trailing==10 and z<=end
   matrices=[];weights=[];scales=[];q=p+33
   for cols,rows in((inputs,hidden-1),(hidden,outputs-1)):
    q=(q+15)//16*16;stride=(cols+15)//16*16;scale=q+stride*rows;assert d[scale]==1 and all(d[scale+1:scale+rows+1])
    matrices.append(dict(WeightsOffset=q,ScalesOffset=scale,Columns=cols,Stride=stride,Rows=rows))
    weights.append(hashlib.sha256(d[q:scale]).hexdigest());scales.append(hashlib.sha256(d[scale:scale+rows+1]).hexdigest());q=scale+rows+1
   assert q==z;network=slot['Networks'][len(actual)];actual.append(network);networks+=1
   assert(network['Offset'],network['End'],network['Kind'],network['InputWidth'],network['HiddenWidth'],network['OutputWidth'])==(p,z,kind,inputs,hidden-1,outputs-1)
   assert network['Matrices']==matrices and [x.lower()for x in network['WeightHashes']]==weights and [x.lower()for x in network['ScaleHashes']]==scales
   assert base64.b64decode(network['Raw'])==d[p:z] and network['Fingerprint'].lower()==hashlib.sha256(d[p:z]).hexdigest()
   r.take(owner,f'network{position}.107',1,z-p)
  assert len(actual)==len(slot['Networks'])
 assert r.p==item['PrefixEnd']
 r.u('78');r.f('7C');r.u('80');r.f('88');r.f('84')
 for i in range(r.u('90',2)):r.lists(f'98[{i}].count',f'98[{i}]',1,'21D')
 for f in('A4','A8','AC','B0','B4'):r.f(f)
 r.u('B8');r.f('BC');r.f('C0');groups=r.u('C8',2)
 if groups:
  boundaries=struct.unpack('<'+'H'*(groups+1),r.take('21D','D0',2,groups+1));assert boundaries==tuple(sorted(boundaries)) and boundaries[0]==0
  labels=struct.unpack('<'+'H'*boundaries[-1],r.take('21D','D8',2,boundaries[-1]));assert all(n<alphabet for n in labels)
  for i in range(groups):r.f(f'E0[{i}]')
 r.u('E8',4);r.u('EC',2);r.u('EE',2)
 for f in('F0','F4','F8'):r.u(f,4)
 r.u('FC');r.f('100');assert r.u('108',4)==r.u('128',4)==r.u('148/150.present')==0
 r.f('120');r.f('124');assert r.p==end and r.fields==item['Fields'];wirebytes+=end-at
print(f'PASS independent actual249/source247/21D119/107:2 owners/{networks} networks/{wirebytes} complete acoustic bytes; no research imports')
