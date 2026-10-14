"""Pinned static13AF30/first23F.FC/frontend1A0 endpoint evidence. No DLL execution."""
import argparse,hashlib,json,pathlib,sys
p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
import capstone
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
f=functions();cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
def asm(start,end):return '\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}' for i in cs.disasm(b.data[b.off(start):b.off(end)],start))
assert b.qwords(0x1802de610+0x1a0,1)[0]==0x180152da0
assert b.qwords(0x1802e1ff0+0x130,1)[0]==0x1801aae70
assert b.qwords(0x1802dc680+0x30,1)[0]==0x18013af30
assert b.qwords(0x1802dc900+0x28,1)[0]==0x18013f410
assert b.qwords(0x1802dc900+0x30,1)[0]==0x18013f370
forward=asm(0x180152da0,0x180152db1);assert '[rdx + 0x70]' in forward and '[rax + 0x130]' in forward
ctor=asm(0x1801edb60,0x1801edc67)
assert 'xor ecx, ecx' in ctor and '1801edc52 mov dword ptr [rdx + 0xfc], ecx' in ctor
symbol=asm(0x18013af94,0x18013b02a)
for pattern in ['[rsp + 0x38], 0','mov r8d, 4','test edx, edx','mov r9d, ecx','cmp cx, 3','cmp edx, 0x2000','jl 0x18013afce','[rsp + 0x30]','shl cx, 0xd','and dx, r11w','and r9w, 3','shl cx, 2','mov r8d, 1','mov r10d, 0xfff8','[rcx + rdx*8 + 4]','[rcx + rdx*8 + 8]']:assert pattern in symbol,pattern
endpoint=f['1801aae70']
for pattern in ['param_3[6] < *(int *)(param_2 + 0x220)','-1 < (short)local_b0[6]','uVar24 = param_2 + 0x1f0','*(int *)(uVar24 + 0x18) == 0','iVar6 = param_3[2]','if (iVar6 == -1) goto LAB_1801ac0a9','(local_b0 == (uint *)0x0)) goto LAB_1801ac0a9','*(int *)(param_4 + 1) = iVar26']:assert pattern in endpoint,pattern
reset=f['1801a7230'];assert '*(undefined4 *)(param_2 + 0x208) = 0' in reset
context=f['18019eb10'];assert 'param_4[3]' in context and 'uVar4 = 0xffffffff' in context and '*param_3 = (short)uVar4' in context
seed=f['1801a1ef0'];assert '(ulonglong)(iVar32 + uVar21) * 0x60 + 4 +' in seed and '*(undefined2 *)(puVar35 + 6) = uVar3' in seed
merge=f['1801a37b0'];assert 'uVar2 = *(undefined2 *)(param_5 + 0x18)' in merge and '*(undefined2 *)(puVar18 + 6) = uVar2' in merge
force=f['18013f370'];normal=f['18013f410']
assert 'local_res8,\n                             param_7)' in force and 'local_res8,0)' in normal
report=dict(bindings=5,firstDescriptorIdBase=0,ctorInstruction='1801EDC52: mov I32[object+FC],ECX (ECX=0)',
 ordinaryContext='19EB10 all four source flags0 ->contextFFFF;1A1EF0 copies compactGroup4 toarc18;1A37B0 preservesleftarc18',
 endpoints='Leading previousNode=-1/trailing currentNode=NodeCount; empty current/fallback metadata ->remaining capacity unchanged, C/D flags0, emittedcount0',
 symbolProduction='count0 ->kind1,label1; externalvariant ->kind4; FFF9..FFFC signedcount<8192 ->packedkind1; otherskind0',
 forceDuplicate='276.virtual30 forwards actuallastflag;276.virtual28 forces0; no fieldFC mutation asserted outside fresh constructor scope',
 scope='Read-only static native evidence; no vendor execution, interior delimiter/protocol geometry and full multilingual decode excluded')
target=pathlib.Path(a.output);target.parent.mkdir(parents=True,exist_ok=True);target.write_text(json.dumps(report,separators=(',',':'))+'\n');print(json.dumps(report))
