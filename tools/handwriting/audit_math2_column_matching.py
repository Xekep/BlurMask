"""Whole-body static matching proof; PE reads only, no vendor execution."""
from pathlib import Path
import hashlib,json,struct,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary('MyScriptDocument.dll')
assert hashlib.sha256(b.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
regions=[(6442713712, 6442714080, 'cbd1a12fca04c3157a4610f9f3b9567f1de7de177b7b7c0acf436954c3324313'), (6442714080, 6442714736, '06255038083ee7571df8155256b62f6e9e6d2d2e521acc598c8d8a042981286e'), (6442714736, 6442715632, '2073d7c5564f42e1f9e2e3398c6956384fce5c7e0f0910f4369d8b5d6f7838b8'), (6442715632, 6442716368, 'cfc76889e45a94ca5ad8eaa013f3be32e400b1b734598fb6bf59a3bfab1dc7c5')]
pins=[(6442713910, 'movss', 'xmm3, dword ptr [rip + 0x1e9a4a]'), (6442713934, 'mov', 'r9d, 0xffffffff'), (6442713977, 'comiss', 'xmm0, xmm1'), (6442713980, 'jbe', '0x180040381'), (6442713991, 'cmovbe', 'eax, r9d'), (6442714012, 'mov', 'dword ptr [rax + r11*4], r9d'), (6442714020, 'movss', 'dword ptr [rax + r11*4], xmm1'), (6442714174, 'call', '0x180040270'), (6442714404, 'cmp', 'ebx, -1'), (6442714448, 'cmp', 'dword ptr [rdx], ebx'), (6442714452, 'mov', 'dword ptr [rdx], 0xffffffff'), (6442714472, 'comiss', 'xmm0, xmm1'), (6442714475, 'jbe', '0x180040573'), (6442714477, 'mov', 'ebp, r8d'), (6442714859, 'mov', 'dword ptr [rax + rcx*4], 0xff7fffff'), (6442715002, 'mov', 'dword ptr [rax + rcx*4], 0xff7fffff'), (6442715115, 'mov', 'dword ptr [rax + rcx*4], 0xff7fffff'), (6442715258, 'mov', 'dword ptr [rax + rcx*4], 0xff7fffff'), (6442715390, 'mov', 'dword ptr [rax + rcx*4], 0x7f7fffff'), (6442715586, 'mov', 'dword ptr [rax + rcx*4], 0x7f7fffff'), (6442715654, 'cmp', 'dword ptr [r9 + 8], 0'), (6442715668, 'je', '0x180040cba'), (6442715678, 'mov', 'r15, qword ptr [r8 + 0x10]'), (6442715685, 'je', '0x180040cb5'), (6442715695, 'mov', 'rbp, qword ptr [r8 + 0x18]'), (6442715732, 'test', 'r14d, r14d'), (6442715750, 'call', '0x180040670'), (6442715780, 'call', '0x1800403e0'), (6442715793, 'mov', 'r14d, dword ptr [rsp + 0x30]'), (6442715801, 'cmp', 'r14, rbp'), (6442715804, 'jae', '0x180040aa3'), (6442715806, 'cmp', 'r14, r15'), (6442715809, 'jb', '0x180040a54')]
instructions={}
for a,z,h in regions:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
 instructions.update({i.address:(i.mnemonic,i.op_str) for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,a)})
for a,m,o in pins:assert instructions[a]==(m,o),(hex(a),instructions.get(a))
constant=0x18004033e+0x1e9a4a
assert b.data[b.off(constant):b.off(constant)+4]==struct.pack('<I',0xff7fffff)
out=Path(sys.argv[1]).resolve();out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(dict(regions=regions,instructionPins=len(pins),
 semantics=['040270 first strict column maximum from -FMAX; finite ties retain earlier row',
 '0403E0 group duplicate rows in first occurrence order; retain strongest column, first tie',
 '040670 all selected rows then columns masked -FMAX; paired cells restored +FMAX afterwards',
 '0409F0 repeats mask/reselect while pair count is below BOTH dimensions'],
 adaptation='Prepared row-major finite inputs strictly within sentinel extrema; bounded payload and cancellation; no native allocation-prefix claim',
 missing=['040CD0 pair averaging and correction side effects','actual existing C25/source producer','owning record integration'],
 vendorExecution=False),indent=2)+'\n',encoding='utf8')
print(f'PASS4 complete matching regions/{len(pins)} pins/negative sentinel binding; prepared finite kernel only')
