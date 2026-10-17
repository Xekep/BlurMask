"""Pinned1F42D0 multiword publication, reset and scalar-delta evidence."""
import argparse,hashlib,json,pathlib,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();fs=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
word=fs['1801f42d0'];checks={}
def terms(name,*values):
    assert all(x in word for x in values),(name,values);checks[name]=list(values)
terms('outerAllocation','sVar6 != *(short *)(param_2 + 0xe2)','sVar6 != *(short *)(param_2 + 0xe4)',
      '*(char *)(*(longlong *)(param_2 + 0x18) + 0x99)')
terms('boundary','& 1) == 0','else if (iVar14 == -1)','local_450 - local_48c','local_45c -',
      'local_47c -','local_450 = local_48c','local_460 = -1','local_420 = 0xffffffffffffffff')
terms('ordinaryDeltas','local_328 = (float)(int)(uVar36 -','local_45c = *(uint *)(lVar29 + uVar30 * 0x28)',
      'local_47c = *(uint *)(lVar29 + 4 + uVar30 * 0x28)','(float)(int)((local_450 - local_48c) * 0x2000)')
terms('rootScalars','fVar42 = (float)(int)local_44c','local_438 + 1','local_328 = -fVar39 * fVar42 * 0.00012207031')
terms('interior','local_360 = 1','local_358 = 0','local_350 = 0','local_4c8._4_4_,0xfffffffe',
      '(iVar31 - iVar14 & 0xffff) << 0x18','local_3e8 = 0x100','local_320 = 0','local_328 = 1.0')
assert word.index('sVar6 != *(short *)(param_2 + 0xe2)')<word.index('& 1) == 0')
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
def instruction(addr,size):return [(i.mnemonic,i.op_str) for i in md.disasm(b.data[b.off(addr):b.off(addr)+size],addr)]
assert instruction(0x1801f5d76,5)==[('mov','dword ptr [rsp + 0x38], r12d')]
assert instruction(0x1801f5d7b,8)==[('mov','dword ptr [rsp + 0x30], 0xffffffff')]
assert instruction(0x1801f59e5,11)==[('mov','qword ptr [rbp + 0x88], 1')]
assert instruction(0x1801f59b8,7)==[('mov','qword ptr [rbp + 0x90], rax')]
publisher=fs['1801416b0'];assert 'local_120 = param_7' in publisher and 'local_124 = param_8' in publisher
assert 'plVar13[4] = *(longlong *)(param_2 + 0x18)' in publisher and '*(longlong **)(param_2 + 0x18) = plVar13' in publisher
report=dict(checks=checks,owner='One fresh6C/noE0/noTraits selected terminal and actual273 pool;279 prepends actual words so reverse walk becomes reading order',
    scalars='Real words use unchecked integer prefix differences and node-span denominator; delimiter words use reset1/1/0/0',
    scope='No case/alias shaping, first source720 or empty-first24A inference; static PE only')
pathlib.Path(args.output).write_bytes((json.dumps(report,indent=2)+'\n').encode())
print('PASS pinned first TEXT outer guard/bit0/delta/root/interior/reset/279 order and boundary-override evidence')
