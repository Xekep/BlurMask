"""Pinned constructor/dispatch/dual-owner proof for fresh first protocol194.
Reads static files only. No vendor code execution or activation bypass.
"""
import argparse,hashlib,json,pathlib,re,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
def asm(start,end):return '\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}' for i in cs.disasm(b.data[b.off(start):b.off(end)],start))
#Registered249 init table, not merely an unreferenced nearby function.
assert b.qwords(0x1802dee00+0x18,1)[0]==0x1802dee68
assert b.qwords(0x1802dee68+0x10,1)[0]==0x1802deea0
assert b.qwords(0x1802deea0,3)==(0x180155d30,0x180155da0,0x180155ef0)
ctor=asm(0x180155d30,0x180155d96)
for address,offset in ((0x180155d4a,0x1d0),(0x180155d73,0x208),(0x180155d7a,0x210)):
    assert f'{address:x} mov qword ptr [rdx + {offset:#x}], rax' in ctor
assert 'xor eax, eax' in ctor and '155d58 mov byte ptr [rdx + 0x1ea], al' in ctor
create=f['180156200'];assert 'if (*(longlong *)(param_2 + 0x1d0) == 0)' in create
assert '(param_1,0x20e)' in create and '*(longlong *)(param_2 + 0x200) = lVar5' in create
assert '(param_1,0x2b3)' in create and 'param_2 + 0x210' in create
#20E registered constructor initializes source-count10; selected23F and2B3
#then receive objects created under that exact no-external branch.
assert b.qwords(0x1802e73d0+0x18,1)[0]==0x1802e7450
assert b.qwords(0x1802e7450+0x10,1)[0]==0x1802e73b8
assert b.qwords(0x1802e73b8,1)[0]==0x1801eada0
e=asm(0x1801eada0,0x1801eadc0);assert '1eada0 xor eax, eax' in e and '1eada6 mov qword ptr [rdx + 0x10], rax' in e
assert b.qwords(0x1802e7418,2)==(0x1801eb1e0,0x1801eb340)
check=f['1801eb1e0'];assert 'if (param_3 != (longlong *)0x0)' in check and check.rstrip().endswith('}')
assert not re.search(r'\*\([^\n]*param_2[^\n]*\)\s*=(?!=)',check), 'Compatibility checker unexpectedly writes its source'
bind=f['180156630'];assert 'iVar5 = (*(code *)*puVar6)' in bind
assert '*(undefined8 *)(param_2 + 0x200)' in bind and '*(undefined8 *)(param_2 + 0x208)' in bind
assert '(**(code **)(lVar7 + 8))' in bind and 'param_2 + 0x208,*plVar9' in bind
dispatch=f['1801eb340'];assert 'if (*(int *)(param_2 + 0x10) == 0)' in dispatch and 'uVar4 = 0x23f' in dispatch
assert '(**(code **)(*param_4 + 0x18))(param_1,param_4,plVar6)' in dispatch
assert '*param_3 = (longlong)plVar6' in dispatch
#2B3 descriptor+18 is a meta-record, whose+8 is the actual object vtable.
assert b.qwords(0x180274aa0+0x18,1)[0]==0x180274b68
assert b.qwords(0x180274b68+8,1)[0]==0x180274af0
assert b.qwords(0x180274af0+0x18,1)[0]==0x18000cab0
dual=asm(0x18000cab0,0x18000cac0)
assert 'cab0 mov qword ptr [rdx + 0x60], r8' in dual and 'cabb mov qword ptr [r8 + 0x18], rdx' in dual
outer=f['1801ef070'];assert 'local_194 = (uint)(*(longlong *)(*(longlong *)(param_2 + 0x18) + 0x60) != param_2)' in outer
report={'chain':'registered249155D30 clears external1D0 and controller208;156200 createsNEW20E; registered1EADA0 clears count10;156630 read-only1EB1E0 then1EB340 selects23F',
 'identity':'registered2B3 descriptor274AA0→meta274B68+8→actualVT274AF0.V18=CAB0 binds container60/controller18 both ways;1EF070194 comparison false',
 'scope':'Only existing owned fresh raw no-external/no-reused/no-serialized-driver factory. Main240/external20E are not assigned defaults.',
 'constructor':ctor,'containerBinding':dual}
out=pathlib.Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report)+'\n',encoding='utf-8');print(json.dumps({k:v for k,v in report.items() if k not in ('constructor','containerBinding')}))
