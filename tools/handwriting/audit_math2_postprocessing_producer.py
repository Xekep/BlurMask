"""Actual attn_seg output enables post-loop036F20/EF4; static only."""
import hashlib,json,struct,subprocess,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
out=Path(sys.argv[1]);child=out.with_suffix('.tensor-layout.json')
subprocess.run([sys.executable,str(root/'tools/handwriting/audit_math2_tensor_lifecycle.py'),str(child)],cwd=root,check=True)
layout=json.loads(child.read_bytes());assert 'attn_seg' in layout['models'][2]['io']['outputs']
b=Binary('MyScriptDocument.dll');assert hashlib.sha256(b.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
regions=[(6442751536, 6442754544, 'e65e9cf06b42789cb9f9f20f9f98a4d9dc8ef55397169de319cead75e92b1e77'), (6442733760, 6442734688, '155f71ec1b5fea2831e30c94fa71dfcd3d3c8753d41bfe22dd27df8b9eb51693'), (6442670576, 6442671184, '44a3ff6d114956fe746eb35b3d80f0e3867f6094fc8d8f0da18c353ef1ce6b48'), (6442676000, 6442677296, 'c4b2044a8fa4bcc00c9ede6610d678daa4f22e379032a38944c9181678559b85'), (6442681872, 6442689120, '180a1bcc3d913b2a19469e34a2b8a184e5a083354ef8f59414bed3d913e2716a')]
md=Cs(CS_ARCH_X86,CS_MODE_64);ins={}
for a,z,digest in regions:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==digest
 ins.update((i.address,(i.mnemonic,i.op_str))for i in md.disasm(raw,a))
pins=[(6442753336, 'mov', 'r13b, 1'), (6442754308, 'lea', 'rdx, [rip + 0x1d09dd]'), (6442754344, 'call', 'r10'), (6442754349, 'jne', '0x18004a140'), (6442754351, 'mov', 'dword ptr [rbx + 0x124], esi'), (6442754359, 'mov', 'byte ptr [rbx + 0x40], r13b'), (6442734199, 'movzx', 'r8d, byte ptr [rax + 0x40]'), (6442734469, 'mov', 'byte ptr [rdi + 0x98], r8b'), (6442670661, 'lea', 'rdi, [r14 + 0x10]'), (6442670810, 'movups', 'xmm0, xmmword ptr [rbp + 0x20]'), (6442670814, 'movups', 'xmmword ptr [rdi + 0x20], xmm0'), (6442687267, 'cmp', 'byte ptr [r13 + 0x38], 0'), (6442687272, 'je', '0x180039bf2'), (6442687463, 'call', '0x180036f20'), (6442676982, 'call', '0x180036c70'), (6442676995, 'mov', 'edx, 0xef4'), (6442677019, 'call', 'rax'), (6442677038, 'mov', 'rdx, qword ptr [rax]'), (6442677092, 'call', 'rdx')]
for a,m,o in pins:assert ins[a]==(m,o),(hex(a),ins.get(a))
o=b.off(0x18004a10b+0x1d09dd);assert b.data[o:o+9]==b'attn_seg\0'
assert b.qwords(0x1801fceb0+0x20,1)==(0x1801fcef8,)
assert b.qwords(0x1801fcef8,1)==(0x180042100,)
#Packet offset arithmetic is explicit;08-byte input flag not C0 model pointer.
assert 0x70+0x28==0x98 and 0x10+0x28==0x38
report=dict(scope='Static actual mode0 post-loop stage producer; no036F20 or EF4 runtime',regions=len(regions),instructionPins=len(pins),bindings=2,actualDecoderOutput='attn_seg',route='EF1.40 true -> EED.98 -> packet28 -> EEE.38 ->036F20 ->EF4.V0=042100',tensorDependency=dict(regions=len(layout['regions']),pins=layout['instructionPins']),remaining='Attention ancestry reconstruction, activation036C70, EF4 algorithms and final owned output')
out.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print('PASS actualattn_seg -> required post-loop stage;'+str(len(regions))+' regions/'+str(len(pins))+' pins/2bindings')
