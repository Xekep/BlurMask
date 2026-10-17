"""Static219580 scan, actual frontend boundary thunks and arc argument."""
import hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a,z=0x180219580,0x180219f80;raw=b.data[b.off(a):b.off(z)]
assert hashlib.sha256(raw).hexdigest()=='9825287ed9aabcefcad22ff893fad2eb058e8be88a54e7614f82990a2e5a7f64'
md=Cs(CS_ARCH_X86,CS_MODE_64);instructions={i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)}
pins={
 0x18021989a:('call','0x180218a30'),
 0x18021989f:('mov','dword ptr [r14 + 0x24a0], ebx'),
 0x1802198af:('cmp','eax, 1'),
 0x1802198bf:('sar','rax, 0x18'),
 0x1802198c3:('movzx','ecx, ax'),
 0x1802198cb:('lea','r9, [rcx - 0x10000]'),
 0x1802198d2:('cmovae','r9, rcx'),
 0x18021990b:('mov','r10, qword ptr [rax + 0x168]'),
 0x180219912:('call','r10'),
 0x180219935:('add','eax, r9d'),
 0x180219938:('cmp','eax, dword ptr [rsi + 8]')}
for at,expected in pins.items():assert instructions[at]==expected,(hex(at),instructions.get(at))
bindings=[(0x1802de610,0x168,0x180152a10),(0x1802de610,0x170,0x180152a30),(0x1802de610,0x178,0x180152ae0),
 (0x1802e1ff0,0xb8,0x1801a7bf0),(0x1802e1ff0,0xc0,0x1801a7c70),(0x1802e1ff0,0xd0,0x1801a7cf0)]
for table,slot,expected in bindings:assert b.qwords(table+slot,1)==(expected,)
decode=[(i.mnemonic,i.op_str)for i in md.disasm(b.data[b.off(0x1801a7cf0):b.off(0x1801a7d0c)],0x1801a7cf0)]
assert decode==[('sar','r8d, 7'),('mov','ecx, r8d'),('and','ecx, 0xffffff'),('bt','r8d, 0x17'),('lea','eax, [rcx - 0x1000000]'),('cmovae','eax, ecx'),('ret','')]
Path(sys.argv[1]).write_text(json.dumps(dict(sha256=hashlib.sha256(raw).hexdigest(),pins=len(pins),bindings=len(bindings),decoderInstructions=len(decode),
 scope='Prepared219580 candidate scan only; actual218A30/emission/WORD augmentation not inferred.',vendorExecution=False,topV68Completed=False),indent=2)+'\n')
print('PASS full219580 region/',len(pins),'pins/6boundary bindings/7context decoder instructions; real R9 arc argument')
