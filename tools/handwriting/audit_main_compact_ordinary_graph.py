"""Static PE only:19C4E0 ordinal-preserving conversion and19C310 selectors."""
import argparse,hashlib,json,pathlib,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
blocks=[(0x18019c310,0x18019c4e0,'4a8b4d3e37b69d2bf6ce7d164b33da145f2d5c882896ddc60b442832d0f902ad'),(0x18019c4e0,0x18019c910,'d7c85ff120bf323721ab0c4c463a4fd4492773d3021aa29ea0c7abdbdfa1839d'),(0x1801a61c0,0x1801a6980,'5601d88e59901a9351625bc133a29baf174528c99f056ac71e1e8e06a47d8a4a')]
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);ins={}
for a,z,h in blocks:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
 ins.update({v.address:(v.mnemonic,v.op_str) for v in md.disasm(raw,a)})
pins={0x18019c580:('cmp','r15d, dword ptr [r13 + 0x70]'),0x18019c58d:('imul','rdx, rcx, 0x38'),0x18019c598:('shl','rdi, 5'),
0x18019c5a0:('movzx','eax, word ptr [rdi]'),0x18019c5a3:('mov','word ptr [rdx], ax'),0x18019c5a6:('movzx','eax, word ptr [rdi + 2]'),0x18019c5aa:('mov','word ptr [rdx + 2], ax'),
0x18019c5ae:('movzx','eax, byte ptr [rdi + 0x50]'),0x18019c5b2:('mov','byte ptr [rdx + 6], al'),0x18019c5b5:('movzx','eax, byte ptr [rdi + 0x51]'),0x18019c5b9:('mov','byte ptr [rdx + 4], al'),0x18019c5bc:('movzx','eax, byte ptr [rdi + 0x52]'),0x18019c5c0:('mov','byte ptr [rdx + 5], al'),
0x18019c5ca:('movups','xmm0, xmmword ptr [rdi + 0x40]'),0x18019c5ce:('movups','xmmword ptr [rdx + 0x28], xmm0'),0x18019c5eb:('test','al, al'),0x18019c5ef:('movups','xmm0, xmmword ptr [rdi + 0x20]'),0x18019c5f3:('movups','xmmword ptr [rdx + 8], xmm0'),
0x18019c710:('call','0x18019c310'),0x18019c771:('call','0x18019c310'),0x18019c781:('inc','edi'),
0x18019c35a:('lea','r9d, [r12 + 1]'),0x18019c36b:('add','bp, word ptr [r8]'),0x18019c371:('add','r8, 0x10'),0x18019c375:('mov','word ptr [rcx], bp'),0x18019c385:('cmp','r10d, 0x800'),0x18019c38c:('jbe','0x18019c3b1'),
0x18019c4b0:('movzx','ecx, word ptr [rax]'),0x18019c4b3:('mov','word ptr [rbx], cx'),0x18019c4ba:('mov','rax, qword ptr [rax + 8]'),0x18019c4c5:('add','rdi, 0x10'),0x1801a695d:('call','0x18019c4e0')}
for a,pin in pins.items():assert ins[a]==pin,(hex(a),ins.get(a),pin)
fs=functions();convert=fs['18019c4e0'];assert 'uVar13 < (uint)param_2[0x1c]' in convert
assert 'uVar10 * 0x38 + local_68' in convert and 'uVar10 * 0x60 +' in convert
report=dict(blocks=[dict(start=hex(a),endExclusive=hex(z),sha256=h) for a,z,h in blocks],pins=[f'{a:x} {m} {o}' for a,(m,o) in pins.items()],
 proof='No post-prune group-ID remap:19C4E0 copies EVERY allocated group ordinal. Root selector conversion precedes node/arc traversal. Selectorcounts accumulateU16, data walks linkedlist order. Safe supported sum<=2048; no wrapped allocation/OOB reproduced.',
 scope='Kind0 graph only,cap1..8,semantic fields/owned ranges. SamplerV90 transition-cache production/global120/context227/NN are separate.')
pathlib.Path(args.output).write_text(json.dumps(report,indent=2)+'\n');print(f'PASS static{len(pins)} compact-layout/selector/order pins in3 exactPEblocks; no vendor execution')
