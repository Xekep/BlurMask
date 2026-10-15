"""Static217DB0 controller contract. Source-service implementation is separate."""
import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
b=Binary();f=functions();md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
start,end=0x180217db0,0x180218560;digest='5cfc62caadf652d8b89b314509b6b280e3733407e4344096f606b09cd08fbdcd'
assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest()==digest
def asm(a,z):return [(i.mnemonic,i.op_str) for i in md.disasm(b.data[b.off(a):b.off(z)],a)]
assert asm(0x18021847b,0x180218486)==[('mov','qword ptr [r14 + rbp*8 + 0x30], rax'),('mov','byte ptr [r14 + rbp*8 + 0x44], 1')]
s=f['180217db0']
contracts=["*(char *)(*(longlong *)(param_2 + 0x18) + 0x99) == '\\0'",
 'local_138 = *(char *)(lVar13 + 0xc1)', 'local_134 = -1',
 'fVar24 = 3.4028235e+38', 'fVar23 < fVar24', 'iVar11 < iVar10',
 'fVar23 = ABS(fVar23) - (fVar3 + fVar3)', '(local_d4 + local_cc) * 0.5 < *pfVar14 - fVar3 * 0.75',
 'iVar11 = (uVar20 - *piVar19) + -1', '*(char *)(puVar18 + 0x1c)',
 'uVar9 = iVar11 + 1', '*(undefined4 *)(param_2 + 0x24b0) = 0',
 '*(int *)(param_2 + 0x24a0) + -1', 'lVar6 + -8 + lVar13 * 0x48']
for contract in contracts:assert contract in s,contract
bindings={0xb8:0x180152040,0xe8:0x180152280,0x198:0x180152d40,0x248:0x1800ca630,0x250:0x1801530f0}
for slot,target in bindings.items():assert b.qwords(0x1802de610+slot,1)[0]==target
assert b.qwords(0x1802e1ff0+0x168,1)[0]==0x1801ace90
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(dict(functionSha256=digest,contracts=len(contracts),bindings=6,
 scope='217DB0 controller over explicit rows/source callbacks; no allocator-failure simulation or vendor execution',
 remaining='actual source/group type, line, bounds, 218 guide owner, alphabet flag, topmark and1AC3C0 merge overlay'),indent=2)+'\n',encoding='utf-8')
print('PASS static217DB0 whole-body pin/14 scalar contracts/6 frontend and merge dispatch bindings; actual source adapters separate')
