"""Static PE only: fresh main24A profile, metadata property and dispatch."""
import argparse,hashlib,json,pathlib,struct,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();fs=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
def instructions(first,last):return {i.address:(i.mnemonic,i.op_str) for i in md.disasm(b.data[b.off(first):b.off(last)],first)}
assert b.qwords(0x180274f20,3)==(0x1012d0070,0x18000d020,0x18000d000)
assert b.qwords(0x1802df2a0,3)==(0x1012d0070,0x1801690c0,0x1801690d0)
getter=instructions(0x18000d020,0x18000d02a);assert getter[0x18000d020]==('movzx','eax, byte ptr [rdx + 0x10]')
setter=instructions(0x1801690d0,0x1801690dd);assert setter[0x1801690d4]==('mov','byte ptr [rdx + 0x80], al')
ctor=instructions(0x18000c860,0x18000ca00)
assert ctor[0x18000c939]==('mov','word ptr [rdi + 0x10], 0')
assert 'FUN_180150800(param_1,param_2,0x12d0070)' in fs['180150d00']
forward=fs['180150800'];assert '*(undefined8 *)(param_2 + 8),param_3' in forward and 'local_res20 + 0x10' in forward
source=fs['1801680d0'];assert '*(undefined1 *)(*(longlong *)(param_2 + 0x120) + 0x14) = *(undefined1 *)(param_2 + 0x80)' in source
state=instructions(0x18016a130,0x18016a250)
assert state[0x18016a22f]==('mov','byte ptr [rdx + 0x168], al')
assert ('xor','eax, eax') in state.values()
copy=instructions(0x180168260,0x180168280)
assert copy[0x180168271]==('movzx','eax, byte ptr [rdi + 0x80]')
assert copy[0x180168278]==('mov','byte ptr [rcx + 0x14], al')
frame=fs['18016e850']
for term in ('(*puVar9 & 0xf) == 0','(*puVar9 & 0xf) == 6','*(int *)(puVar9 + 8) < 0','puVar9 + 0x14','puVar9 + 0x12','FUN_18016e0c0','FUN_18016e570','param_3 + 0x168','0x80000000'):assert term in frame,term
assert "*(char *)(param_2 + 0x14) == '\\0'" in fs['180169750']
assert b.qwords(0x1802df0a0+0x88,1)[0]==0x180169750
raw=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(raw).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<IIIHH',raw,187976)==(0x101fe90,28,0x24a,0,0x102)
report=dict(metadataProperty='NEW2B3.byte10=0 ->12D0070/150800 ->218.byte80 ->secondary14 ->169750 disabled',
    constructorInstructions={hex(k):v for k,v in ctor.items() if '[rdi + 0x10]' in v[1]},
    source='main0 actual249/24B/24A order selects187976; no first6120 substitution',
    branch='unknown ordinary descriptor fits physical24..28 inclusive;assigned descriptors preserved',
    scope='NEW/no external property events/reused state; prepared unknown-descriptor controls are not firstNN empty-result evidence')
pathlib.Path(args.output).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print('PASS static fresh12D0070 metadata flow, native unknown-descriptor24A callback and actualmain profile; vendor DLL not executed')
