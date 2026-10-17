"""Static native177280/175350 unsigned count and allocation ordering."""
import argparse,hashlib,json,pathlib,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
def block(a,z):return {i.address:(i.mnemonic,i.op_str) for i in md.disasm(b.data[b.off(a):b.off(z)],a)}
shear=block(0x180177320,0x1801773bc);gap=block(0x1801753f2,0x180175529);initializer=block(0x1801751f0,0x180175250)
expected={
0x180175224:('mov','byte ptr [rdx + 0x4c], 0'),
0x180177335:('cmp','dword ptr [rcx + 0x10], 0'),
0x18017733e:('dec','r9d'),0x180177341:('sub','eax, dword ptr [rcx + 0x34]'),0x180177344:('add','r9d, eax'),
0x180177372:('mov','eax, r9d'),0x180177378:('cvtsi2ss','xmm15, rax'),
0x18017540e:('dec','r9d'),0x180175419:('sub','r8d, ecx'),0x18017541c:('not','ecx'),0x18017541e:('add','r9d, r8d'),
0x180175421:('add','ecx, dword ptr [rdx + 0x38]'),0x180175424:('cmp','ecx, eax'),0x180175426:('jbe','0x18017542d'),
0x180175428:('dec','ebx'),0x18017542a:('add','ebx, r8d'),0x180175453:('mov','eax, ebx'),
0x180175458:('cvtsi2ss','xmm2, rax'),0x180175491:('test','r9d, r9d'),0x180175494:('jne','0x1801754a7'),
0x180175496:('mov','eax, dword ptr [rbp + 0x24]'),0x180175499:('mov','dword ptr [r13 + 0x9c], eax'),
0x1801754ab:('mov','edx, 4'),0x1801754cf:('mov','r8d, r9d'),0x1801754ea:('call','r9'),
0x1801754fe:('mov','edx, 4'),0x180175507:('mov','r8d, ebx'),0x180175513:('call','r9')}
for address,pin in expected.items():assert (shear|gap|initializer)[address]==pin,(hex(address),(shear|gap|initializer).get(address),pin)
report=dict(pins=[f'{k:x} {v[0]} {v[1]}' for k,v in expected.items()],
    semantics='UINT32 sum(end-first-1); unsigned qualified threshold; zero-extended UINT32->float; total-zero output before either allocation',
    allocation='item size4, totalCount then qualifiedCount; portable helper never allocates native-sized observation arrays',
    scope='Counts/initial blend only. Related-symbol shear weighting, physical shear and EM are not executed here. Empty firstNN provenance unproved.')
pathlib.Path(args.output).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS static{len(expected)} native UINT count, unsigned qualification, float conversion and zero-before-two-allocations pins')
