"""Read-only raw21D/119, first249 lineage and24C inventory. No SDK execution.
This is a static research auditor, not a portable runtime/recognizer loader.
"""
from pathlib import Path
import hashlib,json,struct,sys
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,Reader,parse_alphabet,functions
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();f=functions();md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[
 (0x1801c2940,0x1801c47d0,'5bf1119ed12c9af7188a5922d724d37e677dceffe6025dbf50c0c1fbbc0e8150'),
 (0x1801c25b0,0x1801c2940,'f27f1daf1110086acc80a5c2eb0081e1e40c188f5bf92c2c4e5e12e48c02af33'),
 (0x1801b0760,0x1801b3ce0,'184c45caa2b952f13cea9fa20752a92c2693abe563bbf306b7c3e2a700a2992a'),
 (0x1801be930,0x1801bf0a0,'3540bf30e5f3361cc2276f4d73ae13a8584cbac60e30dad79abe34de37158f4e'),
 (0x1801bf0a0,0x1801bf610,'d44f0f47695233a86b2b2212aa8326abe3542b7b24a1e5eae39817e9691a48d0'),
 (0x180176a10,0x180176fb0,'16899888bda61da733ac6261995cea2973ce6cb3703b404354d0b2f95ad1cea9'),
 (0x180176fb0,0x180177060,'58a690d8516fce062d08a6939f6de7e24042d379f83404329600575a2c75d017'),
 (0x1800ec270,0x1800ec890,'7b9833bb7cd0183eb335c7efc7bc03cd46925dc89c8d8131ed13026952c7b0a1'),
 (0x1800eefd0,0x1800ef040,'fa8126cdcc168e52ad016da740703a640a1f0569aa86f70bcb135fa72620fdda')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
pins={
0x1801c29e8:('mov','eax, 0x118'),0x1801c29f7:('lea','r8, [rbp + 0xc]'),
0x1801c2a30:('mov','dword ptr [rbp + 0xc], 0x3f800000'),
0x1801c3ad3:('mov','eax, 0x116'),0x1801c3d15:('mov','eax, 0x113'),
0x1801c3e6c:('mov','eax, 0x115'),0x1801c4074:('mov','eax, 0x119'),
0x1801b0e54:('cmp','byte ptr [rsi + 0x10], dil'),0x1801b0e58:('je','0x1801b0ec9'),
0x1801b0ec9:('mov','qword ptr [rsi + 0x1b0], r14'),
0x180176b0b:('cmp','r9d, 0x14'),0x180176b0f:('jl','0x180176f92'),
0x180083980:('mov','eax, 0x10')}
for a,v in pins.items():
 i=next(md.disasm(b.data[b.off(a):b.off(a)+16],a));assert(i.mnemonic,i.op_str)==v
assert b.qwords(0x1802dfe20+0x18,1)[0]==0x180176a10
assert b.qwords(0x1802dfe78+0x18,1)[0]==0x180176fb0
for table in(0x1802ba600,0x1802ba4e0,0x1802ba660,0x1802ba5a0,0x1802ba6c0,0x1802ba540):
 assert b.qwords(table,1)[0]==0x180083980
assert 'param_4 < 0x107' in f['1800ec490'] and 'uVar3 - uVar1 % uVar3' in f['1800ec490']
assert 'param_2[0x36] = 0' in f['1801b0760']
for k in ['1801be930','1801bf0a0']:
 assert 'param_2 + 0x1b0' in f[k] and 'FUN_1801b7440' in f[k]

def envelope(d,at,end):
 assert 0<=at<=end-16<=len(d)-16
 magic,size,kind,flags,version=struct.unpack_from('<IIIHH',d,at)
 assert magic==0x101fe90 and flags==0 and 8<=size<=end-at-8
 return dict(offset=at,end=at+8+size,kind=kind,version=version)

def source_layout(d,at,parentEnd):
 root=envelope(d,at,parentEnd);assert root['kind']==0x247 and root['version']in(0x10d,0x110)
 r=Reader(d,at+16,root['end']);base=r.read('H');assert base in(0x201,0x300) and r.read('H')==0x100
 r.bytes(60);marker=r.read('i')
 if marker==-1:r.bytes(4)
 r.bytes(4);assert r.read('B')==1 and r.read('I')==0x2b
 version=r.read('H');assert version in(0x103,0x104)
 #The research helper includes104's finalbyte unconditionally. Native103
 #does not serialize that byte; retain the actual version-specific boundary.
 al=parse_alphabet(d,r.pos);r.pos=al['end']-(version==0x103);children=[]
 def child(role):
  n=envelope(d,r.pos,r.end);n['destination']=role;children.append(n);r.pos=n['end'];return n
 if base==0x300:
  for i in range(r.read('B')):child(f'134.68[{i}]')
 sk=r.read('I');mode=r.read('B');assert sk in range(0x134,0x138)and mode in range(3)
 if mode==2:r.bytes(r.read('B')*4+15)
 if root['version']<0x10f:r.bytes(1)
 r.bytes(6+16);p=list(r.read('BBBB'));assert all(x in(0,1)for x in p);r.bytes(4)
 for role,yes in zip(('28','30','38'),(p[0],p[1]or p[3],p[2])):
  if yes:child('parametersC0.'+role)
 r.bytes(12);n=r.read('i');assert 0<=n<=256
 for i in range(n):child(f'parametersC0.58[{i}]')
 if root['version']>=0x10e and r.read('B'):child('247.150')
 if r.read('B'):
  for role in('130','138','140'):child('247.'+role)
 if root['version']>=0x110 and r.read('B'):child('247.148')
 n=r.read('i');assert 0<=n<=256
 for i in range(n):child(f'247.158[{i}]')
 r.bytes(186);assert r.pos==r.end
 return dict(**root,baseVersion=base,sourceKind=sk,alphabetVersion=version,alphabetCount=al['label_count'],children=children)

def acoustic(d,node):
 assert node['kind']==0x21d and node['version']in(0x110,0x119)
 r=Reader(d,node['offset']+16,node['end']);version=node['version'];networks=[]
 def flag():
  value=r.read('B');assert value in(0,1);return value
 def network():
  p=r.pos;magic,v,size=r.read('HHI');end=p+8+size;assert magic==0xfea0 and v in(0x106,0x107) and end<=r.end
  kind,inputs,hidden,unused1,unused2,outputs,trailing=struct.unpack_from('<HIIIIIH',d,p+9)
  assert d[p+8]==0x12 and unused1==unused2==0 and trailing==10
  assert all(1<n<=4096 for n in(inputs,hidden,outputs));q=p+33;matrices=[]
  for cols,rows in((inputs,hidden),(hidden,outputs)):
   if v==0x107:q=(q+15)&~15;stride=(cols+15)&~15
   else:stride=cols
   weights=q;q+=stride*(rows-1);assert q+rows<=end and d[q]==1 and all(d[q+i]for i in range(1,rows))
   matrices.append(dict(weights=weights,stride=stride,rows=rows-1,scales=q));q+=rows
  assert q==end;r.pos=end
  n=dict(offset=p,end=end,version=v,kind=kind,inputs=inputs,hidden=hidden-1,outputs=outputs-1,
   matrices=matrices,sha256=hashlib.sha256(d[p:end]).hexdigest());networks.append(n);return n
 def shorts(copies):
  count=r.read('h');assert 0<=count<=1024;value=list(r.read('H',count*copies))if count*copies>1 else([r.read('H')]if count else[]);return value
 flags=list(r.read('BB'));target=r.read('h');mult=r.read('f')if version>=0x118 else 1
 initial=network()if flag()else None;context=None
 if initial:context=list(r.read('fffBBHfBBBB'))
 dynamic=list(r.read('H9f'))if flag()else None;profiles=[]
 for ordinal in range(r.read('B')):
  start=r.pos;grid=list(r.read('IIBf'))if flag()else None
  r.bytes(44);lists=[shorts(1),shorts(2),shorts(2)];r.bytes(16);assert flag()==0
  aux=shorts(2)if flag()else[];slots=[]
  for i in range(7):
   conversion=shorts(2)if flag()else[];first,second,mode=r.read('BBB');assert first in(0,1)and second in(0,1)and mode in(0,1,2)
   weight=r.read('f')if mode==0 else None;nn=[]
   for position in range(3):
    if flag():nn.append(dict(position=position,**network()))
   slots.append(dict(slot=i,first=first,second=second,mode=mode,weight=weight,conversion=conversion,networks=nn))
  profiles.append(dict(ordinal=ordinal,offset=start,end=r.pos,grid=grid,lists=lists,aux=aux,slots=slots))
 prefixEnd=r.pos;cache={}
 def value(name,fmt):cache[name]=r.read(fmt);return cache[name]
 for name,fmt in(('78','B'),('7C','f'),('80','B'),('88','f'),('84','f')):value(name,fmt)
 cache['98']=[shorts(1)for _ in range(value('90','H'))]
 if version<0x116:value('A0','f')
 for name in('A4','A8','AC','B0','B4'):value(name,'f')
 value('B8','B');value('BC','f');value('C0','f')
 if 0x10c<=version<0x114:assert flag()==0
 count=value('C8','h');assert 0<=count<=512
 if count:
  cache['D0']=[r.read('H')for _ in range(count+1)];assert cache['D0']==sorted(cache['D0'])
  cache['D8']=[r.read('H')for _ in range(cache['D0'][-1])]
  cache['E0']=[r.read('f')for _ in range(count)]if version>=0x115 else[r.read('f')]*count
 for name,fmt in(('E8','i'),('EC','H'),('EE','H'),('F0','i'),('F4','i'),('F8','i'),('FC','B'),('100','f')):value(name,fmt)
 if version<0x119:assert flag()==0
 calibration=value('108','i');assert calibration in(0,1,2)
 if calibration:
  width=value('10C','H');assert r.read('H')==0x100;r.pos=(r.pos+3)&~3
  count=width*(len(networks)-(initial is not None))*(2 if calibration==1 else 1)
  cache['calibrationFloatCount']=count;r.bytes(count*4)
 count=value('128','i');assert 0<=count<=4096
 if count:
  for name in('12C','130','134'):value(name,'i')
  cache['138']=r.bytes(count).hex()
  if version>=0x112:value('140','i');value('144','i')
 if version>0x110:assert value('148/150present','B')==0
 value('120','f');value('124','f')
 if 0x110<=version<0x117:assert flag()==0
 assert r.pos==r.end
 return dict(**node,flags=flags,target=target,multiplier=mult,initial=initial,context=context,dynamic=dynamic,
  profiles=profiles,prefixEnd=prefixEnd,tailBytes=node['end']-prefixEnd,cache=cache)

models=[];manifest={x['path']:x['sha256']for x in json.loads((ROOT/'models/manifest.json').read_bytes())['resources']}
for name in('en-US/en_US-ak-cur.lite.res','en-US/en_US-ak-cur.res','en-US/en_US-ak-superimposed.lite.res',
 'en-US/en_US-ak-superimposed.res','ru-RU/ru_RU-ak-cur.lite.res'):
 d=(ROOT/'models'/name).read_bytes();sha=hashlib.sha256(d).hexdigest();assert sha==manifest[name]
 root=source_layout(d,0,len(d));main=acoustic(d,next(c for c in root['children']if c['destination']=='247.140'))
 processor=next(c for c in root['children']if c['kind']==0x249);assert processor['version']==0x100
 first=source_layout(d,processor['offset']+16,processor['end']);assert first['end']==processor['end']-4
 firstModel=acoustic(d,next(c for c in first['children']if c['destination']=='247.140'))
 operators=[c for c in first['children']if c['destination'].startswith('parametersC0.58[')]
 if name.startswith('en-US'):
  assert main['initial']is None and firstModel['initial']is None and firstModel['dynamic']is None
  assert [c['kind']for c in operators]==[0x24a,0x24c,0x265]
  turn=operators[1];assert turn['version']==0x100 and turn['end']-turn['offset']==24
  turn['angleBits']=[hex(x)for x in struct.unpack_from('<II',d,turn['offset']+16)]
  assert main['cache']['B8']==1 and main['cache']['C8']==7 and main['cache']['108']==0
  assert firstModel['cache']['B8']==0 and firstModel['cache']['C8']==8 and firstModel['cache']['108']==0
 else:assert[c['kind']for c in operators]==[0x24a,0x265]
 models.append(dict(path=name,sha256=sha,source=root,main=main,first=first,firstAcoustic=firstModel,
  firstSourceScoreExponent=struct.unpack_from('<f',d,first['end'])[0]))
report=dict(noVendorExecution=True,readOnly=True,
 regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],
 pins=[dict(address=hex(a),instruction=' '.join(v))for a,v in pins.items()],models=models,
 scope='Actual resource and pinned-reader inventory only. No119 typed model/evaluator/controller or24C geometry port. Current freshRU andfrozenwire APIs unchanged.')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS{len(regions)} PE ranges/{len(pins)} pins/5 main+5 actual249 first acoustic nodes toexactend; no vendor execution/runtime enable')
