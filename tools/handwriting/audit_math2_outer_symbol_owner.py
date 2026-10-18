"""Read-only F0 -> outer40B symbol ownership and next DOM boundary.

Vendor PE bytes only; procedural selector examples are prepared, not NN output.
"""
from pathlib import Path
import hashlib
import json
import subprocess
import sys
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

if len(sys.argv) != 2:
    raise SystemExit('report.json')
output = Path(sys.argv[1]).resolve()
dependencies = []
for name in ['first_result_copy', 'symbol_dom_transport']:
    path = output.with_suffix('.' + name + '.json')
    subprocess.run([sys.executable, '-B', str(ROOT / ('tools/handwriting/audit_math2_' + name + '.py')),
                    str(path)], check=True)
    dependencies.append(str(path))
document, engine = Binary('MyScriptDocument.dll'), Binary('MyScriptEngine.dll')
assert hashlib.sha256(document.data).hexdigest() == '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
assert hashlib.sha256(engine.data).hexdigest() == 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
regions = [
    (document, 0x180026610, 0x180026690, '6ba217c841d5b335139b504d682f67d87b781068cf52242d5be7820130c624c4'),
    (document, 0x180026690, 0x180026780, '409f1f8eea7ad59d4f0ab831f75f0d5519674dc0fcffe4402dcb82b59d33bb3c'),
    (document, 0x180026780, 0x180026a80, '5ec4f960060dfc2702e03e2ed80a71e88ae3e1503b641fcc1a4a54b03f3fd4e0'),
    (document, 0x180028a80, 0x180028f90, '97200d33a35841e789d9c6fac8dc6b5953a94c02ca456e1dd3963a69469f565d'),
    (document, 0x180029290, 0x18002a440, 'f1c0dbbf545e6b0d33f19eb84c481fc2ed8d47909e4e8f7133fa26a0545f372a'),
    (engine, 0x180032917, 0x180032a23, 'de10a46a74ad0388d9122d53a33d03d1ac0059e4f5950cb3e0c2ee838c565432'),
    (engine, 0x180089770, 0x180089810, '40db49e4cd350c22aab92b7998b3047fa3bd2c069c329a4594875448eded885c'),
    (engine, 0x1801da170, 0x1801da250, '7a6d2d0067b5fc7e092ce2d509299d92b171a1edcb885c9711694ff9bafdd6a5'),
]
decoder = Cs(CS_ARCH_X86, CS_MODE_64)
decoded = {}
for binary, start, end, expected in regions:
    data = binary.data[binary.off(start):binary.off(start) + end - start]
    assert hashlib.sha256(data).hexdigest() == expected, (binary.name, hex(start))
    decoded.update({(binary.name, i.address): (i.mnemonic, i.op_str) for i in decoder.disasm(data, start)})
docpins = [
    (0x180026634, 'mov', 'edx, 0x3e'),
    (0x18002667d, 'mov', 'qword ptr [r14 + 0x10], rax'),
    (0x1800266c7, 'mov', 'rdx, qword ptr [rbx + 0x10]'),
    (0x180026701, 'mov', 'dword ptr [r14 + 8], 0'),
    (0x180026710, 'mov', 'rdx, qword ptr [r14 + 0x10]'),
    (0x180026727, 'mov', 'edx, 0x3e'),
    (0x18002676f, 'mov', 'qword ptr [r14 + 0x10], rax'),
    (0x1800267d2, 'mov', 'r8d, 0x28'),
    (0x18002683a, 'lea', 'rdx, [rbx - 0x18]'),
    (0x180026858, 'call', 'r8'),
    (0x18002685b, 'mov', 'byte ptr [rbx + 8], 0'),
    (0x18002685f, 'mov', 'dword ptr [rbx + 0xc], 0xffffffff'),
    (0x18002687c, 'add', 'dword ptr [r11 + 8], ebp'),
    (0x1800268a9, 'mov', 'byte ptr [rdi + 0x20], al'),
    (0x1800268b1, 'mov', 'dword ptr [rdi + 0x24], eax'),
    (0x18002691e, 'mov', 'r14d, dword ptr [rdx + rcx*8 + 8]'),
    (0x180026923, 'movss', 'xmm6, dword ptr [rdx + rcx*8 + 0xc]'),
    (0x18002693b, 'mov', 'rdx, qword ptr [r11 + 0x10]'),
    (0x18002693f, 'lea', 'r9, [r8 + r14]'),
    (0x18002694e, 'mov', 'r10, qword ptr [rax + 0x10]'),
    (0x180026952, 'call', 'r10'),
    (0x1800269cd, 'mov', 'qword ptr [rdx], rbx'),
    (0x1800269d0, 'mov', 'dword ptr [rdx + 8], esi'),
    (0x1800269d3, 'movss', 'dword ptr [rdx + 0xc], xmm6'),
    (0x1800269d8, 'inc', 'dword ptr [rdi + 0x18]'),
    (0x1800269f1, 'mov', 'r9d, dword ptr [r12 + r15*8 + 8]'),
    (0x1800269fa, 'mov', 'r8, qword ptr [r12 + r15*8]'),
    (0x180026a05, 'mov', 'r10, qword ptr [rax + 0x88]'),
    (0x180026a0c, 'call', 'r10'),
    (0x180028aae, 'call', '0x180026690'),
    (0x180028b6e, 'mov', 'qword ptr [rdx + 8], 7'),
    (0x180028c14, 'mov', 'qword ptr [rdx + 8], 3'),
    (0x180028cb6, 'mov', 'qword ptr [r8 + 8], 8'),
    (0x180028ccc, 'cmp', 'dword ptr [r14 + 8], 1'),
    (0x180028cd1, 'jbe', '0x180028cf6'),
    (0x180028d24, 'imul', 'rsi, rax, 0xf0'),
    (0x180028d2b, 'mov', 'eax, dword ptr [rsi + r14 + 0xa0]'),
    (0x180028d33, 'mov', 'r13, qword ptr [rsi + r14 + 0x98]'),
    (0x180028d4a, 'cmp', 'dword ptr [r13 + 0x18], 0'),
    (0x180028d55, 'mov', 'rcx, qword ptr [r13 + 0x10]'),
    (0x180028d8b, 'call', 'r10'),
    (0x180028da8, 'cmp', 'dword ptr [r13 + rcx*8 + 0x18], 0'),
    (0x180028deb, 'call', 'r10'),
    (0x180028dfe, 'je', '0x180028e28'),
    (0x180028e02, 'je', '0x180028e28'),
    (0x180028e0c, 'cmp', 'r9d, 2'),
    (0x180028e10, 'jbe', '0x180028e28'),
    (0x180028e1e, 'add', 'r9d, -2'),
    (0x180028e22, 'add', 'r8, 0x28'),
    (0x180028e43, 'call', '0x180026780'),
    (0x180028e79, 'call', '0x180026780'),
    (0x180028ed3, 'call', '0x180026780'),
    (0x18002a38e, 'mov', 'rdx, qword ptr [rbp + 0x70]'),
    (0x18002a395, 'mov', 'r8, qword ptr [rbp + 0x88]'),
    (0x18002a39c, 'add', 'rdx, 0x10'),
    (0x18002a3a0, 'call', '0x180028a80'),
    (0x18002a3a5, 'test', 'al, al'),
]
enginepins = [
    (0x18003292e, 'mov', 'r15, qword ptr [rip + 0x21fad3]'),
    (0x180032a18, 'mov', 'rax, qword ptr [r15 + 0x20]'),
    (0x180032a1c, 'mov', 'qword ptr [rcx + 0x90], rax'),
    (0x1800897bc, 'sub', 'r8, r10'),
    (0x1800897f1, 'sub', 'rax, r9'),
    (0x1800897f4, 'cmp', 'r8, rax'),
    (0x1800897f7, 'jbe', '0x1800897ff'),
    (0x1800897ff, 'jae', '0x1800897ac'),
    (0x1800897b2, 'jmp', '0x1801da170'),
    (0x1801da1a0, 'mov', 'al, byte ptr [rcx]'),
    (0x1801da1a2, 'cmp', 'al, byte ptr [rcx + rdx]'),
    (0x1801da1af, 'xor', 'rax, rax'),
]
for binary, pins in [(document, docpins), (engine, enginepins)]:
    for address, mnemonic, operands in pins:
        assert decoded[binary.name, address] == (mnemonic, operands), (hex(address), decoded.get((binary.name, address)))
bindings = [(0x180252408, 0x18020f3c0), (0x18020f3c0 + 0x20, 0x18020f290),
            (0x18020f290 + 0x20, 0x180089770)]
for address, pointer in bindings:
    assert engine.qwords(address, 1) == (pointer,)
for address, value in [(0x18020d520, b'<table>'), (0x18020d52c, b'<\\>'), (0x18020d530, b'</table>')]:
    assert document.data[document.off(address):document.off(address) + len(value) + 1] == value + b'\0'


def prepared_plan(records):
    """Row selection only. This is NOT an actual multi-C23 inference owner."""
    count = len(records)
    result = [('marker', '<table>')] if count > 1 else []
    for index, rows in enumerate(records):
        if count > 1 and not rows:
            raise ValueError('Unproved native nonempty multi-F0 invariant')
        boundary = count > 1 and rows[0] and rows[-1] and rows[0][0] == b'<table>' and rows[-1][0] == b'</table>'
        selected = range(1, len(rows) - 1) if boundary and len(rows) > 2 else range(len(rows))
        result += [('source', index, row) for row in selected]
        if index + 1 < count:
            result.append(('marker', '<\\>'))
    if count > 1:
        result.append(('marker', '</table>'))
    return result


# Deliberately explicit prepared branch discriminators, not expected labels.
plain = [[b'x']]
wrapped = [[b'<table>'], [b'x'], [b'</table>']]
controls = [
    ([], []),
    ([[]], []),
    ([plain], [('source', 0, 0)]),
    ([wrapped], [('source', 0, 0), ('source', 0, 1), ('source', 0, 2)]),
    ([wrapped, plain], [('marker', '<table>'), ('source', 0, 1), ('marker', '<\\>'), ('source', 1, 0), ('marker', '</table>')]),
    ([[[b'<table>'], [b'</table>']], plain], [('marker', '<table>'), ('source', 0, 0), ('source', 0, 1), ('marker', '<\\>'), ('source', 1, 0), ('marker', '</table>')]),
    ([[[b'not-table', b'<table>'], [b'x'], [b'</table>']], plain], [('marker', '<table>'), ('source', 0, 0), ('source', 0, 1), ('source', 0, 2), ('marker', '<\\>'), ('source', 1, 0), ('marker', '</table>')]),
    ([[[b'<table>'], [b'x'], []], plain], [('marker', '<table>'), ('source', 0, 0), ('source', 0, 1), ('source', 0, 2), ('marker', '<\\>'), ('source', 1, 0), ('marker', '</table>')]),
]
for records, expected in controls:
    assert prepared_plan(records) == expected
try:
    prepared_plan([[], plain])
except ValueError:
    pass
else:
    raise AssertionError('Missing explicit unproved-multi-F0 refusal')
result = dict(scope='Read-only actual F0/source-bound40B -> new outer library/vector -> original-source DOM boundary',
              regions=[dict(dll=b.name, start=hex(a), end=hex(z), sha256=h) for b, a, z, h in regions],
              pins=len(docpins) + len(enginepins), bindings=len(bindings), literals=3,
              dependencies=dependencies, preparedSelectionControls=len(controls), preparedMultiEmptyRefusals=1,
              nextOwner='ONE actual completed FirstResultCopy; same Document; fresh outer3E/range copies; no rawunit alias or DOM acceptance',
              unresolved=['multi-C23/common-Document F0 grouping', 'original mathField/category102/CB8/DAF/freeze/edit owners'])
output.parent.mkdir(parents=True, exist_ok=True)
output.write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
print(f'PASS{len(regions)} PE regions/{len(docpins) + len(enginepins)} pins/{len(bindings)} bindings/3 literals; {len(controls)} prepared selectors/1 explicit invariant gap; static only')
