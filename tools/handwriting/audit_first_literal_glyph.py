"""Static fresh6C1F42 symbol/count/category/publication branch; no vendor execution."""
import argparse,hashlib,json,pathlib,sys
p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
import capstone
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
f=functions();init=f['1801edde0'];t=f['1801f42d0']
assert 'uVar4 = (**(code **)(*plVar1 + 0x10))' in init and '*(undefined2 *)(param_2 + 0xe2) = uVar4' in init
assert 'uVar13 == *(ushort *)(param_2 + 0xe2)' in t and 'uVar35 = *(ushort *)((longlong)local_418 + 0xe)' in t
assert '+ 0x6c)' in t and 'local_ba = uVar35;' in t and 'goto LAB_1801f5250;' in t
assert 'iVar14 = 1;' in t and 'iVar21 = 0xff;' in t
assert 'local_4c8 = (undefined1 *)CONCAT44(local_4c8._4_4_,iVar14)' in t
assert 'if ((local_3c0 == 0)' in t and "cVar11 < '\\0'" in t and 'uVar33 = 0;' in t
assert '*(longlong *)(local_3c0 + 0x78)' in t and '*(longlong *)(local_3c0 + 0x50)' in t
assert '(uVar35 == *(ushort *)((longlong)local_418 + 0xe))' in t and 'uVar13 = 8;' in t
assert '*(ushort *)(lVar17 + 4) & 0xfff7' in t
assert 'uVar13 | 0x40' in t and 'uVar35 == *(ushort *)(param_2 + 0xe8)' in t and 'uVar13 | 0x48' in t
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
update='\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}' for i in cs.disasm(b.data[b.off(0x18013af94):b.off(0x18013b000)],0x18013af94))
assert 'cmp byte ptr [rsp + 0x38], 0' in update and 'test edx, edx' in update
assert 'lea ecx, [r9 + 7]' in update and 'cmp cx, 3' in update and 'xor r8d, r8d' in update
report=dict(branch='Fresh6C keeps selected label literally/count1; preceding E2=alphabet count maps to actual headerE virtual label',
 categories='Raw startMap missing ->geometry0, mismatch8 exceptheaderE; publication40 withadditional8foractualU+FFFD',
 actualLabels='HeaderE178 virtual; space180; newline182; FFFD189; F0000190; F0001191(category24/firstID0/geometry13)',
 scope='Prepared selected-label classification/update only; no wholewinner/script substitution; F0001 emits literalprivateUnicode, headerE requireswrapperfilter and F0000requirescircledcodec')
target=pathlib.Path(a.output);target.parent.mkdir(parents=True,exist_ok=True);target.write_text(json.dumps(report,separators=(',',':'))+'\n');print(json.dumps(report))
