"""Static275.V08/140B90 index ordering and two score channels; no SDK run."""
import hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a,z=0x180140b90,0x180141130;raw=b.data[b.off(a):b.off(z)]
assert hashlib.sha256(raw).hexdigest()=='5752ec9b8e3c9445e16ae4c56ff4d49d39b28efd5adf06de84487d4a969a5fc4'
instructions={i.address:(i.mnemonic,i.op_str)for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,a)}
pins={
 0x180140d45:('subss','xmm0, dword ptr [rcx + rdi + 0x10]'),
 0x180140d64:('call','0x18024e970'),
 0x180140df1:('divss','xmm0, xmm1'),
 0x180140e45:('mulss','xmm0, dword ptr [rcx + rdi + 0x10]'),
 0x180140e4b:('divss','xmm0, xmm2'),
 0x180140f3c:('mulss','xmm0, dword ptr [rcx + rdi + 0x10]'),
 0x180140f42:('divss','xmm0, xmm2'),
 0x180141088:('movss','xmm0, dword ptr [rbx + rdi + 0x1c]'),
 0x18014108e:('subss','xmm0, xmm6'),
 0x180141095:('call','0x18025c840'),
 0x18014109d:('movss','dword ptr [rbx + rdi + 0x14], xmm0')}
for at,expected in pins.items():assert instructions[at]==expected,(hex(at),instructions.get(at))
assert b.qwords(0x1802dc828,2)==(0x18013fe20,0x180140b90)
Path(sys.argv[1]).write_text(json.dumps(dict(start=hex(a),end=hex(z),sha256=hashlib.sha256(raw).hexdigest(),
 pins=len(pins),binding='275.V08',scope='Prepared finite120B score finalizer; original CRTs not executed, standalone math exponentials numerically checked.',
 topV68Completed=False,vendorExecution=False),indent=2)+'\n')
print('PASS complete140B90 region/',len(pins),'pins/275.V08 binding; score16, rank20 from log-rank28')
