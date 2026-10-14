"""Static first regular-label dispatch proof; no vendor DLL execution."""
import argparse,hashlib,json,pathlib,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
def asm(start,end):return '\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}' for i in cs.disasm(b.data[b.off(start):b.off(end)],start))
assert b.qwords(0x1802de610+0x98,1)[0]==0x180151fe0
assert b.qwords(0x1802e1ff0+0x28,1)[0]==0x1801a7580
span=asm(0x1801a7580,0x1801a75c7);assert 'inc ecx' in span and 'cmovl ecx, eax' in span
dispatch=asm(0x1801ef5c0,0x1801ef5ec)
assert 'movzx r9d, r15w' in dispatch and 'mov r10, qword ptr [rcx + 0xb0]' in dispatch
assert b.qwords(0x1802de610+0xb0,1)[0]==0x180152030
assert b.qwords(0x1802e1ff0+0x70,1)[0]==0x1801a7850
assert 'movzx eax, byte ptr [rax + rcx + 4]' in asm(0x1801a7850,0x1801a78cf)
assert b.qwords(0x1802de610+0x280,1)[0]==0x180155710
assert b.qwords(0x1802e1ff0+0x1c8,1)[0]==0x18005a400
assert 'xor al, al' in asm(0x18005a400,0x18005a403)
outer=f['1801ef070'];inner=f['1801ee3b0']
assert 'local_274 = 0' in outer and 'local_274 = uVar35 + 1' in outer and 'uVar47 = uVar47 + 1' in outer
assert '0x13 < uVar49' in outer
assert '*(longlong *)(local_1d8 + 0x50)' in outer and '*(longlong *)(local_1d8 + 0x58)' in outer
assert '(local_2b3 == 0)' in outer
assert 'param_10 != *(short *)(param_2 + 0xe4)' in inner
assert 'sVar2 = *(short *)(param_2 + 0xe8)' in inner and 'if (param_10 == sVar2)' in inner
assert 'param_10 == *(short *)(param_2 + 0xe4)' in inner and 'uVar8 = 6;' in inner
assert 'uStack_bec = CONCAT31(uStack_bec._1_3_,2)' in inner
assert '(param_6 - param_8) + -1' in inner
#Constructor fact is recorded separately from the still-opaque global factory.
assert b.qwords(0x1802e73d0+0x18,1)[0]==0x1802e7450
assert b.qwords(0x1802e7450+0x10,1)[0]==0x1802e73b8
assert b.qwords(0x1802e73b8,1)[0]==0x1801eada0
ctor=asm(0x1801eada0,0x1801eadc0);assert 'mov qword ptr [rdx + 0x10], rax' in ctor and 'xor eax, eax' in ctor
report=dict(order='1EF070 node thenascending span thenascendingarc andnative top20;actualarcspan via1EF5CB R9/152030/1A7850',
 categories='startMap50 first, endMap58 whenordinary280 false; duplicates retained; no Unicode-script filtering',
 guards='RootE4 excluded;nonrootkind0E4->6;E8xorprior disableskind0;kind2 unchanged',
 unresolved='Blank5/ancestor/reused-source/protocol-owner194 fullproduction;20e constructorcount0 proven separately, factory-count/identity binding not assumed bythisAPI',
 ctor20e='descriptor2E73D0+18->VT2E7450+10->2E73B8->1EADA0 zero10')
out=pathlib.Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report,separators=(',',':'))+'\n',encoding='utf-8');print(json.dumps(report))
