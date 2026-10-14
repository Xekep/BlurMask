"""Pinned PE-only fresh15B260 output fields/split/source map audit."""
import argparse,hashlib,json,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
parser=argparse.ArgumentParser();parser.add_argument('--output',required=True);args=parser.parse_args()
b=Binary('MyScriptText.dll');fs=functions('MyScriptText.dll');checks={}
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
def includes(addr,*terms):
    text=fs[f'{addr:x}'];assert all(t in text for t in terms),(hex(addr),terms)
    checks[f'{addr:x}']=list(terms)
includes(0x180076780,'param_2 + 0x98) = 0')
includes(0x180076ad0,'param_2 + 0x98) = 0')
includes(0x18007a850,'*(longlong *)(param_2 + 0x98) == 0','if (param_4 == 0)','iVar36 = -0x80000000','+ 0x20 + (longlong)pfVar24) = iVar36','+ 0x24 + (longlong)pfVar24) = 0xffffffff','+ 0x28 + (longlong)pfVar24) = 1')
includes(0x1801598e0,'FUN_18025ee50(lVar2,0,(ulonglong)param_5 * 0x58)','*(uint *)(param_2 + 1) = (int)param_2[1] + param_5')
includes(0x18015b260,'puVar16[0x10] = 2','puVar16[0x11] = 0x200','if (uVar10 == 1)','if (uVar10 < 2)','FUN_1800722c0','puVar15[0xc] = 3','puVar15[0xd] = 0x200','local_f0 == 0','puVar15[0x38] = 3','puVar15[0x41] = 0x3f00','puVar16 = puVar15 + 0x58','FUN_18016aa00')
includes(0x18016a130,'*(undefined4 *)(param_2 + 0x25) = 200')
includes(0x18016aa00,'FUN_18016a6e0','uVar28 = *(uint *)(param_5 + uVar21 * 4)','puVar2[1] = uVar4','*(undefined4 *)(lVar20 + 0x18 + uVar15)','*(int *)(param_2 + 0x120) == 0','(int)(lVar20 >> 3) * iVar25')
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
inst=list(md.disasm(b.data[b.off(0x18015b260):b.off(0x18015b260)+0xb00],0x18015b260))
call=next(n for n,i in enumerate(inst) if i.mnemonic=='call' and i.op_str=='0x1801598e0')
seq=inst[call-16:call+1]
assert any(i.mnemonic=='lea' and i.op_str=='eax, [r13 - 1]' for i in seq)
assert any(i.mnemonic=='add' and i.op_str=='eax, eax' for i in seq)
assert any(i.mnemonic=='mov' and i.op_str=='dword ptr [rsp + 0x20], eax' for i in seq)
assert any(i.mnemonic=='inc' and i.op_str=='r8d' for i in seq)
checks['1598e0-count2Gminus2-aftercurrent']=[f'{i.address:x} {i.mnemonic} {i.op_str}' for i in seq]
for address,expected in [(0x180080360,'eax, dword ptr [rdx + 0x108]'),(0x180080400,'rax, [rdx + 0x100]')]:
    decoded=list(md.disasm(b.data[b.off(address):b.off(address)+8],address));assert decoded[0].op_str==expected
    checks[f'{address:x}-outputowner']=expected
Path(args.output).write_bytes((json.dumps(dict(checks=checks,scope='Fresh main15B260 descriptor split/16AA00, no source720/prior/page lines, unknown2E3 provider metadata;15F050 not claimed'),indent=2)+'\n').encode('utf8'))
print(f'PASS {len(checks)} pinned fresh provider output/split/reorder checks; no vendor execution')
