"""Pinned read-only evidence for ordinary167210 and paired168d00/168e10 metadata."""
import argparse
import hashlib
import json
import pathlib
import sys

import capstone

p = argparse.ArgumentParser()
p.add_argument('--output', required=True)
a = p.parse_args()
root = pathlib.Path(__file__).resolve().parents[2]
sys.path.insert(0, str(root / 'research/myscript/scripts'))
from inspect_text_features import Binary, functions

b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
cs = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)

def instructions(start, length):
    return list(cs.disasm(b.data[b.off(start):b.off(start) + length], start))

first = instructions(0x180168d00, 0xb3)
last = instructions(0x180168e10, 0xb3)
assert len(first) == len(last)
for x, y in zip(first, last):
    assert x.mnemonic == y.mnemonic
    if x.address == 0x180168da3:
        assert x.op_str == 'rax, qword ptr [r10 + 0xf8]'
        assert y.op_str == 'rax, qword ptr [r10 + 0x108]'
    elif x.mnemonic.startswith('j'):
        assert int(y.op_str, 16) - int(x.op_str, 16) == 0x110
    else:
        assert x.op_str == y.op_str, (x.op_str, y.op_str)
text = '\n'.join(f'{x.mnemonic} {x.op_str}' for x in last)
for pattern in ('[r10 + 0x118]', 'div r8d', 'setne cl', 'add ecx, eax',
                'sub r8d, r9d', 'movzx edx, r8b', '[r10 + 0xd8]',
                'lea rdx, [rcx + rcx*2]', 'lea rax, [rax + rdx*4]'):
    assert pattern in text, pattern
f = functions()['180167210']
for pattern in ('FUN_1801595b0(param_1,*(longlong *)(param_2 + 0x120) + 0xf8)',
                'FUN_1801595b0(param_1,*(longlong *)(param_2 + 0x120) + 0x108)',
                'iVar23 = piVar18[uVar19]',
                'iVar23 = piVar18[(int)uVar19 + 1] + -1',
                '*(short *)(param_2 + 0x8c) == 0',
                '8388608.0'):
    assert pattern in f, pattern
report = dict(
    getterInstructions=len(first),
    pairedGetters='168D00/168E10 identical addressing and ceil-map/D8 lookup; only F8 versus108 owner differs',
    endingKey='same first/physical fields; next retained raw ordinal minus1, with raw-count sentinel at final retained point',
    scope='Ordinary owned source keys only; full1A9AE0 graph source-range dispatch is not reconstructed here; no vendor execution')
target = pathlib.Path(a.output)
target.parent.mkdir(parents=True, exist_ok=True)
target.write_text(json.dumps(report, separators=(',', ':')) + '\n', encoding='utf-8')
print(json.dumps(report))
