"""Pinned PE-only main2144 scheduling and marked ordinary1FCF40 source binding."""
import argparse
import hashlib
import json
import sys
from pathlib import Path
import capstone

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

parser = argparse.ArgumentParser()
parser.add_argument('--output', required=True)
args = parser.parse_args()
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
blocks = [
    (0x1801fcf40, 0x1801fd240, '53e84b3af4a85b9dc3a66bdff89eb87b50013b3057d49b2a420cce69bd78d01e'),
    (0x180152280, 0x1801522f0, '4005b31c44a5f4df54dcd4ad6777badf6b3adf3ce2bb573d984bc20e6d339538'),
    (0x1801a7920, 0x1801a7950, '60d5c04347b5e6d02bee62f708b824ac4d70e806a9557c41cf4b5639c3509faf'),
    (0x1802144f0, 0x180215ef0, '9fd59d02765f277f42cad3ab21b65ef09b6592a4813fd74a2a9946f6f8b08c51')]
md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
instructions = {}
for begin, end, digest in blocks:
    raw = b.data[b.off(begin):b.off(end)]
    assert hashlib.sha256(raw).hexdigest() == digest, hex(begin)
    instructions.update({i.address: (i.mnemonic, i.op_str) for i in md.disasm(raw, begin)})
pins = {
    0x1801fcf76: ('mov', 'r11d, dword ptr [r8 + 0x44]'),
    0x1801fcf7f: ('cmp', 'r11d, 5'),
    0x1801fcfb8: ('movzx', 'eax, byte ptr [rbx + 0x1c]'),
    0x1801fcfbc: ('cmp', 'al, 7'),
    0x1801fcfcc: ('cmp', 'byte ptr [rbx + 0x3c], r14b'),
    0x1801fcfd0: ('jne', '0x1801fd0e2'),
    0x1801fd0ee: ('mov', 'r10, qword ptr [rax + 0xe8]'),
    0x1801fd0fa: ('mov', 'r8d, dword ptr [rbx + 4]'),
    0x1801fd0fe: ('mov', 'eax, dword ptr [rbx]'),
    0x1801fd100: ('inc', 'eax'),
    0x1801fd102: ('lea', 'r15d, [r8 + 1]'),
    0x1801fd106: ('cmp', 'r15d, eax'),
    0x1801fd109: ('jge', '0x1801fd169'),
    0x1801fd10b: ('movzx', 'r9d, word ptr [rbx + 0xe]'),
    0x1801fd113: ('call', 'r10'),
    0x1801fd119: ('cmp', 'eax, -1'),
    0x1801fd11c: ('je', '0x1801fd169'),
    0x1801fd122: ('xor', 'r9d, r9d'),
    0x1801fd125: ('mov', 'r8d, r15d'),
    0x1801fd12b: ('mov', 'r10, qword ptr [rcx + 0xe8]'),
    0x1801fd135: ('call', 'r10'),
    0x1801fd13a: ('sub', 'ecx, r14d'),
    0x1801fd13d: ('cmp', 'eax, r14d'),
    0x1801fd140: ('je', '0x1801fd169'),
    0x1801fd142: ('cmp', 'ecx, 1'),
    0x1801fd145: ('jne', '0x1801fd169'),
    0x1801fd147: ('mov', 'eax, dword ptr [rbx + 0x44]'),
    0x1801fd14a: ('cmp', 'eax, 6'),
    0x1801fd14d: ('jl', '0x1801fd154'),
    0x1801fd14f: ('cmp', 'eax, 8'),
    0x1801fd152: ('jle', '0x1801fd165'),
    0x1801fd165: ('mov', 'al, 1'),
    0x1801fd169: ('xor', 'al, al'),
    0x180152297: ('mov', 'r10, qword ptr [rax + 0x68]'),
    0x1801522b0: ('mov', 'r9, qword ptr [rax + 0x78]'),
    0x1801522c6: ('mov', 'r9, qword ptr [rax + 0x10]'),
    0x1801522cd: ('mov', 'eax, dword ptr [rax + 0x10]'),
    0x1801a7944: ('movzx', 'eax, word ptr [r9 + rax]'),
    0x1802148df: ('lea', 'r9d, [rdi - 1]'),
    0x1802148f2: ('mov', 'r10, qword ptr [rax + 0x80]'),
    0x180214910: ('mov', 'word ptr [rsi + 0x230a], ax'),
    0x180214e10: ('mov', 'r10, qword ptr [rax + 0xd0]'),
    0x180214fc3: ('call', '0x1801fe740'),
    0x1802151b9: ('call', '0x1801fd240'),
    0x180215279: ('call', '0x18020e6a0'),
    0x180215dcb: ('add', 'ecx, dword ptr [rsi + 0x2490]'),
    0x180215dc2: ('jge', '0x180215de3'),
    0x180215dd3: ('jl', '0x180215de3'),
    0x180215dde: ('call', '0x180207cb0'),
    0x180215e79: ('mov', 'r9d, edi'),
    0x180215e8b: ('mov', 'r10, qword ptr [rax + 0x80]'),
    0x180215e95: ('inc', 'edi')}
for address, expected in pins.items():
    assert instructions.get(address) == expected, (hex(address), instructions.get(address), expected)
assert b.qwords(0x1802de610 + 0xe8, 1)[0] == 0x180152280
assert b.qwords(0x1802e1ff0 + 0x78, 1)[0] == 0x1801a7920
report = dict(blocks=[dict(start=hex(a), endExclusive=hex(z), sha256=h) for a, z, h in blocks],
              pins=[f'{a:x} {m} {o}' for a, (m, o) in pins.items()], dispatchTargets=2,
              scope='Marked ordinary kind0/2 with actual previous C00 kind6 only. E8 resolves graph group descriptor then source descriptor I32+10 actual line. References04/0E and next04+1, not ending08/10. No kind7/source5 interval approximation.',
              scheduling='2144F0 V80(start-1) before D0 span scheduling; V80(completednode) before increment. Live207CB0 required before end-node V80 if further nodes and node+1 reaches parentDistance54+publicationOffset2490. Global logical-row facade refuses that unported publication extent.',
              dependencies=['audit_main_global_word_transitions.py', 'audit_fresh_case_main_source.py'],
              vendorExecution=False)
Path(args.output).write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
print(f'PASS {len(blocks)} pinned PE blocks/{len(pins)} instructions/2 dispatch targets; no vendor execution')
