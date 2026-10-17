"""Static EEE -> EEF -> EF1 score-channel ownership; no native execution."""
from pathlib import Path
import hashlib,json,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
digests={'MyScriptDocument.dll': '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f', 'MyScriptEngine.dll': 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'}
regions = [
    ('MyScriptDocument.dll', 0x1800450c0, 0x180045460, '155f71ec1b5fea2831e30c94fa71dfcd3d3c8753d41bfe22dd27df8b9eb51693'),
    ('MyScriptDocument.dll', 0x1800359f0, 0x180035c50, '44a3ff6d114956fe746eb35b3d80f0e3867f6094fc8d8f0da18c353ef1ce6b48'),
    ('MyScriptDocument.dll', 0x18003b300, 0x18003b540, '78a8116a27d4cf02342618d1da2ec2af0f4ed7e1c751acf897691cc3aea4865a'),
    ('MyScriptDocument.dll', 0x18003cac0, 0x18003d1e0, 'f51653c489d94baa7dbc4fdca884f33431c585e07ccdd8d41c717bcadcc2091d'),
    ('MyScriptDocument.dll', 0x18003c0d0, 0x18003c390, '82e9af90d2e5280e70f0ad15a6c83138ca7c333dcf6836cfb1e8b720b9d8c056'),
    ('MyScriptEngine.dll', 0x180186290, 0x180186650, 'c585efc12fd22521aad54991102319890e5d9de0e6bd1ad472de8c341d4d580e'),
]
pins = [
    ('MyScriptDocument.dll', 0x18004523b, 'mov', 'edx, 0xef1'),
    ('MyScriptDocument.dll', 0x1800453a3, 'mov', 'qword ptr [rdi + 0x168], rcx'),
    ('MyScriptDocument.dll', 0x1800453d3, 'mov', 'edx, 0xeee'),
    ('MyScriptDocument.dll', 0x1800453f7, 'mov', 'r9d, 0x108'),
    ('MyScriptDocument.dll', 0x180035aad, 'lea', 'r13, [r14 + 0x80]'),
    ('MyScriptDocument.dll', 0x180035ab8, 'mov', 'edx, 0xeef'),
    ('MyScriptDocument.dll', 0x180035b1e, 'lea', 'r8, [rdi + 0x50]'),
    ('MyScriptDocument.dll', 0x180035b22, 'mov', 'r9d, 0xb8'),
    ('MyScriptDocument.dll', 0x180035b2b, 'call', 'rsi'),
    ('MyScriptDocument.dll', 0x180035b33, 'call', 'rbp'),
    ('MyScriptDocument.dll', 0x180035b35, 'mov', 'qword ptr [r13], rax'),
    ('MyScriptDocument.dll', 0x180035b91, 'lea', 'r13, [r14 + 0x80]'),
    ('MyScriptDocument.dll', 0x18003b382, 'mov', 'qword ptr [rbx + 0xb8], rax'),
    ('MyScriptDocument.dll', 0x18003b451, 'mov', 'rdx, qword ptr [rsi + 0xa8]'),
    ('MyScriptDocument.dll', 0x18003b45c, 'mov', 'rax, qword ptr [rax + 0x18]'),
    ('MyScriptDocument.dll', 0x18003b460, 'mov', 'r8, qword ptr [rax + 0x20]'),
    ('MyScriptDocument.dll', 0x18003b464, 'call', 'r8'),
    ('MyScriptDocument.dll', 0x18003b46c, 'mov', 'qword ptr [rbx + 0xb8], rax'),
    ('MyScriptDocument.dll', 0x18003cb05, 'mov', 'rdx, qword ptr [rdx + 0xb8]'),
    ('MyScriptDocument.dll', 0x18003cb5c, 'mov', 'r10, qword ptr [rax + 0x28]'),
    ('MyScriptDocument.dll', 0x18003cd99, 'call', '0x18003c0d0'),
    ('MyScriptDocument.dll', 0x18003ce50, 'mov', 'rax, qword ptr [rdi + 0xe8]'),
    ('MyScriptDocument.dll', 0x18003ce6c, 'mov', 'rax, qword ptr [rdi + 0xe8]'),
    ('MyScriptDocument.dll', 0x18003ce84, 'mov', 'rax, qword ptr [rdi + 0xe8]'),
    ('MyScriptDocument.dll', 0x18003ce9d, 'mov', 'rax, qword ptr [rdi + 0xe8]'),
    ('MyScriptDocument.dll', 0x18003cee2, 'mov', 'rax, qword ptr [rdi + 0xe8]'),
    ('MyScriptDocument.dll', 0x18003d1ba, 'mov', 'rax, qword ptr [rdi + 0xd8]'),
    ('MyScriptDocument.dll', 0x18003d1ce, 'mov', 'rax, qword ptr [rdi + 0xe8]'),
    ('MyScriptDocument.dll', 0x18003c104, 'movss', 'xmm6, dword ptr [rip + 0x1edb60]'),
    ('MyScriptDocument.dll', 0x18003c10f, 'divss', 'xmm6, dword ptr [rdx + 0x20]'),
    ('MyScriptDocument.dll', 0x18003c118, 'mov', 'edx, 0x54'),
    ('MyScriptDocument.dll', 0x18003c13b, 'mov', 'r10, qword ptr [rax + 0x48]'),
    ('MyScriptDocument.dll', 0x18003c13f, 'movss', 'dword ptr [rsp + 0x30], xmm6'),
    ('MyScriptDocument.dll', 0x18003c145, 'mov', 'dword ptr [rsp + 0x28], 0xffffffff'),
    ('MyScriptDocument.dll', 0x18003c340, 'mov', 'eax, dword ptr [rax + rdx*4]'),
    ('MyScriptDocument.dll', 0x18003c343, 'mov', 'dword ptr [rcx + rdx*4], eax'),
    ('MyScriptDocument.dll', 0x18003c34d, 'movss', 'xmm0, dword ptr [rax + rdx*4]'),
    ('MyScriptDocument.dll', 0x18003c352, 'xorps', 'xmm0, xmm1'),
    ('MyScriptDocument.dll', 0x18003c355, 'movss', 'dword ptr [rax + rdx*4], xmm0'),
    ('MyScriptEngine.dll', 0x180186430, 'movss', 'xmm0, dword ptr [rbx + rdi*4]'),
    ('MyScriptEngine.dll', 0x180186435, 'subss', 'xmm0, xmm6'),
    ('MyScriptEngine.dll', 0x180186439, 'mulss', 'xmm0, xmm7'),
    ('MyScriptEngine.dll', 0x18018643d, 'call', '0x1801c45d0'),
    ('MyScriptEngine.dll', 0x180186445, 'addss', 'xmm8, xmm0'),
    ('MyScriptEngine.dll', 0x18018644f, 'movaps', 'xmm0, xmm8'),
    ('MyScriptEngine.dll', 0x180186453, 'call', '0x1801c41d0'),
    ('MyScriptEngine.dll', 0x18018645b, 'mulss', 'xmm6, xmm7'),
    ('MyScriptEngine.dll', 0x180186462, 'addss', 'xmm2, xmm6'),
    ('MyScriptEngine.dll', 0x18018655c, 'subss', 'xmm0, xmm2'),
    ('MyScriptEngine.dll', 0x18018656d, 'subss', 'xmm1, xmm2'),
    ('MyScriptEngine.dll', 0x18018657e, 'subss', 'xmm0, xmm2'),
    ('MyScriptEngine.dll', 0x18018658d, 'subss', 'xmm1, xmm2'),
    ('MyScriptEngine.dll', 0x1801865ba, 'subss', 'xmm0, xmm2'),
    ('MyScriptDocument.dll', 0x180045260, 'mov', 'r9d, 0x40'),
    ('MyScriptDocument.dll', 0x180045269, 'call', 'rbx'),
    ('MyScriptDocument.dll', 0x18004526b, 'mov', 'rcx, rax'),
    ('MyScriptDocument.dll', 0x1800452af, 'jmp', '0x180045328'),
    ('MyScriptDocument.dll', 0x1800453f3, 'lea', 'r8, [rdi + 0x70]'),
    ('MyScriptDocument.dll', 0x180045402, 'mov', 'qword ptr [rdi + 0x220], rax'),
]
bindings = [
    ('MyScriptDocument.dll', 0x1801ed9b8, 0x1801ed960),
    ('MyScriptDocument.dll', 0x1801ed968, 0x1801ed930),
    ('MyScriptDocument.dll', 0x1801edad8, 0x1801eda68),
    ('MyScriptDocument.dll', 0x1801eda70, 0x1801eda20),
    ('MyScriptDocument.dll', 0x1801eda78, 0x1801edaa0),
    ('MyScriptDocument.dll', 0x1801edab0, 0x18003b300),
    ('MyScriptDocument.dll', 0x1801eda50, 0x18003cac0),
    ('MyScriptDocument.dll', 0x1801fd178, 0x18004b9e0),
    ('MyScriptEngine.dll', 0x18022a2e0, 0x18022a200),
    ('MyScriptEngine.dll', 0x18022a248, 0x180186290),
]
binaries={name:Binary(name) for name in digests}
for name,binary in binaries.items():assert hashlib.sha256(binary.data).hexdigest()==digests[name]
md=Cs(CS_ARCH_X86,CS_MODE_64);instructions={}
for name,lo,hi,digest in regions:
 b=binaries[name];raw=b.data[b.off(lo):b.off(hi)];assert hashlib.sha256(raw).hexdigest()==digest
 instructions.update({(name,i.address):(i.mnemonic,i.op_str) for i in md.disasm(raw,lo)})
for name,address,mnemonic,operands in pins:assert instructions[name,address]==(mnemonic,operands),(name,hex(address))
for name,address,target in bindings:assert binaries[name].qwords(address,1)[0]==target
#Scalar literal used by DIVSS; sign mask is checked separately by full body hash.
import struct
assert struct.unpack_from('<I',binaries['MyScriptDocument.dll'].data,binaries['MyScriptDocument.dll'].off(0x180229c6c))[0]==0x3f800000
report=dict(regions=len(regions),pins=len(pins),bindings=len(bindings),
 route='EED168 EF1 -> EEE packet70 -> EEF packet+50/decoder+A8 -> EEF B8 clone; EEE80 is EEF, not EF1',
 channels='D8 retains log probabilities; E8 negates sign bits then adds per-beam F0 cost before heap selection',
 numeric='Engine54.V48 at186290 uses last time frame, batch axis1, vocabulary axis2 and float32 temperature/exp/sum/log/store operations',
 remains=['EEF full tensor and alias lifecycle','native exp/log CPU-dispatch implementations','constraint model and token-mask producers','whole beam and final structural acceptance'],
 noVendorExecution=True,runtimeChange=False)
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS{len(regions)} score-owner regions/{len(pins)} pins/{len(bindings)} bindings; static EEE/EEF/EF1 and float32 channel proof only')
