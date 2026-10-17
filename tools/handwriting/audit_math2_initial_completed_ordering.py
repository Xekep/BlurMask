"""Pin the owning initial-sort boundary; vendor PE reads only."""
import hashlib
import json
from pathlib import Path
import subprocess
import sys

from capstone import Cs, CS_ARCH_X86, CS_MODE_64

if len(sys.argv) != 2:
    raise SystemExit('report.json')
ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

binary = Binary('MyScriptDocument.dll')
assert hashlib.sha256(binary.data).hexdigest() == '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
lo, hi = 0x180038610, 0x18003a260
raw = binary.data[binary.off(lo):binary.off(hi)]
assert hashlib.sha256(raw).hexdigest() == '180a1bcc3d913b2a19469e34a2b8a184e5a083354ef8f59414bed3d913e2716a'
instructions = {item.address: (item.mnemonic, item.op_str)
    for item in Cs(CS_ARCH_X86, CS_MODE_64).disasm(raw, lo)}
regions = [
    (0x180039a96, 0x180039ab8, 'fdd1996ccde4f90fdf1cb68f03041ce586a0dbcc0271c441a28c9ca8649b1b86'),
    (0x180039bf2, 0x180039c2b, '9f9b61dbc1851b65d0529fd261ea360f33798625f7321be1930e39d19c53793f'),
]
for first, last, expected in regions:
    assert hashlib.sha256(binary.data[binary.off(first):binary.off(last)]).hexdigest() == expected
pins = [
    (0x180039a96, 'mov', 'eax, dword ptr [rdi + 8]'),
    (0x180039a99, 'mov', 'esi, dword ptr [rsi + 0x30]'),
    (0x180039a9c, 'cmp', 'eax, 1'), (0x180039a9f, 'jbe', '0x180039ab8'),
    (0x180039aa1, 'dec', 'eax'), (0x180039aa3, 'movsxd', 'rcx, eax'),
    (0x180039aa6, 'imul', 'rdx, rcx, 0xa8'),
    (0x180039aad, 'mov', 'rcx, qword ptr [rdi]'),
    (0x180039ab0, 'add', 'rdx, qword ptr [rdi]'),
    (0x180039ab3, 'call', '0x180036810'),
    (0x180039ac0, 'je', '0x180039bf2'),
    (0x180039acd, 'cmp', 'qword ptr [rax + 0xc0], 0'),
    (0x180039ad7, 'cmp', 'byte ptr [rax + 0xb6], 0'),
    (0x180039ae9, 'call', '0x180038170'),
    (0x180039afc, 'cmp', 'dword ptr [r13 + 0x5c], 0'),
    (0x180039b10, 'call', '0x180038340'),
    (0x180039b23, 'cmp', 'byte ptr [r13 + 0x38], 0'),
    (0x180039be7, 'call', '0x180036f20'),
    (0x180039bf2, 'mov', 'eax, dword ptr [rdi + 8]'),
    (0x180039bf5, 'cmp', 'eax, 1'), (0x180039bf8, 'jbe', '0x180039c2b'),
    (0x180039bfa, 'dec', 'eax'), (0x180039bfc, 'movsxd', 'rcx, eax'),
    (0x180039bff, 'imul', 'rdx, rcx, 0xa8'),
    (0x180039c06, 'mov', 'rcx, qword ptr [rdi]'),
    (0x180039c09, 'add', 'rdx, qword ptr [rdi]'),
    (0x180039c0c, 'call', '0x180036810'),
    (0x180039c11, 'mov', 'r9d, dword ptr [rdi + 8]'),
    (0x180039c15, 'cmp', 'r9d, esi'), (0x180039c18, 'jbe', '0x180039c2b'),
    (0x180039c1a, 'sub', 'r9d, esi'), (0x180039c1d, 'mov', 'r8d, esi'),
    (0x180039c26, 'call', '0x180035000'),
]
for address, mnemonic, operand in pins:
    assert instructions[address] == (mnemonic, operand), (hex(address), instructions.get(address))
assert [(a, op) for a, (mn, op) in instructions.items()
    if mn == 'call' and 0x180039a96 <= a < 0x180039ab8] == [(0x180039ab3, '0x180036810')]

out = Path(sys.argv[1]).resolve()
out.parent.mkdir(parents=True, exist_ok=True)
numeric = out.with_name(out.stem + '-numeric036810.json')
subprocess.run([sys.executable, str(ROOT / 'tools/handwriting/audit_math_candidate_ordering.py'), str(numeric)], check=True)
numeric_report = json.loads(numeric.read_bytes())
assert numeric_report['stride'] == 168 and numeric_report['inclusiveShortRangeMaximum'] == 20
out.write_text(json.dumps(dict(regions=regions, completeDriverSha256=hashlib.sha256(raw).hexdigest(),
    instructionPins=len(pins), numericKernel=numeric_report,
    boundary='Initial039AB3 full-vector ordering only; count0/1 bypass. No release/truncation here.',
    later='038170/038340/036F20 precede039C0C final ordering and039C26 tail035000 destruction',
    representation='Score+ordinal168 projection -> validated permutation -> same actual Record/Source/Library objects',
    adaptation='Managed unique-reference swaps retain all original owners; no native pointer image or intermediate write-order claim',
    missing=['postprocessing and caller-policy', 'final ordering/truncation', 'formula output/acceptance'],
    vendorExecution=False), indent=2) + '\n', encoding='utf8')
print(f'PASS2 caller regions/{len(pins)} pins plus whole036810 numerical kernel; initial ordering only, no vendor execution')
