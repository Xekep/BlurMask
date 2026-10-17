"""Read-only native original-F8 next-record addressing used by source merging."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert b.qwords(0x1802df340+0x98,1)[0]==0x180168dc0
m=Cs(CS_ARCH_X86,CS_MODE_64);a,z=0x180168dc0,0x180168e08
actual=[(i.mnemonic,i.op_str) for i in m.disasm(b.data[b.off(a):b.off(z)],a)]
expected=[('mov','rcx, qword ptr [rdx + 0x120]'),('movabs','rax, 0x2aaaaaaaaaaaaaab'),('mov','r9, qword ptr [rcx + 0xf8]'),('sub','r8, r9'),('imul','r8'),('sar','rdx, 1'),('mov','rax, rdx'),('shr','rax, 0x3f'),('add','rdx, rax'),('mov','eax, dword ptr [rcx + 0x100]'),('dec','eax'),('cmp','edx, eax'),('jne','0x180168dfa'),('xor','eax, eax'),('ret',''),('mov','eax, edx'),('inc','rax'),('lea','rax, [rax + rax*2]'),('lea','rax, [r9 + rax*4]'),('ret','')]
assert actual==expected
report=dict(exactInstructions=len(actual),binding='source218.V98=168DC0',nativeRange=[hex(a),hex(z),hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()],meaning='next record uses originalF8 pointer index/12, not current physical ordinal',scope='static plus separate genuine model-reordered source regression; no vendor execution')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes((json.dumps(report,indent=2)+'\n').encode());print('PASS20 exact instructions/source218.V98 binding; originalF8 next-record addressing.')
