"""PE/resource-only fresh29B and265 bounded owner audit."""
import argparse,hashlib,json,struct,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args();b=Binary();fs=functions();checks={}
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
def terms(address,*parts):
 s=fs[f'{address:x}'];assert all(v in s for v in parts),(hex(address),parts);checks[f'{address:x}']=list(parts)
terms(0x18016a130,'*(undefined4 *)((longlong)param_2 + 0x9c) = 0xbf800000;')
terms(0x180175350,'param_2 + 0x4c','param_3 + 0xc0','if (iVar17 == 0)','*(undefined4 *)(param_3 + 0x9c) = *(undefined4 *)(param_2 + 0x24);','FUN_180175260','fVar48 = -3.4028235e+38;','0.10000000149011612 <= dVar32','*(float *)(param_3 + 0x9c) = (float)dVar45;','local_118 == 0.0')
terms(0x180175260,'*(float *)(param_2 + (longlong)(iVar6 >> 1) * 8)','uVar5 = *(undefined8 *)(param_2 + lVar9 * 8);','param_3 = iVar8;')
terms(0x180173c10,'*(int *)(param_3 + 0x80) == 0','*(longlong *)(param_3 + 0x78) + 4','FUN_1801728c0','*(undefined4 *)(param_3 + 0xa0)','*(undefined4 *)(param_3 + 0xa4)','fVar14,&local_38,&local_48,200','*(int *)(param_3 + 0x120) != 0','uVar13 = (ulonglong)*puVar11 / 200;')
assert b.qwords(0x1802dfcb0+0x18,1)[0]==0x180175350;checks['29B-table']='2DFCB0+18=175350'
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
for address,size,terms_ in [(0x180175160,0x53,['mov byte ptr [rdx + 0x4c], 0']),
 (0x1801751c0,0x2f,['movzx eax, byte ptr [r8 + 0x4c]','mov byte ptr [rdx + 0x4c], al'])]:
 lines=[i.mnemonic+' '+i.op_str for i in md.disasm(b.data[b.off(address):b.off(address)+size],address)]
 assert all(v in lines for v in terms_);checks[f'{address:x}']=lines
ak=(ROOT/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes();assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<IIIHH',ak,188050)==(0x101fe90,50,0x29b,0,0x100);assert ak[188090:188092]==bytes([10,30])
assert struct.unpack_from('<IIIHH',ak,188108)==(0x101fe90,40,0x265,0,0x102)
checks['actualnodes']={'29B':188050,'265':188108}
Path(args.output).write_bytes((json.dumps(dict(checks=checks,scope='Fresh no-prior/no-event typed main source:29B active-mixture blend explicitly refused; source9C−1 preserved when insufficient data.265 uses original15ABE0 metrics, current perdescriptor guides and exact prior-map composition.'),indent=2)+'\n').encode())
print(f'PASS {len(checks)} static29B/265 ownership/ctor/clone/source9C checks + actual resource; PE-only.')
