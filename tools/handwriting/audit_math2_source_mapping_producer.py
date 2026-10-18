"""Actual raw Math2 pipeline order, C79 empty-cache lifecycle and scalar coordinates."""
from pathlib import Path
import hashlib,json,struct,subprocess,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
from parse_math2_resource import parse
if len(sys.argv)!=2:raise SystemExit('report.json')
out=Path(sys.argv[1]).resolve()
subprocess.run([sys.executable,'-B',str(ROOT/'tools/handwriting/audit_math2_source_position_transport.py'),str(out.with_suffix('.transport.json'))],check=True)
resource=(ROOT/'models/math2/math2-unwrapped.res').read_bytes()
assert hashlib.sha256(resource).hexdigest()=='2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
pipeline=parse(resource)['pipeline']['input_processing_pipeline']
expected=[{'stroke_reorder_cogx':{}},{'scaler_group_extrema':{'quantil':.5}},{'normalizer':{}},{'resampler':{'has_interstrokes':False,'resampling':.4,'version':1}}]
assert pipeline['preprocessor_array']==expected
assert pipeline['features']=={'scheme':['symbols','math7','knn6Prox']}
b=Binary('MyScriptInk.dll')
REGIONS=[(6442526128, 6442526512, 'e90a75af25dc5eea1dd76dee9bc3d9c16ebca96b28018f780164663ac8e92eec'),
 (6442529888, 6442531664, '2be42a96d485a8b82470bfe219756db3dae77da1f524d8ee378cf9ad115fe451'),
 (6442531664, 6442531936, 'b9bc3ea1a04d104d72e8ceeb2a24243751e8ff334afa288f8a6376e01f5dbabd'),
 (6442531936, 6442532416, '3d37403f49f9d5b6be1ab2e11370be489013eee2ce7b7b38ab9f08961d1fdd18'),
 (6442535904, 6442536832, 'e8c6bb53bdd4c79190c67f373fabeb88d25675716fa15febdeaa76d0ee9098cb'),
 (6442512688, 6442514400, '4d27644e87e3b7005b94adddcd263324f31294690881553a47f418b86ae13750'),
 (6442515248, 6442517056, '961cf2e64c013635b25f5633cf61aff207727361c93bcd953544dbe0e3167ada'),
 (6442517056, 6442518128, 'a7b34c448509b5951d3d94834905992b067693a76efdc2473140f3775639f1e5'),
 (6442538272, 6442539856, '0867b5f3886f5728b3ec6406c7c9530fcf98996c25dbf66ae5d476e214e0fc61'),
 (6442568048, 6442568672, '70967f647d919437052eea0a819673fe1fb18c2228cd3e9d2852d37c3610e632'),
 (6442577232, 6442577536, 'd7f992a1a08040df246235d20fd1473794be2da8e95eb17d7af0dce504dfe550'),
 (6442577648, 6442579120, 'd9aa998074991ed1727b5f1f0791d0f450b0b8a4e08017666cce02ea06b897a8')]
PINS=[(6442530871, 'xor', 'r12d, r12d'),
 (6442530922, 'mov', 'rax, qword ptr [rcx + r12*8]'),
 (6442531169, 'call', 'r15'),
 (6442531288, 'inc', 'r12d'),
 (6442531685, 'mov', 'rdi, r9'),
 (6442531694, 'call', '0x1800125b0'),
 (6442531790, 'mov', 'eax, dword ptr [rdi + 8]'),
 (6442531872, 'lea', 'r8, [rax + rcx*8]'),
 (6442531894, 'mov', 'qword ptr [r8], r14'),
 (6442531897, 'inc', 'dword ptr [rbx]'),
 (6442536519, 'xor', 'ebp, ebp'),
 (6442536537, 'mov', 'rax, qword ptr [rdx + 0xb8]'),
 (6442536544, 'mov', 'r9, rdi'),
 (6442536558, 'mov', 'rdx, qword ptr [rax + rbp*8]'),
 (6442536565, 'mov', 'r10, qword ptr [rax + 0x10]'),
 (6442536569, 'call', 'r10'),
 (6442536583, 'inc', 'ebp'),
 (6442578320, 'addss', 'xmm6, dword ptr [rcx - 8]'),
 (6442578325, 'addss', 'xmm6, dword ptr [rcx]'),
 (6442578329, 'addss', 'xmm6, dword ptr [rcx + 8]'),
 (6442578334, 'addss', 'xmm6, dword ptr [rcx + 0x10]'),
 (6442578372, 'addss', 'xmm6, dword ptr [rcx]'),
 (6442578389, 'cvtsi2ss', 'xmm0, rsi'),
 (6442578394, 'divss', 'xmm6, xmm0'),
 (6442578496, 'movss', 'dword ptr [rcx + 4], xmm6'),
 (6442577251, 'cmp', 'rax, 0x14'),
 (6442577255, 'jge', '0x18001edcd'),
 (6442512811, 'mov', 'qword ptr [rsi + 0x98], r14'),
 (6442512822, 'mov', 'qword ptr [rsi + 0xa0], r14'),
 (6442512832, 'mov', 'dword ptr [rsi + 0xa8], r14d'),
 (6442516324, 'subss', 'xmm6, xmm1'),
 (6442516328, 'mulss', 'xmm6, xmm0'),
 (6442516332, 'mulss', 'xmm6, xmm7'),
 (6442516336, 'addss', 'xmm6, xmm1'),
 (6442516343, 'call', '0x1800cfcd0'),
 (6442516348, 'cvttss2si', 'eax, xmm0'),
 (6442516366, 'subss', 'xmm6, xmm0'),
 (6442516370, 'addss', 'xmm6, xmm9'),
 (6442516375, 'mulss', 'xmm6, xmm10'),
 (6442516380, 'cvttss2si', 'eax, xmm6'),
 (6442536152, 'cmp', 'byte ptr [rsp + 0x98], 0'),
 (6442536165, 'je', '0x180014da9'),
 (6442536384, 'mov', 'rax, qword ptr [rdi + 0x40]'),
 (6442536388, 'mov', 'byte ptr [rdx + rax], 0'),
 (6442536691, 'cmp', 'byte ptr [rsp + 0x90], 0'),
 (6442536703, 'jne', '0x180014f3c')]
ins={}
for a,z,h in REGIONS:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
 ins.update({i.address:(i.mnemonic,i.op_str)for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,a)})
for a,m,o in PINS:assert ins[a]==(m,o),(hex(a),ins.get(a),m,o)
d=Binary('MyScriptDocument.dll')
docIns={i.address:(i.mnemonic,i.op_str)for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(d.data[d.off(0x180047c50):d.off(0x1800485b0)],0x180047c50)}
docPins=[(0x180047ef1,'xor','esi, esi'),(0x180047f33,'mov','byte ptr [rsp + 0x28], sil'),(0x180047f38,'mov','byte ptr [rsp + 0x20], 1')]
for a,m,o in docPins:assert docIns[a]==(m,o),(hex(a),docIns[a])
# Actual name registry -> type -> constructor primary -> callback.
processors=[('stroke_reorder_cogx',0xc7f,0x1800df7c0,0x1800df780,0x1800df740,0x18001eef0),('scaler_group_extrema',0xc71,0x1800df4f0,0x1800df490,0x1800df450,0x18001c970),('normalizer',0xc73,0x1800dee60,0x1800dee20,0x1800dede0,0x180015520),('resampler',0xc72,0x1800df030,0x1800defe0,0x1800defa0,0x180017060)]
registry={}
for index in range(11):
 ptr,kind=struct.unpack_from('<QI',b.data,b.off(0x18010ec50+index*16));start=b.off(ptr);text=b.data[start:b.data.index(b'\0',start)].decode();registry[text]=kind
for name,kind,descriptor,ctor,primary,process in processors:
 assert registry[name]==kind
 assert struct.unpack_from('<I',b.data,b.off(descriptor+8))[0]==kind
 assert b.qwords(descriptor+0x18,1)==(ctor,)
 assert b.qwords(ctor+8,1)==(primary,)
 assert b.qwords(primary+0x10,1)==(process,)
# The disassembled V18 constants, not source-language double approximations.
constants={0x18000fc79:.005,0x18000fc9c:.0025,0x18000fcab:200.}
for a,value in constants.items():
 op=next(i for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(b.data[b.off(a):b.off(a)+12],a))
 # RIP displacement is encoded in the final four bytes of each 8/9-byte load.
 displacement=struct.unpack_from('<i',op.bytes,len(op.bytes)-4)[0];target=op.address+op.size+displacement
 assert struct.unpack_from('<f',b.data,b.off(target))[0]==struct.unpack('<f',struct.pack('<f',value))[0]
controls=out.with_suffix('.controls.json')
subprocess.run([sys.executable,'-B',str(ROOT/'tools/LegacyMathSourcePositions.Probe/reference_positions.py'),str(controls)],check=True)
r=json.loads(controls.read_bytes())
report=dict(staticOnly=True,vendorExecution=False,resourceSha256=hashlib.sha256(resource).hexdigest(),pipeline=pipeline,
 regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in REGIONS],instructionPins=len(PINS)+len(docPins),processorBindings=processors,registryEntries=len(registry),
 emptyCache='Fresh00F130 clears98/A0/A8; raw V18/V20 preserve cache; scaler/normalizer ignore C79; C6Dparam5true skips V58 import; no external/C25 maps accepted.',
 controlCounts=dict(composition=len(r['scalar']),rounding=len(r['rounding']),snapshots=len(r['snapshots']),**r['discriminators']),
 nextScope='Source-aware producer bound<=20 original strokes before NN; native>=21 source-sort ties differ from old stable host adaptation and remain separately refused pending literal sorter.')
out.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS{len(REGIONS)} regions/{len(PINS)+len(docPins)}pins/4 processor chains/11 registry entries; exact actual order, raw empty-cache lifecycle, prepared coordinate oracle')
