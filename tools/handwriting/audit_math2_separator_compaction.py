"""Static041BA0 four-byte separator removal and assignment repair."""
from pathlib import Path
import hashlib
import json
import sys
from capstone import Cs, CS_ARCH_X86, CS_MODE_64
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary('MyScriptDocument.dll');start,end=0x180041ba0,0x180041cf0
raw=b.data[b.off(start):b.off(end)]
digest=hashlib.sha256(raw).hexdigest()
assert digest=='19f6dfc200a5999cbc55def548dcc1f0d0689aca397eb22d2e089c9f6eace67c'
instructions={i.address:(i.mnemonic,i.op_str)for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,start)}
pins=[
 (0x180041bc2,'movzx','r14d, r8w'),
 (0x180041bc9,'mov','bpl, 1'),
 (0x180041be3,'lea','rcx, [rax + rbx*4]'),
 (0x180041be7,'movzx','eax, word ptr [rax + rbx*4]'),
 (0x180041beb,'cmp','r14w, ax'),
 (0x180041bf3,'test','bpl, bpl'),
 (0x180041c15,'lea','rdx, [rcx + 4]'),
 (0x180041c27,'lea','r8, [rax*4]'),
 (0x180041c2f,'call','0x1801ac820'),
 (0x180041c34,'dec','dword ptr [rdi + 8]'),
 (0x180041c48,'cmp','ecx, -1'),
 (0x180041c4d,'cmp','ecx, ebx'),
 (0x180041c51,'dec','ecx'),
 (0x180041c5b,'mov','dword ptr [rax], 0xffffffff'),
 (0x180041c70,'movzx','ebp, r8b'),
 (0x180041c74,'inc','ebx'),
 (0x180041c86,'test','bpl, bpl'),
 (0x180041c8b,'lea','edx, [rax - 1]'),
 (0x180041c92,'mov','dword ptr [rdi + 8], edx'),
 (0x180041cad,'dec','eax'),
 (0x180041cb7,'mov','dword ptr [rcx], 0xffffffff'),
]
for address,mnemonic,operands in pins:
 assert instructions[address]==(mnemonic,operands),(hex(address),instructions.get(address))
Path(sys.argv[1]).write_text(json.dumps(dict(regionSha256=digest,regions=1,instructionPins=len(pins),
 semantics='Trim leading/trailing separators and duplicate run members; preserve first interior separator and complete four-byte units',
 assignments='Removed index -> -1; higher original indices decrement after each removal',
 scope='Prepared bounded helper only; actual marker producer and EF4 transaction remain separate'),indent=2)+'\n',encoding='utf8')
print(f'PASS041BA0 whole region/{len(pins)} pins; lowU16 test/full4-byte movement/removal-index repair; no vendor execution')
