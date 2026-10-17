"""Static fresh source219 ->live22A ->027670 ->exact C21.VE8 proof."""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
b=Binary();ink=Binary('MyScriptInk.dll');f=functions();m=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(ink.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
def dis(image,s,e):return[(i.mnemonic,i.op_str)for i in m.disasm(image.data[image.off(s):image.off(e)],s)]
def pins(image,s,e,expected):
 rows=dis(image,s,e)
 for pin in expected:assert pin in rows,(hex(s),pin)
 return rows
#The actual initializer's source service, not an unrelated object table.
init=f['180002530'];marker='*(undefined8 *)(*(longlong *)(param_1 + 0x18) + 0x38) = *(undefined8 *)(puVar7 + 0x20);'
assert marker in init and 'puVar7 = &DAT_1802df150;'in init
assert b.qwords(0x1802df150+0x20,1)[0]==0x1802df0a0
assert b.qwords(0x1802df0a0+0x80,1)[0]==0x180169740
assert dis(b,0x180169740,0x180169748)==[('mov','rax, qword ptr [rdx + 0x120]'),('ret','')]
assert b.qwords(0x1802de610+0x250,1)[0]==0x1801530f0
assert dis(b,0x1801530f0,0x1801530f5)==[('mov','rax, qword ptr [rdx + 0x60]'),('ret','')]
pins(b,0x1800284f6,0x180028513,[('mov','r8, qword ptr [rax + 0x250]'),('call','r8'),('mov','qword ptr [rbp + 0x38], rax')])
pins(b,0x180028719,0x18002875b,[('mov','rdx, qword ptr [rbp + 0x38]'),('mov','rax, qword ptr [rax + 0x38]'),
 ('mov','r8, qword ptr [rax + 0x80]'),('call','r8'),('movzx','ecx, byte ptr [rax + 0x14]'),
 ('mov','byte ptr [r14 + 0x22a], cl'),('cmp','dword ptr [rax + 0x70], 0'),('mov','rcx, qword ptr [rax + 0x68]')])
#Fresh no-property-event lineage: forwarding is not replaced by2DA ctor zero.
assert b.qwords(0x180274f20,3)==(0x1012d0070,0x18000d020,0x18000d000)
assert b.qwords(0x1802df2a0,3)==(0x1012d0070,0x1801690c0,0x1801690d0)
pins(b,0x18000c930,0x18000c945,[('mov','word ptr [rdi + 0x10], 0')])
pins(b,0x18000d020,0x18000d02a,[('movzx','eax, byte ptr [rdx + 0x10]')])
pins(b,0x1801690d0,0x1801690dd,[('mov','byte ptr [rdx + 0x80], al')])
assert 'FUN_180150800(param_1,param_2,0x12d0070)'in f['180150d00']
assert '*(undefined8 *)(param_2 + 8),param_3' in f['180150800'] and 'local_res20 + 0x10'in f['180150800']
pins(b,0x180168267,0x18016827b,[('mov','rcx, qword ptr [rdi + 0x120]'),('movzx','eax, byte ptr [rdi + 0x80]'),('mov','byte ptr [rcx + 0x14], al')])
#R15 is cleared before the whole B8 write: BC selects document alternative0.
before=pins(b,0x180028771,0x1800287bf,[('xor','r15d, r15d'),('mov','qword ptr [r14 + 0xb8], r15')])
assert before.index(('xor','r15d, r15d'))<before.index(('mov','qword ptr [r14 + 0xb8], r15'))
pins(b,0x18002965b,0x1800296b3,[('cmp','dword ptr [rdi + 8], 1'),('jbe','0x18002968d'),
 ('cmp','byte ptr [r14 + 0x22a], 0'),('je','0x18002968d'),('test','rax, rax'),('call','0x180026090'),
 ('call','0x180027670'),('call','0x1800248d0')])
pins(b,0x180027699,0x1800276e8,[('xor','ebp, ebp'),('movsxd','r13, dword ptr [rdx + 0xbc]'),
 ('mov','eax, dword ptr [r13 + 0x18]'),('dec','eax'),('cmp','ebp, eax'),('jae','0x180027842')])
pins(b,0x180024929,0x18002499a,[('mov','r9d, dword ptr [rbx + 0x40]'),('mov','r8, qword ptr [rbx + 0x38]'),
 ('mov','r10, qword ptr [rax + 0xb0]'),('mov','rbx, qword ptr [rbx + 0x88]'),
 ('mov','r9, qword ptr [rbp + 0xc0]'),('mov','r10, qword ptr [rax + 0xe8]'),('jne','0x180024a51')])
assert ink.qwords(0x1800e5ac0+0xb0,1)[0]==0x180080300
assert ink.qwords(0x1800e5ac0+0xe8,1)[0]==0x180081700
equal=pins(ink,0x180081700,0x180081746,[('cmp','r8d, dword ptr [rsp + 0x28]'),('jne','0x180081743'),
 ('cmp','qword ptr [rax + rdx - 8], rcx'),('cmp','qword ptr [rax + rdx], rcx'),('mov','al, 1'),('xor','al, al')])
assert not any(op=='call'for op,_ in equal)
regions=[(b,0x1800284f6,0x180028513),(b,0x180028719,0x1800287bf),(b,0x180169740,0x180169748),
 (b,0x180150800,0x180150bfa),(b,0x180168267,0x18016827b),(b,0x18002965b,0x1800296b3),
 (b,0x180027670,0x180027860),(b,0x1800248d0,0x180024a80),(ink,0x180081700,0x180081746)]
report=dict(regions=[dict(binary=x.name,start=hex(s),end=hex(e),sha256=hashlib.sha256(x.data[x.off(s):x.off(e)]).hexdigest())for x,s,e in regions],
 virtualBindings=6,sourceMetadata='fresh2B3.10zero ->12D0070 ->219.80 ->secondary14 ->context38.V80 ->2DA22Afalse',
 cache='secondary68/70 retained cache is not assumed empty;026090shortcircuits onactual22Afalse',
 exactEquality=True,scope='actual immutable fresh main0/no propertyevents/oneWORDperALT/exactWORDunion==TEXTkey',vendorExecution=False,documentAcceptance=False)
a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode());print('PASS9 native regions/6 virtual bindings/liveV250+context38+12D0070 metadata chain/BC0/027670earlyreturn/exactC21VE8.')
