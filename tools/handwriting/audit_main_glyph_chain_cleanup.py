"""Static274.V40/13B1D0 reversal and trailing-record destruction policy."""
import hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a,z=0x18013b1d0,0x18013b350;raw=b.data[b.off(a):b.off(z)]
assert hashlib.sha256(raw).hexdigest()=='b107da5f474204c14527cef1fd4c32a04f9554df7d5730c822643dc10e1409f8'
instructions={i.address:(i.mnemonic,i.op_str)for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,a)}
pins={
 0x18013b1e2:('mov','rbp, r8'),0x18013b1e5:('mov','rdi, r9'),
 0x18013b1f6:('mov','r8, qword ptr [rax + 0x20]'),
 0x18013b1fa:('mov','qword ptr [rax + 0x20], 0xffffffffffffffff'),
 0x18013b219:('mov','rdi, r8'),0x18013b225:('mov','qword ptr [rax + 0x20], rbx'),
 0x18013b272:('mov','rbp, qword ptr [rax + 0x20]'),
 0x18013b2cd:('sar','ecx, 4'),0x18013b2d0:('dec','ecx'),
 0x18013b2d2:('cmp','edx, ecx'),0x18013b2d4:('jne','0x18013b320'),
 0x18013b2e5:('mov','rdx, qword ptr [rdx + rcx*8 + 0x18]'),
 0x18013b30b:('dec','dx'),0x18013b31c:('mov','word ptr [rbx + 0x18], ax')}
for at,expected in pins.items():assert instructions[at]==expected,(hex(at),instructions.get(at))
assert b.qwords(0x1802dc680+0x40,1)==(a,)
Path(sys.argv[1]).write_text(json.dumps(dict(start=hex(a),end=hex(z),sha256=hashlib.sha256(raw).hexdigest(),
 pins=len(pins),binding='274.V40',scope='Owned no-geometry pool, inclusive acyclic segment; native tail-only pop preserves owner capacity/flags.',
 vendorExecution=False,topV68Completed=False),indent=2)+'\n')
print('PASS complete13B1D0 region/',len(pins),'pins/274.V40 binding; reversal then per-owner trailing pop')
