"""Static041CF0 fresh-score arithmetic/floor checkpoint; vendor code is data."""
import hashlib
import json
import struct
import sys
from pathlib import Path

from capstone import Cs, CS_ARCH_X86, CS_MODE_64

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

if len(sys.argv) != 2:
    raise SystemExit('report.json')
b = Binary('MyScriptDocument.dll')
md = Cs(CS_ARCH_X86, CS_MODE_64)
start, end = 0x180041cf0, 0x180041ec0
raw = b.data[b.off(start):b.off(end)]
assert hashlib.sha256(raw).hexdigest() == '58885c0c050e8841686ded2be8584433c4f0cd819621d12efb9ad9ae720cfebe'
instructions = {i.address: (i.mnemonic, i.op_str) for i in md.disasm(raw, start)}
pins = [
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
    (0x180041e26, 'test', 'ebx, ebx'),
    (0x180041e31, 'divss', 'xmm6, xmm0'),
    (0x180041e37, 'xorps', 'xmm6, xmm6'),
    (0x180041e6e, 'mulss', 'xmm6, xmm8'),
    (0x180041e73, 'addss', 'xmm6, dword ptr [rcx]'),
    (0x180041e77, 'movss', 'dword ptr [rcx], xmm6'),
]
for address, mnemonic, operands in pins:
    assert instructions[address] == (mnemonic, operands), (hex(address), instructions.get(address))
assert struct.unpack_from('<I', b.data, b.off(0x180041dd5 + 0x1e7e1b))[0] == 0x2edbe6ff
log = list(md.disasm(b.data[b.off(0x1801a8c30):b.off(0x1801a8c80)], 0x1801a8c30))
assert (log[2].mnemonic, log[2].op_str) == ('vmovd', 'eax, xmm0')
report = dict(scope='Finite prepared fresh/no-deletion score only; native log CPU dispatch is not ported',
              regions=1, instructionPins=len(pins), probabilityFloorBits='2EDBE6FF',
              operations='F32 MAXSS floor; LOG; SUBSS accumulation; DIVSS count; separate MULSS/ADDSS',
              existingMaskRemapImplemented=False, diagnosticWritesImplemented=False)
Path(sys.argv[1]).write_text(json.dumps(report, indent=2) + '\n', encoding='utf8')
print('PASS041CF0 region/19pins/F32log ABI and probability floor; prepared fresh scope only')
