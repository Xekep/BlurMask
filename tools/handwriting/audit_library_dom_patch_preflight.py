"""Read-only DOM.V210/BE2 scratch-tree preflight and actual consumer proof."""
import argparse, hashlib, json, subprocess, sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

root = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(root / 'research/myscript/scripts'))
from inspect_text_features import Binary

p = argparse.ArgumentParser(description=__doc__)
p.add_argument('output', type=Path)
a = p.parse_args()
dependency = Path(str(a.output) + '.history.json')
subprocess.run([sys.executable, str(Path(__file__).with_name('audit_registered_text_patch_history.py')),
                str(dependency)], cwd=root, check=True)
e, d = Binary('MyScriptEngine.dll'), Binary('MyScriptDocument.dll')
for binary, expected in [(e, 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'),
                         (d, '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f')]:
    assert hashlib.sha256(binary.data).hexdigest() == expected
md = Cs(CS_ARCH_X86, CS_MODE_64)
regions, bindings = [], []
pins = 0

def region(binary, start, end, sha, expected):
    global pins
    raw = binary.data[binary.off(start):binary.off(end)]
    assert hashlib.sha256(raw).hexdigest() == sha
    rows = [(x.address, x.mnemonic, x.op_str) for x in md.disasm(raw, start)]
    for item in expected:
        assert item in rows, (binary.name, hex(start), item)
    pins += len(expected)
    regions.append(dict(dll=binary.name, start=hex(start), end=hex(end), sha256=sha))
    return rows

def bind(table, slot, target):
    assert e.qwords(table + slot, 1) == (target,)
    bindings.append(dict(table=hex(table), slot=hex(slot), target=hex(target)))

for table in [0x180227080, 0x1802273a0]:
    bind(table, 0x210, 0x1800efa70)
    bind(table, 0x218, 0x1800efb10)
bind(0x1802265e0, 8, 0x1800e2870)
bind(0x1802265e0, 0x10, 0x1800e2d70)
region(e, 0x1800efa70, 0x1800efb10,
       '0ca1c7d6a6e3dad3c5867a1cc21f19d66a793e484823750d2c56de82888c7706', [
    (0x1800efa96, 'call', '0x1800ebd50'),
    (0x1800efa9e, 'test', 'rsi, rsi'),
    (0x1800efaa8, 'mov', 'byte ptr [rax], 1'),
    (0x1800efaab, 'mov', 'al, 1'),
    (0x1800efacd, 'mov', 'r8, rbp'),
    (0x1800efad0, 'mov', 'rdx, r14'),
    (0x1800efad6, 'mov', 'r10, qword ptr [rax + 8]'),
    (0x1800efae9, 'call', 'r10')])
region(e, 0x1800ebd50, 0x1800ebe40,
       '3549bdafe28d3f22320c543ecdbfdda3d8a77ee14e1b9bab21b5ef77dd1e13c2', [
    (0x1800ebd6e, 'mov', 'edx, 0xbe2'),
    (0x1800ebd88, 'mov', 'rcx, qword ptr [rbp + 0x18]'),
    (0x1800ebd90, 'add', 'rcx, 0x108'),
    (0x1800ebda2, 'call', 'r8'),
    (0x1800ebdac, 'cmp', 'dword ptr [rcx + 0xd8], ebx'),
    (0x1800ebdc0, 'mov', 'rdx, qword ptr [rcx + 0xd0]'),
    (0x1800ebdd7, 'shl', 'rdi, 5'),
    (0x1800ebde6, 'call', 'r9'),
    (0x1800ebe04, 'mov', 'r14, qword ptr [rdi + rax]'),
    (0x1800ebe24, 'call', 'r8')])
rows = region(e, 0x1800e2870, 0x1800e2d70,
       '10104e3d56789792d363c1a3afed16392e072aa4de741fc61d41aad16d19dbe7', [
    # Shared BEE owner, observer refusal BEFORE compatible-output write.
    (0x1800e287b, 'mov', 'rax, qword ptr [r8 + 0x10]'),
    (0x1800e2884, 'mov', 'byte ptr [rsp + 0x30], 0'),
    (0x1800e2889, 'mov', 'qword ptr [rsp + 0x38], rax'),
    (0x1800e289c, 'mov', 'r15b, 1'),
    (0x1800e28a6, 'cmp', 'dword ptr [rdx + 0x50], ecx'),
    (0x1800e28d3, 'xor', 'al, al'),
    (0x1800e28df, 'ret', ''),
    (0x1800e28ed, 'cmp', 'dword ptr [r14 + 8], ecx'),
    (0x1800e2900, 'mov', 'byte ptr [rax], r15b'),
    # Nonempty: optional root fallback, BCC construction, kind4 child copies.
    (0x1800e2932, 'mov', 'r8, qword ptr [rax + 0x20]'),
    (0x1800e2940, 'mov', 'edx, 0xbcc'),
    (0x1800e2972, 'mov', 'r9d, 0x10'),
    (0x1800e297b, 'call', 'rsi'),
    (0x1800e2983, 'je', '0x1800e2a6e'),
    (0x1800e2989, 'cmp', 'byte ptr [rbx], 4'),
    (0x1800e29b8, 'inc', 'r14d'),
    (0x1800e2a18, 'mov', 'r10, qword ptr [rax + 0x88]'),
    (0x1800e2a1f, 'call', 'r10'),
    (0x1800e2a2c, 'je', '0x1800e2a73'),
    (0x1800e2a3b, 'mov', 'r10, qword ptr [rcx + 0xa8]'),
    (0x1800e2a45, 'call', 'r10'),
    # Map allocation, then ALL row paths resolved before any row mutation.
    (0x1800e2a88, 'mov', 'edx, 8'),
    (0x1800e2a8d, 'mov', 'r8d, dword ptr [r14 + 8]'),
    (0x1800e2aaf, 'je', '0x1800e2cf7'),
    (0x1800e2afc, 'mov', 'r11d, dword ptr [r10 + rsi + 0x10]'),
    (0x1800e2b11, 'mov', 'rdx, qword ptr [r10 + rsi + 8]'),
    (0x1800e2b16, 'mov', 'r8d, dword ptr [rdx + r9*4]'),
    (0x1800e2b38, 'mov', 'qword ptr [rbp + rbx*8], 0'),
    (0x1800e2b41, 'xor', 'r15b, r15b'),
    (0x1800e2b69, 'add', 'rsi, 0x28'),
    (0x1800e2b6d, 'jmp', '0x1800e2ae0'),
    (0x1800e2baa, 'mov', 'rbx, qword ptr [rbp + rsi*8]'),
    # Existing child op3/4, missing child incompatibility.
    (0x1800e2c00, 'je', '0x1800e2c30'),
    (0x1800e2c12, 'mov', 'r9, qword ptr [rax + 0xe8]'),
    (0x1800e2c19, 'call', 'r9'),
    (0x1800e2c38, 'xor', 'r15b, r15b'),
    # Op1/2 payload clone is UNCHECKED; don't invent a native false return.
    (0x1800e2c4e, 'mov', 'r8, qword ptr [r8 + r14 + 0x20]'),
    (0x1800e2c60, 'call', 'r10'),
    (0x1800e2c63, 'mov', 'r9, rax'),
    (0x1800e2c66, 'test', 'rdi, rdi'),
    (0x1800e2c69, 'jne', '0x1800e2ccc'),
    (0x1800e2ca3, 'cmp', 'edx, dword ptr [rax + r14 + 0x18]'),
    (0x1800e2ca8, 'jne', '0x1800e2cbd'),
    (0x1800e2cb4, 'mov', 'r10, qword ptr [rax + 0xa8]'),
    (0x1800e2cbd, 'xor', 'r15b, r15b'),
    (0x1800e2cd3, 'mov', 'r10, qword ptr [rax + 0xb8]'),
    (0x1800e2ce0, 'call', 'r10'),
    # Worked separate from compatibility; cleanup precedes output write.
    (0x1800e2cf7, 'xor', 'bl, bl'),
    (0x1800e2cfb, 'movzx', 'ebx, byte ptr [rsp + 0x88]'),
    (0x1800e2d1d, 'call', 'r8'),
    (0x1800e2d39, 'call', 'r8'),
    (0x1800e2d44, 'movzx', 'eax, bl'),
    (0x1800e2d5e, 'mov', 'byte ptr [rcx], r15b')])
unchecked = [x for x in rows if 0x1800e2c60 <= x[0] < 0x1800e2c6b]
assert [x[1:] for x in unchecked] == [('call','r10'),('mov','r9, rax'),('test','rdi, rdi'),('jne','0x1800e2ccc')]
region(d, 0x180115b02, 0x180115db8,
       'bf8f1761afb5724a3511bf52a812aaf1ae97de7bb0e2ade30cd4e22db641040e', [
    (0x180115b38, 'call', 'r10'), (0x180115b3d, 'je', '0x180115dea'),
    (0x180115b5a, 'mov', 'r10, qword ptr [rax + 0x210]'),
    (0x180115b6b, 'call', 'r10'), (0x180115b70, 'je', '0x180115dea'),
    (0x180115b76, 'cmp', 'byte ptr [rsp + 0x43], 0'),
    (0x180115b7e, 'je', '0x180115ded'),
    (0x180115b84, 'cmp', 'byte ptr [rsp + 0x44], 0'),
    (0x180115b89, 'je', '0x180115ded'),
    (0x180115c5e, 'call', 'r10'),
    (0x180115d9b, 'mov', 'r10, qword ptr [rax + 0x218]'),
    (0x180115dac, 'call', 'r10')])
a.output.parent.mkdir(parents=True, exist_ok=True)
a.output.write_bytes((json.dumps(dict(regions=regions, bindings=bindings, pins=pins,
    dependency=json.loads(dependency.read_bytes()), vendorExecution=False,
    scope='DOM preflight algorithm only; library Target/Edit is not original page DOM or CBB acceptance'),
    indent=2) + '\n').encode())
print(f'PASS {len(regions)} regions/{len(bindings)} bindings/{pins} exact pins + original/history caller proof; vendor execution false.')
