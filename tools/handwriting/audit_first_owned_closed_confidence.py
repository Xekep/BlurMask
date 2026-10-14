"""Pinned static first1BCBD0 closed-primitive confidence source ownership."""
import argparse,hashlib,json,pathlib,sys
p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
import capstone
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
f=functions();init=f['1801b0760'];score=f['1801bcbd0'];seed=f['18019f850']
assert 'lVar15 = param_3[4];' in init and 'param_2[0x2e] = lVar15;' in init
assert '0x218)' in init and 'param_2[0x2f] = lVar15;' in init
assert 'uVar1 == *(ushort *)(param_2 + 0x1e4)' in score and '+ 0xc)' in score
begin=score.index('(**(code **)(**(longlong **)(param_2 + 0x180) + 0x90))')
end=score.index('\n    else {',begin);closed=score[begin:end]
assert '(param_1,*(longlong **)(param_2 + 0x180),param_4,local_e8)' in closed
assert '(param_1,*(undefined8 *)(param_2 + 0x170),*local_e8[0])' in closed
assert '*(longlong *)(param_2 + 0x178) + 0x60' in closed
assert '*(undefined4 *)(lVar3 + uVar14 * 4) = uVar17' in closed and 'FUN_18013e8e0' not in closed
assert 'uint *local_138;' in seed and 'puVar32[4] = local_138;' in seed
assert 'local_138[(ulonglong)(uStack_130 - 1) * 3]' in seed
assert b.qwords(0x1802df340+0x60,1)[0]==0x180168c00
assert b.qwords(0x1802e1ff0+0x90,1)[0]==0x1801a7970
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
def asm(start,end):return '\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}' for i in cs.disasm(b.data[b.off(start):b.off(end)],start))
getter=asm(0x180168c00,0x180168c19)
assert '[rdx + 0x120]' in getter and 'imul rcx, rax, 0x1c' in getter
assert '[r9 + 0x58]' in getter and 'movss xmm0, dword ptr [rax + rcx + 0xc]' in getter
ranges=asm(0x1801a7970,0x1801a79dd)
assert '[rdx + 0x288]' in ranges and '[rcx + rax + 7]' in ranges
assert 'movups xmm0, xmmword ptr [rcx + rax + 8]' in ranges
assert 'movups xmm0, xmmword ptr [rcx + rax + 0x20]' in ranges
report=dict(sourceBinding='1B0760 descriptor4 -> ordinary170; global cast218 ->178, not knowledge',
 graphBinding='21B90=1A7970 ordinary kind0 copies compact+8/wide+20 rangevector; firstU32 range field is physicalID',
 value='21860=168C00 original secondary120+58+physical*1C+C;1BCBD0 stores rawfloat directly without sigmoid/mean/clamp',
 ownership='Factory/cuts/graph and actual NN/cache groups; graph order separate from normalization chronology',
 scope='Closed confidence transport only; no generic circle recognition, literal primitive conversion or Russian OCR claim')
target=pathlib.Path(a.output);target.parent.mkdir(parents=True,exist_ok=True);target.write_text(json.dumps(report,separators=(',',':'))+'\n');print(json.dumps(report))
