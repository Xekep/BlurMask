"""Static PE/PortablePDB/source/runtime binding; never loads a managed assembly."""
from pathlib import Path
import struct,hashlib,json,uuid,sys
H=lambda b:hashlib.sha256(b).hexdigest()
def pin(p):
 p=Path(p);b=p.read_bytes();return dict(path=str(p),bytes=len(b),sha256=H(b))
def u16(b,o):return struct.unpack_from('<H',b,o)[0]
def u32(b,o):return struct.unpack_from('<I',b,o)[0]
def streams(b):
 assert b[:4]==b'BSJB';n=u32(b,12);p=(16+n+3)&~3;count=u16(b,p+2);p+=4;out={}
 for _ in range(count):
  off,size=struct.unpack_from('<II',b,p);p+=8;z=b.index(0,p);name=b[p:z].decode('ascii');p=(z+4)&~3
  assert name not in out and off+size<=len(b);out[name]=b[off:off+size]
 return out

def packed(b,p):
 n=b[p]
 if n<128:return n,p+1
 if n<192:return ((n&63)<<8)|b[p+1],p+2
 assert n<224
 return ((n&31)<<24)|(b[p+1]<<16)|(b[p+2]<<8)|b[p+3],p+4

def codeview(dll,pdbstreams):
 b=Path(dll).read_bytes();assert b[:2]==b'MZ';p=u32(b,0x3c);assert b[p:p+4]==b'PE\0\0'
 n=u16(b,p+6);opts=p+24;size=u16(b,p+20);magic=u16(b,opts);assert magic in (0x10b,0x20b)
 dirs=opts+(96 if magic==0x10b else 112);rva,length=struct.unpack_from('<II',b,dirs+6*8);assert length%28==0
 sec=opts+size;segments=[]
 for i in range(n):
  at=sec+40*i;virtual,va,rawsize,raw=struct.unpack_from('<IIII',b,at+8);segments.append((va,max(virtual,rawsize),raw))
 def off(rva):
  for va,size,raw in segments:
   if va<=rva<va+size:return raw+rva-va
  raise AssertionError(('unmappedRVA',hex(rva)))
 at=off(rva);records=[]
 for i in range(length//28):
  row=b[at+28*i:at+28*(i+1)];kind=u32(row,12);count=u32(row,16);raw=u32(row,24)
  if kind!=2:continue
  data=b[raw:raw+count];assert data[:4]==b'RSDS' and len(data)>=25
  assert data[4:20]==pdbstreams['#Pdb'][:16] and u32(data,20)==1
  assert u32(row,4)==u32(pdbstreams['#Pdb'],16)
  records.append(dict(guid=str(uuid.UUID(bytes_le=data[4:20])),stamp=u32(row,4),age=1,path=data[24:].rstrip(b'\0').decode('utf-8')))
 assert len(records)==1;return records

def audit_pdb(pdb,dll,authored,generatedroot):
 raw=Path(pdb).read_bytes();s=streams(raw);t=s['#~'];heap=t[6];valid=struct.unpack_from('<Q',t,8)[0];p=24;rows={}
 for i in range(64):
  if valid>>i&1:rows[i]=u32(t,p);p+=4
 assert not any(i<48 for i in rows)
 bs=4 if heap&4 else 2;gs=4 if heap&2 else 2
 def index(n):
  nonlocal p
  x=int.from_bytes(t[p:p+n],'little');p+=n;return x
 def blob(n):
  size,start=packed(s['#Blob'],n);assert start+size<=len(s['#Blob']);return s['#Blob'][start:start+size]
 def guid(n):
  assert n>0;return str(uuid.UUID(bytes_le=s['#GUID'][(n-1)*16:n*16]))
 docs=[];seen=set();author=0;generated=0
 for _ in range(rows.get(48,0)):
  name=blob(index(bs));algorithm=guid(index(gs));checksum=blob(index(bs));language=guid(index(gs));parts=[];q=1
  assert name and algorithm=='8829d00f-11b8-4213-878b-770e8597ac16'
  assert language=='3f5162f8-07c6-11d3-9053-00c04fa302a1'
  while q<len(name):n,q=packed(name,q);parts.append(blob(n).decode('utf-8'))
  path=Path(chr(name[0]).join(parts));canonical=path.resolve();assert canonical not in seen;seen.add(canonical)
  data=path.read_bytes();assert checksum.hex()==H(data),(str(path),checksum.hex(),H(data))
  kind='authored' if canonical in authored else 'generated'
  if kind=='authored':author+=1
  else:
   assert canonical.is_relative_to(generatedroot.resolve()) and path.suffix=='.cs',('unexpecteddocument',str(path));generated+=1
  docs.append(dict(**pin(path),algorithm=algorithm,language=language,kind=kind))
 assert authored<=seen,('missingauthored',sorted(str(p) for p in authored-seen))
 return dict(pdb=pin(pdb),dll=pin(dll),codeView=codeview(dll,s),authoredDocuments=author,generatedDocuments=generated,documents=docs)

