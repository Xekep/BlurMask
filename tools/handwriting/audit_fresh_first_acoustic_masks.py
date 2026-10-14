"""Fresh first scalar-policy and actual alphabet masks static owner proof."""
import argparse,hashlib,json,pathlib,re,struct,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--resource',required=True);p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
def asm(start,end):return '\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}' for i in cs.disasm(b.data[b.off(start):b.off(end)],start))
assert b.qwords(0x1802e7980+0x18,1)[0]==0x1802e7930 and b.qwords(0x1802e7930+8,1)[0]==0x1802e79d0
assert b.qwords(0x1802e79d0+0xa8,1)[0]==0
mask=f['1801b8850'];assert '+ 0xa8) != 0' in mask and 'FUN_18025ee50(param_4,1,uVar15)' in mask and 'FUN_18025ee50(param_5,1,uVar15)' in mask
assert '*(undefined8 *)(param_2 + 0x38) = 0' in f['1801503f0']
init=f['180150d00'];assert '*(undefined8 *)(param_2 + 0x38)' in init and '+ 0x30))' in init
setter=f['1801c0cf0'];assert 'if (param_3 == 0)' in setter and '*(undefined8 *)(param_2 + 0x2b8) = 0' in setter
assert b.qwords(0x1802de610+0x30,1)[0]==0x180151bb0 and b.qwords(0x1802e4180+0x30,1)[0]==0x1801c0cf0
assert '151bb0 mov qword ptr [rdx + 0x38], r8' in asm(0x180151bb0,0x180151bca)
#Registered cache constructor zeroes271; first-model loader permits second
#networks only.1B0760 tests FIRST network pointers in each active profile.
assert '*(undefined2 *)(param_2 + 0x270) = 1' in f['1801b0330'] #LE270=1/271=0.
netinit=f['1801b0760'];assert 'param_2 + 0x271' in netinit
for offset in ('0xc0','0xf0','0x120','0x150','0x180','0x1b0','0x1e0'):assert f'lVar15 + {offset} + lVar27' in netinit
pre=f['1801b8eb0'];assert "*(char *)(param_2 + 0x271) != '\\0'" in pre
source=f['180163970'];assert 'cVar6 = FUN_18016a130()' in source
secondary=f['18016a130'];assert 'param_2[0x28] = 0' in secondary and 'param_2[0x29] = 0' in secondary
assert b.qwords(0x1802df340+0xd8,1)[0]==0x1801691c0
assert 'cmp dword ptr [rax + 0x148], 0' in asm(0x1801691c0,0x1801691d2)
resample=f['18016aa00'];assert 'if (*(int *)(param_2 + 0x148) != 0)' in resample
cache=f['1801bb020'];assert '+ 0xf5)' in cache and '+ 0x148' not in cache and '+ 0xd8)' in cache
raw=pathlib.Path(a.resource).read_bytes();assert hashlib.sha256(raw).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<8H',raw,6219)==(179,140,65535,178,182,180,181,192)
report={'seed':'Bound first23F.A8 NULL independently of1a7 ->1B8850 unbound seed primaryones; secondary independentlyones',
 'restriction':'Fresh1503F0 clearsfrontend38;150D00 copies to21d.V30;1C0CF0 NULL clears2B8; owned noexternal/reused/setter input only',
 'working':'Firstvalidatedprofile uses second networks only;1B0760 FIRST-network pointers allNULL =>271false;1B8EB0 inactivecopies masks; v110148/150 bounds absent',
 'actualMask':'FirstAL header(179,140,FFFF,178,182,180,181,192). Virtuals140/178/179;140 reconstructed fromdashmembers,178/179 unconditionallyrestored ->both192all1, withownedwidepairlistempty',
 'penalty':'Freshsecondary16A130 clears140/148;ordinary raw ingress hasnoexternalpreexisting source refs;16AA00 only copiespenaltymap when148nonzero.218.D8=1691C0 false ->1BB020 noAL.F0/F8 penalties;freshcontainerF5zero partialscopeinactive',
 'scope':'Ownedfreshraw source only; no main240/external20E policy or Russian quality claim'}
out=pathlib.Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report)+'\n',encoding='utf-8');print(json.dumps(report))
