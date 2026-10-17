"""Static-only closure for the bounded24A/unchanged24C/265 owner.
Research dependency is deliberate; the numerical audit is separately portable.
"""
from pathlib import Path
import hashlib,json,runpy,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
target=Path(sys.argv[1]);repo=Path(__file__).resolve().parents[2];saved=sys.argv[:]
sys.argv=[str(repo/'tools/handwriting/audit_noinitial_first_feature_plan.py'),str(target.with_suffix('.plan.json'))]
g=runpy.run_path(sys.argv[0]);sys.argv=saved;b=g['b'];md=Cs(CS_ARCH_X86,CS_MODE_64)
pins={
0x180176a25:('mov','esi, dword ptr [r8 + 0x44]'),
0x180176a53:('mov','rax, qword ptr [r8 + 0x58]'),
0x180176a60:('lea','rbx, [rax + 4]'),
0x180176a7a:('cmp','dword ptr [rbx], -1'),
0x180176a91:('xor','dl, dl'),
0x180176aab:('comiss','xmm1, xmm0'),
0x180176ab0:('cmp','dl, 1'),
0x180176ab9:('cmovge','eax, r9d'),
0x180176ac0:('comiss','xmm0, xmm1'),
0x180176ac5:('cmp','dl, 0xff'),
0x180176ace:('cmovle','eax, r9d'),
0x180176ae1:('cmp','dl, 0xff'),
0x180176ae8:('cmovne','eax, r9d'),
0x180176aef:('cmp','dl, 1'),
0x180176af4:('inc','r9d'),
0x180176afe:('add','rbx, 0x1c'),
0x180176b0b:('cmp','r9d, 0x14'),
0x180176b0f:('jl','0x180176f92')}
rows=[]
for address,expected in pins.items():
 instruction=next(md.disasm(b.data[b.off(address):b.off(address)+16],address))
 assert(instruction.mnemonic,instruction.op_str)==expected
 rows.append(dict(address=hex(address),bytes=instruction.bytes.hex(),instruction=' '.join(expected)))
assert hashlib.sha256(b.data[b.off(0x180176a10):b.off(0x180176fb0)]).hexdigest()=='16899888bda61da733ac6261995cea2973ce6cb3703b404354d0b2f95ad1cea9'
report=dict(staticOnly=True,noVendorExecution=True,pins=rows,
 dependency=str(target.with_suffix('.plan.json')),
 scope='NEW/no-prior/no-events ordinary selected first row; actual English119/source247 processors; physical+4=-1 survives24A;24C signed count<20 is unchanged;>=20 explicitly refused before265. No feature/classifier/controller claim.')
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS18 exact24C counter ASM pins + prior actual source/assignment/order/profile closure -> {target}')
