"""Static2DA.V30/BCC-BEF literal DOM binding; no runtime document claim."""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
e=Binary('MyScriptEngine.dll');t=Binary('MyScriptText.dll');d=Binary('MyScriptDocument.dll');md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];count=0
for b,sha in [(e,'d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'),(t,'18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'),(d,'17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f')]:assert hashlib.sha256(b.data).hexdigest()==sha
assert t.qwords(0x180275550+0x18,1)==(0x1800283b0,)
assert t.qwords(0x180275550+0x30,1)==(0x180031880,)
for descriptor,metadata,vt in [(0x180227030,0x1802272d8,0x180227080),(0x180227350,0x1802275f8,0x1802273a0)]:
 assert e.qwords(descriptor,4)[3]==metadata and e.qwords(metadata,2)[1]==vt
 assert e.qwords(vt+0x68,1)==(0x1800ed3a0,) and e.qwords(vt+0x80,1)==(0x1800ed510,)
 assert e.qwords(vt+0xa8,1)==(0x1800eaaf0,)
 assert e.qwords(vt+0x110,1)==(0x1800ebaf0,)
assert e.qwords(0x180227350,2)[1]==0xbcc00000bef
assert e.qwords(0x180227010,3)==(0x1800ec250,0x1800ec2b0,0x1800ec260)
assert e.qwords(0x180227330,3)[2]==0x1800f0520
def pins(b,start,end,expected):
 global count
 raw=b.data[b.off(start):b.off(end)];rows=[(x.mnemonic,x.op_str)for x in md.disasm(raw,start)]
 for pair in expected:assert pair in rows,(hex(start),pair)
 count+=len(expected);regions.append(dict(dll=b.name,start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest()))
 return rows
pins(e,0x1800ed510,0x1800ed5a3,[('mov','r8b, 1'),('xor','r9d, r9d'),('call','0x1800ea360'),
 ('call','0x1800eb740'),('call','0x1800f44d0')])
pins(e,0x1800ed3a0,0x1800ed400,[('mov','r8b, 3'),('jmp','0x1800ea360')])
pins(e,0x1800eb740,0x1800eb9c0,[('cmp','ebx, 0xf'),('cmp','ebx, 0x1f4'),('call','0x1801d9730')])
pins(e,0x1800ec250,0x1800ec2ab,[('mov','r8b, 1'),('cmp','r9, 0x10'),('movzx','r8d, byte ptr [r8]'),('jmp','0x1800f1e40')])
pins(e,0x1800f1e40,0x1800f2190,[('mov','edx, 0xbee'),('mov','edx, 0xbec'),('mov','edx, 0xbe2')])
pins(e,0x1800f0520,0x1800f0600,[('mov','qword ptr [rbx + 0x30], 0'),('mov','qword ptr [rbx + 0x18], rax'),('mov','qword ptr [rbx + 0x10], rax')])
pins(d,0x18000d760,0x18000d8d0,[('cmp','eax, 0xbef'),('cmp','eax, 0xbcc'),('mov','edx, 0xbef')])
pins(t,0x1800658e0,0x180065d00,[('call','0x180061510'),('mov','r10, qword ptr [rax + 0x50]'),('mov','qword ptr [r14 + 0x20], rax')])
pins(t,0x180061510,0x180061690,[('mov','r8, qword ptr [r13 + 0x10]'),('mov','r9, qword ptr [rax + 0x30]'),('mov','edx, 0xbe2')])
pins(t,0x1800305d0,0x1800309c0,[('mov','r10, qword ptr [rax + 0x80]')])
pins(t,0x180030cc0,0x180030ef0,[('mov','r10, qword ptr [rax + 0x180]')])
pins(t,0x180031360,0x180031590,[('mov','r10, qword ptr [rax + 0x180]')])
pins(t,0x180031880,0x180031e10,[('call','0x1800309c0'),('call','0x180030ef0'),('call','0x180031360'),('call','0x1800317a0')])
pins(t,0x1800283b0,0x180029990,[('xor','r15d, r15d'),('mov','qword ptr [r14 + 0xb8], r15'),
 ('mov','qword ptr [rdi + 0x18], 0'),('mov','qword ptr [rdi + 0x40], 0xffffffffffffffff')])
pins(t,0x180024d40,0x180024f30,[('mov','dword ptr [rbx + 0x1c], r15d')])
literal={0x1802eab9c:b'gen',0x1802e9ddc:b'tag',0x1802eab90:b'line',0x1802ea740:b'text',0x1802eab88:b'word',0x1802eab70:b'char',0x1802e9dc0:b'ref',0x1802e9e60:b'x',0x1802e9e64:b'y',0x1802e9e68:b'a',0x1802eab98:b'h'}
for addr,value in literal.items():assert t.data[t.off(addr):].split(b'\0',1)[0]==value
a.output.write_bytes((json.dumps(dict(regions=regions,pins=count,literals=len(literal),bindings=15,vendorExecution=False,
 scope='Static literal text nodes and detached fresh library-owned parameterizedBCC plan only; no parent/BE2/C20 acceptance'),indent=2)+'\n').encode())
print(f'PASS{len(regions)} regions/15 bindings/{count} pins/{len(literal)} literal names; no DOM execution.')
