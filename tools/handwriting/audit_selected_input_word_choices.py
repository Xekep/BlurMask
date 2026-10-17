"""Static Text05F050 -> pending C4D/current C20/C51.Next ownership proof."""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
t=Binary('MyScriptText.dll');i=Binary('MyScriptInk.dll')
assert hashlib.sha256(t.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(i.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];count=0
def pins(b,s,e,expected):
 global count
 raw=b.data[b.off(s):b.off(e)];rows=[(x.mnemonic,x.op_str) for x in md.disasm(raw,s)]
 for row in expected:assert row in rows,(hex(s),row)
 count+=len(expected);regions.append(dict(dll=b.name,start=hex(s),end=hex(e),sha256=hashlib.sha256(raw).hexdigest()))
 return rows
# Actual current registry getter, not ObjectCount or postpublication reset.
assert i.qwords(0x1800e78a0+0x20,1)[0]==0x18008e870
assert i.data[i.off(0x18008e870):i.off(0x18008e870)+5]==bytes.fromhex('488b4218c3')
r=pins(t,0x18005f050,0x18005f124,[('mov','qword ptr [rdx], rsi'),
 ('mov','rdx, qword ptr [rsi + 0x40]'),('mov','r8, qword ptr [rax + 0x20]'),
 ('mov','qword ptr [rbx + 0x10], rax'),('mov','edx, 0x20'),('mov','qword ptr [rax + 8], rsi'),
 ('mov','edx, 0xc4d'),('xor','edx, edx'),('mov','r8, qword ptr [rax + 0x48]')])
assert r.index(('mov','qword ptr [rbx + 0x10], rax'))<r.index(('mov','edx, 0xc4d'))
# New command owner7 has empty pending command links, regardless of existing
# C20 history. It does not borrow or clear the C20 C4D history queue.
assert i.qwords(0x1800e5980+0x48,1)[0]==0x18007d9b0
pins(i,0x18007d9b0,0x18007daab,[('mov','edx, 0x28'),('mov','edx, 0xbc5'),
 ('xor','eax, eax'),('mov','dword ptr [rsi + 8], 7'),('mov','qword ptr [rsi + 0x10], rax'),
 ('mov','qword ptr [rsi + 0x18], rax'),('mov','qword ptr [rsi + 0x20], rax')])
# Actual05F200 creates the tuple, gets real DAF32 input rows, then dispatches
# new2DA.V18. No assertion that this batch already models that whole driver.
assert t.qwords(0x180275550+0x18,1)[0]==0x1800283b0
pins(t,0x18005f200,0x18005f610,[('call','0x18005f050'),('mov','edx, 0xdaf'),
 ('mov','r10, qword ptr [rax + 0x38]'),('mov','edx, 0x2da'),
 ('mov','r10, qword ptr [rcx + 0x18]'),('lea','rax, [rbp - 0x61]')])
#0210C0 increments the tuple scratch counter, not live registry Next. The
# actual pending command has kind-selected name CHAR/WORD/TEXT and category100.
pins(t,0x18002127a,0x18002133e,[('mov','edx, 0xc22'),('mov','edx, 0xc4d'),
 ('lea','rax, [rbx + 1]'),('mov','qword ptr [r12 + 0x10], rax'),('mov','dword ptr [rsp + 0x38], 0x64')])
report=dict(regions=regions,pins=count,bindings=3,scope='same selected C20/C51.Next -> separate pending C4D -> actual CHAR/WORD/TEXT publication',
 pending=['actual2DA transform-vector/reset binding','037E80 result writes','full0283B0 acceptance'],vendorExecution=False)
a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode())
print(f'PASS{len(regions)} regions/3 bindings/{count} pins; selected C51.Next and separate pending C4D, not raw ordinal or document acceptance.')
