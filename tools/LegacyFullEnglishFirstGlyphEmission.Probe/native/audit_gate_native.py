"""STATIC B7 gate byte/dispatch/whole-region audit; never loads vendor modules."""
import argparse,hashlib,json,struct
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
q=argparse.ArgumentParser();q.add_argument('--sdk',type=Path,required=True);q.add_argument('--evidence',type=Path,required=True);q.add_argument('--output',type=Path,required=True);a=q.parse_args();md=Cs(CS_ARCH_X86,CS_MODE_64)
class Module:
 def __init__(self,path):
  self.raw=path.read_bytes();p=struct.unpack_from('<I',self.raw,60)[0];o=p+24;assert self.raw[p:p+4]==b'PE\0\0'and struct.unpack_from('<H',self.raw,o)[0]==0x20b;self.base=struct.unpack_from('<Q',self.raw,o+24)[0];assert self.base==0x180000000;s=o+struct.unpack_from('<H',self.raw,p+20)[0];self.sections=[]
  for i in range(struct.unpack_from('<H',self.raw,p+6)[0]):
   _,v,n,at=struct.unpack_from('<IIII',self.raw,s+40*i+8);self.sections.append((v,n,at))
 def read(self,address,n):
  v=int(address,16)if isinstance(address,str)else address;v-=self.base
  for start,size,at in self.sections:
   if start<=v and n<=size-(v-start):return self.raw[at+v-start:at+v-start+n]
  raise AssertionError(('PE extent',hex(v),n))
def sha(b):return hashlib.sha256(b).hexdigest()
mods={n:Module(a.sdk/n)for n in ['MyScriptText.dll','MyScriptEngine.dll']};text=mods['MyScriptText.dll'];factory=json.loads((a.evidence/'gate-factory-registration.json').read_bytes());fc=json.loads((a.evidence/'gate-fc-init-reset-event.json').read_bytes());service=json.loads((a.evidence/'gate-service-class-query.json').read_bytes());provider=json.loads((a.evidence/'gate-provider-header.json').read_bytes());regions={};pins=0;leaves=0

def region(module,r):
 global leaves
 raw=module.read(r['start'],r['bytes']);assert sha(raw)==r['sha256'];ii=list(md.disasm(raw,int(r['start'],16)));assert sum(x.size for x in ii)==len(raw)
 if 'instructions'in r and isinstance(r['instructions'],int):assert len(ii)==r['instructions']
 regions[(sha(module.raw),r['start'],r['bytes'])]=len(ii)
for name,d in factory['binaries'].items():
 m=mods[name];assert sha(m.raw)==d['sha256']
 for r in d['completeRegions']:region(m,r)
 for r in d['dataPins']:assert m.read(r['start'],r['bytes'])==bytes.fromhex(r['hex']);pins+=1
assert sha(text.raw)==service['binarySha256']
for rows in fc['functions'].values():
 for r in rows:region(text,r)
region(text,fc['constructor'])
for r in fc['reads']+[fc['fcZeroWrite']]:
 raw=bytes.fromhex(r['bytes']);assert text.read(r['address'],len(raw))==raw;pins+=1
for r in service['bindings']:
 raw=bytes.fromhex(r['raw']);assert text.read(r['address'],8)==raw and struct.unpack('<Q',raw)[0]==int(r['target'],16);pins+=1
for r in service['closedUnregisteredRegions']:
 region(text,r);leaves+=1
 for i in r['instructions']:
  raw=bytes.fromhex(i['bytes']);assert text.read(i['at'],len(raw))==raw;actual=next(md.disasm(raw,int(i['at'],16)));assert actual.mnemonic+' '+actual.op_str==i['asm']
for i in service['queryAndFlagPins']:
 raw=bytes.fromhex(i['bytes']);assert text.read(i['at'],len(raw))==raw;actual=next(md.disasm(raw,int(i['at'],16)));assert actual.mnemonic+' '+actual.op_str==i['asm'];pins+=1
for r in provider['completeRegions']:region(text,r)
region(text,provider['unregisteredSymbolWindow'])
for r in provider['dataPins']:assert text.read(r['start'],r['bytes'])==bytes.fromhex(r['hex']);pins+=1
assert service['request16']['ordinal6']==65535 and service['request16']['topB']==1 and service['request16']['primitiveC']==0
assert text.read('0x1802e7a60',8)==struct.pack('<Q',0x1801f2560)and text.read('0x1802e7a70',8)==bytes(8)
assert text.read('0x1802ed438',4)==struct.pack('<f',255.)
r=dict(status='PASS_STATIC_B7_GATES',vendorExecuted=False,regions=len(regions),instructions=sum(regions.values()),closedUnregistered=leaves,exactPins=pins,scope='Bounded static initializer/reset/event and admitted ordinary source bindings; no vendor ABI execution, richpool runtime or source720 result claim');a.output.write_text(json.dumps(r,indent=2)+'\n');print(json.dumps(r))
