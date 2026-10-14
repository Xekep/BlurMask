"""Static1EE3B0 packed-row ancestor/control-owner audit. No vendor execution."""
import argparse,hashlib,json,pathlib,sys
p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
s=functions()['1801ee3b0'];start=s.index('while( true )',s.index('local_ba8 = local_b88 * 0x28'))
walk=s[start:s.index('local_ca0 = local_c36',start)]
for field in ('0xe2','0xe6','0xe0','0xea','0xec','0xee'):assert field in walk
assert 'iVar20 - (*(ushort *)(lVar16 + 0xe + lVar1) >> 1 & 7)' in walk
assert 'iVar11 == -1' in walk and 'uVar23 = (ulonglong)*(byte *)(lVar16 + 9 + lVar1)' in walk and 'iVar20 = iVar11' in walk
assert walk.index('iVar11 == -1')<walk.index('uVar23 = (ulonglong)*(byte *)')
after=s[s.index('local_ca0 = local_c36',start):s.index('if (local_c1c != 0)',start)]
assert 'uStack_bfa = *(undefined2 *)(*(longlong *)(lVar17 + lVar4) + 0x10 + lVar1)' in after
assert 'uStack_bf8 = *(undefined2 *)(*(longlong *)(lVar4 + local_ba0) + 0x10 + local_ba8)' in after
assert 'iStack_c04 = iVar20' in after and 'iVar20 = local_bb0' in after
#Replacement xor guard compares the original acoustic parent, while the actual
#callback PreviousSymbol was loaded from the ancestor into uStack_be2.
assert 'uStack_be2 = *(undefined2 *)(*(longlong *)(lVar17 + lVar4) + 0xc + lVar1)' in after
assert '*(short *)(lVar16 + 0xc + lVar17) != sVar2' in s and 'lVar17 = *(longlong *)(local_ba0' in s
report={'walk':'1EE3B0 skipsE2/E6 and whenE0false EA/EC/EE, follows byte9/packedspan atE bits1..3; stops atroot before reading255',
 'separateOwners':'uStack_bfa=ancestor.arc;uStack_bf8=original.arc;iStack_c04=ancestor.node; local_ba0/local_ba8 retain original node/alternative',
 'replacementPolicy':'E8 xor uses original prior symbol; native callback previous symbol uStack_be2 comes from ancestor, so narrow prior-node rewriting is not valid',
 'unresolvedConsumer':'Full F0/F8 compatibility/request producer plus blank acoustic fallback/insertion remains next; scoped resolver does not replace them'}
out=pathlib.Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report)+'\n',encoding='utf-8');print(json.dumps(report))
