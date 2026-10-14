"""Pinned232F10 ancestor/form/AVL rebuilding audit; PE-only."""
import hashlib,json,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2 or Path(sys.argv[1]).suffix.lower()!='.json':raise ValueError('output.json')
t=Binary();e=Binary('MyScriptEngine.dll')
assert hashlib.sha256(t.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(e.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
f=functions()['180232f10']
for term in ['param_5 == param_4','iVar5 == 0x20','(ushort *)(param_5 + 4)','param_5 = (undefined8 *)*param_5',
 'uVar4 = (int)uVar7 - 1','*param_2 + 0x48','local_b0 + uVar8 * 6 + param_6','FUN_1800918d0','*param_4 = puVar9','return (undefined8 *)0x0']:
 assert term in f,term
assert e.qwords(0x1801de1b0+0x48,1)[0]==0x180052b80
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
asm=[i.mnemonic+' '+i.op_str for i in md.disasm(e.data[e.off(0x180052b80):e.off(0x180052b80)+50],0x180052b80)]
for term in ['[rdx + 0xd0]','[rdx + 0xd2]','[rdx + 0xd4]','[rdx + 0xd6]']:assert any(term in i for i in asm)
report={'rewrite232F10':'collect32 ancestors until exact parent; reverse rebuild; typed form table6/offset0,2,4; link only newly created AVL nodes',
 'Engine2BVirtual48':asm,'scope':'owned prepared non-Arabic stored alphabet and present shared context container; uninitialized form reads explicitly refused'}
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print('PASS32-ancestor native context/form rewriting and AL range accessor; PE-only.')
