"""Static fresh-source rotation gate and021AB0 allocation/write proof. No vendor execution."""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
b=Binary();f=functions();m=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
def dis(s,e):return[(i.mnemonic,i.op_str)for i in m.disasm(b.data[b.off(s):b.off(e)],s)]
def pins(s,e,expected):
 rows=dis(s,e)
 for x in expected:assert x in rows,(hex(s),x)
 return rows
assert b.qwords(0x180274e80,3)==(0x1012d000c,0x18000e020,0x18000e030)
assert b.qwords(0x1802df280,3)==(0x1012d000c,0x180169070,0x180169090)
zero=pins(0x18000c860,0x18000ca20,[('xor','eax, eax'),('mov','qword ptr [rdi + 0xa4], rax')])
assert zero.index(('xor','eax, eax'))<zero.index(('mov','qword ptr [rdi + 0xa4], rax'))
assert dis(0x18000e020,0x18000e02d)==[('movzx','eax, byte ptr [rdx + 0xa9]'),('mov','byte ptr [r8], al'),('mov','al, 1'),('ret','')]
assert dis(0x180169090,0x18016909d)==[('movzx','eax, byte ptr [r8]'),('mov','byte ptr [rdx + 0xb4], al'),('mov','al, 1'),('ret','')]
pins(0x180151a8a,0x180151a9b,[('mov','r8d, 0x12d000c'),('call','0x180150800')])
pins(0x1801682d8,0x18016830f,[('cmp','byte ptr [rdi + 0xb4], 0'),('je','0x18016830f'),
 ('mov','rdx, qword ptr [rdi + 0x40]'),('mov','r8, qword ptr [rdi + 0x120]'),('mov','r9, qword ptr [rax + 0x10]'),('call','r9')])
assert b.qwords(0x1802b39c0+0x10,1)[0]==0x180081e90
assert '*(float *)(param_3 + 0xa8) = (float)dVar3'in f['180081e90']
zero=pins(0x18016a130,0x18016a260,[('xor','eax, eax'),('mov','dword ptr [rdx + 0xa8], eax')])
assert zero.index(('xor','eax, eax'))<zero.index(('mov','dword ptr [rdx + 0xa8], eax'))
assert b.qwords(0x1802754b0,1)[0]==0x18001c010
assert b.qwords(0x180275550+0x18,1)[0]==0x1800283b0
zero=pins(0x18001c010,0x18001c2c0,[('xor','ebp, ebp'),('mov','qword ptr [rdi + 0xf8], rbp'),('mov','qword ptr [rdi + 0x100], rbp')])
assert zero.index(('xor','ebp, ebp'))<zero.index(('mov','qword ptr [rdi + 0xf8], rbp'))<zero.index(('mov','qword ptr [rdi + 0x100], rbp'))
assert b.qwords(0x1802de610+0x250,1)[0]==0x1801530f0
assert dis(0x1801530f0,0x1801530f5)==[('mov','rax, qword ptr [rdx + 0x60]'),('ret','')]
assert b.qwords(0x1802df340+0x08,1)[0]==0x180168ac0
assert b.qwords(0x1802df340+0x10,1)[0]==0x180168ad0
assert b.qwords(0x1802df340+0x40,1)[0]==0x180168b90
assert b.qwords(0x1802df340+0x100,1)[0]==0x1801692b0
pins(0x1800288d9,0x18002890f,[('xor','r11d, r11d'),('mov','dword ptr [rbp + 0x78], r11d')])
pins(0x180029407,0x18002941b,[('lea','r8, [rbp + 0x10]'),('call','0x180021ab0')])
native=f['180021ab0'];assert '*(uint *)(param_2 + 0x100)'in native and '(int)plVar1[0xd] + iVar12'in native
assert '*(double *)(lVar9 + 0x30 + lVar2) = (double)*(float *)(lVar8 + 0xc)'in native
pins(0x180021c42,0x180021ca8,[('mov','eax, dword ptr [r12 + 0x68]'),('imul','rcx, rax, 0x38'),
 ('mov','qword ptr [rcx + rax], 0'),('mov','qword ptr [rcx + rax + 0x28], 0xffffffffffffffff'),
 ('movss','xmm0, dword ptr [rbp + 0xc]'),('movsd','qword ptr [rcx + rax + 0x30], xmm0')])
guide=f['1801692b0']
for x in['*(float *)(lVar5 + 0xa8)','*(float *)(lVar5 + 0x18)','*(float *)(lVar5 + 8)','lVar4 = 0xc','lVar4 = 0x14','*(float *)(lVar5 + 0x10)']:assert x in guide,x
regions=[(0x18000c860,0x18000ca20),(0x18000e020,0x18000e02d),(0x180169090,0x18016909d),
 (0x180150d00,0x180151b00),(0x1801682d8,0x18016830f),(0x18016a130,0x18016a260),
 (0x18001c010,0x18001c2c0),(0x1800283b0,0x180029990),(0x180021ab0,0x180021cd0),(0x1801692b0,0x1801694c0)]
report=dict(regions=[dict(start=hex(s),end=hex(e),sha256=hashlib.sha256(b.data[b.off(s):b.off(e)]).hexdigest())for s,e in regions],
 bindings=10,rotationProof='NEW2B3 A4qwordzero coversA9;12D000C->218.B4false skips2D0.V10/081E90;secondary16A130.A8zero',
 counters='one source/configured2: append N twice, reset wrapper68=0 each alternative; second allocated tail remains zero',
 guide='V100 uses guide8/C/10 or corrected14/18, not current0/4',scope='fresh no-property/no-prior owning source only',vendorExecution=False,acceptedDocument=False)
a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode());print('PASS10 static PE regions/10 constructor/property/virtual bindings: actual fresh rotation gate, per-alt write offset vs allocation count, guide pre-deskew metrics.')
