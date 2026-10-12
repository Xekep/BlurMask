"""Opt-in read-only pinned PE audit of ordinary dictionary owner dispatch. Never loads DLLs."""
import argparse, hashlib, json, struct
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('text_dll',type=Path);p.add_argument('report',type=Path);a=p.parse_args()
data=a.text_dll.read_bytes();digest=hashlib.sha256(data).hexdigest()
if digest!='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f':raise ValueError('Unrecognized Text binary; owner binding is pinned')
pe=struct.unpack_from('<I',data,0x3c)[0]
if data[pe:pe+4]!=b'PE\0\0' or struct.unpack_from('<H',data,pe+4)[0]!=0x8664:raise ValueError('Expected x64 PE')
n=struct.unpack_from('<H',data,pe+6)[0];opt=struct.unpack_from('<H',data,pe+20)[0];base=struct.unpack_from('<Q',data,pe+24+24)[0]
sections=[struct.unpack_from('<IIII',data,pe+24+opt+i*40+8) for i in range(n)]
def off(va):
 rva=va-base
 for size,virtual,raw_size,raw in sections:
  if virtual<=rva<virtual+raw_size:return raw+rva-virtual
 raise ValueError(f'Unmapped address {va:x}')
words=[(0x180274af0+0x10,0x18000caa0,'container frontend setter'),(0x180274af0+0x18,0x18000cab0,'container linguistic setter'),(0x1802e7cb0+0x18,0x18020c1c0,'class240 frontend attachment'),(0x1802b4360+0x10,0x180093970,'dictionary preparation owner'),(0x1802b4360+0x88,0x180093ab0,'dictionary eligibility'),(0x1802de610+0x280,0x180155710,'frontend graph getter'),(0x1802e1ff0+0x1c8,0x18005a400,'ordinary graph false getter')]
for va,target,label in words:
 actual=struct.unpack_from('<Q',data,off(va))[0]
 if actual!=target:raise ValueError(f'{label} binding differs: {actual:x}')
proofs=[(0x18000caa0,'4c894248b0014d85c0740449895008c3','container+48/frontend+8 identity'),(0x18000cab0,'4c894260b0014d85c0740449895018c3','container+60/controller+18 identity'),(0x180155710,'488b5270488b024c8b88c801000049ffe1','frontend+70 graph virtual1C8'),(0x18005a400,'32c0c3','ordinary graph returns false')]
for va,hexbytes,label in proofs:
 expected=bytes.fromhex(hexbytes)
 if data[off(va):off(va)+len(expected)]!=expected:raise ValueError(f'{label} implementation differs')
report=dict(binarySha256=digest,staticOnly=True,tableBindings=[dict(address=f'{va:x}',target=f'{target:x}',role=label) for va,target,label in words],byteProofs=[dict(address=f'{va:x}',bytes=code,role=label) for va,code,label in proofs],ordinaryFrontendMode280=False,scope='Prepared class2b3/container + class222/unwrapped21b; upstream factory/preparation outcomes remain explicit')
a.report.parent.mkdir(parents=True,exist_ok=True);a.report.write_text(json.dumps(report,indent=2),encoding='utf8');print('PASS 7 pinned table bindings and 4 exact static implementation proofs; no vendor execution')
