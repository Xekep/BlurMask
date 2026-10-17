"""Static finite fresh041020 argmax and041A30 run-boundary checkpoint."""
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
regions = [
    (0x180041020, 0x180041240, '0eb8647538e57555fb3b2a30a713ef954c06cd08f05707f9a766d242be28faee'),
    (0x180041a30, 0x180041ba0, '8c405e02fcd4c5449c1886ba248ba4c74816bb4f04e321877575bc5d727a44de'),
]
instructions = {}
for start, end, digest in regions:
    raw = b.data[b.off(start):b.off(end)]
    assert hashlib.sha256(raw).hexdigest() == digest
    instructions.update({i.address: (i.mnemonic, i.op_str) for i in md.disasm(raw, start)})
pins = [
    (0x1800410fc, 'cmp', 'byte ptr [rsp + 0xa8], 0'),
    (0x180041104, 'jne', '0x180041121'),
    (0x180041124, 'mov', 'edx, 0xffffffff'),
    (0x180041138, 'imul', 'r9, qword ptr [r13 + 0x78]'),
    (0x18004113d, 'mov', 'rbx, qword ptr [r13 + 0x70]'),
    (0x180041150, 'movss', 'xmm0, dword ptr [r11 + rax*4]'),
    (0x180041156, 'comiss', 'xmm0, xmm1'),
    (0x180041159, 'jbe', '0x18004115e'),
    (0x18004115e, 'comiss', 'xmm0, xmm2'),
    (0x180041164, 'cmovbe', 'eax, edx'),
    (0x18004117b, 'mov', 'ebp, dword ptr [rax + rcx*4]'),
    (0x180041a3a, 'cmp', 'dword ptr [rdx + 8], 0'),
    (0x180041a73, 'cmp', 'edi, dword ptr [r14 + 8]'),
    (0x180041a85, 'cmp', 'ebp, edx'),
    (0x180041a87, 'je', '0x180041afb'),
    (0x180041a89, 'mov', 'dword ptr [rsp + 0x28], edx'),
    (0x180041a8d, 'mov', 'dword ptr [rsp + 0x2c], edi'),
    (0x180041b23, 'mov', 'eax, dword ptr [r14 + 8]'),
    (0x180041b27, 'mov', 'dword ptr [rsp + 0x24], eax'),
    (0x180041b2b, 'mov', 'dword ptr [rsp + 0x20], edx'),
]
for address, mnemonic, operands in pins:
    assert instructions[address] == (mnemonic, operands), (hex(address), instructions.get(address))
assert struct.unpack_from('<I', b.data, b.off(0x1800410f4 + 0x1e8c94))[0] == 0xff7fffff
report = dict(scope='Finite prepared fresh assignment and run compression only; no request-policy selection or full EF4',
              regions=2, instructionPins=len(pins), bestInitialBits='FF7FFFFF',
              tie='First finite winner', runRecord='original primary index / exclusive segment end')
Path(sys.argv[1]).write_text(json.dumps(report, indent=2) + '\n', encoding='utf8')
print('PASS2 pinned segment-helper regions/20pins/negativeFMAX; finite prepared scope only')
