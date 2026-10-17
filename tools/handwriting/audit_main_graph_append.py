"""Static ordinary19D470/19C150/19EB10 and sticky-ref200 audit. No DLL load."""
import argparse,hashlib,json,pathlib,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
ranges=[(0x18019d490,0x18019d4bc),(0x18019d510,0x18019d536),(0x18019d5fc,0x18019d610),
 (0x18019c20e,0x18019c267),(0x18019da15,0x18019da4a),
 (0x18019eb2e,0x18019eb5f),(0x18019ebd0,0x18019ec62),
 (0x1801a18b3,0x1801a18d0),(0x1801a1919,0x1801a193f),
 (0x1801a38ec,0x1801a38fc),(0x1801a6105,0x1801a6112),
 (0x18019c291,0x18019c2a4),(0x18019c2f7,0x18019c303)]
instructions={};blocks=[]
for a,z in ranges:
 raw=b.data[b.off(a):b.off(z)];ins=list(md.disasm(raw,a));assert ins[-1].address+ins[-1].size==z
 instructions.update({i.address:(i.mnemonic,i.op_str) for i in ins});blocks.append(dict(start=hex(a),endExclusive=hex(z),sha256=hashlib.sha256(raw).hexdigest()))
pins={
0x18019d490:('mov','esi, dword ptr [rdx + 0x70]'),0x18019d493:('and','eax, 0xffffff'),
0x18019d498:('mov','r14d, dword ptr [rdx + 0x80]'),0x18019d4a2:('shl','eax, 7'),
0x18019d525:('cmp','byte ptr [rcx + rax + 6], r13b'),0x18019d52a:('jbe','0x18019d532'),
0x18019d52c:('mov','dword ptr [r9 + r8*4], ebx'),0x18019d530:('inc','ebx'),
0x18019d5fc:('movzx','eax, word ptr [rcx + rsi + 4]'),0x18019d601:('cmp','ax, -1'),
0x18019d607:('add','ax, r14w'),0x18019d60b:('mov','word ptr [rcx + rsi + 4], ax'),
0x18019c212:('mov','byte ptr [rdi + 5], cl'),0x18019c219:('mov','dword ptr [rdi], ecx'),
0x18019c21f:('cmp','cx, -1'),0x18019c225:('mov','edx, 0xffffffff'),0x18019c233:('add','edx, ecx'),
0x18019c235:('mov','word ptr [rdi + 0x18], dx'),0x18019c240:('add','ecx, edx'),
0x18019c242:('mov','dword ptr [rdi + 0x10], ecx'),0x18019c245:('add','edx, dword ptr [rsi + 0x14]'),
0x18019c248:('mov','dword ptr [rdi + 0x14], edx'),0x18019c24f:('mov','byte ptr [rdi + 6], cl'),
0x18019c256:('mov','word ptr [rdi + 0x1a], cx'),0x18019c25e:('mov','byte ptr [rdi + 4], cl'),
0x18019c264:('mov','dword ptr [rdi + 0x1c], ecx'),
0x18019da1a:('cmp','dword ptr [rdi + 8], 1'),0x18019da27:('cmp','word ptr [r9], r8w'),0x18019da43:('call','0x18019d270'),
0x18019eb2e:('cmp','byte ptr [r9 + 1], 0'),0x18019eb41:('cmp','byte ptr [r9], 0'),
0x18019eb47:('cmp','byte ptr [r9 + 2], 0'),0x18019eb4e:('cmp','byte ptr [r9 + 3], 0'),
0x18019eb55:('mov','eax, 0xffffffff'),0x18019ebde:('movups','xmmword ptr [rdx], xmm0'),
0x18019ebe1:('movups','xmmword ptr [rdx + 0x10], xmm0'),0x18019ec05:('mov','byte ptr [rcx + rdx], al'),
0x18019ec0c:('mov','byte ptr [rcx + rdx + 1], al'),0x18019ec14:('mov','byte ptr [rcx + rdx + 2], al'),
0x18019ec1c:('mov','byte ptr [rcx + rdx + 3], al'),0x18019ec23:('mov','dword ptr [rcx + rdx + 8], eax'),
0x18019ec2b:('mov','byte ptr [rcx + rdx + 4], al'),0x18019ec33:('mov','byte ptr [rcx + rdx + 5], al'),
0x18019ec52:('call','0x180070e00'),
0x1801a18bb:('movzx','r13d, word ptr [r12 + rax + 4]'),0x1801a1919:('or','byte ptr [rbx + 6], 4'),
0x1801a191d:('mov','word ptr [rbx + 0x18], r13w'),0x1801a1922:('mov','word ptr [rbx + 4], r8w'),
0x1801a1934:('cmp','al, 0xc8'),0x1801a1936:('jae','0x1801a193f'),
0x1801a38f1:('cmp','cl, 0xc8'),0x1801a38f4:('jae','0x1801a38fc'),0x1801a38f6:('inc','cl'),
0x1801a6109:('cmp','al, 0xc8'),0x1801a610b:('jae','0x1801a6112'),0x1801a610d:('dec','al'),
0x18019c29d:('movups','xmmword ptr [rbx], xmm0'),0x18019c2a0:('movups','xmmword ptr [rbx + 0x10], xmm0'),
0x18019c2f7:('or','byte ptr [rbx + 6], 4'),0x18019c2fd:('mov','word ptr [rbx + 0x18], 0xffff')}
for address,pin in pins.items():assert instructions[address]==pin,(hex(address),instructions.get(address),pin)
report=dict(pins=[f'{a:x} {m} {o}' for a,(m,o) in pins.items()],blocks=blocks,
 semantics='Live-byte remap;oldNodes low24<<7 endpoint offset;FFFF auxiliary preservation;sole-root list removal/stitch;19EB10 flag gate/zeroed prefix/ordered controls;sticky200 increment/decrement',
 actualScope='One whole-physical atom per ordinary descriptor only.19C280 root and1A1850 zero-span fields are concrete; larger local allocation-time metadata is not inferred.',
 excluded='Cross-source1A3B30,19E8B0 boundary pruning, token groups, classifier-facing globalGraph and recognition quality.')
pathlib.Path(args.output).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS static{len(pins)} append/remap/pending/root/sticky200 instruction pins in{len(blocks)} exact PE blocks')
