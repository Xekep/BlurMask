"""Pinned first1F42 interior1A0 request and assigned ordinary1AAE70 policy."""
import argparse,hashlib,json,pathlib,struct,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();fs=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
ins=list(md.disasm(b.data[b.off(0x1801f59b6):b.off(0x1801f5a0a)],0x1801f59b6));at={i.address:(i.mnemonic,i.op_str) for i in ins}
assert at[0x1801f59b6]==('xor','eax, eax')
assert at[0x1801f59b8]==('mov','qword ptr [rbp + 0x90], rax')
assert at[0x1801f59bf]==('mov','dword ptr [rbp + 0x98], eax')
assert at[0x1801f59e5]==('mov','qword ptr [rbp + 0x88], 1')
assert at[0x1801f59f0]==('mov','dword ptr [rbp], 0x100')
assert at[0x1801f5a00]==('mov','r10, qword ptr [rax + 0x1a0]')
assert at[0x1801f5a07]==('call','r10')
assert b.qwords(0x1802de610+0x1a0,1)[0]==0x180152da0
assert b.qwords(0x1802e1ff0+0x130,1)[0]==0x1801aae70
s=fs['1801aae70'];assert '*(int *)(puVar16 + 8) == -1' in s and '*(int *)(puVar17 + 8) == -1' in s
assert "*(char *)(param_2 + 9) == '\\0'" in s
assert 'param_3[0xb] == 1' in s and 'param_3[0xd] == 10' in s and 'param_3[0xe] == 10' in s
assert 'puVar28[4] == 0' in s and 'local_a8 = iVar13 + 1' in s
assert hashlib.sha256(b.data[b.off(0x1801aae70):b.off(0x1801ac0d5)]).hexdigest()=='1e05e24372dfe33d1aca0d83a0e18c97615f464d052fb32368fc58db796b6228'
matrix=list(struct.unpack_from('<49i',b.data,b.off(0x1802e1f20)))
assert matrix==[0,1,4,2,4,1,0,2,2,2,2,2,0,0,1,1,2,1,2,0,0,3,1,2,2,2,0,0,1,1,2,1,2,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0]
assert struct.unpack_from('<f',b.data,b.off(0x1802e1ea0))[0]==struct.unpack('<f',struct.pack('<f',2.95))[0]
frame=fs['18016e850'];assert '+ 0x10' in frame and '0x80' in frame and 'FUN_18016e570' in frame
init=fs['1801a6980'];assert '*(char *)(param_2 + 0x1d9) = param_3[1]' in init
frontend=fs['180150d00'];assert '(*(byte *)(local_b8 + 1) >> 4' in frontend
assert '*(undefined2 *)(param_2 + 8) = 0x10' in fs['18000c860']
#local_b8 is a longlong*, so this is container+8, NOT byte+1; its bit4
#cannot be claimed clear. Assigned descriptor line0 excludes that branch.
raw=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(raw).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<I',raw,12727+8)==(0x21b,) and raw[12727+17]==1 and raw[12727+350]==0
assert struct.unpack_from('<H',raw,11506+14)==(0x106,)
report=dict(caller=[f'{i.address:x} {i.mnemonic} {i.op_str}' for i in ins],
    request='First1F42 interior: kind28=1;policy2C=0;gap30=0;special34=0;special38=0;scratch256;neighbor nodes=-1',
    ownership='Actual first assigned24A descriptor line0; graph9=1; ordinary no metadata; sampler106 no custom VE0',
    containerBit='container+8 bit4 starts1; no false1D9 assumption: both-unassigned conjunction is false because actual line0',
    scope='Prepared pairs of genuine first graph arcs; selected word traversal/publication remains separate; no vendor execution')
pathlib.Path(args.output).write_bytes((json.dumps(report,indent=2)+'\n').encode())
print('PASS pinned first interior request scalars,1A0/21B130 bindings,49-category rules and assigned-line scope; static PE only')
