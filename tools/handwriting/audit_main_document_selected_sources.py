"""Read-only bounded01FD40 and selected020000 flags/index44 proof."""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
b=Binary();m=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
def dis(s,e):return[(i.mnemonic,i.op_str)for i in m.disasm(b.data[b.off(s):b.off(e)],s)]
def pins(s,e,expected):
 rows=dis(s,e)
 for x in expected:assert x in rows,(hex(s),x)
 return rows
pins(0x1800297cd,0x1800297ef,[('call','0x18001fd40'),('call','0x180020000'),('call','0x180020920')])
pins(0x18001fd4c,0x18001fdb1,[('mov','r9, qword ptr [rdx + 0x48]'),('mov','eax, 0xffffffff'),
 ('inc','eax'),('mov','qword ptr [rsp + rax*8 + 0x20], r9'),('mov','r9, qword ptr [r9]')])
pins(0x18001fde0,0x18001fe89,[('test','eax, 0x10600000'),('jne','0x18001fe89'),('bt','eax, 0x1d'),
 ('mov','r8, qword ptr [rdi + 0x28]'),('cmp','r8, -1'),('je','0x18001fe89'),
 ('mov','r9, qword ptr [rax + 0x128]'),('call','r9'),('cmp','dword ptr [rdi + 0x40], 1'),
 ('mov','rax, qword ptr [rdi + 0x38]'),('mov','ecx, dword ptr [rax + 4]'),('cmp','ecx, dword ptr [rax + 0xc]'),
 ('cmp','word ptr [rax + 2], 0'),('cmp','byte ptr [rax + 1], 0'),('cmp','word ptr [rax + 0xa], 3'),
 ('cmp','byte ptr [rax + 9], 0xff'),('cmp','eax, 0xc25'),('btr','eax, 0x18')])
pins(0x18001ff0f,0x18001fffe,[('movsxd','rcx, dword ptr [r14 + 0xbc]'),
 ('mov','eax, dword ptr [r10 + 0x40]'),('jns','0x18001ff74'),('mov','eax, dword ptr [rdx + 0x10]'),
 ('test','dword ptr [rax + 0x18], 0x11004000'),('and','dword ptr [r10 + 0x18], 0xfeffffff'),
 ('shl','eax, 0x18'),('or','dword ptr [r10 + 0x18], eax')])
pins(0x18002001d,0x1800200ed,[('movsxd','rdi, dword ptr [rdx + 0xbc]'),
 ('mov','eax, dword ptr [rdx + 0x40]'),('jns','0x180020068'),('mov','eax, dword ptr [r9 + 0x10]'),
 ('inc','eax'),('or','dword ptr [rcx + 0x18], 0x8000000'),('mov','ecx, dword ptr [r8 + 0x10]'),
 ('mov','dword ptr [rdx + 0x44], ecx'),('jb','0x180020090'),
 ('or','dword ptr [rax + 0x18], 0x8000000'),('mov','eax, dword ptr [r9 + 0x10]'),('mov','dword ptr [rcx + 0x44], eax')])
writer=dis(0x180020000,0x1800200ed)
assert not any(op=='call'for op,_ in writer)
assert writer.index(('mov','dword ptr [rdx + 0x44], ecx'))<writer.index(('or','dword ptr [rax + 0x18], 0x8000000'))
regions=[(0x18001fd40,0x18001fffe),(0x180020000,0x1800200ed),(0x1800297cd,0x1800297ef)]
report=dict(regions=[dict(start=hex(s),end=hex(e),sha256=hashlib.sha256(b.data[b.off(s):b.off(e)]).hexdigest())for s,e in regions],
 traversal='complete actualCHARAVL in-order, native64-entry stack; selectedALTBC WORD/CHAR links',
 guards='10600000mask skips;bit29 actuallyusedV128 refuses; literalrangeC25lookup actuallyused refuses',
 writes='WORDbit24 fromallCHAR11004000; selectedCHARthenWORD08000000 OR andsignedrawindex44; lastlinkwins',
 scope='newimmutableoverlay overactualfreshcompleteWORD/CHAR source owners;020920notperformed',vendorExecution=False,documentAcceptance=False)
a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode());print('PASS3 native PE regions/5 exact blocks/lazyprior+C25 guards/WORDbit24/CHARthenWORDflags18+index44/no020000callbacks.')
