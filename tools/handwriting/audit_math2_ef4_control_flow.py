"""Pin the actual request-byte producer and EF4 branch calls; static only."""
import hashlib
import json
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
regions = [
    (0x1800450c0, 0x180045460, '155f71ec1b5fea2831e30c94fa71dfcd3d3c8753d41bfe22dd27df8b9eb51693'),
    (0x1800359f0, 0x180035c50, '44a3ff6d114956fe746eb35b3d80f0e3867f6094fc8d8f0da18c353ef1ce6b48'),
    (0x180036f20, 0x180037430, 'c4b2044a8fa4bcc00c9ede6610d678daa4f22e379032a38944c9181678559b85'),
    (0x180042100, 0x1800429fa, '5759f6dc6cc730baa03b865f1ba5a75a659b75a47801ef520f5fa8041d726599'),
]
instructions = {}
for start, end, digest in regions:
    raw = b.data[b.off(start):b.off(end)]
    assert hashlib.sha256(raw).hexdigest() == digest
    instructions.update({i.address: (i.mnemonic, i.op_str) for i in md.disasm(raw, start)})
pins = [
    (0x18004533a, 'movzx', 'eax, byte ptr [rdx + 4]'),
    (0x18004533e, 'mov', 'byte ptr [rdi + 0xb1], al'),
    (0x180035aea, 'movups', 'xmm0, xmmword ptr [rbp + 0x40]'),
    (0x180035aee, 'movups', 'xmmword ptr [rdi + 0x40], xmm0'),
    (0x18003733a, 'movzx', 'eax, byte ptr [rsi + 0x51]'),
    (0x18003733e, 'mov', 'byte ptr [rsp + 0x40], al'),
    (0x180042406, 'cmp', 'byte ptr [rbp + 0x2a0], 0'),
    (0x18004242f, 'jne', '0x180042473'),
    (0x18004243e, 'mov', 'byte ptr [rsp + 0x28], 1'),
    (0x180042457, 'call', '0x180041020'),
    (0x180042477, 'cmp', 'dword ptr [rax + 8], 0'),
    (0x180042486, 'mov', 'byte ptr [rsp + 0x28], 0'),
    (0x18004249c, 'xor', 'edx, edx'),
    (0x1800424a1, 'call', '0x180041020'),
    (0x180042707, 'call', '0x180040cd0'),
    (0x18004272f, 'call', '0x180041020'),
    (0x1800427bd, 'call', '0x180041240'),
    (0x1800427d7, 'call', '0x180041ec0'),
    (0x180042809, 'call', '0x180041660'),
    (0x1800428d5, 'call', '0x180041ba0'),
    (0x1800428e6, 'call', '0x180041a30'),
    (0x18004290c, 'call', '0x180041cf0'),
]
for address, mnemonic, operands in pins:
    assert instructions[address] == (mnemonic, operands), (hex(address), instructions.get(address))
assert 0x70 + 0x41 == 0xb1 and 0x10 + 0x41 == 0x51
calls = [int(operands, 16) for mnemonic, operands in instructions.values()
         if mnemonic == 'call' and operands.startswith('0x18004')]
for helper in (0x180041020, 0x180040cd0, 0x180041240, 0x180041ec0,
               0x180041660, 0x180041ba0, 0x180041a30, 0x180041cf0):
    assert helper in calls
report = dict(scope='Native request flag and EF4 branch graph; helpers not implemented by this audit',
              regions=4, instructionPins=len(pins),
              flagRoute='request.byte4 -> EED.B1 -> packet41 -> EEE.51 -> EF4.arg9',
              helpers=[hex(x) for x in sorted(set(calls))],
              actualRequestByteDefaultEstablished=False)
Path(sys.argv[1]).write_text(json.dumps(report, indent=2) + '\n', encoding='utf8')
print('PASS4 pinned EF4/control regions/22pins; request flag is not inferred from graph count')
