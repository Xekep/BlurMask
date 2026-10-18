"""Read-only01ED50 mean-X ordering and original record producer proof."""
from pathlib import Path
import hashlib,json,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary('MyScriptInk.dll')
assert hashlib.sha256(b.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
regions=[(6442577232, 6442577536, 'd7f992a1a08040df246235d20fd1473794be2da8e95eb17d7af0dce504dfe550'),
 (6442577648, 6442579120, 'd9aa998074991ed1727b5f1f0791d0f450b0b8a4e08017666cce02ea06b897a8')]
pins={'0x18001ed5f': ('sar', 'rax, 4'),
 '0x18001ed63': ('cmp', 'rax, 0x14'),
 '0x18001ed67': ('jge', '0x18001edcd'),
 '0x18001ed80': ('movups', 'xmm2, xmmword ptr [r8]'),
 '0x18001ed95': ('comiss', 'xmm0, xmm1'),
 '0x18001ed98': ('jbe', '0x18001edba'),
 '0x18001eda0': ('movups', 'xmm0, xmmword ptr [rax]'),
 '0x18001eda3': ('movups', 'xmmword ptr [rax + 0x10], xmm0'),
 '0x18001edb5': ('comiss', 'xmm0, xmm1'),
 '0x18001edb8': ('ja', '0x18001eda0'),
 '0x18001edbe': ('movups', 'xmmword ptr [rax + 0x10], xmm2'),
 '0x18001edda': ('sar', 'rax, 1'),
 '0x18001ede3': ('movups', 'xmm0, xmmword ptr [rcx + rax*8]'),
 '0x18001edec': ('movss', 'xmm2, dword ptr [rsp + 0x24]'),
 '0x18001edf2': ('comiss', 'xmm2, dword ptr [rbx + 4]'),
 '0x18001edf6': ('jbe', '0x18001ee0a'),
 '0x18001ee04': ('comiss', 'xmm2, dword ptr [rbx + 4]'),
 '0x18001ee08': ('ja', '0x18001ee00'),
 '0x18001ee0f': ('comiss', 'xmm0, xmm2'),
 '0x18001ee12': ('jbe', '0x18001ee2e'),
 '0x18001ee29': ('comiss', 'xmm0, xmm2'),
 '0x18001ee2c': ('ja', '0x18001ee20'),
 '0x18001ee33': ('movups', 'xmm1, xmmword ptr [rdx]'),
 '0x18001ee36': ('movups', 'xmm0, xmmword ptr [rbx]'),
 '0x18001ee39': ('movups', 'xmmword ptr [rbx], xmm1'),
 '0x18001ee40': ('movups', 'xmmword ptr [rdx], xmm0'),
 '0x18001ee51': ('call', '0x18001ed50'),
 '0x18001ee61': ('call', '0x18001ed50'),
 '0x18001f190': ('addss', 'xmm6, dword ptr [rcx - 8]'),
 '0x18001f195': ('addss', 'xmm6, dword ptr [rcx]'),
 '0x18001f199': ('addss', 'xmm6, dword ptr [rcx + 8]'),
 '0x18001f19e': ('addss', 'xmm6, dword ptr [rcx + 0x10]'),
 '0x18001f1c4': ('addss', 'xmm6, dword ptr [rcx]'),
 '0x18001f1d5': ('cvtsi2ss', 'xmm0, rsi'),
 '0x18001f1da': ('divss', 'xmm6, xmm0'),
 '0x18001f23e': ('mov', 'dword ptr [rcx], eax'),
 '0x18001f240': ('movss', 'dword ptr [rcx + 4], xmm6'),
 '0x18001f245': ('mov', 'qword ptr [rcx + 8], rbp')}
ins={}
for start,end,wanted in regions:
 raw=b.data[b.off(start):b.off(end)];assert hashlib.sha256(raw).hexdigest()==wanted
 ins.update({hex(i.address):(i.mnemonic,i.op_str)for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,start)})
for address,wanted in pins.items():assert ins[address]==tuple(wanted),(address,ins.get(address),wanted)
# This completed caller actually invokes the sorter on inclusive endpoints.
assert any(m=='call'and p=='0x18001ed50'for m,p in ins.values())
source=(root/'src/InkRecognition.Math/MathPreprocessor.cs').read_text(encoding='utf-8-sig')
assert 'LegacyMath2MeanXOrder.CopySourceOrder(means)'in source
assert source.index('LegacyMath2MeanXOrder.CopySourceOrder(means)')<source.index('GroupExtremaScale(ordered)')
report=dict(staticOnly=True,vendorExecution=False,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],pins=pins,
 scope='Actual original-point float32 sum/divide -> record OriginalIndex00/Mean04/sourceRef08 -> inclusive01ED50; partition threshold is last-first<20, so stable only for at most20 records. Full16-byte payload swaps preserve source identity.',
 boundary='New prepared sorter and source-order projection; no invented native pointer bytes, geometry label inference, C79 production or complete Math2 transport.')
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS2 complete native regions/{len(pins)} pins; original F32 means and native20/21 sorter branch; static only')
