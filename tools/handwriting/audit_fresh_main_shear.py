"""Pinned PE-only24D/AL/physical-record ownership audit. No vendor execution."""
import argparse,hashlib,json,struct,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
text=Binary('MyScriptText.dll');engine=Binary('MyScriptEngine.dll');fs=functions();checks={}
assert hashlib.sha256(text.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(engine.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
def terms(address,*parts):
 s=fs[f'{address:x}'];assert all(v in s for v in parts),(hex(address),parts);checks[f'{address:x}']=list(parts)
terms(0x180163970,'*(undefined1 *)(param_2 + 0x9c) = 0;')
terms(0x180167210,'uVar17 = 0xffffffff;','*(char *)(param_2 + 0x9c)','(*local_res10 & 0xf) != 6','*(uint *)(lVar11 + 4 + lVar16) = uVar17;')
terms(0x18016aa00,'lVar24 = (ulonglong)uVar28 * 0x1c;','puVar1 = (undefined4 *)(lVar24 + lVar23);','uVar4 = puVar1[1];','puVar2[1] = uVar4;','*(undefined4 *)(lVar20 + 0x18 + uVar15)')
terms(0x180157c60,'param_3[0xb]','iVar11 = *(int *)((ulonglong)local_1f8 * 0x1c + 4 + param_3[0xb])','lVar15 = uVar27 * 0x1c;','*(int *)(param_3[0xb] + 4 + lVar15) != -1','FUN_180156950','param_3 + 5')
terms(0x180158970,'*(longlong *)(param_4 + 0x130) != 0','FUN_180157c60','*(char *)(param_4 + 0xc0)')
terms(0x180170400,'FUN_18016f170')
terms(0x1801781e0,'param_2 + 8,1,1','param_2 + 9,1,1','FUN_180250480','param_2 + 0x34','param_2 + 0x28')
terms(0x180177280,'*plVar30 + 0x48','*plVar30 + 0x60','sVar3 == local_114[0]','local_res20 < fVar43','fVar44 = fVar44 + fVar41','*(int *)((ulonglong)uVar29 * 0x1c + 4 + lVar26) == -1','dVar13 / dVar14','fVar41 = (fVar41 + local_118) * 0.5','param_3[0x26]','*pfVar23 = -fVar41 * pfVar23[1] + *pfVar23 + fVar42;')
assert text.qwords(0x1802dff38+0x30,1)[0]==0x180177280
checks['24D-vtable-process']='2DFF38+30=177280'
assert engine.qwords(0x1801de1b0+0x48,1)[0]==0x180052b80
assert engine.qwords(0x1801de1b0+0x60,1)[0]==0x180052c00
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
for address,size,expected in [(0x180052b80,0x32,['[rdx + 0xd0]','[rdx + 0xd2]','[rdx + 0xd4]','[rdx + 0xd6]']),
 (0x180052c00,0xc9,['[rdx + 0x70]','[rdx + 0x78]','[rdx + 0x60]'])]:
 instructions=[i.mnemonic+' '+i.op_str for i in md.disasm(engine.data[engine.off(address):engine.off(address)+size],address)]
 assert all(any(v in i for i in instructions) for v in expected);checks[f'Engine{address:x}']=instructions
ak=(ROOT/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<IIIHH',ak,188012)==(0x101fe90,30,0x24d,0,0x100)
assert ak[188028:188030]==bytes([1,5]);checks['actual24D']={'offset':188012,'countGate':[1,5],'floatParameters':struct.unpack_from('<5f',ak,188030)}
report=dict(checks=checks,scope='Fresh direct source9C0/ordinarykind0/noPrior/noExternal: physical+4 is167210−1, fullcopy16AA00 preserves it; scoped249 and24B produce points/720, not recognized-input physical metadata. General prior/reused/kind6 source not inferred. Native29B/265 remain separate.')
Path(args.output).write_bytes((json.dumps(report,indent=2)+'\n').encode())
print(f'PASS {len(checks)} static24D/source physical metadata/AL accessor bindings + pinned resource; PE bytes only.')
