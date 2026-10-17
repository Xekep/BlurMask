"""Read-only024F30/024D40 and275 source-prefix proof; never executes a DLL."""
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
 actual=dis(s,e)
 for pin in expected:assert pin in actual,(hex(s),pin)
 return actual

#After TEST, CF=0: JBE is ZF. Native skips nonzero kind1 runs withlow bits3.
pins(0x1800250d3,0x1800250fd,[('and','ax, 7'),('cmp','ax, 1'),('mov','edx, 0x7ffc'),
 ('test','dx, ax'),('jbe','0x180025125'),('and','al, 3'),('cmp','al, 3'),('jne','0x180025125'),('mov','byte ptr [rcx], 1')])
assert b.data[b.off(0x1802eaa84):b.off(0x1802eaa84)+4]==b'\xe2\x80\x8d\0'
pins(0x180025401,0x180025451,[('mov','r10, qword ptr [rax + 0x118]'),('jne','0x180025451'),
 ('mov','byte ptr [rax], 4'),('mov','byte ptr [rax + 8], 4')])
pins(0x180025451,0x18002548b,[('mov','dword ptr [rdi + 0x10], 1'),('cmp','byte ptr [rax + 9], 0xff'),
 ('mov','byte ptr [rax + 9], 0'),('mov','byte ptr [rax + 8], 4'),('mov','rax, qword ptr [rcx + 8]'),('mov','qword ptr [rcx], rax')])
calls=dis(0x1800255ff,0x1800256e2)
assert ('call','0x18001b4e0')in calls
assert calls.index(('call','0x1800213a0'))<calls.index(('call','0x180024d40'))
assert ('call','0x180021760')in calls
payload=pins(0x180024d9e,0x180024f0e,[('call','0x1800210c0'),('mov','dword ptr [rbx + 8], 0xffffffff'),
 ('mov','qword ptr [rbx + 0x40], 0xffffffffffffffff'),('mov','qword ptr [rbx], r15'),
 ('mov','dword ptr [rbx + 0x1c], r15d'),('mov','byte ptr [rbx + 0x64], r15b'),
 ('mov','dword ptr [rbx + 0x48], 0xffffffff'),('call','0x180021d10'),('mov','dword ptr [rbx + 0x60], 0xffffffff'),
 ('mov','ecx, dword ptr [rax + 0x10]'),('add','ecx, dword ptr [rdi + 0x68]'),('call','0x18001fae0')])
assert payload.index(('call','0x1800210c0'))<payload.index(('mov','dword ptr [rbx + 8], 0xffffffff'))<payload.index(('call','0x180021d10'))
prior=f['180021d10'];assert "*param_6 = '\\x01'"in prior and "puVar7[7] != 0xc25"in prior and "*param_6 = '\\0'"in prior
pins(0x18001fb91,0x18001fbb9,[('btr','edx, 0x17'),('and','r9d, 1'),('shl','r9d, 0x18'),
 ('and','edx, 0xcfbfffff'),('btr','edx, 0x18'),('xor','r9d, edx'),('btr','r9d, 0x19'),('mov','dword ptr [r10 + 0x18], r9d')])

#275SPANowner is64B and is separate from its120B WORD alternatives.
assert dis(0x1801412b0,0x1801412b6)==[('mov','eax, 0x40'),('ret','')]
pins(0x18013fcd0,0x18013fcf7,[('mov','dword ptr [rdx + 8], r8d'),('mov','dword ptr [rdx + 0xc], r9d'),
 ('and','word ptr [rdx + 0x10], ax'),('mov','qword ptr [rdx + 0x28], rax'),
 ('mov','qword ptr [rdx + 0x30], rax'),('mov','qword ptr [rdx + 0x38], rax')])
assert '*plVar13 = local_e8 + 0x18'in f['1801416b0']
assert b.qwords(0x1802dc740+0x68,1)[0]==0x18013e7f0
assert b.qwords(0x1802e7cb0+0x88,1)[0]==0x180219580
lazy=f['18013e7f0'];assert '(*plVar3 + 0x88)'in lazy and '*puVar1 = *puVar1 | 0x2000'in lazy
assert lazy.index('(*plVar3 + 0x88)')<lazy.index('*puVar1 = *puVar1 | 0x2000')<lazy.index('plVar7 = (longlong *)plVar7[4]')
getter=f['18000f460'];assert getter.index('param_2 + 0x60) + 0x80')<getter.index('(lVar5 + 0x68)')
prefix=f['180219580'].split('if (*(int *)(lVar3 + 0xc) <')[0]
for pin in['lVar3 + 0x28','lVar3 + 0x30','lVar3 + 0x38','CONCAT44(local_1c8._4_4_,2)',
 '*(undefined4 *)(lVar3 + 8)','*(undefined4 *)(lVar3 + 0xc)']:assert pin in prefix,pin
assert prefix.count('+ 0x188)')==2
forward=f['1801a9ae0'].split('param_6[1] = uVar17;')[0]
assert forward.count('param_5')==2 and 'if (param_5 == 2)'in forward
assert '*(uint *)(param_2 + 0x208)'in forward and '*(short *)(*(longlong *)(param_2 + 0x200) + 2) != -1'in forward

regions=[]
for s,e in[(0x180024f30,0x1800258d0),(0x180024d40,0x180024f2d),(0x180021d10,0x180022270),
 (0x18001fae0,0x18001fbb9),(0x18013fcd0,0x18013fcf7),(0x18013e7f0,0x18013e910),
 (0x18000f460,0x18000f700),(0x180219580,0x1802196e0),(0x1801a9ae0,0x1801aa470)]:
 regions.append(dict(start=hex(s),end=hex(e),sha256=hashlib.sha256(b.data[b.off(s):b.off(e)]).hexdigest()))
report=dict(regions=regions,virtualBindings=2,scope='single fresh forward WORD source prefix only; NOT219580 augmentation or fullV68/post0283B0',
 skip='kind1 AND masked run7FFC nonzero AND low2bits3',
 source='WORD clip is its275 source-owner key; document root is distinct Contains118 collection',noVendorExecution=True)
a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode());print('PASS9 static PE regions/2 virtual bindings/source-prefix lifecycle and024F30+024D40 ordering; fullV68 explicitly incomplete.')
