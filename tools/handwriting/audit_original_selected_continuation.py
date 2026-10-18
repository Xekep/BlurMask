"""Read-only original-selected C22 export and operation4/7 continuation.

This proves a worker-selected registry path, not original CBB/page acceptance.
No native DLL is loaded or executed.
"""
import argparse, hashlib, json, subprocess, sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

root = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(root / 'research/myscript/scripts'))
from inspect_text_features import Binary

p = argparse.ArgumentParser(description=__doc__)
p.add_argument('output', type=Path)
a = p.parse_args()
a.output.parent.mkdir(parents=True, exist_ok=True)
dependencies = {}
for name in ('audit_selected_original_context_bootstrap.py',
             'audit_selected_document_input_transforms.py',
             'audit_selected_input_registry_replay.py'):
    output = Path(str(a.output) + '.' + name + '.json')
    subprocess.run([sys.executable, str(Path(__file__).with_name(name)), str(output)],
                   cwd=root, check=True)
    dependencies[name] = json.loads(output.read_text())

shas = {
    'MyScriptInk.dll': 'b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a',
    'MyScriptText.dll': '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f',
}
bs = {name: Binary(name) for name in shas}
for name, b in bs.items():
    assert hashlib.sha256(b.data).hexdigest() == shas[name], name
md = Cs(CS_ARCH_X86, CS_MODE_64)
regions, bindings = [], []
pin_count = 0

def pin(name, start, end, expected):
    global pin_count
    b = bs[name]
    raw = b.data[b.off(start):b.off(end)]
    xs = [(x.address, x.mnemonic, x.op_str) for x in md.disasm(raw, start)]
    for value in expected:
        assert value in xs, (name, hex(start), value)
    pin_count += len(expected)
    regions.append(dict(dll=name, start=hex(start), end=hex(end),
                        sha256=hashlib.sha256(raw).hexdigest()))
    return xs

def bind(name, table, slot, target):
    assert bs[name].qwords(table + slot, 1) == (target,)
    bindings.append(dict(dll=name, table=hex(table), slot=hex(slot), target=hex(target)))

I, T = 'MyScriptInk.dll', 'MyScriptText.dll'
for slot, target in ((0x128, 0x18006c880), (0xf8, 0x18006a100), (0x188, 0x18006ed30)):
    bind(I, 0x1800e5050, slot, target)
for slot, target in ((0x18, 0x18009eca0), (0x20, 0x18008e870),
                     (0x28, 0x18009ed40), (0x30, 0x18009ede0), (0x38, 0x18009ee20)):
    bind(I, 0x1800e78a0, slot, target)

pin(T, 0x18002980e, 0x180029883, [
    (0x18002980e, 'mov', 'r8, qword ptr [r14 + 0x30]'),
    (0x180029820, 'cmp', 'r8, -1'),
    (0x180029824, 'je', '0x18002984a'),
    (0x18002982f, 'mov', 'r9, qword ptr [rax + 0x128]'),
    (0x180029836, 'call', 'r9'),
    (0x180029839, 'mov', 'rcx, qword ptr [rax + 0x18]'),
    (0x18002983d, 'mov', 'eax, dword ptr [rax + 0x20]'),
    (0x18002984e, 'mov', 'byte ptr [rsp + 0x20], 0'),
    (0x18002985c, 'call', '0x180037e80'),
    (0x180029861, 'test', 'al, al'),
    (0x180029863, 'je', '0x180029485'),
    (0x180029876, 'call', '0x180026ef0'),
])
xs = pin(I, 0x18006c880, 0x18006c8e3, [
    (0x18006c88d, 'mov', 'rdx, qword ptr [rdx + 0x40]'),
    (0x18006c899, 'mov', 'r9, qword ptr [rax + 0x30]'),
    (0x18006c89d, 'call', 'r9'),
    (0x18006c8a0, 'test', 'rax, rax'),
    (0x18006c8a3, 'jne', '0x18006c8d9'),
    (0x18006c8bf, 'mov', 'edx, 0x1000000'),
    (0x18006c8d1, 'xor', 'eax, eax'),
    (0x18006c8d9, 'mov', 'rax, qword ptr [rax + 8]'),
])
# The decompiler omits the lookup's third argument. The native R8 tag is
# untouched from entry through C51.V30 dispatch, not a new default ID.
assert not any(op.split(',')[0] in ('r8', 'r8d', 'r8w', 'r8b')
               for address, mnemonic, op in xs if address < 0x18006c89d)
pin(I, 0x18009ede0, 0x18009ee20, [
    (0x18009eded, 'mov', 'eax, r8d'),
    (0x18009ee00, 'cmp', 'r8, qword ptr [rax + 8]'),
    (0x18009ee0e, 'mov', 'rax, r9'),
    (0x18009ee14, 'lea', 'rcx, [rax + 0x10]'),
    (0x18009ee18, 'cmovne', 'r9, rcx'),
])
pin(I, 0x18006ed30, 0x18006ede2, [
    (0x18006ed67, 'mov', 'rsi, qword ptr [rdx + 0x58]'),
    (0x18006ed74, 'test', 'rsi, rsi'),
    (0x18006ed77, 'jne', '0x18006edbd'),
    (0x18006ed80, 'mov', 'r9b, 1'),
    (0x18006ed83, 'lea', 'r8, [rbx + 8]'),
    (0x18006ed87, 'mov', 'rdx, rbp'),
    (0x18006ed8d, 'mov', 'r10, qword ptr [rax + 0x40]'),
    (0x18006ed9f, 'call', 'r10'),
    (0x18006edcf, 'mov', 'rdx, rsi'),
    (0x18006edd5, 'mov', 'r10, qword ptr [rax + 0x38]'),
])
pin(I, 0x18006a100, 0x18006a1be, [
    (0x18006a116, 'mov', 'rsi, r8'),
    (0x18006a12e, 'mov', 'r8, qword ptr [rax + 0x18]'),
    (0x18006a135, 'cmp', 'rax, rsi'),
    (0x18006a138, 'jl', '0x18006a13e'),
    (0x18006a13a, 'lea', 'rsi, [rax + 1]'),
    (0x18006a143, 'mov', 'qword ptr [r14], rsi'),
    (0x18006a15f, 'inc', 'dword ptr [rdi + 0x68]'),
    (0x18006a162, 'mov', 'r8, rsi'),
    (0x18006a17c, 'call', 'r9'),
    (0x18006a17f, 'sub', 'dword ptr [rdi + 0x68], 1'),
    (0x18006a18b, 'call', '0x180064610'),
    (0x18006a1ae, 'mov', 'al, 1'),
])
# Settled observer queues are drained by F8, not silently removed by a new
# history=false flag. This audit does not claim that C5E/version work vanishes.
pin(I, 0x180064610, 0x1800648a0, [
    (0x180064651, 'cmp', 'dword ptr [rbp + 0x88], r15d'),
    (0x180064658, 'jbe', '0x18006478d'),
    (0x180064794, 'mov', 'dword ptr [rbp + 0x88], 0'),
    (0x18006479e, 'cmp', 'dword ptr [rbp + 0x78], edi'),
    (0x1800647a1, 'jbe', '0x180064873'),
])

a.output.write_bytes((json.dumps(dict(peSha256=shas, regions=regions,
    bindings=bindings, pins=pin_count, dependencies=dependencies,
    scope='same original-selected binding; selected C22 tag lookup; identity export; library registry overlay op4/7',
    pending=['CBB original page/store/revisions/history acceptance',
             'native C5E/version/event lifecycle outside the library overlay'],
    vendorExecution=False), indent=2) + '\n').encode())
print(f'PASS {len(regions)} native regions/{len(bindings)} bindings/{pin_count} pins +3 dependencies; selected continuation only.')
