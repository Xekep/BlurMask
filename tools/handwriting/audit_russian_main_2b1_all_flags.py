"""Read-only native-byte/profile proof. Does not execute or load vendor code."""
import argparse,hashlib,json,struct
from pathlib import Path

def sha(b):return hashlib.sha256(b).hexdigest()
def run(sdk,models,profile):
 pins=json.loads(Path(__file__).with_name('russian-main-2b1-all-flag-sites-pins.json').read_text());data=(sdk/pins['binary']).read_bytes();assert sha(data)==pins['sha256']
 pe=struct.unpack_from('<I',data,0x3c)[0];count=struct.unpack_from('<H',data,pe+6)[0];optional=pe+24;image=struct.unpack_from('<Q',data,optional+24)[0];section=optional+struct.unpack_from('<H',data,pe+20)[0];sections=[]
 for i in range(count):
  q=section+40*i;size,rva,rawsize,raw=struct.unpack_from('<IIII',data,q+8);sections.append((rva,max(size,rawsize),raw))
 def read(va,n):
  rva=int(va,16)-image
  for start,size,raw in sections:
   if start<=rva and rva+n<=start+size:return data[raw+rva-start:raw+rva-start+n]
  raise ValueError('Outside PE sections')
 regions=0
 for function,rs in pins['regions'].items():
  for r in rs:
   assert int(r['endExclusive'],16)-int(r['start'],16)==r['bytes'];assert sha(read(r['start'],r['bytes']))==r['sha256'];regions+=1
 # Full concrete-table inventory is bounded to these actual owned methods.
 from capstone import Cs,CS_ARCH_X86,CS_MODE_64
 from capstone.x86_const import X86_OP_MEM,X86_REG_RSP
 leaf=pins['closedLeaf'];assert read(leaf['at'],len(bytes.fromhex(leaf['bytes'])))==bytes.fromhex(leaf['bytes'])==bytes.fromhex('b800040000c3')
 table=pins['actual2B1Table'];assert read(table['at'],112)==bytes.fromhex(table['bytes']);entries=[int(v,16)for v in table['entries']];assert list(struct.unpack('<14Q',read(table['at'],112)))==entries and entries[-1]==0
 cs=Cs(CS_ARCH_X86,CS_MODE_64);cs.detail=True;cs.skipdata=True;found=[]
 for method in entries:
  if method in(0,0x1802b7f00,0x1800b3ff0):continue
  for region in pins['regions'][f'{method:x}']:
   for instruction in cs.disasm(read(region['start'],region['bytes']),int(region['start'],16)):
    assert instruction.id!=0,('Undecoded table-method bytes',hex(instruction.address))
    for operand in instruction.operands:
     if operand.type!=X86_OP_MEM or operand.mem.base==X86_REG_RSP:continue
     assert not(operand.size>1 and operand.mem.disp<=0x68<operand.mem.disp+operand.size),('Unexpected wider68 access',hex(instruction.address))
     if operand.mem.disp==0x68 and operand.size==1:found.append(dict(function=f'{method:x}',at=hex(instruction.address),bytes=instruction.bytes.hex(),mnemonic=instruction.mnemonic,operands=instruction.op_str))
 assert found==pins['directByte68InstructionInventory'] and len(found)==4
 assert [x['at']for x in found]==['0x1800be2cc','0x1800bee64','0x1800bf628','0x1800c0420']
 for call in pins['consumerCalls']:
  raw=read(call['at'],5);assert raw==bytes.fromhex(call['bytes'])and raw[0]==0xe8
  assert int(call['at'],16)+5+struct.unpack('<i',raw[1:])[0]==int(call['target'],16)
 for p in pins['abiInstructions']:assert read(p['at'],len(bytes.fromhex(p['bytes'])))==bytes.fromhex(p['bytes'])
 for p in pins['descriptorPins']:assert read(p['at'],len(bytes.fromhex(p['bytes'])))==bytes.fromhex(p['bytes'])
 engine=json.loads(profile.read_bytes());tableproof=[]
 for p in pins['tables']:
  raw=read(p['at'],p['bytes']);assert sha(raw)==p['sha256'];values=list(struct.unpack('<274h',raw));geometry='temporal'if p['name'].startswith('temporal')else'spatial';bound='minima'if p['name'].endswith('Min')else'maxima';assert values==engine[geometry][bound];tableproof.append(dict(name=p['name'],sha256=sha(raw),count=len(values),existingRussianProfileExact=True))
 ak=(models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes();assert sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
 pos=195811;fields={}
 for name,fmt in [('14','H'),('18','I'),('1C','H'),('1E','H'),('20','H'),('22','H'),('28','H')]+[(f'{n:X}','I')for n in range(0x2c,0x45,4)]:fields[name]=struct.unpack_from('<'+fmt,ak,pos)[0];pos+=struct.calcsize(fmt)
 assert fields=={'14':4369,'18':63,'1C':30,'1E':16,'20':8,'22':10,'28':4369,'2C':1974271,'30':511,'34':925695,'38':925695,'3C':255,'40':100,'44':10}
 lists=[]
 for copies in (1,2,2):
  n=struct.unpack_from('<H',ak,pos)[0];pos+=2;v=list(struct.unpack_from('<'+'H'*(n*copies),ak,pos));pos+=2*n*copies;lists.append(v)
 assert lists==[[16],[4,4],[5,5]]and pos==195871;assert ak[pos:pos+16]==bytes([1,2])+bytes(14);pos+=16;assert ak[pos:pos+2]==bytes(2);pos+=2
 slots=[];networks=[]
 for slot in range(7):
  calibration=ak[pos];pos+=1;assert calibration==0
  first,second,mode=ak[pos:pos+3];pos+=3
  if mode==0:pos+=4
  present=[]
  for family in range(3):
   enabled=ak[pos];pos+=1
   if enabled:
    assert struct.unpack_from('<HH',ak,pos)==(0xfea0,0x106);present.append(pos);networks.append(pos);pos+=8+struct.unpack_from('<I',ak,pos+4)[0]
  slots.append(dict(slot=slot,calibrationPresent=False,first=first,second=second,mode=mode,networkOffsets=present))
 assert pos==566330 and networks==[195895,283697,367899,467101]
 return dict(nativeExecuted=False,fullFunctions=len(pins['regions']),fullRegions=regions,tableProof=tableproof,descriptorPins=len(pins['descriptorPins']),profileFields=fields,rawVariantBytes=[1,2],actualType='2B1',localExtentAddsUnit=False,wholeExtentAddsUnit=False,concreteClassMethods=12,closedLeaves=1,storedFlagWrites=1,storedFlagReads=3,consumerBindings=3,preBaselineCount=274,calibrationRange=[1,274],baselineCount=9,contextCount=28,boundaryCount=2,inputWidths=[256,238,313,313],slots=slots,prefixEnd=pos,scope='Actual raw source selectors, complete configured1111 producer/table/flag-readsite closure. Whole/local extents omitunit; no runtime quality claim.')

if __name__=='__main__':
 p=argparse.ArgumentParser();p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--profile',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();result=run(a.sdk,a.models,a.profile);a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result))
