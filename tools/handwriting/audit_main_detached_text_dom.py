"""Native literal DOM values, metadata lanes and helper failure ownership."""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
e=Binary('MyScriptEngine.dll');t=Binary('MyScriptText.dll');md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];count=0
assert hashlib.sha256(e.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
assert hashlib.sha256(t.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
def pin(b,start,end,expected):
 global count
 raw=b.data[b.off(start):b.off(end)];rows=[(x.mnemonic,x.op_str)for x in md.disasm(raw,start)]
 for pair in expected:assert pair in rows,(hex(start),pair)
 count+=len(expected);regions.append(dict(dll=b.name,start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest()));return rows
for table in [0x180227080,0x1802273a0]:
 for offset,target in [(0x68,0x1800ed3a0),(0x80,0x1800ed510),(0x90,0x1800ea480),(0xa8,0x1800eaaf0),(0xd0,0x1800ed5b0),(0x110,0x1800ebaf0),(0x118,0x1800edb00),(0x120,0x1800edb60),(0x128,0x1800eda40),(0x140,0x1800edc30),(0x148,0x1800edca0)]:assert e.qwords(table+offset,1)==(target,)
#V68 resolves a retained name FIRST; only the subsequent EA360(kind3)
#tail-call enters the fallible F3530 node allocator. A failed node does
#not undo name interning. Pin instruction addresses/order, not only membership.
ordered=[(x.address,x.mnemonic,x.op_str)for x in md.disasm(e.data[e.off(0x1800ed3a0):e.off(0x1800ed3f2)],0x1800ed3a0)]
assert [(a,m,o)for a,m,o in ordered if a in(0x1800ed3c4,0x1800ed3c7,0x1800ed3d7,0x1800ed3da,0x1800ed3ed)]==[
 (0x1800ed3c4,'call','r10'),(0x1800ed3c7,'test','rax, rax'),(0x1800ed3d7,'mov','r9, rax'),
 (0x1800ed3da,'mov','r8b, 3'),(0x1800ed3ed,'jmp','0x1800ea360')]
pin(e,0x1800ed3a0,0x1800ed3f2,[('mov','rdx, qword ptr [rdx + 0x10]'),('mov','r10, qword ptr [rax + 0x10]')])
pin(e,0x1800ea360,0x1800ea472,[('call','0x1800f3530'),('mov','qword ptr [rax + 0x30], rbx')])
count+=5
pin(e,0x1800eb740,0x1800eb9c0,[('test','r15, r15'),('cmp','ebx, 0xf'),('ja','0x1800eb89f'),
 ('mov','r15b, 2'),('cmp','ebx, 0x1f4'),('jbe','0x1800eb8c8'),('mov','r15b, 1'),('mov','r15b, 3'),
 ('call','0x1801d9730'),('mov','byte ptr [rsi + 1], r15b'),('movups','xmmword ptr [rsi + 0x20], xmm0')])
pin(e,0x1800ed510,0x1800ed5a3,[('call','0x1800ea360'),('call','0x1800eb740'),('call','0x1800f44d0')])
#No-history helper cleanup: an element's attribute chain is freed before
#ED5B0 clears children. Preserve the native helper-specific orphan rules.
pin(e,0x1800ea5a6,0x1800ea61d,[('mov','rbx, qword ptr [rsi + 0x38]'),
 ('call','0x1800ea480'),('call','0x1800ed5b0')])
pin(e,0x1800ed6e0,0x1800ed71c,[('call','0x1800eb090'),('call','0x1800ea480')])
for start,end,tag in [(0x1800edb00,0x1800edb60,7),(0x1800edb60,0x1800edbc0,8),(0x1800eda40,0x1800edaa0,4),(0x1800edc30,0x1800edca0,5),(0x1800edca0,0x1800edd00,6)]:
 pin(e,start,end,[('mov',f'byte ptr [rsp + 0x38], {tag}'),('call','0x1800ed830')])
pin(t,0x180031880,0x180031e10,[('mov','eax, dword ptr [r13 + 0xbc]'),('mov','ecx, dword ptr [r13 + 0xb8]'),
 ('mov','r9, qword ptr [rax + 0xd0]'),('mov','r9, qword ptr [rax + 0x90]')])
pin(t,0x1800283b0,0x180029990,[('xor','r15d, r15d'),('mov','qword ptr [r14 + 0xb8], r15'),
 ('mov','qword ptr [rdi + 0x18], 0'),('mov','qword ptr [rdi + 0x40], 0xffffffffffffffff')])
pin(t,0x180031360,0x180031590,[('je','0x180031556'),('mov','r9, qword ptr [rax + 0x90]'),('call','r9')])
pin(t,0x180030cc0,0x180030ef0,[('je','0x180030e3a'),('mov','r9, qword ptr [rax + 0x90]')])
pin(t,0x180031590,0x1800317a0,[('movsd','xmm3, qword ptr [r14 + 8]'),('movsd','xmm0, qword ptr [r14 + 0x18]')])
#Fresh seed does not import existing DOM text. These are actual distinct
#writers; ordinary0283B0 is not the caller-UTF8 or existing-text loader path.
f=functions();assert '+ 0xe8)'not in f['1800283b0']and '+ 0xf0)'not in f['1800283b0']
for name,start,end in [('fresh2DA',0x18001c010,0x18001c2c0),('callerUTF8writer',0x18002a3d0,0x18002b720),('existingDOMloader',0x18002d8c0,0x1800305d0)]:
 raw=t.data[t.off(start):t.off(end)];regions.append(dict(dll=t.name,start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest(),scope=name))
a.output.write_bytes((json.dumps(dict(regions=regions,pins=count,bindings=22,vendorExecution=False,scope='Detached library-owned parameterized BCC arena; no SDKparent/BE2/C20 application',unsupported=['031360 null textCandidate V90-null boundary','external400/math800/prior/transforms']),indent=2)+'\n').encode())
print(f'PASS{len(regions)} regions/22 bindings/{count} pins; ordered name-before-node, native15/16/501/value tags andBC vscoverage distinction.')
