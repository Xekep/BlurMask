"""Read-only pinned preparation/missing-Arabic-map branch audit."""
import hashlib,json,struct,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2 or Path(sys.argv[1]).suffix.lower()!='.json':raise ValueError('output.json')
text=Binary();engine=Binary('MyScriptEngine.dll');checks={}
assert hashlib.sha256(text.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(engine.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
f=functions()['1802324e0']
for part in ['0x1d','local_64 = *(ushort *)((longlong)plVar2 + 0xd6)','local_64 = *(ushort *)((longlong)plVar2 + 0xd2)',
 '*plVar2 + 0x60','local_48 == 0','local_68 == 0xffff','local_78 = CONCAT22(local_74,(ushort)local_78)',
 '*(undefined1 *)(local_50 + lVar16) = 0','uVar5 == 0xfffc','uVar5 + 9','uVar5 + 0x1d',
 '*(undefined1 *)(lVar10 + lVar12) = 7','*(undefined1 *)(iVar17 + lVar16) = 6']:
 assert part in f,part
checks['preparation2324e0']='two extended tables, mode1 repair, mode2 substitution, native second-table zero write, literal overrides'
for address,cp in [(0x1802e9b80,45),(0x1802e9b70,39),(0x1802eb6c4,105),(0x1802eb6c8,106),
 (0x1802e9b7c,44),(0x1802e9b8c,58),(0x1802e9b90,59),(0x1802e9b84,46),(0x1802e9b64,33),(0x1802e9b9c,63)]:
 assert struct.unpack_from('<I',text.data,text.off(address))[0]==cp
checks['literal-scalars']=[45,39,105,106,44,58,59,46,33,63]
fs=functions('MyScriptEngine.dll');arabic=fs['1800573b0'];gate=fs['18004e280']
assert 'uVar6 == 0' in arabic and 'return 1;' in arabic and 'param_2[0x11] = lVar10' in arabic
assert 'iVar4 != 5' in gate and 'iVar2 != 1' in gate and 'iVar2 - 0x660U < 10' in gate and 'iVar2 - 0x6f0U < 10' in gate
assert engine.data[engine.off(0x180052640):engine.off(0x180052640)+13].hex()=='33c0483982880000000f95c0c3'
checks['ArabicMapAbsent']='0573B0 returns without allocation when04E280 finds no script5 string;052640 checks88 pointer. Conservative scope rejects any script5 scalar.'
Path(sys.argv[1]).write_text(json.dumps(checks,indent=2)+'\n',encoding='utf8');print('PASS pinned case preparation,10 literal scalars and conservative no-Arabic-map owner proof.')
