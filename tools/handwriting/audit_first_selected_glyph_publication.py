"""Pin1F42D0 outer E2/E4/99 guard, before glyph append and root publication."""
import argparse,hashlib,json,pathlib,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
def asm(start,end):return '\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}' for i in cs.disasm(b.data[b.off(start):b.off(end)],start))
s=f['1801f42d0'];k=s.index('if (((sVar6 != *(short *)(param_2 + 0xe2))')
guard=s[k:k+520]
assert "sVar6 != *(short *)(param_2 + 0xe4)" in guard and "+ 0x99) != '\\0'" in guard
raw=asm(0x1801f4df5,0x1801f4e23)
for address,meaning in [(0x1801f4df5,'cmp cx, word ptr [r13 + 0xe2]'),(0x1801f4dff,'cmp cx, word ptr [r13 + 0xe4]'),(0x1801f4e0d,'cmp byte ptr [rax + 0x99], 0'),(0x1801f4e1e,'jmp 0x1801f62b9')]:assert f'{address:x} {meaning}' in raw
#The first allocation after the guard precedes the literal blank conversion,
#all1B8/1C0 score reads, and the zero-symbol/root publication branch.
tail=s[k:];allocation=tail.index('uVar13 = (**(code **)');replacement=tail.index('if (uVar13 == *(ushort *)(param_2 + 0xe2))')
assert allocation<replacement<tail.index('+ 0x1b8')<tail.index('+ 0x1c0')
assert 'local_420 = uVar28' in tail[:replacement]
root_check=tail.index('iVar14 = (**(code **)(*(longlong *)',tail.index('local_320 = 0;',tail.index('local_370 = local_494')))
assert 'if (iVar14 != 0)' in tail[root_check:root_check+1200] and '(lVar24,0x279)' in tail[root_check:root_check+1500]
backedge=asm(0x1801f62b9,0x1801f62d0);assert '1f62ca jne 0x1801f4d00' in backedge
report={'predicate':'(selected!=E2 && selected!=E4) || owned99; checked1F4DF5/4DFF/4E0D',
 'skip':'1F4E1E jumps1F62B9 then predecessorbackedge1F62CA→1F4D00, bypassing append/link/classify/confidence/rootwordpublish/leadingendpoint',
 'blankReplacement':'E2→alphabetheaderE is later INSIDE allocation branch; fresh99false cannot reach it forselectedblank',
 'scope':'Single-word owned fresh literal materializer; no aliases/traits/interior separators/all273alternatives/native languagewinner claimed',
 'instructions':raw,'backedge':backedge}
out=pathlib.Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report)+'\n',encoding='utf-8');print(json.dumps({k:v for k,v in report.items() if k not in ('instructions','backedge')}))
