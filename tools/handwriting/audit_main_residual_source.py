"""Static PE only:217DB0 -> source graph168 -> ordinary1AC3C0."""
import hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();f=functions();cs=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
blocks=[(0x1801ac290,0x1801ac3b2,'84c6c6a6cf4aa4d81128d58b2959c2694d74be4ca4bd1a92297d3df8522de484'),
 (0x1801ac3c0,0x1801aca44,'cd9905ee200c1e1bc4768090ac0fbd9527eee7d88d8775bb7f91d6ec73729761'),
 (0x1801a77d0,0x1801a784d,'b0977a186cedd4570a3d12effa15e3d4097c71431c6d83b886b5e0ba1cad4d64'),
 (0x1801ace90,0x1801acef0,'5cfe4b2beac550fe0e42a53e2c102208c0fb11644a57fa0ad7ab03ab13d646c0'),
 (0x1800ca630,0x1800ca635,'0a6f8b667b1b596d05bbe6a2099e2a0034bf94c7c4546d0626e3745e425f1f3f')]
for a,z,s in blocks:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==s
assert b.qwords(0x1802de610+0x248,1)[0]==0x1800ca630
assert b.qwords(0x1802e1ff0+0x168,1)[0]==0x1801ace90
assert b.qwords(0x1802e1ff0+0x68,1)[0]==0x1801a77d0
sort=f['1801ac290'];merge=f['1801ac3c0'];association=f['180217db0']
assert '((int)uVar13 + param_4) / 2' in sort and 'iVar5 = *(int *)(param_2 + 4 + lVar8 * 0xc)' in sort
assert 'if (param_3 == 1)' in merge and '*param_5 = *param_4' in merge
assert merge.index('*(longlong *)(param_2 + 0x248)')<merge.index('*(longlong *)(param_2 + 0x2c8)')
assert '(double)local_a4 * 1.5' in merge and 'local_6c = local_a4;' in merge
assert '*(int *)(lVar13 + 8 + uVar10 * 0xc) + -1' in merge
assert merge.index('*(uint *)(param_2 + 0x290)')<merge.index('local_a8 = 0;')
assert 'local_68 = uVar12;' in merge and 'local_80[0] = 0;' in merge
assert 'FUN_18019c080(param_1,lVar11,local_80)' in merge
assert 'FUN_1801ac3c0' in f['1801ace90']
assert '*plVar5 + 0x248' in association and '*local_100 + 0x168' in association
assert 'lVar6 + 0x30 + lVar13 * 0x48' in association and 'lVar6 + 0x44 + lVar13 * 0x48' in association
asm=[(i.mnemonic,i.op_str) for i in cs.disasm(b.data[b.off(0x1800ca630):b.off(0x1800ca635)],0x1800ca630)]
assert asm==[('mov','rax, qword ptr [rdx + 0x70]'),('ret','')]
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(dict(blocks=[dict(start=hex(a),end=hex(z),sha256=s) for a,z,s in blocks],
 chain='217DB0 frontend248 ->graph; graph168=1ACE90 selects ordinary1AC3C0',
 higher='node-local32 typed record, ranges/capacity and originalLastRef18; graph68 preserves original base NNidentity',
 order='native unstable middle-pivot(Stroke,Start), adjacent shared-endpoint only',
 search='lastnode extra cache first; FIRSTglobalordinary exactrange thenallnodes signedstartMin/endMax',
 refusal='multi-input higher is unsafe before cache because concatenation directly reads base arrays',
 scope='fresh135 no99 override; actualbacktrackrows retained; currentrealfixture haszero residuals',noVendorExecution=True),indent=2)+'\n',encoding='utf-8')
print('PASS static5 pinned blocks/3bindings, native sort/coalesce/cache/firstglobalgroup/capacity/lastref and217DB0 row30/44 publication')
