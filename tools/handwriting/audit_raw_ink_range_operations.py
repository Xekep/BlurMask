"""Static-only C21 operators, original-document point counts and raw FF branches."""
import sys,hashlib,json,argparse
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args();b=Binary('MyScriptInk.dll')
assert hashlib.sha256(b.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
md=Cs(CS_ARCH_X86,CS_MODE_64)
def dis(s,e):return[(i.mnemonic,i.op_str)for i in md.disasm(b.data[b.off(s):b.off(e)],s)]
for off,target in [(0xb0,0x180080300),(0xc0,0x180080b40),(0xd0,0x1800812d0),(0xe8,0x180081700),(0x118,0x180081cc0)]:assert b.qwords(0x1800e5ac0+off,1)[0]==target
assert b.qwords(0x1800e4ff0+0x18,1)[0]==0x1800e4ef0
assert b.qwords(0x1800e4ef0+8,1)[0]==0x1800e5050
assert b.qwords(0x1800e5050+0x140,1)[0]==0x18006c9a0
point=dis(0x18006c9a0,0x18006c9ca)
for pin in [('test','rdx, rdx'),('cmp','r8d, dword ptr [rdx + 0x18]'),('jae','0x18006c9c4'),('mov','edx, dword ptr [rax + rcx*8 + 0xc]'),('mov','eax, 0x8000'),('cmovne','eax, edx')]:assert pin in point,pin
neighbor=dis(0x180080cf2,0x180080e8e)
for pin in [('dec','r8d'),('mov','word ptr [rsp + 0x88], 0xff05'),('inc','r8d'),('cmp','eax, 1'),('cmp','eax, 0x8000'),('mov','r9, qword ptr [rax + 0x140]')]:assert pin in neighbor,pin
contains=dis(0x180081cc0,0x180081d84)
for pin in [('cmp','rcx, r11'),('setbe','al')]:assert pin in contains,pin
#The append kernel's subtraction is unsigned. A first endpoint1 is outside
#canonical fresh source geometry but genuinely wraps at first-minus2; a
#portable implementation must not silently replace it with geometric union.
append=dis(0x18007faf0,0x1800802c4)
assert ('add','rcx, -2')in append and ('jae','0x18007fca3')in append
assert ('jbe','0x18007fd1a')in append
regions=[]
for s,e in [(0x18007faf0,0x1800802c4),(0x180080300,0x180080390),(0x1800804e0,0x180080820),(0x180080830,0x180080b30),(0x180080b40,0x180080c50),(0x180080c60,0x1800812c0),(0x1800812d0,0x180081380),(0x180081700,0x18008175a),(0x180081cc0,0x180081d84),(0x18006c9a0,0x18006c9ca)]:
 regions.append(dict(start=hex(s),end=hex(e),sha256=hashlib.sha256(b.data[b.off(s):b.off(e)]).hexdigest()))
a.output.write_bytes((json.dumps(dict(binary=b.name,regions=regions,virtualBindings=8,scope='C21 active ordered ranges, original RawDocument pointcounts,presence; allocationfailure/stalecapacitynotmodeled',noVendorExecution=True),indent=2)+'\n').encode());print('PASS10 native C21/C20 PE regions/8virtual bindings/neighbor-count and raw containment pins; no vendor execution.')
