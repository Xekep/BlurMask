"""Fresh, unmodified EEB correction-byte producer; static binaries only."""
from pathlib import Path
import hashlib
import json
import struct
import subprocess
import sys
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv) != 2:
    raise SystemExit('report.json')
out = Path(sys.argv[1]).resolve()
dependency = out.with_suffix('.ef4.json')
subprocess.run([sys.executable, '-B', str(ROOT/'tools/handwriting/audit_math2_ef4_control_flow.py'), str(dependency)], check=True)
b = Binary('MyScriptDocument.dll')
assert hashlib.sha256(b.data).hexdigest() == '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
regions = [
    (0x1800277b0, 0x180027840, '4379afdb74b7d3a8f2c39de533454a84aec09b82420e03859cba5caa54cfebe8'),
    (0x180027970, 0x180027c20, 'ef572e99b5e6f65d4bc94e8b77a9ae65edb867b41b64ed3ab804199122c2eef9'),
    (0x180045460, 0x180045b90, '8a9d69ac15350ad940b5871b5753d679ea833dafffe8eba010d8699cb47ff908'),
]
instructions = {}
for start, end, digest in regions:
    raw = b.data[b.off(start):b.off(end)]
    assert hashlib.sha256(raw).hexdigest() == digest
    instructions.update({i.address: (i.mnemonic, i.op_str) for i in Cs(CS_ARCH_X86, CS_MODE_64).disasm(raw, start)})
pins = [
    (0x1800277be, 'mov', 'rdi, rdx'),
    (0x1800277f9, 'xor', 'ecx, ecx'),
    (0x180027806, 'mov', 'qword ptr [rdi + 0x48], rcx'),
    (0x180027829, 'mov', 'dword ptr [rdi + 0x38], 0xffffffff'),
    (0x180027830, 'mov', 'word ptr [rdi + 0x3c], 0x101'),
    (0x180027836, 'mov', 'byte ptr [rdi + 0x3e], 1'),
    (0x18002797c, 'mov', 'rbp, rdx'),
    (0x180027ad9, 'lea', 'r8, [rbp + 0x38]'),
    (0x180027ae0, 'mov', 'rax, qword ptr [rax + 8]'),
    (0x180027ae8, 'call', 'rax'),
    (0x180045938, 'cmp', 'qword ptr [rdi + 0x220], r12'),
    (0x18004593f, 'jne', '0x180045980'),
    (0x18004594a, 'call', '0x1800450c0'),
]
for address, mnemonic, operands in pins:
    assert instructions[address] == (mnemonic, operands), (hex(address), instructions.get(address))
bindings = [
    (0x1801b7390+8, 0x400000eeb),
    (0x1801b7390+0x18, 0x1801b7458),
    (0x1801b7458+0x10, 0x1801b73d8),
    (0x1801b73d8, 0x1800277b0),
    (0x1801b7390+0x20, 0x1801b7420),
    (0x1801b7420+0x28, 0x180027970),
    (0x1801fd090+0x20, 0x1801fd0d8),
    (0x1801fd0d8+8, 0x180045460),
]
for address, value in bindings:
    assert b.qwords(address, 1)[0] == value
packet = struct.pack('<IHB', 0xffffffff, 0x101, 1)
assert packet[4] == 1 and 0x38+4 == 0x3c
dep = json.loads(dependency.read_text())
assert dep['flagRoute'] == 'request.byte4 -> EED.B1 -> packet41 -> EEE.51 -> EF4.arg9'
report = dict(regions=len(regions), instructionPins=len(pins), tableBindings=len(bindings),
    freshUnmodifiedEebRequestByte4=packet[4], correctionBranch='nonzero',
    route='EEB.3C low byte of0101 -> EEB+38 request.byte4 -> EED.B1 -> EEE.51 -> EF4.arg9',
    prerequisites=['Successful fresh EEB constructor', 'Unmodified request bytes',
                   'Cold EED source whose cloned220 decoder is null', 'Successful045460/0450C0'],
    exclusions=['External request setters and callbacks', 'Warm/copied/raw-initialized prepared EED decoder',
                'No claim that arbitrary host request defaults are one', 'Correction helpers not implemented by this audit'],
    ef4Dependency=dep)
out.write_text(json.dumps(report, indent=2)+'\n', encoding='utf8')
print(f'PASS fresh unmodified EEB correctionbyte=1;3regions/{len(pins)}pins/{len(bindings)}bindings plus EF4 consumer; no runtime change')
