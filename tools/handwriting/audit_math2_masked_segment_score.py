"""Static041CF0 mask/remap/count/diagnostic store proof; no vendor execution."""
from pathlib import Path
import hashlib
import json
import sys
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

if len(sys.argv) != 2:
    raise SystemExit('report.json')
b = Binary('MyScriptDocument.dll')
start, end = 0x180041cf0, 0x180041ec0
raw = b.data[b.off(start):b.off(end)]
digest = hashlib.sha256(raw).hexdigest()
assert digest == '58885c0c050e8841686ded2be8584433c4f0cd819621d12efb9ad9ae720cfebe'
instructions = {i.address: (i.mnemonic, i.op_str) for i in Cs(CS_ARCH_X86, CS_MODE_64).disasm(raw, start)}
pins = [
    (0x180041d0c, 'mov', 'edi, dword ptr [r9 + 8]'),
    (0x180041d33, 'call', '0x180022df0'),
    (0x180041d3d, 'test', 'al, al'),
    (0x180041d3f, 'jne', '0x180041d49'),
    (0x180041d41, 'xor', 'r12b, r12b'),
    (0x180041d90, 'mov', 'dword ptr [r9], edx'),
    (0x180041d93, 'lea', 'eax, [rdx + 1]'),
    (0x180041d96, 'movzx', 'ecx, byte ptr [r8]'),
    (0x180041d9e, 'test', 'cl, cl'),
    (0x180041da4, 'cmovne', 'eax, edx'),
    (0x180041da7, 'mov', 'edx, eax'),
    (0x180041dd5, 'movsxd', 'rax, dword ptr [rsi]'),
    (0x180041dd8, 'cmp', 'eax, -1'),
    (0x180041ddb, 'je', '0x180041e15'),
    (0x180041de0, 'inc', 'ebx'),
    (0x180041de5, 'cmp', 'byte ptr [rcx + rax], 0'),
    (0x180041de9, 'jne', '0x180041e15'),
    (0x180041deb, 'movsxd', 'rcx, dword ptr [r15 + rcx*4]'),
    (0x180041df2, 'imul', 'rax, qword ptr [r13 + 0x78]'),
    (0x180041df7, 'imul', 'rcx, qword ptr [r13 + 0x70]'),
    (0x180041e03, 'movss', 'xmm0, dword ptr [rax + rcx*4]'),
    (0x180041e08, 'maxss', 'xmm0, xmm7'),
    (0x180041e0c, 'call', '0x1801a8c30'),
    (0x180041e11, 'subss', 'xmm6, xmm0'),
    (0x180041e31, 'divss', 'xmm6, xmm0'),
    (0x180041e4a, 'mov', 'rdx, qword ptr [rcx + 0x90]'),
    (0x180041e56, 'mov', 'eax, dword ptr [rcx]'),
    (0x180041e58, 'mov', 'dword ptr [rdx], eax'),
    (0x180041e61, 'movss', 'dword ptr [rax + 4], xmm6'),
    (0x180041e6e, 'mulss', 'xmm6, xmm8'),
    (0x180041e73, 'addss', 'xmm6, dword ptr [rcx]'),
    (0x180041e77, 'movss', 'dword ptr [rcx], xmm6'),
]
for address, mnemonic, operands in pins:
    assert instructions[address] == (mnemonic, operands), (hex(address), instructions.get(address))
report = dict(regionSha256=digest, regions=1, instructionPins=len(pins),
    mask='Any nonzero byte deletes; remap is zero-byte count strictly before original index',
    denominator='All nonnegative assigned original indices, including deleted positions',
    diagnostics='Before and unweighted penalty stores precede separate F32 multiply/add',
    scope='Prepared finite compressed matrix only; not EF4 request dispatch or record publication',
    nativeAllocatorFailure='Return false before numerical stores; managed preparation throws instead')
Path(sys.argv[1]).write_text(json.dumps(report, indent=2)+'\n', encoding='utf8')
print(f'PASS041CF0 whole region/{len(pins)} pins; mask remap, denominator and diagnostics order; no vendor execution')
