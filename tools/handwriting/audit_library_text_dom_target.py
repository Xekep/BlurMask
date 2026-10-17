"""Fresh parameterized BCC kind4 target, shared names and BE2 root insert."""
import argparse, hashlib, json, sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
e=Binary('MyScriptEngine.dll');t=Binary('MyScriptText.dll')
assert hashlib.sha256(e.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
assert hashlib.sha256(t.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];count=0;bindings=0
def pin(b,start,end,pairs=(),exact=()):
 global count
 raw=b.data[b.off(start):b.off(end)];rows=[(i.address,i.mnemonic,i.op_str)for i in md.disasm(raw,start)]
 for item in pairs:assert item in[(m,o)for _,m,o in rows],(b.name,hex(start),item)
 for item in exact:assert item in rows,(b.name,item)
 count+=len(pairs)+len(exact);regions.append(dict(dll=b.name,start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest()))
def bound(table,slot,target):
 global bindings
 assert e.qwords(table+slot,1)==(target,);bindings+=1
for slot,target in [(0x20,0x18003efb0),(0x88,0x1800ebe40),(0xa8,0x1800eaaf0),(0x218,0x1800efb10)]:bound(0x180227080,slot,target)
for slot,target in [(0x50,0x1800e35c0),(0x10,0x1800e2d70)]:bound(0x1802265e0,slot,target)

pin(e,0x1800ec260,0x1800ec2ab,[('cmp','r9, 0x10'),('movzx','r8d, byte ptr [r8]'),('jmp','0x1800f1e40')])
pin(e,0x1800f1e40,0x1800f2190,exact=[
 (0x1800f1ea6,'mov','edx, 0xbee'),(0x1800f1eeb,'mov','edx, 0x150'),
 (0x1800f1f5c,'mov','byte ptr [rax], 4'),
 (0x1800f1f84,'mov','qword ptr [rax + 8], r15'),
 (0x1800f1f9c,'mov','qword ptr [rax + 0x20], r15'),
 (0x1800f1fa4,'mov','qword ptr [rax + 0x28], r15'),
 (0x1800f2092,'mov','qword ptr [rax + 0xd0], r15'),
 (0x1800f2099,'mov','qword ptr [rax + 0xd8], r15'),
 (0x1800f20fb,'test','bpl, bpl'),(0x1800f20fe,'je','0x1800f217a')])
pin(e,0x18003efb0,0x18003efb5,[('mov','rax, qword ptr [rdx + 0x18]'),('ret','')])
#The same proven native new-writer constructor receives reader's name owner.
pin(t,0x18005f283,0x18005f2e9,exact=[
 (0x18005f283,'mov','byte ptr [rbp + 0x1f], 0'),
 (0x18005f28c,'mov','rax, qword ptr [r14 + 0x10]'),
 (0x18005f290,'mov','qword ptr [rbp + 0x27], rax'),
 (0x18005f2cc,'lea','r8, [rbp + 0x1f]'),(0x18005f2d0,'mov','r9d, 0x10')])
#No kind3-only guard: root kind4 has the same parent/first/last-child lanes.
pin(e,0x1800eaaf0,0x1800eac51,exact=[
 (0x1800eabc8,'mov','qword ptr [rbx + 8], rdi'),
 (0x1800eabd5,'mov','qword ptr [rdi + 0x20], rbx'),
 (0x1800eabf6,'mov','qword ptr [rdi + 0x28], rbx')])
#Zero path when action parent is exactly the export target; no root sentinel
#or synthetic name is inserted into the path vector.
pin(e,0x1800e35c0,0x1800e3a70,exact=[
 (0x1800e38e1,'cmp','rsi, rbx'),(0x1800e38e4,'je','0x1800e38f6'),
 (0x1800e3947,'cmp','rsi, rbx'),(0x1800e394a,'je','0x1800e3899'),
 (0x1800e3768,'mov','r10, qword ptr [rax + 0x88]'),(0x1800e376f,'call','r10')])
pin(e,0x1800e2d70,0x1800e3500,exact=[
 (0x1800e2f59,'cmp','byte ptr [rdi], 3'),(0x1800e2f5c,'jb','0x1800e2f87'),
 (0x1800e2f5e,'mov','rsi, qword ptr [rdi + 0x20]'),
 (0x1800e2fad,'mov','r9b, 1'),(0x1800e2fbe,'mov','r10, qword ptr [rax + 0x88]'),
 (0x1800e2fc5,'call','r10'),(0x1800e2fd9,'test','rsi, rsi'),
 (0x1800e2fde,'mov','rax, qword ptr [rax + 0xa8]'),(0x1800e2ffc,'call','rax')])
pin(e,0x1800ebd50,0x1800ebe40,[('mov','edx, 0xbe2')])
pin(e,0x1800efb10,0x1800efbc0,[('call','0x1800ebd50'),('mov','r10, qword ptr [rax + 0x10]')])
pin(e,0x1800ebe40,0x1800ec220,exact=[
 (0x1800ebf6f,'call','0x1800ea360'),(0x1800ebf9a,'call','0x1800eb9c0'),
 (0x1800ebfaf,'call','0x1800eb3b0'),(0x1800ebfe0,'call','0x1800ebe40'),
 (0x1800ebff6,'call','0x1800eaaf0'),(0x1800ec02d,'call','0x1800ea480')])
pin(e,0x1800eb9c0,0x1800ebb10,[('call','0x1800eb740')])
pin(t,0x180061510,0x180061680,[('mov','rbx, qword ptr [r8 + 8]'),('mov','rbx, qword ptr [rbx + 0x20]'),
 ('je','0x1800615fa'),('mov','r10, qword ptr [rax + 0x60]')])

a.output.write_bytes((json.dumps(dict(regions=regions,bindings=bindings,pins=count,vendorExecution=False,
 scope='New library-owned parameterized BCC empty kind4 target; no SDK textField parent/C20 accepted result',
 limitations=['No native incoming binding for Text05F720 proved','Native apply clone-null continues into Append(null); explicit bounded refusal needed','Numeric assigned lanes only, not native node/padding/pointer bytes']),indent=2)+'\n').encode())
print(f'PASS{len(regions)} regions/{bindings} bindings/{count} pins; fresh kind4/root-name/path/recursive-clone proof only.')
