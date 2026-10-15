"""Pinned static selected-resource to final marker-cap proof, no DLL loading."""
import argparse, hashlib, json, struct, sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
b=Binary();fs=functions();checks={}
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
def terms(address,name,*values):
    assert all(v in fs[address] for v in values),(name,values)
    checks[name]=dict(address=address,terms=values)
terms('1801d4950','version-gated-optional148',
      "local_56[0] = '\\0';",'if (0x10f < param_4)',
      '*(undefined8 *)(param_2 + 0x148) = 0;',"if (local_56[0] != '\\0')")
assert b.qwords(0x1802e5a20+0xa0,1)[0]==0x1800c2430
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
asm=list(md.disasm(b.data[b.off(0x1800c2430):b.off(0x1800c2430)+12],0x1800c2430))
assert asm[0].mnemonic=='mov' and asm[0].op_str=='rax, qword ptr [rdx + 0x148]' and asm[1].mnemonic=='ret'
checks['selected247-virtualA0']=['0x1802e5ac0','0x1800c2430',asm[0].mnemonic+' '+asm[0].op_str]
terms('180150d00','driver80-owner',
      'if ((*(longlong *)(param_2 + 0x80) == 0)',
      '*plVar11 + 0xa0', '*(longlong *)(param_2 + 0x80) = lVar13;',
      'local_78 = *(longlong *)(param_2 + 0x80);')
terms('1801a6980','297-init-category-owner',
      'local_20 = *(undefined8 *)(param_3 + 0x28);',
      '*plVar3 + 0x18', 'param_1,plVar3,&local_38')
terms('18018d920','297-stored-category-owner',
      '*(undefined8 *)(param_2 + 0x1c8) = *(undefined8 *)(param_3 + 0x18);')
terms('180191430','closed-cap-absence-consumer',
      '*(longlong **)(param_2 + 0x1c8)', 'iVar8 = -1;',
      '*plVar4 + 0x30', 'if (-1 < iVar8)',
      'if ((bVar7) && (iVar8 < iVar9))', 'iVar9 = iVar8;',
      'if ((int)(uint)*(byte *)(param_2 + 300) < iVar23)')
ak=(ROOT/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<H',ak,14)[0]==0x10d and struct.unpack_from('<H',ak,188171)[0]==0x106
assert ak[188157+59]==6 and struct.unpack_from('<f',ak,188157+60)[0]==4
tail=189734+8+struct.unpack_from('<I',ak,189738)[0]
assert struct.unpack_from('<I',ak,tail)[0]==0 and 8+struct.unpack_from('<I',ak,4)[0]-tail==190
checks['actual-resource']=dict(sourceVersion=0x10d,samplerOffset=188157,samplerVersion=0x106,
    finalMinimumGate=6,finalWidthScale=4,optional148Present=False,source283Extensions=0)
Path(args.output).write_bytes((json.dumps(dict(checks=checks,scope=
    'Fresh no-property selected main0 only; optional148 absent by reader version, '
    'not by a fabricated class count. Nonnull models and external replacements unsupported.'),indent=2)+'\n').encode())
print(f'PASS {len(checks)} static selected148 to driver80 to runtime297.1C8 to191430 policy checks; no vendor execution.')
