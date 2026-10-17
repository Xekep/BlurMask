"""Read-only pending operation4 category -> C20/C51 storage proof."""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
b=Binary('MyScriptInk.dll');assert hashlib.sha256(b.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];pins=0
def check(s,e,expected):
 global pins
 raw=b.data[b.off(s):b.off(e)];rows=[(x.mnemonic,x.op_str) for x in md.disasm(raw,s)]
 for row in expected:assert row in rows,(hex(s),row)
 pins+=len(expected);regions.append(dict(start=hex(s),end=hex(e),sha256=hashlib.sha256(raw).hexdigest()))
 return rows
assert b.qwords(0x1800e5980,1)[0]==0x18007a5b0
assert b.qwords(0x1800e5050+0xe0,1)[0]==0x18006be30
assert b.qwords(0x1800e78a0+0x38,1)[0]==0x18009ee20
check(0x18007a6f5,0x18007a703,[('mov','eax, dword ptr [rsp + 0x88]'),('mov','dword ptr [rdi + 0x40], eax')])
check(0x18007843e,0x1800784b0,[('mov','eax, dword ptr [r14 + 0x40]'),('mov','dword ptr [rsp + 0x48], eax'),
 ('mov','r10, qword ptr [rax + 0xe0]')])
check(0x18006c000,0x18006c0d0,[('mov','r15d, dword ptr [rbp + 0x88]'),('mov','dword ptr [rsp + 0x40], r15d'),
 ('mov','r10, qword ptr [rax + 0x38]')])
check(0x18009f212,0x18009f23f,[('mov','dword ptr [r14 + 0x18], edi')])
a.output.write_bytes((json.dumps(dict(peSha256=hashlib.sha256(b.data).hexdigest(),regions=regions,pins=pins,bindings=3,
 semantics='operation4 command40 -> C20.VE0 final category -> C51.Add uint category; not observer priority',vendorExecution=False),indent=2)+'\n').encode())
print(f'PASS{len(regions)} static regions/3 bindings/{pins} pins; pending selection Category, no numeric change.')
