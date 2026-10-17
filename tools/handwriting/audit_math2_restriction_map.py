"""Static Math2 C8 restriction-map ownership and special-cost mask sources."""
import hashlib
import json
import runpy
import sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

if len(sys.argv) != 2:
    raise SystemExit('report.json')
out = Path(sys.argv[1])
b = Binary('MyScriptDocument.dll')
assert hashlib.sha256(b.data).hexdigest() == '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
regions = [
    (0x1800359f0, 0x180035c50, '44a3ff6d114956fe746eb35b3d80f0e3867f6094fc8d8f0da18c353ef1ce6b48'),
    (0x18003b300, 0x18003b540, '78a8116a27d4cf02342618d1da2ec2af0f4ed7e1c751acf897691cc3aea4865a'),
    (0x18003ba20, 0x18003ba70, '69a7aba8908185df663c560b12ffb4dcc5addbc3d931be61fb39a695cb93932c'),
    (0x180035e60, 0x180035eb0, 'f69881df8d0cdb07b0d90fb6d7d031647e233d0e1ce4f6600dd13c767604e77a'),
    (0x180035eb0, 0x180035f70, 'f471b7e4122bc6b0d0c4557a2b322b33aa912e5b1e51f57f8422d2f03f563456'),
    (0x18003b920, 0x18003ba20, '19bbda42d6e989bc5d469b9d5ec8fd29198ac2176465257031f1715db8d00074'),
    (0x180045bb0, 0x180045c60, 'c4239d407594958030c0c6a73ee7769b415c8335f2352cfd92767ca937f761a9'),
    (0x1800277b0, 0x180027840, '4379afdb74b7d3a8f2c39de533454a84aec09b82420e03859cba5caa54cfebe8'),
    (0x180027970, 0x180027c20, 'ef572e99b5e6f65d4bc94e8b77a9ae65edb867b41b64ed3ab804199122c2eef9'),
    (0x180027c20, 0x180027d70, '30e94fa3f231bf0d7ffbd3da7aa01117d1f04f1cf35de116aa22b07f522a6eee'),
    (0x1800486b0, 0x180048df0, '74792cb49818c071b5987e92ebbdf1f160288713c5374d7280b399c1f18f6489'),
    (0x1800463e0, 0x180046ae0, '5baf2350dcf64e74104527bdd84a547639fa932efe0aa448b43aecce00e5ec7c'),
    (0x180036360, 0x180036810, '54efd9a7e0cda95487edd0c356867d855d99500b122903ec847d86a338c597ee'),
    (0x18003c690, 0x18003cac0, '723e3183c354f78bca5aa652c36b494d46781c70c294da358dab092fba252e98'),
    (0x180038610, 0x18003a260, '180a1bcc3d913b2a19469e34a2b8a184e5a083354ef8f59414bed3d913e2716a'),
]
md = Cs(CS_ARCH_X86, CS_MODE_64)
ins = {}
for start, end, sha in regions:
    raw = b.data[b.off(start):b.off(end)]
    assert hashlib.sha256(raw).hexdigest() == sha
    ins.update((i.address, (i.mnemonic, i.op_str)) for i in md.disasm(raw, start))
pins = {
    0x180035b1e: ('lea', 'r8, [rdi + 0x50]'),
    0x18003b364: ('lea', 'rbp, [rbx + 0x10]'),
    0x18003b37d: ('xor', 'eax, eax'),
    0x18003b3b3: ('mov', 'qword ptr [rbx + 0xc8], rax'),
    0x18003ba30: ('mov', 'rdx, qword ptr [rdx + 0xc8]'),
    0x18003ba37: ('cmp', 'rdx, r8'),
    0x18003ba3a: ('je', '0x18003ba55'),
    0x18003ba4b: ('call', 'r9'),
    0x18003ba4e: ('mov', 'qword ptr [rdi + 0xc8], rbx'),
    0x18003ba5a: ('mov', 'al, 1'),
    0x180035e73: ('mov', 'r8, qword ptr [rax + 0x18]'),
    0x180035e77: ('call', 'r8'),
    0x180035ec8: ('test', 'r8, r8'),
    0x180035ecb: ('jne', '0x180035eed'),
    0x180035ed7: ('mov', 'r9, qword ptr [rax + 0x20]'),
    0x180035eea: ('jmp', 'r9'),
    0x180035eed: ('mov', 'word ptr [rdx + 0x78], 0xffff'),
    0x180035f14: ('mov', 'rdx, qword ptr [rdi + 0x60]'),
    0x180035f18: ('lea', 'r8, [rdi + 0x68]'),
    0x180035f28: ('mov', 'qword ptr [rsp + 0x28], rbx'),
    0x180035f34: ('mov', 'byte ptr [rsp + 0x20], 1'),
    0x180035f39: ('call', 'r10'),
    0x180035f3c: ('mov', 'rdx, qword ptr [rdi + 0x80]'),
    0x180035f43: ('mov', 'r8, rax'),
    0x180035f50: ('call', 'r9'),
    0x180045bfd: ('mov', 'qword ptr [rsi + 0x230], rax'),
    0x180045c1d: ('mov', 'r8, rdi'),
    0x180045c26: ('mov', 'r9, qword ptr [rax + 0x20]'),
    0x180045c2a: ('call', 'r9'),
    0x18002781d: ('mov', 'qword ptr [rdi + 0x60], rcx'),
    0x180027af7: ('mov', 'r15, qword ptr [rbp + 0x60]'),
    0x180027afb: ('test', 'r15, r15'),
    0x180027afe: ('je', '0x180027b3d'),
    0x180027b28: ('mov', 'r9, qword ptr [rax + 0x18]'),
    0x180027cb4: ('mov', 'edx, 0x13e'),
    0x180027d0f: ('mov', 'qword ptr [rbp + 0x60], rax'),
    0x18004640b: ('movzx', 'r13d, word ptr [rdx + 0xa]'),
    0x180048c90: ('mov', 'word ptr [rsi + 0x160], ax'),
    0x180048caf: ('mov', 'word ptr [rsi + 0x162], ax'),
    0x180048cc8: ('mov', 'word ptr [rsi + 0x164], ax'),
    0x18003b43f: ('mov', 'rax, qword ptr [rsi + 0xa0]'),
    0x18003b446: ('mov', 'qword ptr [rbp + 0xa0], rax'),
    0x18003c778: ('movzx', 'ecx, word ptr [r9 + 0xb0]'),
    0x18003c78d: ('mov', 'dword ptr [rax + rcx*4], 0x7f7fffff'),
    0x18003c79a: ('lea', 'rbx, [r9 + 0xb4]'),
    0x18003c7a1: ('mov', 'eax, 0xffff'),
    0x18003c7a6: ('cmp', 'word ptr [rbx], ax'),
    0x18003c7a9: ('je', '0x18003caa7'),
    0x180038fe6: ('cmp', 'qword ptr [rax + 0xc8], 0'),
    0x180038ff2: ('je', '0x1800390cb'),
    0x18003900e: ('call', '0x180036360'),
    0x18003917d: ('call', '0x1800351e0'),
    0x1800365d7: ('call', '0x1800351e0'),
    0x18003664b: ('call', '0x1800351e0'),
    0x180036743: ('call', '0x1800351e0'),
}
for address, expected in pins.items():
    assert ins[address] == expected, (hex(address), ins[address], expected)
bindings = [
    (0x1801eda20 + 0x18, 0x18003b920),
    (0x1801eda20 + 0x20, 0x18003ba20),
    (0x1801ed930 + 0x18, 0x180035e60),
    (0x1801ed930 + 0x20, 0x180035eb0),
    (0x1801fd0d8 + 0x18, 0x180045bb0),
    (0x1801fd0d8 + 0x30, 0x1800463e0),
]
for address, expected in bindings:
    assert b.qwords(address, 1)[0] == expected, hex(address)
for instruction, next_address, displacement, expected in [
    (0x180048c38, 0x180048c3f, 0x1d1c19, b'[bos]\0'),
    (0x180048c97, 0x180048c9e, 0x1d1bc2, b'[eos]\0'),
    (0x180048cb6, 0x180048cbd, 0x1c4ae7, b' \0'),
]:
    address = next_address + displacement
    assert b.data[b.off(address):b.off(address) + len(expected)] == expected
category_sha = hashlib.sha256(b.data[b.off(0x180255150):b.off(0x180255150) + 14 * 72]).hexdigest()
assert category_sha == '0e9aa99e7af9d0b7adcb11f70b9c5a712890b5aed533b0fc5529abe3a16b44b7'

raw = (ROOT / 'models/math2/math2-unwrapped.res').read_bytes()
assert hashlib.sha256(raw).hexdigest() == '2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
labels, _ = runpy.run_path(str(ROOT / 'tools/export-ink-math-model.py'))['parse'](raw)
specials = [labels.index(x) if x in labels else 65535 for x in ('[bos]', '[eos]', ' ')]
assert specials == [0, 1, 65535] and len(labels) == 297
report = dict(regions=[dict(start=hex(a), end=hex(z), sha256=s) for a, z, s in regions],
    instructionPins=len(pins), bindings=len(bindings), categoryTableSha256=category_sha,
    rawSpecialIds=dict(B0=specials[0], B2=specials[1], B4=specials[2]), mapBytes=297,
    coldEefMap=None, freshEebSk=None, resetClearsMap=False,
    costSentinelBits='0x7f7fffff', actualUnconditionalMaskedClass=0,
    actualOptionalB2B4MaskBranch=False,
    configured='EED service18 -> EEE V20 -> EED V30(0463E0,param5=1) -> EEF V20 adopt C8',
    nullMap='035EB0 forwards NULL producer return without checking;03BA20 adopts NULL and returns1',
    expansion='C8 NULL: all classes ->0351E0; nonnull:036360 raw heap + permitted observable alternatives/alias transport',
    exclusions=['General SK13E membership implementation', 'Arbitrary configured map bytes',
                'Warm/copied source has no implicit C8 reset', 'Candidate heap/beam implementation',
                'CPU exp/log dispatch and complete-hypothesis ordering'],
    runtimeChanges=False, vendorExecution=False, ortExecution=False)
out.parent.mkdir(parents=True, exist_ok=True)
out.write_text(json.dumps(report, indent=2) + '\n', encoding='utf8')
print(f'PASS {len(regions)} regions/{len(pins)} pins/{len(bindings)} bindings; '
      f'actual B0/B2/B4={specials}, cold C8 NULL; static only')
