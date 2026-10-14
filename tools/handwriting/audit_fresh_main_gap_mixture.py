"""Pinned static EM native scalar/owner/loop audit, no vendor execution."""
import argparse,hashlib,json,struct,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args();b=Binary();fs=functions();checks={}
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
s=fs['180175350']
for label,terms in {
 'qualifying':('*(byte *)(param_2 + 0x20) <','~*(uint *)(puVar16 + 0x1a) + *(int *)(puVar16 + 0x1c)','(!bVar10)'),
 'item-gate':('piVar18 = (int *)(*(longlong *)(param_3 + 200) + 0x10','if (*piVar18 < 0)'),
 'retries':('fVar26 = (float)(1 << ((byte)uVar12 & 0x1f));','param_2 + 0x38','param_2 + 0x44','uVar12 < 5'),
 'pseudo':('dVar45 = (double)(*(float *)(param_2 + 0x10) / fVar38)','dVar28 = (double)*(float *)(param_2 + 0x34) * dVar45','dVar36 = (double)fVar38','dVar7 = dVar45;','dVar8 = dVar29;'),
 'density':('dVar46 = dVar34 + dVar30','4.440892098500626e-16','2.220446049250313e-16','dVar36 = dVar36 + 1.0','dVar7 = dVar7 + auVar41._0_8_','dVar8 = dVar8 + dVar44'),
 'publication':('fVar49 = (float)(dVar28 / dVar7)','fVar48 = (float)(dVar39 / dVar8)','(float)(dVar8 / dVar36)','(float)(dVar7 / dVar36)','1e-06 <= fVar27'),
 'intersection':('6.283185307179586','dVar28 == 0.0','(dVar39 * -0.5) / dVar36','dVar36 * dVar36 - dVar28 * dVar39','(double)(fVar49 + fVar48) * 0.5'),
 'blend':('(double)(1.0 - local_118) * dVar29 + (double)((float)dVar28 * local_118)','*(float *)(param_3 + 0x9c) = (float)dVar45;')}.items():
 assert all(t in s for t in terms),(label,terms);checks[label]=list(terms)
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);instructions=list(md.disasm(b.data[b.off(0x180175350):b.off(0x180175350)+0x1320],0x180175350))
lookup={i.address:i for i in instructions}
for address,target in [(0x180175d26,'0x18024e970'),(0x180175d54,'0x18024e970'),(0x180175f86,'0x18024f110')]:
 assert lookup[address].mnemonic=='call'and lookup[address].op_str==target
 n=instructions.index(lookup[address]);checks[f'ASM{address:x}']=[hex(i.address)+' '+i.mnemonic+' '+i.op_str for i in instructions[n-12:n+4]]
assert lookup[0x180175e76].mnemonic=='comisd';checks['convergence-double']=[lookup[0x180175e72].mnemonic+' '+lookup[0x180175e72].op_str,lookup[0x180175e76].mnemonic+' '+lookup[0x180175e76].op_str]
ak=(ROOT/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes();assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
checks['actualprofile']={'node':188050,'gaussians':struct.unpack_from('<6f',ak,188066),'countGate':list(ak[188090:188092])}
Path(args.output).write_bytes((json.dumps(dict(checks=checks,scope='Fresh unknown720 +10=-1 family from156950. Reused recognized-input physical1000/nibbles gate excluded. Numeric EM and complete actual qualifying-gap/blend output restored; no OCR label aliases.'),indent=2)+'\n').encode())
print(f'PASS {len(checks)} native active29B owner/EM/intersection/blend/ASM checks + actual resource; no DLL execution.')
