"""Read-only static checks for fresh selected-child context policy; no DLL load."""
import argparse,hashlib,json,struct
from pathlib import Path
import capstone
from audit_legacy_character_properties import PE

EXPECTED='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
p=argparse.ArgumentParser();p.add_argument('--dll',type=Path,default=Path('research/myscript/sdk/build/x64/MyScriptText.dll'));p.add_argument('--output',type=Path,required=True);args=p.parse_args()
raw=args.dll.read_bytes()
if hashlib.sha256(raw).hexdigest()!=EXPECTED:raise ValueError('Unknown native evidence fingerprint')
pe=PE(raw)
checks={0x18000c8f3:'668987c4000000',0x1801e88f9:'8888c4000000'}
for address,expected in checks.items():
 if bytes(pe.read(address,f'{len(expected)//2}B')).hex()!=expected:raise ValueError(f'Native C4 write changed at{address:x}')
tables={0x180274af0+0x18:0x18000cab0,0x180274af0+0x28:0x18000cb50,0x1802e7418+8:0x1801eb340}
for address,target in tables.items():
 if pe.read(address,'Q')[0]!=target:raise ValueError(f'Native owner/binder table changed at{address:x}')
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);cs.detail=True;cs.skipdata=True
writes=[]
for rva,size,offset in pe.sections:
 if rva!=0x1000:continue
 for insn in cs.disasm(raw[offset:offset+size],pe.base+rva):
  if not insn.id or not insn.operands or not insn.mnemonic.startswith('mov'):continue
  dest=insn.operands[0]
  if dest.type!=capstone.x86.X86_OP_MEM or dest.mem.disp!=0xc4 or dest.size>2:continue
  if dest.mem.base in [capstone.x86.X86_REG_RSP,capstone.x86.X86_REG_RBP]:continue
  writes.append(dict(address=f'{insn.address:x}',instruction=insn.mnemonic+' '+insn.op_str))
if [int(w['address'],16) for w in writes]!=[0x18000c8f3,0x1801e88f9]:raise ValueError('Nonstack direct byte/word C4 write inventory changed')
args.output.parent.mkdir(parents=True,exist_ok=True)
args.output.write_text(json.dumps(dict(schemaVersion=1,dllSha256=EXPECTED,tableChecks={f'{a:x}':f'{b:x}' for a,b in tables.items()},nonstackDirectC4Writes=writes,
 scope='fresh new selected child249; caller ownership chain required, no generic indirect memory-writer completeness claim'),indent=2),encoding='utf-8')
print('PASS pinned native constructor/owner tables and two direct nonstack C4 writes; DLL was not executed.')
