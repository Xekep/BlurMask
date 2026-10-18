"""Static-only original raw C20 domain -> ordinary C4D preflight outputs."""
import argparse,hashlib,json,struct,subprocess,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
dependencies={}
for name in ('audit_registered_text_patch_history.py','audit_raw_ink_current_view.py','audit_raw_ink_range_operations.py'):
    output=Path(str(a.output)+'.'+name+'.json')
    subprocess.run([sys.executable,str(Path(__file__).with_name(name)),str(output)],cwd=root,check=True)
    dependencies[name]=json.loads(output.read_bytes())
i=Binary('MyScriptInk.dll');assert hashlib.sha256(i.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];pins=0
def pin(start,end,expected):
    global pins
    raw=i.data[i.off(start):i.off(end)];xs=[(x.address,x.mnemonic,x.op_str)for x in md.disasm(raw,start)]
    for v in expected:assert v in xs,(hex(start),v)
    pins+=len(expected);regions.append(dict(start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest()))
assert i.qwords(0x1800e5ac0+0xa8,1)==(0x1800802e0,)
assert i.qwords(0x1800e5ac0+0x120,1)==(0x180081d90,)
pin(0x1800802e0,0x1800802fc,[
 (0x1800802e0,'cmp','r8b, 4'),(0x1800802e4,'je','0x1800802f9'),
 (0x1800802ef,'cmp','byte ptr [rax], 4'),(0x1800802f4,'jmp','0x18007faf0')])
pin(0x180065b77,0x180065c09,[
 (0x180065b77,'mov','ebx, dword ptr [r15 + 0x18]'),(0x180065ba0,'cmp','eax, 1'),
 (0x180065ba3,'lea','rdx, [r15 + 0x20]'),(0x180065bac,'mov','ecx, 0xff03'),
 (0x180065bb1,'mov','ebx, 3'),(0x180065bb6,'cmove','ebx, ecx'),(0x180065bbf,'or','rbx, rcx'),
 (0x180065bf1,'or','rdi, 0xff05'),(0x180065bff,'mov','r10, qword ptr [rax + 0xa8]'),(0x180065c06,'call','r10')])
pin(0x180077850,0x1800778d0,[
 (0x180077881,'mov','bl, 1'),(0x180077889,'movzx','r15d, bl'),(0x180077895,'test','r14, r14'),
 (0x18007789a,'mov','byte ptr [r9], dil'),(0x18007789d,'movzx','eax, bl'),
 (0x1800778c7,'test','r13, r13'),(0x1800778ca,'je','0x180077b48')])
pin(0x180077b48,0x180077c0d,[
 (0x180077b51,'mov','r12d, dword ptr [r14 + 0x18]'),(0x180077b78,'mov','r9d, dword ptr [r14 + 0x28]'),
 (0x180077b80,'mov','r8, qword ptr [r14 + 0x20]'),(0x180077b8b,'mov','r10, qword ptr [rax + 0x88]'),
 (0x180077b92,'call','r10'),(0x180077b97,'je','0x180077ffc'),(0x180077ba1,'mov','r8d, r12d'),
 (0x180077ba8,'mov','edx, 1'),(0x180077bb6,'call','r9'),(0x180077bc6,'je','0x180077ffc')])
pin(0x180077c0d,0x180077c51,[
 (0x180077c23,'mov','rbx, qword ptr [rbx + 0x10]'),(0x180077c27,'test','r15b, r15b'),
 (0x180077c2a,'je','0x180077fff'),(0x180077c33,'je','0x180077fff'),
 (0x180077c39,'movzx','eax, byte ptr [rbx]'),(0x180077c4f,'jmp','rcx')])
pin(0x180077d8b,0x180077dda,[
 (0x180077d94,'mov','r9, qword ptr [rbx + 0x30]'),(0x180077d98,'mov','ecx, dword ptr [r9 + 0x20]'),
 (0x180077d9e,'je','0x180077dda'),(0x180077da4,'mov','r9, qword ptr [r9 + 0x18]'),
 (0x180077dc1,'mov','r10, qword ptr [rax + 0x120]'),(0x180077dc8,'call','r10'),
 (0x180077dd6,'cmove','r15d, eax')])
pin(0x180077fd9,0x180078064,[
 (0x180077fd9,'mov','rbx, qword ptr [rbx + 8]'),(0x180077ffc,'xor','dil, dil'),
 (0x18007800b,'mov','byte ptr [rax], r15b'),(0x180078048,'movzx','eax, dil')])
pin(0x180081d90,0x180081ea0,[
 (0x180081dab,'test','r15d, r15d'),(0x180081db4,'setne','al'),(0x180081dbc,'je','0x180081e76'),
 (0x180081dd6,'cmp','rax, rsi'),(0x180081dd9,'ja','0x180081e60'),
 (0x180081ded,'cmp','al, 0xff'),(0x180081e8a,'cmp','rsi, r9'),(0x180081e8d,'ja','0x180081e60')])
#Jump table entries are RVAs relative to the image base: operation7 is a
#literal no-op, operation4 enters the checked C22 range branch.
assert struct.unpack_from('<I',i.data,i.off(0x180078064)+4*4)[0]==0x77d8b
assert struct.unpack_from('<I',i.data,i.off(0x180078064)+7*4)[0]==0x77fd9
a.output.parent.mkdir(parents=True,exist_ok=True)
a.output.write_bytes((json.dumps(dict(regions=regions,pins=pins,bindings=2,jumpTableEntries=2,dependencies=dependencies,
 scope='original C20 raw-domain construction and prepared ordinary4/7 compatibility; no CBB/page acceptance',vendorExecution=False),indent=2)+'\n').encode())
print(f'PASS{len(regions)} regions/2 bindings/2 literal dispatch entries/{pins} pins +original/raw/range dependencies; no vendor execution.')
