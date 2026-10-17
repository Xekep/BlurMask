"""Read-only C21 source range formatter/canonicalization/caller proof."""
import argparse,hashlib,json,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
i=Binary('MyScriptInk.dll');t=Binary('MyScriptText.dll');md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];total=0
assert hashlib.sha256(i.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
assert hashlib.sha256(t.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert i.qwords(0x1800e5c90,5)[4]==0x1800e5ac0
assert i.qwords(0x1800e5ac0+0x180,1)==(0x180083df0,)
assert i.qwords(0x1800e5ac0+0xb0,1)==(0x180080300,)
def pins(b,start,end,expected):
 global total
 raw=b.data[b.off(start):b.off(end)];rows=[(x.mnemonic,x.op_str)for x in md.disasm(raw,start)]
 for pair in expected:assert pair in rows,(hex(start),pair)
 total+=len(expected);regions.append(dict(dll=b.name,start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest()))
 return rows
pins(i,0x180083df0,0x1800840d3,[('call','0x180080430'),('call','0x18007f830'),('mov','r14d, 1'),
 ('cmp','r8b, 4'),('cmp','byte ptr [rsi], 4'),('call','0x18007faf0'),('mov','dword ptr [rcx], eax'),
 ('lea','ecx, [rax*2 + 1]'),('mov','byte ptr [rdx], 0x55'),('call','0x18007f690'),
 ('mov','byte ptr [rax - 1], 0x2c'),('cmp','byte ptr [r15 + 8], 5'),('cmp','byte ptr [r15], 3'),
 ('call','0x180036b70'),('add','dword ptr [rdi], r15d'),('mov','r8, qword ptr [rax + 0x40]')])
pins(i,0x180080430,0x1800804d4,[('cmp','r8b, 4'),('je','0x1800804b0'),('cmp','rax, r8'),
 ('jbe','0x1800804c2'),('inc','edx'),('test','si, r8w'),('cmp','eax, 3'),('cmp','eax, 0xff03')])
pins(i,0x18007f690,0x18007f7e8,[('movzx','eax, byte ptr [rcx + 1]'),('cmp','al, 0xff'),
 ('mov','byte ptr [rdx - 1], 0x24'),('lea','ecx, [rax + rax*4]'),('mov','byte ptr [r8], 0x2e'),
 ('movzx','ecx, word ptr [r10 + 2]'),('mov','byte ptr [r8], 0x3a'),('mov','ecx, dword ptr [r10 + 4]')])
assert i.data[i.off(0x1800e5cd8):i.off(0x1800e5cd8)+10]==b'0123456789'
pins(i,0x180036b70,0x180036be6,[('inc','r9d'),('mov','dword ptr [rbx + 0xc], edi')])
pins(i,0x18000db00,0x18000dbc1,[('cmp','al, 0xc8'),('inc','cx'),('mov','byte ptr [rsp + 0x41], 0xff'),
 ('mov','byte ptr [rsp + 0x41], 0'),('mov','word ptr [rsp + 0x42], cx')])
#Raw F32 constants underpin the supported fraction domain; no ad-hoc clamp.
assert struct.unpack_from('<f',i.data,i.off(0x1800fa364))[0]==200
assert struct.unpack_from('<f',i.data,i.off(0x1800fa1b0))[0]==.5
pins(t,0x1800309c0,0x180030cc0,[('mov','r10, qword ptr [rax + 0x180]'),
 ('test','rax, rax'),('je','0x180030a9b'),('test','dword ptr [r15 + 0x18], 0x400000'),
 ('mov','ecx, 0x7d'),('mov','eax, 0x7b'),('cmp','byte ptr [rdx], 0x5b'),('mov','byte ptr [rdx], cl')])
pins(t,0x180030ef0,0x180031360,[('mov','r10, qword ptr [rax + 0x180]'),
 ('test','dword ptr [r15 + 0x18], 0x400000'),('mov','ecx, 0x7d'),('mov','eax, 0x7b'),('cmp','byte ptr [rdx], 0x5b')])
pins(t,0x180031880,0x180031e10,[('mov','qword ptr [rsp + 0x40], rsi'),('mov','qword ptr [rsp + 0x48], rsi'),
 ('mov','rbp, qword ptr [r13 + 0x50]'),('call','0x1800309c0'),('call','0x180030ef0')])
a.output.write_bytes((json.dumps(dict(regions=regions,pins=total,bindings=3,vendorExecution=False,
 scope='C21.V180 raw source IDs; caller brace transformation is separate; no DOM acceptance'),indent=2)+'\n').encode())
print(f'PASS{len(regions)} static regions/3 bindings/{total} pins; raw ranges, decimal domain and separate caller rewrite.')
