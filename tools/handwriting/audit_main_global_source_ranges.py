"""Static-only compact ordinary DFS ownership proof; old metadata audit separate."""
import argparse,hashlib,json,pathlib,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a,z=0x1801a8e60,0x1801aa470;raw=b.data[b.off(a):b.off(z)];h=hashlib.sha256(raw).hexdigest();assert h=='c352079ae0a5fd1333eccb769881aac3b6acd9df7d3832424c9b0fa8020e964f'
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);ins={i.address:(i.mnemonic,i.op_str) for i in md.disasm(raw,a)}
pins={0x1801aa07d:('cmp','byte ptr [r8 + 4], dil'),0x1801aa081:('je','0x1801aa098'),0x1801aa098:('cmp','dword ptr [r8 + 0x10], r13d'),0x1801aa0b9:('call','0x1801a9070'),0x1801aa0d7:('inc','ebx'),
0x1801a9451:('movzx','ecx, word ptr [rbx]'),0x1801a9454:('mov','rax, qword ptr [rbx + 0x18]'),0x1801a9458:('movzx','r12d, word ptr [rbx + 2]'),0x1801a9466:('sub','r12d, ecx'),0x1801a946f:('lea','rcx, [r8 + 1]'),
0x1801a948a:('movzx','r8d, word ptr [rbp]'),0x1801a94a7:('call','0x1801a9070'),0x1801a94cb:('add','rbp, 2')}
for address,pin in pins.items():assert ins[address]==pin,(hex(address),ins.get(address),pin)
bindings={0x1802e1ff0+0x120:0x1801a9ae0,0x1802df340+0x90:0x180168d00,0x1802df340+0xa0:0x180168e10,0x1802df340+0xc8:0x180168f80}
for slot,target in bindings.items():assert b.qwords(slot,1)[0]==target
report=dict(start=hex(a),endExclusive=hex(z),sha256=h,pins=[f'{address:x} {m} {o}' for address,(m,o) in pins.items()],bindings={hex(k):hex(v)for k,v in bindings.items()},scope='Fresh ordinary kind0 only,span0 start arcs,compactselector1 outgoing list,shared original/prepared source metadata. No reverse/protocol/global227 or NN.')
pathlib.Path(args.output).write_text(json.dumps(report,indent=2)+'\n');print(f'PASS static{len(pins)} global compact DFS pins/4bindings in1 exactPEblock; no vendor execution')
