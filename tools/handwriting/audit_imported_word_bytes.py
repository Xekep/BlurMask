"""Pinned post-import primary-alphabet selection and detach order; PE reads only."""
from pathlib import Path
import hashlib,json,sys
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pins={0x18005bad0:('cmp','byte ptr [r8 + 0x20], 0'),0x18005bae0:('mov','rax, qword ptr [r8 + 0x10]'),0x18005bae4:('mov','rax, qword ptr [rax + 0x18]'),0x18005baea:('mov','rax, qword ptr [r8 + 0x18]'),0x18005baee:('mov','rax, qword ptr [rax + 0x60]'),0x18005bbae:('mov','r8, qword ptr [rsp + 0x80]'),0x18005bbc0:('mov','r10, qword ptr [rax + 0x50]'),0x18005bbce:('call','r10'),0x180136b86:('mov','byte ptr [rbx + 0x20], 1'),0x180136ba8:('mov','r8, qword ptr [rbx + 0x18]'),0x180136bb7:('mov','rax, qword ptr [rax + 0x30]'),0x180136bbb:('call','rax'),0x180136be2:('mov','rax, qword ptr [rax + 0x38]'),0x180136be6:('call','rax'),0x180136c07:('mov','qword ptr [rax + 8], 0'),0x180136c13:('mov','qword ptr [rax + 0x10], 0'),0x180136c2d:('call','r8'),0x180136c3e:('mov','qword ptr [rdi], 0')}
regions=[]
for lo,hi in [(0x18005bac0,0x18005bce0),(0x180136b40,0x180136c60)]:
 data=b.data[b.off(lo):b.off(hi)];ins={i.address:(i.mnemonic,i.op_str) for i in md.disasm(data,lo)}
 for a,v in pins.items():
  if lo<=a<hi:assert ins[a]==v,(hex(a),ins.get(a))
 regions.append(dict(start=hex(lo),end=hex(hi),sha256=hashlib.sha256(data).hexdigest()))
for off,target in [(0x30,0x18013be80),(0x38,0x18013c1d0),(0x50,0x18013c920)]:assert b.qwords(0x1802dc740+off,1)[0]==target
#13A0B0 raw interval precedes its service-E0 call and terminating storage NUL.
#The runtime intentionally stops before that service/canonical boundary.
raw=b.data[b.off(0x18013a0b0):b.off(0x18013a620)]
report=dict(noVendorExecution=True,tableBindings=3,pins=[dict(address=hex(a),mnemonic=v[0],operands=v[1]) for a,v in pins.items()],regions=regions,rawEmitterRegion=dict(start='18013A0B0',end='18013A620',sha256=hashlib.sha256(raw).hexdigest()),scope='Raw byte concatenation only;0491A0/canonical/circled/codec completion not implemented here')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(report,indent=2)+'\n');print('PASS18postimport/detachpins/3tablebindings; static-only raw-byte boundary')
