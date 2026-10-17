"""Read-only025E40/025940 ordinary assigned-line branch proof."""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
m=Cs(CS_ARCH_X86,CS_MODE_64)
def dis(s,e):return[(i.mnemonic,i.op_str)for i in m.disasm(b.data[b.off(s):b.off(e)],s)]
def pins(s,e,expected):
 rows=dis(s,e)
 for x in expected:assert x in rows,(hex(s),x)
 return rows
caller=pins(0x180029624,0x18002964c,[('mov','r9d, 2'),('call','0x1800210c0'),('call','0x180025e40')])
assert caller.index(('call','0x1800210c0'))<caller.index(('call','0x180025e40'))
pins(0x180028e4f,0x180028ecb,[('lea','rdi, [rax + 0x18]'),('je','0x180029158'),
 ('mov','qword ptr [rdi + 0x40], 0xffffffffffffffff')])
pins(0x180025e4c,0x180025f35,[('cmp','dword ptr [rdx + 0x100], 0'),('mov','r9d, 0xffffffff'),
 ('cmp','eax, dword ptr [rbp + 0xd8]'),('shl','r13, 5'),('bt','r8d, 0x15'),('jb','0x180025fbb'),
 ('mov','eax, dword ptr [rsi + 0x40]'),('jns','0x180025ef4'),('mov','eax, dword ptr [rdx + 0x10]'),
 ('shl','rdx, 4'),('mov','r15d, dword ptr [rcx + 0x60]'),('cmovl','r9d, r15d'),
 ('bt','r8d, 0xc'),('mov','edi, dword ptr [rsi + 0x58]')])
pins(0x180025f40,0x180025fa9,[('dec','edi'),('je','0x18002601b'),
 ('cmp','dword ptr [rcx + 0x60], r15d'),('je','0x180025fa5'),('call','0x180024a90')])
pins(0x18002601b,0x180026038,[('call','0x180025940'),('test','al, al'),('je','0x180025fa9')])
predicate=pins(0x1800259c9,0x180025a32,[('mov','eax, dword ptr [rdi + 0x60]'),('mov','ecx, dword ptr [rsi + 0x60]'),
 ('cmp','eax, ecx'),('je','0x180025df6'),('cmp','eax, -1'),('cmp','ecx, -1'),
 ('test','dword ptr [rdi + 0x18], 0x4000'),('test','dword ptr [rsi + 0x18], 0x4000'),
 ('call','r8'),('or','dword ptr [rax + 0x18], 0x4000'),('or','dword ptr [r14 + 8], 0x4000')])
assert predicate.index(('je','0x180025df6'))<predicate.index(('call','r8'))<predicate.index(('or','dword ptr [rax + 0x18], 0x4000'))
pins(0x180025df6,0x180025e18,[('dec','r13d'),('jne','0x180025997'),('inc','eax'),('jne','0x180025970'),('movzx','eax, bl')])
pins(0x180025fd8,0x180026015,[('mov','eax, dword ptr [rbp + 0x100]'),('lea','ecx, [r9 + 1]'),
 ('not','edi'),('add','edi, dword ptr [rbp + 0x100]'),('cmp','ecx, eax'),('jae','0x180026075'),
 ('sub','eax, ecx'),('cmp','eax, edi'),('ja','0x180026038'),('mov','dword ptr [rbp + 0x100], ecx')])
regions=[(0x180025e40,0x180026088),(0x180025940,0x180025e33),(0x180028e4f,0x180028ecb),(0x180029624,0x18002964c)]
report=dict(regions=[dict(start=hex(s),end=hex(e),sha256=hashlib.sha256(b.data[b.off(s):b.off(e)]).hexdigest())for s,e in regions],
 scope='actual configured shared WORD source/all CHAR line0/registered TEXT; immutable new56B line snapshot',
 helper='equal actual adjacent CHAR line IDs bypass every025940 write; no024A90 split',
 trim='maxSignedLine+1=1, previouscount2 ->count1; previous snapshot retained',vendorExecution=False,documentAcceptance=False)
a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode());print('PASS4 static PE regions/8 branch blocks/TEXT-before025E40/real279fallback/025940 equality bypass/56B tail truncation.')
