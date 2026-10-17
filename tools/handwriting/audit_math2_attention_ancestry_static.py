"""Static attention-history producer and backward index proof; no DLL execution."""
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
md = Cs(CS_ARCH_X86, CS_MODE_64)
document, engine = Binary('MyScriptDocument.dll'), Binary('MyScriptEngine.dll')
regions = [
    (document, 0x180036f20, 0x180037430, 'c4b2044a8fa4bcc00c9ede6610d678daa4f22e379032a38944c9181678559b85'),
    (document, 0x18003cac0, 0x18003d200, '1615d98a188c9c1e1e3f4aa2f3279e196f8a361f854e694f1ff2cf0c68f8cc96'),
    (engine, 0x180185a50, 0x180185d30, '9231530771de92793d4e7ab33e55450814caa3e10942b9ee1f04509891269974'),
    (engine, 0x180186650, 0x1801869a0, '18457a2b4fb14051d9717c1a983cf910d355a2384181a44007a9371229bf1f6b'),
]
instructions = {}
for binary, start, end, digest in regions:
    raw = binary.data[binary.off(start):binary.off(end)]
    assert hashlib.sha256(raw).hexdigest() == digest
    instructions.update({i.address: (i.mnemonic, i.op_str) for i in md.disasm(raw, start)})
pins = [
    # Completed parent60 -> ancestry88[parent][prefixCount].
    (0x180036ff4, 'movsxd', 'rdx, dword ptr [rbx + 0x60]'),
    (0x180036ff8, 'lea', 'r13d, [rdi - 1]'),
    (0x180036ffc, 'mov', 'rax, qword ptr [rsi + 0x88]'),
    (0x180037008, 'mov', 'rax, qword ptr [rax + rdx*8]'),
    (0x18003700c, 'movsxd', 'r12, dword ptr [rax + rcx*4]'),
    (0x1800370aa, 'movzx', 'ebx, word ptr [rdi + rax]'),
    (0x1800370d0, 'mov', 'rax, qword ptr [rax + 0x10]'),
    # Two squeezeAlongDim calls: time singleton2/0, then beam1/ancestor.
    (0x18003715f, 'mov', 'r9d, 2'),
    (0x180037165, 'mov', 'qword ptr [rsp + 0x20], rbx'),
    (0x180037174, 'mov', 'rax, qword ptr [rax + 0x50]'),
    (0x180037198, 'mov', 'r9d, 1'),
    (0x18003719e, 'mov', 'qword ptr [rsp + 0x20], r12'),
    (0x1800371ad, 'mov', 'rax, qword ptr [rax + 0x50]'),
    (0x180037277, 'mov', 'rax, 0xffffffffff7fffff'),
    (0x18003727e, 'rep stosd', 'dword ptr [rdi], eax'),
    (0x180037290, 'dec', 'r13d'),
    (0x1800372a7, 'mov', 'rax, qword ptr [rax + 0x88]'),
    (0x1800372b2, 'movsxd', 'r12, dword ptr [rcx + rdx]'),
    (0x1800372b6, 'sub', 'rdx, 4'),
    # EEF per-time saved attention producer.
    (0x18003d0b5, 'cmp', 'byte ptr [rdi + 0x30], 0'),
    (0x18003d0f0, 'mov', 'rdx, qword ptr [rdi + 0xf8]'),
    (0x18003d100, 'call', '0x180034630'),
    (0x18003d191, 'mov', 'rdx, qword ptr [rsp + 0x50]'),
    (0x18003d196, 'mov', 'r9d, 2'),
    (0x18003d19f, 'mov', 'dword ptr [rsp + 0x20], 0xffffffff'),
    (0x18003d1aa, 'mov', 'r10, qword ptr [rax + 0x30]'),
    # copySubTensor special last-frame selection and squeeze rank/index guards.
    (0x180185b03, 'cmp', 'ecx, -1'),
    (0x180185b08, 'mov', 'ecx, dword ptr [r14 + rbp*8 + 0x10]'),
    (0x180185b0d, 'dec', 'ecx'),
    (0x1801866b3, 'cmp', 'r15, r8'),
    (0x1801866b8, 'cmp', 'r8, 1'),
    (0x1801866c6, 'cmp', 'rax, qword ptr [r13 + r15*8 + 0x10]'),
]
for address, mnemonic, operands in pins:
    assert instructions[address] == (mnemonic, operands), (hex(address), instructions.get(address))
assert engine.qwords(0x18022a200 + 0x30, 1)[0] == 0x180185a50
assert engine.qwords(0x18022a200 + 0x50, 1)[0] == 0x180186650
report = dict(scope='Static native producer/backward indexing only; carried-state identity tested separately',
              regions=len(regions), instructionPins=len(pins), bindings=2,
              completedParentOffset=0x60, ancestryOffset=0x88, perTimeAttentionOffset=0xf8)
Path(sys.argv[1]).write_text(json.dumps(report, indent=2) + '\n', encoding='utf8')
print('PASS4 pinned attention regions/32 instruction pins/2 actual tensor bindings; no vendor execution')
