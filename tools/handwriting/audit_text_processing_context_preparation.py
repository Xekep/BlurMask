"""Read-only 2C1/2DA construction and nonempty INPUT request preparation proof."""
import argparse
import hashlib
import json
import sys
from pathlib import Path

from capstone import Cs, CS_ARCH_X86, CS_MODE_64

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('output', type=Path)
args = parser.parse_args()
text = Binary('MyScriptText.dll')
assert hashlib.sha256(text.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
decoder = Cs(CS_ARCH_X86, CS_MODE_64)
regions, bindings = [], []
pin_count = 0

def region(start, end, pins):
    global pin_count
    raw = text.data[text.off(start):text.off(end)]
    rows = [(i.address, i.mnemonic, i.op_str) for i in decoder.disasm(raw, start)]
    for pin in pins:
        assert pin in rows, pin
    pin_count += len(pins)
    regions.append(dict(start=hex(start), end=hex(end), sha256=hashlib.sha256(raw).hexdigest()))

def bind(table, slot, target):
    assert text.qwords(table + slot, 1) == (target,)
    bindings.append(dict(table=hex(table), slot=hex(slot), target=hex(target)))

bind(0x180276a08, 0, 0x180053c30)
bind(0x1802769a0, 0, 0x180053de0)
bind(0x1802769a0, 0x10, 0x180054340)
bind(0x1802754b0, 0, 0x18001c010)
bind(0x180275550, 0x28, 0x18002d8c0)
bind(0x180275550, 0x240, 0x180020b40)
bind(0x180276740, 0x18, 0x180050660)

region(0x180053c30, 0x180053de0, [
    (0x180053c64, 'test', 'al, al'), (0x180053c66, 'jne', '0x180053c73'),
    (0x180053c78, 'mov', 'edx, 0x2da'),
    (0x180053c84, 'mov', 'qword ptr [rdi + 0x10], rbp'),
    (0x180053c88, 'mov', 'qword ptr [rdi + 0x58], rbp'),
    (0x180053cbb, 'mov', 'qword ptr [rdi + 0x10], rax'),
    (0x180053cc2, 'jne', '0x180053d32'),
    (0x180053cd9, 'call', 'r8'), (0x180053cf6, 'call', 'r8'),
    (0x180053cfd, 'mov', 'edx, 4'), (0x180053d1d, 'call', 'r8'),
    (0x180053d51, 'mov', 'byte ptr [rdi + 0x50], 1'),
    (0x180053dbe, 'mov', 'qword ptr [rdi + 0x100], rbp'),
    (0x180053dc5, 'mov', 'qword ptr [rdi + 0x108], rbp')])
region(0x18001c010, 0x18001c2c0, [
    (0x18001c058, 'mov', 'qword ptr [rdi + 0x30], 0xffffffffffffffff'),
    (0x18001c104, 'mov', 'edx, 0x3e'),
    (0x18001c109, 'mov', 'dword ptr [rdi + 0xd8], ebp'),
    (0x18001c139, 'mov', 'qword ptr [rdi + 0x110], rbp'),
    (0x18001c164, 'mov', 'qword ptr [rdi + 0x118], rax'),
    (0x18001c16e, 'je', '0x18001c2a6'),
    (0x18001c25c, 'lea', 'rdx, [rdi + 0x208]'),
    (0x18001c272, 'call', 'r8')])
region(0x180053de0, 0x180053f91, [
    (0x180053df6, 'mov', 'rdx, qword ptr [rdx + 0x10]'),
    (0x180053e0c, 'call', 'r8'),
    (0x180053e2f, 'call', '0x1800538b0'),
    (0x180053e57, 'call', '0x180053500'),
    (0x180053e9d, 'mov', 'rdx, qword ptr [rbp + 0x58]'),
    (0x180053eb0, 'call', 'r8'),
    (0x180053ee7, 'mov', 'eax, dword ptr [rbp + 0x108]'),
    (0x180053f29, 'call', 'r8')])
region(0x180054340, 0x180054a23, [
    (0x18005435e, 'test', 'r8, r8'), (0x180054361, 'je', '0x1800549ba'),
    (0x180054367, 'cmp', 'dword ptr [r8 + 0x20], 0'),
    (0x18005436c, 'je', '0x1800549b4'),
    (0x180054376, 'mov', 'edx, 0x2db'),
    (0x1800543c1, 'mov', 'eax, dword ptr [rdi + 0x28]'),
    (0x1800543d3, 'je', '0x1800545c1'),
    (0x1800545c1, 'test', 'bl, bl'), (0x1800545c3, 'je', '0x180054547'),
    (0x1800545db, 'mov', 'r8, qword ptr [rcx + 0x10]'),
    (0x1800545d8, 'mov', 'r9, qword ptr [rbx]'),
    (0x1800545e5, 'mov', 'r10, qword ptr [rax + 0x28]'),
    (0x1800545e9, 'mov', 'rax, qword ptr [r13 + 0x10]'),
    (0x1800545ed, 'mov', 'qword ptr [rsp + 0x20], rax'),
    (0x1800545f2, 'call', 'r10'), (0x1800545f7, 'je', '0x180054547'),
    (0x180054599, 'mov', 'r10, qword ptr [rax + 0x18]'),
    (0x1800545a6, 'call', 'r10'), (0x1800545ab, 'je', '0x18005472a'),
    (0x18005472d, 'mov', 'dword ptr [rdi + 0x18], 0'),
    (0x18005474e, 'cmp', 'dword ptr [rax + 0x210], 0'),
    (0x180054755, 'ja', '0x180054936'),
    (0x180054936, 'mov', 'byte ptr [rdi + 0x18], 1'),
    (0x18005495f, 'mov', 'r10, qword ptr [rax + 0x240]'),
    (0x18005498f, 'call', 'r8'),
    (0x180054a16, 'mov', 'dword ptr [rdi + 0x18], 0x100')])
region(0x1800304a3, 0x1800304f7, [
    (0x1800304a7, 'cmp', 'r8, -1'),
    (0x1800304b7, 'mov', 'r9, qword ptr [rax + 0x128]'),
    (0x1800304be, 'call', 'r9'),
    (0x1800304c6, 'mov', 'r9d, dword ptr [rax + 0x20]'),
    (0x1800304cf, 'lea', 'rdx, [rsi + 0x208]'),
    (0x1800304da, 'mov', 'r8, qword ptr [rax + 0x18]'),
    (0x1800304e6, 'mov', 'r10, qword ptr [rcx + 0x88]'),
    (0x1800304f0, 'call', 'r10')])

args.output.write_bytes((json.dumps(dict(regions=regions, bindings=bindings, pins=pin_count,
    vendorExecution=False, scope='Construction/import branch only; no full049370, no existing library Seed coercion'),
    indent=2) + '\n').encode())
print(f'PASS {len(regions)} regions / {len(bindings)} bindings / {pin_count} pins; nonempty C22 preparation is not zero DAF.')
